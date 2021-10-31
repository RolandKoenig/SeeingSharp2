﻿/*
    SeeingSharp and all applications distributed together with it. 
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
namespace SeeingSharp.Multimedia.Core
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