using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeeingSharp
{
    public interface ISeeingSharpExtensions
    {
        IEnumerable<IDisposable> CreateAdditionalDeviceHandlers(EngineDevice device);

        IEnumerable<IInputHandler> CreateInputHandlers();
    }
}
