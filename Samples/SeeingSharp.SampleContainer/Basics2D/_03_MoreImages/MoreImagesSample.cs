/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/

using System.ComponentModel;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Basics2D._03_MoreImages
{
    [SampleDescription(
        "More Images", 3, nameof(Basics2D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Basics2D/_03_MoreImages",
        typeof(ImageSampleSettings))]
    public class MoreImagesSample : SampleBase
    {
        private ImageSampleSettings _castedSettings;

        private StandardBitmapResource _bitmap;

        public override Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            _castedSettings = (ImageSampleSettings)settings;

            _bitmap = new StandardBitmapResource(
                new AssemblyResourceLink(
                    this.GetType(),
                    "SimpleImage.png"));

            return Task.FromResult<object>(null);
        }

        public override async Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop)
        {
            await mainOrChildRenderLoop.Register2DDrawingLayerAsync(graphics =>
            {
                // Clear the screen
                this.Draw2DBackground(graphics);

                // Get all parameters
                var transparency = _castedSettings.Transparent ? 0.4f : 1f;
                var imageWidth = EngineMath.Clamp(_castedSettings.ImageWidth, 5, 500);
                var imageHeight = imageWidth;
                var screenWidth = (int)graphics.ScreenWidth;
                var screenHeight = (int)graphics.ScreenHeight;
                var interpolationMode = _castedSettings.HighQuality
                    ? BitmapInterpolationMode.NearestNeighbor
                    : BitmapInterpolationMode.Linear;

                // Draw all bitmaps
                for (var loopX = 0; loopX < screenWidth / imageWidth + 1; loopX++)
                {
                    for (var loopY = 0; loopY < screenHeight / imageHeight + 1; loopY++)
                    {
                        graphics.DrawBitmap(
                            _bitmap,
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
