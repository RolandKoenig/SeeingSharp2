using System;
using SeeingSharp.Core;
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Drawing2D
{
    public abstract class BitmapResource : Drawing2DResourceBase, IImage, IImageInternal
    {
        public abstract int PixelWidth
        {
            get;
        }

        public abstract int PixelHeight
        {
            get;
        }

        public abstract double DpiX
        {
            get;
        }

        public abstract double DpiY
        {
            get;
        }

        public abstract int TotalFrameCount
        {
            get;
        }

        public abstract int FrameCountX
        {
            get;
        }

        public abstract int FrameCountY
        {
            get;
        }

        public abstract int SingleFramePixelWidth
        {
            get;
        }

        public abstract int SingleFramePixelHeight
        {
            get;
        }
        internal abstract D2D.Bitmap GetBitmap(EngineDevice engineDevice);

        /// <summary>
        /// Gets the input object for an effect.
        /// </summary>
        /// <param name="device">The device for which to get the input.</param>
        IDisposable IImageInternal.GetImageObject(EngineDevice device)
        {
            return this.GetBitmap(device)
                .QueryInterface<D2D.Bitmap>();
        }

        /// <summary>
        /// Tries to get the <see cref="BitmapResource"/> which is the source of this image.
        /// </summary>
        BitmapResource IImageInternal.TryGetSourceBitmap()
        {
            return this;
        }
    }
}
