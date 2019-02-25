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

    public class Move2DToAnimation : AnimationBase
    {
        #region Parameters
        private IAnimatableObjectPosition2D m_targetObject;
        private Vector2 m_targetVector;
        private TimeSpan m_paramDuration;
        private MovementSpeed m_paramMoveSpeed;
        #endregion

        #region Runtime values
        private MovementAnimationHelper m_moveHelper;
        private Vector2 m_startVector;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Move2DByAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetVector">The target position of the object.</param>
        /// <param name="duration">The duration.</param>
        public Move2DToAnimation(IAnimatableObjectPosition2D targetObject, Vector2 targetVector, TimeSpan duration)
            : base(targetObject)
        {
            m_targetObject = targetObject;
            m_targetVector = targetVector;
            m_paramDuration = duration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Move2DByAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetVector">The target position of the object.</param>
        ///´<param name="speed">The total movement speed.</param>
        public Move2DToAnimation(IAnimatableObjectPosition2D targetObject, Vector2 targetVector, MovementSpeed speed)
            : base(targetObject)
        {
            m_targetObject = targetObject;
            m_targetVector = targetVector;
            m_paramMoveSpeed = speed;
        }

        /// <summary>
        /// Called when animation starts.
        /// </summary>
        protected override void OnStartAnimation()
        {
            m_startVector = m_targetObject.Position;
            Vector3 moveVector = new Vector3(m_targetVector - m_startVector, 0f);

            // Create move-helper individually
            if (m_paramDuration > TimeSpan.Zero)
            {
                m_moveHelper = new MovementAnimationHelper(
                    new MovementSpeed(moveVector, m_paramDuration),
                    moveVector);
            }
            else if(m_paramMoveSpeed != MovementSpeed.Empty)
            {
                m_moveHelper = new MovementAnimationHelper(m_paramMoveSpeed, moveVector);
            }
            else
            {
                m_moveHelper = new MovementAnimationHelper(
                    new MovementSpeed(moveVector, TimeSpan.FromMilliseconds(1.0)),
                    moveVector);
            }

            // Change the type of this animation in the base class
            base.ChangeToFixedTime(m_moveHelper.MovementTime);
        }

        /// <summary>
        /// Resets this animation.
        /// </summary>
        public override void OnReset()
        {
            base.OnReset();
            base.ChangeToEventBased();
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            m_targetObject.Position = m_startVector + Vector3Ex.GetXY(m_moveHelper.GetPartialMoveDistance(base.CurrentTime));
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// (Sets final state to the target object and clears all runtime values).
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            m_targetObject.Position = m_targetVector;
            m_startVector = Vector2.Zero;
            m_moveHelper = null;
        }
    }
}