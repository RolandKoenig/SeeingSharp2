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
using System.Collections.Generic;
using System.Text;
using SeeingSharp.Checking;

namespace SeeingSharp.Multimedia.Core
{
    public class DeviceResourceManager
    {
        private EngineDevice m_device;
        private List<IEngineDeviceResource> m_deviceResources;
        private List<IEngineDeviceResource> m_deviceResourcesPrev;
        private bool m_cleanupNeeded;

        internal DeviceResourceManager(EngineDevice device)
        {
            m_device = device;
            m_deviceResources = new List<IEngineDeviceResource>(1024);
            m_deviceResourcesPrev = new List<IEngineDeviceResource>(1024);
            m_cleanupNeeded = false;
        }

        internal void Cleanup()
        {
            var newResourceList = m_deviceResourcesPrev;
            var oldResourceList = m_deviceResources;

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
                    actOldResource.SetDeviceResourceIndex(m_device, newResourceList.Count - 1);
                }
            }

            m_deviceResources = newResourceList;
            m_deviceResourcesPrev = oldResourceList;
            m_deviceResourcesPrev.Clear();

            m_cleanupNeeded = false;
        }

        internal void UnloadResources()
        {
            for (var loop = 0; loop < m_deviceResources.Count; loop++)
            {
                var actDeviceResource = m_deviceResources[loop];
                actDeviceResource?.UnloadResources(m_device);
            }
        }

        internal void RegisterDeviceResource(IEngineDeviceResource resource)
        {
            var currentDeviceResourceIndex = resource.GetDeviceResourceIndex(m_device);
            currentDeviceResourceIndex.EnsureNegativeAndNotZero(nameof(currentDeviceResourceIndex));

            m_deviceResources.Add(resource);
            resource.SetDeviceResourceIndex(m_device, m_deviceResources.Count -1);
        }

        internal void DeregisterDeviceResource(IEngineDeviceResource resource)
        {
            var currentDeviceResourceIndex = resource.GetDeviceResourceIndex(m_device);
            currentDeviceResourceIndex.EnsurePositiveOrZero(nameof(currentDeviceResourceIndex));

            m_deviceResources[currentDeviceResourceIndex] = null;
            resource.SetDeviceResourceIndex(m_device, -1);

            m_cleanupNeeded = true;
        }

        internal bool CleanupNeeded => m_cleanupNeeded;
    }
}
