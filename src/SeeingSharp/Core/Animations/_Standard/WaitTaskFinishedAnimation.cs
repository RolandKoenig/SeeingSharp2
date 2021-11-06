using System.Threading.Tasks;

namespace SeeingSharp.Core.Animations
{
    public class WaitTaskFinishedAnimation : AnimationBase
    {
        private Task _taskToWaitFor;

        /// <summary>
        /// Is this animation a blocking animation?
        /// If true, all following animation have to wait for finish-event.
        /// </summary>
        public override bool IsBlockingAnimation => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitTaskFinishedAnimation" /> class.
        /// </summary>
        public WaitTaskFinishedAnimation(Task taskToWaitFor)
            : base(null, AnimationType.FinishedByEvent)
        {
            _taskToWaitFor = taskToWaitFor;
        }

        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            base.OnCurrentTimeUpdated(updateState, animationState);

            if (_taskToWaitFor.IsCanceled ||
               _taskToWaitFor.IsCompleted ||
               _taskToWaitFor.IsFaulted)
            {
                this.NotifyAnimationFinished();
            }
        }
    }
}