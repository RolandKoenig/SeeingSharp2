using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Input;
using SeeingSharp.Multimedia.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeeingSharp
{
    public interface ISeeingSharpExtensions
    {
        IEnumerable<IDisposable> CreateAdditionalDeviceHandlers(EngineDevice device);

        IEnumerable<IInputHandler> CreateInputHandlers();

        IEnumerable<IModelImporter> CreateModelImporters();

        IEnumerable<IModelExporter> CreateModelExporters();
    }
}
