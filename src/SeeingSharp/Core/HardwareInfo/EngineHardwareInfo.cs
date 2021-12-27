using System;
using System.Linq;
using System.Collections.Generic;
using SeeingSharp.Core.Devices;
using SharpGen.Runtime;

namespace SeeingSharp.Core.HardwareInfo
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

            var lastResult = Result.Ok;
            var actIndex = 0;
            do
            {
                lastResult = factory.DXGI.Factory.EnumAdapters1(actIndex, out var actAdapter);
                if(lastResult.Success)
                {
                    _adapters.Add(new EngineAdapterInfo(actIndex, actAdapter));
                }
                actIndex++;
            }
            while (lastResult.Success);
        }
    }
}
