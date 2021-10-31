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
using System.Numerics;

namespace SeeingSharp.Multimedia.Core
{
    public class Move2DByAnimation : AnimationBase
    {
        // Parameters
        private IAnimatableObjectPosition2D _targetObject;
        private Vector2 _moveVector;
        private MovementAnimationHelper _moveHelper;

        // Runtime values
        private Vector2 _targetVector;
        private Vector2 _startVector;

        /// <summary>
        /// Initializes a new instance of the <see cref="Move2DByAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="moveVector">The move vector.</param>
        /// <param name="duration">The duration.</param>
        public Move2DByAnimation(IAnimatableObjectPosition2D targetObject, Vector2 moveVector, TimeSpan duration)
            : base(targetObject)
        {
            _targetObject = targetObject;
            _moveVector = moveVector;
            _moveHelper = new MovementAnimationHelper(
                new MovementSpeed(new Vector3(moveVector, 0f), duration),
                new Vector3(moveVector, 0f));

            // Switch animation to fixed-time type
            this.ChangeToFixedTime(_moveHelper.MovementTime);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Move2DByAnimation"/> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="moveVector">The move vector.</param>
        /// <param name="moveSpeed">The speed which is used for movement calculation.</param>
        public Move2DByAnimation(IAnimatableObjectPosition2D targetObject, Vector2 moveVector, MovementSpeed moveSpeed)
            : base(targetObject)
        {
            _targetObject = targetObject;
            _moveVector = moveVector;
            _moveHelper = new MovementAnimationHelper(moveSpeed, new Vector3(moveVector, 0f));

            // Switch animation to fixed-time type
            this.ChangeToFixedTime(_moveHelper.MovementTime);
        }

        /// <summary>
        /// Called when animation starts.
        /// (Checks the target object for compatibility and initializes runtime values).
        /// </summary>
        protected override void OnStartAnimation()
        {
            _targetVector = _targetObject.Position + _moveVector;
            _startVector = _targetObject.Position;
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            _targetObject.Position = _startVector + Vector3Ex.GetXY(_moveHelper.GetPartialMoveDistance(this.CurrentTime));
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// (Sets final state to the target object and clears all runtime values).
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            _targetObject.Position = _targetVector;
            _targetVector = Vector2.Zero;
            _startVector = Vector2.Zero;
        }
    }
}