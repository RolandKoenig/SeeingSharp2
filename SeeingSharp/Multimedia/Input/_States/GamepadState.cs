#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
using SeeingSharp.Checking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.Multimedia.Input
{
    public class GamepadState : InputStateBase
    {
        public static readonly GamepadState Dummy = new GamepadState(0);

        #region State variables
        private int m_controllerIndex;
        private GamepadReportedState m_prevState;
        private GamepadReportedState m_currentState;
        private bool m_isConnected;
        #endregion

        /// <summary>
        /// Prevents a default instance of the <see cref="GamepadState"/> class from being created.
        /// </summary>
        public GamepadState()
        {
            m_prevState = new GamepadReportedState();
            m_currentState = new GamepadReportedState();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GamepadState"/> class.
        /// </summary>
        internal GamepadState(int controllerIndex)
            : this()
        {
            m_controllerIndex = controllerIndex;
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
        /// Copies this object and then resets it
        /// in preparation of the next update pass.
        /// Called by update-render loop.
        /// </summary>
        protected override void CopyAndResetForUpdatePassInternal(InputStateBase targetState)
        {
            GamepadState targetStateCasted = targetState as GamepadState;
            targetStateCasted.EnsureNotNull(nameof(targetStateCasted));

            targetStateCasted.m_controllerIndex = this.m_controllerIndex;
            targetStateCasted.m_isConnected = this.m_isConnected;
            targetStateCasted.m_prevState = this.m_prevState;
            targetStateCasted.m_currentState = this.m_currentState;
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

            bool prevDown = ((short)m_prevState.Buttons & (short)button) == (short)button;
            bool currentDown = ((short)m_currentState.Buttons & (short)button) == (short)button;

            
            return (!prevDown) && currentDown;
        }

        /// <summary>
        /// Do we have any controller connected on this point.
        /// </summary>
        public bool IsConnected
        {
            get { return m_isConnected; }
        }

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
        public float LeftThumbX
        {
            get { return m_currentState.LeftThumbstickX; }
        }

        /// <summary>
        /// The position of the left thumbstick on the Y-axis. The value is between -1.0 and and 1.0.
        /// </summary>
        public float LeftThumbY
        {
            get { return m_currentState.LeftThumbstickY; }
        }

        /// <summary>
        /// The position of the left trigger. The value is between 0.0 (not depressed) and 1.0 (fully depressed).
        /// </summary>
        public float LeftTrigger
        {
            get { return m_currentState.LeftTrigger; }
        }

        /// <summary>
        /// The position of the right thumbstick on the X-axis. The value is between -1.0 and and 1.0.
        /// </summary>
        public float RightThumbX
        {
            get { return m_currentState.RightThumbstickX; }
        }

        /// <summary>
        /// The position of the right thumbstick on the Y-axis. The value is between -1.0 and and 1.0.
        /// </summary>
        public float RightThumbY
        {
            get { return m_currentState.RightThumbstickY; }
        }

        /// <summary>
        /// The position of the right trigger. The value is between 0.0 (not depressed) and 1.0 (fully depressed).
        /// </summary>
        public float RightTrigger
        {
            get { return m_currentState.RightTrigger; }
        }
    }
}
