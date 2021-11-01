using System;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public static class AnimatableObjectExtensions
    {
        /// <summary>
        /// Moves the given Camera to the given ViewPoint.
        /// </summary>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetViewPoint">The target ViewPoint object.</param>
        /// <param name="animationTime">Total time for the animation.</param>
        public static IAnimationSequenceBuilder<TTargetObject> CameraStraightMoveTo<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, Camera3DViewPoint targetViewPoint, TimeSpan animationTime)
            where TTargetObject : Camera3DBase
        {
            sequenceBuilder.Add(
                new CameraStraightMoveAnimation(sequenceBuilder.TargetObject, targetViewPoint, animationTime));
            return sequenceBuilder;
        }
    }
}
