using System;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Util;
using DXGI = Vortice.DXGI;
using D2D = Vortice.Direct2D1;
using static Vortice.Direct2D1.D2D1;

namespace SeeingSharp.Core.Devices
{
    public class DeviceHandlerD2D
    {
        // Main references for Direct2D
        private D2D.ID2D1RenderTarget _renderTarget;
        private D2D.ID2D1Device _deviceD2D;
        private D2D.ID2D1DeviceContext _deviceContextD2D;

        public bool IsLoaded => _renderTarget != null;

        /// <summary>
        /// Gets a reference to the Direct2D view to the device.
        /// </summary>
        internal D2D.ID2D1Device Device => _deviceD2D;

        /// <summary>
        /// Gets a reference to the device DeviceContext for rendering.
        /// </summary>
        internal D2D.ID2D1DeviceContext DeviceContext => _deviceContextD2D;

        internal D2D.ID2D1RenderTarget RenderTarget => _renderTarget;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceHandlerD3D11" /> class.
        /// </summary>
        internal DeviceHandlerD2D(GraphicsDeviceConfiguration deviceConfig, EngineFactory engineFactory, EngineDevice engineDevice)
        {
            try
            {
                // Simulate exception if requested
                if (deviceConfig.CoreConfiguration.ThrowD2DInitDeviceError)
                {
                    throw new SeeingSharpGraphicsException("Simulation Direct2D device init exception");
                }

                using (var dxgiDevice = engineDevice.DeviceD3D11_1.QueryInterface<DXGI.IDXGIDevice>())
                {
                    _deviceD2D = engineFactory.FactoryD2D_2.CreateDevice(dxgiDevice);
                    _deviceContextD2D = _deviceD2D
                        .CreateDeviceContext(D2D.DeviceContextOptions.None);
                    _renderTarget = _deviceContextD2D;
                }
            }
            catch (Exception)
            {
                SeeingSharpUtil.SafeDispose(ref _deviceContextD2D);
                SeeingSharpUtil.SafeDispose(ref _deviceD2D);
                SeeingSharpUtil.SafeDispose(ref _renderTarget);
            }
        }

        public void UnloadResources()
        {
            SeeingSharpUtil.SafeDispose(ref _deviceContextD2D);
            SeeingSharpUtil.SafeDispose(ref _deviceD2D);
            SeeingSharpUtil.SafeDispose(ref _renderTarget);
        }
    }
}