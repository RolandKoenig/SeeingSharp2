// This code is ported from SharpDX.Toolkit
// see: https://github.com/sharpdx/Toolkit

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using SeeingSharp.Util.Sdx;
using SharpGen.Runtime.Win32;
using Vortice.DXGI;
using Vortice.WIC;

#nullable disable

namespace SeeingSharp.Util.SdxTK
{
    internal class WICHelper
    {
        private static IWICImagingFactory _factory = new IWICImagingFactory();

        private static readonly WICTranslate[] WICToDXGIFormats =
            {
                new WICTranslate(Vortice.WIC.PixelFormat.Format128bppRGBAFloat, Format.R32G32B32A32_Float),

                new WICTranslate(Vortice.WIC.PixelFormat.Format64bppRGBAHalf, Format.R16G16B16A16_Float),
                new WICTranslate(Vortice.WIC.PixelFormat.Format64bppRGBA, Format.R16G16B16A16_UNorm),

                new WICTranslate(Vortice.WIC.PixelFormat.Format32bppRGBA, Format.R8G8B8A8_UNorm),
                new WICTranslate(Vortice.WIC.PixelFormat.Format32bppBGRA, Format.B8G8R8A8_UNorm), // DXGI 1.1
                new WICTranslate(Vortice.WIC.PixelFormat.Format32bppBGR, Format.B8G8R8X8_UNorm), // DXGI 1.1

                new WICTranslate(Vortice.WIC.PixelFormat.Format32bppRGBA1010102XR, Format.R10G10B10_Xr_Bias_A2_UNorm), // DXGI 1.1
                new WICTranslate(Vortice.WIC.PixelFormat.Format32bppRGBA1010102, Format.R10G10B10A2_UNorm),
                new WICTranslate(Vortice.WIC.PixelFormat.Format32bppRGBE, Format.R9G9B9E5_SharedExp),

                new WICTranslate(Vortice.WIC.PixelFormat.Format16bppBGRA5551, Format.B5G5R5A1_UNorm),
                new WICTranslate(Vortice.WIC.PixelFormat.Format16bppBGR565, Format.B5G6R5_UNorm),

                new WICTranslate(Vortice.WIC.PixelFormat.Format32bppGrayFloat, Format.R32_Float),
                new WICTranslate(Vortice.WIC.PixelFormat.Format16bppGrayHalf, Format.R16_Float),
                new WICTranslate(Vortice.WIC.PixelFormat.Format16bppGray, Format.R16_UNorm),
                new WICTranslate(Vortice.WIC.PixelFormat.Format8bppGray, Format.R8_UNorm),

                new WICTranslate(Vortice.WIC.PixelFormat.Format8bppAlpha, Format.A8_UNorm),

                new WICTranslate(Vortice.WIC.PixelFormat.FormatBlackWhite, Format.R1_UNorm)
            };

