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
using System;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public abstract class Drawing2DResourceBase : IDisposable, ICheckDisposed, IEngineDeviceResource
    {
        // Helper flags
        private bool m_isDisposed;

        // Device resource handling
        private int[] m_deviceResourceIndices;

        protected Drawing2DResourceBase()
        {
            m_deviceResourceIndices = new int[GraphicsCore.Current.DeviceCount];
            for (var loop = 0; loop < m_deviceResourceIndices.Length; loop++)
            {
                m_deviceResourceIndices[loop] = -1;
            }
        }

        /// <summary>
        /// Disposes this object.
        /// </summary>
        public virtual void Dispose()
        {
            if (!m_isDisposed)
            {
                m_isDisposed = true;
                GraphicsCore.Current.MainLoop.RegisterForUnload(this);
            }
        }

        /// <summary>
        /// Unloads all resources loaded on the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to unload the resource.</param>
        internal abstract void UnloadResources(EngineDevice engineDevice);

        public int GetDeviceResourceIndex(EngineDevice device)
        {
            return m_deviceResourceIndices[device.DeviceIndex];
        }

        public void SetDeviceResourceIndex(EngineDevice device, int resourceIndex)
        {
            m_deviceResourceIndices[device.DeviceIndex] = resourceIndex;
        }

        void IEngineDeviceResource.UnloadResources(EngineDevice device)
        {
            this.UnloadResources(device);
        }

        /// <summary>
        /// Is this object disposed?
        /// </summary>
        public bool IsDisposed => m_isDisposed;


    }
}