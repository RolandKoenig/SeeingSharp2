using System;
using System.Collections.Generic;
using System.Text;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Input;

namespace SeeingSharp.Multimedia.Objects
{
    public class DefaultImporterExporterExtensions : ISeeingSharpExtensions
    {
        public IEnumerable<IDisposable> CreateAdditionalDeviceHandlers(EngineDevice device)
        {
            yield break;
        }

        public IEnumerable<IInputHandler> CreateInputHandlers()
        {
            yield break;
        }

        public IEnumerable<IModelExporter> CreateModelExporters()
        {
            yield break;
        }

        public IEnumerable<IModelImporter> CreateModelImporters()
        {
            yield return new ACImporter();
            yield return new ObjImporter();
            yield return new StLImporter();
            yield return new XglImporter();
        }
    }
}
