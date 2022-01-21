// This code is ported from SharpDX.Toolkit
// see: https://github.com/sharpdx/Toolkit

using System;
using System.IO;
using SeeingSharp.Util.Sdx;
using Vortice.DXGI;

#nullable disable

namespace SeeingSharp.Util.SdxTK
{
    /// <summary>
    /// An unmanaged buffer of pixels.
    /// </summary>
    internal sealed class PixelBuffer
    {
        private Format format;

        /// <summary>
        /// True when RowStride == sizeof(pixelformat) * width
        /// </summary>
        private bool isStrictRowStride;

        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>The width.</value>
        public int Width { get; }

        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <value>The height.</value>
        public int Height { get; }

        /// <summary>
        /// Gets the format (this value can be changed)
        /// </summary>
        /// <value>The format.</value>
        public Format Format
        {
            get => format;
            set
            {
                if (this.PixelSize != FormatHelper.SizeOfInBytes(value))
                {
                    throw new ArgumentException(
                        $"Format [{value}] doesn't have same pixel size in bytes than current format [{format}]");
                }
                format = value;
            }
        }

        /// <summary>
        /// Gets the pixel size in bytes.
        /// </summary>
        /// <value>The pixel size in bytes.</value>
        public int PixelSize { get; }

        /// <summary>
        /// Gets the row stride in number of bytes.
        /// </summary>
        /// <value>The row stride in number of bytes.</value>
        public int RowStride { get; }

        /// <summary>
        /// Gets the total size in bytes of this pixel buffer.
        /// </summary>
        /// <value>The size in bytes of the pixel buffer.</value>
        public int BufferStride { get; }

