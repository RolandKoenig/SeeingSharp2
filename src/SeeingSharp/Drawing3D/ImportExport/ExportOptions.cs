using SeeingSharp.Checking;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;

namespace SeeingSharp.Drawing3D.ImportExport
{
    public abstract class ExportOptions
    {
        private EngineDevice _device;

        public EngineDevice ExportDevice
        {
            get => _device;
            set
            {
                value.EnsureNotNull(nameof(value));
                _device = value;
            }
        }

        protected ExportOptions()
        {
            _device = GraphicsCore.Current.DefaultDevice;

        }
    }
}