        private static readonly WICConvert[] WICConvertTable =
            {
                // Directly support the formats listed in XnaTexUtil::g_WICFormats, so no conversion required
                // Note target Guid in this conversion table must be one of those directly supported formats.

                new WICConvert(Vortice.WIC.PixelFormat.Format1bppIndexed, Vortice.WIC.PixelFormat.Format32bppRGBA), // DXGI.Format.R8G8B8A8_UNorm
                new WICConvert(Vortice.WIC.PixelFormat.Format2bppIndexed, Vortice.WIC.PixelFormat.Format32bppRGBA), // DXGI.Format.R8G8B8A8_UNorm
                new WICConvert(Vortice.WIC.PixelFormat.Format4bppIndexed, Vortice.WIC.PixelFormat.Format32bppRGBA), // DXGI.Format.R8G8B8A8_UNorm
                new WICConvert(Vortice.WIC.PixelFormat.Format8bppIndexed, Vortice.WIC.PixelFormat.Format32bppRGBA), // DXGI.Format.R8G8B8A8_UNorm

                new WICConvert(Vortice.WIC.PixelFormat.Format2bppGray, Vortice.WIC.PixelFormat.Format8bppGray), // DXGI.Format.R8_UNorm
                new WICConvert(Vortice.WIC.PixelFormat.Format4bppGray, Vortice.WIC.PixelFormat.Format8bppGray), // DXGI.Format.R8_UNorm

                new WICConvert(Vortice.WIC.PixelFormat.Format16bppGrayFixedPoint, Vortice.WIC.PixelFormat.Format16bppGrayHalf), // DXGI.Format.R16_FLOAT
                new WICConvert(Vortice.WIC.PixelFormat.Format32bppGrayFixedPoint, Vortice.WIC.PixelFormat.Format32bppGrayFloat), // DXGI.Format.R32_FLOAT

                new WICConvert(Vortice.WIC.PixelFormat.Format16bppBGR555, Vortice.WIC.PixelFormat.Format16bppBGRA5551), // DXGI.Format.B5G5R5A1_UNorm
                new WICConvert(Vortice.WIC.PixelFormat.Format32bppBGR101010, Vortice.WIC.PixelFormat.Format32bppRGBA1010102), // DXGI.Format.R10G10B10A2_UNorm

                new WICConvert(Vortice.WIC.PixelFormat.Format24bppBGR, Vortice.WIC.PixelFormat.Format32bppRGBA), // DXGI.Format.R8G8B8A8_UNorm
                new WICConvert(Vortice.WIC.PixelFormat.Format24bppRGB, Vortice.WIC.PixelFormat.Format32bppRGBA), // DXGI.Format.R8G8B8A8_UNorm
                new WICConvert(Vortice.WIC.PixelFormat.Format32bppPBGRA, Vortice.WIC.PixelFormat.Format32bppRGBA), // DXGI.Format.R8G8B8A8_UNorm
                new WICConvert(Vortice.WIC.PixelFormat.Format32bppPRGBA, Vortice.WIC.PixelFormat.Format32bppRGBA), // DXGI.Format.R8G8B8A8_UNorm

                new WICConvert(Vortice.WIC.PixelFormat.Format48bppRGB, Vortice.WIC.PixelFormat.Format64bppRGBA), // DXGI.Format.R16G16B16A16_UNorm
                new WICConvert(Vortice.WIC.PixelFormat.Format48bppBGR, Vortice.WIC.PixelFormat.Format64bppRGBA), // DXGI.Format.R16G16B16A16_UNorm
                new WICConvert(Vortice.WIC.PixelFormat.Format64bppBGRA, Vortice.WIC.PixelFormat.Format64bppRGBA), // DXGI.Format.R16G16B16A16_UNorm
                new WICConvert(Vortice.WIC.PixelFormat.Format64bppPRGBA, Vortice.WIC.PixelFormat.Format64bppRGBA), // DXGI.Format.R16G16B16A16_UNorm
                new WICConvert(Vortice.WIC.PixelFormat.Format64bppPBGRA, Vortice.WIC.PixelFormat.Format64bppRGBA), // DXGI.Format.R16G16B16A16_UNorm

                new WICConvert(Vortice.WIC.PixelFormat.Format48bppRGBFixedPoint, Vortice.WIC.PixelFormat.Format64bppRGBAHalf), // DXGI.Format.R16G16B16A16_FLOAT
                new WICConvert(Vortice.WIC.PixelFormat.Format48bppBGRFixedPoint, Vortice.WIC.PixelFormat.Format64bppRGBAHalf), // DXGI.Format.R16G16B16A16_FLOAT
                new WICConvert(Vortice.WIC.PixelFormat.Format64bppRGBAFixedPoint, Vortice.WIC.PixelFormat.Format64bppRGBAHalf), // DXGI.Format.R16G16B16A16_FLOAT
                new WICConvert(Vortice.WIC.PixelFormat.Format64bppBGRAFixedPoint, Vortice.WIC.PixelFormat.Format64bppRGBAHalf), // DXGI.Format.R16G16B16A16_FLOAT
                new WICConvert(Vortice.WIC.PixelFormat.Format64bppRGBFixedPoint, Vortice.WIC.PixelFormat.Format64bppRGBAHalf), // DXGI.Format.R16G16B16A16_FLOAT
                new WICConvert(Vortice.WIC.PixelFormat.Format64bppRGBHalf, Vortice.WIC.PixelFormat.Format64bppRGBAHalf), // DXGI.Format.R16G16B16A16_FLOAT
                new WICConvert(Vortice.WIC.PixelFormat.Format48bppRGBHalf, Vortice.WIC.PixelFormat.Format64bppRGBAHalf), // DXGI.Format.R16G16B16A16_FLOAT

                new WICConvert(Vortice.WIC.PixelFormat.Format128bppPRGBAFloat, Vortice.WIC.PixelFormat.Format128bppRGBAFloat), // DXGI.Format.R32G32B32A32_FLOAT
                new WICConvert(Vortice.WIC.PixelFormat.Format128bppRGBFloat, Vortice.WIC.PixelFormat.Format128bppRGBAFloat), // DXGI.Format.R32G32B32A32_FLOAT
                new WICConvert(Vortice.WIC.PixelFormat.Format128bppRGBAFixedPoint, Vortice.WIC.PixelFormat.Format128bppRGBAFloat), // DXGI.Format.R32G32B32A32_FLOAT
                new WICConvert(Vortice.WIC.PixelFormat.Format128bppRGBFixedPoint, Vortice.WIC.PixelFormat.Format128bppRGBAFloat), // DXGI.Format.R32G32B32A32_FLOAT

                new WICConvert(Vortice.WIC.PixelFormat.Format32bppCMYK, Vortice.WIC.PixelFormat.Format32bppRGBA), // DXGI.Format.R8G8B8A8_UNorm
                new WICConvert(Vortice.WIC.PixelFormat.Format64bppCMYK, Vortice.WIC.PixelFormat.Format64bppRGBA), // DXGI.Format.R16G16B16A16_UNorm
                new WICConvert(Vortice.WIC.PixelFormat.Format40bppCMYKAlpha, Vortice.WIC.PixelFormat.Format64bppRGBA), // DXGI.Format.R16G16B16A16_UNorm
                new WICConvert(Vortice.WIC.PixelFormat.Format80bppCMYKAlpha, Vortice.WIC.PixelFormat.Format64bppRGBA), // DXGI.Format.R16G16B16A16_UNorm

                new WICConvert(Vortice.WIC.PixelFormat.Format96bppRGBFixedPoint, Vortice.WIC.PixelFormat.Format128bppRGBAFloat) // DXGI.Format.R32G32B32A32_FLOAT

                // We don't support n-channel formats
            };

