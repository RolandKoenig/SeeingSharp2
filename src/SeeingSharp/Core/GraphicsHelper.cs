﻿using System;
using System.IO;
using System.Linq;
using SeeingSharp.Checking;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Core.Devices;
using SeeingSharp.Drawing3D.Resources;
using SeeingSharp.Resources;
using SeeingSharp.Util;
using SeeingSharp.Util.Sdx;
using Vortice.Direct2D1;
using Vortice.DXGI;
using Vortice.WIC;
using VMath = Vortice.Mathematics;
using D3D11 = Vortice.Direct3D11;
using SDXTK = SeeingSharp.Util.SdxTK;

namespace SeeingSharp.Core
{
    public static class GraphicsHelper
    {
        /// <summary>
        /// Creates a new texture from a bitmap.
        /// </summary>
        internal static D3D11.ID3D11Texture2D CreateTexture(EngineDevice device, ResourceLink source)
        {
            using (var inStream = source.OpenInputStream())
            using (var rawImage = SDXTK.Image.Load(inStream))
            {
                return CreateTexture(device, rawImage);
            }
        }

        /// <summary>
        /// Creates a new image based on the given raw image data.
        /// </summary>
        /// <param name="device">Graphics device.</param>
        /// <param name="rawImage">Raw image data.</param>
        internal static D3D11.ID3D11Texture2D CreateTexture(EngineDevice device, SDXTK.Image rawImage)
        {
            var textureDescription = new D3D11.Texture2DDescription
            {
                Width = rawImage.Description.Width,
                Height = rawImage.Description.Height,
                MipLevels = rawImage.Description.MipLevels,
                ArraySize = rawImage.Description.ArraySize,
                Format = rawImage.Description.Format,
                Usage = D3D11.ResourceUsage.Default,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = D3D11.BindFlags.ShaderResource,
                CpuAccessFlags = D3D11.CpuAccessFlags.None,
                OptionFlags = D3D11.ResourceOptionFlags.None
            };

            // Special handling for cube textures
            if (rawImage.Description.Dimension == SDXTK.TextureDimension.TextureCube)
            {
                textureDescription.OptionFlags = D3D11.ResourceOptionFlags.TextureCube;
            }

            return device.DeviceD3D11_1.CreateTexture2D(textureDescription, rawImage.ToD3D11SubresourceData());
        }

