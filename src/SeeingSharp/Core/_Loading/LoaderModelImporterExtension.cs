using System;
using System.Collections.Generic;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Core.Devices;
using SeeingSharp.Core.HardwareInfo;
using SeeingSharp.Drawing3D.ImportExport;
using SeeingSharp.Input;

namespace SeeingSharp.Core
{
    internal class LoaderModelImporterExtension : ISeeingSharpExtensions
    {
        private IModelImporter _modelImporter;

        public LoaderModelImporterExtension(IModelImporter modelImporter)
        {
            _modelImporter = modelImporter;
        }

        public IEnumerable<IDisposable>? CreateAdditionalDeviceHandlers(EngineDevice device)
        {
            return null;
        }

        public IEnumerable<IInputHandler>? CreateInputHandlers()
        {
            return null;
        }

        public IEnumerable<IModelImporter> CreateModelImporters()
        {
            yield return _modelImporter;
        }

        public IEnumerable<IModelExporter>? CreateModelExporters()
        {
            return null;
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
