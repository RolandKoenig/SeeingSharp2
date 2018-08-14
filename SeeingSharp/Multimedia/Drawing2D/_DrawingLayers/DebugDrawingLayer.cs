#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
#endregion
using SeeingSharp.Multimedia.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public class DebugDrawingLayer : Custom2DDrawingLayer, IDisposable
    {
        #region Drawing resources
        private TextFormatResource m_textFormat;
        private SolidBrushResource m_solidBrushForeground;
        private SolidBrushResource m_solidBrushBackground;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugDrawingLayer"/> class.
        /// </summary>
        public DebugDrawingLayer()
        {
            m_textFormat = new TextFormatResource("Arial", 30f);
            m_textFormat.TextAlignment = TextAlignment.Center;
            m_textFormat.ParagraphAlignment = ParagraphAlignment.Center;

            Color4 grayColor = Color4Ex.LightGray;
            grayColor.ChangeAlphaTo(0.8f);
            m_solidBrushBackground = new SolidBrushResource(grayColor);
            m_solidBrushForeground = new SolidBrushResource(Color4Ex.RedColor);
        }

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben aus, die mit dem Freigeben, Zurückgeben oder Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        public void Dispose()
        {
            SeeingSharpTools.SafeDispose(ref m_textFormat);
            SeeingSharpTools.SafeDispose(ref m_solidBrushBackground);
            SeeingSharpTools.SafeDispose(ref m_solidBrushForeground);
        }

        /// <summary>
        /// Performs custom 2D rendering.
        /// Be carefull: This method is called from the rendering thread!
        /// </summary>
        /// <param name="graphics">The graphics object used for drawing.</param>
        protected override void Draw2D(Graphics2D graphics)
        {
            if (m_textFormat == null) { return; }

            // Check for minimum screen size
            Size2F screenSize = graphics.ScreenSize;
            if((screenSize.Width < 300f) ||
               (screenSize.Height < 100f))
            {
                return;
            }

            // Draw the debug message on the upper right corner
            RectangleF targetRect = new RectangleF(
                screenSize.Width - 300f, 10f,
                280f, 80f);
            graphics.FillRoundedRectangle(targetRect, 10f, 10f, m_solidBrushBackground);
            graphics.DrawText("! Debug-Mode !", m_textFormat, targetRect, m_solidBrushForeground);
        }
    }
}
