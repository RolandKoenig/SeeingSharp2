using System;
using System.Collections.Generic;
using System.Text;
using SeeingSharp.Checking;

namespace SeeingSharp.Multimedia.Core._Devices
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
