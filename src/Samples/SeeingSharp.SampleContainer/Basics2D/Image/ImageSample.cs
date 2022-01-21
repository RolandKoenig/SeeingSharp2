using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Core;
using SeeingSharp.Drawing2D;
using SeeingSharp.Drawing2D.Resources;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Basics2D.Image
{
    [SampleDescription(
        "Image", 2, nameof(Basics2D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/src/Samples/SeeingSharp.SampleContainer/Basics2D/Image",
        typeof(ImageSampleSettings))]
    public class ImageSample : SampleBase
    {
        private const float IMAGE_WIDTH = 64;
        private const float IMAGE_HEIGHT = 64;

        private ImageSampleSettings? _castedSettings;
        private StandardBitmapResource? _bitmap;

        public override Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            _castedSettings = (ImageSampleSettings)settings;

            _bitmap = new StandardBitmapResource(
                new AssemblyResourceLink(
                    this.GetType(),
                    "SimpleImage.png"));

            return Task.FromResult<object?>(null);
        }

        public override async Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop)
        {
            await mainOrChildRenderLoop.Register2DDrawingLayerAsync(graphics =>
            {
                // Clear the screen
                this.Draw2DBackground(graphics);

                var width = IMAGE_WIDTH * EngineMath.Clamp(_castedSettings!.Scaling, 0f, 100f);
                var height = IMAGE_HEIGHT * EngineMath.Clamp(_castedSettings!.Scaling, 0f, 100f);
                var bitmapRect = new RectangleF(
                    graphics.ScreenWidth / 2f - width / 2f,
                    graphics.ScreenHeight / 2f - height / 2f,
                    width, height);

                graphics.DrawBitmap(
                    _bitmap!,
                    bitmapRect,
                    _castedSettings.Transparent ? 0.5f : 1f,
                    BitmapInterpolationMode.Linear);
            });
        }

        public override void OnSampleClosed()
        {
            base.OnSampleClosed();

            SeeingSharpUtil.SafeDispose(ref _bitmap);
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class ImageSampleSettings : SampleSettings
        {
            [Category("Image")]
            public float Scaling { get; set; } = 5f;

            [Category("Image")]
            public bool Transparent { get; set; } = false;
        }
    }
}
