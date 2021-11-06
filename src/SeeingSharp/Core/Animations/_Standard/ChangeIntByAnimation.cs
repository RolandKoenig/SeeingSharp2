using System;

namespace SeeingSharp.Core.Animations
{
    public class ChangeIntByAnimation : AnimationBase
    {
        // Members for running animation
        private int _alreadyIncreased;

        // Configuration members
        private Func<int> _getValueFunc;
        private Action<int> _setValueAction;
        private int _increaseTotal;

        /// <summary>
        /// Is this animation a blocking animation?
        /// If true, all following animation have to wait for finish-event.
        /// </summary>
        public override bool IsBlockingAnimation => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeIntByAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="getValueFunc">The get value func.</param>
        /// <param name="setValueAction">The set value action.</param>
        /// <param name="increaseTotal">The increase total.</param>
        /// <param name="timeSpan">The timespan.</param>
        public ChangeIntByAnimation(object targetObject, Func<int> getValueFunc, Action<int> setValueAction, int increaseTotal, TimeSpan timeSpan)
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
            _alreadyIncreased = 0;
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            var currentLocationPercent = (float)(this.CurrentTime.TotalMilliseconds / this.FixedTime.TotalMilliseconds);
            var toIncreaseTotal = (int)(_increaseTotal * currentLocationPercent);
            var toIncrease = toIncreaseTotal - _alreadyIncreased;

            _setValueAction(_getValueFunc() + toIncrease);

            _alreadyIncreased = toIncreaseTotal;
        }
    }
}