        /// <summary>
        /// Is the given texture multisampled?
        /// </summary>
        internal static bool IsMultisampled(D3D11.Texture2DDescription textureDesc)
        {
            return textureDesc.SampleDescription.Count > 1 || textureDesc.SampleDescription.Quality > 0;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        public static class Internals
        {
            // All default texture formats
            public const Format DEFAULT_TEXTURE_FORMAT = Format.B8G8R8A8_UNorm;
            public const Format DEFAULT_TEXTURE_FORMAT_SHARING = Format.B8G8R8A8_UNorm;
            public const Format DEFAULT_TEXTURE_FORMAT_SHARING_DIRECT_2D = Format.B8G8R8A8_UNorm;
            public const Format DEFAULT_TEXTURE_FORMAT_NORMAL_DEPTH = Format.R16G16B16A16_Float;
            public const Format DEFAULT_TEXTURE_FORMAT_OBJECT_ID = Format.R32_Float;
            public const Format DEFAULT_TEXTURE_FORMAT_DEPTH = Format.D32_Float_S8X24_UInt;
            public static readonly Guid DEFAULT_WIC_BITMAP_FORMAT = PixelFormat.Format32bppBGRA;
            public static readonly Guid DEFAULT_WIC_BITMAP_FORMAT_DIRECT_2D = PixelFormat.Format32bppPBGRA;

            /// <summary>
            /// Loads a bitmap using WIC.
            /// </summary>
            public static WicBitmapSourceInternal LoadBitmapSource(ResourceLink resource)
            {
                using (var inStream = resource.OpenInputStream())
                {
                    return LoadBitmapSource(inStream);
                }
            }

            /// <summary>
            /// Loads a bitmap using WIC.
            /// </summary>
            /// <param name="inStream">The stream from which to load the texture file.</param>
            public static WicBitmapSourceInternal LoadBitmapSource(Stream inStream)
            {
                inStream.EnsureNotNull(nameof(inStream));
                inStream.EnsureReadable(nameof(inStream));

                GraphicsCore.EnsureGraphicsSupportLoaded();

                var wicFactory = GraphicsCore.Current.FactoryWIC;
                var bitmapDecoder = wicFactory!.CreateDecoderFromStream(
                    inStream, DecodeOptions.CacheOnDemand);
                var formatConverter = wicFactory.CreateFormatConverter();
                formatConverter.Initialize(
                    bitmapDecoder.GetFrame(0),
                    DEFAULT_WIC_BITMAP_FORMAT,
                    BitmapDitherType.None,
                    null,
                    0.0,
                    BitmapPaletteType.Custom);

                return new WicBitmapSourceInternal(bitmapDecoder, formatConverter);
            }

            /// <summary>
            /// Loads a bitmap using WIC.
            /// </summary>
            public static WicBitmapSourceInternal LoadBitmapSource_D2D(ResourceLink resource)
            {
                using (var inStream = resource.OpenInputStream())
                {
                    return LoadBitmapSource_D2D(inStream);
                }
            }

            /// <summary>
            /// Loads a bitmap using WIC.
            /// </summary>
            /// <param name="inStream">The stream from which to load the texture file.</param>
            public static WicBitmapSourceInternal LoadBitmapSource_D2D(Stream inStream)
            {
                inStream.EnsureNotNull(nameof(inStream));
                inStream.EnsureReadable(nameof(inStream));

                GraphicsCore.EnsureGraphicsSupportLoaded();

                // Parameter changed to represent this article (important is the correct Direct2D format):
                // https://msdn.microsoft.com/en-us/library/windows/desktop/dd756686(v=vs.85).aspx

                var wicFactory = GraphicsCore.Current.FactoryWIC;
                var bitmapDecoder = wicFactory!.CreateDecoderFromStream(
                    inStream, DecodeOptions.CacheOnLoad);
                var formatConverter = wicFactory.CreateFormatConverter();
                formatConverter.Initialize(
                    bitmapDecoder.GetFrame(0),
                    DEFAULT_WIC_BITMAP_FORMAT_DIRECT_2D,
                    BitmapDitherType.None,
                    null,
                    0.0,
                    BitmapPaletteType.MedianCut);

                return new WicBitmapSourceInternal(bitmapDecoder, formatConverter);
            }

            /// <summary>
            /// Creates a Direct3D 11 texture that can be shared between more devices.
            /// </summary>
            /// <param name="device">The Direct3D 11 device.</param>
            /// <param name="width">The width of the generated texture.</param>
            /// <param name="height">The height of the generated texture.</param>
            public static D3D11.ID3D11Texture2D CreateSharedTexture(EngineDevice device, int width, int height)
            {
                device.EnsureNotNull(nameof(device));
                width.EnsurePositiveOrZero(nameof(width));
                height.EnsurePositiveOrZero(nameof(height));

                var textureDescription = new D3D11.Texture2DDescription
                {
                    BindFlags = D3D11.BindFlags.RenderTarget | D3D11.BindFlags.ShaderResource,
                    Format = DEFAULT_TEXTURE_FORMAT_SHARING,
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = D3D11.ResourceUsage.Default,
                    OptionFlags = D3D11.ResourceOptionFlags.Shared,
                    CpuAccessFlags = D3D11.CpuAccessFlags.None,
                    ArraySize = 1
                };

                return device.DeviceD3D11_1.CreateTexture2D(textureDescription);
            }

            public static D3D11.ID3D11Texture2D LoadTexture2DFromMappedTexture(EngineDevice device, MemoryMappedTexture<int> mappedTexture, bool generateMiplevels)
            {
                // Create the texture
                var dataRectangle = new D3D11.SubresourceData(
                    mappedTexture.Pointer,
                    mappedTexture.Width * 4);

                D3D11.ID3D11Texture2D result;
                if (generateMiplevels)
                {
                    result = device.DeviceD3D11_1.CreateTexture2D(new D3D11.Texture2DDescription
                        {
                            Width = mappedTexture.Width,
                            Height = mappedTexture.Height,
                            ArraySize = 1,
                            BindFlags = D3D11.BindFlags.ShaderResource | D3D11.BindFlags.RenderTarget,
                            Usage = D3D11.ResourceUsage.Default,
                            CpuAccessFlags = D3D11.CpuAccessFlags.None,
                            Format = DEFAULT_TEXTURE_FORMAT,
                            MipLevels = 0,
                            OptionFlags = D3D11.ResourceOptionFlags.None | D3D11.ResourceOptionFlags.GenerateMips,
                            SampleDescription = new SampleDescription(1, 0)
                        }, 
                        new D3D11.SubresourceData[] 
                        { 
                            dataRectangle, dataRectangle, dataRectangle, dataRectangle,
                            dataRectangle, dataRectangle, dataRectangle, dataRectangle,
                            dataRectangle, dataRectangle, dataRectangle, dataRectangle
                        });

                    // Auto generate miplevels
                    using (var shaderResourceView = device.DeviceD3D11_1.CreateShaderResourceView(result))
                    {
                        device.DeviceImmediateContextD3D11.GenerateMips(shaderResourceView);
                    }
                }
                else
                {
                    result = device.DeviceD3D11_1.CreateTexture2D(new D3D11.Texture2DDescription
                        {
                            Width = mappedTexture.Width,
                            Height = mappedTexture.Height,
                            ArraySize = 1,
                            BindFlags = D3D11.BindFlags.ShaderResource,
                            Usage = D3D11.ResourceUsage.Default,
                            CpuAccessFlags = D3D11.CpuAccessFlags.None,
                            Format = DEFAULT_TEXTURE_FORMAT,
                            MipLevels = 1,
                            OptionFlags = D3D11.ResourceOptionFlags.None,
                            SampleDescription = new SampleDescription(1, 0)
                        }, 
                        new D3D11.SubresourceData[] { dataRectangle });
                }

                return result;
            }

            /// <summary>
            /// Creates a default viewport for the given width and height
            /// </summary>
            public static VMath.Viewport CreateDefaultViewport(int width, int height)
            {
                width.EnsurePositiveOrZero(nameof(width));
                height.EnsurePositiveOrZero(nameof(height));
                
                var result = new VMath.Viewport(
                    0f, 0f,
                    width, height,
                    0f, 1f);

                return result;
            }

            /// <summary>
            /// Creates a standard texture with the given width and height.
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="width">Width of generated texture.</param>
            /// <param name="height">Height of generated texture.</param>
            /// <param name="format">The format which is used to create the texture.</param>
            public static D3D11.ID3D11Texture2D CreateTexture(EngineDevice device, int width, int height, Format format = DEFAULT_TEXTURE_FORMAT)
            {
                device.EnsureNotNull(nameof(device));
                width.EnsurePositiveOrZero(nameof(width));
                height.EnsurePositiveOrZero(nameof(height));

                var textureDescription = new D3D11.Texture2DDescription
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = format,
                    Usage = D3D11.ResourceUsage.Default,
                    SampleDescription = new SampleDescription(1, 0),
                    BindFlags = D3D11.BindFlags.ShaderResource,
                    CpuAccessFlags = D3D11.CpuAccessFlags.None,
                    OptionFlags = D3D11.ResourceOptionFlags.None
                };

                return device.DeviceD3D11_1.CreateTexture2D(textureDescription);
            }

            /// <summary>
            /// Creates a standard texture with the given width and height.
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="width">Width of generated texture.</param>
            /// <param name="height">Height of generated texture.</param>
            /// <param name="format">The format which is used to create the texture.</param>
            public static D3D11.ID3D11Texture2D CreateCpuWritableTexture(EngineDevice device, int width, int height, Format format = DEFAULT_TEXTURE_FORMAT)
            {
                device.EnsureNotNull(nameof(device));
                width.EnsurePositiveOrZero(nameof(width));
                height.EnsurePositiveOrZero(nameof(height));

                var textureDescription = new D3D11.Texture2DDescription
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = format,
                    Usage = D3D11.ResourceUsage.Dynamic,
                    SampleDescription = new SampleDescription(1, 0),
                    BindFlags = D3D11.BindFlags.ShaderResource,
                    CpuAccessFlags = D3D11.CpuAccessFlags.Write,
                    OptionFlags = D3D11.ResourceOptionFlags.None
                };

                return device.DeviceD3D11_1.CreateTexture2D(textureDescription);
            }

