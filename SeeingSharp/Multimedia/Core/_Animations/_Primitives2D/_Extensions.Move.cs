#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
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
    using System.Collections.Generic;
    using SharpDX;

    #endregion

    public static partial class Drawing2DAnimationExtensions
    {
        /// <summary>
        /// Moves current object by the given move vector.
        /// </summary>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="moveVector">The move vector.</param>
        /// <param name="animationTime">Total time for the animation.</param>
        public static IAnimationSequenceBuilder<TargetObject> Move2DBy<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, Vector2 moveVector, TimeSpan animationTime)
            where TargetObject : class, IAnimatableObjectPosition2D
        {
            sequenceBuilder.Add(
                new Move2DByAnimation(sequenceBuilder.TargetObject, moveVector, animationTime));
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
        public static IAnimationSequenceBuilder<HostObject> Move2DBy<HostObject, TargetObject>(this IAnimationSequenceBuilder<HostObject> sequenceBuilder, TargetObject targetObject, Vector2 moveVector, TimeSpan animationTime)
            where HostObject : class
            where TargetObject : class, IAnimatableObjectPosition2D
        {
            sequenceBuilder.Add(
                new Move2DByAnimation(targetObject, moveVector, animationTime));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object by the given move vector.
        /// </summary>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="moveVector">The move vector.</param>
        /// <param name="speed">Total speed of the movement animation.</param>
        public static IAnimationSequenceBuilder<TargetObject> Move2DBy<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, Vector2 moveVector, MovementSpeed speed)
            where TargetObject : class, IAnimatableObjectPosition2D
        {
            sequenceBuilder.Add(
                new Move2DByAnimation(sequenceBuilder.TargetObject, moveVector, speed));
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
        public static IAnimationSequenceBuilder<HostObject> Move2DBy<HostObject, TargetObject>(this IAnimationSequenceBuilder<HostObject> sequenceBuilder, TargetObject targetObject, Vector2 moveVector, MovementSpeed speed)
            where HostObject : class
            where TargetObject : class, IAnimatableObjectPosition2D
        {
            sequenceBuilder.Add(
                new Move2DByAnimation(targetObject, moveVector, speed));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object by the given move vector.
        /// </summary>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="moveVector">The move vector.</param>
        /// <param name="speed">Total speed of the movement animation.</param>
        public static IAnimationSequenceBuilder<TargetObject> Move2DBy<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, Vector2 moveVector, float speed)
            where TargetObject : class, IAnimatableObjectPosition2D
        {
            sequenceBuilder.Add(
                new Move2DByAnimation(sequenceBuilder.TargetObject, moveVector, new MovementSpeed(speed)));
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
        public static IAnimationSequenceBuilder<HostObject> Move2DBy<HostObject, TargetObject>(this IAnimationSequenceBuilder<HostObject> sequenceBuilder, TargetObject targetObject, Vector2 moveVector, float speed)
            where HostObject : class
            where TargetObject : class, IAnimatableObjectPosition2D
        {
            sequenceBuilder.Add(
                new Move2DByAnimation(targetObject, moveVector, new MovementSpeed(speed)));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object to the given target position.
        /// </summary>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetVector">The target position for the object.</param>
        /// <param name="animationTime">Total time for the animation.</param>
        public static IAnimationSequenceBuilder<TargetObject> Move2DTo<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, Vector2 targetVector, TimeSpan animationTime)
            where TargetObject : class, IAnimatableObjectPosition2D
        {
            sequenceBuilder.Add(
                new Move2DToAnimation(sequenceBuilder.TargetObject, targetVector, animationTime));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object to the given target position.
        /// </summary>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetVector">The target position for the object.</param>
        /// <param name="speed">Speed configuration for the movement.</param>
        public static IAnimationSequenceBuilder<TargetObject> Move2DTo<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, Vector2 targetVector, MovementSpeed speed)
            where TargetObject : class, IAnimatableObjectPosition2D
        {
            sequenceBuilder.Add(
                new Move2DToAnimation(sequenceBuilder.TargetObject, targetVector, speed));
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
        public static IAnimationSequenceBuilder<TargetObject> Move2DTo<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, Vector2 targetVector, float speed)
            where TargetObject : class, IAnimatableObjectPosition2D
        {
            sequenceBuilder.Add(
                new Move2DToAnimation(sequenceBuilder.TargetObject, targetVector, new MovementSpeed(speed)));
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
        public static IAnimationSequenceBuilder<TargetObject> Move2DTo<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, Vector2 targetVector, float speed, float acceleration, float deceleration)
            where TargetObject : class, IAnimatableObjectPosition2D
        {
            sequenceBuilder.Add(
                new Move2DToAnimation(sequenceBuilder.TargetObject, targetVector, new MovementSpeed(speed, acceleration, deceleration)));
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
        public static IAnimationSequenceBuilder<HostObject> Move2DTo<HostObject, TargetObject>(this IAnimationSequenceBuilder<HostObject> sequenceBuilder, TargetObject targetObject, Vector2 targetVector, float speed)
            where TargetObject : class, IAnimatableObjectPosition2D
            where HostObject : class
        {
            sequenceBuilder.Add(
                new Move2DToAnimation(targetObject, targetVector, new MovementSpeed(speed)));
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
        public static IAnimationSequenceBuilder<HostObject> Move2DTo<HostObject, TargetObject>(this IAnimationSequenceBuilder<HostObject> sequenceBuilder, TargetObject targetObject, Vector2 targetVector, MovementSpeed speed)
            where TargetObject : class, IAnimatableObjectPosition2D
            where HostObject : class
        {
            sequenceBuilder.Add(
                new Move2DToAnimation(targetObject, targetVector, speed));
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
        public static IAnimationSequenceBuilder<HostObject> Move2DTo<HostObject, TargetObject>(this IAnimationSequenceBuilder<HostObject> sequenceBuilder, IEnumerable<TargetObject> targetObjects, Vector2 targetVector, float speed)
            where TargetObject : class, IAnimatableObjectPosition2D
            where HostObject : class
        {
            foreach(var actTargetObject in targetObjects)
            {
                sequenceBuilder.Add(
                    new Move2DToAnimation(actTargetObject, targetVector, new MovementSpeed(speed)));
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
        public static IAnimationSequenceBuilder<HostObject> Move2DTo<HostObject, TargetObject>(this IAnimationSequenceBuilder<HostObject> sequenceBuilder, IEnumerable<TargetObject> targetObjects, Vector2 targetVector, MovementSpeed speed)
            where TargetObject : class, IAnimatableObjectPosition2D
            where HostObject : class
        {
            foreach (var actTargetObject in targetObjects)
            {
                sequenceBuilder.Add(
                    new Move2DToAnimation(actTargetObject, targetVector, speed));
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
        public static IAnimationSequenceBuilder<HostObject> Move2DTo<HostObject, TargetObject>(this IAnimationSequenceBuilder<HostObject> sequenceBuilder, TargetObject targetObject, Vector2 targetVector, float speed, float acceleration, float deceleration)
            where TargetObject : class, IAnimatableObjectPosition2D
            where HostObject : class
        {
            sequenceBuilder.Add(
                new Move2DToAnimation(targetObject, targetVector, new MovementSpeed(speed, acceleration, deceleration)));
            return sequenceBuilder;
        }
    }
}