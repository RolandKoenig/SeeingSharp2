// This code is ported from SharpDX.Toolkit
// see: https://github.com/sharpdx/Toolkit

using System;
using System.Runtime.InteropServices;
using Vortice.DXGI;

#nullable disable

namespace SeeingSharp.Util.SdxTK
{
    /// <summary>
    /// PixelFormat is equivalent to <see cref="Vortice.DXGI.Format"/>.
    /// </summary>
    /// <remarks>
    /// This structure is implicitly castable to and from <see cref="Vortice.DXGI.Format"/>, you can use it inplace where <see cref="Vortice.DXGI.Format"/> is required
    /// and vice-versa.
    /// Usage is slightly different from <see cref="Vortice.DXGI.Format"/>, as you have to select the type of the pixel format first (Typeless, SInt...etc)
    /// and then access the available pixel formats for this type. Example: PixelFormat.UNorm.R8.
    /// </remarks>
    /// <msdn-id>bb173059</msdn-id>
    /// <unmanaged>DXGI_FORMAT</unmanaged>
    /// <unmanaged-short>DXGI_FORMAT</unmanaged-short>
    [StructLayout(LayoutKind.Sequential, Size = 4)]
    internal struct PixelFormat : IEquatable<PixelFormat>
    {
        /// <summary>
        /// Gets the value as a <see cref="Vortice.DXGI.Format"/> enum.
        /// </summary>
        public readonly Format Value;

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="format"></param>
        private PixelFormat(Format format)
        {
            Value = format;
        }

        public int SizeInBytes => FormatHelper.SizeOfInBytes(this);

        public static readonly PixelFormat Unknown = new PixelFormat(Format.Unknown);

        public static class A8
        {
            public static readonly PixelFormat UNorm = new PixelFormat(Format.A8_UNorm);
        }

        public static class B5G5R5A1
        {
            public static readonly PixelFormat UNorm = new PixelFormat(Format.B5G5R5A1_UNorm);
        }

        public static class B5G6R5
        {
            public static readonly PixelFormat UNorm = new PixelFormat(Format.B5G6R5_UNorm);
        }

        public static class B8G8R8A8
        {
            public static readonly PixelFormat Typeless = new PixelFormat(Format.B8G8R8A8_Typeless);
            public static readonly PixelFormat UNorm = new PixelFormat(Format.B8G8R8A8_UNorm);
            public static readonly PixelFormat UNormSRgb = new PixelFormat(Format.B8G8R8A8_UNorm_SRgb);
        }

        public static class B8G8R8X8
        {
            public static readonly PixelFormat Typeless = new PixelFormat(Format.B8G8R8X8_Typeless);
            public static readonly PixelFormat UNorm = new PixelFormat(Format.B8G8R8X8_UNorm);
            public static readonly PixelFormat UNormSRgb = new PixelFormat(Format.B8G8R8X8_UNorm_SRgb);
        }

        public static class BC1
        {
            public static readonly PixelFormat Typeless = new PixelFormat(Format.BC1_Typeless);
            public static readonly PixelFormat UNorm = new PixelFormat(Format.BC1_UNorm);
            public static readonly PixelFormat UNormSRgb = new PixelFormat(Format.BC1_UNorm_SRgb);
        }

        public static class BC2
        {
            public static readonly PixelFormat Typeless = new PixelFormat(Format.BC2_Typeless);
            public static readonly PixelFormat UNorm = new PixelFormat(Format.BC2_UNorm);
            public static readonly PixelFormat UNormSRgb = new PixelFormat(Format.BC2_UNorm_SRgb);
        }

        public static class BC3
        {
            public static readonly PixelFormat Typeless = new PixelFormat(Format.BC3_Typeless);
            public static readonly PixelFormat UNorm = new PixelFormat(Format.BC3_UNorm);
            public static readonly PixelFormat UNormSRgb = new PixelFormat(Format.BC3_UNorm_SRgb);
        }

        public static class BC4
        {
            public static readonly PixelFormat SNorm = new PixelFormat(Format.BC4_SNorm);
            public static readonly PixelFormat Typeless = new PixelFormat(Format.BC4_Typeless);
            public static readonly PixelFormat UNorm = new PixelFormat(Format.BC4_UNorm);
        }

        public static class BC5
        {
            public static readonly PixelFormat SNorm = new PixelFormat(Format.BC5_SNorm);
            public static readonly PixelFormat Typeless = new PixelFormat(Format.BC5_Typeless);
            public static readonly PixelFormat UNorm = new PixelFormat(Format.BC5_UNorm);
        }

        public static class BC6H
        {
            public static readonly PixelFormat Typeless = new PixelFormat(Format.BC6H_Typeless);
        }

        public static class BC7
        {
            public static readonly PixelFormat Typeless = new PixelFormat(Format.BC7_Typeless);
            public static readonly PixelFormat UNorm = new PixelFormat(Format.BC7_UNorm);
            public static readonly PixelFormat UNormSRgb = new PixelFormat(Format.BC7_UNorm_SRgb);
        }

