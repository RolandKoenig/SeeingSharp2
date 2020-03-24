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
