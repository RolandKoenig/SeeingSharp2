using System;
using System.Collections.Generic;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.Multimedia.Input
{
    /// <summary>
    /// An InputFrame describes states of all input devices on one specific time frame.
    /// Each view produces its own input states, therefore in a multi-view scenario you get more state objects of the same type.
    /// </summary>
    public class InputFrame
    {
        private TimeSpan _frameDuration;
        private List<InputStateBase> _inputStates;
        private List<InputStateBase> _recoveredStates;

        /// <summary>
        /// Gets the total count of input states.
        /// </summary>
        public int CountStates => _inputStates.Count;

        /// <summary>
        /// Gets a collection containing all gathered input states.
        /// </summary>
        public IEnumerable<InputStateBase> InputStates => _inputStates;

        /// <summary>
        /// Gets the <see cref="MouseOrPointerState"/> from the first view.
        /// </summary>
        public MouseOrPointerState DefaultMouseOrPointer
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="GamepadState"/> from the first view.
        /// </summary>
        public GamepadState DefaultGamepad
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="KeyboardState"/> from the first view.
        /// </summary>
        public KeyboardState DefaultKeyboard
        {
            get;
            private set;
        }

        public TimeSpan FrameDuration => _frameDuration;

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
        /// The returned collection may contain no elements if there is no suitable state object available.
        /// </summary>
        /// <param name="viewInfo">The view for which to get the requested input state objects</param>
        public IEnumerable<KeyboardState> TryGetKeyboardStates(ViewInformation viewInfo = null)
        {
            return this.TryGetStates<KeyboardState>(viewInfo);
        }

        /// <summary>
        /// Gets the <see cref="KeyboardState"/> for the given view.
        /// This method may return null if there is no <see cref="KeyboardState"/> available.
        /// </summary>
        /// <param name="viewInfo">The view for which to get the requested input state object</param>
        public KeyboardState TryGetKeyboardState(ViewInformation viewInfo)
        {
            return this.TryGetState<KeyboardState>(viewInfo);
        }

        /// <summary>
        /// Gets all <see cref="MouseOrPointerState"/> objects.
        /// The returned collection may contain no elements if there is no suitable state object available.
        /// </summary>
        /// <param name="viewInfo">The view for which to get the requested input state objects</param>
        public IEnumerable<MouseOrPointerState> TryGetMouseOrPointerStates(ViewInformation viewInfo = null)
        {
            return this.TryGetStates<MouseOrPointerState>(viewInfo);
        }

        /// <summary>
        /// Gets the <see cref="MouseOrPointerState"/> for the given view.
        /// This method may return null if there is no <see cref="MouseOrPointerState"/> available.
        /// </summary>
        /// <param name="viewInfo">The view for which to get the requested input state object</param>
        public MouseOrPointerState TryGetMouseOrPointerState(ViewInformation viewInfo)
        {
            return this.TryGetState<MouseOrPointerState>(viewInfo);
        }

        /// <summary>
        /// Gets all <see cref="GamepadState"/> objects.
        /// The returned collection may contain no elements if there is no suitable state object available.
        /// </summary>
        /// <param name="viewInfo">The view for which to get the requested input state objects</param>
        public IEnumerable<GamepadState> TryGetGamepadStates(ViewInformation viewInfo = null)
        {
            return this.TryGetStates<GamepadState>(viewInfo);
        }

        /// <summary>
        /// Gets the <see cref="GamepadState"/> for the given view.
        /// This method may return null if there is no <see cref="GamepadState"/> available.
        /// </summary>
        /// <param name="viewInfo">The view for which to get the requested input state object</param>
        public GamepadState TryGetGamepadState(ViewInformation viewInfo)
        {
            return this.TryGetState<GamepadState>(viewInfo);
        }

        /// <summary>
        /// Gets all states of the given type.
        /// The returned collection may contain no elements if there is no suitable state object available.
        /// </summary>
        /// <param name="viewInfo">The view for which to get the requested input state objects</param>
        public IEnumerable<T> TryGetStates<T>(ViewInformation viewInfo = null)
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
        /// Gets the input state of the given type for the given view.
        /// </summary>
        /// <param name="viewInfo">The view for which to get the requested input state object</param>
        public T TryGetState<T>(ViewInformation viewInfo)
            where T : InputStateBase
        {
            viewInfo.EnsureNotNull(nameof(viewInfo));

            var inputStateCount = _inputStates.Count;
            for (var loop = 0; loop < inputStateCount; loop++)
            {
                var actStateCasted = _inputStates[loop] as T;
                if(actStateCasted == null){ continue; }

                if(actStateCasted.RelatedView != viewInfo) { continue; }

                return actStateCasted;
            }

            return null;
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
    }
}
