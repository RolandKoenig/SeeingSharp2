﻿// This code is ported from SharpDX
// https://github.com/sharpdx/SharpDX

using System;
using System.Runtime.InteropServices;

namespace SeeingSharp.Util.Sdx
{
    /// <summary>
    /// Pointer to a native buffer with a specific size.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct DataPointer : IEquatable<DataPointer>
    {
        /// <summary>
        /// Gets an Empty Data Pointer.
        /// </summary>
        public static readonly DataPointer Zero = new DataPointer(IntPtr.Zero, 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPointer" /> struct.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="size">The size.</param>
        public DataPointer(IntPtr pointer, int size)
        {
            Pointer = pointer;
            Size = size;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPointer" /> struct.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="size">The size.</param>
        public unsafe DataPointer(void* pointer, int size)
        {
            Pointer = (IntPtr)pointer;
            Size = size;
        }

        /// <summary>
        /// Pointer to the buffer.
        /// </summary>
        public IntPtr Pointer;

        /// <summary>
        /// Size in bytes of the buffer.
        /// </summary>
        public int Size;

        /// <summary>
        /// Gets a value indicating whether this instance is empty (zeroed).
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty
        {
            get { return Equals(Zero); }
        }

        public bool Equals(DataPointer other)
        {
            return Pointer.Equals(other.Pointer) && Size == other.Size;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is DataPointer && Equals((DataPointer)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Pointer.GetHashCode() * 397) ^ Size;
            }
        }

        /// <summary>
        /// Converts this DataPointer to a <see cref="DataStream"/>.
        /// </summary>
        /// <returns>An instance of a <see cref="DataStream"/>.</returns>
        public DataStream ToDataStream()
        {
            if (Pointer == IntPtr.Zero) throw new InvalidOperationException("DataPointer is Zero");

            return new DataStream(this);
        }

        /// <summary>
        /// Converts this DataPointer to a <see cref="DataBuffer"/>.
        /// </summary>
        /// <returns>An instance of a <see cref="DataBuffer"/>.</returns>
        public DataBuffer ToDataBuffer()
        {
            if (Pointer == IntPtr.Zero) throw new InvalidOperationException("DataPointer is Zero");

            return new DataBuffer(this);
        }

        /// <summary>
        /// Converts this instance to a read only byte buffer.
        /// </summary>
        /// <returns>A readonly byte buffer.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// DataPointer is Zero
        /// or
        /// Size cannot be &lt; 0
        /// </exception>
        public byte[] ToArray()
        {
            if (Pointer == IntPtr.Zero) throw new InvalidOperationException("DataPointer is Zero");
            if (Size < 0) throw new InvalidOperationException("Size cannot be < 0");
            var buffer = new byte[Size];
            SdxUtilities.Read(Pointer, buffer, 0, Size);
            return buffer;
        }

        /// <summary>
        /// Converts this instance to a read only typed buffer.
        /// </summary>
        /// <typeparam name="T">Type of a buffer element</typeparam>
        /// <returns>A readonly typed buffer.</returns>
        /// <exception cref="System.InvalidOperationException">DataPointer is Zero</exception>
        public T[] ToArray<T>() where T : struct
        {
            if (Pointer == IntPtr.Zero) throw new InvalidOperationException("DataPointer is Zero");
            var buffer = new T[Size / SdxUtilities.SizeOf<T>()];
            CopyTo(buffer, 0, buffer.Length);
            return buffer;
        }

        /// <summary>
        /// Reads the content of the unmanaged memory location of this instance to the specified buffer.
        /// </summary>
        /// <typeparam name="T">Type of a buffer element</typeparam>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset in the array to write to.</param>
        /// <param name="count">The number of T element to read from the memory location.</param>
        /// <exception cref="System.ArgumentNullException">buffer</exception>
        /// <exception cref="System.InvalidOperationException">DataPointer is Zero</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">buffer;Total buffer size cannot be larger than size of this data pointer</exception>
        public void CopyTo<T>(T[] buffer, int offset, int count) where T : struct
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (Pointer == IntPtr.Zero) throw new InvalidOperationException("DataPointer is Zero");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset", "Must be >= 0");
            if (count <= 0) throw new ArgumentOutOfRangeException("count", "Must be > 0");
            if (count * SdxUtilities.SizeOf<T>() > Size) throw new ArgumentOutOfRangeException("buffer", "Total buffer size cannot be larger than size of this data pointer");
            SdxUtilities.Read(Pointer, buffer, offset, count);
        }

        /// <summary>
        /// Writes the content of the specified buffer to the unmanaged memory location of this instance.
        /// </summary>
        /// <typeparam name="T">Type of a buffer element</typeparam>
        /// <param name="buffer">The buffer.</param>
        /// <exception cref="System.ArgumentNullException">buffer</exception>
        /// <exception cref="System.InvalidOperationException">DataPointer is Zero</exception>
        public void CopyFrom<T>(T[] buffer) where T : struct
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (Pointer == IntPtr.Zero) throw new InvalidOperationException("DataPointer is Zero");
            CopyFrom(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes the content of the specified buffer to the unmanaged memory location of this instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer">The buffer to read from.</param>
        /// <param name="offset">The offset in the array to read from.</param>
        /// <param name="count">The number of T element to write to the memory location.</param>
        /// <exception cref="System.ArgumentNullException">buffer</exception>
        /// <exception cref="System.InvalidOperationException">DataPointer is Zero</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">buffer;Total buffer size cannot be larger than size of this data pointer</exception>
        public void CopyFrom<T>(T[] buffer, int offset, int count) where T : struct
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (Pointer == IntPtr.Zero) throw new InvalidOperationException("DataPointer is Zero");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset", "Must be >= 0");
            if (count <= 0) throw new ArgumentOutOfRangeException("count", "Must be > 0");
            if (count * SdxUtilities.SizeOf<T>() > Size) throw new ArgumentOutOfRangeException("buffer", "Total buffer size cannot be larger than size of this data pointer");
            SdxUtilities.Write(Pointer, buffer, offset, count);
        }

        /// <summary>
        /// Implements the ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(DataPointer left, DataPointer right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(DataPointer left, DataPointer right)
        {
            return !left.Equals(right);
        }
    }
}
