#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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

using D2D = SharpDX.Direct2D1;
using D3D11 = SharpDX.Direct3D11;

#endregion

namespace SeeingSharp.Multimedia.Core
{
    #region using

    using System;
    using SeeingSharp.Util;

    #endregion

    public class DeviceHandlerD2D
    {
        #region Main references for Direct2D
        private D2D.RenderTarget m_renderTarget;
        private D2D.Device m_deviceD2D;
        private D2D.DeviceContext m_deviceContextD2D;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceHandlerD3D11" /> class.
        /// </summary>
        internal DeviceHandlerD2D(DeviceLoadSettings deviceLoadSettings, EngineFactory engineFactory, EngineDevice engineDevice)
        {
            try
            {
                // Simulate exception if requested
                if (deviceLoadSettings.ThrowD2DInitDeviceError)
                {
                    throw new SeeingSharpGraphicsException("Simulation Direct2D device init exception");
                }

                using (var dxgiDevice = engineDevice.DeviceD3D11_1.QueryInterface<SharpDX.DXGI.Device>())
                {
                    m_deviceD2D = new D2D.Device1(engineFactory.FactoryD2D_2, dxgiDevice);
                    m_deviceContextD2D = new SharpDX.Direct2D1.DeviceContext(
                        m_deviceD2D,
                        D2D.DeviceContextOptions.None);
                    m_renderTarget = m_deviceContextD2D;
                }
            }
            catch (Exception)
            {
                SeeingSharpUtil.SafeDispose(ref m_deviceContextD2D);
                SeeingSharpUtil.SafeDispose(ref m_deviceD2D);
                SeeingSharpUtil.SafeDispose(ref m_renderTarget);
            }
        }

        public bool IsLoaded
        {
            get
            {
                return m_renderTarget != null;
            }
        }

        /// <summary>
        /// Gets a reference to the Direct2D view to the device.
        /// </summary>
        public D2D.Device Device
        {
            get { return m_deviceD2D; }
        }

        /// <summary>
        /// Gets a reference to the device DeviceContext for rendering.
        /// </summary>
        public D2D.DeviceContext DeviceContext
        {
            get { return m_deviceContextD2D; }
        }

        internal D2D.RenderTarget RenderTarget
        {
            get
            {
                return m_renderTarget;
            }
        }
    }
}