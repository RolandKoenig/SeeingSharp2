using System;

namespace SeeingSharp.Multimedia.Core
{
    public class DelayAnimation : AnimationBase
    {
        /// <summary>
        /// Is this animation a blocking animation?
        /// If true, all following animation have to wait for finish-event.
        /// </summary>
        public override bool IsBlockingAnimation => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayAnimation"/> class.
        /// </summary>
        /// <param name="duration">The duration.</param>
        public DelayAnimation(TimeSpan duration)
            : base(null, AnimationType.FixedTime, duration)
        {

        }

        /// <summary>
        /// Gets the time in milliseconds till this animation is finished.
        /// This method is relevant for event-driven processing and tells the system by what amount the clock is to be increased next.
        /// </summary>
        /// <param name="previousMinFinishTime">The minimum TimeSpan previous animations take.</param>
        /// <param name="previousMaxFinishTime">The maximum TimeSpan previous animations take.</param>
        /// <param name="defaultCycleTime">The default cycle time if we would be in continuous calculation mode.</param>
        /// <returns></returns>
        public override TimeSpan GetTimeTillNextEvent(TimeSpan previousMinFinishTime, TimeSpan previousMaxFinishTime, TimeSpan defaultCycleTime)
        {
            return this.FixedTime;
        }
    }
}