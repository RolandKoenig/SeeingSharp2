#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
using SeeingSharp.Multimedia.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public abstract class BitmapResource : Drawing2DResourceBase, IImage, IImageInternal
    {
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
    }
}
