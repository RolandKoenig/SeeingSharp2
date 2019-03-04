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
using SharpDX;

namespace SeeingSharp.SampleContainer.Basics2D._02_Image
{
    [SampleDescription(
        "Image", 2, nameof(Basics2D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Basics2D/_02_Image",
        settingsType: typeof(ImageSampleSettings))]
    public class ImageSample : SampleBase
    {
        private const float IMAGE_WIDTH = 64;
        private const float IMAGE_HEIGHT = 64;

        private StandardBitmapResource m_bitmap;

        /// <summary>
        /// Called when the sample has to startup.
        /// </summary>
        public override async Task OnStartupAsync(RenderLoop targetRenderLoop, SampleSettings settings)
        {
            targetRenderLoop.EnsureNotNull(nameof(targetRenderLoop));

            var castedSettings = (ImageSampleSettings) settings;

            m_bitmap = new StandardBitmapResource(
                new AssemblyResourceLink(
                    this.GetType(),
                    "SimpleImage.png"));

            await targetRenderLoop.Register2DDrawingLayerAsync(graphics =>
            {
                var width = IMAGE_WIDTH * EngineMath.Clamp(castedSettings.Scaling, 0f, 100f);
                var height = IMAGE_HEIGHT * EngineMath.Clamp(castedSettings.Scaling, 0f, 100f);
                var bitmapRect = new RectangleF(
                    (graphics.ScreenWidth / 2f) - (width / 2f),
                    (graphics.ScreenHeight / 2f) - (height / 2f),
                    (float)width, (float)height);

                graphics.Clear(Color4Ex.WhiteSmoke);
                graphics.DrawBitmap(
                    m_bitmap,
                    bitmapRect,
                    castedSettings.Transparent ? 0.5f : 1f,
                    BitmapInterpolationMode.Linear);
            });
        }

        public override void NotifyClosed()
        {
            if (m_bitmap != null)
            {
                m_bitmap.Dispose();
                m_bitmap = null;
            }
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
