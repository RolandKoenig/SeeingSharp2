#region License information
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

    public class EngineHardwareInfo : IDisposable, ICheckDisposed
    {
        private SharpDX.DXGI.Factory1 m_dxgiFactory;
        private List<EngineAdapterInfo> m_adapters;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineHardwareInfo" /> class.
        /// </summary>
        public EngineHardwareInfo()
        {
            this.CreateFactory();
            this.LoadAdapterInformation();
        }

        /// <summary>
        /// Create the DXGI factory object.
        /// </summary>
        private void CreateFactory()
        {
            m_dxgiFactory = SeeingSharpUtil.TryExecute(() => new SharpDX.DXGI.Factory4());
            if (m_dxgiFactory == null) { m_dxgiFactory = SeeingSharpUtil.TryExecute(() => new SharpDX.DXGI.Factory2()); }
            if (m_dxgiFactory == null) { m_dxgiFactory = SeeingSharpUtil.TryExecute(() => new SharpDX.DXGI.Factory1()); }
            if (m_dxgiFactory == null) { throw new SeeingSharpGraphicsException("Unable to create the DXGI Factory object!"); }
        }

        /// <summary>
        /// Loads all adapter information and builds up all needed view models in a background thread.
        /// </summary>
        private void LoadAdapterInformation()
        {
            m_adapters = new List<EngineAdapterInfo>();

            var adapterCount = m_dxgiFactory.GetAdapterCount1();

            for (var loop = 0; loop < adapterCount; loop++)
            {
                try
                {
                    var actAdapter = m_dxgiFactory.GetAdapter1(loop);
                    m_adapters.Add(new EngineAdapterInfo(loop, actAdapter));
                }
                catch (Exception)
                {
                    //No exception handling needed here
                    // .. adapter information simply can not be gathered
                }
            }
        }

        internal SharpDX.DXGI.Output GetOutputByOutputInfo(EngineOutputInfo outputInfo)
        {
            int adapterCount = m_dxgiFactory.GetAdapterCount1();

            if (outputInfo.AdapterIndex >= adapterCount)
            {
                throw new SeeingSharpException($"Unable to find adapter with index {outputInfo.AdapterIndex}!");
            }

            using (var adapter = m_dxgiFactory.GetAdapter1(outputInfo.AdapterIndex))
            {
                int outputCount = adapter.GetOutputCount();

                if (outputInfo.OutputIndex >= outputCount)
                {
                    throw new SeeingSharpException($"Unable to find output with index {outputInfo.OutputIndex} on adapter {outputInfo.AdapterIndex}!");
                }

                return adapter.GetOutput(outputInfo.OutputIndex);
            }
        }

        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref m_dxgiFactory);

            foreach(var actAdapter in m_adapters)
            {
                SeeingSharpUtil.DisposeObject(actAdapter);
            }

            m_adapters.Clear();
        }

        /// <summary>
        /// Gets a collection containing all adapters.
        /// </summary>
        public List<EngineAdapterInfo> Adapters
        {
            get { return m_adapters; }
        }

        public EngineAdapterInfo SoftwareAdapter
        {
            get
            {
                foreach(var actAdapter in m_adapters)
                {
                    if (actAdapter.IsSoftwareAdapter) { return actAdapter; }
                }
                return null;
            }
        }

        public bool IsDisposed => m_dxgiFactory == null;
    }
}
