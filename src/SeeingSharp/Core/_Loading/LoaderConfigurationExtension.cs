using System;
using System.Collections.Generic;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Drawing3D;
using SeeingSharp.Input;

namespace SeeingSharp.Core
{
    internal class LoaderConfigurationExtension : ISeeingSharpExtensions
    {
        private Action<GraphicsCoreConfiguration> _manipulateCoreConfig;
        private Action<EngineAdapterInfo, GraphicsDeviceConfiguration> _manipulateDeviceConfig;
        private Action<RenderLoop, GraphicsViewConfiguration> _manipulateViewConfig;

        public LoaderConfigurationExtension(
            Action<GraphicsCoreConfiguration> manipulateCoreConfig,
            Action<EngineAdapterInfo, GraphicsDeviceConfiguration> manipulateDeviceConfig,
            Action<RenderLoop, GraphicsViewConfiguration> manipulateViewConfig)
        {
            _manipulateCoreConfig = manipulateCoreConfig;
            _manipulateDeviceConfig = manipulateDeviceConfig;
            _manipulateViewConfig = manipulateViewConfig;
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
            return null;
        }

        public void EditCoreConfiguration(GraphicsCoreConfiguration coreConfig)
        {
            _manipulateCoreConfig?.Invoke(coreConfig);
        }

        public void EditDeviceConfiguration(EngineAdapterInfo adapterInfo, GraphicsDeviceConfiguration deviceConfig)
        {
            _manipulateDeviceConfig?.Invoke(adapterInfo, deviceConfig);
        }

        public void EditViewConfiguration(RenderLoop renderLoop, GraphicsViewConfiguration viewConfig)
        {
            _manipulateViewConfig?.Invoke(renderLoop, viewConfig);
        }
    }
}
