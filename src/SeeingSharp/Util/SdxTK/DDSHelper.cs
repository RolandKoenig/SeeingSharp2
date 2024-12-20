﻿// This code is ported from SharpDX.Toolkit
// see: https://github.com/sharpdx/Toolkit

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using SeeingSharp.Util.Sdx;
using Vortice.DXGI;
using D3D11 = Vortice.Direct3D11;

#nullable disable

namespace SeeingSharp.Util.SdxTK
{
    internal class DDSHelper
    {
        private static readonly LegacyMap[] LegacyMaps = 
        {
            new LegacyMap(Format.BC1_UNorm, ConversionFlags.None, DDS.PixelFormat.DXT1), // D3DFMT_DXT1
            new LegacyMap(Format.BC2_UNorm, ConversionFlags.None, DDS.PixelFormat.DXT3), // D3DFMT_DXT3
            new LegacyMap(Format.BC3_UNorm, ConversionFlags.None, DDS.PixelFormat.DXT5), // D3DFMT_DXT5

            new LegacyMap(Format.BC2_UNorm, ConversionFlags.None, DDS.PixelFormat.DXT2), // D3DFMT_DXT2 (ignore premultiply)
            new LegacyMap(Format.BC3_UNorm, ConversionFlags.None, DDS.PixelFormat.DXT4), // D3DFMT_DXT4 (ignore premultiply)

            new LegacyMap(Format.BC4_UNorm, ConversionFlags.None, DDS.PixelFormat.BC4_UNorm),
            new LegacyMap(Format.BC4_SNorm, ConversionFlags.None, DDS.PixelFormat.BC4_SNorm),
            new LegacyMap(Format.BC5_UNorm, ConversionFlags.None, DDS.PixelFormat.BC5_UNorm),
            new LegacyMap(Format.BC5_SNorm, ConversionFlags.None, DDS.PixelFormat.BC5_SNorm),

            new LegacyMap(Format.BC4_UNorm, ConversionFlags.None, new DDS.PixelFormat(DDS.PixelFormatFlags.FourCC, new FourCC('A', 'T', 'I', '1'), 0, 0, 0, 0, 0)),
            new LegacyMap(Format.BC5_UNorm, ConversionFlags.None, new DDS.PixelFormat(DDS.PixelFormatFlags.FourCC, new FourCC('A', 'T', 'I', '2'), 0, 0, 0, 0, 0)),

            new LegacyMap(Format.R8G8_B8G8_UNorm, ConversionFlags.None, DDS.PixelFormat.R8G8_B8G8), // D3DFMT_R8G8_B8G8
            new LegacyMap(Format.G8R8_G8B8_UNorm, ConversionFlags.None, DDS.PixelFormat.G8R8_G8B8), // D3DFMT_G8R8_G8B8

            new LegacyMap(Format.B8G8R8A8_UNorm, ConversionFlags.None, DDS.PixelFormat.A8R8G8B8), // D3DFMT_A8R8G8B8 (uses DXGI 1.1 format)
            new LegacyMap(Format.B8G8R8X8_UNorm, ConversionFlags.None, DDS.PixelFormat.X8R8G8B8), // D3DFMT_X8R8G8B8 (uses DXGI 1.1 format)
            new LegacyMap(Format.R8G8B8A8_UNorm, ConversionFlags.None, DDS.PixelFormat.A8B8G8R8), // D3DFMT_A8B8G8R8
            new LegacyMap(Format.R8G8B8A8_UNorm, ConversionFlags.NoAlpha, DDS.PixelFormat.X8B8G8R8), // D3DFMT_X8B8G8R8
            new LegacyMap(Format.R16G16_UNorm, ConversionFlags.None, DDS.PixelFormat.G16R16), // D3DFMT_G16R16

            new LegacyMap(Format.R10G10B10A2_UNorm, ConversionFlags.Swizzle, new DDS.PixelFormat(DDS.PixelFormatFlags.Rgb, 0, 32, 0x000003ff, 0x000ffc00, 0x3ff00000, 0xc0000000)),
            // D3DFMT_A2R10G10B10 (D3DX reversal issue workaround)
            new LegacyMap(Format.R10G10B10A2_UNorm, ConversionFlags.None, new DDS.PixelFormat(DDS.PixelFormatFlags.Rgb, 0, 32, 0x3ff00000, 0x000ffc00, 0x000003ff, 0xc0000000)),
            // D3DFMT_A2B10G10R10 (D3DX reversal issue workaround)

            new LegacyMap(Format.R8G8B8A8_UNorm, ConversionFlags.Expand
                                                 | ConversionFlags.NoAlpha
                                                 | ConversionFlags.Format888, DDS.PixelFormat.R8G8B8), // D3DFMT_R8G8B8

            new LegacyMap(Format.B5G6R5_UNorm, ConversionFlags.Format565, DDS.PixelFormat.R5G6B5), // D3DFMT_R5G6B5
            new LegacyMap(Format.B5G5R5A1_UNorm, ConversionFlags.Format5551, DDS.PixelFormat.A1R5G5B5), // D3DFMT_A1R5G5B5
            new LegacyMap(Format.B5G5R5A1_UNorm, ConversionFlags.Format5551
                                                 | ConversionFlags.NoAlpha, new DDS.PixelFormat(DDS.PixelFormatFlags.Rgb, 0, 16, 0x7c00, 0x03e0, 0x001f, 0x0000)), // D3DFMT_X1R5G5B5

            new LegacyMap(Format.R8G8B8A8_UNorm, ConversionFlags.Expand
                                                 | ConversionFlags.Format8332, new DDS.PixelFormat(DDS.PixelFormatFlags.Rgb, 0, 16, 0x00e0, 0x001c, 0x0003, 0xff00)),
            // D3DFMT_A8R3G3B2
            new LegacyMap(Format.B5G6R5_UNorm, ConversionFlags.Expand
                                               | ConversionFlags.Format332, new DDS.PixelFormat(DDS.PixelFormatFlags.Rgb, 0, 8, 0xe0, 0x1c, 0x03, 0x00)), // D3DFMT_R3G3B2

            new LegacyMap(Format.R8_UNorm, ConversionFlags.None, DDS.PixelFormat.L8), // D3DFMT_L8
            new LegacyMap(Format.R16_UNorm, ConversionFlags.None, DDS.PixelFormat.L16), // D3DFMT_L16
            new LegacyMap(Format.R8G8_UNorm, ConversionFlags.None, DDS.PixelFormat.A8L8), // D3DFMT_A8L8

            new LegacyMap(Format.A8_UNorm, ConversionFlags.None, DDS.PixelFormat.A8), // D3DFMT_A8

            new LegacyMap(Format.R16G16B16A16_UNorm, ConversionFlags.None, new DDS.PixelFormat(DDS.PixelFormatFlags.FourCC, 36, 0, 0, 0, 0, 0)), // D3DFMT_A16B16G16R16
            new LegacyMap(Format.R16G16B16A16_SNorm, ConversionFlags.None, new DDS.PixelFormat(DDS.PixelFormatFlags.FourCC, 110, 0, 0, 0, 0, 0)), // D3DFMT_Q16W16V16U16
            new LegacyMap(Format.R16_Float, ConversionFlags.None, new DDS.PixelFormat(DDS.PixelFormatFlags.FourCC, 111, 0, 0, 0, 0, 0)), // D3DFMT_R16F
            new LegacyMap(Format.R16G16_Float, ConversionFlags.None, new DDS.PixelFormat(DDS.PixelFormatFlags.FourCC, 112, 0, 0, 0, 0, 0)), // D3DFMT_G16R16F
            new LegacyMap(Format.R16G16B16A16_Float, ConversionFlags.None, new DDS.PixelFormat(DDS.PixelFormatFlags.FourCC, 113, 0, 0, 0, 0, 0)), // D3DFMT_A16B16G16R16F
            new LegacyMap(Format.R32_Float, ConversionFlags.None, new DDS.PixelFormat(DDS.PixelFormatFlags.FourCC, 114, 0, 0, 0, 0, 0)), // D3DFMT_R32F
            new LegacyMap(Format.R32G32_Float, ConversionFlags.None, new DDS.PixelFormat(DDS.PixelFormatFlags.FourCC, 115, 0, 0, 0, 0, 0)), // D3DFMT_G32R32F
            new LegacyMap(Format.R32G32B32A32_Float, ConversionFlags.None, new DDS.PixelFormat(DDS.PixelFormatFlags.FourCC, 116, 0, 0, 0, 0, 0)), // D3DFMT_A32B32G32R32F

            new LegacyMap(Format.R32_Float, ConversionFlags.None, new DDS.PixelFormat(DDS.PixelFormatFlags.Rgb, 0, 32, 0xffffffff, 0x00000000, 0x00000000, 0x00000000)),
            // D3DFMT_R32F (D3DX uses FourCC 114 instead)

            new LegacyMap(Format.R8G8B8A8_UNorm, ConversionFlags.Expand
                                                 | ConversionFlags.Pal8
                                                 | ConversionFlags.FormatA8P8, new DDS.PixelFormat(DDS.PixelFormatFlags.Pal8, 0, 16, 0, 0, 0, 0)), // D3DFMT_A8P8
            new LegacyMap(Format.R8G8B8A8_UNorm, ConversionFlags.Expand
                                                 | ConversionFlags.Pal8, new DDS.PixelFormat(DDS.PixelFormatFlags.Pal8, 0, 8, 0, 0, 0, 0)), // D3DFMT_P8

            // !DXGI_1_2_FORMATS
            new LegacyMap(Format.R8G8B8A8_UNorm, ConversionFlags.Expand
                                                 | ConversionFlags.Format4444, DDS.PixelFormat.A4R4G4B4), // D3DFMT_A4R4G4B4
            new LegacyMap(Format.R8G8B8A8_UNorm, ConversionFlags.Expand
                                                 | ConversionFlags.NoAlpha
                                                 | ConversionFlags.Format4444, new DDS.PixelFormat(DDS.PixelFormatFlags.Rgb, 0, 16, 0x0f00, 0x00f0, 0x000f, 0x0000)),
            // D3DFMT_X4R4G4B4
            new LegacyMap(Format.R8G8B8A8_UNorm, ConversionFlags.Expand
                                                 | ConversionFlags.Format44, new DDS.PixelFormat(DDS.PixelFormatFlags.Luminance, 0, 8, 0x0f, 0x00, 0x00, 0xf0)) // D3DFMT_A4L4
        };

