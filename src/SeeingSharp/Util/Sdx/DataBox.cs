// This code is ported from SharpDX
// https://github.com/sharpdx/SharpDX

using System;
using System.Runtime.InteropServices;

namespace SeeingSharp.Util.Sdx
{
    /// <summary>
    /// Provides access to data organized in 3D.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DataBox
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataBox"/> struct.
        /// </summary>
        /// <param name="datapointer">The datapointer.</param>
        /// <param name="rowPitch">The row pitch.</param>
        /// <param name="slicePitch">The slice pitch.</param>
        public DataBox(IntPtr datapointer, int rowPitch, int slicePitch)
        {
            DataPointer = datapointer;
            RowPitch = rowPitch;
            SlicePitch = slicePitch;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataBox"/> struct.
        /// </summary>
        /// <param name="dataPointer">The data pointer.</param>
        public DataBox(IntPtr dataPointer)
        {
            DataPointer = dataPointer;
            RowPitch = 0;
            SlicePitch = 0;
        }

        /// <summary>
        /// Pointer to the data.
        /// </summary>
        public IntPtr DataPointer;

        /// <summary>
        /// Gets the number of bytes per row.
        /// </summary>
        public int RowPitch;

        /// <summary>
        /// Gets the number of bytes per slice (for a 3D texture, a slice is a 2D image)
        /// </summary>
        public int SlicePitch;

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty
        {
            get
            {
                return DataPointer == IntPtr.Zero && RowPitch == 0 && SlicePitch == 0;
            }
        }
    }
}
