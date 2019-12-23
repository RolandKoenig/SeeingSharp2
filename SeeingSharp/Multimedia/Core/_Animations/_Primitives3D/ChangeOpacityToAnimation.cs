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
using SeeingSharp.Checking;
using System;

namespace SeeingSharp.Multimedia.Core
{
    public class ChangeOpacityToAnimation : AnimationBase
    {
        // Parameters
        private float m_startOpacity;
        private TimeSpan m_duration;
        private float m_moveOpacity;
        private float m_targetOpacity;
        private IAnimatableObjectOpacity m_targetObject;

        /// <summary>
        /// Initialize a new Instance of the <see cref="Move3DByAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetOpacity">The target opacity.</param>
        /// <param name="duration">The duration.</param>
        /// <exception cref="System.Exception">Opacity value can be between 0 and 1, not greater than 1 and not lower than 0!</exception>
        public ChangeOpacityToAnimation(IAnimatableObjectOpacity targetObject, float targetOpacity, TimeSpan duration)
            : base(targetObject, AnimationType.FixedTime, duration)
        {
            targetObject.EnsureNotNull(nameof(targetObject));
            targetOpacity.EnsureInRange(0f, 1f, nameof(targetOpacity));
            duration.EnsureLongerThanZero(nameof(duration));

            m_targetObject = targetObject;
            m_duration = duration;
            m_targetOpacity = targetOpacity;

            if (targetOpacity < 0f || targetOpacity > 1f)
            {
                throw new Exception("Opacity value can be between 0 and 1, not greater than 1 and not lower than 0!");
            }
        }

        /// <summary>
        /// Called when animation starts.
        /// </summary>
        protected override void OnStartAnimation()
        {
            m_startOpacity = m_targetObject.Opacity;
            m_moveOpacity = m_targetOpacity - m_startOpacity;
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            var changeFactor = this.CurrentTime.Ticks / (float)this.FixedTime.Ticks;
            m_targetObject.Opacity = m_startOpacity + m_moveOpacity * changeFactor;
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// (Sets final state to the target object and clears all runtime values).
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            m_targetObject.Opacity = m_targetOpacity;

            m_moveOpacity = 0;
            m_startOpacity = 1;
        }
    }
}