using System;

namespace SeeingSharp.Core
{
    public static partial class Drawing3DAnimationExtensions
    {
        public static IAnimationSequenceBuilder<TTargetObject> ChangeOpacityTo<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, float targetOpacity, TimeSpan animationTime)
            where TTargetObject : class, IAnimatableObjectOpacity
        {
            sequenceBuilder.Add(
                new ChangeOpacityToAnimation(sequenceBuilder.TargetObject, targetOpacity, animationTime));
            return sequenceBuilder;
        }

        public static IAnimationSequenceBuilder<TTargetObject> ChangeAccentuationFactorTo<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, float targetAccentuation, TimeSpan animationTime)
            where TTargetObject : class, IAnimatableObjectAccentuation
        {
            sequenceBuilder.Add(
                new ChangeAccentuationToAnimation(sequenceBuilder.TargetObject, targetAccentuation, animationTime));
            return sequenceBuilder;
        }
    }
}