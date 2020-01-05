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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Input;
using System;
using System.Collections.Generic;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class DefaultImporterExporterExtensions : ISeeingSharpExtensions
    {
        public IEnumerable<IDisposable> CreateAdditionalDeviceHandlers(EngineDevice device)
        {
            return null;
        }

        public IEnumerable<IInputHandler> CreateInputHandlers()
        {
            return null;
        }

        public IEnumerable<IModelExporter> CreateModelExporters()
        {
            return null;
        }

        public IEnumerable<IModelImporter> CreateModelImporters()
        {
            yield return new ACImporter();
            yield return new StLImporter();
        }

        public void EditCoreConfiguration(GraphicsCoreConfiguration coreConfig)
        {
     
        }

        public void EditDeviceConfiguration(EngineAdapterInfo adapterInfo, GraphicsDeviceConfiguration deviceConfig)
        {

        }

        public void EditViewConfiguration(RenderLoop renderLoop, GraphicsViewConfiguration viewConfig)
        {

        }
    }
}
