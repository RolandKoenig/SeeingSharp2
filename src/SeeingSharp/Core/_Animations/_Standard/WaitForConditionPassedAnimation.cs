using System;
using SeeingSharp.Checking;

namespace SeeingSharp.Core
{
    public class WaitForConditionPassedAnimation : AnimationBase
    {
        private Func<bool> _checkFunction;

        /// <summary>
        /// Is this animation a blocking animation?
        /// If true, all following animation have to wait for finish-event.
        /// </summary>
        public override bool IsBlockingAnimation => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitForConditionPassedAnimation" /> class.
        /// </summary>
        public WaitForConditionPassedAnimation(Func<bool> checkFunction)
            : base(null)
        {
            checkFunction.EnsureNotNull(nameof(checkFunction));

            _checkFunction = checkFunction;
        }

        /// <summary>
        /// Gets the time in milliseconds till this animation is finished.
        /// This method is relevant for event-driven processing and tells the system by what amount the clock is to be increased next.
        /// </summary>
        /// <param name="previousMinFinishTime">The minimum TimeSpan previous animations take.</param>
        /// <param name="previousMaxFinishTime">The maximum TimeSpan previous animations take.</param>
        /// <param name="defaultCycleTime">The default cycle time if we would be in continuous calculation mode.</param>
        public override TimeSpan GetTimeTillNextEvent(TimeSpan previousMinFinishTime, TimeSpan previousMaxFinishTime, TimeSpan defaultCycleTime)
        {
            return defaultCycleTime;
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        /// <param name="updateState"></param>
        /// <param name="animationState"></param>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            if (_checkFunction())
            {
                this.NotifyAnimationFinished();
            }
        }
    }
}