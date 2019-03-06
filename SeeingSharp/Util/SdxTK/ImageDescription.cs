#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion

// Namespace mappings

// This code is ported from SharpDX.Toolkit
// see: https://github.com/sharpdx/Toolkit

namespace SeeingSharp.Multimedia.Util.SdxTK
{
    #region using

    using System;
    using System.Runtime.InteropServices;

    #endregion

    /// <summary>
    /// A description for <see cref="Image"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ImageDescription : IEquatable<ImageDescription>
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
        /// <dd> <p>Texture format (see <strong><see cref="SharpDX.DXGI.Format"/></strong>).</p> </dd>
        /// </summary>
        /// <msdn-id>ff476252</msdn-id>
        /// <unmanaged>DXGI_FORMAT Format</unmanaged>
        /// <unmanaged-short>DXGI_FORMAT Format</unmanaged-short>
        public SharpDX.DXGI.Format Format;

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

            return obj is ImageDescription && Equals((ImageDescription)obj);
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