        private static IWICImagingFactory Factory => _factory ?? (_factory = new IWICImagingFactory());

        public static void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref _factory);
        }

        //-------------------------------------------------------------------------------------
        // Load a WIC-supported file in memory
        //-------------------------------------------------------------------------------------
        internal static Image LoadFromWICMemory(IntPtr pSource, int size, bool makeACopy, GCHandle? handle)
        {
            var flags = WICFlags.AllFrames;

            Image image = null;
            // Create input stream for memory
            using (var stream = Factory.CreateStream(new DataStream(pSource, size, true, false)))
            {
                // If the decoder is unable to decode the image, than return null
                IWICBitmapDecoder decoder = null;
                try
                {
                    decoder = Factory.CreateDecoderFromStream(stream, DecodeOptions.CacheOnDemand);
                    using (var frame = decoder.GetFrame(0))
                    {
                        // Get metadata
                        Guid convertGuid;
                        var tempDesc = DecodeMetadata(flags, decoder, frame, out convertGuid);

                        // If not supported.
                        if (!tempDesc.HasValue)
                        {
                            return null;
                        }

                        var mdata = tempDesc.Value;

                        if (mdata.ArraySize > 1 && (flags & WICFlags.AllFrames) != 0)
                        {
                            return DecodeMultiframe(flags, mdata, decoder);
                        }

                        image = DecodeSingleFrame(flags, mdata, convertGuid, frame);
                    }
                }
                catch
                {
                    image = null;
                }
                finally
                {
                    if (decoder != null)
                    {
                        decoder.Dispose();
                    }
                }
            }

            // For WIC, we are not keeping the original buffer.
            if (image != null && !makeACopy)
            {
                if (handle.HasValue)
                {
                    handle.Value.Free();
                }
                else
                {
                    SdxUtilities.FreeMemory(pSource);
                }
            }
            return image;
        }

        internal static void SaveGifToWICMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            SaveToWICMemory(pixelBuffers, count, WICFlags.AllFrames, ImageFileType.Gif, imageStream);
        }

        internal static void SaveTiffToWICMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            SaveToWICMemory(pixelBuffers, count, WICFlags.AllFrames, ImageFileType.Tiff, imageStream);
        }

        internal static void SaveBmpToWICMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            SaveToWICMemory(pixelBuffers, 1, WICFlags.None, ImageFileType.Bmp, imageStream);
        }

        internal static void SaveJpgToWICMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            SaveToWICMemory(pixelBuffers, 1, WICFlags.None, ImageFileType.Jpg, imageStream);
        }

        internal static void SavePngToWICMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            SaveToWICMemory(pixelBuffers, 1, WICFlags.None, ImageFileType.Png, imageStream);
        }

        internal static void SaveWmpToWICMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            SaveToWICMemory(pixelBuffers, 1, WICFlags.None, ImageFileType.Wmp, imageStream);
        }

        /// <summary>
        /// Converts a WIC <see cref="Vortice.WIC.PixelFormat"/> to a <see cref="Vortice.DXGI.Format"/>.
        /// </summary>
        /// <param name="guid">A WIC <see cref="Vortice.WIC.PixelFormat"/> </param>
        /// <returns>A <see cref="Vortice.DXGI.Format"/></returns>
        private static Format ToDXGI(Guid guid)
        {
            for (var i = 0; i < WICToDXGIFormats.Length; ++i)
            {
                if (WICToDXGIFormats[i].WIC == guid)
                {
                    return WICToDXGIFormats[i].Format;
                }
            }

            return Format.Unknown;
        }

        /// <summary>
        /// Converts a <see cref="Vortice.DXGI.Format"/> to a a WIC <see cref="Vortice.WIC.PixelFormat"/>.
        /// </summary>
        /// <param name="format">A <see cref="Vortice.DXGI.Format"/></param>
        /// <param name="guid">A WIC <see cref="Vortice.WIC.PixelFormat"/> Guid.</param>
        /// <returns>True if conversion succeed, false otherwise.</returns>
        private static bool ToWIC(Format format, out Guid guid)
        {
            for (var i = 0; i < WICToDXGIFormats.Length; ++i)
            {
                if (WICToDXGIFormats[i].Format == format)
                {
                    guid = WICToDXGIFormats[i].WIC;
                    return true;
                }
            }

            // Special cases
            switch (format)
            {
                case Format.R8G8B8A8_UNorm_SRgb:
                    guid = Vortice.WIC.PixelFormat.Format32bppRGBA;
                    return true;

                case Format.D32_Float:
                    guid = Vortice.WIC.PixelFormat.Format32bppGrayFloat;
                    return true;

                case Format.D16_UNorm:
                    guid = Vortice.WIC.PixelFormat.Format16bppGray;
                    return true;

                case Format.B8G8R8A8_UNorm_SRgb:
                    guid = Vortice.WIC.PixelFormat.Format32bppBGRA;
                    return true;

                case Format.B8G8R8X8_UNorm_SRgb:
                    guid = Vortice.WIC.PixelFormat.Format32bppBGR;
                    return true;
            }

            guid = Guid.Empty;
            return false;
        }

        /// <summary>
        /// Gets the number of bits per pixels for a WIC <see cref="Vortice.WIC.PixelFormat"/> Guid.
        /// </summary>
        /// <param name="targetGuid">A WIC <see cref="Vortice.WIC.PixelFormat"/> Guid.</param>
        /// <returns>The number of bits per pixels for a WIC. If this method is failing to calculate the number of pixels, return 0.</returns>
        private static int GetBitsPerPixel(Guid targetGuid)
        {
            using (var info = Factory.CreateComponentInfo(targetGuid))
            {
                if (info.ComponentType != ComponentType.PixelFormat)
                {
                    return 0;
                }

                var pixelFormatInfo = info.QueryInterfaceOrNull<IWICPixelFormatInfo>();
                if (pixelFormatInfo == null)
                {
                    return 0;
                }

                var bpp = pixelFormatInfo.BitsPerPixel;
                pixelFormatInfo.Dispose();
                return bpp;
            }
        }

        //-------------------------------------------------------------------------------------
        // Returns the DXGI format and optionally the WIC pixel Guid to convert to
        //-------------------------------------------------------------------------------------
        private static Format DetermineFormat(Guid pixelFormat, WICFlags flags, out Guid pixelFormatOut)
        {
            var format = ToDXGI(pixelFormat);
            pixelFormatOut = Guid.Empty;

            if (format == Format.Unknown)
            {
                for (var i = 0; i < WICConvertTable.Length; ++i)
                {
                    if (WICConvertTable[i].source == pixelFormat)
                    {
                        pixelFormatOut = WICConvertTable[i].target;

                        format = ToDXGI(WICConvertTable[i].target);
                        Debug.Assert(format != Format.Unknown);
                        break;
                    }
                }
            }

            // Handle special cases based on flags
            switch (format)
            {
                case Format.B8G8R8A8_UNorm: // BGRA
                case Format.B8G8R8X8_UNorm: // BGRX
                    if ((flags & WICFlags.ForceRgb) != 0)
                    {
                        format = Format.R8G8B8A8_UNorm;
                        pixelFormatOut = Vortice.WIC.PixelFormat.Format32bppRGBA;
                    }
                    break;

                case Format.R10G10B10_Xr_Bias_A2_UNorm:
                    if ((flags & WICFlags.NoX2Bias) != 0)
                    {
                        format = Format.R10G10B10A2_UNorm;
                        pixelFormatOut = Vortice.WIC.PixelFormat.Format32bppRGBA1010102;
                    }
                    break;

                case Format.B5G5R5A1_UNorm:
                case Format.B5G6R5_UNorm:
                    if ((flags & WICFlags.No16Bpp) != 0)
                    {
                        format = Format.R8G8B8A8_UNorm;
                        pixelFormatOut = Vortice.WIC.PixelFormat.Format32bppRGBA;
                    }
                    break;

                case Format.R1_UNorm:
                    if ((flags & WICFlags.FlagsAllowMono) == 0)
                    {
                        // By default we want to promote a black & white to greyscale since R1 is not a generally supported D3D format
                        format = Format.R8_UNorm;
                        pixelFormatOut = Vortice.WIC.PixelFormat.Format8bppGray;
                    }
                    break;
            }

            return format;
        }

        /// <summary>
        /// Determines metadata for image
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="decoder">The decoder.</param>
        /// <param name="frame">The frame.</param>
        /// <param name="pixelFormat">The pixel format.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">If pixel format is not supported.</exception>
        private static ImageDescription? DecodeMetadata(WICFlags flags, IWICBitmapDecoder decoder, IWICBitmapFrameDecode frame, out Guid pixelFormat)
        {
            var size = frame.Size;

            var metadata = new ImageDescription
            {
                Dimension = TextureDimension.Texture2D,
                Width = size.Width,
                Height = size.Height,
                Depth = 1,
                MipLevels = 1,
                ArraySize = (flags & WICFlags.AllFrames) != 0 ? decoder.FrameCount : 1,
                Format = DetermineFormat(frame.PixelFormat, flags, out pixelFormat)
            };

            if (metadata.Format == Format.Unknown)
            {
                return null;
            }

            return metadata;
        }

        private static BitmapDitherType GetWICDither(WICFlags flags)
        {
            if ((flags & WICFlags.Dither) != 0)
            {
                return BitmapDitherType.Ordered4x4;
            }

            if ((flags & WICFlags.DitherDiffusion) != 0)
            {
                return BitmapDitherType.ErrorDiffusion;
            }

            return BitmapDitherType.None;
        }

        private static BitmapInterpolationMode GetWICInterp(WICFlags flags)
        {
            if ((flags & WICFlags.FilterPoint) != 0)
            {
                return BitmapInterpolationMode.NearestNeighbor;
            }

            if ((flags & WICFlags.FilterLinear) != 0)
            {
                return BitmapInterpolationMode.Linear;
            }

            if ((flags & WICFlags.FilterCubic) != 0)
            {
                return BitmapInterpolationMode.Cubic;
            }

            return BitmapInterpolationMode.Fant;
        }

        //-------------------------------------------------------------------------------------
        // Decodes a single frame
        //-------------------------------------------------------------------------------------
        private static Image DecodeSingleFrame(WICFlags flags, ImageDescription metadata, Guid convertGuid, IWICBitmapFrameDecode frame)
        {
            var image = Image.New(metadata);

            var pixelBuffer = image.PixelBuffer[0];

            if (convertGuid == Guid.Empty)
            {
                frame.CopyPixels(pixelBuffer.RowStride, pixelBuffer.BufferStride, pixelBuffer.DataPointer);
            }
            else
            {
                using (var converter = Factory.CreateFormatConverter())
                {
                    converter.Initialize(frame, convertGuid, GetWICDither(flags), null, 0, BitmapPaletteType.Custom);
                    converter.CopyPixels(pixelBuffer.RowStride, pixelBuffer.BufferStride, pixelBuffer.DataPointer);
                }
            }

            return image;
        }

        //-------------------------------------------------------------------------------------
        // Decodes an image array, resizing/format converting as needed
        //-------------------------------------------------------------------------------------
        private static Image DecodeMultiframe(WICFlags flags, ImageDescription metadata, IWICBitmapDecoder decoder)
        {
            var image = Image.New(metadata);

            Guid sourceGuid;
            if (!ToWIC(metadata.Format, out sourceGuid))
            {
                return null;
            }

            for (var index = 0; index < metadata.ArraySize; ++index)
            {
                var pixelBuffer = image.PixelBuffer[index, 0];

                using (var frame = decoder.GetFrame(index))
                {
                    var pfGuid = frame.PixelFormat;
                    var size = frame.Size;

                    if (pfGuid == sourceGuid)
                    {
                        if (size.Width == metadata.Width && size.Height == metadata.Height)
                        {
                            // This frame does not need resized or format converted, just copy...
                            frame.CopyPixels(pixelBuffer.RowStride, pixelBuffer.BufferStride, pixelBuffer.DataPointer);
                        }
                        else
                        {
                            // This frame needs resizing, but not format converted
                            using (var scaler = Factory.CreateBitmapScaler())
                            {
                                scaler.Initialize(frame, metadata.Width, metadata.Height, GetWICInterp(flags));
                                scaler.CopyPixels(pixelBuffer.RowStride, pixelBuffer.BufferStride, pixelBuffer.DataPointer);
                            }
                        }
                    }
                    else
                    {
                        // This frame required format conversion
                        using (var converter = Factory.CreateFormatConverter())
                        {
                            converter.Initialize(frame, pfGuid, GetWICDither(flags), null, 0, BitmapPaletteType.Custom);

                            if (size.Width == metadata.Width && size.Height == metadata.Height)
                            {
                                converter.CopyPixels(pixelBuffer.RowStride, pixelBuffer.BufferStride, pixelBuffer.DataPointer);
                            }
                            else
                            {
                                // This frame needs resizing, but not format converted
                                using (var scaler = Factory.CreateBitmapScaler())
                                {
                                    scaler.Initialize(frame, metadata.Width, metadata.Height, GetWICInterp(flags));
                                    scaler.CopyPixels(pixelBuffer.RowStride, pixelBuffer.BufferStride, pixelBuffer.DataPointer);
                                }
                            }
                        }
                    }
                }
            }
            return image;
        }

        //-------------------------------------------------------------------------------------
        // Encodes a single frame
        //-------------------------------------------------------------------------------------
        private static unsafe void EncodeImage(PixelBuffer image, WICFlags flags, IWICBitmapFrameEncode frame)
        {
            Guid pfGuid;
            if (!ToWIC(image.Format, out pfGuid))
            {
                throw new NotSupportedException("Format not supported");
            }

            frame.Initialize();
            frame.SetSize(image.Width, image.Height);
            frame.SetResolution(72, 72);
            var targetGuid = pfGuid;
            frame.SetPixelFormat(ref targetGuid);

            if (targetGuid != pfGuid)
            {
                using (var source = Factory.CreateBitmapFromMemory(image.Width, image.Height, pfGuid, image.RowStride, image.BufferStride, image.DataPointer.ToPointer()))
                {
                    using (var converter = Factory.CreateFormatConverter())
                    {
                        using (var palette = Factory.CreatePalette())
                        {
                            palette.InitializeFromBitmap(source, 256, true);
                            converter.Initialize(source, targetGuid, GetWICDither(flags), palette, 0, BitmapPaletteType.Custom);

                            var bpp = GetBitsPerPixel(targetGuid);
                            if (bpp == 0)
                            {
                                throw new NotSupportedException("Unable to determine the Bpp for the target format");
                            }

                            var rowPitch = (image.Width * bpp + 7) / 8;
                            var slicePitch = rowPitch * image.Height;

                            var temp = SdxUtilities.AllocateMemory(slicePitch);
                            try
                            {
                                converter.CopyPixels(rowPitch, slicePitch, temp);
                                frame.SetPalette(palette);
                                frame.WritePixels(image.Height, temp, rowPitch, slicePitch);
                            }
                            finally
                            {
                                SdxUtilities.FreeMemory(temp);
                            }
                        }
                    }
                }
            }
            else
            {
                // No conversion required
                frame.WritePixels(image.Height, image.DataPointer, image.RowStride, image.BufferStride);
            }

            frame.Commit();
        }

        private static void EncodeSingleFrame(PixelBuffer pixelBuffer, WICFlags flags, Guid guidContainerFormat, Stream stream)
        {
            using (var encoder = Factory.CreateEncoder(guidContainerFormat, stream))
            using (var propBag = new PropertyBag())
            {
                if(guidContainerFormat == ContainerFormatGuids.Bmp)
                {
                    try
                    {
                        propBag.Set("EnableV5Header32bppBGRA", true);
                    }
                    catch
                    {

                    }
                    
                }

                using (var frame = encoder.CreateNewFrame(propBag))
                {
                    EncodeImage(pixelBuffer, flags, frame);
                    encoder.Commit();
                }
            }
        }

        //-------------------------------------------------------------------------------------
        // Encodes an image array
        //-------------------------------------------------------------------------------------
        private static void EncodeMultiframe(PixelBuffer[] images, int count, WICFlags flags, Guid guidContainerFormat, Stream stream)
        {
            if (images.Length < 2)
            {
                throw new ArgumentException("Cannot encode to multiple frame. Image doesn't have multiple frame");
            }

            using (var encoder = Factory.CreateEncoder(guidContainerFormat))
            using (var propBag = new PropertyBag())
            {
                using (var eInfo = encoder.EncoderInfo)
                {
                    if (!eInfo.DoesSupportMultiframe())
                    {
                        throw new NotSupportedException("Cannot encode to multiple frame. Format is not supporting multiple frame");
                    }
                }

                encoder.Initialize(stream);

                for (var i = 0; i < Math.Min(images.Length, count); i++)
                {
                    var pixelBuffer = images[i];
                    using (var frame = encoder.CreateNewFrame(propBag))
                    {
                        EncodeImage(pixelBuffer, flags, frame);
                    }
                }

                encoder.Commit();
            }
        }

        private static Guid GetContainerFormatFromFileType(ImageFileType fileType)
        {
            switch (fileType)
            {
                case ImageFileType.Bmp:
                    return ContainerFormatGuids.Bmp;
                case ImageFileType.Jpg:
                    return ContainerFormatGuids.Jpeg;
                case ImageFileType.Gif:
                    return ContainerFormatGuids.Gif;
                case ImageFileType.Png:
                    return ContainerFormatGuids.Png;
                case ImageFileType.Tiff:
                    return ContainerFormatGuids.Tiff;
                case ImageFileType.Wmp:
                    return ContainerFormatGuids.Wmp;
                default:
                    throw new NotSupportedException("Format not supported");
            }
        }

        private static void SaveToWICMemory(PixelBuffer[] pixelBuffer, int count, WICFlags flags, ImageFileType fileType, Stream stream)
        {
            if (count > 1)
            {
                EncodeMultiframe(pixelBuffer, count, flags, GetContainerFormatFromFileType(fileType), stream);
            }
            else
            {
                EncodeSingleFrame(pixelBuffer[0], flags, GetContainerFormatFromFileType(fileType), stream);
            }
        }

        //-------------------------------------------------------------------------------------
        // WIC Pixel Format Translation Data
        //-------------------------------------------------------------------------------------
        private struct WICTranslate
        {
            public WICTranslate(Guid wic, Format format)
            {
                WIC = wic;
                Format = format;
            }

            public readonly Guid WIC;
            public readonly Format Format;
        }

        //-------------------------------------------------------------------------------------
        // WIC Pixel Format nearest conversion table
        //-------------------------------------------------------------------------------------

        private struct WICConvert
        {
            public WICConvert(Guid source, Guid target)
            {
                this.source = source;
                this.target = target;
            }

            public readonly Guid source;
            public readonly Guid target;
        }
    }
}
