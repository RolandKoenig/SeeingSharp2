using System.Collections.Generic;
using SeeingSharp.Checking;

namespace SeeingSharp.Core
{
    public class DeviceResourceManager
    {
        private EngineDevice _device;
        private List<IEngineDeviceResource> _deviceResources;
        private List<IEngineDeviceResource> _deviceResourcesPrev;
        private bool _cleanupNeeded;

        internal bool CleanupNeeded => _cleanupNeeded;

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
    }
}
