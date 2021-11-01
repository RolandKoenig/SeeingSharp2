using System;
using System.Collections.Generic;
using SharpDX.DXGI;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Core
{
    public class EngineAdapterInfo
    {
        private AdapterDescription _adapterDescription;
        private D3D.FeatureLevel _d3d11FeatureLevel;

        /// <summary>
        /// Gets all outputs supported by this adapter.
        /// </summary>
        public List<EngineOutputInfo> Outputs { get; }

        /// <summary>
        /// Gets the index of the adapter.
        /// </summary>
        public int AdapterIndex { get; }

        public string MaxFeatureLevelD3D11 => _d3d11FeatureLevel.ToString();

        public bool IsSoftwareAdapter { get; }

        /// <summary>
        /// Gets the description of the adapter.
        /// </summary>
        public string AdapterDescription => _adapterDescription.Description.Replace("\0", "");

        public string DedicatedSystemMemory => _adapterDescription.DedicatedSystemMemory.ToString();

        public string DedicatedVideoMemory => _adapterDescription.DedicatedVideoMemory.ToString();

        public string SharedSystemMemory => _adapterDescription.SharedSystemMemory.ToString();

        public long Luid => _adapterDescription.Luid;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineAdapterInfo" /> class.
        /// </summary>
        internal EngineAdapterInfo(int adapterIndex, Adapter1 adapter)
        {
            this.Outputs = new List<EngineOutputInfo>();
            this.AdapterIndex = adapterIndex;

            _adapterDescription = adapter.Description;
            this.IsSoftwareAdapter =
                _adapterDescription.Description == "Microsoft Basic Render Driver" ||
                !string.IsNullOrEmpty(_adapterDescription.Description) && _adapterDescription.Description.Contains("Software") ||
                !string.IsNullOrEmpty(_adapterDescription.Description) && _adapterDescription.Description.Contains("Microsoft Basic Render Driver");

            _d3d11FeatureLevel = D3D11.Device.GetSupportedFeatureLevel(adapter);

            //Query for output information
            var outputs = adapter.Outputs;
            for (var loop = 0; loop < outputs.Length; loop++)
            {
                try
                {
                    var actOutput = outputs[loop];

                    try
                    {
                        this.Outputs.Add(new EngineOutputInfo(adapterIndex, loop, actOutput));
                    }
                    finally
                    {
                        actOutput.Dispose();
                    }
                }
                catch (Exception)
                {
                    //Query for output information not possible
                    // .. no special handling needed here
                }
            }
        }
    }
}
