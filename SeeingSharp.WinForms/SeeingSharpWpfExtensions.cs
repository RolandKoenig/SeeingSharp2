using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Input;

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
    }
}
