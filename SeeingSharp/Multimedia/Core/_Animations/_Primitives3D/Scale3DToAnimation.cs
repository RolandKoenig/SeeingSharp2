#region License information
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
    using SharpDX;

    #endregion

    public class Scale3DToAnimation : AnimationBase
    {
        #region Parameters
        private IAnimatableObjectScaling m_targetObject;
        private Vector3 m_targetScaleVector;
        private TimeSpan m_duration;
        #endregion

        #region Runtime values
        private Vector3 m_startScaleVector;
        private Vector3 m_differenceVector;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Move3DByAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="scaleVector">The move vector.</param>
        /// <param name="duration">The duration.</param>
        public Scale3DToAnimation(IAnimatableObjectScaling targetObject, Vector3 scaleVector, TimeSpan duration)
            : base(targetObject, AnimationType.FixedTime, duration)
        {
            m_targetObject = targetObject;
            m_targetScaleVector = scaleVector;
            m_duration = duration;
        }

        /// <summary>
        /// Called when animation starts.
        /// </summary>
        protected override void OnStartAnimation()
        {
            m_startScaleVector = m_targetObject.Scaling;

            m_differenceVector = m_targetScaleVector - m_startScaleVector;
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            float scaleFactor = (float)base.CurrentTime.Ticks / (float)base.FixedTime.Ticks;

            m_targetObject.Scaling = m_startScaleVector + m_differenceVector * scaleFactor;
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            m_targetObject.Scaling = m_targetScaleVector;
            m_startScaleVector = Vector3.Zero;
            m_differenceVector = Vector3.Zero;
        }
    }
}