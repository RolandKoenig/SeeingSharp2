using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.Multimedia.Drawing3D
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
