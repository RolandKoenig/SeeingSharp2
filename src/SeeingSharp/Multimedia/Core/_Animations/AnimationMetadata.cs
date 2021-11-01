using System;

namespace SeeingSharp.Multimedia.Core
{
    public class AnimationMetadata
    {
        /// <summary>
        /// Gets the animation.
        /// </summary>
        public IAnimation Animation
        {
            get;
        }

        /// <summary>
        /// Gets the finished callback.
        /// </summary>
        public Action FinishedCallback
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationMetadata"/> class.
        /// </summary>
        /// <param name="animation">The animation.</param>
        /// <param name="finishedCallback">The finished callback.</param>
        public AnimationMetadata(IAnimation animation, Action finishedCallback)
        {
            this.Animation = animation;
            this.FinishedCallback = finishedCallback;
        }
    }
}
