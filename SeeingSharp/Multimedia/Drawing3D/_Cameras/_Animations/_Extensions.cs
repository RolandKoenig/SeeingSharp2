/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
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
