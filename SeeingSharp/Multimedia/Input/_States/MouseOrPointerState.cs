#region License information
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
#endregion

using System;
using SeeingSharp.Checking;
using SharpDX;

namespace SeeingSharp.Multimedia.Input
{
    #region using
    #endregion

    /// <summary>
    /// A state object describing current mouse or pointer input.
    /// </summary>
    public class MouseOrPointerState : InputStateBase
    {
        public static readonly MouseOrPointerState Dummy = new MouseOrPointerState();
        private static readonly int BUTTON_COUNT = Enum.GetValues(typeof(MouseButton)).Length;

        /// <summary>
        /// Returns true if the user pressed the given button in this frame.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        public bool IsButtonHit(MouseButton mouseButton)
        {
            return m_buttonsHit[(int)mouseButton];
        }

        /// <summary>
        /// Returns true if the user pressed this button before and still don't loosed it.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        public bool IsButtonDown(MouseButton mouseButton)
        {
            return m_buttonsDown[(int)mouseButton];
        }

        /// <summary>
        /// Returns true if the user loosed the given button in this frame.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        public bool IsButtonUp(MouseButton mouseButton)
        {
            return m_buttonsUp[(int)mouseButton];
        }

        internal void NotifyButtonDown(MouseButton button)
        {
            var index = (int)button;
            UpdateMouseButtonState(index, true);
        }

        internal void NotifyButtonUp(MouseButton button)
        {
            var index = (int)button;
            UpdateMouseButtonState(index, false);
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
            buttonStates.EnsureCountEquals(BUTTON_COUNT, nameof(buttonStates));

            // Update mouse states
            for(var loop=0; loop<buttonStates.Length; loop++)
            {
                UpdateMouseButtonState(loop, buttonStates[loop]);
            }
        }

        /// <summary>
        /// Notifies some information about the mouse pointer.
        /// Called by input handler.
        /// </summary>
        internal void NotifyMouseLocation(Vector2 pixelPosition, Vector2 moveDistancePixel, Vector2 screenSizePixel)
        {
            m_positionPixel = pixelPosition;
            m_moveDistancePixel = moveDistancePixel;
            m_screenSizePixel = screenSizePixel;
        }

        /// <summary>
        /// Notifies the mouse wheel.
        /// Called by input handler.
        /// </summary>
        internal void NotifyMouseWheel(int wheelDelta)
        {
            m_wheelDelta += wheelDelta;
        }

        internal void NotifyInside(bool isMouseInside)
        {
            m_isInside = isMouseInside;

            if (!m_isInside)
            {
                for (var loop = 0; loop < BUTTON_COUNT; loop++)
                {
                    m_buttonsDown[loop] = false;
                    m_buttonsHit[loop] = false;
                    m_buttonsUp[loop] = false;
                }
            }
        }

