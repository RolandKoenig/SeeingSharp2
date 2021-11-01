using SeeingSharp.Util;
using SharpDX.DXGI;

namespace SeeingSharp.Multimedia.Core
{
    public class DeviceHandlerDXGI
    {
        private Adapter1 _adapter;
        private Factory2 _factory;

        public bool IsInitialized =>
            _factory != null &&
            _adapter != null;

        /// <summary>
        /// Gets current factory object.
        /// </summary>
        /// <value>The factory.</value>
        internal Factory2 Factory => _factory;

        /// <summary>
        /// Gets current adapter used for drawing.
        /// </summary>
        internal Adapter1 Adapter => _adapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceHandlerDXGI"/> class.
        /// </summary>
        internal DeviceHandlerDXGI(EngineFactory factory, EngineAdapterInfo adapterInfo)
        {
            _adapter = factory.DXGI.Factory.GetAdapter1(adapterInfo.AdapterIndex);
            _factory = _adapter.GetParent<Factory2>();
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
