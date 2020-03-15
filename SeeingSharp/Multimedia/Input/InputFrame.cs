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
using SeeingSharp.Multimedia.Core;
using System;
using System.Collections.Generic;
using SeeingSharp.Checking;

namespace SeeingSharp.Multimedia.Input
{
    /// <summary>
    /// An InputFrame describes states of all input devices on one specific time frame.
    /// </summary>
    public class InputFrame
    {
        private TimeSpan _frameDuration;
        private List<InputStateBase> _inputStates;
        private List<InputStateBase> _recoveredStates;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputFrame"/> class.
        /// </summary>
        /// <param name="expectedStateCount">The expected state count.</param>
        /// <param name="frameDuration">The TimeSpan which is covered by this InputFrame.</param>
        internal InputFrame(int expectedStateCount, TimeSpan frameDuration)
        {
            this.Reset(expectedStateCount, frameDuration);
        }

        /// <summary>
        /// Gets all input state for the given view.
        /// </summary>
        /// <param name="viewInfo">The view for which to get all input states.</param>
        public IEnumerable<InputStateBase> GetInputStates(ViewInformation viewInfo)
        {
            viewInfo.EnsureNotNull(nameof(viewInfo));

            var inputStateCount = _inputStates.Count;
            for (var loop = 0; loop < inputStateCount; loop++)
            {
                if (_inputStates[loop].RelatedView == viewInfo)
                {
                    yield return _inputStates[loop];
                }
            }
        }

        /// <summary>
        /// Gets all <see cref="KeyboardState"/> objects.
        /// </summary>
        public IEnumerable<KeyboardState> GetKeyboardStates(ViewInformation viewInfo = null)
        {
            return this.GetStates<KeyboardState>(viewInfo);
        }

        /// <summary>
        /// Gets all <see cref="MouseOrPointerState"/> objects.
        /// </summary>
        public IEnumerable<MouseOrPointerState> GetMouseOrPointerStates(ViewInformation viewInfo = null)
        {
            return this.GetStates<MouseOrPointerState>(viewInfo);
        }

        /// <summary>
        /// Gets all <see cref="GamepadState"/> objects.
        /// </summary>
        public IEnumerable<GamepadState> GetGamepadStates(ViewInformation viewInfo = null)
        {
            return this.GetStates<GamepadState>(viewInfo);
        }

        /// <summary>
        /// Gets all states of the given type.
        /// </summary>
        public IEnumerable<T> GetStates<T>(ViewInformation viewInfo = null)
            where T : InputStateBase
        {
            var inputStateCount = _inputStates.Count;
            for (var loop = 0; loop < inputStateCount; loop++)
            {
                var actStateCasted = _inputStates[loop] as T;
                if(actStateCasted == null){ continue; }

                if (viewInfo != null)
                {
                    if(actStateCasted.RelatedView != viewInfo) { continue; }
                }

                yield return actStateCasted;
            }
        }

        /// <summary>
        /// Resets this InputFrame to use this object more than once.
        /// </summary>
        /// <param name="expectedStateCount">The expected state count.</param>
        /// <param name="frameDuration">The TimeSpan which is covered by this InputFrame.</param>
        internal void Reset(int expectedStateCount, TimeSpan frameDuration)
        {
            if (_inputStates == null)
            {
                _inputStates = new List<InputStateBase>(expectedStateCount);
                _recoveredStates = new List<InputStateBase>(expectedStateCount);
            }
            else
            {
                _recoveredStates.AddRange(_inputStates);
                _inputStates.Clear();
            }

            _frameDuration = frameDuration;

            this.DefaultMouseOrPointer = MouseOrPointerState.Dummy;
            this.DefaultGamepad = GamepadState.Dummy;
            this.DefaultKeyboard = KeyboardState.Dummy;
        }

        internal void AddCopyOfState(InputStateBase inputState, ViewInformation viewInfo)
        {
            // Get the state object to where to copy the given InputState
            InputStateBase targetState = null;
            for (var loop = 0; loop < _recoveredStates.Count; loop++)
            {
                if (_recoveredStates[loop].CurrentType == inputState.CurrentType)
                {
                    targetState = _recoveredStates[loop];
                    _recoveredStates.RemoveAt(loop);
                    break;
                }
            }
            if (targetState == null)
            {
                targetState = (InputStateBase)Activator.CreateInstance(inputState.CurrentType);
            }

            // Copy all state data
            inputState.CopyAndResetForUpdatePass(targetState);
            targetState.RelatedView = viewInfo;

            this.AddState(targetState);
        }

        private void AddState(InputStateBase inputState)
        {
            _inputStates.Add(inputState);

            // Register first MouseOrPointer state as default
            if (this.DefaultMouseOrPointer == MouseOrPointerState.Dummy)
            {
                if (inputState is MouseOrPointerState mouseOrPointer)
                {
                    this.DefaultMouseOrPointer = mouseOrPointer;
                }
            }

            // Register first Gamepad state as default
            if (this.DefaultGamepad == GamepadState.Dummy)
            {
                if (inputState is GamepadState gamepadState)
                {
                    this.DefaultGamepad = gamepadState;
                }
            }

            // Register first keyboard state as default
            if (this.DefaultKeyboard == KeyboardState.Dummy)
            {
                if (inputState is KeyboardState keyboardState)
                {
                    this.DefaultKeyboard = keyboardState;
                }
            }
        }

        /// <summary>
        /// Gets the total count of input states.
        /// </summary>
        public int CountStates => _inputStates.Count;

        /// <summary>
        /// Gets a collection containing all gathered input states.
        /// </summary>
        public IEnumerable<InputStateBase> InputStates => _inputStates;

        /// <summary>
        /// Gets the first MouseOrPointerState.
        /// </summary>
        public MouseOrPointerState DefaultMouseOrPointer
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the default GamepadState.
        /// </summary>
        public GamepadState DefaultGamepad
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the default KeyboardState.
        /// </summary>
        public KeyboardState DefaultKeyboard
        {
            get;
            private set;
        }

        public TimeSpan FrameDuration => _frameDuration;
    }
}