        public static class R10G10B10A2
        {
            public static readonly PixelFormat Typeless = new PixelFormat(Format.R10G10B10A2_Typeless);
            public static readonly PixelFormat UInt = new PixelFormat(Format.R10G10B10A2_UInt);
            public static readonly PixelFormat UNorm = new PixelFormat(Format.R10G10B10A2_UNorm);
        }

        public static class R11G11B10
        {
            public static readonly PixelFormat Float = new PixelFormat(Format.R11G11B10_Float);
        }

        public static class R16
        {
            public static readonly PixelFormat Float = new PixelFormat(Format.R16_Float);
            public static readonly PixelFormat SInt = new PixelFormat(Format.R16_SInt);
            public static readonly PixelFormat SNorm = new PixelFormat(Format.R16_SNorm);
            public static readonly PixelFormat Typeless = new PixelFormat(Format.R16_Typeless);
            public static readonly PixelFormat UInt = new PixelFormat(Format.R16_UInt);
            public static readonly PixelFormat UNorm = new PixelFormat(Format.R16_UNorm);
        }

        public static class R16G16
        {
            public static readonly PixelFormat Float = new PixelFormat(Format.R16G16_Float);
            public static readonly PixelFormat SInt = new PixelFormat(Format.R16G16_SInt);
            public static readonly PixelFormat SNorm = new PixelFormat(Format.R16G16_SNorm);
            public static readonly PixelFormat Typeless = new PixelFormat(Format.R16G16_Typeless);
            public static readonly PixelFormat UInt = new PixelFormat(Format.R16G16_UInt);
            public static readonly PixelFormat UNorm = new PixelFormat(Format.R16G16_UNorm);
        }

        public static class R16G16B16A16
        {
            public static readonly PixelFormat Float = new PixelFormat(Format.R16G16B16A16_Float);

            public static readonly PixelFormat SInt = new PixelFormat(Format.R16G16B16A16_SInt);

            public static readonly PixelFormat SNorm = new PixelFormat(Format.R16G16B16A16_SNorm);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.R16G16B16A16_Typeless);

            public static readonly PixelFormat UInt = new PixelFormat(Format.R16G16B16A16_UInt);

            public static readonly PixelFormat UNorm = new PixelFormat(Format.R16G16B16A16_UNorm);
        }

        public static class R32
        {
            public static readonly PixelFormat Float = new PixelFormat(Format.R32_Float);

            public static readonly PixelFormat SInt = new PixelFormat(Format.R32_SInt);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.R32_Typeless);

            public static readonly PixelFormat UInt = new PixelFormat(Format.R32_UInt);
        }

        public static class R32G32
        {
            public static readonly PixelFormat Float = new PixelFormat(Format.R32G32_Float);

            public static readonly PixelFormat SInt = new PixelFormat(Format.R32G32_SInt);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.R32G32_Typeless);

            public static readonly PixelFormat UInt = new PixelFormat(Format.R32G32_UInt);
        }

        public static class R32G32B32
        {
            public static readonly PixelFormat Float = new PixelFormat(Format.R32G32B32_Float);

            public static readonly PixelFormat SInt = new PixelFormat(Format.R32G32B32_SInt);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.R32G32B32_Typeless);

            public static readonly PixelFormat UInt = new PixelFormat(Format.R32G32B32_UInt);
        }

        public static class R32G32B32A32
        {
            public static readonly PixelFormat Float = new PixelFormat(Format.R32G32B32A32_Float);

            public static readonly PixelFormat SInt = new PixelFormat(Format.R32G32B32A32_SInt);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.R32G32B32A32_Typeless);

            public static readonly PixelFormat UInt = new PixelFormat(Format.R32G32B32A32_UInt);
        }

        public static class R8
        {
            public static readonly PixelFormat SInt = new PixelFormat(Format.R8_SInt);

            public static readonly PixelFormat SNorm = new PixelFormat(Format.R8_SNorm);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.R8_Typeless);

            public static readonly PixelFormat UInt = new PixelFormat(Format.R8_UInt);

            public static readonly PixelFormat UNorm = new PixelFormat(Format.R8_UNorm);
        }

        public static class R8G8
        {
            public static readonly PixelFormat SInt = new PixelFormat(Format.R8G8_SInt);

            public static readonly PixelFormat SNorm = new PixelFormat(Format.R8G8_SNorm);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.R8G8_Typeless);

            public static readonly PixelFormat UInt = new PixelFormat(Format.R8G8_UInt);

            public static readonly PixelFormat UNorm = new PixelFormat(Format.R8G8_UNorm);
        }

        public static class R8G8B8A8
        {
            public static readonly PixelFormat SInt = new PixelFormat(Format.R8G8B8A8_SInt);

            public static readonly PixelFormat SNorm = new PixelFormat(Format.R8G8B8A8_SNorm);

            public static readonly PixelFormat Typeless = new PixelFormat(Format.R8G8B8A8_Typeless);

            public static readonly PixelFormat UInt = new PixelFormat(Format.R8G8B8A8_UInt);

            public static readonly PixelFormat UNorm = new PixelFormat(Format.R8G8B8A8_UNorm);

            public static readonly PixelFormat UNormSRgb = new PixelFormat(Format.R8G8B8A8_UNorm_SRgb);
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
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is PixelFormat && this.Equals((PixelFormat)obj);
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
            return $"{Value}";
        }
    }
}
