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

    public class WicBitmapSource : IDisposable
    {
        #region Native resources
        private WicBitmapSourceInternal m_wicBitmapSource;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="WicBitmapSource"/> class.
        /// </summary>
        /// <param name="bitmapSource">The bitmap source.</param>
        private WicBitmapSource(WicBitmapSourceInternal bitmapSource)
        {
            m_wicBitmapSource = bitmapSource;
        }

        /// <summary>
        /// Creates a WIC BitmapSource object from the given source.
        /// </summary>
        /// <param name="resourceLink">The source of the resource.</param>
        public static async Task<WicBitmapSource> FromResourceSourceAsync(ResourceLink resourceLink)
        {
            WicBitmapSourceInternal wicBitmapSource = null;
            using (Stream inStream = await resourceLink.OpenInputStreamAsync())
            {
                wicBitmapSource = await SeeingSharpTools.CallAsync(() => GraphicsHelper.LoadBitmapSource(inStream));
            }

            return new WicBitmapSource(wicBitmapSource);
        }

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, der Zurückgabe oder dem Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        public void Dispose()
        {
            SeeingSharpTools.SafeDispose(ref m_wicBitmapSource);
        }

        public int Width
        {
            get 
            {
                if (m_wicBitmapSource == null) { throw new ObjectDisposedException("WicBitmapSource"); }
                return m_wicBitmapSource.Converter.Size.Width;
            }
        }

        public int Height
        {
            get
            {
                if (m_wicBitmapSource == null) { throw new ObjectDisposedException("WicBitmapSource"); }
                return m_wicBitmapSource.Converter.Size.Height;
            }
        }

        internal SharpDX.WIC.BitmapSource BitmapSource
        {
            get { return m_wicBitmapSource.Converter; }
        }
    }
}
