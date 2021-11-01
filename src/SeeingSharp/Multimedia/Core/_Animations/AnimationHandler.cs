using System;

namespace SeeingSharp.Multimedia.Core
{
    public class AnimationHandler : AnimationSequence
    {
        /// <summary>
        /// Gets the owner object.
        /// </summary>
        public object Owner { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationHandler"/> class.
        /// </summary>
        /// <param name="owner">The owner object of this AnimationHandler.</param>
        public AnimationHandler(object owner)
        {
            this.Owner = owner;
        }

        /// <summary>
        /// Called when an animation throws an exception during execution.
        /// </summary>
        /// <param name="animation">The failed animation.</param>
        /// <param name="ex">The exception thrown.</param>
        protected override AnimationFailedReaction OnAnimationFailed(IAnimation animation, Exception ex)
        {
            return AnimationFailedReaction.RemoveAndContinue;
        }

        /// <summary>
        /// Starts building an animation sequence for the current target object.
        /// </summary>
        internal IAnimationSequenceBuilder<TTargetObject> BuildAnimationSequence<TTargetObject>()
            where TTargetObject : class, IAnimatableObject
        {
            return new AnimationSequenceBuilder<TTargetObject>(this);
        }

        /// <summary>
        /// Starts building an animation sequence for the current target object.
        /// </summary>
        /// <param name="animatedObject">The target object which is to be animated.</param>
        internal IAnimationSequenceBuilder<TTargetObject> BuildAnimationSequence<TTargetObject>(TTargetObject animatedObject)
            where TTargetObject : class
        {
            return new AnimationSequenceBuilder<TTargetObject>(this, animatedObject);
        }
    }
}