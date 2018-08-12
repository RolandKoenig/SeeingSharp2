#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
using SeeingSharp.Multimedia.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SeeingSharp.Multimedia.Core
{
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
