using System;

namespace SeeingSharp.Core
{
    public class ScaleSpriteToAnimation : AnimationBase
    {
        // Parameters
        private IAnimatableObjectSprite _targetObject;
        private float _targetScaling;

        // Runtime
        private float _startScaling;
        private float _moveScaling;

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
            _targetObject = targetObject;
            _targetScaling = targetScaling;

            if (targetScaling < 0f)
            {
                throw new Exception("Scaling value can be less than 0!");
            }
        }

        /// <summary>
        /// Called when animation starts.
        /// </summary>
        protected override void OnStartAnimation()
        {
            _startScaling = _targetObject.Scaling;
            _moveScaling = _targetScaling - _startScaling;
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            var changeFactor = this.CurrentTime.Ticks / (float)this.FixedTime.Ticks;
            _targetObject.Scaling = _startScaling + _moveScaling * changeFactor;
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// (Sets final state to the target object and clears all runtime values).
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            _targetObject.Scaling = _targetScaling;

            _moveScaling = 0;
            _startScaling = 1;
        }
    }
}