        /// <summary>
        /// Load a DDS file in memory
        /// </summary>
        /// <param name="pSource">Source buffer</param>
        /// <param name="size">Size of the DDS texture.</param>
        /// <param name="makeACopy">Whether or not to make a copy of the DDS</param>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static unsafe Image LoadFromDDSMemory(IntPtr pSource, int size, bool makeACopy, GCHandle? handle)
        {
            var flags = makeACopy ? DDSFlags.CopyMemory : DDSFlags.None;

            ConversionFlags convFlags;
            ImageDescription mdata;
            // If the memory pointed is not a DDS memory, return null.
            if (!DecodeDDSHeader(pSource, size, flags, out mdata, out convFlags))
            {
                return null;
            }

            var offset = sizeof(uint) + SdxUtilities.SizeOf<DDS.Header>();
            if ((convFlags & ConversionFlags.DX10) != 0)
            {
                offset += SdxUtilities.SizeOf<DDS.HeaderDXT10>();
            }

            var pal8 = (int*)0;
            if ((convFlags & ConversionFlags.Pal8) != 0)
            {
                pal8 = (int*)((byte*)pSource + offset);
                offset += 256 * sizeof(uint);
            }

            if (size < offset)
            {
                throw new InvalidOperationException();
            }

            var image = CreateImageFromDDS(pSource, offset, size - offset, mdata, (flags & DDSFlags.LegacyDword) != 0 ? Image.PitchFlags.LegacyDword : Image.PitchFlags.None, convFlags, pal8, handle);
            return image;
        }

        public static void SaveToDDSStream(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            SaveToDDSStream(pixelBuffers, count, description, DDSFlags.None, imageStream);
        }

        //-------------------------------------------------------------------------------------
        // Save a DDS to a stream
        //-------------------------------------------------------------------------------------
        public static unsafe void SaveToDDSStream(PixelBuffer[] pixelBuffers, int count, ImageDescription metadata, DDSFlags flags, Stream stream)
        {
            // Determine memory required
            var totalSize = 0;
            var headerSize = 0;
            EncodeDDSHeader(metadata, flags, IntPtr.Zero, 0, out totalSize);
            headerSize = totalSize;

            var maxSlice = 0;

            for (var i = 0; i < pixelBuffers.Length; ++i)
            {
                var slice = pixelBuffers[i].BufferStride;
                totalSize += slice;
                if (slice > maxSlice)
                {
                    maxSlice = slice;
                }
            }

            Debug.Assert(totalSize > 0);

            // Allocate a single temporary buffer to save the headers and each slice.
            var buffer = new byte[Math.Max(maxSlice, headerSize)];

            fixed (void* pbuffer = buffer)
            {
                int required;
                EncodeDDSHeader(metadata, flags, (IntPtr)pbuffer, headerSize, out required);
                stream.Write(buffer, 0, headerSize);
            }

            var remaining = totalSize - headerSize;
            Debug.Assert(remaining > 0);

            var index = 0;
            for (var item = 0; item < metadata.ArraySize; ++item)
            {
                var d = metadata.Depth;

                for (var level = 0; level < metadata.MipLevels; ++level)
                {
                    for (var slice = 0; slice < d; ++slice)
                    {
                        var pixsize = pixelBuffers[index].BufferStride;
                        SdxUtilities.Read(pixelBuffers[index].DataPointer, buffer, 0, pixsize);
                        stream.Write(buffer, 0, pixsize);
                        ++index;
                    }

                    if (d > 1)
                    {
                        d >>= 1;
                    }
                }
            }
        }

