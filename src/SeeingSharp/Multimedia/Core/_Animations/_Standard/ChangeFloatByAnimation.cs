using System;

namespace SeeingSharp.Multimedia.Core
{
    public class ChangeFloatByAnimation : AnimationBase
    {
        // Members for running animation
        private float _alreadyIncreased;

        // Configuration members
        private Func<float> _getValueFunc;
        private float _increaseTotal;
        private Action<float> _setValueAction;

        /// <summary>
        /// Is this animation a blocking animation?
        /// If true, all following animation have to wait for finish-event.
        /// </summary>
        public override bool IsBlockingAnimation => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeFloatByAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="getValueFunc">The get value func.</param>
        /// <param name="setValueAction">The set value action.</param>
        /// <param name="increaseTotal">The increase total.</param>
        /// <param name="timeSpan">The timespan.</param>
        public ChangeFloatByAnimation(object targetObject, Func<float> getValueFunc, Action<float> setValueAction, float increaseTotal, TimeSpan timeSpan)
            : base(targetObject, AnimationType.FixedTime, timeSpan)
        {
            _getValueFunc = getValueFunc;
            _setValueAction = setValueAction;
            _increaseTotal = increaseTotal;
        }

        /// <summary>
        /// Called when animation starts.
        /// </summary>
        protected override void OnStartAnimation()
        {
            _alreadyIncreased = 0f;
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            var currentLocationPercent = (float)(this.CurrentTime.TotalMilliseconds / this.FixedTime.TotalMilliseconds);
            var toIncreaseTotal = _increaseTotal * currentLocationPercent;
            var toIncrease = toIncreaseTotal - _alreadyIncreased;

            _setValueAction(_getValueFunc() + toIncrease);

            _alreadyIncreased = toIncreaseTotal;
        }
    }
}