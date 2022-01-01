using System;
using SeeingSharp.Core.Devices;

namespace SeeingSharp.Drawing2D.Resources
{
    public interface IImage
    {
    }

    internal interface IImageInternal
    {
        /// <summary>
        /// Gets the image object..
        /// </summary>
        /// <param name="device">The device for which to get the image.</param>
        IDisposable GetImageObject(EngineDevice device);

        /// <summary>
        /// Tries to get the <see cref="BitmapResource"/> which is the source of this image.
        /// </summary>
        BitmapResource TryGetSourceBitmap();
    }
}
