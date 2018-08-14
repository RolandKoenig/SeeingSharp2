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
    /// Used by <see cref="Image"/> to provide a selector to a <see cref="PixelBuffer"/>.
    /// </summary>
    public sealed class PixelBufferArray
    {
        private readonly Image image;

        internal PixelBufferArray(Image image)
        {
            this.image = image;
        }

        /// <summary>
        /// Gets the pixel buffer.
        /// </summary>
        /// <returns>A <see cref="PixelBuffer"/>.</returns>
        public PixelBuffer this[int bufferIndex]
        {
            get
            {
                return this.image.pixelBuffers[bufferIndex];
            }
        }

        /// <summary>
        /// Gets the total number of pixel buffers.
        /// </summary>
        /// <returns>The total number of pixel buffers.</returns>
        public int Count { get { return this.image.pixelBuffers.Length; } }

        /// <summary>
        /// Gets the pixel buffer for the specified array/z slice and mipmap level.
        /// </summary>
        /// <param name="arrayOrDepthSlice">For 3D image, the parameter is the Z slice, otherwise it is an index into the texture array.</param>
        /// <param name="mipIndex">The mip map slice index.</param>
        /// <returns>A <see cref="PixelBuffer"/>.</returns>
        public PixelBuffer this[int arrayOrDepthSlice, int mipIndex]
        {
            get
            {
                return this.image.GetPixelBuffer(arrayOrDepthSlice, mipIndex);
            }
        }

        /// <summary>
        /// Gets the pixel buffer for the specified array/z slice and mipmap level.
        /// </summary>
        /// <param name="arrayIndex">Index into the texture array. Must be set to 0 for 3D images.</param>
        /// <param name="zIndex">Z index for 3D image. Must be set to 0 for all 1D/2D images.</param>
        /// <param name="mipIndex">The mip map slice index.</param>
        /// <returns>A <see cref="PixelBuffer"/>.</returns>
        public PixelBuffer this[int arrayIndex, int zIndex, int mipIndex]
        {
            get
            {
                return this.image.GetPixelBuffer(arrayIndex, zIndex, mipIndex);
            }
        }
    }
}
