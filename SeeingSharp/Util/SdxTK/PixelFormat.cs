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
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

// This code is ported from SharpDX.Toolkit
// see: https://github.com/sharpdx/Toolkit

namespace SeeingSharp.Multimedia.Util.SdxTK
{
    /// <summary>
    /// PixelFormat is equivalent to <see cref="SharpDX.DXGI.Format"/>.
    /// </summary>
    /// <remarks>
    /// This structure is implicitly castable to and from <see cref="SharpDX.DXGI.Format"/>, you can use it inplace where <see cref="SharpDX.DXGI.Format"/> is required
    /// and vice-versa.
    /// Usage is slightly different from <see cref="SharpDX.DXGI.Format"/>, as you have to select the type of the pixel format first (Typeless, SInt...etc)
    /// and then access the available pixel formats for this type. Example: PixelFormat.UNorm.R8.
    /// </remarks>
    /// <msdn-id>bb173059</msdn-id>	
    /// <unmanaged>DXGI_FORMAT</unmanaged>	
    /// <unmanaged-short>DXGI_FORMAT</unmanaged-short>	
    [StructLayout(LayoutKind.Sequential, Size = 4)]
    public struct PixelFormat : IEquatable<PixelFormat>
    {
        /// <summary>
        /// Gets the value as a <see cref="SharpDX.DXGI.Format"/> enum.
        /// </summary>
        public readonly Format Value;

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="format"></param>
        private PixelFormat(Format format)
        {
            this.Value = format;
        }

        public int SizeInBytes { get { return (int)FormatHelper.SizeOfInBytes(this); } }

        public static readonly PixelFormat Unknown = new PixelFormat(Format.Unknown);

        public static class A8
        {
            #region Constants and Fields

            public static readonly PixelFormat UNorm = new PixelFormat(Format.A8_UNorm);

            #endregion
        }

        public static class B5G5R5A1
        {
            #region Constants and Fields

            public static readonly PixelFormat UNorm = new PixelFormat(Format.B5G5R5A1_UNorm);

            #endregion
        }

        public static class B5G6R5
        {
            #region Constants and Fields

            public static readonly PixelFormat UNorm = new PixelFormat(Format.B5G6R5_UNorm);

            #endregion
        }

        public static class B8G8R8A8
        {
            #region Constants and Fields

            public static readonly PixelFormat Typeless = new PixelFormat(Format.B8G8R8A8_Typeless);

            public static readonly PixelFormat UNorm = new PixelFormat(Format.B8G8R8A8_UNorm);

            public static readonly PixelFormat UNormSRgb = new PixelFormat(Format.B8G8R8A8_UNorm_SRgb);

            #endregion
        }

        public static class B8G8R8X8
        {
            #region Constants and Fields

            public static readonly PixelFormat Typeless = new PixelFormat(Format.B8G8R8X8_Typeless);

            public static readonly PixelFormat UNorm = new PixelFormat(Format.B8G8R8X8_UNorm);

            public static readonly PixelFormat UNormSRgb = new PixelFormat(Format.B8G8R8X8_UNorm_SRgb);

            #endregion
        }

        public static class BC1
        {
            #region Constants and Fields

            public static readonly PixelFormat Typeless = new PixelFormat(Format.BC1_Typeless);

            public static readonly PixelFormat UNorm = new PixelFormat(Format.BC1_UNorm);

            public static readonly PixelFormat UNormSRgb = new PixelFormat(Format.BC1_UNorm_SRgb);

            #endregion
        }

        public static class BC2
        {
            #region Constants and Fields

            public static readonly PixelFormat Typeless = new PixelFormat(Format.BC2_Typeless);

            public static readonly PixelFormat UNorm = new PixelFormat(Format.BC2_UNorm);

            public static readonly PixelFormat UNormSRgb = new PixelFormat(Format.BC2_UNorm_SRgb);

            #endregion
        }

        public static class BC3
        {
            #region Constants and Fields

            public static readonly PixelFormat Typeless = new PixelFormat(Format.BC3_Typeless);

            public static readonly PixelFormat UNorm = new PixelFormat(Format.BC3_UNorm);

            public static readonly PixelFormat UNormSRgb = new PixelFormat(Format.BC3_UNorm_SRgb);

            #endregion
        }

        public static class BC4
        {
            #region Constants and Fields

            public static readonly PixelFormat SNorm = new PixelFormat(Format.BC4_SNorm);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.BC4_Typeless);

            public static readonly PixelFormat UNorm = new PixelFormat(Format.BC4_UNorm);

            #endregion
        }

        public static class BC5
        {
            #region Constants and Fields

            public static readonly PixelFormat SNorm = new PixelFormat(Format.BC5_SNorm);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.BC5_Typeless);

            public static readonly PixelFormat UNorm = new PixelFormat(Format.BC5_UNorm);

