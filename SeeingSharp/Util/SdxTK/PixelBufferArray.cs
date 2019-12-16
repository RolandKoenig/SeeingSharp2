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

// This code is ported from SharpDX.Toolkit
// see: https://github.com/sharpdx/Toolkit

namespace SeeingSharp.Util.SdxTK
{
    /// <summary>
    /// Used by <see cref="Image"/> to provide a selector to a <see cref="PixelBuffer"/>.
    /// </summary>
    internal sealed class PixelBufferArray
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
        public PixelBuffer this[int bufferIndex] => image.pixelBuffers[bufferIndex];

        /// <summary>
        /// Gets the total number of pixel buffers.
        /// </summary>
        /// <returns>The total number of pixel buffers.</returns>
        public int Count => image.pixelBuffers.Length;

        /// <summary>
        /// Gets the pixel buffer for the specified array/z slice and mipmap level.
        /// </summary>
        /// <param name="arrayOrDepthSlice">For 3D image, the parameter is the Z slice, otherwise it is an index into the texture array.</param>
        /// <param name="mipIndex">The mip map slice index.</param>
        /// <returns>A <see cref="PixelBuffer"/>.</returns>
        public PixelBuffer this[int arrayOrDepthSlice, int mipIndex] => image.GetPixelBuffer(arrayOrDepthSlice, mipIndex);

        /// <summary>
        /// Gets the pixel buffer for the specified array/z slice and mipmap level.
        /// </summary>
        /// <param name="arrayIndex">Index into the texture array. Must be set to 0 for 3D images.</param>
        /// <param name="zIndex">Z index for 3D image. Must be set to 0 for all 1D/2D images.</param>
        /// <param name="mipIndex">The mip map slice index.</param>
        /// <returns>A <see cref="PixelBuffer"/>.</returns>
        public PixelBuffer this[int arrayIndex, int zIndex, int mipIndex] => image.GetPixelBuffer(arrayIndex, zIndex, mipIndex);
    }
}
