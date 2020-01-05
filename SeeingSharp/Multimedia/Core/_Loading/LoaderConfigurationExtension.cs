using System;
using System.Collections.Generic;
using System.Text;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Input;

namespace SeeingSharp.Multimedia.Core
{
    internal class LoaderConfigurationExtension : ISeeingSharpExtensions
    {
        private Action<GraphicsCoreConfiguration> m_manipulateCoreConfig;
        private Action<EngineAdapterInfo, GraphicsDeviceConfiguration> m_manipulateDeviceConfig;
        private Action<RenderLoop, GraphicsViewConfiguration> m_manipulateViewConfig;

        public LoaderConfigurationExtension(
            Action<GraphicsCoreConfiguration> manipulateCoreConfig,
            Action<EngineAdapterInfo, GraphicsDeviceConfiguration> manipulateDeviceConfig,
            Action<RenderLoop, GraphicsViewConfiguration> manipulateViewConfig)
        {
            m_manipulateCoreConfig = manipulateCoreConfig;
            m_manipulateDeviceConfig = manipulateDeviceConfig;
            m_manipulateViewConfig = manipulateViewConfig;
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
            m_manipulateCoreConfig?.Invoke(coreConfig);
        }

        public void EditDeviceConfiguration(EngineAdapterInfo adapterInfo, GraphicsDeviceConfiguration deviceConfig)
        {
            m_manipulateDeviceConfig?.Invoke(adapterInfo, deviceConfig);
        }

        public void EditViewConfiguration(RenderLoop renderLoop, GraphicsViewConfiguration viewConfig)
        {
            m_manipulateViewConfig?.Invoke(renderLoop, viewConfig);
        }
    }
}
