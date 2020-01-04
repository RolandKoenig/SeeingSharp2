using System;
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
            yield break;
        }

        public IEnumerable<IInputHandler> CreateInputHandlers()
        {
            yield break;
        }

        public IEnumerable<IModelImporter> CreateModelImporters()
        {
            yield break;
        }

        public IEnumerable<IModelExporter> CreateModelExporters()
        {
            yield return m_modelExporter;
        }
    }
}
