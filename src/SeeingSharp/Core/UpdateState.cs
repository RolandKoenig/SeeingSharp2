using System;
using System.Collections.Generic;
using SeeingSharp.Input;

namespace SeeingSharp.Core
{
    /// <summary>
    /// A state object created by the EngineMainLoop object which controls
    /// the update pass.
    /// </summary>
    public class UpdateState : IAnimationUpdateState
    {
        // Constants
        private static readonly InputFrame[] s_dummyFrameCollection = new InputFrame[0];

        // Parameters passed by global loop
        private int _updateTimeMilliseconds;
        private TimeSpan _updateTime;
        private IEnumerable<InputFrame> _inputFrames;
        private ulong _cycleId;

        /// <summary>
        /// Gets current update time.
        /// </summary>
        public TimeSpan UpdateTime => _updateTime;

        /// <summary>
        /// Gets the current update time in milliseconds.
        /// </summary>
        public int UpdateTimeMilliseconds => _updateTimeMilliseconds;

        public bool IgnorePauseState
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection containing all gathered InputFrames since last update pass.
        /// </summary>
        public IEnumerable<InputFrame> InputFrames => _inputFrames;

        /// <summary>
        /// Gets a number which is incremented on each cycle of the <see cref="EngineMainLoop"/>.
        /// It starts again at 0 when it reaches maximum.
        /// </summary>
        public ulong MainLoopCycleId => _cycleId;

        /// <summary>
        /// Prevents a default instance of the <see cref="UpdateState"/> class from being created.
        /// </summary>
        private UpdateState()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateState"/> class.
        /// </summary>
        /// <param name="updateTime">The update time.</param>
        /// <param name="inputFrames">A list containing all gathered InputFrames since last update pass.</param>
        internal UpdateState(TimeSpan updateTime, IEnumerable<InputFrame> inputFrames)
            : this()
        {
            this.Reset(updateTime, inputFrames);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Update time: {_updateTimeMilliseconds}, Cycle Id: {_cycleId}";
        }

        /// <summary>
        /// Handles keyboard input.
        /// </summary>
        /// <param name="inputHandlingAction">The action which reacts on input.</param>
        public void HandleKeyboardInput(Action<KeyboardState> inputHandlingAction)
        {
            foreach (var actInputFame in _inputFrames)
            {
                foreach (var actInputState in actInputFame.TryGetStates<KeyboardState>())
                {
                    inputHandlingAction(actInputState);
                }
            }
        }

        /// <summary>
        /// Handles keyboard input from the given view.
        /// </summary>
        /// <param name="viewInfo">The view for which to handle input.</param>
        /// <param name="inputHandlingAction">The action which reacts on input.</param>
        public void HandleKeyboardInput(ViewInformation viewInfo, Action<KeyboardState> inputHandlingAction)
        {
            foreach (var actInputFame in _inputFrames)
            {
                foreach (var actInputState in actInputFame.TryGetStates<KeyboardState>(viewInfo))
                {
                    inputHandlingAction(actInputState);
                }
            }
        }

        /// <summary>
        /// Handles mouse or pointer input.
        /// </summary>
        /// <param name="inputHandlingAction">The action which reacts on input.</param>
        public void HandleMouseOrPointerInput(Action<MouseOrPointerState> inputHandlingAction)
        {
            foreach (var actInputFame in _inputFrames)
            {
                foreach (var actInputState in actInputFame.TryGetStates<MouseOrPointerState>())
                {
                    inputHandlingAction(actInputState);
                }
            }
        }

        /// <summary>
        /// Handles mouse or pointer input from the given view.
        /// </summary>
        /// <param name="viewInfo">The view for which to handle input.</param>
        /// <param name="inputHandlingAction">The action which reacts on input.</param>
        public void HandleMouseOrPointerInput(ViewInformation viewInfo, Action<MouseOrPointerState> inputHandlingAction)
        {
            foreach (var actInputFame in _inputFrames)
            {
                foreach (var actInputState in actInputFame.TryGetStates<MouseOrPointerState>(viewInfo))
                {
                    inputHandlingAction(actInputState);
                }
            }
        }

        /// <summary>
        /// Handles gamepad input.
        /// </summary>
        /// <param name="inputHandlingAction">The action which reacts on input.</param>
        public void HandleGamepadInput(Action<GamepadState> inputHandlingAction)
        {
            foreach (var actInputFame in _inputFrames)
            {
                foreach (var actInputState in actInputFame.TryGetStates<GamepadState>())
                {
                    inputHandlingAction(actInputState);
                }
            }
        }

        /// <summary>
        /// Handles gamepad input from the given view.
        /// </summary>
        /// <param name="viewInfo">The view for which to handle input.</param>
        /// <param name="inputHandlingAction">The action which reacts on input.</param>
        public void HandleGamepadInput(ViewInformation viewInfo, Action<GamepadState> inputHandlingAction)
        {
            foreach (var actInputFame in _inputFrames)
            {
                foreach (var actInputState in actInputFame.TryGetStates<GamepadState>(viewInfo))
                {
                    inputHandlingAction(actInputState);
                }
            }
        }

        /// <summary>
        /// Called internally by EngineMainLoop and creates a copy of this object
        /// for each updated scene.
        /// </summary>
        internal UpdateState CopyForSceneUpdate()
        {
            var result = new UpdateState
            {
                _updateTime = _updateTime,
                _updateTimeMilliseconds = _updateTimeMilliseconds
            };

            return result;
        }

        /// <summary>
        /// Resets this UpdateState to the given update time.
        /// </summary>
        /// <param name="updateTime">The update time.</param>
        /// <param name="inputFrames">A list containing all gathered InputFrames since last update pass.</param>
        internal void Reset(TimeSpan updateTime, IEnumerable<InputFrame> inputFrames)
        {
            _updateTime = updateTime;
            _updateTimeMilliseconds = (int)updateTime.TotalMilliseconds;

            _inputFrames = inputFrames;
            if (_inputFrames == null) { _inputFrames = s_dummyFrameCollection; }

            if (_cycleId < ulong.MaxValue) { _cycleId++; }
            else { _cycleId = 0; }
        }
    }
}
