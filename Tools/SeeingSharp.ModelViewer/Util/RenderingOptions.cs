using PropertyTools.DataAnnotations;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.ModelViewer.Util
{
    public class RenderingOptions : PropertyChangedBase
    {
        private readonly RenderLoop m_renderLoop;

        public RenderingOptions(RenderLoop renderLoop)
        {
            m_renderLoop = renderLoop;

            this.Reset = new DelegateCommand(() =>
            {
                m_renderLoop.Configuration.Reset();
                this.RaisePropertyChanged(string.Empty);
            });
        }

        [Category("Common")]
        public DelegateCommand Reset { get; }

        [Category("Rendering")]
        public bool Antialiasing
        {
            get => m_renderLoop.Configuration.AntialiasingEnabled;
            set
            {
                m_renderLoop.Configuration.AntialiasingEnabled = value;
                this.RaisePropertyChanged(nameof(this.Antialiasing));
            } 
        }

        [Category("Rendering")]
        public AntialiasingQualityLevel AntialiasingQuality
        {
            get => m_renderLoop.Configuration.AntialiasingQuality;
            set
            {
                if (value != m_renderLoop.Configuration.AntialiasingQuality)
                {
                    m_renderLoop.Configuration.AntialiasingQuality = value;
                    this.RaisePropertyChanged(nameof(this.AntialiasingQuality));
                }
            }
        }

        [Category("Lighting")]
        [Slidable(Minimum = 0, Maximum = 1, LargeChange = 0.05, SmallChange = 0.05, TickFrequency = 0.05)]
        [FormatString("N2")]
        public float AmbientFactor
        {
            get => m_renderLoop.Configuration.AmbientFactor;
            set
            {
                m_renderLoop.Configuration.AmbientFactor = value;
                this.RaisePropertyChanged(nameof(this.AmbientFactor));
            }
        }

        [Category("Lighting")]
        [Slidable(Minimum = 0, Maximum = 1, LargeChange = 0.05, SmallChange = 0.05, TickFrequency = 0.05)]
        [FormatString("N2")]
        public float LightPower
        {
            get => m_renderLoop.Configuration.LightPower;
            set
            {
                m_renderLoop.Configuration.LightPower = value;
                this.RaisePropertyChanged(nameof(this.LightPower));
            }
        }

        [Category("Lighting")]
        [Slidable(Minimum = 0, Maximum = 3, LargeChange = 0.05, SmallChange = 0.05, TickFrequency = 0.05)]
        [FormatString("N2")]
        public float StrongLightFactor
        {
            get => m_renderLoop.Configuration.StrongLightFactor;
            set
            {
                m_renderLoop.Configuration.StrongLightFactor = value;
                this.RaisePropertyChanged(nameof(this.StrongLightFactor));
            }
        }
    }
}
