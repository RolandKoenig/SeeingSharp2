﻿// This code is ported from SharpDX
// https://github.com/sharpdx/SharpDX

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SharpGen.Runtime;
using D3D = Vortice.Direct3D;

namespace SeeingSharp.Util.Sdx
{
    /// <summary>
    /// Provides methods to perform fast read/write random access data on a buffer located in an unmanaged memory.
    /// </summary>
    /// <remarks>
    /// This class doesn't validate the position read/write from. It is the responsibility of the client of this class
    /// to verify that access is done within the size of the buffer.
    /// </remarks>
    internal class DataBuffer : DisposeBase
    {
        private unsafe sbyte* _buffer;
        private GCHandle _gCHandle;
        private readonly bool _ownsBuffer;
        private readonly int _size;
        private D3D.Blob? _blob;

        /// <summary>
        /// Creates the specified user buffer.
        /// </summary>
        /// <typeparam name="T">Type of the buffer.</typeparam>
        /// <param name="userBuffer">The buffer to use as a DataBuffer.</param>
        /// <param name="index">Index inside the buffer in terms of element count (not size in bytes).</param>
        /// <param name="pinBuffer">True to keep the managed buffer and pin it, false will allocate unmanaged memory and make a copy of it. Default is true.</param>
        /// <returns>An instance of a DataBuffer</returns>
        public static DataBuffer Create<T>(T[] userBuffer, int index = 0, bool pinBuffer = true) where T : struct
        {
            unsafe
            {
                if (userBuffer == null)
                    throw new ArgumentNullException("userBuffer");

                if (index < 0 || index > userBuffer.Length)
                    throw new ArgumentException("Index is out of range [0, userBuffer.Length-1]", "index");

                DataBuffer buffer;

                var sizeOfBuffer = SdxUtilities.SizeOf(userBuffer);
                var indexOffset = index * SdxUtilities.SizeOf<T>();

                if (pinBuffer)
                {
                    var handle = GCHandle.Alloc(userBuffer, GCHandleType.Pinned);
                    buffer = new DataBuffer(indexOffset + (byte*)handle.AddrOfPinnedObject(), sizeOfBuffer - indexOffset, handle);
                }
                else
                {
                    // The .NET Native compiler crashes if '(IntPtr)' is removed.
                    buffer = new DataBuffer(indexOffset + (byte*)(IntPtr)Interop.Fixed(userBuffer), sizeOfBuffer - indexOffset, true);
                }

                return buffer;
            }
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "DataBuffer" /> class, and allocates a new buffer to use as a backing store.
        /// </summary>
        /// <param name = "sizeInBytes">The size of the buffer to be allocated, in bytes.</param>
        /// <exception cref = "T:System.ArgumentOutOfRangeException">
        ///   <paramref name = "sizeInBytes" /> is less than 1.</exception>
        public DataBuffer(int sizeInBytes)
        {
            unsafe
            {
                Debug.Assert(sizeInBytes > 0);

                _buffer = (sbyte*)SdxUtilities.AllocateMemory(sizeInBytes);
                _size = sizeInBytes;
                _ownsBuffer = true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataBuffer"/> class.
        /// </summary>
        /// <param name="dataPointer">The data pointer.</param>
        public DataBuffer(DataPointer dataPointer)
            : this(dataPointer.Pointer, dataPointer.Size)
        {
        }


        /// <summary>
        ///   Initializes a new instance of the <see cref = "DataBuffer" /> class, using an unmanaged buffer as a backing store.
        /// </summary>
        /// <param name = "userBuffer">A pointer to the buffer to be used as a backing store.</param>
        /// <param name = "sizeInBytes">The size of the buffer provided, in bytes.</param>
        public unsafe DataBuffer(IntPtr userBuffer, int sizeInBytes)
            : this((void*)userBuffer, sizeInBytes, false)
        {
        }


        internal unsafe DataBuffer(void* buffer, int sizeInBytes, GCHandle handle)
        {
            Debug.Assert(sizeInBytes > 0);

            _buffer = (sbyte*)buffer;
            _size = sizeInBytes;
            _gCHandle = handle;
            _ownsBuffer = false;
        }

        internal unsafe DataBuffer(void* buffer, int sizeInBytes, bool makeCopy)
        {
            Debug.Assert(sizeInBytes > 0);

            if (makeCopy)
            {
                _buffer = (sbyte*)SdxUtilities.AllocateMemory(sizeInBytes);
                SdxUtilities.CopyMemory((IntPtr)_buffer, (IntPtr)buffer, sizeInBytes);
            }
            else
            {
                _buffer = (sbyte*)buffer;
            }
            _size = sizeInBytes;
            _ownsBuffer = makeCopy;
        }

        internal unsafe DataBuffer(D3D.Blob buffer)
        {
            Debug.Assert(buffer.BufferSize > 0);

            _buffer = (sbyte*)buffer.BufferPointer;
            _size = buffer.BufferSize;
            _blob = buffer;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
#if !WINDOWS_UWP
                if (_blob != null)
                {
                    _blob.Dispose();
                    _blob = null;
                }
#endif
            }

            if (_gCHandle.IsAllocated)
                _gCHandle.Free();

            unsafe
            {
                if (_ownsBuffer && _buffer != (sbyte*)0)
                {
                    SdxUtilities.FreeMemory((IntPtr)_buffer);
                    _buffer = (sbyte*)0;
                }
            }
        }

        /// <summary>
        /// Clears the buffer.
        /// </summary>
        public unsafe void Clear(byte value = 0)
        {
            SdxUtilities.ClearMemory((IntPtr)_buffer, value, Size);
        }

        /// <summary>
        ///   Gets a single value from the current buffer at the specified position.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <typeparam name = "T">The type of the value to be read from the buffer.</typeparam>
        /// <returns>The value that was read.</returns>
        public T Get<T>(int positionInBytes) where T : struct
        {
            unsafe
            {
                T result = default(T);
                SdxUtilities.Read((IntPtr)(_buffer + positionInBytes), ref result);
                return result;
            }
        }

        /// <summary>
        /// Gets a single value from the current buffer at the specified position.
        /// </summary>
        /// <typeparam name="T">The type of the value to be read from the buffer.</typeparam>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <param name="value">The value as out.</param>
        /// <returns>The value that was read.</returns>
        public void Get<T>(int positionInBytes, out T value) where T : struct
        {
            unsafe
            {
                SdxUtilities.ReadOut((IntPtr)(_buffer + positionInBytes), out value);
            }
        }

        /// <summary>
        ///   Gets an array of values from a position in the buffer.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <param name="count">number of T instance to get from the positionInBytes</param>
        /// <typeparam name = "T">The type of the values to be read from the buffer.</typeparam>
        /// <returns>An array of values that was read from the current buffer.</returns>
        public T[] GetRange<T>(int positionInBytes, int count) where T : struct
        {
            unsafe
            {
                var result = new T[count];
                SdxUtilities.Read((IntPtr)(_buffer + positionInBytes), result, 0, count);
                return result;
            }
        }

        /// <summary>
        ///   Gets a sequence of elements from a position in the buffer into a target buffer.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <param name = "buffer">An array of values to be read from the buffer.</param>
        /// <param name = "offset">The zero-based byte offset in buffer at which to begin storing
        ///   the data read from the current buffer.</param>
        /// <param name = "count">The number of values to be read from the current buffer.</param>
        public void GetRange<T>(int positionInBytes, T[] buffer, int offset, int count) where T : struct
        {
            unsafe
            {
                SdxUtilities.Read((IntPtr)(_buffer + positionInBytes), buffer, offset, count);
            }
        }

        /// <summary>
        ///   Sets a single value to the buffer at a specified position.
        /// </summary>
        /// <typeparam name = "T">The type of the value to be written to the buffer.</typeparam>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name = "value">The value to write to the buffer.</param>
        public void Set<T>(int positionInBytes, ref T value) where T : struct
        {
            unsafe
            {
                Interop.CopyInline(_buffer + positionInBytes, ref value);
            }
        }

        /// <summary>
        ///   Sets a single value to the buffer at a specified position.
        /// </summary>
        /// <typeparam name = "T">The type of the value to be written to the buffer.</typeparam>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name = "value">The value to write to the buffer.</param>
        public void Set<T>(int positionInBytes, T value) where T : struct
        {
            unsafe
            {
                Interop.CopyInline(_buffer + positionInBytes, ref value);
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name="value">The value.</param>
        public void Set(int positionInBytes, bool value)
        {
            unsafe
            {
                *((int*)(_buffer + positionInBytes)) = value ? 1 : 0;
            }
        }

        /// <summary>
        ///   Sets an array of values to a specified position into the buffer.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name = "data">An array of values to be written to the current buffer.</param>
        public void Set<T>(int positionInBytes, T[] data) where T : struct
        {
            Set(positionInBytes, data, 0, data.Length);
        }

        /// <summary>
        ///   Sets a range of data to a specified position into the buffer.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name = "source">A pointer to the location to start copying from.</param>
        /// <param name = "count">The number of bytes to copy from source to the current buffer.</param>
        public void Set(int positionInBytes, IntPtr source, long count)
        {
            unsafe
            {
                SdxUtilities.CopyMemory((IntPtr)(_buffer + positionInBytes), source, (int)count);
            }
        }

        /// <summary>
        ///   Sets an array of values to a specified position into the buffer.
        /// </summary>
        /// <typeparam name = "T">The type of the values to be written to the buffer.</typeparam>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name = "data">An array of values to be written to the buffer.</param>
        /// <param name = "offset">The zero-based offset in data at which to begin copying values to the current buffer.</param>
        /// <param name = "count">The number of values to be written to the current buffer. If this is zero,
        ///   all of the contents <paramref name = "data" /> will be written.</param>
        public void Set<T>(int positionInBytes, T[] data, int offset, int count) where T : struct
        {
            unsafe
            {
                SdxUtilities.Write((IntPtr)(_buffer + positionInBytes), data, offset, count);
            }
        }


        /// <summary>
        ///   Gets a pointer to the buffer used as a backing store..
        /// </summary>
        /// <value>An IntPtr to the buffer being used as a backing store.</value>
        public IntPtr DataPointer
        {
            get
            {
                unsafe
                {
                    return new IntPtr(_buffer);
                }
            }
        }

        /// <summary>
        ///   Gets the length in bytes of the buffer.
        /// </summary>
        /// <value>A long value representing the length of the buffer in bytes.</value>
        public int Size
        {
            get { return _size; }
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="DataBuffer"/> to <see cref="DataPointer"/>.
        /// </summary>
        /// <param name="from">The from value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator DataPointer(DataBuffer from)
        {
            return new DataPointer(from.DataPointer, (int)from.Size);
        }
    }
}
