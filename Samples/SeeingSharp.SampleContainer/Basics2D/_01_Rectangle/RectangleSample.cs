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
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Util;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SeeingSharp.SampleContainer.Basics2D._01_Rectangle
{
    [SampleDescription(
        "Rectangle", 1, nameof(Basics2D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Basics2D/_01_Rectangle",
        typeof(RectangleSampleSettings))]
    public class RectangleSample : SampleBase
    {
        private RectangleSampleSettings m_castedSettings;

        private SolidBrushResource m_fillBrush;
        private SolidBrushResource m_fillBrushTransparent;

        public override Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            m_castedSettings = (RectangleSampleSettings)settings;

            m_fillBrush = new SolidBrushResource(Color4.Gray);
            m_fillBrushTransparent = new SolidBrushResource(Color4.Gray, 0.5f);

            return Task.FromResult<object>(null);
        }

        public override async Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop)
        {
            await mainOrChildRenderLoop.Register2DDrawingLayerAsync(graphics =>
            {
                // Clear the screen
                base.Draw2DBackground(graphics);

                // Calculate rectangle location
                var width = m_castedSettings.Width;
                var height = m_castedSettings.Height;
                var rectToDraw = new RectangleF(
                    graphics.ScreenWidth / 2f - width / 2f,
                    graphics.ScreenHeight / 2f - height / 2f,
                    width, height);

                // Draw the rectangle
                if (m_castedSettings.Rounded)
                {
                    graphics.FillRoundedRectangle(
                        rectToDraw,
                        m_castedSettings.RoundedRadius, m_castedSettings.RoundedRadius,
                        m_castedSettings.Transparent ? m_fillBrushTransparent : m_fillBrush);
                }
                else
                {
                    graphics.FillRectangle(
                        rectToDraw,
                        m_castedSettings.Transparent ? m_fillBrushTransparent : m_fillBrush);
                }
            });
        }

        public override void OnClosed()
        {
            base.OnClosed();

            SeeingSharpUtil.SafeDispose(ref m_fillBrush);
            SeeingSharpUtil.SafeDispose(ref m_fillBrushTransparent);
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
