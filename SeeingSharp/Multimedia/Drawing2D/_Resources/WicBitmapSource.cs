﻿/*
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
using SharpDX.WIC;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public class WicBitmapSource : IDisposable
    {
        // Native resources
        private WicBitmapSourceInternal m_wicBitmapSource;

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

            using (var inStream = await resourceLink.OpenInputStreamAsync())
            {
                wicBitmapSource = await SeeingSharpUtil.CallAsync(() => GraphicsHelper.LoadBitmapSource(inStream));
            }

            return new WicBitmapSource(wicBitmapSource);
        }

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, der Zurückgabe oder dem Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref m_wicBitmapSource);
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

        internal BitmapSource BitmapSource => m_wicBitmapSource.Converter;
    }
}
