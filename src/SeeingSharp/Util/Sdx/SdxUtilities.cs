// This code is ported from SharpDX
// https://github.com/sharpdx/SharpDX

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace SeeingSharp.Util.Sdx
{
    internal static class SdxUtilities
    {
        /// <summary>
        /// Allocate an aligned memory buffer.
        /// </summary>
        /// <param name="sizeInBytes">Size of the buffer to allocate.</param>
        /// <param name="align">Alignment, 16 bytes by default.</param>
        /// <returns>A pointer to a buffer aligned.</returns>
        /// <remarks>
        /// To free this buffer, call <see cref="FreeMemory"/>.
        /// </remarks>
        public unsafe static IntPtr AllocateMemory(int sizeInBytes, int align = 16)
        {
            int mask = align - 1;
            var memPtr = Marshal.AllocHGlobal(sizeInBytes + mask + IntPtr.Size);
            var ptr = (long)((byte*)memPtr + sizeof(void*) + mask) & ~mask;
            ((IntPtr*)ptr)[-1] = memPtr;
            return new IntPtr((void*)ptr);
        }

        /// <summary>
        /// Allocate an aligned memory buffer.
        /// </summary>
        /// <returns>A pointer to a buffer aligned.</returns>
        /// <remarks>
        /// The buffer must have been allocated with <see cref="AllocateMemory"/>.
        /// </remarks>
        public unsafe static void FreeMemory(IntPtr alignedBuffer)
        {
            if (alignedBuffer == IntPtr.Zero) return;
            Marshal.FreeHGlobal(((IntPtr*)alignedBuffer)[-1]);
        }

        /// <summary>
        /// Clears the memory.
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <param name="value">The value.</param>
        /// <param name="sizeInBytesToClear">The size in bytes to clear.</param>
        public static void ClearMemory(IntPtr dest, byte value, int sizeInBytesToClear)
        {
            unsafe
            {
                Interop.memset((void*)dest, value, sizeInBytesToClear);
            }
        }

        /// <summary>
        /// Return the sizeof a struct from a CLR. Equivalent to sizeof operator but works on generics too.
        /// </summary>
        /// <typeparam name="T">A struct to evaluate.</typeparam>
        /// <returns>Size of this struct.</returns>
        public static int SizeOf<T>() where T : struct
        {
            return Interop.SizeOf<T>();
        }

        /// <summary>
        /// Return the sizeof an array of struct. Equivalent to sizeof operator but works on generics too.
        /// </summary>
        /// <typeparam name="T">A struct.</typeparam>
        /// <param name="array">The array of struct to evaluate.</param>
        /// <returns>Size in bytes of this array of struct.</returns>
        public static int SizeOf<T>(T[] array) where T : struct
        {
            return array == null ? 0 : array.Length * Interop.SizeOf<T>();
        }

        /// <summary>
        /// Reads the specified T data from a memory location.
        /// </summary>
        /// <typeparam name="T">Type of a data to read.</typeparam>
        /// <param name="source">Memory location to read from.</param>
        /// <returns>The data read from the memory location.</returns>
        public static T Read<T>(IntPtr source) where T : struct
        {
            unsafe
            {
                return Interop.ReadInline<T>((void*)source);
            }
        }

        /// <summary>
        /// Reads the specified array T[] data from a memory location.
        /// </summary>
        /// <typeparam name="T">Type of a data to read.</typeparam>
        /// <param name="source">Memory location to read from.</param>
        /// <param name="data">The data write to.</param>
        /// <param name="offset">The offset in the array to write to.</param>
        /// <param name="count">The number of T element to read from the memory location.</param>
        /// <returns>source pointer + sizeof(T) * count.</returns>
        public static IntPtr Read<T>(IntPtr source, T[] data, int offset, int count) where T : struct
        {
            unsafe
            {
                return (IntPtr)Interop.Read((void*)source, data, offset, count);
            }
        }

        /// <summary>
        /// Reads the specified T data from a memory location.
        /// </summary>
        /// <typeparam name="T">Type of a data to read.</typeparam>
        /// <param name="source">Memory location to read from.</param>
        /// <param name="data">The data write to.</param>
        /// <returns>source pointer + sizeof(T).</returns>
        public static void Read<T>(IntPtr source, ref T data) where T : struct
        {
            unsafe
            {
                Interop.CopyInline(ref data, (void*)source);
            }
        }

        /// <summary>
        /// Reads the specified T data from a memory location.
        /// </summary>
        /// <typeparam name="T">Type of a data to read.</typeparam>
        /// <param name="source">Memory location to read from.</param>
        /// <param name="data">The data write to.</param>
        /// <returns>source pointer + sizeof(T).</returns>
        public static void ReadOut<T>(IntPtr source, out T data) where T : struct
        {
            unsafe
            {
                Interop.CopyInlineOut(out data, (void*)source);
            }
        }

        /// <summary>
        /// Reads the specified T data from a memory location.
        /// </summary>
        /// <typeparam name="T">Type of a data to read.</typeparam>
        /// <param name="source">Memory location to read from.</param>
        /// <param name="data">The data write to.</param>
        /// <returns>source pointer + sizeof(T).</returns>
        public static IntPtr ReadAndPosition<T>(IntPtr source, ref T data) where T : struct
        {
            unsafe
            {
                return (IntPtr)Interop.Read((void*)source, ref data);
            }
        }

        /// <summary>
        /// Writes the specified T data to a memory location.
        /// </summary>
        /// <typeparam name="T">Type of a data to write.</typeparam>
        /// <param name="destination">Memory location to write to.</param>
        /// <param name="data">The data to write.</param>
        /// <returns>destination pointer + sizeof(T).</returns>
        public static void Write<T>(IntPtr destination, ref T data) where T : struct
        {
            unsafe
            {
                Interop.CopyInline((void*)destination, ref data);
            }
        }

        /// <summary>
        /// Writes the specified array T[] data to a memory location.
        /// </summary>
        /// <typeparam name="T">Type of a data to write.</typeparam>
        /// <param name="destination">Memory location to write to.</param>
        /// <param name="data">The array of T data to write.</param>
        /// <param name="offset">The offset in the array to read from.</param>
        /// <param name="count">The number of T element to write to the memory location.</param>
        /// <returns>destination pointer + sizeof(T) * count.</returns>
        public static IntPtr Write<T>(IntPtr destination, T[] data, int offset, int count) where T : struct
        {
            unsafe
            {
                return (IntPtr)Interop.Write((void*)destination, data, offset, count);
            }
        }

        /// <summary>
        /// Writes the specified T data to a memory location.
        /// </summary>
        /// <typeparam name="T">Type of a data to write.</typeparam>
        /// <param name="destination">Memory location to write to.</param>
        /// <param name="data">The data to write.</param>
        /// <returns>destination pointer + sizeof(T).</returns>
        public static IntPtr WriteAndPosition<T>(IntPtr destination, ref T data) where T : struct
        {
            unsafe
            {
                return (IntPtr)Interop.Write((void*)destination, ref data);
            }
        }

        /// <summary>
        /// Native memcpy.
        /// </summary>
        /// <param name="dest">The destination memory location.</param>
        /// <param name="src">The source memory location.</param>
        /// <param name="sizeInBytesToCopy">The byte count.</param>
        public static void CopyMemory(IntPtr dest, IntPtr src, int sizeInBytesToCopy)
        {
            unsafe
            {
                // TODO plug in Interop a pluggable CopyMemory using cpblk or memcpy based on architecture
                Interop.memcpy((void*)dest, (void*)src, sizeInBytesToCopy);
            }
        }

        /// <summary>
        /// Read stream to a byte[] buffer.
        /// </summary>
        /// <param name="stream">Input stream.</param>
        /// <returns>A byte[] buffer.</returns>
        public static byte[] ReadStream(Stream stream)
        {
            int readLength = 0;
            return ReadStream(stream, ref readLength);
        }

        /// <summary>
        /// Read stream to a byte[] buffer.
        /// </summary>
        /// <param name="stream">Input stream.</param>
        /// <param name="readLength">Length to read.</param>
        /// <returns>A byte[] buffer.</returns>
        public static byte[] ReadStream(Stream stream, ref int readLength)
        {
            Debug.Assert(stream != null);
            Debug.Assert(stream.CanRead);
            int num = readLength;
            Debug.Assert(num <= (stream.Length - stream.Position));
            if (num == 0)
                readLength = (int)(stream.Length - stream.Position);
            num = readLength;

            Debug.Assert(num >= 0);
            if (num == 0)
                return new byte[0];

            byte[] buffer = new byte[num];
            int bytesRead = 0;
            if (num > 0)
            {
                do
                {
                    bytesRead += stream.Read(buffer, bytesRead, readLength - bytesRead);
                } while (bytesRead < readLength);
            }
            return buffer;
        }
    }
}
