using System;
using System.Collections.Generic;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Input;

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
