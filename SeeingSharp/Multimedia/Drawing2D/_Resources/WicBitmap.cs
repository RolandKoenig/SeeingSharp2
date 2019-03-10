/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Threading.Tasks;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public class WicBitmap : IDisposable
    {
        // Native resource
        private Bitmap m_wicBitmap;

        /// <summary>
        /// Initializes a new instance of the <see cref="WicBitmap"/> class.
        /// </summary>
        /// <param name="wicBitmap">The unmanaged bitmap data object.</param>
        private WicBitmap(Bitmap wicBitmap)
        {
            m_wicBitmap = wicBitmap;
        }

        public static async Task<WicBitmap> FromWicBitmapSourceAsync(WicBitmapSource bitmapSource, int width, int height)
        {
            Bitmap wicBitmap = null;
            await Task.Factory.StartNew(() =>
            {
                wicBitmap = new Bitmap(
                    GraphicsCore.Current.FactoryWIC,
                    bitmapSource.BitmapSource,
                    new RawBox(0, 0, width, height));
            });
            return new WicBitmap(wicBitmap);
        }

        /// <summary>
        /// Creates a WIC bitmap from the given source.
        /// </summary>
        /// <param name="resourceLink">The source of the resource.</param>
        public static async Task<WicBitmap> FromResourceLinkAsync(ResourceLink resourceLink)
        {
            Bitmap wicBitmap = null;

            using (var inStream = await resourceLink.OpenInputStreamAsync())

            using (var bitmapSourceWrapper = await SeeingSharpUtil.CallAsync(() => GraphicsHelper.LoadBitmapSource(inStream)))
            {
                wicBitmap = new Bitmap(
                    GraphicsCore.Current.FactoryWIC, bitmapSourceWrapper.Converter, BitmapCreateCacheOption.CacheOnLoad);
            }

            return new WicBitmap(wicBitmap);
        }

        /// <summary>
        /// Gets an array containing all pixels of the underlying bitmap in Bgra format.
        /// </summary>
        public byte[] GetPixelsAsBgra()
        {
            if (m_wicBitmap == null) { throw new ObjectDisposedException("WicBitmap"); }

            var result = new byte[m_wicBitmap.Size.Width * m_wicBitmap.Size.Height * 4];
            m_wicBitmap.CopyPixels(result, m_wicBitmap.Size.Width * 4);
            return result;
        }

        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref m_wicBitmap);
        }

        public int Width
        {
            get
            {
                if (m_wicBitmap == null) { throw new ObjectDisposedException("WicBitmap"); }
                return m_wicBitmap.Size.Width;
            }
        }

        public int Height
        {
            get
            {
                if (m_wicBitmap == null) { throw new ObjectDisposedException("WicBitmap"); }
                return m_wicBitmap.Size.Height;
            }
        }
    }
}