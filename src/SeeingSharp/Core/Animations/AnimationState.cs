namespace SeeingSharp.Core.Animations
{
    public class AnimationState
    {
        /// <summary>
        /// Gets the index within the collection of running animations.
        /// </summary>
        public int RunningAnimationsIndex { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationState" /> class.
        /// </summary>
        public AnimationState()
        {
            this.RunningAnimationsIndex = 0;
        }
    }
}