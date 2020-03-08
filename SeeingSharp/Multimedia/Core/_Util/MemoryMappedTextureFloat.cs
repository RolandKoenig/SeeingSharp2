///*
//    Seeing# and all applications distributed together with it. 
//	Exceptions are projects where it is noted otherwise.
//    More info at 
//     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
//     - http://www.rolandk.de (the authors homepage, german)
//    Copyright (C) 2019 Roland König (RolandK)
    
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU Lesser General Public License as published
//    by the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
    
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU Lesser General Public License for more details.

//    You should have received a copy of the GNU Lesser General Public License
//    along with this program.  If not, see http://www.gnu.org/licenses/.
//*/
//using System;
//using System.Runtime.InteropServices;

//namespace SeeingSharp.Multimedia.Core
//{
//    public unsafe class MemoryMappedTextureFloat : IDisposable
//    {


//        // The native structure, where we store all ObjectIDs uploaded from graphics hardware
//        private IntPtr m_pointer;
//        private float* m_pointerNative;
//        private Size2 m_size;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="MemoryMappedTextureFloat"/> class.
//        /// </summary>
//        /// <param name="size">The total size of the texture.</param>
//        public MemoryMappedTextureFloat(Size2 size)
//        {
//            m_pointer = Marshal.AllocHGlobal(size.Width * size.Height * 4);
//            m_pointerNative = (float*)m_pointer.ToPointer();
//            m_size = size;
//        }



//        public void Dispose()
//        {
//            Marshal.FreeHGlobal(m_pointer);
//            m_pointer = IntPtr.Zero;
//            m_pointerNative = (float*)0;
//            m_size = new Size2(0, 0);
//        }

//        /// <summary>
//        /// Gets the float value which is located on the given location.
//        /// </summary>
//        /// <param name="xPos">The x location of the float value.</param>
//        /// <param name="yPos">The y location of the float value.</param>
//        public float this[int xPos, int yPos]
//        {
//            get
//            {
//                if (xPos < 0) { throw new ArgumentException("xPos"); }
//                if (xPos >= m_size.Width) { throw new ArgumentException("xPos"); }
//                if (yPos < 0) { throw new ArgumentException("yPos"); }
//                if (yPos >= m_size.Height) { throw new ArgumentException("yPos"); }

//                if (m_pointer == IntPtr.Zero) { throw new ObjectDisposedException("MemoryMappedTextureFloat"); }
//                return m_pointerNative[yPos * m_size.Width + xPos];
//            }
//        }

//        /// <summary>
//        /// Gets the total size of the buffer in bytes.
//        /// </summary>
//        public uint SizeInBytes => (uint)(m_size.Width * m_size.Height * 4);

//        /// <summary>
//        /// Gets the width of the buffer.
//        /// </summary>
//        public int Width => m_size.Width;

//        /// <summary>
//        /// Gets the height of the buffer.
//        /// </summary>
//        public int Height => m_size.Height;

//        /// <summary>
//        /// Gets the pointer of the buffer.
//        /// </summary>
//        public IntPtr Pointer
//        {
//            get
//            {
//                if (m_pointer == IntPtr.Zero) { throw new ObjectDisposedException("MemoryMappedTextureFloat"); }
//                return m_pointer;
//            }
//        }
//    }
//}
