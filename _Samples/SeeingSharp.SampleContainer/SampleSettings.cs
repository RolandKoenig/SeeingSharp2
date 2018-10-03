using SeeingSharp.Multimedia.Core;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SeeingSharp.SampleContainer
{
    public class SampleSettings : INotifyPropertyChanged
    {
        private RenderLoop m_renderLoop;
        private SampleMetadata m_sampleMetadata;

        public event PropertyChangedEventHandler PropertyChanged;

        public SampleSettings()
        {
            this.Command_ShowSource = new DelegateCommand(
                () => Process.Start(m_sampleMetadata.SourceCodeUrl),
                () => !string.IsNullOrEmpty(m_sampleMetadata?.SourceCodeUrl));
        }

        protected void RaisePropertyChanged([CallerMemberName] string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public void SetEnvironment(RenderLoop renderLoop, SampleMetadata sampleMetadata)
        {
            m_renderLoop = renderLoop;
            m_sampleMetadata = sampleMetadata;

            // Trigger refresh of all properties
            RaisePropertyChanged();
        }

        [Category("Basics")]
        public bool EnableAntialiasing
        {
            get => m_renderLoop?.ViewConfiguration.AntialiasingEnabled ?? false;
            set
            {
                if(value != m_renderLoop?.ViewConfiguration.AntialiasingEnabled)
                {
                    m_renderLoop.ViewConfiguration.AntialiasingEnabled = value;
                    RaisePropertyChanged(nameof(EnableAntialiasing));
                }
            }
        }

        [Category("Basics")]
        public bool EnableWireframe
        {
            get => m_renderLoop?.ViewConfiguration.WireframeEnabled ?? false;
            set
            {
                if (value != m_renderLoop?.ViewConfiguration.WireframeEnabled)
                {
                    m_renderLoop.ViewConfiguration.WireframeEnabled = value;
                    RaisePropertyChanged(nameof(EnableWireframe));
                }
            }
        }

        [DisplayName("Show sourcecode")]
        [Category("Basics")]
        public ICommand Command_ShowSource
        {
            get;
            private set;
        }
    }
}
