/*
    SeeingSharp and all applications distributed together with it. 
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
using System.Numerics;
using SeeingSharp.Checking;

namespace SeeingSharp.Multimedia.Input
{
    /// <summary>
    /// A state object describing current mouse or pointer input.
    /// </summary>
    public class MouseOrPointerState : InputStateBase
    {
        public static readonly MouseOrPointerState Dummy = new MouseOrPointerState();
        private static readonly int s_buttonCount = Enum.GetValues(typeof(MouseButton)).Length;

        // Current state
        private Vector2 _moveDistancePixel;
        private Vector2 _positionPixel;
        private Vector2 _screenSizePixel;
        private int _wheelDelta;
        private bool[] _buttonsHit;        // Only for one frame at true
        private bool[] _buttonsDown;       // All following frames the mouse is down
        private bool[] _buttonsUp;         // True on the frame when the button changes to up
        private bool _isInside;

        public Vector2 MoveDistanceDip => _moveDistancePixel;

        public Vector2 PositionDip => _positionPixel;

        public Vector2 MoveDistanceRelative
        {
            get
            {
                if (EngineMath.EqualsWithTolerance(_screenSizePixel.X, 0f)) { return Vector2.Zero; }
                if (EngineMath.EqualsWithTolerance(_screenSizePixel.Y, 0f)) { return Vector2.Zero; }

                return _moveDistancePixel / _screenSizePixel;
            }
        }

        public Vector2 PositionRelative
        {
            get
            {
                if (EngineMath.EqualsWithTolerance(_screenSizePixel.X, 0f)) { return Vector2.Zero; }
                if (EngineMath.EqualsWithTolerance(_screenSizePixel.Y, 0f)) { return Vector2.Zero; }

                return _positionPixel / _screenSizePixel;
            }
        }

        /// <summary>
        /// Gets the size of the screen in device independent pixel.
        /// </summary>
        public Vector2 ScreenSizeDip => _screenSizePixel;

        /// <summary>
        /// Gets the current wheel delta.
        /// </summary>
        public int WheelDelta => _wheelDelta;

        public bool IsInsideView => _isInside;

        public MouseOrPointerType Type { get; internal set; }

        public MouseOrPointerStateInternals Internals { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseOrPointerState"/> class.
        /// </summary>
        public MouseOrPointerState()
        {
            var buttonCount = s_buttonCount;
            if (buttonCount <= 0)
            {
                buttonCount = Enum.GetValues(typeof(MouseButton)).Length;
            }

            this.Internals = new MouseOrPointerStateInternals(this);

            _buttonsHit = new bool[buttonCount];
            _buttonsDown = new bool[buttonCount];
            _buttonsUp = new bool[buttonCount];
        }

        /// <summary>
        /// Returns true if the user pressed the given button in this frame.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        public bool IsButtonHit(MouseButton mouseButton)
        {
            return _buttonsHit[(int)mouseButton];
        }

        /// <summary>
        /// Returns true if the user pressed this button before and still don't loosed it.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        public bool IsButtonDown(MouseButton mouseButton)
        {
            return _buttonsDown[(int)mouseButton];
        }

        /// <summary>
        /// Returns true if the user loosed the given button in this frame.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        public bool IsButtonUp(MouseButton mouseButton)
        {
            return _buttonsUp[(int)mouseButton];
        }

        /// <summary>
        /// Copies this object and then resets it
        /// in preparation of the next update pass.
        /// Called by update-render loop.
        /// </summary>
        protected override void CopyAndResetForUpdatePassInternal(InputStateBase targetState)
        {
            var targetStateCasted = (MouseOrPointerState) targetState;

            targetStateCasted._moveDistancePixel = _moveDistancePixel;
            targetStateCasted._screenSizePixel = _screenSizePixel;
            targetStateCasted._positionPixel = _positionPixel;
            targetStateCasted._wheelDelta = _wheelDelta;
            targetStateCasted._isInside = _isInside;
            targetStateCasted.Type = this.Type;
            for (var loop = 0; loop < s_buttonCount; loop++)
            {
                targetStateCasted._buttonsDown[loop] = _buttonsDown[loop];
                targetStateCasted._buttonsHit[loop] = _buttonsHit[loop];
                targetStateCasted._buttonsUp[loop] = _buttonsUp[loop];
            }

            // Reset current object
            _moveDistancePixel = Vector2.Zero;
            _wheelDelta = 0;
            for (var loop = 0; loop < s_buttonCount; loop++)
            {
                _buttonsUp[loop] = false;

                if (_buttonsHit[loop] || _buttonsDown[loop])
                {
                    _buttonsHit[loop] = false;
                    _buttonsDown[loop] = true;
                }
                else
                {
                    _buttonsHit[loop] = false;
                    _buttonsDown[loop] = false;
                }
            }
        }

        internal void NotifyButtonDown(MouseButton button)
        {
            var index = (int)button;
            this.UpdateMouseButtonState(index, true);
        }

        internal void NotifyButtonUp(MouseButton button)
        {
            var index = (int)button;
            this.UpdateMouseButtonState(index, false);
        }

        /// <summary>
        /// Notifies the state of the mouse buttons.
        /// Called by input handler.
        /// </summary>
        internal void NotifyButtonStates(
            bool isLeftButtonDown,
            bool isMiddleButtonDown,
            bool isRightButtonDown,
            bool isExtended1ButtonDown,
            bool isExtended2ButtonDown)
        {
            bool[] buttonStates = {
                isLeftButtonDown,
                isMiddleButtonDown,
                isRightButtonDown,
                isExtended1ButtonDown,
                isExtended2ButtonDown
            };

            // Check correct count of buttons
            buttonStates.EnsureCountEquals(s_buttonCount, nameof(buttonStates));

            // Update mouse states
            for (var loop = 0; loop < buttonStates.Length; loop++)
            {
                this.UpdateMouseButtonState(loop, buttonStates[loop]);
            }
        }

        /// <summary>
        /// Notifies some information about the mouse pointer.
        /// Called by input handler.
        /// </summary>
        internal void NotifyMouseLocation(Vector2 pixelPosition, Vector2 moveDistancePixel, Vector2 screenSizePixel)
        {
            _positionPixel = pixelPosition;
            _moveDistancePixel = moveDistancePixel;
            _screenSizePixel = screenSizePixel;
        }

        /// <summary>
        /// Notifies the mouse wheel.
        /// Called by input handler.
        /// </summary>
        internal void NotifyMouseWheel(int wheelDelta)
        {
            _wheelDelta += wheelDelta;
        }

        internal void NotifyInside(bool isMouseInside)
        {
            _isInside = isMouseInside;

            if (!_isInside)
            {
                for (var loop = 0; loop < s_buttonCount; loop++)
                {
                    _buttonsDown[loop] = false;
                    _buttonsHit[loop] = false;
                    _buttonsUp[loop] = false;
                }
            }
        }

        /// <summary>
        /// Helper method: Updates the button state at the given index based
        /// on currently notified press-state.
        /// </summary>
        /// <param name="buttonIndex">Index of the button.</param>
        /// <param name="pressedState">True, if the button is pressed currently.</param>
        private void UpdateMouseButtonState(int buttonIndex, bool pressedState)
        {
            var isHitOrDown = _buttonsHit[buttonIndex] | _buttonsDown[buttonIndex];
            if (isHitOrDown == pressedState) { return; }

            if (pressedState)
            {
                _buttonsHit[buttonIndex] = true;
                _buttonsDown[buttonIndex] = true;
            }
            else
            {
                _buttonsHit[buttonIndex] = false;
                _buttonsDown[buttonIndex] = false;
                _buttonsUp[buttonIndex] = true;
            }
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        public class MouseOrPointerStateInternals
        {
            private MouseOrPointerState _host;

            public MouseOrPointerType Type
            {
                get => _host.Type;
                set => _host.Type = value;
            }

            internal MouseOrPointerStateInternals(MouseOrPointerState host)
            {
                _host = host;
            }

            public void NotifyButtonDown(MouseButton button)
            {
                _host.NotifyButtonDown(button);
            }

            public void NotifyButtonUp(MouseButton button)
            {
                _host.NotifyButtonUp(button);
            }

            public void NotifyButtonStates(
                bool isLeftButtonDown,
                bool isMiddleButtonDown,
                bool isRightButtonDown,
                bool isExtended1ButtonDown,
                bool isExtended2ButtonDown)
            {
                _host.NotifyButtonStates(isLeftButtonDown, isMiddleButtonDown, isRightButtonDown, isExtended1ButtonDown, isExtended2ButtonDown);
            }

            public void NotifyMouseLocation(Vector2 pixelPosition, Vector2 moveDistancePixel, Vector2 screenSizePixel)
            {
                _host.NotifyMouseLocation(pixelPosition, moveDistancePixel, screenSizePixel);
            }

            public void NotifyMouseWheel(int wheelDelta)
            {
                _host.NotifyMouseWheel(wheelDelta);
            }

            public void NotifyInside(bool isMouseInside)
            {
                _host.NotifyInside(isMouseInside);
            }
        }
    }
}