        /// <summary>
        /// Copies this object and then resets it
        /// in preparation of the next update pass.
        /// Called by update-render loop.
        /// </summary>
        protected override void CopyAndResetForUpdatePassInternal(InputStateBase targetState)
        {
            var targetStateCasted = targetState as MouseOrPointerState;
            targetStateCasted.EnsureNotNull(nameof(targetStateCasted));

            targetStateCasted.m_moveDistancePixel = m_moveDistancePixel;
            targetStateCasted.m_screenSizePixel = m_screenSizePixel;
            targetStateCasted.m_positionPixel = m_positionPixel;
            targetStateCasted.m_wheelDelta = m_wheelDelta;
            targetStateCasted.m_isInside = m_isInside;
            targetStateCasted.Type = Type;
            for (var loop = 0; loop < BUTTON_COUNT; loop++)
            {
                targetStateCasted.m_buttonsDown[loop] = m_buttonsDown[loop];
                targetStateCasted.m_buttonsHit[loop] = m_buttonsHit[loop];
                targetStateCasted.m_buttonsUp[loop] = m_buttonsUp[loop];
            }

            // Reset current object
            m_moveDistancePixel = Vector2.Zero;
            m_wheelDelta = 0;
            for(var loop=0; loop<BUTTON_COUNT; loop++)
            {
                m_buttonsUp[loop] = false;

                if(m_buttonsHit[loop] || m_buttonsDown[loop])
                {
                    m_buttonsHit[loop] = false;
                    m_buttonsDown[loop] = true;
                }
                else
                {
                    m_buttonsHit[loop] = false;
                    m_buttonsDown[loop] = false;
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
            var isHitOrDown = m_buttonsHit[buttonIndex] | m_buttonsDown[buttonIndex];
            if (isHitOrDown == pressedState) { return; }

            if (pressedState)
            {
                m_buttonsHit[buttonIndex] = true;
                m_buttonsDown[buttonIndex] = true;
            }
            else
            {
                m_buttonsHit[buttonIndex] = false;
                m_buttonsDown[buttonIndex] = false;
                m_buttonsUp[buttonIndex] = true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseOrPointerState"/> class.
        /// </summary>
        public MouseOrPointerState()
        {
            var buttonCount = BUTTON_COUNT;
            if (buttonCount <= 0)
            {
                buttonCount = Enum.GetValues(typeof(MouseButton)).Length;
            }

            Internals = new MouseOrPointerStateInternals(this);

            m_buttonsHit = new bool[buttonCount];
            m_buttonsDown = new bool[buttonCount];
            m_buttonsUp = new bool[buttonCount];
        }

        public Vector2 MoveDistanceDip => m_moveDistancePixel;

        public Vector2 PositionDip => m_positionPixel;

        public Vector2 MoveDistanceRelative
        {
            get
            {
                if (m_screenSizePixel.X == 0f) { return Vector2.Zero; }
                if (m_screenSizePixel.Y == 0f) { return Vector2.Zero; }

                return m_moveDistancePixel / m_screenSizePixel;
            }
        }

        public Vector2 PositionRelative
        {
            get
            {
                if(m_screenSizePixel.X == 0f) { return Vector2.Zero; }
                if(m_screenSizePixel.Y == 0f) { return Vector2.Zero; }

                return m_positionPixel / m_screenSizePixel;
            }
        }

        /// <summary>
        /// Gets the size of the screen in device independent pixel.
        /// </summary>
        public Vector2 ScreenSizeDip => m_screenSizePixel;

        /// <summary>
        /// Gets the current wheel delta.
        /// </summary>
        public int WheelDelta => m_wheelDelta;

        public bool IsInsideView => m_isInside;

        public MouseOrPointerType Type { get; internal set; }

        public MouseOrPointerStateInternals Internals { get; }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        public class MouseOrPointerStateInternals
        {
            private MouseOrPointerState m_host;

            public void NotifyButtonDown(MouseButton button)
            {
                m_host.NotifyButtonDown(button);
            }

            public void NotifyButtonUp(MouseButton button)
            {
                m_host.NotifyButtonUp(button);
            }

            public void NotifyButtonStates(
                bool isLeftButtonDown,
                bool isMiddleButtonDown,
                bool isRightButtonDown,
                bool isExtended1ButtonDown,
                bool isExtended2ButtonDown)
            {
                m_host.NotifyButtonStates(isLeftButtonDown, isMiddleButtonDown, isRightButtonDown, isExtended1ButtonDown, isExtended2ButtonDown);
            }

            public void NotifyMouseLocation(Vector2 pixelPosition, Vector2 moveDistancePixel, Vector2 screenSizePixel)
            {
                m_host.NotifyMouseLocation(pixelPosition, moveDistancePixel, screenSizePixel);
            }

            public void NotifyMouseWheel(int wheelDelta)
            {
                m_host.NotifyMouseWheel(wheelDelta);
            }

            public void NotifyInside(bool isMouseInside)
            {
                m_host.NotifyInside(isMouseInside);
            }

            internal MouseOrPointerStateInternals(MouseOrPointerState host)
            {
                m_host = host;
            }

            public MouseOrPointerType Type
            {
                get => m_host.Type;
                set => m_host.Type = value;
            }
        }

        #region Generic info
        #endregion

        #region Current state
        private Vector2 m_moveDistancePixel;
        private Vector2 m_positionPixel;
        private Vector2 m_screenSizePixel;
        private int m_wheelDelta;
        private bool[] m_buttonsHit;        // Only for one frame at true
        private bool[] m_buttonsDown;       // All following frames the mouse is down
        private bool[] m_buttonsUp;         // True on the frame when the button changes to up
        private bool m_isInside;
        #endregion
    }
}
