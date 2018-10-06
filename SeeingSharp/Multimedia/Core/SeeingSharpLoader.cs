using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Checking;

namespace SeeingSharp.Multimedia.Core
{
    public class SeeingSharpLoader
    {
        private List<ISeeingSharpExtensions> m_extensions;

        internal SeeingSharpLoader()
        {
            m_extensions = new List<ISeeingSharpExtensions>();
            this.LoadSettings = new DeviceLoadSettings();
        }

        public void RegisterExtensions(ISeeingSharpExtensions extensions)
        {
            extensions.EnsureNotNull(nameof(extensions));

            m_extensions.Add(extensions);
        }

        public void Load()
        {
            GraphicsCore.Load(this);
        }

        public Task LoadAsync()
        {
            return Task.Factory.StartNew(() => GraphicsCore.Load(this));
        }

        public SeeingSharpLoader Configure(DeviceLoadSettings loadSettings)
        {
            loadSettings.EnsureNotNull(nameof(loadSettings));
            this.LoadSettings = loadSettings;

            return this;
        }

        public SeeingSharpLoader Configure(Action<DeviceLoadSettings> manipulateConfigAction)
        {
            manipulateConfigAction.EnsureNotNull(nameof(manipulateConfigAction));

            manipulateConfigAction(this.LoadSettings);
            return this;
        }

        public SeeingSharpLoader EnableDirectXDebugMode()
        {
            return this.Configure(
                loadSettings => loadSettings.DebugEnabled = true);
        }

        public IEnumerable<ISeeingSharpExtensions> Extensions
        {
            get => m_extensions;
        }

        public DeviceLoadSettings LoadSettings
        {
            get;
            set;
        }
    }
}
