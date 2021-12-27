using System;
using System.Drawing;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using DXGI = Vortice.DXGI;
using D2D = Vortice.Direct2D1;

namespace SeeingSharp.Drawing2D
{
    public class WritableBitmapResource : BitmapResource
    {
        // Configuration
        private Size _bitmapSize;
        private Vortice.DCommon.PixelFormat _pixelFormat;
        private double _dpiX;
        private double _dpiY;

        // Resources
        private D2D.ID2D1Bitmap[] _loadedBitmaps;

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
            Size bitmapSize,
            BitmapFormat format = BitmapFormat.Bgra,
            AlphaMode alphaMode = AlphaMode.Straight,
            double dpiX = 96.0, double dpiY = 96.0)
        {
            _loadedBitmaps = new D2D.ID2D1Bitmap[GraphicsCore.Current.DeviceCount];
            _bitmapSize = bitmapSize;
            _pixelFormat = new Vortice.DCommon.PixelFormat(
                (DXGI.Format)format,
                (Vortice.DCommon.AlphaMode)alphaMode);
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
        internal override D2D.ID2D1Bitmap GetBitmap(EngineDevice engineDevice)
        {
            // Check for disposed state
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            var result = _loadedBitmaps[engineDevice.DeviceIndex];
            if (result == null)
            {
                using var mapped = new MemoryMappedTexture<int>(_bitmapSize);

                // Load the bitmap initially
                result = engineDevice.FakeRenderTarget2D.CreateBitmap(
                    new System.Drawing.Size(_bitmapSize.Width, _bitmapSize.Height),
                    mapped.Pointer, mapped.Width * 4,
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
