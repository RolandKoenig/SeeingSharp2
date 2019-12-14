using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Util;
using SharpDX;
using SharpDX.DirectWrite;

namespace SeeingSharp.SampleContainer.Util
{
    public class PerformanceMeasureDrawingLayer : Custom2DDrawingLayer
    {
        // Data source
        private PerformanceAnalyzer m_performaneAnalyzer;
        private List<TimeSpan> m_lastTimeSpans;
        private DateTime m_lastRender;
        private float m_verticalPadding;

        // Drawing resources
        private TextFormatResource m_textFormat;
        private SolidBrushResource m_backBrush;
        private SolidBrushResource m_foreBrush;
        private SolidBrushResource m_borderBrush;

        public PerformanceMeasureDrawingLayer(PerformanceAnalyzer performaneAnalyzer, float verticalPadding)
        {
            m_performaneAnalyzer = performaneAnalyzer;
            m_verticalPadding = verticalPadding;

            m_lastTimeSpans = new List<TimeSpan>();
            m_lastRender = DateTime.UtcNow;

            // Define drawing resources
            m_textFormat = new TextFormatResource(
                "Arial", 18f);
            m_textFormat.TextAlignment = TextAlignment.Center;

            m_backBrush = new SolidBrushResource(Color4.LightGray);
            m_foreBrush = new SolidBrushResource(Color4.Black);
            m_borderBrush = new SolidBrushResource(Color4.DarkGray);
        }

        protected override void Draw2D(Graphics2D graphics)
        {
            base.Draw2D(graphics);

            // Get display text
            var fpsText = "-";
            if (m_lastRender != DateTime.MinValue)
            {
                m_lastTimeSpans.Add(DateTime.UtcNow - m_lastRender);
                while (m_lastTimeSpans.Count > 30) { m_lastTimeSpans.RemoveAt(0); }

                var averageTime = m_lastTimeSpans
                    .Select(actTimeSpan => actTimeSpan.TotalMilliseconds)
                    .Average();
                fpsText = Math.Round(1000f / averageTime, 0).ToString(CultureInfo.InvariantCulture);
            }
            m_lastRender = DateTime.UtcNow;

            // Render display text
            var targetRect = new RectangleF(
                graphics.ScreenWidth - 120f, 10f + m_verticalPadding, 100f, 22f);
            graphics.FillRectangle(targetRect, m_backBrush);
            graphics.DrawRectangle(targetRect, m_borderBrush);
            graphics.DrawText($"FPS: {fpsText}", m_textFormat, targetRect, m_foreBrush);
        }
    }
}