            /// <summary>
            /// Creates a staging texture which enables copying data from gpu to cpu memory.
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="width">Width of generated texture.</param>
            /// <param name="height">Height of generated texture.</param>
            /// <param name="format">The format used to create the texture.</param>
            public static D3D11.ID3D11Texture2D CreateStagingTexture(EngineDevice device, int width, int height, Format format = DEFAULT_TEXTURE_FORMAT)
            {
                device.EnsureNotNull(nameof(device));
                width.EnsurePositiveOrZero(nameof(width));
                height.EnsurePositiveOrZero(nameof(height));

                //For handling of staging resource see
                // http://msdn.microsoft.com/en-us/library/windows/desktop/ff476259(v=vs.85).aspx

                var textureDescription = new D3D11.Texture2DDescription
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = format,
                    Usage = D3D11.ResourceUsage.Staging,
                    SampleDescription = new SampleDescription(1, 0),
                    BindFlags = D3D11.BindFlags.None,
                    CpuAccessFlags = D3D11.CpuAccessFlags.Read,
                    OptionFlags = D3D11.ResourceOptionFlags.None
                };

                return device.DeviceD3D11_1.CreateTexture2D(textureDescription);
            }

            /// <summary>
            /// Creates a staging texture which enables copying data from gpu to cpu memory.
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="width">Width of generated texture.</param>
            /// <param name="height">Height of generated texture.</param>
            public static D3D11.ID3D11Texture2D CreateStagingTexture(EngineDevice device, int width, int height)
            {
                device.EnsureNotNull(nameof(device));
                width.EnsurePositiveOrZero(nameof(width));
                height.EnsurePositiveOrZero(nameof(height));

                //For handling of staging resource see
                // http://msdn.microsoft.com/en-us/library/windows/desktop/ff476259(v=vs.85).aspx

                var textureDescription = new D3D11.Texture2DDescription
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = DEFAULT_TEXTURE_FORMAT,
                    Usage = D3D11.ResourceUsage.Staging,
                    SampleDescription = new SampleDescription(1, 0),
                    BindFlags = D3D11.BindFlags.None,
                    CpuAccessFlags = D3D11.CpuAccessFlags.Read,
                    OptionFlags = D3D11.ResourceOptionFlags.None
                };

