using System;
using System.Collections.Generic;
using SeeingSharp.Core;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Core.Devices;
using SeeingSharp.Core.HardwareInfo;
using SeeingSharp.Drawing3D.ImportExport;
using SeeingSharp.Input;

namespace SeeingSharp
{
    internal class SeeingSharpUwpExtensions : ISeeingSharpExtensions
    {
        public IEnumerable<IInputHandler> CreateInputHandlers()
        {
            yield return new WinRTKeyAndMouseInputHandler();
        }

        public IEnumerable<IDisposable> CreateAdditionalDeviceHandlers(EngineDevice device)
        {
            return null;
        }

        public IEnumerable<IModelImporter> CreateModelImporters()
        {
            return null;
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