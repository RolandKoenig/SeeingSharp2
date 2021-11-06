using System;
using System.Collections.Generic;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Drawing3D;
using SeeingSharp.Input;

namespace SeeingSharp.Core
{
    public interface ISeeingSharpExtensions
    {
        IEnumerable<IDisposable> CreateAdditionalDeviceHandlers(EngineDevice device);

        IEnumerable<IInputHandler> CreateInputHandlers();

        IEnumerable<IModelImporter> CreateModelImporters();

        IEnumerable<IModelExporter> CreateModelExporters();

        /// <summary>
        /// Change some settings before loading the GraphicsCore object.
        /// </summary>
        /// <param name="coreConfig">Current core configuration.</param>
        void EditCoreConfiguration(GraphicsCoreConfiguration coreConfig);

        /// <summary>
        /// Change some settings before loading a graphics device the first time.
        /// </summary>
        /// <param name="adapterInfo">The adapter which gets loaded currently.</param>
        /// <param name="deviceConfig">Current device configuration.</param>
        void EditDeviceConfiguration(EngineAdapterInfo adapterInfo, GraphicsDeviceConfiguration deviceConfig);

        /// <summary>
        /// Change some settings before loading a view the first time.
        /// </summary>
        /// <param name="renderLoop">The view which is loaded currently.</param>
        /// <param name="viewConfig">Current view configuration.</param>
        void EditViewConfiguration(RenderLoop renderLoop, GraphicsViewConfiguration viewConfig);
    }
}