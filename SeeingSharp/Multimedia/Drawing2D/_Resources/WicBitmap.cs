#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion

namespace SeeingSharp.Multimedia.Drawing2D
{
    #region using

    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Core;
    using SeeingSharp.Util;

    #endregion

    public class WicBitmap : IDisposable
    {
        #region Native resource
        private SharpDX.WIC.Bitmap m_wicBitmap;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="WicBitmap"/> class.
        /// </summary>
        /// <param name="wicBitmap">The unmanaged bitmap data object.</param>
        private WicBitmap(SharpDX.WIC.Bitmap wicBitmap)
        {
            m_wicBitmap = wicBitmap;
        }

        public static async Task<WicBitmap> FromWicBitmapSourceAsync(WicBitmapSource bitmapSource, int width, int height)
        {
            SharpDX.WIC.Bitmap wicBitmap = null;
            await Task.Factory.StartNew(() =>
            {
                wicBitmap = new SharpDX.WIC.Bitmap(
                    GraphicsCore.Current.FactoryWIC,
                    bitmapSource.BitmapSource,
                    new SharpDX.Mathematics.Interop.RawBox(0, 0, width, height));
            });
            return new WicBitmap(wicBitmap);
        }

        /// <summary>
        /// Creates a WIC bitmap from the given source.
        /// </summary>
        /// <param name="resourceLink">The source of the resource.</param>
        public static async Task<WicBitmap> FromResourceLinkAsync(ResourceLink resourceLink)
        {
            SharpDX.WIC.Bitmap wicBitmap = null;

            using (var inStream = await resourceLink.OpenInputStreamAsync())

            using (var bitmapSourceWrapper = await SeeingSharpTools.CallAsync(() => GraphicsHelper.LoadBitmapSource(inStream)))
            {
                wicBitmap = new SharpDX.WIC.Bitmap(
                    GraphicsCore.Current.FactoryWIC, bitmapSourceWrapper.Converter, SharpDX.WIC.BitmapCreateCacheOption.CacheOnLoad);
            }

            return new WicBitmap(wicBitmap);
        }

        /// <summary>
        /// Gets an array containing all pixels of the underlying bitmap in Bgra format.
        /// </summary>
        public byte[] GetPixelsAsBgra()
        {
            if (m_wicBitmap == null) { throw new ObjectDisposedException("WicBitmap"); }

            byte[] result = new byte[m_wicBitmap.Size.Width * m_wicBitmap.Size.Height * 4];
            m_wicBitmap.CopyPixels(result, m_wicBitmap.Size.Width * 4);
            return result;
        }

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben aus, die mit dem Freigeben, Zurückgeben oder Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        public void Dispose()
        {
            SeeingSharpTools.SafeDispose(ref m_wicBitmap);
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