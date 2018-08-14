#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// This code is ported from SharpDX.Toolkit
// see: https://github.com/sharpdx/Toolkit

namespace SeeingSharp.Multimedia.Util.SdxTK
{
    /// <summary>
    /// Defines the dimension of a texture.
    /// </summary>
    public enum TextureDimension
    {
        /// <summary>
        /// The texture dimension is 1D.
        /// </summary>
        Texture1D,

        /// <summary>
        /// The texture dimension is 2D.
        /// </summary>
        Texture2D,

        /// <summary>
        /// The texture dimension is 3D.
        /// </summary>
        Texture3D,

        /// <summary>
        /// The texture dimension is a CubeMap.
        /// </summary>
        TextureCube,
    }

    public enum ImageFileType
    {
        /// <summary>
        /// A DDS file.
        /// </summary>
        Dds,

        /// <summary>
        /// A PNG file.
        /// </summary>
        Png,

        /// <summary>
        /// A GIF file.
        /// </summary>
        Gif,

        /// <summary>
        /// A JPG file.
        /// </summary>
        Jpg,

        /// <summary>
        /// A BMP file.
        /// </summary>
        Bmp,

        /// <summary>
        /// A TIFF file.
        /// </summary>
        Tiff,

        /// <summary>
        /// A WMP file.
        /// </summary>
        Wmp,

        /// <summary>
        /// A TGA File.
        /// </summary>
        Tga
    }

    [Flags]
    internal enum WICFlags
    {
        None = 0x0,

        ForceRgb = 0x1,
        // Loads DXGI 1.1 BGR formats as DXGI_FORMAT_R8G8B8A8_UNORM to avoid use of optional WDDM 1.1 formats

        NoX2Bias = 0x2,
        // Loads DXGI 1.1 X2 10:10:10:2 format as DXGI_FORMAT_R10G10B10A2_UNORM

        No16Bpp = 0x4,
        // Loads 565, 5551, and 4444 formats as 8888 to avoid use of optional WDDM 1.2 formats

        FlagsAllowMono = 0x8,
        // Loads 1-bit monochrome (black & white) as R1_UNORM rather than 8-bit greyscale

        AllFrames = 0x10,
        // Loads all images in a multi-frame file, converting/resizing to match the first frame as needed, defaults to 0th frame otherwise

        Dither = 0x10000,
        // Use ordered 4x4 dithering for any required conversions

        DitherDiffusion = 0x20000,
        // Use error-diffusion dithering for any required conversions

        FilterPoint = 0x100000,
        FilterLinear = 0x200000,
        FilterCubic = 0x300000,
        FilterFant = 0x400000, // Combination of Linear and Box filter
        // Filtering mode to use for any required image resizing (only needed when loading arrays of differently sized images; defaults to Fant)
    };

    /// <summary>
    /// Flags used by <see cref="DDSHelper.LoadFromDDSMemory"/>.
    /// </summary>
    [Flags]
    internal enum DDSFlags
    {
        None = 0x0,
        LegacyDword = 0x1, // Assume pitch is DWORD aligned instead of BYTE aligned (used by some legacy DDS files)
        NoLegacyExpansion = 0x2, // Do not implicitly convert legacy formats that result in larger pixel sizes (24 bpp, 3:3:2, A8L8, A4L4, P8, A8P8) 
        NoR10B10G10A2Fixup = 0x4, // Do not use work-around for long-standing D3DX DDS file format issue which reversed the 10:10:10:2 color order masks
        ForceRgb = 0x8, // Convert DXGI 1.1 BGR formats to Format.R8G8B8A8_UNorm to avoid use of optional WDDM 1.1 formats
        No16Bpp = 0x10, // Conversions avoid use of 565, 5551, and 4444 formats and instead expand to 8888 to avoid use of optional WDDM 1.2 formats
        CopyMemory = 0x20, // The content of the memory passed to the DDS Loader is copied to another internal buffer.
        ForceDX10Ext = 0x10000, // Always use the 'DX10' header extension for DDS writer (i.e. don't try to write DX9 compatible DDS files)
    };
}
