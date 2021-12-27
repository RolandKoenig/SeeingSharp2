using System;
using System.Threading.Tasks;
using SeeingSharp.Core;
using SeeingSharp.Util;
using Vortice.WIC;

namespace SeeingSharp.Drawing2D
{
    public class WicBitmapSource : IDisposable
    {
        // Native resources
        private WicBitmapSourceInternal _wicBitmapSource;

        public int Width
        {
            get
            {
                if (_wicBitmapSource == null) { throw new ObjectDisposedException("WicBitmapSource"); }
                return _wicBitmapSource.Converter.Size.Width;
            }
        }

        public int Height
        {
            get
            {
                if (_wicBitmapSource == null) { throw new ObjectDisposedException("WicBitmapSource"); }
                return _wicBitmapSource.Converter.Size.Height;
            }
        }

        internal IWICBitmapSource BitmapSource => _wicBitmapSource.Converter;

        /// <summary>
        /// Initializes a new instance of the <see cref="WicBitmapSource"/> class.
        /// </summary>
        /// <param name="bitmapSource">The bitmap source.</param>
        private WicBitmapSource(WicBitmapSourceInternal bitmapSource)
        {
            _wicBitmapSource = bitmapSource;
        }

        /// <summary>
        /// Creates a WIC BitmapSource object from the given source.
        /// </summary>
        /// <param name="resourceLink">The source of the resource.</param>
        public static async Task<WicBitmapSource> FromResourceSourceAsync(ResourceLink resourceLink)
        {
            WicBitmapSourceInternal wicBitmapSource = null;

            using (var inStream = await resourceLink.OpenInputStreamAsync())
            {
                wicBitmapSource = await SeeingSharpUtil.CallAsync(() => GraphicsHelper.Internals.LoadBitmapSource(inStream));
            }

            return new WicBitmapSource(wicBitmapSource);
        }

        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref _wicBitmapSource);
        }
    }
}
