/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Resources;
using SeeingSharp.Util;
using SeeingSharp.Util.SdxTK;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using D3D11 = SharpDX.Direct3D11;
using PixelFormat = SharpDX.WIC.PixelFormat;

namespace SeeingSharp.Multimedia.Core
{
    public static class GraphicsHelper
    {
        /// <summary>
        /// Creates a new image based on the given raw image data.
        /// </summary>
        /// <param name="device">Graphics device.</param>
        /// <param name="rawImage">Raw image data.</param>
        internal static D3D11.Texture2D CreateTexture(EngineDevice device, Image rawImage)
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
            if (rawImage.Description.Dimension == TextureDimension.TextureCube)
            {
                textureDescription.OptionFlags = D3D11.ResourceOptionFlags.TextureCube;
            }

            return new D3D11.Texture2D(device.DeviceD3D11_1, textureDescription, rawImage.ToDataBox());
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        public static class Internals
        {
            // All default texture formats
            public const Format DEFAULT_TEXTURE_FORMAT = Format.B8G8R8A8_UNorm;
            public const Format DEFAULT_TEXTURE_FORMAT_SHARING = Format.B8G8R8A8_UNorm;
            public const Format DEFAULT_TEXTURE_FORMAT_SHARING_D2D = Format.B8G8R8A8_UNorm;
            public const Format DEFAULT_TEXTURE_FORMAT_NORMAL_DEPTH = Format.R16G16B16A16_Float;
            public const Format DEFAULT_TEXTURE_FORMAT_OBJECT_ID = Format.R32_Float;
            public const Format DEFAULT_TEXTURE_FORMAT_DEPTH = Format.D32_Float_S8X24_UInt;
            public static readonly Guid DEFAULT_WIC_BITMAP_FORMAT = PixelFormat.Format32bppBGRA;
            public static readonly Guid DEFAULT_WIC_BITMAP_FORMAT_D2D = PixelFormat.Format32bppPBGRA;

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

                var bitmapDecoder = new BitmapDecoder(
                    GraphicsCore.Current.FactoryWIC,
                    inStream,
                    DecodeOptions.CacheOnDemand);

                var formatConverter = new FormatConverter(GraphicsCore.Current.FactoryWIC);
                formatConverter.Initialize(
                    bitmapDecoder.GetFrame(0),
                    Internals.DEFAULT_WIC_BITMAP_FORMAT,
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

                // Parameter changed to represent this article (important is the correct Direct2D format):
                // https://msdn.microsoft.com/en-us/library/windows/desktop/dd756686(v=vs.85).aspx

                var bitmapDecoder = new BitmapDecoder(
                    GraphicsCore.Current.FactoryWIC,
                    inStream,
                    DecodeOptions.CacheOnLoad);

                var formatConverter = new FormatConverter(GraphicsCore.Current.FactoryWIC);
                formatConverter.Initialize(
                    bitmapDecoder.GetFrame(0),
                    Internals.DEFAULT_WIC_BITMAP_FORMAT_D2D,
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
            public static D3D11.Texture2D CreateSharedTexture(EngineDevice device, int width, int height)
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

                return new D3D11.Texture2D(device.DeviceD3D11_1, textureDescription);
            }

            /// <summary>
            /// Loads the texture2D from stream.
            /// </summary>
            /// <param name="device">The device on which to create the texture.</param>
            /// <param name="inStream">The source stream.</param>
            /// <returns></returns>
            public static D3D11.Texture2D LoadTexture2D(EngineDevice device, Stream inStream)
            {
                using (var bitmapSourceWrapper = LoadBitmapSource(inStream))
                {
                    return LoadTexture2DFromBitmap(device, bitmapSourceWrapper);
                }
            }

            /// <summary>
            /// Loads a new texture from the given file path.
            /// </summary>
            /// <param name="device">The device on which to create the texture.</param>
            /// <param name="fileName">The source file</param>
            /// <returns></returns>
            public static D3D11.Texture2D LoadTexture2D(EngineDevice device, string fileName)
            {
                using (var bitmapSourceWrapper = LoadBitmapSource(fileName))
                {
                    return LoadTexture2DFromBitmap(device, bitmapSourceWrapper);
                }
            }

            public static D3D11.Texture2D LoadTexture2DFromMappedTexture(EngineDevice device, MemoryMappedTexture32bpp m_mappedTexture)
            {
                //Create the texture
                var dataRectangle = new SharpDX.DataRectangle(
                    m_mappedTexture.Pointer,
                    m_mappedTexture.Width * 4);
                var result = new D3D11.Texture2D(device.DeviceD3D11_1, new D3D11.Texture2DDescription
                {
                    Width = m_mappedTexture.Width,
                    Height = m_mappedTexture.Height,
                    ArraySize = 1,
                    BindFlags = D3D11.BindFlags.ShaderResource | D3D11.BindFlags.RenderTarget,
                    Usage = D3D11.ResourceUsage.Default,
                    CpuAccessFlags = D3D11.CpuAccessFlags.None,
                    Format = DEFAULT_TEXTURE_FORMAT,
                    MipLevels = 0,
                    OptionFlags = D3D11.ResourceOptionFlags.None | D3D11.ResourceOptionFlags.GenerateMipMaps,
                    SampleDescription = new SampleDescription(1, 0)
                }, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle);

                //Workaround for now... auto generate mip-levels
                // TODO: Dispatch this call to render-thread..
                using (var shaderResourceView = new D3D11.ShaderResourceView(device.DeviceD3D11_1, result))
                {
                    device.DeviceImmediateContextD3D11.GenerateMips(shaderResourceView);
                }

                return result;
            }

            /// <summary>
            /// Creates a <see cref="SharpDX.Direct3D11.Texture2D"/> from a WIC <see cref="SharpDX.WIC.BitmapSource"/>
            /// </summary>
            /// <param name="device">The Direct3D11 device</param>
            /// <param name="bitmapSourceWrapper">The WIC bitmap source</param>
            /// <returns>A Texture2D</returns>
            public static D3D11.Texture2D LoadTexture2DFromBitmap(EngineDevice device, WicBitmapSourceInternal bitmapSourceWrapper)
            {
                device.EnsureNotNull(nameof(device));
                bitmapSourceWrapper.EnsureNotNullOrDisposed(nameof(bitmapSourceWrapper));

                BitmapSource bitmapSource = bitmapSourceWrapper.Converter;

                // Allocate DataStream to receive the WIC image pixels
                var stride = bitmapSource.Size.Width * 4;
                using (var buffer = new SharpDX.DataStream(bitmapSource.Size.Height * stride, true, true))
                {
                    // Copy the content of the WIC to the buffer
                    bitmapSource.CopyPixels(stride, buffer);

                    //Create the texture
                    var dataRectangle = new SharpDX.DataRectangle(buffer.DataPointer, stride);

                    var result = new D3D11.Texture2D(device.DeviceD3D11_1, new D3D11.Texture2DDescription
                    {
                        Width = bitmapSource.Size.Width,
                        Height = bitmapSource.Size.Height,
                        ArraySize = 1,
                        BindFlags = D3D11.BindFlags.ShaderResource | D3D11.BindFlags.RenderTarget,
                        Usage = D3D11.ResourceUsage.Default,
                        CpuAccessFlags = D3D11.CpuAccessFlags.None,
                        Format = DEFAULT_TEXTURE_FORMAT,
                        MipLevels = 0,
                        OptionFlags = D3D11.ResourceOptionFlags.None | D3D11.ResourceOptionFlags.GenerateMipMaps,
                        SampleDescription = new SampleDescription(1, 0)
                    }, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle, dataRectangle);

                    //Workaround for now... auto generate mip-levels
                    // TODO: Dispatch this call to render-thread..
                    using (var shaderResourceView = new D3D11.ShaderResourceView(device.DeviceD3D11_1, result))
                    {
                        device.DeviceImmediateContextD3D11.GenerateMips(shaderResourceView);
                    }

                    //Return the generated texture
                    return result;
                }
            }

            /// <summary>
            /// Creates a default viewport for the given width and height
            /// </summary>
            public static RawViewportF CreateDefaultViewport(int width, int height)
            {
                width.EnsurePositiveOrZero(nameof(width));
                height.EnsurePositiveOrZero(nameof(height));

                var result = new RawViewportF
                {
                    X = 0f,
                    Y = 0f,
                    Width = width,
                    Height = height,
                    MinDepth = 0f,
                    MaxDepth = 1f
                };

                return result;
            }

            /// <summary>
            /// Creates a standard texture with the given width and height.
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="width">Width of generated texture.</param>
            /// <param name="height">Height of generated texture.</param>
            /// <param name="format">The format which is used to create the texture.</param>
            public static D3D11.Texture2D CreateTexture(EngineDevice device, int width, int height, Format format = DEFAULT_TEXTURE_FORMAT)
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

                return new D3D11.Texture2D(device.DeviceD3D11_1, textureDescription);
            }

            /// <summary>
            /// Creates a standard texture with the given width and height.
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="width">Width of generated texture.</param>
            /// <param name="height">Height of generated texture.</param>
            /// <param name="format">The format which is used to create the texture.</param>
            public static D3D11.Texture2D CreateCpuWritableTexture(EngineDevice device, int width, int height, Format format = DEFAULT_TEXTURE_FORMAT)
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

                return new D3D11.Texture2D(device.DeviceD3D11_1, textureDescription);
            }

