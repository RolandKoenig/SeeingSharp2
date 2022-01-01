using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using SeeingSharp.Core;
using SeeingSharp.Drawing2D;
using SeeingSharp.Drawing2D.Resources;
using SeeingSharp.Input;
using SeeingSharp.Mathematics;

namespace SeeingSharp.SampleContainer.Util
{
    public class PerformanceMeasureDrawingLayer : Custom2DDrawingLayer
    {
        // Data source (FPS)
        private List<TimeSpan> _lastTimeSpans;
        private DateTime _lastRender;

        // Data source (Keys down)
        private List<WinVirtualKey> _keysDown;

        // Common state values
        private ViewInformation _view;
        private float _verticalPadding;
        private bool _enabled;
        private PerformanceDrawingLayerType _currentType;

        // Drawing resources
        private TextFormatResource _textFormatCentered;
        private TextFormatResource _textFormatLeftAligned;
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
            _textFormatCentered = new TextFormatResource("Arial", 18f);
            _textFormatCentered.TextAlignment = TextAlignment.Center;
            _textFormatLeftAligned = new TextFormatResource("Arial", 18f);
            _textFormatLeftAligned.TextAlignment = TextAlignment.Leading;

            _backBrush = new SolidBrushResource(Color4.LightGray);
            _foreBrush = new SolidBrushResource(Color4.Black);
            _borderBrush = new SolidBrushResource(Color4.DarkGray);

            _enabled = true;
            _currentType = PerformanceDrawingLayerType.FramesPerSecond;
            _keysDown = new List<WinVirtualKey>(12);
        }

        /// <inheritdoc />
        protected override void Update(UpdateState updateState)
        {
            base.Update(updateState);

            updateState.HandleKeyboardInput(_view, (keyboardState) =>
            {
                // Remember all keys which are down currently
                _keysDown.Clear();
                foreach (var actKeyDown in keyboardState.KeysDown)
                {
                    _keysDown.Add(actKeyDown);
                }

                // Checks for number key (switches current mode for rendering)
                foreach (var actHitKey in keyboardState.KeysHit)
                {
                    switch (actHitKey)
                    {
                        case WinVirtualKey.D1:
                            _enabled = true;
                            _currentType = PerformanceDrawingLayerType.FramesPerSecond;
                            break;

                        case WinVirtualKey.D2:
                            _enabled = true;
                            _currentType = PerformanceDrawingLayerType.PressedKeys;
                            break;

                        case WinVirtualKey.D0:
                            _enabled = false;
                            _currentType = PerformanceDrawingLayerType.None;
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

            RectangleF targetRectFull;
            switch (_currentType)
            {
                case PerformanceDrawingLayerType.FramesPerSecond:

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
                    targetRectFull = new RectangleF(
                        graphics.ScreenWidth - 120f, 10f + _verticalPadding, 100f, 22f);
                    graphics.FillRectangle(targetRectFull, _backBrush);
                    graphics.DrawRectangle(targetRectFull, _borderBrush);
                    graphics.DrawText($"FPS: {fpsText}", _textFormatCentered, targetRectFull, _foreBrush, DrawTextOptions.Clip);
                    break;
                
                case PerformanceDrawingLayerType.PressedKeys:

                    // Build display text
                    var stringBuilder = new StringBuilder(16);
                    foreach (var actKeyDown in _keysDown)
                    {
                        if (stringBuilder.Length > 0) { stringBuilder.Append(", "); }
                        stringBuilder.Append(actKeyDown.ToString());
                    }

                    // Render background for display text
                    targetRectFull = new RectangleF(
                        graphics.ScreenWidth - 220f, 10f + _verticalPadding, 200f, 66f);
                    graphics.FillRectangle(targetRectFull, _backBrush);
                    graphics.DrawRectangle(targetRectFull, _borderBrush);

                    // Render header
                    var targetRectHeader = targetRectFull;
                    targetRectHeader.Height -= 44f;
                    targetRectHeader.Inflate(-3f, -3f);
                    graphics.DrawText("Keys down:", _textFormatLeftAligned, targetRectHeader, _foreBrush);

                    // Render content area
                    var targetRectContent = targetRectFull;
                    targetRectContent.Height -= 22f;
                    targetRectContent.Y += 22f;
                    targetRectContent.Inflate(-3f, -3f);
                    graphics.DrawText(stringBuilder.ToString(), _textFormatLeftAligned, targetRectContent, _foreBrush);
                    break;
            }
        }
    }
}
