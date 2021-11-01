using System;

namespace SeeingSharp.Multimedia.Core
{
    public class CallActionAnimation : AnimationBase
    {
        private Action _actionToCall;
        private Action _cancelAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallActionAnimation" /> class.
        /// </summary>
        public CallActionAnimation(Action actionToCall)
            : base(null, AnimationType.FixedTime, TimeSpan.Zero)
        {
            _actionToCall = actionToCall;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CallActionAnimation"/> class.
        /// </summary>
        /// <param name="actionToCall">The action to call.</param>
        /// <param name="cancelAction">The cancel action.</param>
        public CallActionAnimation(Action actionToCall, Action cancelAction)
            : base(null, AnimationType.FixedTime, TimeSpan.Zero)
        {
            _actionToCall = actionToCall;
            _cancelAction = cancelAction;
        }

        /// <summary>
        /// Called when this animation was canceled.
        /// </summary>
        public override void OnCanceled()
        {
            _cancelAction?.Invoke();
        }

        /// <summary>
        /// Called when this animation was finished.
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            _actionToCall?.Invoke();
        }
    }
}