using System;
using System.Linq;
using System.Collections.Generic;
using SeeingSharp.Core.Devices;
using SharpGen.Runtime;

namespace SeeingSharp.Core.HardwareInfo
{
    public class EngineHardwareInfo
    {
        /// <summary>
        /// Gets a collection containing all adapters.
        /// </summary>
        public List<EngineAdapterInfo> Adapters { get; }

        public EngineAdapterInfo? SoftwareAdapter
        {
            get
            {
                return this.Adapters.FirstOrDefault(actAdapter => actAdapter.IsSoftwareAdapter);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineHardwareInfo" /> class.
        /// </summary>
        public EngineHardwareInfo(EngineFactory factory)
        {
            this.Adapters = new List<EngineAdapterInfo>();

            var lastResult = Result.Ok;
            var actIndex = 0;
            do
            {
                lastResult = factory.DXGI.Factory.EnumAdapters1(actIndex, out var actAdapter);
                if(lastResult.Success)
                {
                    this.Adapters.Add(new EngineAdapterInfo(actIndex, actAdapter));
                }
                actIndex++;
            }
            while (lastResult.Success);
        }
    }
}
