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
using System.Numerics;

namespace SeeingSharp.Multimedia.Core
{
    public class RotateQuaternionToAnimation : AnimationBase
    {
        // Parameters
        private IAnimatableObjectQuaternion m_targetObject;
        private Quaternion m_startQuaternion;
        private Quaternion m_targetQuaternion;

        /// <summary>
        /// Initializes a new instance of the <see cref="RotateQuaternionToAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetQuaternion">The target quaternion.</param>
        /// <param name="timeSpan">The time span.</param>
        public RotateQuaternionToAnimation(IAnimatableObjectQuaternion targetObject, Quaternion targetQuaternion, TimeSpan timeSpan)
            : base(targetObject, AnimationType.FixedTime, timeSpan)
        {
            m_targetObject = targetObject;
            m_targetQuaternion = targetQuaternion;
        }

        /// <summary>
        /// Called when animation starts.
        /// </summary>
        protected override void OnStartAnimation()
        {
            m_startQuaternion = m_targetObject.RotationQuaternion;
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        /// <param name="updateState"></param>
        /// <param name="animationState"></param>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            //how does Slerp work: --> http://en.wikipedia.org/wiki/Slerp
            var changeFactor = this.CurrentTime.Ticks / (float)this.FixedTime.Ticks;

            var slerpQuaternion = Quaternion.Slerp(m_startQuaternion, m_targetQuaternion, changeFactor);
            m_targetObject.RotationQuaternion = slerpQuaternion;
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// (Sets final state to the target object and clears all runtime values).
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            m_targetObject.RotationQuaternion = m_targetQuaternion;
        }
    }
}