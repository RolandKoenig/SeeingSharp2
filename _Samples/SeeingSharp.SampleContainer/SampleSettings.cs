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
    public class SampleSettings : PropertyChangedBase
    {
        private RenderLoop m_renderLoop;
        private SampleMetadata m_sampleMetadata;

        public SampleSettings()
        {

        }

        public virtual IEnumerable<SampleCommand> GetCommands()
        {
            yield return new SampleCommand(
                "Show Source",
                () => PlatformDependentMethods.OpenUrlInBrowser(m_sampleMetadata.SourceCodeUrl),
                () => !string.IsNullOrEmpty(m_sampleMetadata?.SourceCodeUrl));
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

        [Category("Basics")]
        public AntialiasingQualityLevel AntialiasingQuality
        {
            get => m_renderLoop?.ViewConfiguration.AntialiasingQuality ?? AntialiasingQualityLevel.Medium;
            set
            {
                if(value != m_renderLoop?.ViewConfiguration.AntialiasingQuality)
                {
                    m_renderLoop.ViewConfiguration.AntialiasingQuality = value;
                    RaisePropertyChanged(nameof(AntialiasingQuality));
                }
            }
        }
    }
}
