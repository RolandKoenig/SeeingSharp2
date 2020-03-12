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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using SharpDX.WIC;
using System;
using System.Threading.Tasks;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public class WicBitmapSource : IDisposable
    {
        // Native resources
        private WicBitmapSourceInternal _wicBitmapSource;

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

        internal BitmapSource BitmapSource => _wicBitmapSource.Converter;
    }
}