        /// <summary>
        /// Copies an image row with optional clearing of alpha value to 1.0.
        /// </summary>
        /// <remarks>
        /// This method can be used in place as well, otherwise copies the image row unmodified.
        /// </remarks>
        /// <param name="pDestination">The destination buffer.</param>
        /// <param name="outSize">The destination size.</param>
        /// <param name="pSource">The source buffer.</param>
        /// <param name="inSize">The source size.</param>
        /// <param name="format">The <see cref="Vortice.DXGI.Format"/> of the source scanline.</param>
        /// <param name="flags">Scanline flags used when copying the scanline.</param>
        internal static unsafe void CopyScanline(IntPtr pDestination, int outSize, IntPtr pSource, int inSize, Format format, ScanlineFlags flags)
        {
            if ((flags & ScanlineFlags.SetAlpha) != 0)
            {
                switch (format)
                {
                    //-----------------------------------------------------------------------------
                    case Format.R32G32B32A32_Typeless:
                    case Format.R32G32B32A32_Float:
                    case Format.R32G32B32A32_UInt:
                    case Format.R32G32B32A32_SInt:
                        {
                            uint alpha;
                            if (format == Format.R32G32B32A32_Float)
                            {
                                alpha = 0x3f800000;
                            }
                            else if (format == Format.R32G32B32A32_SInt)
                            {
                                alpha = 0x7fffffff;
                            }
                            else
                            {
                                alpha = 0xffffffff;
                            }

                            if (pDestination == pSource)
                            {
                                var dPtr = (uint*)pDestination;
                                for (var count = 0; count < outSize; count += 16)
                                {
                                    dPtr += 3;
                                    *dPtr++ = alpha;
                                }
                            }
                            else
                            {
                                var sPtr = (uint*)pSource;
                                var dPtr = (uint*)pDestination;
                                var size = Math.Min(outSize, inSize);
                                for (var count = 0; count < size; count += 16)
                                {
                                    *dPtr++ = *sPtr++;
                                    *dPtr++ = *sPtr++;
                                    *dPtr++ = *sPtr++;
                                    *dPtr++ = alpha;
                                    sPtr++;
                                }
                            }
                        }
                        return;

                    //-----------------------------------------------------------------------------
                    case Format.R16G16B16A16_Typeless:
                    case Format.R16G16B16A16_Float:
                    case Format.R16G16B16A16_UNorm:
                    case Format.R16G16B16A16_UInt:
                    case Format.R16G16B16A16_SNorm:
                    case Format.R16G16B16A16_SInt:
                        {
                            ushort alpha;
                            if (format == Format.R16G16B16A16_Float)
                            {
                                alpha = 0x3c00;
                            }
                            else if (format == Format.R16G16B16A16_SNorm || format == Format.R16G16B16A16_SInt)
                            {
                                alpha = 0x7fff;
                            }
                            else
                            {
                                alpha = 0xffff;
                            }

                            if (pDestination == pSource)
                            {
                                var dPtr = (ushort*)pDestination;
                                for (var count = 0; count < outSize; count += 8)
                                {
                                    dPtr += 3;
                                    *dPtr++ = alpha;
                                }
                            }
                            else
                            {
                                var sPtr = (ushort*)pSource;
                                var dPtr = (ushort*)pDestination;
                                var size = Math.Min(outSize, inSize);
                                for (var count = 0; count < size; count += 8)
                                {
                                    *dPtr++ = *sPtr++;
                                    *dPtr++ = *sPtr++;
                                    *dPtr++ = *sPtr++;
                                    *dPtr++ = alpha;
                                    sPtr++;
                                }
                            }
                        }
                        return;

                    //-----------------------------------------------------------------------------
                    case Format.R10G10B10A2_Typeless:
                    case Format.R10G10B10A2_UNorm:
                    case Format.R10G10B10A2_UInt:
                    case Format.R10G10B10_Xr_Bias_A2_UNorm:
                        {
                            if (pDestination == pSource)
                            {
                                var dPtr = (uint*)pDestination;
                                for (var count = 0; count < outSize; count += 4)
                                {
                                    *dPtr |= 0xC0000000;
                                    ++dPtr;
                                }
                            }
                            else
                            {
                                var sPtr = (uint*)pSource;
                                var dPtr = (uint*)pDestination;
                                var size = Math.Min(outSize, inSize);
                                for (var count = 0; count < size; count += 4)
                                {
                                    *dPtr++ = *sPtr++ | 0xC0000000;
                                }
                            }
                        }
                        return;

                    //-----------------------------------------------------------------------------
                    case Format.R8G8B8A8_Typeless:
                    case Format.R8G8B8A8_UNorm:
                    case Format.R8G8B8A8_UNorm_SRgb:
                    case Format.R8G8B8A8_UInt:
                    case Format.R8G8B8A8_SNorm:
                    case Format.R8G8B8A8_SInt:
                    case Format.B8G8R8A8_UNorm:
                    case Format.B8G8R8A8_Typeless:
                    case Format.B8G8R8A8_UNorm_SRgb:
                        {
                            var alpha = format == Format.R8G8B8A8_SNorm || format == Format.R8G8B8A8_SInt ? 0x7f000000 : 0xff000000;

                            if (pDestination == pSource)
                            {
                                var dPtr = (uint*)pDestination;
                                for (var count = 0; count < outSize; count += 4)
                                {
                                    var t = *dPtr & 0xFFFFFF;
                                    t |= alpha;
                                    *dPtr++ = t;
                                }
                            }
                            else
                            {
                                var sPtr = (uint*)pSource;
                                var dPtr = (uint*)pDestination;
                                var size = Math.Min(outSize, inSize);
                                for (var count = 0; count < size; count += 4)
                                {
                                    var t = *sPtr++ & 0xFFFFFF;
                                    t |= alpha;
                                    *dPtr++ = t;
                                }
                            }
                        }
                        return;

                    //-----------------------------------------------------------------------------
                    case Format.B5G5R5A1_UNorm:
                        {
                            if (pDestination == pSource)
                            {
                                var dPtr = (ushort*)pDestination;
                                for (var count = 0; count < outSize; count += 2)
                                {
                                    *dPtr++ |= 0x8000;
                                }
                            }
                            else
                            {
                                var sPtr = (ushort*)pSource;
                                var dPtr = (ushort*)pDestination;
                                var size = Math.Min(outSize, inSize);
                                for (var count = 0; count < size; count += 2)
                                {
                                    *dPtr++ = (ushort)(*sPtr++ | 0x8000);
                                }
                            }
                        }
                        return;

                    //-----------------------------------------------------------------------------
                    case Format.A8_UNorm:
                        SdxUtilities.ClearMemory(pDestination, 0xff, outSize);
                        return;

                        // DXGI_1_2_FORMATS
                }
            }

            // Fall-through case is to just use memcpy (assuming this is not an in-place operation)
            if (pDestination == pSource)
            {
                return;
            }

            SdxUtilities.CopyMemory(pDestination, pSource, Math.Min(outSize, inSize));
        }

