using System;
using System.Collections.Generic;
using SeeingSharp.Core;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Core.Devices;
using SeeingSharp.Core.HardwareInfo;
using SeeingSharp.Input;

namespace SeeingSharp.Drawing3D.ImportExport
{
    public class DefaultImporterExporterExtensions : ISeeingSharpExtensions
    {
        public IEnumerable<IDisposable>? CreateAdditionalDeviceHandlers(EngineDevice device)
        {
            return null;
        }

        public IEnumerable<IInputHandler>? CreateInputHandlers()
        {
            return null;
        }

        public IEnumerable<IModelExporter>? CreateModelExporters()
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
