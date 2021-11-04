namespace SeeingSharp.Core
{
    public static class AnimatableObjectExtensions
    {
        /// <summary>
        /// Starts building an AnimationSequence for this object.
        /// </summary>
        /// <typeparam name="TTargetObjectType">The type of the target object.</typeparam>
        /// <param name="animatableObject">The object to be animated.</param>
        public static IAnimationSequenceBuilder<TTargetObjectType> BuildAnimationSequence<TTargetObjectType>(this TTargetObjectType animatableObject)
            where TTargetObjectType : class, IAnimatableObject
        {
            return animatableObject.AnimationHandler.BuildAnimationSequence<TTargetObjectType>();
        }

        /// <summary>
        /// Starts building an AnimationSequence for the given object.
        /// The generated animation is managed by this object.
        /// </summary>
        /// <typeparam name="TTargetObjectType">The type of the target object.</typeparam>
        /// <param name="animationHost">The host object of the animation.</param>
        /// <param name="animatableObject">The object to be animated.</param>
        public static IAnimationSequenceBuilder<TTargetObjectType> BuildAnimationSequence<TTargetObjectType>(this IAnimatableObject animationHost, TTargetObjectType animatableObject)
            where TTargetObjectType : class
        {
            return animationHost.AnimationHandler.BuildAnimationSequence(animatableObject);
        }

        /// <summary>
        /// Checks whether the given animation is finished or canceled.
        /// </summary>
        /// <param name="animation">The animation to be checked.</param>
        public static bool IsFinishedOrCanceled(this IAnimation animation)
        {
            return animation.Finished || animation.Canceled;
        }
    }
}