            /// <summary>
            /// Creates a standard texture with the given width and height.
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="width">Width of generated texture.</param>
            /// <param name="height">Height of generated texture.</param>
            /// <param name="rawData">Raw data to be loaded into the texture.</param>
            public static D3D11.Texture2D CreateTexture(EngineDevice device, int width, int height, SharpDX.DataBox[] rawData)
            {
                device.EnsureNotNull(nameof(device));
                width.EnsurePositiveOrZero(nameof(width));
                height.EnsurePositiveOrZero(nameof(height));
                rawData.EnsureNotNull(nameof(rawData));

                var textureDescription = new D3D11.Texture2DDescription
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = DEFAULT_TEXTURE_FORMAT,
                    Usage = D3D11.ResourceUsage.Default,
                    SampleDescription = new SampleDescription(1, 0),
                    BindFlags = D3D11.BindFlags.ShaderResource,
                    CpuAccessFlags = D3D11.CpuAccessFlags.None,
                    OptionFlags = D3D11.ResourceOptionFlags.None
                };

                return new D3D11.Texture2D(device.DeviceD3D11_1, textureDescription, rawData);
            }

            /// <summary>
            /// Creates a staging texture which enables copying data from gpu to cpu memory.
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="width">Width of generated texture.</param>
            /// <param name="height">Height of generated texture.</param>
            /// <param name="format">The format used to create the texture.</param>
            public static D3D11.Texture2D CreateStagingTexture(EngineDevice device, int width, int height, Format format = DEFAULT_TEXTURE_FORMAT)
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

