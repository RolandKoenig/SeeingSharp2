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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Multimedia.Input;

namespace SeeingSharp.SampleContainer.Util
{
    public class PerformanceMeasureDrawingLayer : Custom2DDrawingLayer
    {
        // Data source
        private List<TimeSpan> _lastTimeSpans;
        private DateTime _lastRender;
        private bool _enabled;
        private ViewInformation _view;
        private float _verticalPadding;

        // Drawing resources
        private TextFormatResource _textFormat;
        private SolidBrushResource _backBrush;
        private SolidBrushResource _foreBrush;
        private SolidBrushResource _borderBrush;

        public PerformanceMeasureDrawingLayer(float verticalPadding, ViewInformation view)
        {
            _view = view;
            _verticalPadding = verticalPadding;

            _lastTimeSpans = new List<TimeSpan>();
            _lastRender = DateTime.UtcNow;

            // Define drawing resources
            _textFormat = new TextFormatResource(
                "Arial", 18f);
            _textFormat.TextAlignment = TextAlignment.Center;

            _backBrush = new SolidBrushResource(Color4.LightGray);
            _foreBrush = new SolidBrushResource(Color4.Black);
            _borderBrush = new SolidBrushResource(Color4.DarkGray);

            _enabled = true;
        }

        /// <inheritdoc />
        protected override void Update(UpdateState updateState)
        {
            base.Update(updateState);

            updateState.HandleKeyboardInput(_view, (keyboardState) =>
            {
                foreach (var actHitKey in keyboardState.KeysHit)
                {
                    switch (actHitKey)
                    {
                        case WinVirtualKey.D1:
                            _enabled = true;
                            break;

                        case WinVirtualKey.D0:
                            _enabled = false;
                            break;
                    }
                }
            });
        }

        /// <inheritdoc />
        protected override void Draw2D(Graphics2D graphics)
        {
            base.Draw2D(graphics);

            if (!_enabled) { return; }

            // Get display text
            var fpsText = "-";
            if (_lastRender != DateTime.MinValue)
            {
                _lastTimeSpans.Add(DateTime.UtcNow - _lastRender);
                while (_lastTimeSpans.Count > 30) { _lastTimeSpans.RemoveAt(0); }

                var averageTime = _lastTimeSpans
                    .Select(actTimeSpan => actTimeSpan.TotalMilliseconds)
                    .Average();
                fpsText = Math.Round(1000f / averageTime, 0).ToString(CultureInfo.InvariantCulture);
            }
            _lastRender = DateTime.UtcNow;

            // Render display text
            var targetRect = new RectangleF(
                graphics.ScreenWidth - 120f, 10f + _verticalPadding, 100f, 22f);
            graphics.FillRectangle(targetRect, _backBrush);
            graphics.DrawRectangle(targetRect, _borderBrush);
            graphics.DrawText($"FPS: {fpsText}", _textFormat, targetRect, _foreBrush);
        }
    }
}
