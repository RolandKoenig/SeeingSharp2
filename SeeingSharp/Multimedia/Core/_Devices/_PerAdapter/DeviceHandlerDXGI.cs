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

using D3D11 = SharpDX.Direct3D11;

#endregion

namespace SeeingSharp.Multimedia.Core
{
    #region using

    using SeeingSharp.Util;

    #endregion

    public class DeviceHandlerDXGI
    {
        private SharpDX.DXGI.Factory2 m_factory;
        private SharpDX.DXGI.Adapter1 m_adapter;
        private SharpDX.DXGI.Device3 m_device;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceHandlerDXGI"/> class.
        /// </summary>
        internal DeviceHandlerDXGI(SharpDX.DXGI.Adapter1 adapter, D3D11.Device device)
        {
            m_device = device.QueryInterface<SharpDX.DXGI.Device3>();
            m_adapter = adapter;

            m_factory = m_adapter.GetParent<SharpDX.DXGI.Factory2>();
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        internal void UnloadResources()
        {
            m_factory = SeeingSharpUtil.DisposeObject(m_factory);
            m_adapter = SeeingSharpUtil.DisposeObject(m_adapter);
            m_device = SeeingSharpUtil.DisposeObject(m_device);
        }

        /// <summary>
        /// Gets current factory object.
        /// </summary>
        /// <value>The factory.</value>
        internal SharpDX.DXGI.Factory2 Factory
        {
            get { return m_factory; }
        }

        /// <summary>
        /// Gets the DXGI device.
        /// </summary>
        internal SharpDX.DXGI.Device3 Device
        {
            get { return m_device; }
        }

        /// <summary>
        /// Gets current adapter used for drawing.
        /// </summary>
        internal SharpDX.DXGI.Adapter1 Adapter
        {
            get { return m_adapter; }
        }

        public bool IsInitialized
        {
            get
            {
                return (m_factory != null) &&
                       (m_device != null) &&
                       (m_adapter != null);
            }
        }
    }
}
