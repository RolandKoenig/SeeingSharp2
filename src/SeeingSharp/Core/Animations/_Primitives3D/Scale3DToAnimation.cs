using System;
using System.Numerics;

namespace SeeingSharp.Core.Animations
{
    public class Scale3DToAnimation : AnimationBase
    {
        // Parameters
        private IAnimatableObjectScaling _targetObject;
        private Vector3 _targetScaleVector;

        // Runtime values
        private Vector3 _startScaleVector;
        private Vector3 _differenceVector;

        /// <summary>
        /// Initializes a new instance of the <see cref="Move3DByAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="scaleVector">The move vector.</param>
        /// <param name="duration">The duration.</param>
        public Scale3DToAnimation(IAnimatableObjectScaling targetObject, Vector3 scaleVector, TimeSpan duration)
            : base(targetObject, AnimationType.FixedTime, duration)
        {
            _targetObject = targetObject;
            _targetScaleVector = scaleVector;
        }

        /// <summary>
        /// Called when animation starts.
        /// </summary>
        protected override void OnStartAnimation()
        {
            _startScaleVector = _targetObject.Scaling;

            _differenceVector = _targetScaleVector - _startScaleVector;
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            var scaleFactor = this.CurrentTime.Ticks / (float)this.FixedTime.Ticks;

            _targetObject.Scaling = _startScaleVector + _differenceVector * scaleFactor;
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            _targetObject.Scaling = _targetScaleVector;
            _startScaleVector = Vector3.Zero;
            _differenceVector = Vector3.Zero;
        }
    }
}