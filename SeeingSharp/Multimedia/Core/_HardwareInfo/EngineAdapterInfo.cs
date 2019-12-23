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
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Core
{
    public class EngineAdapterInfo
    {
        private AdapterDescription m_adapterDescription;
        private D3D.FeatureLevel m_d3d11FeatureLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineAdapterInfo" /> class.
        /// </summary>
        internal EngineAdapterInfo(int adapterIndex, Adapter1 adapter)
        {
            this.Outputs = new List<EngineOutputInfo>();
            this.AdapterIndex = adapterIndex;

            m_adapterDescription = adapter.Description;
            this.IsSoftwareAdapter =
                m_adapterDescription.Description == "Microsoft Basic Render Driver" ||
                !string.IsNullOrEmpty(m_adapterDescription.Description) && m_adapterDescription.Description.Contains("Software") ||
                !string.IsNullOrEmpty(m_adapterDescription.Description) && m_adapterDescription.Description.Contains("Microsoft Basic Render Driver");

            m_d3d11FeatureLevel = D3D11.Device.GetSupportedFeatureLevel(adapter);

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

        /// <summary>
        /// Gets all outputs supported by this adapter.
        /// </summary>
        public List<EngineOutputInfo> Outputs { get; }

        /// <summary>
        /// Gets the index of the adapter.
        /// </summary>
        public int AdapterIndex { get; }

        public string MaxFeatureLevelD3D11 => m_d3d11FeatureLevel.ToString();

        public bool IsSoftwareAdapter { get; }

        /// <summary>
        /// Gets the description of the adapter.
        /// </summary>
        public string AdapterDescription => m_adapterDescription.Description.Replace("\0", "") ?? nameof(EngineAdapterInfo);

        public string DedicatedSystemMemory => m_adapterDescription.DedicatedSystemMemory.ToString();

        public string DedicatedVideoMemory => m_adapterDescription.DedicatedVideoMemory.ToString();

        public string SharedSystemMemory => m_adapterDescription.SharedSystemMemory.ToString();

        public long Luid => m_adapterDescription.Luid;
    }
}
