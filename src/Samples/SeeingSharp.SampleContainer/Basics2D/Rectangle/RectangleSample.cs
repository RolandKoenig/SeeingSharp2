using System.ComponentModel;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Core;
using SeeingSharp.Drawing2D;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Basics2D.Rectangle
{
    [SampleDescription(
        "Rectangle", 1, nameof(Basics2D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Basics2D/Rectangle",
        typeof(RectangleSampleSettings))]
    public class RectangleSample : SampleBase
    {
        private RectangleSampleSettings _castedSettings;

        private SolidBrushResource _fillBrush;
        private SolidBrushResource _fillBrushTransparent;

        public override Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            _castedSettings = (RectangleSampleSettings)settings;

            _fillBrush = new SolidBrushResource(Color4.Gray);
            _fillBrushTransparent = new SolidBrushResource(Color4.Gray, 0.5f);

            return Task.FromResult<object>(null);
        }

        public override async Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop)
        {
            await mainOrChildRenderLoop.Register2DDrawingLayerAsync(graphics =>
            {
                // Clear the screen
                this.Draw2DBackground(graphics);

                // Calculate rectangle location
                var width = _castedSettings.Width;
                var height = _castedSettings.Height;
                var rectToDraw = new RectangleF(
                    graphics.ScreenWidth / 2f - width / 2f,
                    graphics.ScreenHeight / 2f - height / 2f,
                    width, height);

                // Draw the rectangle
                if (_castedSettings.Rounded)
                {
                    graphics.FillRoundedRectangle(
                        rectToDraw,
                        _castedSettings.RoundedRadius, _castedSettings.RoundedRadius,
                        _castedSettings.Transparent ? _fillBrushTransparent : _fillBrush);
                }
                else
                {
                    graphics.FillRectangle(
                        rectToDraw,
                        _castedSettings.Transparent ? _fillBrushTransparent : _fillBrush);
                }
            });
        }

        public override void OnSampleClosed()
        {
            base.OnSampleClosed();

            SeeingSharpUtil.SafeDispose(ref _fillBrush);
            SeeingSharpUtil.SafeDispose(ref _fillBrushTransparent);
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class RectangleSampleSettings : SampleSettings
        {
            [Category("Rectangle")]
            public bool Rounded { get; set; } = true;

            [Category("Rectangle")]
            public float RoundedRadius { get; set; } = 25f;

            [Category("Rectangle")]
            public float Width { get; set; } = 256f;

            [Category("Rectangle")]
            public float Height { get; set; } = 256f;

            [Category("Rectangle")]
            public bool Transparent { get; set; } = false;
        }
    }
}