                return new D3D11.Texture2D(device.DeviceD3D11_1, textureDescription);
            }

            /// <summary>
            /// Creates a staging texture which enables copying data from gpu to cpu memory.
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="width">Width of generated texture.</param>
            /// <param name="height">Height of generated texture.</param>
            public static D3D11.Texture2D CreateStagingTexture(EngineDevice device, int width, int height)
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

                return new D3D11.Texture2D(device.DeviceD3D11_1, textureDescription);
            }

            /// <summary>
            /// Creates a render target texture with the given width and height.
            /// This texture is used to receive normal and depth data (xyzw components) and stores data in floating point format.
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="width">Width of generated texture.</param>
            /// <param name="height">Height of generated texture.</param>
            /// <param name="gfxConfig">The GFX configuration.</param>
            public static D3D11.Texture2D CreateRenderTargetTextureNormalDepth(
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

                return new D3D11.Texture2D(device.DeviceD3D11_1, textureDescription);
            }

            /// <summary>
            /// Creates a render target texture with the given width and height.
            /// This texture is used to receive ObjectIDs and stores data as single unsigned integers (32-Bit).
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="width">Width of generated texture.</param>
            /// <param name="height">Height of generated texture.</param>
            /// <param name="gfxConfig">The GFX configuration.</param>
            public static D3D11.Texture2D CreateRenderTargetTextureObjectIDs(
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

                return new D3D11.Texture2D(device.DeviceD3D11_1, textureDescription);
            }

            /// <summary>
            /// Creates a render target texture with the given width and height.
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="width">Width of generated texture.</param>
            /// <param name="height">Height of generated texture.</param>
            /// <param name="gfxConfig">The GFX configuration.</param>
            public static D3D11.Texture2D CreateRenderTargetTexture(
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

                return new D3D11.Texture2D(device.DeviceD3D11_1, textureDescription);
            }

            /// <summary>
            /// Creates a depth buffer texture with given width and height.
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="width">Width of generated texture.</param>
            /// <param name="height">Height of generated texture.</param>
            /// <param name="gfxConfig">Current graphics configuration.</param>
            public static D3D11.Texture2D CreateDepthBufferTexture(EngineDevice device, int width, int height, GraphicsViewConfiguration gfxConfig)
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
                switch (device.DriverLevel)
                {
                    case HardwareDriverLevel.Direct3D12:
                    case HardwareDriverLevel.Direct3D11:
                    case HardwareDriverLevel.Direct3D10:
                        textureDescription.Format = Format.D32_Float_S8X24_UInt;
                        break;

                    default:
                        throw new SeeingSharpGraphicsException("Unable to create depth buffer texture because of unsupported DriverLevel: " + device.DriverLevel);
                }

                // Create the texture finally
                return new D3D11.Texture2D(device.DeviceD3D11_1, textureDescription);
            }

            /// <summary>
            /// Create a new DepthBuffer view to bind the given depth buffer to the rendering device.
            /// </summary>
            /// <param name="device">The device on which to create the view.</param>
            /// <param name="depthBuffer">The target resource.</param>
            public static D3D11.DepthStencilView CreateDepthBufferView(EngineDevice device, D3D11.Texture2D depthBuffer)
            {
                device.EnsureNotNull(nameof(device));
                depthBuffer.EnsureNotNullOrDisposed(nameof(depthBuffer));

                return new D3D11.DepthStencilView(device.DeviceD3D11_1, depthBuffer);
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
            public static D3D11.Buffer CreateDynamicVertexBuffer<T>(EngineDevice device, int vertexCount)
                where T : struct
            {
                device.EnsureNotNull(nameof(device));
                vertexCount.EnsurePositiveOrZero(nameof(vertexCount));

                var vertexType = typeof(T);
                var vertexSize = Marshal.SizeOf<T>();

                var bufferDescription = new D3D11.BufferDescription
                {
                    BindFlags = D3D11.BindFlags.VertexBuffer,
                    CpuAccessFlags = D3D11.CpuAccessFlags.Write,
                    OptionFlags = D3D11.ResourceOptionFlags.None,
                    SizeInBytes = vertexCount * vertexSize,
                    Usage = D3D11.ResourceUsage.Dynamic,
                    StructureByteStride = vertexCount * vertexSize
                };

                return new D3D11.Buffer(device.DeviceD3D11_1, bufferDescription);
            }

            /// <summary>
            /// Creates an immutable vertex buffer from the given vertex array.
            /// </summary>
            /// <typeparam name="T">Type of a vertex.</typeparam>
            /// <param name="device">Graphics device.</param>
            /// <param name="vertices">The vertex array.</param>
            public static D3D11.Buffer CreateImmutableVertexBuffer<T>(EngineDevice device, params T[][] vertices)
                where T : struct
            {
                device.EnsureNotNull(nameof(device));
                vertices.EnsureNotNull(nameof(vertices));

                var vertexType = typeof(T);
                var vertexCount = vertices.Sum(actArray => actArray.Length);
                var vertexSize = Marshal.SizeOf<T>();
                var outStream = new SharpDX.DataStream(
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

                var result = new D3D11.Buffer(device.DeviceD3D11_1, outStream, bufferDescription);
                outStream.Dispose();

                return result;
            }

            /// <summary>
            /// Creates an immutable index buffer from the given index array.
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="indices">Source index array.</param>
            public static D3D11.Buffer CreateImmutableIndexBuffer(EngineDevice device, params int[][] indices)
            {
                device.EnsureNotNull(nameof(device));
                indices.EnsureNotNull(nameof(indices));

                var countIndices = indices.Sum(actArray => actArray.Length);
                var bytesPerIndex = Marshal.SizeOf<uint>();

                var outStreamIndex = new SharpDX.DataStream(
                    countIndices *
                    bytesPerIndex, true, true);

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
                    SizeInBytes = countIndices * bytesPerIndex,
                    Usage = D3D11.ResourceUsage.Immutable
                };

                // Load the index buffer
                var result = new D3D11.Buffer(device.DeviceD3D11_1, outStreamIndex, bufferDescriptionIndex);

                outStreamIndex.Dispose();

                return result;
            }

            /// <summary>
            /// Creates a default texture sampler state.
            /// </summary>
            /// <param name="device">The device to create the state for.</param>
            /// <param name="samplerQualityLevel">The target sampler quality</param>
            public static D3D11.SamplerState CreateDefaultTextureSampler(EngineDevice device, TextureSamplerQualityLevel samplerQualityLevel)
            {
                device.EnsureNotNull(nameof(device));

                // Set state parameters
                var samplerDesk = D3D11.SamplerStateDescription.Default();
                switch (device.DriverLevel)
                {
                    case HardwareDriverLevel.Direct3D11:
                    case HardwareDriverLevel.Direct3D10:
                        switch (samplerQualityLevel)
                        {
                            case TextureSamplerQualityLevel.High:
                                if (!device.IsHighDetailSupported) { goto case TextureSamplerQualityLevel.Low; }
                                samplerDesk.AddressU = D3D11.TextureAddressMode.Wrap;
                                samplerDesk.AddressV = D3D11.TextureAddressMode.Wrap;
                                samplerDesk.Filter = D3D11.Filter.Anisotropic;
                                samplerDesk.MaximumAnisotropy = 16;
                                break;

                            case TextureSamplerQualityLevel.Medium:
                                if (!device.IsHighDetailSupported) { goto case TextureSamplerQualityLevel.Low; }
                                samplerDesk.AddressU = D3D11.TextureAddressMode.Wrap;
                                samplerDesk.AddressV = D3D11.TextureAddressMode.Wrap;
                                samplerDesk.Filter = D3D11.Filter.Anisotropic;
                                samplerDesk.MaximumAnisotropy = 8;
                                break;

                            case TextureSamplerQualityLevel.Low:
                                samplerDesk.AddressU = D3D11.TextureAddressMode.Wrap;
                                samplerDesk.AddressV = D3D11.TextureAddressMode.Wrap;
                                samplerDesk.Filter = D3D11.Filter.MinMagMipLinear;
                                break;
                        }
                        break;

                    default:
                        samplerDesk.AddressU = D3D11.TextureAddressMode.Wrap;
                        samplerDesk.AddressV = D3D11.TextureAddressMode.Wrap;
                        samplerDesk.Filter = D3D11.Filter.MinMagMipLinear;
                        break;
                }

                // Create the state object finally
                return new D3D11.SamplerState(device.DeviceD3D11_1, samplerDesk);
            }

            /// <summary>
            /// Creates a render target texture with the given width and height.
            /// </summary>
            /// <param name="device">Graphics device.</param>
            /// <param name="width">Width of generated texture.</param>
            /// <param name="height">Height of generated texture.</param>
            public static D3D11.Texture2D CreateRenderTargetTextureDummy(D3D11.Device device, int width, int height)
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
                    Format = DEFAULT_TEXTURE_FORMAT,
                    Usage = D3D11.ResourceUsage.Default,
                    SampleDescription = new SampleDescription(1, 0),
                    BindFlags = D3D11.BindFlags.ShaderResource | D3D11.BindFlags.RenderTarget,
                    CpuAccessFlags = D3D11.CpuAccessFlags.None,
                    OptionFlags = D3D11.ResourceOptionFlags.None
                };

                return new D3D11.Texture2D(device, textureDescription);
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
                AssemblyResourceLink resourceLink = null;
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