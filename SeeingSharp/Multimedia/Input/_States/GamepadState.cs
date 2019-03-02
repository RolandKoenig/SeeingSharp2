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

namespace SeeingSharp.Multimedia.Input
{
    public class GamepadState : InputStateBase
    {
        public static readonly GamepadState Dummy = new GamepadState(0);

        // State variables
        private int m_controllerIndex;
        private GamepadReportedState m_prevState;
        private GamepadReportedState m_currentState;
        private bool m_isConnected;

        /// <summary>
        /// Prevents a default instance of the <see cref="GamepadState"/> class from being created.
        /// </summary>
        public GamepadState()
        {
            m_prevState = new GamepadReportedState();
            m_currentState = new GamepadReportedState();

            Internals = new GamepadStateInternals(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GamepadState"/> class.
        /// </summary>
        internal GamepadState(int controllerIndex)
            : this()
        {
            m_controllerIndex = controllerIndex;
        }

        /// <summary>
        /// Is the given button down currently?
        /// </summary>
        /// <param name="button">The button to be checked.</param>
        public bool IsButtonDown(GamepadButton button)
        {
            if (!m_isConnected) { return false; }

            return ((short)m_currentState.Buttons & (short)button) == (short)button;
        }

        /// <summary>
        /// Is the given button hit exactly this frame?
        /// </summary>
        /// <param name="button">The button to be checked.</param>
        public bool IsButtonHit(GamepadButton button)
        {
            if (!m_isConnected) { return false; }

            var prevDown = ((short)m_prevState.Buttons & (short)button) == (short)button;
            var currentDown = ((short)m_currentState.Buttons & (short)button) == (short)button;

            return !prevDown && currentDown;
        }

        /// <summary>
        /// Copies this object and then resets it
        /// in preparation of the next update pass.
        /// Called by update-render loop.
        /// </summary>
        protected override void CopyAndResetForUpdatePassInternal(InputStateBase targetState)
        {
            var targetStateCasted = targetState as GamepadState;
            targetStateCasted.EnsureNotNull(nameof(targetStateCasted));

            targetStateCasted.m_controllerIndex = m_controllerIndex;
            targetStateCasted.m_isConnected = m_isConnected;
            targetStateCasted.m_prevState = m_prevState;
            targetStateCasted.m_currentState = m_currentState;
        }

        internal void NotifyConnected(bool isConnected)
        {
            m_isConnected = isConnected;
        }

        internal void NotifyState(GamepadReportedState controllerState)
        {
            m_prevState = m_currentState;
            m_currentState = controllerState;
        }

        /// <summary>
        /// Do we have any controller connected on this point.
        /// </summary>
        public bool IsConnected => m_isConnected;

        /// <summary>
        /// Gets the currently pressed buttons.
        /// </summary>
        public GamepadButton PressedButtons
        {
            get
            {
                if (!m_isConnected) { return GamepadButton.None; }
                return m_currentState.Buttons;
            }
        }

        /// <summary>
        /// The position of the left thumbstick on the X-axis. The value is between -1.0 and and 1.0.
        /// </summary>
        public float LeftThumbX => m_currentState.LeftThumbstickX;

        /// <summary>
        /// The position of the left thumbstick on the Y-axis. The value is between -1.0 and and 1.0.
        /// </summary>
        public float LeftThumbY => m_currentState.LeftThumbstickY;

        /// <summary>
        /// The position of the left trigger. The value is between 0.0 (not depressed) and 1.0 (fully depressed).
        /// </summary>
        public float LeftTrigger => m_currentState.LeftTrigger;

        /// <summary>
        /// The position of the right thumbstick on the X-axis. The value is between -1.0 and and 1.0.
        /// </summary>
        public float RightThumbX => m_currentState.RightThumbstickX;

        /// <summary>
        /// The position of the right thumbstick on the Y-axis. The value is between -1.0 and and 1.0.
        /// </summary>
        public float RightThumbY => m_currentState.RightThumbstickY;

        /// <summary>
        /// The position of the right trigger. The value is between 0.0 (not depressed) and 1.0 (fully depressed).
        /// </summary>
        public float RightTrigger => m_currentState.RightTrigger;

        public GamepadStateInternals Internals
        {
            get;
            private set;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        public class GamepadStateInternals
        {
            private GamepadState m_host;

            internal GamepadStateInternals(GamepadState host)
            {
                m_host = host;
            }

            public void NotifyConnected(bool isConnected)
            {
                m_host.NotifyConnected(isConnected);
            }

            public void NotifyState(GamepadReportedState controllerState)
            {
                m_host.NotifyState(controllerState);
            }
        }
    }
}