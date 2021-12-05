using System;
using SeeingSharp.Core.Devices;
using SeeingSharp.Drawing2D;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;
using DXGI = Vortice.DXGI;
using D2D = Vortice.Direct2D1;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Core
{
    /// <summary>
    /// This class makes a Direct3D 11 texture available for 2D rendering with Direct2D.
    /// </summary>
    internal class Direct2DOverlayRenderer : IDisposable
    {
        // Graphics object
        private Graphics2D _graphics2D;

        // Given resources
        private EngineDevice _device;
        private D3D11.ID3D11Texture2D _renderTarget3D;

        // Own 2D render target resource
        private D2D.ID2D1RenderTarget _renderTarget2D;
        private D2D.ID2D1Bitmap1 _renderTargetBitmap;

        /// <summary>
        /// Is this resource loaded correctly?
        /// </summary>
        public bool IsLoaded => _renderTarget2D != null;

        internal bool IsRenderTargetDisposed => _renderTarget2D?.IsDisposed == true;

        /// <summary>
        /// Gets the Direct2D render target.
        /// </summary>
        internal D2D.ID2D1RenderTarget RenderTarget2D => _renderTarget2D;

        /// <summary>
        /// Gets the 2D graphics object.
        /// </summary>
        internal Graphics2D Graphics => _graphics2D;

        /// <summary>
        /// Initializes a new instance of the <see cref="Direct2DOverlayRenderer"/> class.
        /// </summary>
        public Direct2DOverlayRenderer(EngineDevice device, D3D11.ID3D11Texture2D renderTarget3D, int viewWidth, int viewHeight, DpiScaling dpiScaling)
            : this(device, renderTarget3D, viewWidth, viewHeight, dpiScaling, false)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Direct2DOverlayRenderer"/> class.
        /// </summary>
        internal Direct2DOverlayRenderer(EngineDevice device, D3D11.ID3D11Texture2D renderTarget3D, int viewWidth, int viewHeight, DpiScaling dpiScaling, bool forceInit)
        {
            _device = device;
            _renderTarget3D = renderTarget3D;

            this.CreateResources(viewWidth, viewHeight, dpiScaling, forceInit);
        }

        /// <summary>
        /// Begins the draw.
        /// </summary>
        public void BeginDraw()
        {
            if (_renderTarget2D == null) { return; }
            if (_renderTarget2D.IsDisposed) { return; }

            _device.DeviceContextD2D.Target = _renderTargetBitmap;
            _device.DeviceContextD2D.Dpi = _renderTargetBitmap.Dpi;

            // Start Direct2D rendering
            _renderTarget2D.BeginDraw();
        }

        /// <summary>
        /// Finishes Direct2D drawing.
        /// </summary>
        public void EndDraw()
        {
            if (_renderTarget2D == null) { return; }
            if (_renderTarget2D.IsDisposed) { return; }

            try
            {
                _renderTarget2D.EndDraw();
            }
            finally
            {
                // Finish Direct2D drawing
                _device.DeviceContextD2D.Target = null;
            }
        }

        /// <summary>
        /// Disposes all resources of this object completely.
        /// </summary>
        public void Dispose()
        {
            // Dispose all created objects
            if (_renderTarget2D != null)
            {
                SeeingSharpUtil.SafeDispose(ref _renderTargetBitmap);
                _renderTarget2D = null;
            }
            else
            {
                SeeingSharpUtil.SafeDispose(ref _renderTarget2D);
            }
        }

        /// <summary>
        /// Creates all resources
        /// </summary>
        private void CreateResources(int viewWidth, int viewHeight, DpiScaling dpiScaling, bool forceInit)
        {
            // Calculate the screen size in device independent units
            var scaledScreenSize = new Size2F(
                viewWidth / dpiScaling.ScaleFactorX,
                viewHeight / dpiScaling.ScaleFactorY);

            // Cancel here if the device does not support 2D rendering
            if (!forceInit &&
               !_device.Supports2D)
            {
                return;
            }

            // Create the render target
            using (var dxgiSurface = _renderTarget3D.QueryInterface<DXGI.IDXGISurface>())
            {
                var bitmapProperties = new D2D.BitmapProperties1
                {
                    DpiX = dpiScaling.DpiX,
                    DpiY = dpiScaling.DpiY,
                    BitmapOptions = D2D.BitmapOptions.Target | D2D.BitmapOptions.CannotDraw,
                    PixelFormat = new Vortice.DCommon.PixelFormat(
                        GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT,
                        Vortice.DCommon.AlphaMode.Premultiplied)
                };

                _renderTargetBitmap = _device.DeviceContextD2D.CreateBitmapFromDxgiSurface(dxgiSurface, bitmapProperties);
                _renderTarget2D = _device.DeviceContextD2D;
                _graphics2D = new Graphics2D(_device, _device.DeviceContextD2D, scaledScreenSize);
            }
        }
    }
}