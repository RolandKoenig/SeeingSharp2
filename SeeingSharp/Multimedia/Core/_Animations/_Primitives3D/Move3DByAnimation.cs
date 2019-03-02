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
namespace SeeingSharp.Multimedia.Core
{
    #region using

    using System;
    using SharpDX;

    #endregion

    public class Move3DByAnimation : AnimationBase
    {
        #region Parameters
        private IAnimatableObjectPosition m_targetObject;
        private Vector3 m_moveVector;
        private MovementAnimationHelper m_moveHelper;
        #endregion

        #region Runtime values
        private Vector3 m_targetVector;
        private Vector3 m_startVector;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Move3DByAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="moveVector">The move vector.</param>
        /// <param name="duration">The duration.</param>
        public Move3DByAnimation(IAnimatableObjectPosition targetObject, Vector3 moveVector, TimeSpan duration)
            : base(targetObject)
        {
            m_targetObject = targetObject;
            m_moveVector = moveVector;
            m_moveHelper = new MovementAnimationHelper(
                new MovementSpeed(moveVector, duration),
                moveVector);

            // Switch animation to fixed-time type
            base.ChangeToFixedTime(m_moveHelper.MovementTime);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Move3DByAnimation"/> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="moveVector">The move vector.</param>
        /// <param name="moveSpeed">The speed which is used for movement calculation.</param>
        public Move3DByAnimation(IAnimatableObjectPosition targetObject, Vector3 moveVector, MovementSpeed moveSpeed)
            : base(targetObject)
        {
            m_targetObject = targetObject;
            m_moveVector = moveVector;
            m_moveHelper = new MovementAnimationHelper(moveSpeed, moveVector);

            // Switch animation to fixed-time type
            base.ChangeToFixedTime(m_moveHelper.MovementTime);
        }

        /// <summary>
        /// Called when animation starts.
        /// (Checks the target object for compatibility and initializes runtime values).
        /// </summary>
        protected override void OnStartAnimation()
        {
            m_targetVector = m_targetObject.Position + m_moveVector;
            m_startVector = m_targetObject.Position;
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            m_targetObject.Position = m_startVector + m_moveHelper.GetPartialMoveDistance(base.CurrentTime);
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// (Sets final state to the target object and clears all runtime values).
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            m_targetObject.Position = m_targetVector;
            m_targetVector = Vector3.Zero;
            m_startVector = Vector3.Zero;
        }
    }
}