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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using SharpDX.DXGI;
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public class WritableBitmapResource : BitmapResource
    {
        // Configuration
        private Size2 _bitmapSize;
        private D2D.PixelFormat _pixelFormat;
        private double _dpiX;
        private double _dpiY;

        // Resources
        private D2D.Bitmap[] _loadedBitmaps;

        public override int PixelWidth => _bitmapSize.Width;

        public override int PixelHeight => _bitmapSize.Height;

        public override double DpiX => _dpiX;

        public override double DpiY => _dpiY;

        public override int FrameCountX => 1;

        public override int FrameCountY => 1;

        public override int TotalFrameCount => 1;

        public override int SingleFramePixelWidth => _bitmapSize.Width;

        public override int SingleFramePixelHeight => _bitmapSize.Height;

        /// <summary>
        /// Initializes a new instance of the <see cref="WritableBitmapResource"/> class.
        /// </summary>
        public WritableBitmapResource(
            Size2 bitmapSize,
            BitmapFormat format = BitmapFormat.Bgra,
            AlphaMode alphaMode = AlphaMode.Straight,
            double dpiX = 96.0, double dpiY = 96.0)
        {
            _loadedBitmaps = new D2D.Bitmap[GraphicsCore.Current.DeviceCount];
            _bitmapSize = bitmapSize;
            _pixelFormat = new D2D.PixelFormat(
                (Format)format,
                (D2D.AlphaMode)alphaMode);
            _dpiX = dpiX;
            _dpiY = dpiY;
        }

        /// <summary>
        /// Sets the bitmaps contents.
        /// </summary>
        /// <param name="graphics">The graphics object.</param>
        /// <param name="pointer">The pointer to the source data.</param>
        /// <param name="pitch"></param>
        public void SetBitmapContent(Graphics2D graphics, IntPtr pointer, int pitch)
        {
            var bitmap = this.GetBitmap(graphics.Device);
            bitmap.CopyFromMemory(pointer, pitch);
        }

        /// <summary>
        /// Gets the bitmap for the given device..
        /// </summary>
        /// <param name="engineDevice">The engine device.</param>
        internal override D2D.Bitmap GetBitmap(EngineDevice engineDevice)
        {
            // Check for disposed state
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            var result = _loadedBitmaps[engineDevice.DeviceIndex];
            if (result == null)
            {
                // Load the bitmap initially
                result = new D2D.Bitmap(
                    engineDevice.FakeRenderTarget2D,
                    SdxMathHelper.SdxFromSize2(_bitmapSize),
                    new D2D.BitmapProperties(_pixelFormat, (float)_dpiX, (float)_dpiY));
                _loadedBitmaps[engineDevice.DeviceIndex] = result;
                engineDevice.RegisterDeviceResource(this);
            }

            return result;
        }

        /// <summary>
        /// Unloads all resources loaded on the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to unload the resource.</param>
        internal override void UnloadResources(EngineDevice engineDevice)
        {
            var bitmap = _loadedBitmaps[engineDevice.DeviceIndex];
            if (bitmap != null)
            {
                engineDevice.DeregisterDeviceResource(this);

                SeeingSharpUtil.DisposeObject(bitmap);
                _loadedBitmaps[engineDevice.DeviceIndex] = null;
            }
        }
    }
}
