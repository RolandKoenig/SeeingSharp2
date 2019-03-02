#region License information
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
#endregion

using System;

namespace SeeingSharp.Multimedia.Core
{
    #region using
    #endregion

    public class ScaleSpriteToAnimation : AnimationBase
    {
        /// <summary>
        /// Called when animation starts.
        /// </summary>
        protected override void OnStartAnimation()
        {
            m_startScaling = m_targetObject.Scaling;
            m_moveScaling = m_targetScaling - m_startScaling;
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            var changeFactor = CurrentTime.Ticks / (float)FixedTime.Ticks;
            m_targetObject.Scaling = m_startScaling + m_moveScaling * changeFactor;
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// (Sets final state to the target object and clears all runtime values).
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            m_targetObject.Scaling = m_targetScaling;

            m_moveScaling = 0;
            m_startScaling = 1;
        }

        /// <summary>
        /// Initialize a new Instance of the <see cref="ScaleSpriteToAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetScaling">The target scaling factor.</param>
        /// <param name="duration">The duration.</param>
        /// <exception cref="System.Exception">Opacity value can be between 0 and 1, not greater than 1 and not lower than 0!</exception>
        public ScaleSpriteToAnimation(IAnimatableObjectSprite targetObject, float targetScaling, TimeSpan duration)
            : base(targetObject, AnimationType.FixedTime, duration)
        {
            m_targetObject = targetObject;
            m_duration = duration;
            m_targetScaling = targetScaling;

            if (targetScaling < 0f)
            {
                throw new Exception("Scaling value can be less than 0!");
            }
        }

        #region Parameters
        private IAnimatableObjectSprite m_targetObject;
        private TimeSpan m_duration;
        private float m_targetScaling;
        #endregion Parameters

        #region Runtime
        private float m_startScaling;
        private float m_moveScaling;
        #endregion Runtime
    }
}