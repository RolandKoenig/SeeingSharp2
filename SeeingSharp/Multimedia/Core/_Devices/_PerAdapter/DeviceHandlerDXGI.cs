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
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Core
{
    public class DeviceHandlerDXGI
    {
        private Adapter1 m_adapter;
        private Factory2 m_factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceHandlerDXGI"/> class.
        /// </summary>
        internal DeviceHandlerDXGI(EngineHardwareInfo hardwareInfo, EngineAdapterInfo adapterInfo)
        {
            m_adapter = hardwareInfo.DXGIFactory.GetAdapter1(adapterInfo.AdapterIndex);
            m_factory = m_adapter.GetParent<Factory2>();
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        internal void UnloadResources()
        {
            m_factory = SeeingSharpUtil.DisposeObject(m_factory);
            m_adapter = SeeingSharpUtil.DisposeObject(m_adapter);
        }

        public bool IsInitialized =>
            m_factory != null &&
            m_adapter != null;

        /// <summary>
        /// Gets current factory object.
        /// </summary>
        /// <value>The factory.</value>
        internal Factory2 Factory => m_factory;

        /// <summary>
        /// Gets current adapter used for drawing.
        /// </summary>
        internal Adapter1 Adapter => m_adapter;
    }
}