            #endregion
        }

        public static class BC6H
        {
            #region Constants and Fields

            public static readonly PixelFormat Typeless = new PixelFormat(Format.BC6H_Typeless);

            #endregion
        }

        public static class BC7
        {
            #region Constants and Fields

            public static readonly PixelFormat Typeless = new PixelFormat(Format.BC7_Typeless);

            public static readonly PixelFormat UNorm = new PixelFormat(Format.BC7_UNorm);

            public static readonly PixelFormat UNormSRgb = new PixelFormat(Format.BC7_UNorm_SRgb);

            #endregion
        }

        public static class R10G10B10A2
        {
            #region Constants and Fields

            public static readonly PixelFormat Typeless = new PixelFormat(Format.R10G10B10A2_Typeless);

            public static readonly PixelFormat UInt = new PixelFormat(Format.R10G10B10A2_UInt);

            public static readonly PixelFormat UNorm = new PixelFormat(Format.R10G10B10A2_UNorm);

            #endregion
        }

        public static class R11G11B10
        {
            #region Constants and Fields

            public static readonly PixelFormat Float = new PixelFormat(Format.R11G11B10_Float);

            #endregion
        }

        public static class R16
        {
            #region Constants and Fields

            public static readonly PixelFormat Float = new PixelFormat(Format.R16_Float);

            public static readonly PixelFormat SInt = new PixelFormat(Format.R16_SInt);

            public static readonly PixelFormat SNorm = new PixelFormat(Format.R16_SNorm);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.R16_Typeless);

            public static readonly PixelFormat UInt = new PixelFormat(Format.R16_UInt);

            public static readonly PixelFormat UNorm = new PixelFormat(Format.R16_UNorm);

            #endregion
        }

        public static class R16G16
        {
            #region Constants and Fields

            public static readonly PixelFormat Float = new PixelFormat(Format.R16G16_Float);

            public static readonly PixelFormat SInt = new PixelFormat(Format.R16G16_SInt);

            public static readonly PixelFormat SNorm = new PixelFormat(Format.R16G16_SNorm);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.R16G16_Typeless);

            public static readonly PixelFormat UInt = new PixelFormat(Format.R16G16_UInt);

            public static readonly PixelFormat UNorm = new PixelFormat(Format.R16G16_UNorm);

            #endregion
        }

        public static class R16G16B16A16
        {
            #region Constants and Fields

            public static readonly PixelFormat Float = new PixelFormat(Format.R16G16B16A16_Float);

            public static readonly PixelFormat SInt = new PixelFormat(Format.R16G16B16A16_SInt);

            public static readonly PixelFormat SNorm = new PixelFormat(Format.R16G16B16A16_SNorm);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.R16G16B16A16_Typeless);

            public static readonly PixelFormat UInt = new PixelFormat(Format.R16G16B16A16_UInt);

            public static readonly PixelFormat UNorm = new PixelFormat(Format.R16G16B16A16_UNorm);

            #endregion
        }

        public static class R32
        {
            #region Constants and Fields

            public static readonly PixelFormat Float = new PixelFormat(Format.R32_Float);

            public static readonly PixelFormat SInt = new PixelFormat(Format.R32_SInt);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.R32_Typeless);

            public static readonly PixelFormat UInt = new PixelFormat(Format.R32_UInt);

            #endregion
        }

        public static class R32G32
        {
            #region Constants and Fields

            public static readonly PixelFormat Float = new PixelFormat(Format.R32G32_Float);

            public static readonly PixelFormat SInt = new PixelFormat(Format.R32G32_SInt);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.R32G32_Typeless);

            public static readonly PixelFormat UInt = new PixelFormat(Format.R32G32_UInt);

            #endregion
        }

        public static class R32G32B32
        {
            #region Constants and Fields

            public static readonly PixelFormat Float = new PixelFormat(Format.R32G32B32_Float);

            public static readonly PixelFormat SInt = new PixelFormat(Format.R32G32B32_SInt);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.R32G32B32_Typeless);

            public static readonly PixelFormat UInt = new PixelFormat(Format.R32G32B32_UInt);

            #endregion
        }

        public static class R32G32B32A32
        {
            #region Constants and Fields

            public static readonly PixelFormat Float = new PixelFormat(Format.R32G32B32A32_Float);

            public static readonly PixelFormat SInt = new PixelFormat(Format.R32G32B32A32_SInt);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.R32G32B32A32_Typeless);

            public static readonly PixelFormat UInt = new PixelFormat(Format.R32G32B32A32_UInt);

            #endregion
        }

        public static class R8
        {
            #region Constants and Fields

            public static readonly PixelFormat SInt = new PixelFormat(Format.R8_SInt);

            public static readonly PixelFormat SNorm = new PixelFormat(Format.R8_SNorm);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.R8_Typeless);

            public static readonly PixelFormat UInt = new PixelFormat(Format.R8_UInt);

            public static readonly PixelFormat UNorm = new PixelFormat(Format.R8_UNorm);

            #endregion
        }

        public static class R8G8
        {
            #region Constants and Fields

            public static readonly PixelFormat SInt = new PixelFormat(Format.R8G8_SInt);

            public static readonly PixelFormat SNorm = new PixelFormat(Format.R8G8_SNorm);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.R8G8_Typeless);

            public static readonly PixelFormat UInt = new PixelFormat(Format.R8G8_UInt);

            public static readonly PixelFormat UNorm = new PixelFormat(Format.R8G8_UNorm);

            #endregion
        }

        public static class R8G8B8A8
        {
            #region Constants and Fields

            public static readonly PixelFormat SInt = new PixelFormat(Format.R8G8B8A8_SInt);

            public static readonly PixelFormat SNorm = new PixelFormat(Format.R8G8B8A8_SNorm);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.R8G8B8A8_Typeless);

            public static readonly PixelFormat UInt = new PixelFormat(Format.R8G8B8A8_UInt);

            public static readonly PixelFormat UNorm = new PixelFormat(Format.R8G8B8A8_UNorm);

            public static readonly PixelFormat UNormSRgb = new PixelFormat(Format.R8G8B8A8_UNorm_SRgb);

            #endregion
        }

        public static implicit operator Format(PixelFormat from)
        {
            return from.Value;
        }

        public static implicit operator PixelFormat(Format from)
        {
            return new PixelFormat(from);
        }

        public bool Equals(PixelFormat other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is PixelFormat && Equals((PixelFormat)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(PixelFormat left, PixelFormat right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PixelFormat left, PixelFormat right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("{0}", Value);
        }
    }
}
