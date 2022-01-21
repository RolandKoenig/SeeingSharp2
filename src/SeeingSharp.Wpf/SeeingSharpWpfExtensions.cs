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
    internal class SeeingSharpWpfExtensions : ISeeingSharpExtensions
    {
        public IEnumerable<IInputHandler>? CreateInputHandlers()
        {
            yield return new WpfKeyAndMouseInputHandler();
        }

        public IEnumerable<IDisposable>? CreateAdditionalDeviceHandlers(EngineDevice device)
        {
            yield return new DeviceHandlerD3D9(
                device.Internals.Adapter!,
                device.IsSoftware);
        }

        public IEnumerable<IModelImporter>? CreateModelImporters()
        {
            return null;
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