        /// <summary>
        /// Gets the pointer to the pixel buffer.
        /// </summary>
        /// <value>The pointer to the pixel buffer.</value>
        public IntPtr DataPointer { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PixelBuffer" /> struct.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">The format.</param>
        /// <param name="rowStride">The row pitch.</param>
        /// <param name="bufferStride">The slice pitch.</param>
        /// <param name="dataPointer">The pixels.</param>
        public PixelBuffer(int width, int height, Format format, int rowStride, int bufferStride, IntPtr dataPointer)
        {
            if (dataPointer == IntPtr.Zero)
            {
                throw new ArgumentException("Pointer cannot be equal to IntPtr.Zero", "dataPointer");
            }

            this.Width = width;
            this.Height = height;
            this.format = format;
            this.RowStride = rowStride;
            this.BufferStride = bufferStride;
            this.DataPointer = dataPointer;
            this.PixelSize = FormatHelper.SizeOfInBytes(this.format);
            isStrictRowStride = this.PixelSize * width == rowStride;
        }

        /// <summary>
        /// Copies this pixel buffer to a destination pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">The destination pixel buffer.</param>
        /// <remarks>
        /// The destination pixel buffer must have exactly the same dimensions (width, height) and format than this instance.
        /// Destination buffer can have different row stride.
        /// </remarks>
        public unsafe void CopyTo(PixelBuffer pixelBuffer)
        {
            // Check that buffers are identical
            if (this.Width != pixelBuffer.Width
                || this.Height != pixelBuffer.Height
                || this.PixelSize != FormatHelper.SizeOfInBytes(pixelBuffer.Format))
            {
                throw new ArgumentException("Invalid destination pixelBufferArray. Mush have same Width, Height and Format", "pixelBuffer");
            }

            // If buffers have same size, than we can copy it directly
            if (this.BufferStride == pixelBuffer.BufferStride)
            {
                SdxUtilities.CopyMemory(pixelBuffer.DataPointer, this.DataPointer, this.BufferStride);
            }
            else
            {
                var srcPointer = (byte*)this.DataPointer;
                var dstPointer = (byte*)pixelBuffer.DataPointer;
                var rowStride = Math.Min(this.RowStride, pixelBuffer.RowStride);

                // Copy per scanline
                for (var i = 0; i < this.Height; i++)
                {
                    SdxUtilities.CopyMemory(new IntPtr(dstPointer), new IntPtr(srcPointer), rowStride);
                    srcPointer += this.RowStride;
                    dstPointer += pixelBuffer.RowStride;
                }
            }
        }

        /// <summary>
        /// Saves this pixel buffer to a file.
        /// </summary>
        /// <param name="fileName">The destination file.</param>
        /// <param name="fileType">Specify the output format.</param>
        /// <remarks>This method support the following format: <c>dds, bmp, jpg, png, gif, tiff, wmp, tga</c>.</remarks>
        public void Save(string fileName, ImageFileType fileType)
        {
            using (var imageStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                this.Save(imageStream, fileType);
            }
        }

        /// <summary>
        /// Saves this pixel buffer to a stream.
        /// </summary>
        /// <param name="imageStream">The destination stream.</param>
        /// <param name="fileType">Specify the output format.</param>
        /// <remarks>This method support the following format: <c>dds, bmp, jpg, png, gif, tiff, wmp, tga</c>.</remarks>
        public void Save(Stream imageStream, ImageFileType fileType)
        {
            var description = new ImageDescription
            {
                Width = this.Width,
                Height = this.Height,
                Depth = 1,
                ArraySize = 1,
                Dimension = TextureDimension.Texture2D,
                Format = format,
                MipLevels = 1
            };
            Image.Save(new[] { this }, 1, description, imageStream, fileType);
        }

        /// <summary>
        /// Gets the pixel value at a specified position.
        /// </summary>
        /// <typeparam name="T">Type of the pixel data</typeparam>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>The pixel value.</returns>
        /// <remarks>
        /// Caution, this method doesn't check bounding.
        /// </remarks>
        public unsafe T GetPixel<T>(int x, int y) where T : struct
        {
            return SdxUtilities.Read<T>(new IntPtr((byte*)this.DataPointer + this.RowStride * y + x * this.PixelSize));
        }

        /// <summary>
        /// Gets the pixel value at a specified position.
        /// </summary>
        /// <typeparam name="T">Type of the pixel data</typeparam>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="value">The pixel value.</param>
        /// <remarks>
        /// Caution, this method doesn't check bounding.
        /// </remarks>
        public unsafe void SetPixel<T>(int x, int y, T value) where T : struct
        {
            SdxUtilities.Write(new IntPtr((byte*)this.DataPointer + this.RowStride * y + x * this.PixelSize), ref value);
        }

        /// <summary>
        /// Gets scanline pixels from the buffer.
        /// </summary>
        /// <typeparam name="T">Type of the pixel data</typeparam>
        /// <param name="yOffset">The y line offset.</param>
        /// <returns>Scanline pixels from the buffer</returns>
        /// <exception cref="System.ArgumentException">If the sizeof(T) is an invalid size</exception>
        public T[] GetPixels<T>(int yOffset = 0) where T : struct
        {
            var sizeOfOutputPixel = SdxUtilities.SizeOf<T>();
            var totalSize = this.Width * this.Height * this.PixelSize;
            if (totalSize % sizeOfOutputPixel != 0)
            {
                throw new ArgumentException($"Invalid sizeof(T), not a multiple of current size [{totalSize}]in bytes ");
            }

            var buffer = new T[totalSize / sizeOfOutputPixel];
            this.GetPixels(buffer, yOffset);
            return buffer;
        }

        /// <summary>
        /// Gets scanline pixels from the buffer.
        /// </summary>
        /// <typeparam name="T">Type of the pixel data</typeparam>
        /// <param name="pixels">An allocated scanline pixel buffer</param>
        /// <param name="yOffset">The y line offset.</param>
        /// <returns>Scanline pixels from the buffer</returns>
        /// <exception cref="System.ArgumentException">If the sizeof(T) is an invalid size</exception>
        public void GetPixels<T>(T[] pixels, int yOffset = 0) where T : struct
        {
            this.GetPixels(pixels, yOffset, 0, pixels.Length);
        }

        /// <summary>
        /// Gets scanline pixels from the buffer.
        /// </summary>
        /// <typeparam name="T">Type of the pixel data</typeparam>
        /// <param name="pixels">An allocated scanline pixel buffer</param>
        /// <param name="yOffset">The y line offset.</param>
        /// <param name="pixelIndex">Offset into the destination pixels buffer.</param>
        /// <param name="pixelCount">Number of pixels to write into the destination pixels" buffer.</param>
        /// <exception cref="System.ArgumentException">If the sizeof(T) is an invalid size</exception>
        /// <remarks>
        /// This method is working on a row basis. The yOffset is specifying the first row to get
        /// the pixels from.
        /// </remarks>
        public unsafe void GetPixels<T>(T[] pixels, int yOffset, int pixelIndex, int pixelCount) where T : struct
        {
            var pixelPointer = (byte*)this.DataPointer + yOffset * this.RowStride;
            if (isStrictRowStride)
            {
                SdxUtilities.Read(new IntPtr(pixelPointer), pixels, 0, pixelCount);
            }
            else
            {
                var sizeOfOutputPixel = SdxUtilities.SizeOf<T>() * pixelCount;
                var sizePerWidth = sizeOfOutputPixel / this.Width;
                var remainingPixels = sizeOfOutputPixel % this.Width;
                for (var i = 0; i < sizePerWidth; i++)
                {
                    SdxUtilities.Read(new IntPtr(pixelPointer), pixels, pixelIndex, this.Width);
                    pixelPointer += this.RowStride;
                    pixelIndex += this.Width;
                }
                if (remainingPixels > 0)
                {
                    SdxUtilities.Read(new IntPtr(pixelPointer), pixels, pixelIndex, remainingPixels);
                }
            }
        }

        /// <summary>
        /// Sets scanline pixels to the buffer.
        /// </summary>
        /// <typeparam name="T">Type of the pixel data</typeparam>
        /// <param name="sourcePixels">Source pixel buffer</param>
        /// <param name="yOffset">The y line offset.</param>
        /// <exception cref="System.ArgumentException">If the sizeof(T) is an invalid size</exception>
        /// <remarks>
        /// This method is working on a row basis. The yOffset is specifying the first row to get
        /// the pixels from.
        /// </remarks>
        public void SetPixels<T>(T[] sourcePixels, int yOffset = 0) where T : struct
        {
            this.SetPixels(sourcePixels, yOffset, 0, sourcePixels.Length);
        }

        /// <summary>
        /// Sets scanline pixels to the buffer.
        /// </summary>
        /// <typeparam name="T">Type of the pixel data</typeparam>
        /// <param name="sourcePixels">Source pixel buffer</param>
        /// <param name="yOffset">The y line offset.</param>
        /// <param name="pixelIndex">Offset into the source sourcePixels buffer.</param>
        /// <param name="pixelCount">Number of pixels to write into the source sourcePixels buffer.</param>
        /// <exception cref="System.ArgumentException">If the sizeof(T) is an invalid size</exception>
        /// <remarks>
        /// This method is working on a row basis. The yOffset is specifying the first row to get
        /// the pixels from.
        /// </remarks>
        public unsafe void SetPixels<T>(T[] sourcePixels, int yOffset, int pixelIndex, int pixelCount) where T : struct
        {
            var pixelPointer = (byte*)this.DataPointer + yOffset * this.RowStride;
            if (isStrictRowStride)
            {
                SdxUtilities.Write(new IntPtr(pixelPointer), sourcePixels, 0, pixelCount);
            }
            else
            {
                var sizeOfOutputPixel = SdxUtilities.SizeOf<T>() * pixelCount;
                var sizePerWidth = sizeOfOutputPixel / this.Width;
                var remainingPixels = sizeOfOutputPixel % this.Width;
                for (var i = 0; i < sizePerWidth; i++)
                {
                    SdxUtilities.Write(new IntPtr(pixelPointer), sourcePixels, pixelIndex, this.Width);
                    pixelPointer += this.RowStride;
                    pixelIndex += this.Width;
                }
                if (remainingPixels > 0)
                {
                    SdxUtilities.Write(new IntPtr(pixelPointer), sourcePixels, pixelIndex, remainingPixels);
                }
            }
        }
    }
}
