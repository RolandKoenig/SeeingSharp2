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
using SeeingSharp.Checking;
using System.Collections.Generic;

namespace SeeingSharp.Multimedia.Core
{
    public class DeviceResourceManager
    {
        private EngineDevice _device;
        private List<IEngineDeviceResource> _deviceResources;
        private List<IEngineDeviceResource> _deviceResourcesPrev;
        private bool _cleanupNeeded;

        internal DeviceResourceManager(EngineDevice device)
        {
            _device = device;
            _deviceResources = new List<IEngineDeviceResource>(1024);
            _deviceResourcesPrev = new List<IEngineDeviceResource>(1024);
            _cleanupNeeded = false;
        }

        internal void Cleanup()
        {
            var newResourceList = _deviceResourcesPrev;
            var oldResourceList = _deviceResources;

            var anyNullDiscovered = false;
            for (var loop = 0; loop < oldResourceList.Count; loop++)
            {
                var actOldResource = oldResourceList[loop];
                if (actOldResource == null)
                {
                    anyNullDiscovered = true;
                    continue;
                }

                newResourceList.Add(actOldResource);
                if (anyNullDiscovered)
                {
                    actOldResource.SetDeviceResourceIndex(_device, newResourceList.Count - 1);
                }
            }

            _deviceResources = newResourceList;
            _deviceResourcesPrev = oldResourceList;
            _deviceResourcesPrev.Clear();

            _cleanupNeeded = false;
        }

        internal void UnloadResources()
        {
            for (var loop = 0; loop < _deviceResources.Count; loop++)
            {
                var actDeviceResource = _deviceResources[loop];
                actDeviceResource?.UnloadResources(_device);
            }
        }

        internal void RegisterDeviceResource(IEngineDeviceResource resource)
        {
            var currentDeviceResourceIndex = resource.GetDeviceResourceIndex(_device);
            currentDeviceResourceIndex.EnsureNegativeAndNotZero(nameof(currentDeviceResourceIndex));

            _deviceResources.Add(resource);
            resource.SetDeviceResourceIndex(_device, _deviceResources.Count - 1);
        }

        internal void DeregisterDeviceResource(IEngineDeviceResource resource)
        {
            var currentDeviceResourceIndex = resource.GetDeviceResourceIndex(_device);
            currentDeviceResourceIndex.EnsurePositiveOrZero(nameof(currentDeviceResourceIndex));

            _deviceResources[currentDeviceResourceIndex] = null;
            resource.SetDeviceResourceIndex(_device, -1);

            _cleanupNeeded = true;
        }

        internal bool CleanupNeeded => _cleanupNeeded;
    }
}
