#region License information
/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
#endregion
namespace SeeingSharp.Multimedia.Core
{
    #region using

    using System;
    using SharpDX;

    #endregion

    public static partial class Drawing3DAnimationExtensions
    {
        /// <summary>
        /// Scales current object by the given move vector.
        /// </summary>
        /// <typeparam name="TargetObject">The type of the arget object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="scaleVector">The scale vector.</param>
        /// <param name="animationTime">Total time for the animation.</param>
        /// <returns></returns>
        public static IAnimationSequenceBuilder<TargetObject> Scale3DTo<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, Vector3 scaleVector, TimeSpan animationTime)
            where TargetObject : class, IAnimatableObjectScaling
        {
            sequenceBuilder.Add(
                new Scale3DToAnimation(sequenceBuilder.TargetObject, scaleVector, animationTime));
            return sequenceBuilder;
        }

        /// <summary>
        /// Scales current object by the given move vector.
        /// </summary>
        /// <typeparam name="TargetObject">The type of the arget object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetScaling">The targing scaling factor.</param>
        /// <param name="animationTime">Total time for the animation.</param>
        /// <returns></returns>
        public static IAnimationSequenceBuilder<TargetObject> Scale3DTo<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, float targetScaling, TimeSpan animationTime)
            where TargetObject : class, IAnimatableObjectScaling
        {
            sequenceBuilder.Add(
                new Scale3DToAnimation(
                    sequenceBuilder.TargetObject,
                    new Vector3(targetScaling, targetScaling, targetScaling),
                    animationTime));
            return sequenceBuilder;
        }

        public static IAnimationSequenceBuilder<TargetObject> ScaleTo<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, float targetScaling, TimeSpan animationTime)
            where TargetObject : class, IAnimatableObjectSprite
        {
            sequenceBuilder.Add(
                new ScaleSpriteToAnimation(sequenceBuilder.TargetObject, targetScaling, animationTime));
            return sequenceBuilder;
        }
    }
}