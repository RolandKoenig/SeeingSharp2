#region License information
/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
namespace SeeingSharp.Multimedia.Core
{
    #region using

    using System;
    using System.Collections.Generic;
    using Checking;
    using Input;
    using SharpDX;

    #endregion

    /// <summary>
    /// A UpdateState object which holds special variables for a Scene.
    /// </summary>
    public class SceneRelatedUpdateState : IAnimationUpdateState
    {
        #region Constants
        private static readonly InputFrame[] DUMMY_FRAME_COLLECTION = new InputFrame[0];
        #endregion

        #region parameters
        private Scene m_owner;
        #endregion

        #region parameters for single update step
        private bool m_isPaused;
        private UpdateState m_updateState;
        private IEnumerable<InputFrame> m_inputFrames;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneRelatedUpdateState"/> class.
        /// </summary>
        internal SceneRelatedUpdateState(Scene owner)
        {
            m_owner = owner;
            World = new Matrix4Stack(Matrix.Identity);
            m_inputFrames = null;
        }

        /// <summary>
        /// Called just before the update pass of a scene object starts.
        /// </summary>
        /// <param name="targetScene">The scene for which to prepare this state object</param>
        /// <param name="updateState">The update state.</param>
        /// <param name="inputFrames">A list containing all gathered InputFrames since last update pass.</param>
        internal void OnStartSceneUpdate(Scene targetScene, UpdateState updateState, IEnumerable<InputFrame> inputFrames)
        {
            targetScene.EnsureNotNull(nameof(targetScene));
            updateState.EnsureNotNull(nameof(updateState));

            m_isPaused = targetScene.IsPaused;
            IgnorePauseState = updateState.IgnorePauseState;

            World.ResetStackToIdentity();

            m_updateState = updateState;
            SceneLayer = null;

            m_inputFrames = inputFrames;
            if(m_inputFrames == null) { m_inputFrames = DUMMY_FRAME_COLLECTION; }
        }

        /// <summary>
        /// Gets current update time.
        /// </summary>
        public TimeSpan UpdateTime
        {
            get
            {
                m_updateState.EnsureNotNull(nameof(m_updateState));

                if (m_isPaused && (!IgnorePauseState)) { return TimeSpan.Zero; }
                return m_updateState.UpdateTime;
            }
        }

        /// <summary>
        /// Gets the current update time in milliseconds.
        /// </summary>
        public int UpdateTimeMilliseconds
        {
            get
            {
                m_updateState.EnsureNotNull(nameof(m_updateState));

                if (m_isPaused && (!IgnorePauseState)) { return 0; }
                return m_updateState.UpdateTimeMilliseconds;
            }
        }

        public Matrix4Stack World { get; }

        public SceneLayer SceneLayer { get; internal set; }

        public Scene Scene
        {
            get
            {
                if (SceneLayer == null) { return null; }
                else { return SceneLayer.Scene; }
            }
        }

        public bool IsPaused
        {
            get { return m_isPaused; }
        }

        public bool IgnorePauseState { get; set; }

        /// <summary>
        /// Gets a collection containing all gathered InputFrames since last update pass.
        /// </summary>
        public IEnumerable<InputFrame> InputFrames
        {
            get { return m_inputFrames; }
        }

        internal bool ForceTransformUpdatesOnChilds;
    }
}