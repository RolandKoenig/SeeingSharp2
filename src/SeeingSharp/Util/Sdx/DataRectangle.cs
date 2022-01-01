using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SeeingSharp.Util.Sdx
{
    /// <summary>
    /// Provides access to data organized in 2D.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct DataRectangle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataRectangle"/> class.
        /// </summary>
        /// <param name="dataPointer">The data pointer.</param>
        /// <param name="pitch">The pitch.</param>
        public DataRectangle(IntPtr dataPointer, int pitch)
        {
            DataPointer = dataPointer;
            Pitch = pitch;
        }
        /// <summary>
        /// Pointer to the data.
        /// </summary>
        public IntPtr DataPointer;

        /// <summary>
        /// Gets the number of bytes per row.
        /// </summary>
        public int Pitch;
    }
}
