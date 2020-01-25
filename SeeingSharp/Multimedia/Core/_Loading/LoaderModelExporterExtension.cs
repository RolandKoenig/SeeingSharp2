﻿using System;
using System.Collections.Generic;
using System.Text;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Input;

namespace SeeingSharp.Multimedia.Core
{
    internal class LoaderModelExporterExtension : ISeeingSharpExtensions
    {
        private IModelExporter m_modelExporter;

        public LoaderModelExporterExtension(IModelExporter modelExporter)
        {
            m_modelExporter = modelExporter;
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
            yield return m_modelExporter;
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