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

using System;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public class DebugDrawingLayer : Custom2DDrawingLayer, IDisposable
    {
        // Drawing resources
        private TextFormatResource _textFormat;
        private SolidBrushResource _solidBrushForeground;
        private SolidBrushResource _solidBrushBackground;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugDrawingLayer"/> class.
        /// </summary>
        public DebugDrawingLayer()
        {
            _textFormat = new TextFormatResource("Arial", 30f)
            {
                TextAlignment = TextAlignment.Center,
                ParagraphAlignment = ParagraphAlignment.Center
            };

            var grayColor = Color4.LightGray;
            grayColor.ChangeAlphaTo(0.8f);
            _solidBrushBackground = new SolidBrushResource(grayColor);
            _solidBrushForeground = new SolidBrushResource(Color4.RedColor);
        }

        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref _textFormat);
            SeeingSharpUtil.SafeDispose(ref _solidBrushBackground);
            SeeingSharpUtil.SafeDispose(ref _solidBrushForeground);
        }

        /// <summary>
        /// Performs custom 2D rendering.
        /// Be careful: This method is called from the rendering thread!
        /// </summary>
        /// <param name="graphics">The graphics object used for drawing.</param>
        protected override void Draw2D(Graphics2D graphics)
        {
            if (_textFormat == null) { return; }

            // Check for minimum screen size
            var screenSize = graphics.ScreenSize;

            if (screenSize.Width < 300f ||
               screenSize.Height < 100f)
            {
                return;
            }

            // Draw the debug message on the upper right corner
            var targetRect = new RectangleF(
                10f, 10f,
                280f, 80f);
            graphics.FillRoundedRectangle(targetRect, 10f, 10f, _solidBrushBackground);
            graphics.DrawText("! Debug-Mode !", _textFormat, targetRect, _solidBrushForeground);
        }
    }
}