#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion

#region using

using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;

#endregion

namespace SeeingSharp.Multimedia.Core
{
    #region using

    using System;
    using System.Collections.Generic;
    using SeeingSharp.Util;

    #endregion

    public class EngineAdapterInfo : IDisposable, ICheckDisposed
    {
        private const string TRANSLATABLE_GROUP_COMMON_HARDWARE_INFO = "Common hardware information";

        private List<EngineOutputInfo> m_outputs;
        private SharpDX.DXGI.Adapter1 m_adapter;
        private int m_adapterIndex;
        private bool m_isSoftware;
        private D3D.FeatureLevel m_d3d11FeatureLevel;
        private SharpDX.DXGI.AdapterDescription m_adapterDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineAdapterInfo" /> class.
        /// </summary>
        internal EngineAdapterInfo(int adapterIndex, SharpDX.DXGI.Adapter1 adapter)
        {
            m_outputs = new List<EngineOutputInfo>();
            m_adapter = adapter;
            m_adapterIndex = adapterIndex;

            m_adapterDescription = adapter.Description;
            m_isSoftware =
                (m_adapterDescription.Description == "Microsoft Basic Render Driver") ||
                ((!string.IsNullOrEmpty(m_adapterDescription.Description)) && m_adapterDescription.Description.Contains("Software")) ||
                ((!string.IsNullOrEmpty(m_adapterDescription.Description)) && m_adapterDescription.Description.Contains("Microsoft Basic Render Driver"));

            m_d3d11FeatureLevel = D3D11.Device.GetSupportedFeatureLevel(adapter);

            //Query for output information
            SharpDX.DXGI.Output[] outputs = adapter.Outputs;
            for (int loop = 0; loop < outputs.Length; loop++)
            {
                try
                {
                    var actOutput = outputs[loop];

                    try
                    {
                        m_outputs.Add(new EngineOutputInfo(adapterIndex, loop, actOutput));
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

        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref m_adapter);
            m_outputs.Clear();
        }

        /// <summary>
        /// Gets all outputs supported by this adapter.
        /// </summary>
        public List<EngineOutputInfo> Outputs
        {
            get { return m_outputs; }
        }

        /// <summary>
        /// Gets the corresponding adapter.
        /// </summary>
        internal SharpDX.DXGI.Adapter1 Adapter
        {
            get { return m_adapter; }
        }

        /// <summary>
        /// Gets the index of the adapter.
        /// </summary>
        public int AdapterIndex
        {
            get { return m_adapterIndex; }
        }

        public string MaxFeatureLevelD3D11
        {
            get { return m_d3d11FeatureLevel.ToString(); }
        }

        public bool IsSoftwareAdapter
        {
            get { return m_isSoftware; }
        }

        /// <summary>
        /// Gets the description of the adapter.
        /// </summary>
        public string AdapterDescription
        {
            get { return m_adapterDescription.Description; }
        }

        public string DedicatedSystemMemory
        {
            get { return m_adapterDescription.DedicatedSystemMemory.ToString(); }
        }

        public string DedicatedVideoMemory
        {
            get { return m_adapterDescription.DedicatedVideoMemory.ToString(); }
        }

        public string SharedSystemMemory
        {
            get { return m_adapterDescription.SharedSystemMemory.ToString(); }
        }

        public bool IsDisposed => m_adapter == null;
    }
}
