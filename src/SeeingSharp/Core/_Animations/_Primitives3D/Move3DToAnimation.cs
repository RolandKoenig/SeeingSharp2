using System;
using System.Numerics;

namespace SeeingSharp.Core
{
    public class Move3DToAnimation : AnimationBase
    {
        // Parameters
        private IAnimatableObjectPosition _targetObject;
        private Vector3 _targetVector;
        private TimeSpan _paramDuration;
        private MovementSpeed _paramMoveSpeed;

        // Runtime values
        private MovementAnimationHelper _moveHelper;
        private Vector3 _startVector;

        /// <summary>
        /// Initializes a new instance of the <see cref="Move3DByAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetVector">The target position of the object.</param>
        /// <param name="duration">The duration.</param>
        public Move3DToAnimation(IAnimatableObjectPosition targetObject, Vector3 targetVector, TimeSpan duration)
            : base(targetObject)
        {
            _targetObject = targetObject;
            _targetVector = targetVector;
            _paramDuration = duration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Move3DByAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetVector">The target position of the object.</param>
        ///´<param name="speed">The total movement speed.</param>
        public Move3DToAnimation(IAnimatableObjectPosition targetObject, Vector3 targetVector, MovementSpeed speed)
            : base(targetObject)
        {
            _targetObject = targetObject;
            _targetVector = targetVector;
            _paramMoveSpeed = speed;
        }

        /// <summary>
        /// Resets this animation.
        /// </summary>
        public override void OnReset()
        {
            base.OnReset();
            this.ChangeToEventBased();
        }

        /// <summary>
        /// Called when animation starts.
        /// </summary>
        protected override void OnStartAnimation()
        {
            _startVector = _targetObject.Position;
            var moveVector = _targetVector - _startVector;

            // Create move-helper individually
            if (_paramDuration > TimeSpan.Zero)
            {
                _moveHelper = new MovementAnimationHelper(
                    new MovementSpeed(moveVector, _paramDuration),
                    moveVector);
            }
            else if (_paramMoveSpeed != MovementSpeed.Empty)
            {
                _moveHelper = new MovementAnimationHelper(_paramMoveSpeed, moveVector);
            }
            else
            {
                _moveHelper = new MovementAnimationHelper(
                    new MovementSpeed(moveVector, TimeSpan.FromMilliseconds(1.0)),
                    moveVector);
            }

            // Change the type of this animation in the base class
            this.ChangeToFixedTime(_moveHelper.MovementTime);
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
            _startVector = Vector3.Zero;
            _moveHelper = null;
        }
    }
}