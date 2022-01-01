using System;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;

namespace SeeingSharp.Drawing2D.Resources
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