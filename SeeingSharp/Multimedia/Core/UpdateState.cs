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
using System;
using System.Collections.Generic;
using SeeingSharp.Multimedia.Input;

namespace SeeingSharp.Multimedia.Core
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
    }
}
