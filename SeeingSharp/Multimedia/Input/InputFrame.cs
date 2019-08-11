﻿/*
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
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.Multimedia.Input
{
    /// <summary>
    /// An InputFrame describes states of all input devices on one specific time frame.
    /// </summary>
    public class InputFrame
    {
        private TimeSpan m_frameDuration;
        private List<InputStateBase> m_inputStates;
        private List<InputStateBase> m_recoveredStates;

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
            var inputStateCount = m_inputStates.Count;
            for (var loop = 0; loop < inputStateCount; loop++)
            {
                if (m_inputStates[loop].RelatedView == viewInfo)
                {
                    yield return m_inputStates[loop];
                }
            }
        }

        /// <summary>
        /// Resets this InputFrame to use this object more than once.
        /// </summary>
        /// <param name="expectedStateCount">The expected state count.</param>
        /// <param name="frameDuration">The TimeSpan which is covered by this InputFrame.</param>
        internal void Reset(int expectedStateCount, TimeSpan frameDuration)
        {
            if(m_inputStates == null)
            {
                m_inputStates = new List<InputStateBase>(expectedStateCount);
                m_recoveredStates = new List<InputStateBase>(expectedStateCount);
            }
            else
            {
                m_recoveredStates.AddRange(m_inputStates);
                m_inputStates.Clear();
            }

            m_frameDuration = frameDuration;

            this.DefaultMouseOrPointer = MouseOrPointerState.Dummy;
            this.DefaultGamepad = GamepadState.Dummy;
            this.DefaultKeyboard = KeyboardState.Dummy;
        }

        internal void AddCopyOfState(InputStateBase inputState, ViewInformation viewInfo)
        {
            // Get the state object to where to copy the given InputState
            InputStateBase targetState = null;
            for(var loop=0; loop<m_recoveredStates.Count; loop++)
            {
                if(m_recoveredStates[loop].CurrentType == inputState.CurrentType)
                {
                    targetState = m_recoveredStates[loop];
                    m_recoveredStates.RemoveAt(loop);
                    break;
                }
            }
            if(targetState == null)
            {
                targetState = (InputStateBase) Activator.CreateInstance(inputState.CurrentType);
            }

            // Copy all state data
            inputState.CopyAndResetForUpdatePass(targetState);
            targetState.RelatedView = viewInfo;

            this.AddState(targetState);
        }

        private void AddState(InputStateBase inputState)
        {
            m_inputStates.Add(inputState);

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
        public int CountStates => m_inputStates.Count;

        /// <summary>
        /// Gets a collection containing all gathered input states.
        /// </summary>
        public IEnumerable<InputStateBase> InputStates => m_inputStates;

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

        public TimeSpan FrameDuration => m_frameDuration;
    }
}
