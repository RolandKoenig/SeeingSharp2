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
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using SharpDX.DXGI;
using SharpDX.WIC;
using System;
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Multimedia.Drawing2D
{
    /// <summary>
    /// This object represents a in-memory cached bitmap which is
    /// loaded from a ResourceLink (e. g. a file).
    /// </summary>
    public class StandardBitmapResource : BitmapResource
    {
        // Resources
        private D2D.Bitmap[] _loadedBitmaps;

        // Configuration
        private ResourceLink _resourceLink;
        private int _framesX;
        private int _framesY;
        private int _totalFrameCount;

        // RuntimeValues
        private bool _firstLoadDone;
        private int _framePixelWidth;
        private int _framePixelHeight;
        private int _pixelWidth;
        private int _pixelHeight;
        private double _dpiX;
        private double _dpyY;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardBitmapResource"/> class.
        /// </summary>
        /// <param name="resource">The resource from w.</param>
        /// <param name="frameCountX">Total count of frames in x direction.</param>
        /// <param name="frameCountY">Total count of frames in y direction.</param>
        public StandardBitmapResource(ResourceLink resource, int frameCountX = 1, int frameCountY = 1)
        {
            frameCountX.EnsurePositiveAndNotZero(nameof(frameCountX));
            frameCountY.EnsurePositiveAndNotZero(nameof(frameCountY));

            _loadedBitmaps = new D2D.Bitmap[GraphicsCore.Current.DeviceCount];
            _resourceLink = resource;

            // Set default values (modified after first load)
            _firstLoadDone = false;
            _pixelWidth = 0;
            _pixelHeight = 0;
            _dpiX = 96.0;
            _dpyY = 96.0;
            _framesX = frameCountX;
            _framesY = frameCountY;
            _totalFrameCount = _framesX * _framesY;
        }

        public override string ToString()
        {
            return $"Bitmap ({_pixelWidth}x{_pixelHeight} pixels)";
        }

        /// <summary>
        /// Disposes this object.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        /// <summary>
        /// Gets the bitmap.
        /// </summary>
        /// <param name="engineDevice">The engine device.</param>
        internal override D2D.Bitmap GetBitmap(EngineDevice engineDevice)
        {
            if (this.IsDisposed) { throw new ObjectDisposedException(this.GetType().Name); }

            var result = _loadedBitmaps[engineDevice.DeviceIndex];
            if (result == null)
            {
                using (var inputStream = _resourceLink.OpenInputStream())
                using (var bitmapSourceWrapper = GraphicsHelper.Internals.LoadBitmapSource_D2D(inputStream))
                {
                    BitmapSource bitmapSource = bitmapSourceWrapper.Converter;

                    // Store common properties about the bitmap
                    if (!_firstLoadDone)
                    {
                        _firstLoadDone = true;
                        _pixelWidth = bitmapSource.Size.Width;
                        _pixelHeight = bitmapSource.Size.Height;
                        if (_totalFrameCount > 1)
                        {
                            _framePixelWidth = _pixelWidth / _framesX;
                            _framePixelHeight = _pixelHeight / _framesY;
                        }
                        else
                        {
                            _framePixelWidth = _pixelWidth;
                            _framePixelHeight = _pixelHeight;
                        }
                        bitmapSource.GetResolution(out _dpiX, out _dpyY);
                    }

                    // Load the bitmap into Direct2D
                    result = D2D.Bitmap.FromWicBitmap(
                        engineDevice.FakeRenderTarget2D, bitmapSource,
                        new D2D.BitmapProperties(new D2D.PixelFormat(
                            Format.B8G8R8A8_UNorm,
                            D2D.AlphaMode.Premultiplied)));

                    // Register loaded bitmap
                    _loadedBitmaps[engineDevice.DeviceIndex] = result;
                    engineDevice.RegisterDeviceResource(this);
                }
            }

            return result;
        }

        /// <summary>
        /// Unloads all resources loaded on the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to unload the resource.</param>
        internal override void UnloadResources(EngineDevice engineDevice)
        {
            var brush = _loadedBitmaps[engineDevice.DeviceIndex];

            if (brush != null)
            {
                engineDevice.DeregisterDeviceResource(this);

                SeeingSharpUtil.DisposeObject(brush);
                _loadedBitmaps[engineDevice.DeviceIndex] = null;
            }
        }

        /// <summary>
        /// Gets the width of the bitmap in pixel´.
        /// </summary>
        public override int PixelWidth => _pixelWidth;

        /// <summary>
        /// Gets the height of the bitmap in pixel.
        /// </summary>
        public override int PixelHeight => _pixelHeight;

        public override double DpiX => _dpiX;

        public override double DpiY => _dpyY;

        public override int FrameCountX => _framesX;

        public override int FrameCountY => _framesY;

        public override int TotalFrameCount => _totalFrameCount;

        public override int SingleFramePixelWidth => _framePixelWidth;

        public override int SingleFramePixelHeight => _framePixelHeight;
    }
}
