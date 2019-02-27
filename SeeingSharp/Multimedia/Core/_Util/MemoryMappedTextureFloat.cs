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

namespace SeeingSharp.Multimedia.Core
{
    #region using

    using System;
    using System.Runtime.InteropServices;
    using SharpDX;

    #endregion

    public unsafe class MemoryMappedTextureFloat : IDisposable
    {
        // Some query steps, relevant for QueryForObjectID method
        private static readonly Point[] QUERY_OBJECT_ID_STEPS = new Point[]
        {
            new Point(1, 0),new Point(1, 0),
            new Point(0, 1),
            new Point(-1, 0), new Point(-1, 0), new Point(-1, 0), new Point(-1, 0),
            new Point(0, -1), new Point(0, -1),
            new Point(1, 0), new Point(1, 0), new Point(1, 0), new Point(1, 0),
        };

        // The native structure, where we store all ObjectIDs uploaded from graphics hardware
        private IntPtr m_pointer;
        private float* m_pointerNative;
        private Size2 m_size;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryMappedTextureFloat"/> class.
        /// </summary>
        /// <param name="size">The total size of the texture.</param>
        public MemoryMappedTextureFloat(Size2 size)
        {
            m_pointer = Marshal.AllocHGlobal(size.Width * size.Height * 4);
            m_pointerNative = (float*)m_pointer.ToPointer();
            m_size = size;
        }

        /// <summary>
        /// Queries for the ObjectID at the given location.
        /// </summary>
        /// <param name="xPos">The x position where to start.</param>
        /// <param name="yPos">The y position where to start.</param>
        public float QueryForObjectID(int xPos, int yPos)
        {
            if (xPos < 0) { throw new ArgumentException("xPos"); }
            if (xPos >= m_size.Width) { throw new ArgumentException("xPos"); }
            if (yPos < 0) { throw new ArgumentException("yPos"); }
            if (yPos >= m_size.Height) { throw new ArgumentException("yPos"); }

            // Loop over more pixels to be sure, that we are directly facing one object
            //  => This is needed because of manipulations done by multisampling (=Antialiasing)
            int currentX = xPos;
            int currentY = yPos;
            float lastObjID = m_pointerNative[currentY * m_size.Width + currentX];

            for (var loopActQueryStep = 0; loopActQueryStep < QUERY_OBJECT_ID_STEPS.Length; loopActQueryStep++)
            {
                // Calculate current query location
                var currentStep = QUERY_OBJECT_ID_STEPS[loopActQueryStep];
                currentX = currentX + currentStep.X;
                currentY = currentY + currentStep.Y;

                // Check whether we are still in a valid pixel coordinate
                if (currentX < 0) { continue; }
                if (currentX >= m_size.Width) { continue; }
                if (currentY < 0) { continue; }
                if (currentY >= m_size.Height) { continue; }

                // Read current value and compare with last one
                //  (If equal, than at least two pixels are the same => Return this ObjectID)
                float currObjID = m_pointerNative[currentY * m_size.Width + currentX];
                if (currObjID == lastObjID) { return currObjID; }

                // No match found, continue with next one
                lastObjID = currObjID;
            }

            // No clear match found
            return 0f;
        }

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben aus, die mit dem Freigeben, Zurückgeben oder Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        public void Dispose()
        {
            Marshal.FreeHGlobal(m_pointer);
            m_pointer = IntPtr.Zero;
            m_pointerNative = (float*)0;
            m_size = new Size2(0, 0);
        }

        /// <summary>
        /// Gets the float value which is located on the given location.
        /// </summary>
        /// <param name="xPos">The x location of the float value.</param>
        /// <param name="yPos">The y location of the float value.</param>
        public unsafe float this[int xPos, int yPos]
        {
            get
            {
                if (xPos < 0) { throw new ArgumentException("xPos"); }
                if (xPos >= m_size.Width) { throw new ArgumentException("xPos"); }
                if (yPos < 0) { throw new ArgumentException("yPos"); }
                if (yPos >= m_size.Height) { throw new ArgumentException("yPos"); }

                if (m_pointer == IntPtr.Zero) { throw new ObjectDisposedException("MemoryMappedTextureFloat"); }
                return m_pointerNative[yPos * m_size.Width + xPos];
            }
        }

        /// <summary>
        /// Gets the total size of the buffer in bytes.
        /// </summary>
        public uint SizeInBytes
        {
            get
            {
                return (uint)(m_size.Width * m_size.Height * 4);
            }
        }

        /// <summary>
        /// Gets the width of the buffer.
        /// </summary>
        public int Width
        {
            get { return m_size.Width; }
        }

        /// <summary>
        /// Gets the height of the buffer.
        /// </summary>
        public int Height
        {
            get { return m_size.Height; }
        }

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
