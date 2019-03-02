#region License information
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
#endregion
#region using
using System;
using SeeingSharp.Multimedia.Core;
using D2D = SharpDX.Direct2D1;

#endregion

namespace SeeingSharp.Multimedia.Drawing2D
{
    #region using
    #endregion

    public abstract class BitmapResource : Drawing2DResourceBase, IImage, IImageInternal
    {
        internal abstract D2D.Bitmap GetBitmap(EngineDevice engineDevice);

        /// <summary>
        /// Gets the input object for an effect.
        /// </summary>
        /// <param name="device">The device for which to get the input.</param>
        IDisposable IImageInternal.GetImageObject(EngineDevice device)
        {
            return GetBitmap(device)
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
