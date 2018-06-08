#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using SeeingSharp.Util;

//Some namespace mappings
using DXGI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Core
{
    public class EngineHardwareInfo 
    {
        private DXGI.Factory1 m_dxgiFactory;
        private List<EngineAdapterInfo> m_adapters;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineHardwareInfo" /> class.
        /// </summary>
        internal EngineHardwareInfo()
        {
            this.CreateFactory();
            this.LoadAdapterInformation();
        }

        /// <summary>
        /// Create the DXGI factory object.
        /// </summary>
        private void CreateFactory()
        {
            m_dxgiFactory = SeeingSharpUtil.TryExecute(() => new DXGI.Factory4());
            if (m_dxgiFactory == null) { m_dxgiFactory = SeeingSharpUtil.TryExecute(() => new DXGI.Factory2()); }
            if (m_dxgiFactory == null) { m_dxgiFactory = SeeingSharpUtil.TryExecute(() => new DXGI.Factory1()); }
            if (m_dxgiFactory == null) { throw new SeeingSharpGraphicsException("Unable to create the DXGI Factory object!"); }
        }

        /// <summary>
        /// Loads all adapter information and builds up all needed view models in a background thread.
        /// </summary>
        private void LoadAdapterInformation()
        {
            m_adapters = new List<EngineAdapterInfo>();

            int adapterCount = m_dxgiFactory.GetAdapterCount1();
            for (int loop = 0; loop < adapterCount; loop++)
            {
                try
                {
                    DXGI.Adapter1 actAdapter = m_dxgiFactory.GetAdapter1(loop);
                    m_adapters.Add(new EngineAdapterInfo(loop, actAdapter));
                }
                catch (Exception)
                {
                    //No exception handling needed here
                    // .. adapter information simply can not be gathered
                }
            }
        }

        internal DXGI.Output GetOutputByOutputInfo(EngineOutputInfo outputInfo)
        {
            int adapterCount = m_dxgiFactory.GetAdapterCount1();
            if(outputInfo.AdapterIndex >= adapterCount) { throw new SeeingSharpException($"Unable to find adapter with index {outputInfo.AdapterIndex}!"); }

            using (DXGI.Adapter1 adapter = m_dxgiFactory.GetAdapter1(outputInfo.AdapterIndex))
            {
                int outputCount = adapter.GetOutputCount();
                if(outputInfo.OutputIndex >= outputCount) { throw new SeeingSharpException($"Unable to find output with index {outputInfo.OutputIndex} on adapter {outputInfo.AdapterIndex}!"); }

                return adapter.GetOutput(outputInfo.OutputIndex);
            }
        }

        /// <summary>
        /// Gets a collection containing all adapters.
        /// </summary>
        public List<EngineAdapterInfo> Adapters
        {
            get { return m_adapters; }
        }
    }
}
