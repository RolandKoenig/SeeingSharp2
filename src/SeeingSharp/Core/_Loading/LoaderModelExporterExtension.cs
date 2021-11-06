using System;
using System.Collections.Generic;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Drawing3D;
using SeeingSharp.Input;

namespace SeeingSharp.Core
{
    internal class LoaderModelExporterExtension : ISeeingSharpExtensions
    {
        private IModelExporter _modelExporter;

        public LoaderModelExporterExtension(IModelExporter modelExporter)
        {
            _modelExporter = modelExporter;
        }

        public IEnumerable<IDisposable> CreateAdditionalDeviceHandlers(EngineDevice device)
        {
            return null;
        }

        public IEnumerable<IInputHandler> CreateInputHandlers()
        {
            return null;
        }

        public IEnumerable<IModelImporter> CreateModelImporters()
        {
            return null;
        }

        public IEnumerable<IModelExporter> CreateModelExporters()
        {
            yield return _modelExporter;
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
