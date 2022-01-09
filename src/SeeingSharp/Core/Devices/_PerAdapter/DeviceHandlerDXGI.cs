using System;
using SeeingSharp.Core.HardwareInfo;
using SeeingSharp.Util;
using Vortice.DXGI;

namespace SeeingSharp.Core.Devices
{
    public class DeviceHandlerDXGI
    {
        private IDXGIAdapter1? _adapter;
        private IDXGIFactory2? _factory;

        public bool IsInitialized =>
            _factory != null &&
            _adapter != null;

        /// <summary>
        /// Gets current factory object.
        /// </summary>
        /// <value>The factory.</value>
        internal IDXGIFactory2 Factory
        {
            get
            {
                if (_factory == null) { throw new ObjectDisposedException(nameof(DeviceHandlerDXGI)); }
                return _factory;
            }
        }

        /// <summary>
        /// Gets current adapter used for drawing.
        /// </summary>
        internal IDXGIAdapter1 Adapter
        {
            get
            {
                if (_adapter == null) { throw new ObjectDisposedException(nameof(DeviceHandlerDXGI)); }
                return _adapter;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceHandlerDXGI"/> class.
        /// </summary>
        internal DeviceHandlerDXGI(EngineFactory factory, EngineAdapterInfo adapterInfo)
        {
            _adapter = factory.DXGI.Factory.GetAdapter1(adapterInfo.AdapterIndex);
            if (_adapter == null)
            {
                throw new SeeingSharpGraphicsException(
                    $"Unable to get DXGI Adapter from device {adapterInfo.AdapterDescription}!");
            }

            _factory = _adapter.GetParent<IDXGIFactory2>();
            if (_factory == null)
            {
                throw new SeeingSharpGraphicsException(
                    $"Unable to get DXGI Factory from adapter {adapterInfo.AdapterDescription}!");
            }
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        internal void UnloadResources()
        {
            _factory = SeeingSharpUtil.DisposeObject(_factory);
            _adapter = SeeingSharpUtil.DisposeObject(_adapter);
        }
    }
}
