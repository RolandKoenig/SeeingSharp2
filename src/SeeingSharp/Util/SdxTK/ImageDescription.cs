// This code is ported from SharpDX.Toolkit
// see: https://github.com/sharpdx/Toolkit

using System;
using System.Runtime.InteropServices;
using Vortice.DXGI;

namespace SeeingSharp.Util.SdxTK
{
    /// <summary>
    /// A description for <see cref="Image"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ImageDescription : IEquatable<ImageDescription>
    {
        /// <summary>
        /// The dimension of a texture.
        /// </summary>
        public TextureDimension Dimension;
        public int Width;
        public int Height;
        public int Depth;
        public int ArraySize;
        public int MipLevels;

        /// <summary>
        /// <dd> <p>Texture format (see <strong><see cref="Vortice.DXGI.Format"/></strong>).</p> </dd>
        /// </summary>
        /// <msdn-id>ff476252</msdn-id>
        /// <unmanaged>DXGI_FORMAT Format</unmanaged>
        /// <unmanaged-short>DXGI_FORMAT Format</unmanaged-short>
        public Format Format;

        public bool Equals(ImageDescription other)
        {
            return Dimension.Equals(other.Dimension) && Width == other.Width && Height == other.Height && Depth == other.Depth && ArraySize == other.ArraySize && MipLevels == other.MipLevels && Format.Equals(other.Format);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is ImageDescription && this.Equals((ImageDescription)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Dimension.GetHashCode();
                hashCode = (hashCode * 397) ^ Width;
                hashCode = (hashCode * 397) ^ Height;
                hashCode = (hashCode * 397) ^ Depth;
                hashCode = (hashCode * 397) ^ ArraySize;
                hashCode = (hashCode * 397) ^ MipLevels;
                hashCode = (hashCode * 397) ^ Format.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ImageDescription left, ImageDescription right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ImageDescription left, ImageDescription right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return
                $"Dimension: {Dimension}, Width: {Width}, Height: {Height}, Depth: {Depth}, Format: {Format}, ArraySize: {ArraySize}, MipLevels: {MipLevels}";
        }
    }
}
