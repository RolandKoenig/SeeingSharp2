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
using SeeingSharp.Util;
using SharpDX.DXGI;
using System;
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Multimedia.Core
{
    public class DeviceHandlerD2D
    {
        // Main references for Direct2D
        private D2D.RenderTarget _renderTarget;
        private D2D.Device _deviceD2D;
        private D2D.DeviceContext _deviceContextD2D;

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
    }
}