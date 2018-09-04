using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Input;
using SeeingSharp.Multimedia.Objects;

namespace SeeingSharp
{
    internal class SeeingSharpWinFormsExtensions : ISeeingSharpExtensions
    {
        public IEnumerable<IInputHandler> CreateInputHandlers()
        {
            yield return new WinFormsKeyAndMouseInputHandler();
        }

        public IEnumerable<IDisposable> CreateAdditionalDeviceHandlers(EngineDevice device)
        {
            yield break;
        }

        public IEnumerable<IModelImporter> CreateModelImporters()
        {
            yield break;
        }

        public IEnumerable<IModelExporter> CreateModelExporters()
        {
            yield break;
        }
    }
}