                return device.DeviceD3D11_1.CreateTexture2D(textureDescription);
            }

            /// <summary>
            /// Creates a render target texture with the given width and height.
            /// This texture is used to receive normal and depth data (xyzw components) and stores data in floating point format.
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="width">Width of generated texture.</param>
            /// <param name="height">Height of generated texture.</param>
            /// <param name="gfxConfig">The GFX configuration.</param>
            public static D3D11.ID3D11Texture2D CreateRenderTargetTextureNormalDepth(
                EngineDevice device, int width, int height, GraphicsViewConfiguration gfxConfig)
            {
                device.EnsureNotNull(nameof(device));
                width.EnsurePositiveOrZero(nameof(width));
                height.EnsurePositiveOrZero(nameof(height));
                gfxConfig.EnsureNotNull(nameof(gfxConfig));

                var textureDescription = new D3D11.Texture2DDescription();

                if (gfxConfig.AntialiasingEnabled &&
                    device.IsStandardAntialiasingPossible)
                {
                    textureDescription.Width = width;
                    textureDescription.Height = height;
                    textureDescription.MipLevels = 1;
                    textureDescription.ArraySize = 1;
                    textureDescription.Format = DEFAULT_TEXTURE_FORMAT_NORMAL_DEPTH;
                    textureDescription.Usage = D3D11.ResourceUsage.Default;
                    textureDescription.SampleDescription = device.GetSampleDescription(gfxConfig.AntialiasingQuality);
                    textureDescription.BindFlags = D3D11.BindFlags.ShaderResource | D3D11.BindFlags.RenderTarget;
                    textureDescription.CpuAccessFlags = D3D11.CpuAccessFlags.None;
                    textureDescription.OptionFlags = D3D11.ResourceOptionFlags.None;
                }
                else
                {
                    textureDescription.Width = width;
                    textureDescription.Height = height;
                    textureDescription.MipLevels = 1;
                    textureDescription.ArraySize = 1;
                    textureDescription.Format = DEFAULT_TEXTURE_FORMAT_NORMAL_DEPTH;
                    textureDescription.Usage = D3D11.ResourceUsage.Default;
                    textureDescription.SampleDescription = new SampleDescription(1, 0);
                    textureDescription.BindFlags = D3D11.BindFlags.ShaderResource | D3D11.BindFlags.RenderTarget;
                    textureDescription.CpuAccessFlags = D3D11.CpuAccessFlags.None;
                    textureDescription.OptionFlags = D3D11.ResourceOptionFlags.None;
                }

                return device.DeviceD3D11_1.CreateTexture2D(textureDescription);
            }

            /// <summary>
            /// Creates a render target texture with the given width and height.
            /// This texture is used to receive ObjectIds and stores data as single unsigned integers (32-Bit).
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="width">Width of generated texture.</param>
            /// <param name="height">Height of generated texture.</param>
            /// <param name="gfxConfig">The GFX configuration.</param>
            public static D3D11.ID3D11Texture2D CreateRenderTargetTextureObjectIds(
                EngineDevice device, int width, int height, GraphicsViewConfiguration gfxConfig)
            {
                device.EnsureNotNull(nameof(device));
                width.EnsurePositiveOrZero(nameof(width));
                height.EnsurePositiveOrZero(nameof(height));
                gfxConfig.EnsureNotNull(nameof(gfxConfig));

                var textureDescription = new D3D11.Texture2DDescription();

                if (gfxConfig.AntialiasingEnabled &&
                    device.IsStandardAntialiasingPossible)
                {
                    textureDescription.Width = width;
                    textureDescription.Height = height;
                    textureDescription.MipLevels = 1;
                    textureDescription.ArraySize = 1;
                    textureDescription.Format = DEFAULT_TEXTURE_FORMAT_OBJECT_ID;
                    textureDescription.Usage = D3D11.ResourceUsage.Default;
                    textureDescription.SampleDescription = device.GetSampleDescription(gfxConfig.AntialiasingQuality);
                    textureDescription.BindFlags = D3D11.BindFlags.ShaderResource | D3D11.BindFlags.RenderTarget;
                    textureDescription.CpuAccessFlags = D3D11.CpuAccessFlags.None;
                    textureDescription.OptionFlags = D3D11.ResourceOptionFlags.None;
                }
                else
                {
                    textureDescription.Width = width;
                    textureDescription.Height = height;
                    textureDescription.MipLevels = 1;
                    textureDescription.ArraySize = 1;
                    textureDescription.Format = DEFAULT_TEXTURE_FORMAT_OBJECT_ID;
                    textureDescription.Usage = D3D11.ResourceUsage.Default;
                    textureDescription.SampleDescription = new SampleDescription(1, 0);
                    textureDescription.BindFlags = D3D11.BindFlags.ShaderResource | D3D11.BindFlags.RenderTarget;
                    textureDescription.CpuAccessFlags = D3D11.CpuAccessFlags.None;
                    textureDescription.OptionFlags = D3D11.ResourceOptionFlags.None;
                }

                return device.DeviceD3D11_1.CreateTexture2D(textureDescription);
            }

            /// <summary>
            /// Creates a render target texture with the given width and height.
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="width">Width of generated texture.</param>
            /// <param name="height">Height of generated texture.</param>
            /// <param name="gfxConfig">The GFX configuration.</param>
            public static D3D11.ID3D11Texture2D CreateRenderTargetTexture(
                EngineDevice device, int width, int height, GraphicsViewConfiguration gfxConfig)
            {
                device.EnsureNotNull(nameof(device));
                width.EnsurePositiveOrZero(nameof(width));
                height.EnsurePositiveOrZero(nameof(height));
                gfxConfig.EnsureNotNull(nameof(gfxConfig));

                var textureDescription = new D3D11.Texture2DDescription();

                if (gfxConfig.AntialiasingEnabled &&
                    device.IsStandardAntialiasingPossible)
                {
                    textureDescription.Width = width;
                    textureDescription.Height = height;
                    textureDescription.MipLevels = 1;
                    textureDescription.ArraySize = 1;
                    textureDescription.Format = DEFAULT_TEXTURE_FORMAT;
                    textureDescription.Usage = D3D11.ResourceUsage.Default;
                    textureDescription.SampleDescription = device.GetSampleDescription(gfxConfig.AntialiasingQuality);
                    textureDescription.BindFlags = D3D11.BindFlags.ShaderResource | D3D11.BindFlags.RenderTarget;
                    textureDescription.CpuAccessFlags = D3D11.CpuAccessFlags.None;
                    textureDescription.OptionFlags = D3D11.ResourceOptionFlags.None;
                }
                else
                {
                    textureDescription.Width = width;
                    textureDescription.Height = height;
                    textureDescription.MipLevels = 1;
                    textureDescription.ArraySize = 1;
                    textureDescription.Format = DEFAULT_TEXTURE_FORMAT;
                    textureDescription.Usage = D3D11.ResourceUsage.Default;
                    textureDescription.SampleDescription = new SampleDescription(1, 0);
                    textureDescription.BindFlags = D3D11.BindFlags.ShaderResource | D3D11.BindFlags.RenderTarget;
                    textureDescription.CpuAccessFlags = D3D11.CpuAccessFlags.None;
                    textureDescription.OptionFlags = D3D11.ResourceOptionFlags.None;
                }

                return device.DeviceD3D11_1.CreateTexture2D(textureDescription);
            }

            /// <summary>
            /// Creates a depth buffer texture with given width and height.
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="width">Width of generated texture.</param>
            /// <param name="height">Height of generated texture.</param>
            /// <param name="gfxConfig">Current graphics configuration.</param>
            public static D3D11.ID3D11Texture2D CreateDepthBufferTexture(EngineDevice device, int width, int height, GraphicsViewConfiguration gfxConfig)
            {
                device.EnsureNotNull(nameof(device));
                width.EnsurePositiveOrZero(nameof(width));
                height.EnsurePositiveOrZero(nameof(height));
                gfxConfig.EnsureNotNull(nameof(gfxConfig));

                var textureDescription = new D3D11.Texture2DDescription();

                if (gfxConfig.AntialiasingEnabled &&
                    device.IsStandardAntialiasingPossible)
                {
                    textureDescription.Width = width;
                    textureDescription.Height = height;
                    textureDescription.MipLevels = 1;
                    textureDescription.ArraySize = 1;
                    textureDescription.Usage = D3D11.ResourceUsage.Default;
                    textureDescription.SampleDescription = device.GetSampleDescription(gfxConfig.AntialiasingQuality);
                    textureDescription.BindFlags = D3D11.BindFlags.DepthStencil;
                    textureDescription.CpuAccessFlags = D3D11.CpuAccessFlags.None;
                    textureDescription.OptionFlags = D3D11.ResourceOptionFlags.None;
                }
                else
                {
                    textureDescription.Width = width;
                    textureDescription.Height = height;
                    textureDescription.MipLevels = 1;
                    textureDescription.ArraySize = 1;
                    textureDescription.Usage = D3D11.ResourceUsage.Default;
                    textureDescription.SampleDescription = new SampleDescription(1, 0);
                    textureDescription.BindFlags = D3D11.BindFlags.DepthStencil;
                    textureDescription.CpuAccessFlags = D3D11.CpuAccessFlags.None;
                    textureDescription.OptionFlags = D3D11.ResourceOptionFlags.None;
                }

                // Set buffer format
                textureDescription.Format = Format.D32_Float_S8X24_UInt;

                // Create the texture finally
                return device.DeviceD3D11_1.CreateTexture2D(textureDescription);
            }

            /// <summary>
            /// Create a new DepthBuffer view to bind the given depth buffer to the rendering device.
            /// </summary>
            /// <param name="device">The device on which to create the view.</param>
            /// <param name="depthBuffer">The target resource.</param>
            public static D3D11.ID3D11DepthStencilView CreateDepthBufferView(EngineDevice device, D3D11.ID3D11Texture2D depthBuffer)
            {
                device.EnsureNotNull(nameof(device));
                depthBuffer.EnsureNotNullOrDisposed(nameof(depthBuffer));

                return device.DeviceD3D11_1.CreateDepthStencilView(depthBuffer);
            }

            /// <summary>
            /// Calculate the Bias value depending on the given value to be added to the depth buffer.
            /// </summary>
            /// <param name="device">The device for which to get the value.</param>
            /// <param name="zValue">The z value to be added.</param>
            public static int GetDepthBiasValue(EngineDevice device, float zValue)
            {
                device.EnsureNotNull(nameof(device));

                // Calculation depends on depth buffer format
                // see http://msdn.microsoft.com/de-de/library/windows/desktop/cc308048(v=vs.85).aspx
                // see Book "3D Game Programming With Direct3D 11, Frank D. Luna, 2012" Page 678

                switch (device.DriverLevel)
                {
                    case HardwareDriverLevel.Direct3D12:
                    case HardwareDriverLevel.Direct3D11:
                    case HardwareDriverLevel.Direct3D10:
                        return (int)(zValue / (1 / Math.Pow(2, 23)));

                    default:
                        throw new SeeingSharpGraphicsException("Unable to calculate depth bias value: Target hardware unknown!");
                }
            }

            /// <summary>
            /// Creates a dynamic vertex buffer for the given vertex type and maximum capacity.
            /// </summary>
            /// <typeparam name="T">Type of the vertices.</typeparam>
            /// <param name="device">Graphics device.</param>
            /// <param name="vertexCount">Maximum count of vertices within the buffer.</param>
            public static unsafe D3D11.ID3D11Buffer CreateDynamicVertexBuffer<T>(EngineDevice device, int vertexCount)
                where T : unmanaged
            {
                device.EnsureNotNull(nameof(device));
                vertexCount.EnsurePositiveOrZero(nameof(vertexCount));

                var vertexSize = sizeof(T);
                var bufferDescription = new D3D11.BufferDescription
                {
                    BindFlags = D3D11.BindFlags.VertexBuffer,
                    CpuAccessFlags = D3D11.CpuAccessFlags.Write,
                    OptionFlags = D3D11.ResourceOptionFlags.None,
                    SizeInBytes = vertexCount * vertexSize,
                    Usage = D3D11.ResourceUsage.Dynamic,
                    StructureByteStride = vertexCount * vertexSize
                };

                return device.DeviceD3D11_1.CreateBuffer(bufferDescription);
            }

            /// <summary>
            /// Creates an immutable vertex buffer from the given vertex array.
            /// </summary>
            /// <typeparam name="T">Type of a vertex.</typeparam>
            /// <param name="device">Graphics device.</param>
            /// <param name="vertices">The vertex array.</param>
            public static unsafe D3D11.ID3D11Buffer CreateImmutableVertexBuffer<T>(EngineDevice device, params T[][] vertices)
                where T : unmanaged
            {
                device.EnsureNotNull(nameof(device));
                vertices.EnsureNotNull(nameof(vertices));

                var vertexCount = vertices.Sum(actArray => actArray.Length);
                var vertexSize = sizeof(T);
                using var outStream = new DataStream(
                    vertexCount * vertexSize,
                    true, true);

                foreach (var actArray in vertices)
                {
                    outStream.WriteRange(actArray);
                }
                outStream.Position = 0;

                var bufferDescription = new D3D11.BufferDescription
                {
                    BindFlags = D3D11.BindFlags.VertexBuffer,
                    CpuAccessFlags = D3D11.CpuAccessFlags.None,
                    OptionFlags = D3D11.ResourceOptionFlags.None,
                    SizeInBytes = vertexCount * vertexSize,
                    Usage = D3D11.ResourceUsage.Immutable,
                    StructureByteStride = vertexSize
                };

                var result = device.DeviceD3D11_1.CreateBuffer(bufferDescription, outStream.DataPointer);

                return result;
            }

            /// <summary>
            /// Creates an immutable index buffer from the given index array.
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="indices">Source index array.</param>
            public static D3D11.ID3D11Buffer CreateImmutableIndexBuffer(EngineDevice device, params int[][] indices)
            {
                device.EnsureNotNull(nameof(device));
                indices.EnsureNotNull(nameof(indices));

                const int BYTES_PER_INDEX = sizeof(uint);
                var countIndices = indices.Sum(actArray => actArray.Length);
                using var outStreamIndex = new DataStream(
                    countIndices *
                    BYTES_PER_INDEX, true, true);

                // Write all instance data to the target stream
                foreach (var actArray in indices)
                {
                    var actArrayLength = actArray.Length;

                    for (var loop = 0; loop < actArrayLength; loop++)
                    {
                        outStreamIndex.Write((uint)actArray[loop]);
                    }
                }

                outStreamIndex.Position = 0;

                // ConfigureLoading index buffer
                var bufferDescriptionIndex = new D3D11.BufferDescription
                {
                    BindFlags = D3D11.BindFlags.IndexBuffer,
                    CpuAccessFlags = D3D11.CpuAccessFlags.None,
                    OptionFlags = D3D11.ResourceOptionFlags.None,
                    SizeInBytes = countIndices * BYTES_PER_INDEX,
                    Usage = D3D11.ResourceUsage.Immutable
                };

                // Load the index buffer
                var result = device.DeviceD3D11_1.CreateBuffer(bufferDescriptionIndex, outStreamIndex.DataPointer);

                return result;
            }

            /// <summary>
            /// Gets a vertex shader resource pointing to given shader file.
            /// </summary>
            /// <param name="device">The target device object.</param>
            /// <param name="subdirectory">The subdirectory where the shader is located.</param>
            /// <param name="shaderNameWithoutExt">The name of the shader without extension.</param>
            public static VertexShaderResource GetVertexShaderResource(EngineDevice device, string subdirectory, string shaderNameWithoutExt)
            {
                device.EnsureNotNull(nameof(device));

                var resourceLink = GetShaderResourceLink(subdirectory, shaderNameWithoutExt);

                return new VertexShaderResource(device.DefaultVertexShaderModel, resourceLink);
            }

            /// <summary>
            /// Gets a geometry shader resource pointing to given shader file.
            /// </summary>
            /// <param name="device">The target device object.</param>
            /// <param name="subdirectory">The subdirectory where the shader is located.</param>
            /// <param name="shaderNameWithoutExt">The name of the shader without extension.</param>
            public static GeometryShaderResource GetGeometryShaderResource(EngineDevice device, string subdirectory, string shaderNameWithoutExt)
            {
                device.EnsureNotNull(nameof(device));

                var resourceLink = GetShaderResourceLink(subdirectory, shaderNameWithoutExt);

                return new GeometryShaderResource(device.DefaultGeometryShaderModel, resourceLink);
            }

            /// <summary>
            /// Gets a pixel shader resource pointing to given shader file.
            /// </summary>
            /// <param name="device">The target device object.</param>
            /// <param name="subdirectory">The subdirectory where the shader is located.</param>
            /// <param name="shaderNameWithoutExt">The name of the shader without extension.</param>
            public static PixelShaderResource GetPixelShaderResource(EngineDevice device, string subdirectory, string shaderNameWithoutExt)
            {
                device.EnsureNotNull(nameof(device));

                var resourceLink = GetShaderResourceLink(subdirectory, shaderNameWithoutExt);

                return new PixelShaderResource(device.DefaultPixelShaderModel, resourceLink);
            }

            public static AssemblyResourceLink GetShaderResourceLink(string subdirectory, string shaderNameWithoutExt)
            {
                AssemblyResourceLink resourceLink;
                if (string.IsNullOrEmpty(subdirectory))
                {
                    resourceLink = new AssemblyResourceLink(
                        typeof(SeeingSharpResources),
                        "Shaders",
                        $"{shaderNameWithoutExt}.hlsl");
                }
                else
                {
                    resourceLink = new AssemblyResourceLink(
                        typeof(SeeingSharpResources),
                        $"Shaders.{subdirectory}",
                        $"{shaderNameWithoutExt}.hlsl");
                }
                return resourceLink;
            }
        }
    }
}