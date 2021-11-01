using System;
using System.Numerics;

namespace SeeingSharp.Multimedia.Core
{
    public class Move3DByAnimation : AnimationBase
    {
        // Parameters
        private IAnimatableObjectPosition _targetObject;
        private Vector3 _moveVector;
        private MovementAnimationHelper _moveHelper;

        // Runtime values
        private Vector3 _targetVector;
        private Vector3 _startVector;

        /// <summary>
        /// Initializes a new instance of the <see cref="Move3DByAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="moveVector">The move vector.</param>
        /// <param name="duration">The duration.</param>
        public Move3DByAnimation(IAnimatableObjectPosition targetObject, Vector3 moveVector, TimeSpan duration)
            : base(targetObject)
        {
            _targetObject = targetObject;
            _moveVector = moveVector;
            _moveHelper = new MovementAnimationHelper(
                new MovementSpeed(moveVector, duration),
                moveVector);

            // Switch animation to fixed-time type
            this.ChangeToFixedTime(_moveHelper.MovementTime);
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
            _targetObject = targetObject;
            _moveVector = moveVector;
            _moveHelper = new MovementAnimationHelper(moveSpeed, moveVector);

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
            _targetObject.Position = _startVector + _moveHelper.GetPartialMoveDistance(this.CurrentTime);
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// (Sets final state to the target object and clears all runtime values).
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            _targetObject.Position = _targetVector;
            _targetVector = Vector3.Zero;
            _startVector = Vector3.Zero;
        }
    }
}