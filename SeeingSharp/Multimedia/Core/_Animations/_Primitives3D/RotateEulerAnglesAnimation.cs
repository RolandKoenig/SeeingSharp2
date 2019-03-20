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
using SharpDX;

namespace SeeingSharp.Multimedia.Core
{
    public class RotateEulerAnglesAnimation : AnimationBase
    {
        // Parameters
        private IAnimatableObjectEulerRotation m_targetObject;
        private RotationCalculationComponent m_calculationComponents;
        private AnimationStateChangeMode m_stateChangeMode;
        private Vector3 m_paramRotation;
        private TimeSpan m_duration;

        // Runtime values
        private Vector3 m_startRotation;
        private Vector3 m_targetRotation;
        private Vector3 m_changeRotation;

        /// <summary>
        /// Rotates the object to the target rotation vector.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetVector">The target rotation vector.</param>
        /// <param name="duration">Total duration of the animation.</param>
        /// <param name="calculationComponent">The components which are to be modified.</param>
        /// <param name="stateChangeMode">The state-change mode (to or by).</param>
        public RotateEulerAnglesAnimation(
            IAnimatableObjectEulerRotation targetObject, Vector3 targetVector, TimeSpan duration,
            RotationCalculationComponent calculationComponent = RotationCalculationComponent.All,
            AnimationStateChangeMode stateChangeMode = AnimationStateChangeMode.ChangeStateTo)
            : base(targetObject, AnimationType.FixedTime, duration)
        {
            m_targetObject = targetObject;
            m_paramRotation = targetVector;
            m_duration = duration;
            m_calculationComponents = calculationComponent;
            m_stateChangeMode = stateChangeMode;
        }

        /// <summary>
        /// Called when animation starts.
        /// </summary>
        protected override void OnStartAnimation()
        {
            // Prepare this animation
            m_startRotation = m_targetObject.RotationEuler;
            switch(m_stateChangeMode)
            {
                case AnimationStateChangeMode.ChangeStateTo:
                    m_changeRotation = m_paramRotation - m_startRotation;
                    m_targetRotation = m_paramRotation;
                    break;

                case AnimationStateChangeMode.ChangeStateBy:
                    m_changeRotation = m_paramRotation;
                    m_targetRotation = m_startRotation + m_changeRotation;
                    break;

                default:
                    throw new SeeingSharpGraphicsException("Unknown AnimationStateChangeMode in RotateEulerAnglesAnimation: " + m_stateChangeMode + "!");
            }


            // Some optimization logic to take the shortest way
            //  => e. g. object rotation 45° instead of 315°
            if (m_changeRotation.X > EngineMath.RAD_180DEG) { m_changeRotation.X = -(m_changeRotation.X - EngineMath.RAD_180DEG); }
            if (m_changeRotation.Y > EngineMath.RAD_180DEG) { m_changeRotation.Y = -(m_changeRotation.Y - EngineMath.RAD_180DEG); }
            if (m_changeRotation.Z > EngineMath.RAD_180DEG) { m_changeRotation.Z = -(m_changeRotation.Z - EngineMath.RAD_180DEG); }
            if (m_changeRotation.X < -EngineMath.RAD_180DEG) { m_changeRotation.X = -(m_changeRotation.X + EngineMath.RAD_180DEG); }
            if (m_changeRotation.Y < -EngineMath.RAD_180DEG) { m_changeRotation.Y = -(m_changeRotation.Y + EngineMath.RAD_180DEG); }
            if (m_changeRotation.Z < -EngineMath.RAD_180DEG) { m_changeRotation.Z = -(m_changeRotation.Z + EngineMath.RAD_180DEG); }

            // Set components to zero which should not not be changed using this animation
            if(!m_calculationComponents.HasFlag(RotationCalculationComponent.Pitch))
            {
                m_changeRotation.X = 0f;
            }
            if (!m_calculationComponents.HasFlag(RotationCalculationComponent.Yaw))
            {
                m_changeRotation.Y = 0f;
            }
            if (!m_calculationComponents.HasFlag(RotationCalculationComponent.Roll))
            {
                m_changeRotation.Z = 0f;
            }
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            var percentagePassed = this.CurrentTime.Ticks / (float)m_duration.Ticks;
            m_targetObject.RotationEuler = m_startRotation + m_changeRotation * percentagePassed;
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            m_targetObject.RotationEuler = m_targetRotation;
            m_startRotation = Vector3.Zero;
            m_changeRotation = Vector3.Zero;
            m_targetRotation = Vector3.Zero;
        }
    }
}