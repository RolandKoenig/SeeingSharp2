using System;
using System.Threading.Tasks;
using SeeingSharp.Core;
using SeeingSharp.Util;
using WIC = Vortice.WIC;

namespace SeeingSharp.Drawing2D
{
    public class WicBitmap : IDisposable
    {
        // Native resource
        private WIC.IWICBitmap _wicBitmap;

        public int Width
        {
            get
            {
                if (_wicBitmap == null) { throw new ObjectDisposedException("WicBitmap"); }
                return _wicBitmap.Size.Width;
            }
        }

        public int Height
        {
            get
            {
                if (_wicBitmap == null) { throw new ObjectDisposedException("WicBitmap"); }
                return _wicBitmap.Size.Height;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WicBitmap"/> class.
        /// </summary>
        /// <param name="wicBitmap">The unmanaged bitmap data object.</param>
        private WicBitmap(WIC.IWICBitmap wicBitmap)
        {
            _wicBitmap = wicBitmap;
        }

        public static async Task<WicBitmap> FromWicBitmapSourceAsync(WicBitmapSource bitmapSource, int width, int height)
        {
            WIC.IWICBitmap wicBitmap = null;
            await Task.Factory.StartNew(() =>
            {
                GraphicsCore.Current.FactoryWIC.CreateBitmapFromSourceRect(
                    bitmapSource.BitmapSource, 
                    0, 0, width, height);
            });
            return new WicBitmap(wicBitmap);
        }

        /// <summary>
        /// Creates a WIC bitmap from the given source.
        /// </summary>
        /// <param name="resourceLink">The source of the resource.</param>
        public static async Task<WicBitmap> FromResourceLinkAsync(ResourceLink resourceLink)
        {
            WIC.IWICBitmap wicBitmap = null;

            using (var inStream = await resourceLink.OpenInputStreamAsync())

            using (var bitmapSourceWrapper = await SeeingSharpUtil.CallAsync(() => GraphicsHelper.Internals.LoadBitmapSource(inStream)))
            {
                GraphicsCore.Current.FactoryWIC.CreateBitmapFromSource(
                    bitmapSourceWrapper.Converter, 
                    WIC.BitmapCreateCacheOption.CacheOnLoad);
            }

            return new WicBitmap(wicBitmap);
        }

        /// <summary>
        /// Gets an array containing all pixels of the underlying bitmap in Bgra format.
        /// </summary>
        public byte[] GetPixelsAsBgra()
        {
            if (_wicBitmap == null) { throw new ObjectDisposedException("WicBitmap"); }

            var result = new byte[_wicBitmap.Size.Width * _wicBitmap.Size.Height * 4];
            _wicBitmap.CopyPixels(_wicBitmap.Size.Width * 4, result);
            return result;
        }

        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref _wicBitmap);
        }
    }
}