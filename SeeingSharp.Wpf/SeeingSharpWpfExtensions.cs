using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Input;

namespace SeeingSharp
{
    internal class SeeingSharpWpfExtensions : ISeeingSharpExtensions
    {
        public IEnumerable<IInputHandler> CreateInputHandlers()
        {
            yield return new WpfKeyAndMouseInputHandler();
        }

        public IEnumerable<IDisposable> CreateAdditionalDeviceHandlers(EngineDevice device)
        {
            yield return new DeviceHandlerD3D9(
                device.Internals.Adapter,
                device.IsSoftware,
                device.DebugEnabled);
        }
    }
}
