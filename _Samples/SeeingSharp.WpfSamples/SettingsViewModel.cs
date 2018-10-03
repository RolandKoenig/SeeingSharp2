using SeeingSharp.Multimedia.Core;
using SeeingSharp.SampleContainer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SeeingSharp.WpfSamples
{
    public class SettingsViewModel : ViewModelBase
    {
        private RenderLoop m_renderLoop;
        private SampleMetadata m_sampleMetadata;

        public SettingsViewModel(RenderLoop renderLoop, SampleMetadata sampleMetadata)
        {
            m_renderLoop = renderLoop;
            m_sampleMetadata = sampleMetadata;

            this.Command_ShowSource = new PropertyTools.Wpf.DelegateCommand(
                () => Process.Start(sampleMetadata.SourceCodeUrl),
                () => !string.IsNullOrEmpty(sampleMetadata.SourceCodeUrl));
        }

        [DisplayName("Show sourcecode")]
        [Category("Basics")]
        public ICommand Command_ShowSource
        {
            get;
            private set;
        }

        [Category("Basics")]
        public bool EnableAntialiasing
        {
            get => m_renderLoop.ViewConfiguration.AntialiasingEnabled;
            set
            {
                if(value != m_renderLoop.ViewConfiguration.AntialiasingEnabled)
                {
                    m_renderLoop.ViewConfiguration.AntialiasingEnabled = value;
                    RaisePropertyChanged(nameof(EnableAntialiasing));
                }
            }
        }

        [Category("Basics")]
        public bool EnableWireframe
        {
            get => m_renderLoop.ViewConfiguration.WireframeEnabled;
            set
            {
                if (value != m_renderLoop.ViewConfiguration.WireframeEnabled)
                {
                    m_renderLoop.ViewConfiguration.WireframeEnabled = value;
                    RaisePropertyChanged(nameof(EnableWireframe));
                }
            }
        }
    }
}
