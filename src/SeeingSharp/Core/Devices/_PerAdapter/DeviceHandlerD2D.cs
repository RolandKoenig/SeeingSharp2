using System;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Util;
using SharpDX.DXGI;
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Core.Devices
{
    public class DeviceHandlerD2D
    {
        // Main references for Direct2D
        private D2D.RenderTarget _renderTarget;
        private D2D.Device _deviceD2D;
        private D2D.DeviceContext _deviceContextD2D;

        public bool IsLoaded => _renderTarget != null;

        /// <summary>
        /// Gets a reference to the Direct2D view to the device.
        /// </summary>
        internal D2D.Device Device => _deviceD2D;

        /// <summary>
        /// Gets a reference to the device DeviceContext for rendering.
        /// </summary>
        internal D2D.DeviceContext DeviceContext => _deviceContextD2D;

        internal D2D.RenderTarget RenderTarget => _renderTarget;

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

                using (var dxgiDevice = engineDevice.DeviceD3D11_1.QueryInterface<Device>())
                {
                    _deviceD2D = new D2D.Device1(engineFactory.FactoryD2D_2, dxgiDevice);
                    _deviceContextD2D = new D2D.DeviceContext(
                        _deviceD2D,
                        D2D.DeviceContextOptions.None);
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