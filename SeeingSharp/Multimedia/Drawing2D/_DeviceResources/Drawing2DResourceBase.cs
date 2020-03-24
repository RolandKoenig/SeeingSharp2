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
        private bool _isDisposed;

        // Device resource handling
        private int[] _deviceResourceIndices;

        /// <summary>
        /// Is this object disposed?
        /// </summary>
        public bool IsDisposed => _isDisposed;

        protected Drawing2DResourceBase()
        {
            _deviceResourceIndices = new int[GraphicsCore.Current.DeviceCount];
            for (var loop = 0; loop < _deviceResourceIndices.Length; loop++)
            {
                _deviceResourceIndices[loop] = -1;
            }
        }

        /// <summary>
        /// Disposes this object.
        /// </summary>
        public virtual void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                GraphicsCore.Current.MainLoop.RegisterForUnload(this);
            }
        }

        public int GetDeviceResourceIndex(EngineDevice device)
        {
            return _deviceResourceIndices[device.DeviceIndex];
        }

        public void SetDeviceResourceIndex(EngineDevice device, int resourceIndex)
        {
            _deviceResourceIndices[device.DeviceIndex] = resourceIndex;
        }

        /// <summary>
        /// Unloads all resources loaded on the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to unload the resource.</param>
        internal abstract void UnloadResources(EngineDevice engineDevice);

        void IEngineDeviceResource.UnloadResources(EngineDevice device)
        {
            this.UnloadResources(device);
        }
    }
}