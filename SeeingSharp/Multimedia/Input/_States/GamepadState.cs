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

namespace SeeingSharp.Multimedia.Input
{
    public class GamepadState : InputStateBase
    {
        public static readonly GamepadState Dummy = new GamepadState(0);

        // State variables
        private int _controllerIndex;
        private GamepadReportedState _prevState;
        private GamepadReportedState _currentState;
        private bool _isConnected;

        /// <summary>
        /// Prevents a default instance of the <see cref="GamepadState"/> class from being created.
        /// </summary>
        public GamepadState()
        {
            _prevState = new GamepadReportedState();
            _currentState = new GamepadReportedState();

            this.Internals = new GamepadStateInternals(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GamepadState"/> class.
        /// </summary>
        internal GamepadState(int controllerIndex)
            : this()
        {
            _controllerIndex = controllerIndex;
        }

        /// <summary>
        /// Is the given button down currently?
        /// </summary>
        /// <param name="button">The button to be checked.</param>
        public bool IsButtonDown(GamepadButton button)
        {
            if (!_isConnected) { return false; }

            return ((short)_currentState.Buttons & (short)button) == (short)button;
        }

        /// <summary>
        /// Is the given button hit exactly this frame?
        /// </summary>
        /// <param name="button">The button to be checked.</param>
        public bool IsButtonHit(GamepadButton button)
        {
            if (!_isConnected) { return false; }

            var prevDown = ((short)_prevState.Buttons & (short)button) == (short)button;
            var currentDown = ((short)_currentState.Buttons & (short)button) == (short)button;

            return !prevDown && currentDown;
        }

        /// <summary>
        /// Copies this object and then resets it
        /// in preparation of the next update pass.
        /// Called by update-render loop.
        /// </summary>
        protected override void CopyAndResetForUpdatePassInternal(InputStateBase targetState)
        {
            var targetStateCasted = (GamepadState) targetState;

            targetStateCasted._controllerIndex = _controllerIndex;
            targetStateCasted._isConnected = _isConnected;
            targetStateCasted._prevState = _prevState;
            targetStateCasted._currentState = _currentState;
        }

        internal void NotifyConnected(bool isConnected)
        {
            _isConnected = isConnected;
        }

        internal void NotifyState(GamepadReportedState controllerState)
        {
            _prevState = _currentState;
            _currentState = controllerState;
        }

        /// <summary>
        /// Do we have any controller connected on this point.
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// Gets the currently pressed buttons.
        /// </summary>
        public GamepadButton PressedButtons
        {
            get
            {
                if (!_isConnected) { return GamepadButton.None; }
                return _currentState.Buttons;
            }
        }

        /// <summary>
        /// The position of the left thumbstick on the X-axis. The value is between -1.0 and and 1.0.
        /// </summary>
        public float LeftThumbX => _currentState.LeftThumbstickX;

        /// <summary>
        /// The position of the left thumbstick on the Y-axis. The value is between -1.0 and and 1.0.
        /// </summary>
        public float LeftThumbY => _currentState.LeftThumbstickY;

        /// <summary>
        /// The position of the left trigger. The value is between 0.0 (not depressed) and 1.0 (fully depressed).
        /// </summary>
        public float LeftTrigger => _currentState.LeftTrigger;

        /// <summary>
        /// The position of the right thumbstick on the X-axis. The value is between -1.0 and and 1.0.
        /// </summary>
        public float RightThumbX => _currentState.RightThumbstickX;

        /// <summary>
        /// The position of the right thumbstick on the Y-axis. The value is between -1.0 and and 1.0.
        /// </summary>
        public float RightThumbY => _currentState.RightThumbstickY;

        /// <summary>
        /// The position of the right trigger. The value is between 0.0 (not depressed) and 1.0 (fully depressed).
        /// </summary>
        public float RightTrigger => _currentState.RightTrigger;

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
            private GamepadState _host;

            internal GamepadStateInternals(GamepadState host)
            {
                _host = host;
            }

            public void NotifyConnected(bool isConnected)
            {
                _host.NotifyConnected(isConnected);
            }

            public void NotifyState(GamepadReportedState controllerState)
            {
                _host.NotifyState(controllerState);
            }
        }
    }
}