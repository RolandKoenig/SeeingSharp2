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
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D2D = SharpDX.Direct2D1;

// Some namespace mappings
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;

namespace SeeingSharp.Multimedia.Core
{
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

                using (DXGI.Device dxgiDevice = engineDevice.DeviceD3D11_1.QueryInterface<DXGI.Device>())
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