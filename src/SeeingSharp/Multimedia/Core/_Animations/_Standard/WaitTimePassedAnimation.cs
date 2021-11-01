using System;

namespace SeeingSharp.Multimedia.Core
{
    public class WaitTimePassedAnimation : AnimationBase
    {
        /// <summary>
        /// Is this animation a blocking animation?
        /// If true, all following animation have to wait for finish-event.
        /// </summary>
        public override bool IsBlockingAnimation => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitFinishedAnimation" /> class.
        /// </summary>
        public WaitTimePassedAnimation(TimeSpan timeToWait)
            : base(null, AnimationType.FixedTime, timeToWait)
        {

        }
    }
}