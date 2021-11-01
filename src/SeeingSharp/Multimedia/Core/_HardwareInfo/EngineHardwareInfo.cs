using System;
using System.Collections.Generic;
using System.Linq;

namespace SeeingSharp.Multimedia.Core
{
    public class EngineHardwareInfo
    {
        private List<EngineAdapterInfo> _adapters;

        /// <summary>
        /// Gets a collection containing all adapters.
        /// </summary>
        public List<EngineAdapterInfo> Adapters => _adapters;

        public EngineAdapterInfo SoftwareAdapter
        {
            get
            {
                return _adapters.FirstOrDefault(actAdapter => actAdapter.IsSoftwareAdapter);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineHardwareInfo" /> class.
        /// </summary>
        public EngineHardwareInfo(EngineFactory factory)
        {
            this.LoadAdapterInformation(factory);
        }

        /// <summary>
        /// Loads all adapter information and builds up all needed view models in a background thread.
        /// </summary>
        private void LoadAdapterInformation(EngineFactory factory)
        {
            _adapters = new List<EngineAdapterInfo>();

            var adapterCount = factory.DXGI.Factory.GetAdapterCount1();
            for (var loop = 0; loop < adapterCount; loop++)
            {
                try
                {
                    var actAdapter = factory.DXGI.Factory.GetAdapter1(loop);
                    _adapters.Add(new EngineAdapterInfo(loop, actAdapter));
                }
                catch (Exception)
                {
                    //No exception handling needed here
                    // .. adapter information simply can not be gathered
                }
            }
        }
    }
}
