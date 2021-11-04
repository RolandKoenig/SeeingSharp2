using System.ComponentModel;
using SeeingSharp.Core;

namespace SeeingSharp.SampleContainer
{
    public class SampleSettingsWith3D : SampleSettings
    {
        [Category("3D Rendering")]
        public bool EnableAntialiasing
        {
            get => this.RenderLoop?.Configuration.AntialiasingEnabled ?? false;
            set
            {
                if (this.RenderLoop == null) { return; }

                if (value != this.RenderLoop?.Configuration.AntialiasingEnabled)
                {
                    this.RenderLoop.Configuration.AntialiasingEnabled = value;
                    this.RaisePropertyChanged(nameof(this.EnableAntialiasing));
                }
            }
        }

        [Category("3D Rendering")]
        public bool EnableWireframe
        {
            get => this.RenderLoop?.Configuration.WireframeEnabled ?? false;
            set
            {
                if (this.RenderLoop == null) { return; }

                if (value != this.RenderLoop?.Configuration.WireframeEnabled)
                {
                    this.RenderLoop.Configuration.WireframeEnabled = value;
                    this.RaisePropertyChanged(nameof(this.EnableWireframe));
                }
            }
        }

        [Category("3D Rendering")]
        public AntialiasingQualityLevel AntialiasingQuality
        {
            get => this.RenderLoop?.Configuration.AntialiasingQuality ?? AntialiasingQualityLevel.Medium;
            set
            {
                if (this.RenderLoop == null) { return; }

                if (value != this.RenderLoop?.Configuration.AntialiasingQuality)
                {
                    this.RenderLoop.Configuration.AntialiasingQuality = value;
                    this.RaisePropertyChanged(nameof(this.AntialiasingQuality));
                }
            }
        }
    }
}
