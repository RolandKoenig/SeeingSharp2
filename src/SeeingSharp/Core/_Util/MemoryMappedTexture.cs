using System;
using System.Drawing;
using System.Runtime.InteropServices;
using SeeingSharp.Util;

namespace SeeingSharp.Core
{
    public unsafe class MemoryMappedTexture<T> : IDisposable, ICheckDisposed
        where T : unmanaged
    {
        // The native structure, where we store all texture data
        private IntPtr _pointer;
        private T* _pointerNative;
        private Size _size;

        /// <summary>
        /// Gets the float value which is located on the given location.
        /// </summary>
        /// <param name="xPos">The x location of the float value.</param>
        /// <param name="yPos">The y location of the float value.</param>
        public T this[int xPos, int yPos]
        {
            get
            {
                if (xPos < 0) { throw new ArgumentException("xPos"); }
                if (xPos >= _size.Width) { throw new ArgumentException("xPos"); }
                if (yPos < 0) { throw new ArgumentException("yPos"); }
                if (yPos >= _size.Height) { throw new ArgumentException("yPos"); }
                if (_pointer == IntPtr.Zero) { throw new ObjectDisposedException(nameof(MemoryMappedTexture<T>)); }

                return _pointerNative[yPos * _size.Width + xPos];
            }
        }

        /// <summary>
        /// Gets the total size of the buffer in bytes.
        /// </summary>
        public uint SizeInBytes
        {
            get
            {
                if (_pointer == IntPtr.Zero) { throw new ObjectDisposedException(nameof(MemoryMappedTexture<T>)); }
                return (uint) (_size.Width * _size.Height * sizeof(T));
            }
        }

        /// <summary>
        /// Gets the pixel size of this texture.
        /// </summary>
        public Size PixelSize
        {
            get
            {
                if (_pointer == IntPtr.Zero) { throw new ObjectDisposedException(nameof(MemoryMappedTexture<T>)); }
                return _size;
            }
        }

        /// <summary>
        /// Gets the width of the buffer.
        /// </summary>
        public int Width
        {
            get
            {
                if (_pointer == IntPtr.Zero) { throw new ObjectDisposedException(nameof(MemoryMappedTexture<T>)); }
                return _size.Width;
            }
        }

        /// <summary>
        /// Gets the height of the buffer.
        /// </summary>
        public int Height
        {
            get
            {
                if (_pointer == IntPtr.Zero) { throw new ObjectDisposedException(nameof(MemoryMappedTexture<T>)); }
                return _size.Height;
            }
        }

        /// <summary>
        /// Gets the pointer of the buffer.
        /// </summary>
        public IntPtr Pointer
        {
            get
            {
                if (_pointer == IntPtr.Zero) { throw new ObjectDisposedException(nameof(MemoryMappedTexture<T>)); }
                return _pointer;
            }
        }

        /// <inheritdoc />
        public bool IsDisposed => _pointer == IntPtr.Zero;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryMappedTexture{T}"/> class.
        /// </summary>
        /// <param name="size">The total size of the texture.</param>
        public MemoryMappedTexture(Size size)
        {
            _pointer = Marshal.AllocHGlobal(size.Width * size.Height * 4);
            _pointerNative = (T*)_pointer.ToPointer();
            _size = size;
        }

        /// <summary>
        /// Disposes this object.
        /// </summary>
        public void Dispose()
        {
            Marshal.FreeHGlobal(_pointer);
            _pointer = IntPtr.Zero;
            _pointerNative = (T*)0;
            _size = new Size(0, 0);
        }

        /// <summary>
        /// Gets the native pointer to the memory.
        /// </summary>
        public T* GetNativePointer()
        {
            if (_pointer == IntPtr.Zero) { throw new ObjectDisposedException(nameof(MemoryMappedTexture<T>)); }
            return _pointerNative;
        }
    }
}
