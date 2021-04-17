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
using System.Collections.Generic;
using System.Numerics;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Input;

namespace SeeingSharp.Multimedia.Core
{
    /// <summary>
    /// A UpdateState object which holds special variables for a Scene.
    /// </summary>
    public class SceneRelatedUpdateState : IAnimationUpdateState
    {
        // Constants
        private static readonly InputFrame[] s_dummyFrameCollection = new InputFrame[0];

        // parameters for single update step
        private bool _isPaused;
        private UpdateState _updateState;

        /// <summary>
        /// Gets current update time.
        /// </summary>
        public TimeSpan UpdateTime
        {
            get
            {
                _updateState.EnsureNotNull(nameof(_updateState));

                if (_isPaused && !this.IgnorePauseState) { return TimeSpan.Zero; }
                return _updateState.UpdateTime;
            }
        }

        /// <summary>
        /// Gets the current update time in milliseconds.
        /// </summary>
        public int UpdateTimeMilliseconds
        {
            get
            {
                _updateState.EnsureNotNull(nameof(_updateState));

                if (_isPaused && !this.IgnorePauseState) { return 0; }
                return _updateState.UpdateTimeMilliseconds;
            }
        }

        /// <summary>
        /// Gets the overall update state of Seeing#.
        /// </summary>
        public UpdateState UpdateState => _updateState;

        public bool IgnorePauseState { get; set; }

        public Matrix4Stack World { get; }

        public SceneLayer SceneLayer { get; internal set; }

        public Scene Scene => this.SceneLayer?.Scene;

        public bool IsPaused => _isPaused;

        /// <summary>
        /// Gets a collection containing all gathered InputFrames since last update pass.
        /// </summary>
        public IEnumerable<InputFrame> InputFrames => _updateState.InputFrames;

        internal bool ForceTransformUpdatesOnChildren;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneRelatedUpdateState"/> class.
        /// </summary>
        internal SceneRelatedUpdateState()
        {
            this.World = new Matrix4Stack(Matrix4x4.Identity);
        }

        /// <summary>
        /// Called just before the update pass of a scene object starts.
        /// </summary>
        /// <param name="targetScene">The scene for which to prepare this state object</param>
        /// <param name="updateState">The update state.</param>
        internal void OnStartSceneUpdate(Scene targetScene, UpdateState updateState)
        {
            targetScene.EnsureNotNull(nameof(targetScene));
            updateState.EnsureNotNull(nameof(updateState));

            _isPaused = targetScene.IsPaused;
            this.IgnorePauseState = updateState.IgnorePauseState;

            this.World.ResetStackToIdentity();

            _updateState = updateState;
            this.SceneLayer = null;
        }
    }
}