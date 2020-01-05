using System;
using System.Collections.Generic;
using System.Text;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Input;

namespace SeeingSharp.Multimedia.Core
{
    internal class LoaderModelImporterExtension : ISeeingSharpExtensions
    {
        private IModelImporter m_modelImporter;

        public LoaderModelImporterExtension(IModelImporter modelImporter)
        {
            m_modelImporter = modelImporter;
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
            yield return m_modelImporter;
        }

        public IEnumerable<IModelExporter> CreateModelExporters()
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
