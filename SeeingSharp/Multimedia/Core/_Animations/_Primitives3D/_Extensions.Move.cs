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
using System.Collections.Generic;
using SharpDX;

namespace SeeingSharp.Multimedia.Core
{
    public static partial class Drawing3DAnimationExtensions
    {
        /// <summary>
        /// Moves current object by the given move vector.
        /// </summary>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="moveVector">The move vector.</param>
        /// <param name="animationTime">Total time for the animation.</param>
        public static IAnimationSequenceBuilder<TargetObject> Move3DBy<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, Vector3 moveVector, TimeSpan animationTime)
            where TargetObject : class, IAnimatableObjectPosition
        {
            sequenceBuilder.Add(
                new Move3DByAnimation(sequenceBuilder.TargetObject, moveVector, animationTime));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object by the given move vector.
        /// </summary>
        /// <typeparam name="HostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TargetObject">The type of the arget object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetObject">The target object.</param>
        /// <param name="moveVector">The move vector.</param>
        /// <param name="animationTime">Total time for the animation.</param>
        public static IAnimationSequenceBuilder<HostObject> Move3DBy<HostObject, TargetObject>(this IAnimationSequenceBuilder<HostObject> sequenceBuilder, TargetObject targetObject, Vector3 moveVector, TimeSpan animationTime)
            where HostObject : class
            where TargetObject : class, IAnimatableObjectPosition
        {
            sequenceBuilder.Add(
                new Move3DByAnimation(targetObject, moveVector, animationTime));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object by the given move vector.
        /// </summary>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="moveVector">The move vector.</param>
        /// <param name="speed">Total speed of the movement animation.</param>
        public static IAnimationSequenceBuilder<TargetObject> Move3DBy<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, Vector3 moveVector, MovementSpeed speed)
            where TargetObject : class, IAnimatableObjectPosition
        {
            sequenceBuilder.Add(
                new Move3DByAnimation(sequenceBuilder.TargetObject, moveVector, speed));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object by the given move vector.
        /// </summary>
        /// <typeparam name="HostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TargetObject">The type of the arget object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetObject">The target object.</param>
        /// <param name="moveVector">The move vector.</param>
        /// <param name="speed">Total speed of the movement animation.</param>
        public static IAnimationSequenceBuilder<HostObject> Move3DBy<HostObject, TargetObject>(this IAnimationSequenceBuilder<HostObject> sequenceBuilder, TargetObject targetObject, Vector3 moveVector, MovementSpeed speed)
            where HostObject : class
            where TargetObject : class, IAnimatableObjectPosition
        {
            sequenceBuilder.Add(
                new Move3DByAnimation(targetObject, moveVector, speed));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object by the given move vector.
        /// </summary>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="moveVector">The move vector.</param>
        /// <param name="speed">Total speed of the movement animation.</param>
        public static IAnimationSequenceBuilder<TargetObject> Move3DBy<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, Vector3 moveVector, float speed)
            where TargetObject : class, IAnimatableObjectPosition
        {
            sequenceBuilder.Add(
                new Move3DByAnimation(sequenceBuilder.TargetObject, moveVector, new MovementSpeed(speed)));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object by the given move vector.
        /// </summary>
        /// <typeparam name="HostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TargetObject">The type of the arget object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetObject">The target object.</param>
        /// <param name="moveVector">The move vector.</param>
        /// <param name="speed">Total speed of the movement animation.</param>
        public static IAnimationSequenceBuilder<HostObject> Move3DBy<HostObject, TargetObject>(this IAnimationSequenceBuilder<HostObject> sequenceBuilder, TargetObject targetObject, Vector3 moveVector, float speed)
            where HostObject : class
            where TargetObject : class, IAnimatableObjectPosition
        {
            sequenceBuilder.Add(
                new Move3DByAnimation(targetObject, moveVector, new MovementSpeed(speed)));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object to the given target position.
        /// </summary>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetVector">The target position for the object.</param>
        /// <param name="animationTime">Total time for the animation.</param>
        public static IAnimationSequenceBuilder<TargetObject> Move3DTo<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, Vector3 targetVector, TimeSpan animationTime)
            where TargetObject : class, IAnimatableObjectPosition
        {
            sequenceBuilder.Add(
                new Move3DToAnimation(sequenceBuilder.TargetObject, targetVector, animationTime));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object to the given target position.
        /// </summary>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetVector">The target position for the object.</param>
        /// <param name="speed">Speed configuration for the movement.</param>
        public static IAnimationSequenceBuilder<TargetObject> Move3DTo<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, Vector3 targetVector, MovementSpeed speed)
            where TargetObject : class, IAnimatableObjectPosition
        {
            sequenceBuilder.Add(
                new Move3DToAnimation(sequenceBuilder.TargetObject, targetVector, speed));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object to the given target position.
        /// </summary>
        /// <typeparam name="TargetObject">The type of the arget object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetVector">The target position for the object.</param>
        /// <param name="speed">The speed for animation calculation.</param>
        /// <returns></returns>
        public static IAnimationSequenceBuilder<TargetObject> Move3DTo<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, Vector3 targetVector, float speed)
            where TargetObject : class, IAnimatableObjectPosition
        {
            sequenceBuilder.Add(
                new Move3DToAnimation(sequenceBuilder.TargetObject, targetVector, new MovementSpeed(speed)));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object to the given target position.
        /// </summary>
        /// <typeparam name="TargetObject">The type of the arget object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetVector">The target position for the object.</param>
        /// <param name="speed">The speed for animation calculation.</param>
        /// <param name="acceleration">The acceleration.</param>
        /// <param name="deceleration">The deceleration.</param>
        /// <returns></returns>
        public static IAnimationSequenceBuilder<TargetObject> Move3DTo<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, Vector3 targetVector, float speed, float acceleration, float deceleration)
            where TargetObject : class, IAnimatableObjectPosition
        {
            sequenceBuilder.Add(
                new Move3DToAnimation(sequenceBuilder.TargetObject, targetVector, new MovementSpeed(speed, acceleration, deceleration)));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object to the given target position.
        /// </summary>
        /// <typeparam name="HostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TargetObject">The type of the arget object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetVector">The target position for the object.</param>
        /// <param name="speed">The speed for animation calculation.</param>
        /// <returns></returns>
        public static IAnimationSequenceBuilder<HostObject> Move3DTo<HostObject, TargetObject>(this IAnimationSequenceBuilder<HostObject> sequenceBuilder, TargetObject targetObject, Vector3 targetVector, float speed)
            where TargetObject : class, IAnimatableObjectPosition
            where HostObject : class
        {
            sequenceBuilder.Add(
                new Move3DToAnimation(targetObject, targetVector, new MovementSpeed(speed)));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object to the given target position.
        /// </summary>
        /// <typeparam name="HostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TargetObject">The type of the arget object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetVector">The target position for the object.</param>
        /// <param name="speed">The speed for animation calculation.</param>
        /// <returns></returns>
        public static IAnimationSequenceBuilder<HostObject> Move3DTo<HostObject, TargetObject>(this IAnimationSequenceBuilder<HostObject> sequenceBuilder, TargetObject targetObject, Vector3 targetVector, MovementSpeed speed)
            where TargetObject : class, IAnimatableObjectPosition
            where HostObject : class
        {
            sequenceBuilder.Add(
                new Move3DToAnimation(targetObject, targetVector, speed));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object to the given target position.
        /// </summary>
        /// <typeparam name="HostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TargetObject">The type of the arget object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetObjects">A collection containing all target objects.</param>
        /// <param name="targetVector">The target position for the object.</param>
        /// <param name="speed">The speed for animation calculation.</param>
        /// <returns></returns>
        public static IAnimationSequenceBuilder<HostObject> Move3DTo<HostObject, TargetObject>(this IAnimationSequenceBuilder<HostObject> sequenceBuilder, IEnumerable<TargetObject> targetObjects, Vector3 targetVector, float speed)
            where TargetObject : class, IAnimatableObjectPosition
            where HostObject : class
        {
            foreach(var actTargetObject in targetObjects)
            {
                sequenceBuilder.Add(
                    new Move3DToAnimation(actTargetObject, targetVector, new MovementSpeed(speed)));
            }
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object to the given target position.
        /// </summary>
        /// <typeparam name="HostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TargetObject">The type of the arget object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetObjects">A collection containing all target objects.</param>
        /// <param name="targetVector">The target position for the object.</param>
        /// <param name="speed">The speed for animation calculation.</param>
        public static IAnimationSequenceBuilder<HostObject> Move3DTo<HostObject, TargetObject>(this IAnimationSequenceBuilder<HostObject> sequenceBuilder, IEnumerable<TargetObject> targetObjects, Vector3 targetVector, MovementSpeed speed)
            where TargetObject : class, IAnimatableObjectPosition
            where HostObject : class
        {
            foreach (var actTargetObject in targetObjects)
            {
                sequenceBuilder.Add(
                    new Move3DToAnimation(actTargetObject, targetVector, speed));
            }
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object to the given target position.
        /// </summary>
        /// <typeparam name="HostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TargetObject">The type of the arget object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetVector">The target position for the object.</param>
        /// <param name="speed">The speed for animation calculation.</param>
        /// <param name="acceleration">The acceleration.</param>
        /// <param name="deceleration">The deceleration.</param>
        public static IAnimationSequenceBuilder<HostObject> Move3DTo<HostObject, TargetObject>(this IAnimationSequenceBuilder<HostObject> sequenceBuilder, TargetObject targetObject, Vector3 targetVector, float speed, float acceleration, float deceleration)
            where TargetObject : class, IAnimatableObjectPosition
            where HostObject : class
        {
            sequenceBuilder.Add(
                new Move3DToAnimation(targetObject, targetVector, new MovementSpeed(speed, acceleration, deceleration)));
            return sequenceBuilder;
        }
    }
}