using System;
using SeeingSharp.Checking;

namespace SeeingSharp.Core.Animations
{
    public class ChangeAccentuationToAnimation : AnimationBase
    {
        // Parameters
        private float _startAccentuation;
        private float _moveAccentuation;
        private float _targetAccentuation;
        private IAnimatableObjectAccentuation _targetObject;

        /// <summary>
        /// Initialize a new Instance of the <see cref="ChangeAccentuationToAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetAccentuation">The target accentuation.</param>
        /// <param name="duration">The duration.</param>
        /// <exception cref="System.Exception">Accentuation value can be between 0 and 1, not greater than 1 and not lower than 0!</exception>
        public ChangeAccentuationToAnimation(IAnimatableObjectAccentuation targetObject, float targetAccentuation, TimeSpan duration)
            : base(targetObject, AnimationType.FixedTime, duration)
        {
            targetObject.EnsureNotNull(nameof(targetObject));
            targetAccentuation.EnsureInRange(0f, 1f, nameof(targetAccentuation));
            duration.EnsureLongerThanZero(nameof(duration));

            _targetObject = targetObject;
            _targetAccentuation = targetAccentuation;

            if (targetAccentuation < 0f || targetAccentuation > 1f)
            {
                throw new Exception("Accentuation value can be between 0 and 1, not greater than 1 and not lower than 0!");
            }
        }

        /// <summary>
        /// Called when animation starts.
        /// </summary>
        protected override void OnStartAnimation()
        {
            _startAccentuation = _targetObject.AccentuationFactor;
            _moveAccentuation = _targetAccentuation - _startAccentuation;
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            var changeFactor = this.CurrentTime.Ticks / (float)this.FixedTime.Ticks;
            _targetObject.AccentuationFactor = _startAccentuation + _moveAccentuation * changeFactor;
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// (Sets final state to the target object and clears all runtime values).
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            _targetObject.AccentuationFactor = _targetAccentuation;

            _moveAccentuation = 0;
            _startAccentuation = 1;
        }
    }
}