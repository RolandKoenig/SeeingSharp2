using System;
using System.Numerics;

namespace SeeingSharp.Core
{
    public static partial class Drawing3DAnimationExtensions
    {
        /// <summary>
        /// Scales current object by the given move vector.
        /// </summary>
        /// <typeparam name="TTargetObject">The type of the target object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="scaleVector">The scale vector.</param>
        /// <param name="animationTime">Total time for the animation.</param>
        /// <returns></returns>
        public static IAnimationSequenceBuilder<TTargetObject> Scale3DTo<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, Vector3 scaleVector, TimeSpan animationTime)
            where TTargetObject : class, IAnimatableObjectScaling
        {
            sequenceBuilder.Add(
                new Scale3DToAnimation(sequenceBuilder.TargetObject, scaleVector, animationTime));
            return sequenceBuilder;
        }

        /// <summary>
        /// Scales current object by the given move vector.
        /// </summary>
        /// <typeparam name="TTargetObject">The type of the target object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetScaling">The target scaling factor.</param>
        /// <param name="animationTime">Total time for the animation.</param>
        /// <returns></returns>
        public static IAnimationSequenceBuilder<TTargetObject> Scale3DTo<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, float targetScaling, TimeSpan animationTime)
            where TTargetObject : class, IAnimatableObjectScaling
        {
            sequenceBuilder.Add(
                new Scale3DToAnimation(
                    sequenceBuilder.TargetObject,
                    new Vector3(targetScaling, targetScaling, targetScaling),
                    animationTime));
            return sequenceBuilder;
        }

        public static IAnimationSequenceBuilder<TTargetObject> ScaleTo<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, float targetScaling, TimeSpan animationTime)
            where TTargetObject : class, IAnimatableObjectSprite
        {
            sequenceBuilder.Add(
                new ScaleSpriteToAnimation(sequenceBuilder.TargetObject, targetScaling, animationTime));
            return sequenceBuilder;
        }
    }
}