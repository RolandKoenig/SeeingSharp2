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
using System;
using System.Runtime.InteropServices;
using SharpDX;

namespace SeeingSharp.Multimedia.Core
{
    public unsafe class MemoryMappedTexture32bpp : IDisposable
    {
        // The native structure of this texture
        private IntPtr m_pointer;
        private int* m_pointerNative;
        private Size2 m_size;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryMappedTexture32bpp"/> class.
        /// </summary>
        /// <param name="size">The total size of the texture.</param>
        public MemoryMappedTexture32bpp(Size2 size)
        {
            m_pointer = Marshal.AllocHGlobal(size.Width * size.Height * 4);
            m_pointerNative = (int*)m_pointer.ToPointer();
            m_size = size;
            this.CountInts = m_size.Width * m_size.Height;
        }

        /// <summary>
        /// Converts the underlying buffer to a managed byte array.
        /// </summary>
        public byte[] ToArray()
        {
            var result = new byte[this.SizeInBytes];
            Marshal.Copy(m_pointer, result, 0, (int) this.SizeInBytes);
            return result;
        }

        /// <summary>
        /// Gets the value at the given (pixel) location.
        /// </summary>
        /// <param name="xPos">The x position.</param>
        /// <param name="yPos">The y position.</param>
        public int GetValue(int xPos, int yPos)
        {
            return m_pointerNative[xPos + yPos * m_size.Width];
        }

        /// <summary>
        /// Sets all alpha values to one.
        /// </summary>
        public void SetAllAlphaValuesToOne_ARGB()
        {
            const uint ALPHA_BYTE_VALUE = 0xFF000000;

            var pointerUInt = (uint*)m_pointerNative;
            for (var loopIndex = 0; loopIndex < this.CountInts; loopIndex++)
            {
                pointerUInt[loopIndex] |= ALPHA_BYTE_VALUE;
            }
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(m_pointer);
            m_pointer = IntPtr.Zero;
            m_pointerNative = (int*)0;
            m_size = new Size2(0, 0);
        }

        /// <summary>
        /// Gets the total size of the buffer in bytes.
        /// </summary>
        public uint SizeInBytes => (uint)(m_size.Width * m_size.Height * 4);

        public int CountInts { get; }

        /// <summary>
        /// Gets the width of the buffer.
        /// </summary>
        public int Width => m_size.Width;

        /// <summary>
        /// Gets the pitch of the underlying texture data.
        /// (pitch = stride, see https://msdn.microsoft.com/en-us/library/windows/desktop/aa473780(v=vs.85).aspx )
        /// </summary>
        public int Pitch => m_size.Width * 4;

        /// <summary>
        /// Gets the pitch of the underlying texture data.
        /// (pitch = stride, see https://msdn.microsoft.com/en-us/library/windows/desktop/aa473780(v=vs.85).aspx )
        /// </summary>
        public int Stride => m_size.Width * 4;

        /// <summary>
        /// Gets the height of the buffer.
        /// </summary>
        public int Height => m_size.Height;

        /// <summary>
        /// Gets the pixel size of this texture.
        /// </summary>
        public Size2 PixelSize => m_size;

        /// <summary>
        /// Gets the pointer of the buffer.
        /// </summary>
        public IntPtr Pointer
        {
            get
            {
                if (m_pointer == IntPtr.Zero) { throw new ObjectDisposedException("MemoryMappedTextureFloat"); }
                return m_pointer;
            }
        }
    }
}