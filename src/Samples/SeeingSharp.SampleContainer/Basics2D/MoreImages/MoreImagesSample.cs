using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Core;
using SeeingSharp.Drawing2D;
using SeeingSharp.Drawing2D.Resources;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Basics2D.MoreImages
{
    [SampleDescription(
        "More Images", 3, nameof(Basics2D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/src/Samples/SeeingSharp.SampleContainer/Basics2D/MoreImages",
        typeof(ImageSampleSettings))]
    public class MoreImagesSample : SampleBase
    {
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

                // Get all parameters
                var transparency = _castedSettings!.Transparent ? 0.4f : 1f;
                var imageWidth = EngineMath.Clamp(_castedSettings!.ImageWidth, 5, 500);
                var imageHeight = imageWidth;
                var screenWidth = (int)graphics.ScreenWidth;
                var screenHeight = (int)graphics.ScreenHeight;
                var interpolationMode = _castedSettings!.HighQuality
                    ? BitmapInterpolationMode.NearestNeighbor
                    : BitmapInterpolationMode.Linear;

                // Draw all bitmaps
                for (var loopX = 0; loopX < screenWidth / imageWidth + 1; loopX++)
                {
                    for (var loopY = 0; loopY < screenHeight / imageHeight + 1; loopY++)
                    {
                        graphics.DrawBitmap(
                            _bitmap!,
                            new RectangleF(
                                loopX * (imageWidth + 3), loopY * (imageHeight + 3),
                                imageWidth, imageHeight),
                            transparency,
                            interpolationMode);
                    }
                }
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
            public bool Transparent { get; set; } = false;

            [Category("Image")]
            public bool HighQuality { get; set; } = true;

            [Category("Image")]
            public int ImageWidth { get; set; } = 40;
        }
    }
}