        /// <summary>
        /// Swizzles (RGB &lt;-&gt; BGR) an image row with optional clearing of alpha value to 1.0.
        /// </summary>
        /// <param name="pDestination">The destination buffer.</param>
        /// <param name="outSize">The destination size.</param>
        /// <param name="pSource">The source buffer.</param>
        /// <param name="inSize">The source size.</param>
        /// <param name="format">The <see cref="Vortice.DXGI.Format"/> of the source scanline.</param>
        /// <param name="flags">Scanline flags used when copying the scanline.</param>
        /// <remarks>
        /// This method can be used in place as well, otherwise copies the image row unmodified.
        /// </remarks>
        internal static unsafe void SwizzleScanline(IntPtr pDestination, int outSize, IntPtr pSource, int inSize, Format format, ScanlineFlags flags)
        {
            switch (format)
            {
                //---------------------------------------------------------------------------------
                case Format.R10G10B10A2_Typeless:
                case Format.R10G10B10A2_UNorm:
                case Format.R10G10B10A2_UInt:
                case Format.R10G10B10_Xr_Bias_A2_UNorm:
                    if ((flags & ScanlineFlags.Legacy) != 0)
                    {
                        // Swap Red (R) and Blue (B) channel (used for D3DFMT_A2R10G10B10 legacy sources)
                        if (pDestination == pSource)
                        {
                            var dPtr = (uint*)pDestination;
                            for (var count = 0; count < outSize; count += 4)
                            {
                                var t = *dPtr;

                                var t1 = (t & 0x3ff00000) >> 20;
                                var t2 = (t & 0x000003ff) << 20;
                                var t3 = t & 0x000ffc00;
                                var ta = (flags & ScanlineFlags.SetAlpha) != 0 ? 0xC0000000 : t & 0xC0000000;

                                *dPtr++ = t1 | t2 | t3 | ta;
                            }
                        }
                        else
                        {
                            var sPtr = (uint*)pSource;
                            var dPtr = (uint*)pDestination;
                            var size = Math.Min(outSize, inSize);
                            for (var count = 0; count < size; count += 4)
                            {
                                var t = *sPtr++;

                                var t1 = (t & 0x3ff00000) >> 20;
                                var t2 = (t & 0x000003ff) << 20;
                                var t3 = t & 0x000ffc00;
                                var ta = (flags & ScanlineFlags.SetAlpha) != 0 ? 0xC0000000 : t & 0xC0000000;

                                *dPtr++ = t1 | t2 | t3 | ta;
                            }
                        }
                        return;
                    }
                    break;

                //---------------------------------------------------------------------------------
                case Format.R8G8B8A8_Typeless:
                case Format.R8G8B8A8_UNorm:
                case Format.R8G8B8A8_UNorm_SRgb:
                case Format.B8G8R8A8_UNorm:
                case Format.B8G8R8X8_UNorm:
                case Format.B8G8R8A8_Typeless:
                case Format.B8G8R8A8_UNorm_SRgb:
                case Format.B8G8R8X8_Typeless:
                case Format.B8G8R8X8_UNorm_SRgb:
                    // Swap Red (R) and Blue (B) channels (used to convert from DXGI 1.1 BGR formats to DXGI 1.0 RGB)
                    if (pDestination == pSource)
                    {
                        var dPtr = (uint*)pDestination;
                        for (var count = 0; count < outSize; count += 4)
                        {
                            var t = *dPtr;

                            var t1 = (t & 0x00ff0000) >> 16;
                            var t2 = (t & 0x000000ff) << 16;
                            var t3 = t & 0x0000ff00;
                            var ta = (flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : t & 0xFF000000;

                            *dPtr++ = t1 | t2 | t3 | ta;
                        }
                    }
                    else
                    {
                        var sPtr = (uint*)pSource;
                        var dPtr = (uint*)pDestination;
                        var size = Math.Min(outSize, inSize);
                        for (var count = 0; count < size; count += 4)
                        {
                            var t = *sPtr++;

                            var t1 = (t & 0x00ff0000) >> 16;
                            var t2 = (t & 0x000000ff) << 16;
                            var t3 = t & 0x0000ff00;
                            var ta = (flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : t & 0xFF000000;

                            *dPtr++ = t1 | t2 | t3 | ta;
                        }
                    }
                    return;
            }

            // Fall-through case is to just use memcpy (assuming this is not an in-place operation)
            if (pDestination == pSource)
            {
                return;
            }

            SdxUtilities.CopyMemory(pDestination, pSource, Math.Min(outSize, inSize));
        }

        // Note that many common DDS reader/writers (including D3DX) swap the
        // the RED/BLUE masks for 10:10:10:2 formats. We assume
        // below that the 'backwards' header mask is being used since it is most
        // likely written by D3DX. The more robust solution is to use the 'DX10'
        // header extension and specify the Format.R10G10B10A2_UNorm format directly

        // We do not support the following legacy Direct3D 9 formats:
        //      BumpDuDv D3DFMT_V8U8, D3DFMT_Q8W8V8U8, D3DFMT_V16U16, D3DFMT_A2W10V10U10
        //      BumpLuminance D3DFMT_L6V5U5, D3DFMT_X8L8V8U8
        //      FourCC "UYVY" D3DFMT_UYVY
        //      FourCC "YUY2" D3DFMT_YUY2
        //      FourCC 117 D3DFMT_CxV8U8
        //      ZBuffer D3DFMT_D16_LOCKABLE
        //      FourCC 82 D3DFMT_D32F_LOCKABLE
        private static Format GetDXGIFormat(ref DDS.PixelFormat pixelFormat, DDSFlags flags, out ConversionFlags conversionFlags)
        {
            conversionFlags = ConversionFlags.None;

            var index = 0;
            for (index = 0; index < LegacyMaps.Length; ++index)
            {
                var entry = LegacyMaps[index];

                if ((pixelFormat.Flags & entry.PixelFormat.Flags) != 0)
                {
                    if ((entry.PixelFormat.Flags & DDS.PixelFormatFlags.FourCC) != 0)
                    {
                        if (pixelFormat.FourCC == entry.PixelFormat.FourCC)
                        {
                            break;
                        }
                    }
                    else if ((entry.PixelFormat.Flags & DDS.PixelFormatFlags.Pal8) != 0)
                    {
                        if (pixelFormat.RGBBitCount == entry.PixelFormat.RGBBitCount)
                        {
                            break;
                        }
                    }
                    else if (pixelFormat.RGBBitCount == entry.PixelFormat.RGBBitCount)
                    {
                        // RGB, RGBA, ALPHA, LUMINANCE
                        if (pixelFormat.RBitMask == entry.PixelFormat.RBitMask
                            && pixelFormat.GBitMask == entry.PixelFormat.GBitMask
                            && pixelFormat.BBitMask == entry.PixelFormat.BBitMask
                            && pixelFormat.ABitMask == entry.PixelFormat.ABitMask)
                        {
                            break;
                        }
                    }
                }
            }

            if (index >= LegacyMaps.Length)
            {
                return Format.Unknown;
            }

            conversionFlags = LegacyMaps[index].ConversionFlags;
            var format = LegacyMaps[index].Format;

            if ((conversionFlags & ConversionFlags.Expand) != 0 && (flags & DDSFlags.NoLegacyExpansion) != 0)
            {
                return Format.Unknown;
            }

            if (format == Format.R10G10B10A2_UNorm && (flags & DDSFlags.NoR10B10G10A2Fixup) != 0)
            {
                conversionFlags ^= ConversionFlags.Swizzle;
            }

            return format;
        }

        /// <summary>
        /// Decodes DDS header including optional DX10 extended header
        /// </summary>
        /// <param name="headerPtr">Pointer to the DDS header.</param>
        /// <param name="size">Size of the DDS content.</param>
        /// <param name="flags">Flags used for decoding the DDS header.</param>
        /// <param name="description">Output texture description.</param>
        /// <param name="convFlags">Output conversion flags.</param>
        /// <exception cref="ArgumentException">If the argument headerPtr is null</exception>
        /// <exception cref="InvalidOperationException">If the DDS header contains invalid data.</exception>
        /// <returns>True if the decoding is successful, false if this is not a DDS header.</returns>
        private static unsafe bool DecodeDDSHeader(IntPtr headerPtr, int size, DDSFlags flags, out ImageDescription description, out ConversionFlags convFlags)
        {
            description = new ImageDescription();
            convFlags = ConversionFlags.None;

            if (headerPtr == IntPtr.Zero)
            {
                throw new ArgumentException("Pointer to DDS header cannot be null", "headerPtr");
            }

            if (size < SdxUtilities.SizeOf<DDS.Header>() + sizeof(uint))
            {
                return false;
            }

            // DDS files always start with the same magic number ("DDS ")
            if (*(uint*)headerPtr != DDS.MagicHeader)
            {
                return false;
            }

            var header = *(DDS.Header*)((byte*)headerPtr + sizeof(int));

            // Verify header to validate DDS file
            if (header.Size != SdxUtilities.SizeOf<DDS.Header>() || header.PixelFormat.Size != SdxUtilities.SizeOf<DDS.PixelFormat>())
            {
                return false;
            }

            // Setup MipLevels
            description.MipLevels = header.MipMapCount;
            if (description.MipLevels == 0)
            {
                description.MipLevels = 1;
            }

            // Check for DX10 extension
            if ((header.PixelFormat.Flags & DDS.PixelFormatFlags.FourCC) != 0 && new FourCC('D', 'X', '1', '0') == header.PixelFormat.FourCC)
            {
                // Buffer must be big enough for both headers and magic value
                if (size < SdxUtilities.SizeOf<DDS.Header>() + sizeof(uint) + SdxUtilities.SizeOf<DDS.HeaderDXT10>())
                {
                    return false;
                }

                var headerDX10 = *(DDS.HeaderDXT10*)((byte*)headerPtr + sizeof(int) + SdxUtilities.SizeOf<DDS.Header>());
                convFlags |= ConversionFlags.DX10;

                description.ArraySize = headerDX10.ArraySize;
                if (description.ArraySize == 0)
                {
                    throw new InvalidOperationException("Unexpected ArraySize == 0 from DDS HeaderDX10 ");
                }

                description.Format = headerDX10.DXGIFormat;
                if (!FormatHelper.IsValid(description.Format))
                {
                    throw new InvalidOperationException("Invalid Format from DDS HeaderDX10 ");
                }

                switch (headerDX10.ResourceDimension)
                {
                    case D3D11.ResourceDimension.Texture1D:

                        // D3DX writes 1D textures with a fixed Height of 1
                        if ((header.Flags & DDS.HeaderFlags.Height) != 0 && header.Height != 1)
                        {
                            throw new InvalidOperationException("Unexpected Height != 1 from DDS HeaderDX10 ");
                        }

                        description.Width = header.Width;
                        description.Height = 1;
                        description.Depth = 1;
                        description.Dimension = TextureDimension.Texture1D;
                        break;

                    case D3D11.ResourceDimension.Texture2D:
                        if ((headerDX10.MiscFlags & D3D11.ResourceOptionFlags.TextureCube) != 0)
                        {
                            description.ArraySize *= 6;
                            description.Dimension = TextureDimension.TextureCube;
                        }
                        else
                        {
                            description.Dimension = TextureDimension.Texture2D;
                        }

                        description.Width = header.Width;
                        description.Height = header.Height;
                        description.Depth = 1;
                        break;

                    case D3D11.ResourceDimension.Texture3D:
                        if ((header.Flags & DDS.HeaderFlags.Volume) == 0)
                        {
                            throw new InvalidOperationException("Texture3D missing HeaderFlags.Volume from DDS HeaderDX10");
                        }

                        if (description.ArraySize > 1)
                        {
                            throw new InvalidOperationException("Unexpected ArraySize > 1 for Texture3D from DDS HeaderDX10");
                        }

                        description.Width = header.Width;
                        description.Height = header.Height;
                        description.Depth = header.Depth;
                        description.Dimension = TextureDimension.Texture3D;
                        break;

                    default:
                        throw new InvalidOperationException(
                            $"Unexpected dimension [{headerDX10.ResourceDimension}] from DDS HeaderDX10");
                }
            }
            else
            {
                description.ArraySize = 1;

                if ((header.Flags & DDS.HeaderFlags.Volume) != 0)
                {
                    description.Width = header.Width;
                    description.Height = header.Height;
                    description.Depth = header.Depth;
                    description.Dimension = TextureDimension.Texture3D;
                }
                else
                {
                    if ((header.CubemapFlags & DDS.CubemapFlags.CubeMap) != 0)
                    {
                        // We require all six faces to be defined
                        if ((header.CubemapFlags & DDS.CubemapFlags.AllFaces) != DDS.CubemapFlags.AllFaces)
                        {
                            throw new InvalidOperationException("Unexpected CubeMap, expecting all faces from DDS Header");
                        }

                        description.ArraySize = 6;
                        description.Dimension = TextureDimension.TextureCube;
                    }
                    else
                    {
                        description.Dimension = TextureDimension.Texture2D;
                    }

                    description.Width = header.Width;
                    description.Height = header.Height;
                    description.Depth = 1;
                    // Note there's no way for a legacy Direct3D 9 DDS to express a '1D' texture
                }

                description.Format = GetDXGIFormat(ref header.PixelFormat, flags, out convFlags);

                if (description.Format == Format.Unknown)
                {
                    throw new InvalidOperationException("Unsupported PixelFormat from DDS Header");
                }
            }

            // Special flag for handling BGR DXGI 1.1 formats
            if ((flags & DDSFlags.ForceRgb) != 0)
            {
                switch (description.Format)
                {
                    case Format.B8G8R8A8_UNorm:
                        description.Format = Format.R8G8B8A8_UNorm;
                        convFlags |= ConversionFlags.Swizzle;
                        break;

                    case Format.B8G8R8X8_UNorm:
                        description.Format = Format.R8G8B8A8_UNorm;
                        convFlags |= ConversionFlags.Swizzle | ConversionFlags.NoAlpha;
                        break;

                    case Format.B8G8R8A8_Typeless:
                        description.Format = Format.R8G8B8A8_Typeless;
                        convFlags |= ConversionFlags.Swizzle;
                        break;

                    case Format.B8G8R8A8_UNorm_SRgb:
                        description.Format = Format.R8G8B8A8_UNorm_SRgb;
                        convFlags |= ConversionFlags.Swizzle;
                        break;

                    case Format.B8G8R8X8_Typeless:
                        description.Format = Format.R8G8B8A8_Typeless;
                        convFlags |= ConversionFlags.Swizzle | ConversionFlags.NoAlpha;
                        break;

                    case Format.B8G8R8X8_UNorm_SRgb:
                        description.Format = Format.R8G8B8A8_UNorm_SRgb;
                        convFlags |= ConversionFlags.Swizzle | ConversionFlags.NoAlpha;
                        break;
                }
            }

            // Pass DDSFlags copy memory to the conversion flags
            if ((flags & DDSFlags.CopyMemory) != 0)
            {
                convFlags |= ConversionFlags.CopyMemory;
            }

            // Special flag for handling 16bpp formats
            if ((flags & DDSFlags.No16Bpp) != 0)
            {
                switch (description.Format)
                {
                    case Format.B5G6R5_UNorm:
                    case Format.B5G5R5A1_UNorm:
                        description.Format = Format.R8G8B8A8_UNorm;
                        convFlags |= ConversionFlags.Expand;
                        if (description.Format == Format.B5G6R5_UNorm)
                        {
                            convFlags |= ConversionFlags.NoAlpha;
                        }
                        break;
                }
            }
            return true;
        }

        /// <summary>
        /// Encodes DDS file header (magic value, header, optional DX10 extended header)
        /// </summary>
        /// <param name="flags">Flags used for decoding the DDS header.</param>
        /// <param name="description">Output texture description.</param>
        /// <param name="pDestination">Pointer to the DDS output header. Can be set to IntPtr.Zero to calculated the required bytes.</param>
        /// <param name="maxsize">The maximum size of the destination buffer.</param>
        /// <param name="required">Output the number of bytes required to write the DDS header.</param>
        /// <exception cref="ArgumentException">If the argument headerPtr is null</exception>
        /// <exception cref="InvalidOperationException">If the DDS header contains invalid data.</exception>
        /// <returns>True if the decoding is successful, false if this is not a DDS header.</returns>
        private static unsafe void EncodeDDSHeader(ImageDescription description, DDSFlags flags, IntPtr pDestination, int maxsize, out int required)
        {
            if (description.ArraySize > 1)
            {
                if (description.ArraySize != 6 || description.Dimension != TextureDimension.Texture2D || description.Dimension != TextureDimension.TextureCube)
                {
                    flags |= DDSFlags.ForceDX10Ext;
                }
            }

            var ddpf = default(DDS.PixelFormat);
            if ((flags & DDSFlags.ForceDX10Ext) == 0)
            {
                switch (description.Format)
                {
                    case Format.R8G8B8A8_UNorm:
                        ddpf = DDS.PixelFormat.A8B8G8R8;
                        break;
                    case Format.R16G16_UNorm:
                        ddpf = DDS.PixelFormat.G16R16;
                        break;
                    case Format.R8G8_UNorm:
                        ddpf = DDS.PixelFormat.A8L8;
                        break;
                    case Format.R16_UNorm:
                        ddpf = DDS.PixelFormat.L16;
                        break;
                    case Format.R8_UNorm:
                        ddpf = DDS.PixelFormat.L8;
                        break;
                    case Format.A8_UNorm:
                        ddpf = DDS.PixelFormat.A8;
                        break;
                    case Format.R8G8_B8G8_UNorm:
                        ddpf = DDS.PixelFormat.R8G8_B8G8;
                        break;
                    case Format.G8R8_G8B8_UNorm:
                        ddpf = DDS.PixelFormat.G8R8_G8B8;
                        break;
                    case Format.BC1_UNorm:
                        ddpf = DDS.PixelFormat.DXT1;
                        break;
                    case Format.BC2_UNorm:
                        ddpf = DDS.PixelFormat.DXT3;
                        break;
                    case Format.BC3_UNorm:
                        ddpf = DDS.PixelFormat.DXT5;
                        break;
                    case Format.BC4_UNorm:
                        ddpf = DDS.PixelFormat.BC4_UNorm;
                        break;
                    case Format.BC4_SNorm:
                        ddpf = DDS.PixelFormat.BC4_SNorm;
                        break;
                    case Format.BC5_UNorm:
                        ddpf = DDS.PixelFormat.BC5_UNorm;
                        break;
                    case Format.BC5_SNorm:
                        ddpf = DDS.PixelFormat.BC5_SNorm;
                        break;
                    case Format.B5G6R5_UNorm:
                        ddpf = DDS.PixelFormat.R5G6B5;
                        break;
                    case Format.B5G5R5A1_UNorm:
                        ddpf = DDS.PixelFormat.A1R5G5B5;
                        break;
                    case Format.B8G8R8A8_UNorm:
                        ddpf = DDS.PixelFormat.A8R8G8B8;
                        break; // DXGI 1.1
                    case Format.B8G8R8X8_UNorm:
                        ddpf = DDS.PixelFormat.X8R8G8B8;
                        break; // DXGI 1.1

                    // Legacy D3DX formats using D3DFMT enum value as FourCC
                    case Format.R32G32B32A32_Float:
                        ddpf.Size = SdxUtilities.SizeOf<DDS.PixelFormat>();
                        ddpf.Flags = DDS.PixelFormatFlags.FourCC;
                        ddpf.FourCC = 116; // D3DFMT_A32B32G32R32F
                        break;
                    case Format.R16G16B16A16_Float:
                        ddpf.Size = SdxUtilities.SizeOf<DDS.PixelFormat>();
                        ddpf.Flags = DDS.PixelFormatFlags.FourCC;
                        ddpf.FourCC = 113; // D3DFMT_A16B16G16R16F
                        break;
                    case Format.R16G16B16A16_UNorm:
                        ddpf.Size = SdxUtilities.SizeOf<DDS.PixelFormat>();
                        ddpf.Flags = DDS.PixelFormatFlags.FourCC;
                        ddpf.FourCC = 36; // D3DFMT_A16B16G16R16
                        break;
                    case Format.R16G16B16A16_SNorm:
                        ddpf.Size = SdxUtilities.SizeOf<DDS.PixelFormat>();
                        ddpf.Flags = DDS.PixelFormatFlags.FourCC;
                        ddpf.FourCC = 110; // D3DFMT_Q16W16V16U16
                        break;
                    case Format.R32G32_Float:
                        ddpf.Size = SdxUtilities.SizeOf<DDS.PixelFormat>();
                        ddpf.Flags = DDS.PixelFormatFlags.FourCC;
                        ddpf.FourCC = 115; // D3DFMT_G32R32F
                        break;
                    case Format.R16G16_Float:
                        ddpf.Size = SdxUtilities.SizeOf<DDS.PixelFormat>();
                        ddpf.Flags = DDS.PixelFormatFlags.FourCC;
                        ddpf.FourCC = 112; // D3DFMT_G16R16F
                        break;
                    case Format.R32_Float:
                        ddpf.Size = SdxUtilities.SizeOf<DDS.PixelFormat>();
                        ddpf.Flags = DDS.PixelFormatFlags.FourCC;
                        ddpf.FourCC = 114; // D3DFMT_R32F
                        break;
                    case Format.R16_Float:
                        ddpf.Size = SdxUtilities.SizeOf<DDS.PixelFormat>();
                        ddpf.Flags = DDS.PixelFormatFlags.FourCC;
                        ddpf.FourCC = 111; // D3DFMT_R16F
                        break;
                }
            }

            required = sizeof(int) + SdxUtilities.SizeOf<DDS.Header>();

            if (ddpf.Size == 0)
            {
                required += SdxUtilities.SizeOf<DDS.HeaderDXT10>();
            }

            if (pDestination == IntPtr.Zero)
            {
                return;
            }

            if (maxsize < required)
            {
                throw new ArgumentException("Not enough size for destination buffer", "maxsize");
            }

            *(uint*)pDestination = DDS.MagicHeader;

            var header = (DDS.Header*)((byte*)pDestination + sizeof(int));

            SdxUtilities.ClearMemory((IntPtr)header, 0, SdxUtilities.SizeOf<DDS.Header>());
            header->Size = SdxUtilities.SizeOf<DDS.Header>();
            header->Flags = DDS.HeaderFlags.Texture;
            header->SurfaceFlags = DDS.SurfaceFlags.Texture;

            if (description.MipLevels > 0)
            {
                header->Flags |= DDS.HeaderFlags.Mipmap;
                header->MipMapCount = description.MipLevels;

                if (header->MipMapCount > 1)
                {
                    header->SurfaceFlags |= DDS.SurfaceFlags.Mipmap;
                }
            }

            switch (description.Dimension)
            {
                case TextureDimension.Texture1D:
                    header->Height = description.Height;
                    header->Width = header->Depth = 1;
                    break;

                case TextureDimension.Texture2D:
                case TextureDimension.TextureCube:
                    header->Height = description.Height;
                    header->Width = description.Width;
                    header->Depth = 1;

                    if (description.Dimension == TextureDimension.TextureCube)
                    {
                        header->SurfaceFlags |= DDS.SurfaceFlags.Cubemap;
                        header->CubemapFlags |= DDS.CubemapFlags.AllFaces;
                    }
                    break;

                case TextureDimension.Texture3D:

                    header->Flags |= DDS.HeaderFlags.Volume;
                    header->CubemapFlags |= DDS.CubemapFlags.Volume;
                    header->Height = description.Height;
                    header->Width = description.Width;
                    header->Depth = description.Depth;
                    break;
            }

            int rowPitch, slicePitch;
            int newWidth;
            int newHeight;
            Image.ComputePitch(description.Format, description.Width, description.Height, out rowPitch, out slicePitch, out newWidth, out newHeight);

            if (FormatHelper.IsCompressed(description.Format))
            {
                header->Flags |= DDS.HeaderFlags.LinearSize;
                header->PitchOrLinearSize = slicePitch;
            }
            else
            {
                header->Flags |= DDS.HeaderFlags.Pitch;
                header->PitchOrLinearSize = rowPitch;
            }

            if (ddpf.Size == 0)
            {
                header->PixelFormat = DDS.PixelFormat.DX10;

                var ext = (DDS.HeaderDXT10*)((byte*)header + SdxUtilities.SizeOf<DDS.Header>());

                SdxUtilities.ClearMemory((IntPtr)ext, 0, SdxUtilities.SizeOf<DDS.HeaderDXT10>());

                ext->DXGIFormat = description.Format;
                switch (description.Dimension)
                {
                    case TextureDimension.Texture1D:
                        ext->ResourceDimension = D3D11.ResourceDimension.Texture1D;
                        break;
                    case TextureDimension.Texture2D:
                    case TextureDimension.TextureCube:
                        ext->ResourceDimension = D3D11.ResourceDimension.Texture2D;
                        break;
                    case TextureDimension.Texture3D:
                        ext->ResourceDimension = D3D11.ResourceDimension.Texture3D;
                        break;

                }

                if (description.Dimension == TextureDimension.TextureCube)
                {
                    ext->MiscFlags |= D3D11.ResourceOptionFlags.TextureCube;
                    ext->ArraySize = description.ArraySize / 6;
                }
                else
                {
                    ext->ArraySize = description.ArraySize;
                }
            }
            else
            {
                header->PixelFormat = ddpf;
            }
        }

        private static TEXP_LEGACY_FORMAT FindLegacyFormat(ConversionFlags flags)
        {
            var lformat = TEXP_LEGACY_FORMAT.UNKNOWN;

            if ((flags & ConversionFlags.Pal8) != 0)
            {
                lformat = (flags & ConversionFlags.FormatA8P8) != 0 ? TEXP_LEGACY_FORMAT.A8P8 : TEXP_LEGACY_FORMAT.P8;
            }
            else if ((flags & ConversionFlags.Format888) != 0)
            {
                lformat = TEXP_LEGACY_FORMAT.R8G8B8;
            }
            else if ((flags & ConversionFlags.Format332) != 0)
            {
                lformat = TEXP_LEGACY_FORMAT.R3G3B2;
            }
            else if ((flags & ConversionFlags.Format8332) != 0)
            {
                lformat = TEXP_LEGACY_FORMAT.A8R3G3B2;
            }
            else if ((flags & ConversionFlags.Format44) != 0)
            {
                lformat = TEXP_LEGACY_FORMAT.A4L4;
            }
            else if ((flags & ConversionFlags.Format4444) != 0)
            {
                lformat = TEXP_LEGACY_FORMAT.B4G4R4A4;
            }
            return lformat;
        }

        /// <summary>
        /// Converts an image row with optional clearing of alpha value to 1.0
        /// </summary>
        /// <param name="pDestination"></param>
        /// <param name="outSize"></param>
        /// <param name="outFormat"></param>
        /// <param name="pSource"></param>
        /// <param name="inSize"></param>
        /// <param name="inFormat"></param>
        /// <param name="pal8"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private static unsafe bool LegacyExpandScanline(IntPtr pDestination, int outSize, Format outFormat,
                                            IntPtr pSource, int inSize, TEXP_LEGACY_FORMAT inFormat,
                                            int* pal8, ScanlineFlags flags)
        {
            switch (inFormat)
            {
                case TEXP_LEGACY_FORMAT.R8G8B8:
                    if (outFormat != Format.R8G8B8A8_UNorm)
                    {
                        return false;
                    }

                    // D3DFMT_R8G8B8 -> Format.R8G8B8A8_UNorm
                    {
                        var sPtr = (byte*)pSource;
                        var dPtr = (int*)pDestination;

                        for (int ocount = 0, icount = 0; icount < inSize && ocount < outSize; icount += 3, ocount += 4)
                        {
                            // 24bpp Direct3D 9 files are actually BGR, so need to swizzle as well
                            var t1 = *sPtr << 16;
                            var t2 = *(sPtr + 1) << 8;
                            int t3 = *(sPtr + 2);

                            *dPtr++ = (int)(t1 | t2 | t3 | 0xff000000);
                            sPtr += 3;
                        }
                    }
                    return true;

                case TEXP_LEGACY_FORMAT.R3G3B2:
                    switch (outFormat)
                    {
                        case Format.R8G8B8A8_UNorm:
                            // D3DFMT_R3G3B2 -> Format.R8G8B8A8_UNorm
                            {
                                var sPtr = (byte*)pSource;
                                var dPtr = (int*)pDestination;

                                for (int ocount = 0, icount = 0; icount < inSize && ocount < outSize; ++icount, ocount += 4)
                                {
                                    var t = *sPtr++;

                                    var t1 = (t & 0xe0) | ((t & 0xe0) >> 3) | ((t & 0xc0) >> 6);
                                    var t2 = ((t & 0x1c) << 11) | ((t & 0x1c) << 8) | ((t & 0x18) << 5);
                                    var t3 = ((t & 0x03) << 22) | ((t & 0x03) << 20) | ((t & 0x03) << 18) | ((t & 0x03) << 16);

                                    *dPtr++ = (int)(t1 | t2 | t3 | 0xff000000);
                                }
                            }
                            return true;

                        case Format.B5G6R5_UNorm:
                            // D3DFMT_R3G3B2 -> Format.B5G6R5_UNorm
                            {
                                var sPtr = (byte*)pSource;
                                var dPtr = (short*)pDestination;

                                for (int ocount = 0, icount = 0; icount < inSize && ocount < outSize; ++icount, ocount += 2)
                                {
                                    var t = *sPtr++;

                                    var t1 = (short)(((t & 0xe0) << 8) | ((t & 0xc0) << 5));
                                    var t2 = (short)(((t & 0x1c) << 6) | ((t & 0x1c) << 3));
                                    var t3 = (short)(((t & 0x03) << 3) | ((t & 0x03) << 1) | ((t & 0x02) >> 1));

#pragma warning disable
                                    *dPtr++ = (short)(t1 | t2 | t3);
#pragma warning restore
                                }
                            }
                            return true;
                    }
                    break;

                case TEXP_LEGACY_FORMAT.A8R3G3B2:
                    if (outFormat != Format.R8G8B8A8_UNorm)
                    {
                        return false;
                    }

                    // D3DFMT_A8R3G3B2 -> Format.R8G8B8A8_UNorm
                    {
                        var sPtr = (short*)pSource;
                        var dPtr = (int*)pDestination;

                        for (int ocount = 0, icount = 0; icount < inSize && ocount < outSize; icount += 2, ocount += 4)
                        {
                            var t = *sPtr++;

                            var t1 = (t & 0x00e0) | ((t & 0x00e0) >> 3) | ((t & 0x00c0) >> 6);
                            var t2 = ((t & 0x001c) << 11) | ((t & 0x001c) << 8) | ((t & 0x0018) << 5);
                            var t3 = ((t & 0x0003) << 22) | ((t & 0x0003) << 20) | ((t & 0x0003) << 18) | ((t & 0x0003) << 16);
                            var ta = (flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : (uint)((t & 0xff00) << 16);

#pragma warning disable
                            *dPtr++ = (int)(t1 | t2 | t3 | ta);
#pragma warning restore
                        }
                    }
                    return true;

                case TEXP_LEGACY_FORMAT.P8:
                    if (outFormat != Format.R8G8B8A8_UNorm || pal8 == null)
                    {
                        return false;
                    }

                    // D3DFMT_P8 -> Format.R8G8B8A8_UNorm
                    {
                        var sPtr = (byte*)pSource;
                        var dPtr = (int*)pDestination;

                        for (int ocount = 0, icount = 0; icount < inSize && ocount < outSize; ++icount, ocount += 4)
                        {
                            var t = *sPtr++;

                            *dPtr++ = pal8[t];
                        }
                    }
                    return true;

                case TEXP_LEGACY_FORMAT.A8P8:
                    if (outFormat != Format.R8G8B8A8_UNorm || pal8 == null)
                    {
                        return false;
                    }

                    // D3DFMT_A8P8 -> Format.R8G8B8A8_UNorm
                    {
                        var sPtr = (short*)pSource;
                        var dPtr = (int*)pDestination;

                        for (int ocount = 0, icount = 0; icount < inSize && ocount < outSize; icount += 2, ocount += 4)
                        {
                            var t = *sPtr++;

                            var t1 = pal8[t & 0xff];
                            var ta = (flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : (uint)((t & 0xff00) << 16);

#pragma warning disable
                            *dPtr++ = (int)(t1 | ta);
#pragma warning restore
                        }
                    }
                    return true;

                case TEXP_LEGACY_FORMAT.A4L4:
                    switch (outFormat)
                    {
                        case Format.B4G4R4A4_UNorm:
                            // D3DFMT_A4L4 -> Format.B4G4R4A4_UNorm
                            {
                                var sPtr = (byte*)pSource;
                                var dPtr = (short*)pDestination;

                                for (int ocount = 0, icount = 0; icount < inSize && ocount < outSize; ++icount, ocount += 2)
                                {
                                    var t = *sPtr++;

                                    var t1 = (short)(t & 0x0f);
                                    var ta = (flags & ScanlineFlags.SetAlpha) != 0 ? (ushort)0xf000 : (ushort)((t & 0xf0) << 8);

#pragma warning disable
                                    *dPtr++ = (short)(t1 | (t1 << 4) | (t1 << 8) | ta);
#pragma warning restore
                                }
                            }
                            return true;
                        // DXGI_1_2_FORMATS

                        case Format.R8G8B8A8_UNorm:
                            // D3DFMT_A4L4 -> Format.R8G8B8A8_UNorm
                            {
                                var sPtr = (byte*)pSource;
                                var dPtr = (int*)pDestination;

                                for (int ocount = 0, icount = 0; icount < inSize && ocount < outSize; ++icount, ocount += 4)
                                {
                                    var t = *sPtr++;

                                    var t1 = ((t & 0x0f) << 4) | (t & 0x0f);
                                    var ta = (flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : (uint)(((t & 0xf0) << 24) | ((t & 0xf0) << 20));

#pragma warning disable
                                    *dPtr++ = (int)(t1 | (t1 << 8) | (t1 << 16) | ta);
#pragma warning restore
                                }
                            }
                            return true;
                    }
                    break;

                case TEXP_LEGACY_FORMAT.B4G4R4A4:
                    if (outFormat != Format.R8G8B8A8_UNorm)
                    {
                        return false;
                    }

                    // D3DFMT_A4R4G4B4 -> Format.R8G8B8A8_UNorm
                    {
                        var sPtr = (short*)pSource;
                        var dPtr = (int*)pDestination;

                        for (int ocount = 0, icount = 0; icount < inSize && ocount < outSize; icount += 2, ocount += 4)
                        {
                            var t = *sPtr++;

                            var t1 = ((t & 0x0f00) >> 4) | ((t & 0x0f00) >> 8);
                            var t2 = ((t & 0x00f0) << 8) | ((t & 0x00f0) << 4);
                            var t3 = ((t & 0x000f) << 20) | ((t & 0x000f) << 16);
                            var ta = (flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : (uint)(((t & 0xf000) << 16) | ((t & 0xf000) << 12));

#pragma warning disable
                            *dPtr++ = (int)(t1 | t2 | t3 | ta);
#pragma warning restore
                        }
                    }
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Converts or copies image data from pPixels into scratch image data
        /// </summary>
        /// <param name="pDDS"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="metadata"></param>
        /// <param name="cpFlags"></param>
        /// <param name="convFlags"></param>
        /// <param name="pal8"></param>
        /// <param name="handle"></param>
        /// <returns></returns>
        private static unsafe Image CreateImageFromDDS(IntPtr pDDS, int offset, int size, ImageDescription metadata, Image.PitchFlags cpFlags, ConversionFlags convFlags, int* pal8, GCHandle? handle)
        {
            if ((convFlags & ConversionFlags.Expand) != 0)
            {
                if ((convFlags & ConversionFlags.Format888) != 0)
                {
                    cpFlags |= Image.PitchFlags.Bpp24;
                }
                else if ((convFlags & (ConversionFlags.Format565 | ConversionFlags.Format5551 | ConversionFlags.Format4444 | ConversionFlags.Format8332 | ConversionFlags.FormatA8P8)) != 0)
                {
                    cpFlags |= Image.PitchFlags.Bpp16;
                }
                else if ((convFlags & (ConversionFlags.Format44 | ConversionFlags.Format332 | ConversionFlags.Pal8)) != 0)
                {
                    cpFlags |= Image.PitchFlags.Bpp8;
                }
            }

            // If source image == dest image and no swizzle/alpha is required, we can return it as-is
            var isCopyNeeded = (convFlags & (ConversionFlags.Expand | ConversionFlags.CopyMemory)) != 0 || (cpFlags & Image.PitchFlags.LegacyDword) != 0;

            var image = new Image(metadata, pDDS, offset, handle, !isCopyNeeded, cpFlags);

            // Size must be inferior to destination size.
            //Debug.Assert(size <= image.TotalSizeInBytes);

            if (!isCopyNeeded && (convFlags & (ConversionFlags.Swizzle | ConversionFlags.NoAlpha)) == 0)
            {
                return image;
            }

            var imageDst = isCopyNeeded ? new Image(metadata, IntPtr.Zero, 0, null, false) : image;

            var images = image.PixelBuffer;
            var imagesDst = imageDst.PixelBuffer;

            var tflags = (convFlags & ConversionFlags.NoAlpha) != 0 ? ScanlineFlags.SetAlpha : ScanlineFlags.None;
            if ((convFlags & ConversionFlags.Swizzle) != 0)
            {
                tflags |= ScanlineFlags.Legacy;
            }

            var index = 0;

            var checkSize = size;

            for (var arrayIndex = 0; arrayIndex < metadata.ArraySize; arrayIndex++)
            {
                var d = metadata.Depth;
                // Else we need to go through each mips/depth slice to convert all scanlines.
                for (var level = 0; level < metadata.MipLevels; ++level)
                {
                    for (var slice = 0; slice < d; ++slice, ++index)
                    {
                        var pSrc = images[index].DataPointer;
                        var pDest = imagesDst[index].DataPointer;
                        checkSize -= images[index].BufferStride;
                        if (checkSize < 0)
                        {
                            throw new InvalidOperationException("Unexpected end of buffer");
                        }

                        if (FormatHelper.IsCompressed(metadata.Format))
                        {
                            SdxUtilities.CopyMemory(pDest, pSrc, Math.Min(images[index].BufferStride, imagesDst[index].BufferStride));
                        }
                        else
                        {
                            var spitch = images[index].RowStride;
                            var dpitch = imagesDst[index].RowStride;

                            for (var h = 0; h < images[index].Height; ++h)
                            {
                                if ((convFlags & ConversionFlags.Expand) != 0)
                                {
                                    if ((convFlags & (ConversionFlags.Format565 | ConversionFlags.Format5551)) != 0)
                                    {
                                        ExpandScanline(pDest, dpitch, pSrc, spitch, (convFlags & ConversionFlags.Format565) != 0 ? Format.B5G6R5_UNorm : Format.B5G5R5A1_UNorm, tflags);
                                    }
                                    else
                                    {
                                        var lformat = FindLegacyFormat(convFlags);
                                        LegacyExpandScanline(pDest, dpitch, metadata.Format, pSrc, spitch, lformat, pal8, tflags);
                                    }
                                }
                                else if ((convFlags & ConversionFlags.Swizzle) != 0)
                                {
                                    SwizzleScanline(pDest, dpitch, pSrc, spitch, metadata.Format, tflags);
                                }
                                else
                                {
                                    if (pSrc != pDest)
                                    {
                                        CopyScanline(pDest, dpitch, pSrc, spitch, metadata.Format, tflags);
                                    }
                                }

                                pSrc = (IntPtr)((byte*)pSrc + spitch);
                                pDest = (IntPtr)((byte*)pDest + dpitch);
                            }
                        }
                    }

                    if (d > 1)
                    {
                        d >>= 1;
                    }
                }
            }

            // Return the imageDst or the original image
            if (isCopyNeeded)
            {
                image.Dispose();
                image = imageDst;
            }
            return image;
        }

        /// <summary>
        /// Converts an image row with optional clearing of alpha value to 1.0
        /// </summary>
        /// <param name="pDestination"></param>
        /// <param name="outSize"></param>
        /// <param name="pSource"></param>
        /// <param name="inSize"></param>
        /// <param name="inFormat"></param>
        /// <param name="flags"></param>
        private static unsafe void ExpandScanline(IntPtr pDestination, int outSize, IntPtr pSource, int inSize, Format inFormat, ScanlineFlags flags)
        {
            switch (inFormat)
            {
                case Format.B5G6R5_UNorm:
                    // DXGI.Format.B5G6R5_UNorm -> DXGI.Format.R8G8B8A8_UNorm
                    {
                        var sPtr = (ushort*)pSource;
                        var dPtr = (uint*)pDestination;

                        for (uint ocount = 0, icount = 0; icount < inSize && ocount < outSize; icount += 2, ocount += 4)
                        {
                            var t = *sPtr++;

                            var t1 = (uint)(((t & 0xf800) >> 8) | ((t & 0xe000) >> 13));
                            var t2 = (uint)(((t & 0x07e0) << 5) | ((t & 0x0600) >> 5));
                            var t3 = (uint)(((t & 0x001f) << 19) | ((t & 0x001c) << 14));

                            *dPtr++ = t1 | t2 | t3 | 0xff000000;
                        }
                    }
                    break;

                case Format.B5G5R5A1_UNorm:
                    // DXGI.Format.B5G5R5A1_UNorm -> DXGI.Format.R8G8B8A8_UNorm
                    {
                        var sPtr = (ushort*)pSource;
                        var dPtr = (uint*)pDestination;

                        for (uint ocount = 0, icount = 0; icount < inSize && ocount < outSize; icount += 2, ocount += 4)
                        {
                            var t = *sPtr++;

                            var t1 = (uint)(((t & 0x7c00) >> 7) | ((t & 0x7000) >> 12));
                            var t2 = (uint)(((t & 0x03e0) << 6) | ((t & 0x0380) << 1));
                            var t3 = (uint)(((t & 0x001f) << 19) | ((t & 0x001c) << 14));
                            var ta = (flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : (t & 0x8000) != 0 ? 0xff000000 : 0;

                            *dPtr++ = t1 | t2 | t3 | ta;
                        }
                    }
                    break;

                    // DXGI_1_2_FORMATS
            }
        }

        [Flags]
        public enum ConversionFlags
        {
            None = 0x0,
            Expand = 0x1, // Conversion requires expanded pixel size
            NoAlpha = 0x2, // Conversion requires setting alpha to known value
            Swizzle = 0x4, // BGR/RGB order swizzling required
            Pal8 = 0x8, // Has an 8-bit palette
            Format888 = 0x10, // Source is an 8:8:8 (24bpp) format
            Format565 = 0x20, // Source is a 5:6:5 (16bpp) format
            Format5551 = 0x40, // Source is a 5:5:5:1 (16bpp) format
            Format4444 = 0x80, // Source is a 4:4:4:4 (16bpp) format
            Format44 = 0x100, // Source is a 4:4 (8bpp) format
            Format332 = 0x200, // Source is a 3:3:2 (8bpp) format
            Format8332 = 0x400, // Source is a 8:3:3:2 (16bpp) format
            FormatA8P8 = 0x800, // Has an 8-bit palette with an alpha channel
            CopyMemory = 0x1000, // The content of the memory passed to the DDS Loader is copied to another internal buffer.
            DX10 = 0x10000 // Has the 'DX10' extension header
        }

        [Flags]
        internal enum ScanlineFlags
        {
            None = 0,
            SetAlpha = 0x1, // Set alpha channel to known opaque value
            Legacy = 0x2 // Enables specific legacy format conversion cases
        }

        private enum TEXP_LEGACY_FORMAT
        {
            UNKNOWN = 0,
            R8G8B8,
            R3G3B2,
            A8R3G3B2,
            P8,
            A8P8,
            A4L4,
            B4G4R4A4
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct LegacyMap
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LegacyMap" /> struct.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="conversionFlags">The conversion flags.</param>
            /// <param name="pixelFormat">The pixel format.</param>
            public LegacyMap(Format format, ConversionFlags conversionFlags, DDS.PixelFormat pixelFormat)
            {
                Format = format;
                ConversionFlags = conversionFlags;
                PixelFormat = pixelFormat;
            }

            public Format Format;
            public ConversionFlags ConversionFlags;
            public DDS.PixelFormat PixelFormat;
        }
    }
}
