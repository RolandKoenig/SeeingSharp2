﻿#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
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

    #endregion

    /// <summary>
    /// A state object created by the EngineMainLoop object which controls
    /// the update pass.
    /// </summary>
    public class UpdateState : IAnimationUpdateState
    {
        #region Parameters passed by global loop

        #endregion

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
        public UpdateState(TimeSpan updateTime)
            : this()
        {
            UpdateTime = updateTime;
            UpdateTimeMilliseconds = (int)updateTime.TotalMilliseconds;
        }

        /// <summary>
        /// Called internally by EngineMainLoop and creates a copy of this object
        /// for each updated scene.
        /// </summary>
        internal UpdateState CopyForSceneUpdate()
        {
            var result = new UpdateState
            {
                UpdateTime = UpdateTime,
                UpdateTimeMilliseconds = UpdateTimeMilliseconds
            };

            return result;
        }

        /// <summary>
        /// Resets this UpdateState to the given update time.
        /// </summary>
        /// <param name="updateTime">The update time.</param>
        internal void Reset(TimeSpan updateTime)
        {
            UpdateTime = updateTime;
            UpdateTimeMilliseconds = (int)updateTime.TotalMilliseconds;
        }

        /// <summary>
        /// Gets current update time.
        /// </summary>
        public TimeSpan UpdateTime { get; private set; }

        /// <summary>
        /// Gets the current update time in milliseconds.
        /// </summary>
        public int UpdateTimeMilliseconds { get; private set; }

        public bool IgnorePauseState
        {
            get;
            set;
        }
    }
}