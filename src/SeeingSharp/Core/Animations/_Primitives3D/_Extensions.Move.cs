using System;
using System.Collections.Generic;
using System.Numerics;
using SeeingSharp.Mathematics;

namespace SeeingSharp.Core.Animations
{
    public static partial class Drawing3DAnimationExtensions
    {
        /// <summary>
        /// Moves current object by the given move vector.
        /// </summary>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="moveVector">The move vector.</param>
        /// <param name="animationTime">Total time for the animation.</param>
        public static IAnimationSequenceBuilder<TTargetObject> Move3DBy<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, Vector3 moveVector, TimeSpan animationTime)
            where TTargetObject : class, IAnimatableObjectPosition
        {
            sequenceBuilder.Add(
                new Move3DByAnimation(sequenceBuilder.TargetObject, moveVector, animationTime));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object by the given move vector.
        /// </summary>
        /// <typeparam name="THostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TTargetObject">The type of the target object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetObject">The target object.</param>
        /// <param name="moveVector">The move vector.</param>
        /// <param name="animationTime">Total time for the animation.</param>
        public static IAnimationSequenceBuilder<THostObject> Move3DBy<THostObject, TTargetObject>(this IAnimationSequenceBuilder<THostObject> sequenceBuilder, TTargetObject targetObject, Vector3 moveVector, TimeSpan animationTime)
            where THostObject : class
            where TTargetObject : class, IAnimatableObjectPosition
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
        public static IAnimationSequenceBuilder<TTargetObject> Move3DBy<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, Vector3 moveVector, MovementSpeed speed)
            where TTargetObject : class, IAnimatableObjectPosition
        {
            sequenceBuilder.Add(
                new Move3DByAnimation(sequenceBuilder.TargetObject, moveVector, speed));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object by the given move vector.
        /// </summary>
        /// <typeparam name="THostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TTargetObject">The type of the target object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetObject">The target object.</param>
        /// <param name="moveVector">The move vector.</param>
        /// <param name="speed">Total speed of the movement animation.</param>
        public static IAnimationSequenceBuilder<THostObject> Move3DBy<THostObject, TTargetObject>(this IAnimationSequenceBuilder<THostObject> sequenceBuilder, TTargetObject targetObject, Vector3 moveVector, MovementSpeed speed)
            where THostObject : class
            where TTargetObject : class, IAnimatableObjectPosition
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
        public static IAnimationSequenceBuilder<TTargetObject> Move3DBy<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, Vector3 moveVector, float speed)
            where TTargetObject : class, IAnimatableObjectPosition
        {
            sequenceBuilder.Add(
                new Move3DByAnimation(sequenceBuilder.TargetObject, moveVector, new MovementSpeed(speed)));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object by the given move vector.
        /// </summary>
        /// <typeparam name="THostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TTargetObject">The type of the target object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetObject">The target object.</param>
        /// <param name="moveVector">The move vector.</param>
        /// <param name="speed">Total speed of the movement animation.</param>
        public static IAnimationSequenceBuilder<THostObject> Move3DBy<THostObject, TTargetObject>(this IAnimationSequenceBuilder<THostObject> sequenceBuilder, TTargetObject targetObject, Vector3 moveVector, float speed)
            where THostObject : class
            where TTargetObject : class, IAnimatableObjectPosition
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
        public static IAnimationSequenceBuilder<TTargetObject> Move3DTo<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, Vector3 targetVector, TimeSpan animationTime)
            where TTargetObject : class, IAnimatableObjectPosition
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
        public static IAnimationSequenceBuilder<TTargetObject> Move3DTo<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, Vector3 targetVector, MovementSpeed speed)
            where TTargetObject : class, IAnimatableObjectPosition
        {
            sequenceBuilder.Add(
                new Move3DToAnimation(sequenceBuilder.TargetObject, targetVector, speed));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object to the given target position.
        /// </summary>
        /// <typeparam name="TTargetObject">The type of the target object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetVector">The target position for the object.</param>
        /// <param name="speed">The speed for animation calculation.</param>
        /// <returns></returns>
        public static IAnimationSequenceBuilder<TTargetObject> Move3DTo<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, Vector3 targetVector, float speed)
            where TTargetObject : class, IAnimatableObjectPosition
        {
            sequenceBuilder.Add(
                new Move3DToAnimation(sequenceBuilder.TargetObject, targetVector, new MovementSpeed(speed)));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object to the given target position.
        /// </summary>
        /// <typeparam name="TTargetObject">The type of the target object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetVector">The target position for the object.</param>
        /// <param name="speed">The speed for animation calculation.</param>
        /// <param name="acceleration">The acceleration.</param>
        /// <param name="deceleration">The deceleration.</param>
        /// <returns></returns>
        public static IAnimationSequenceBuilder<TTargetObject> Move3DTo<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, Vector3 targetVector, float speed, float acceleration, float deceleration)
            where TTargetObject : class, IAnimatableObjectPosition
        {
            sequenceBuilder.Add(
                new Move3DToAnimation(sequenceBuilder.TargetObject, targetVector, new MovementSpeed(speed, acceleration, deceleration)));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object to the given target position.
        /// </summary>
        /// <typeparam name="THostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TTargetObject">The type of the target object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetVector">The target position for the object.</param>
        /// <param name="speed">The speed for animation calculation.</param>
        /// <returns></returns>
        public static IAnimationSequenceBuilder<THostObject> Move3DTo<THostObject, TTargetObject>(this IAnimationSequenceBuilder<THostObject> sequenceBuilder, TTargetObject targetObject, Vector3 targetVector, float speed)
            where TTargetObject : class, IAnimatableObjectPosition
            where THostObject : class
        {
            sequenceBuilder.Add(
                new Move3DToAnimation(targetObject, targetVector, new MovementSpeed(speed)));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object to the given target position.
        /// </summary>
        /// <typeparam name="THostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TTargetObject">The type of the target object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetVector">The target position for the object.</param>
        /// <param name="speed">The speed for animation calculation.</param>
        /// <returns></returns>
        public static IAnimationSequenceBuilder<THostObject> Move3DTo<THostObject, TTargetObject>(this IAnimationSequenceBuilder<THostObject> sequenceBuilder, TTargetObject targetObject, Vector3 targetVector, MovementSpeed speed)
            where TTargetObject : class, IAnimatableObjectPosition
            where THostObject : class
        {
            sequenceBuilder.Add(
                new Move3DToAnimation(targetObject, targetVector, speed));
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object to the given target position.
        /// </summary>
        /// <typeparam name="THostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TTargetObject">The type of the target object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetObjects">A collection containing all target objects.</param>
        /// <param name="targetVector">The target position for the object.</param>
        /// <param name="speed">The speed for animation calculation.</param>
        /// <returns></returns>
        public static IAnimationSequenceBuilder<THostObject> Move3DTo<THostObject, TTargetObject>(this IAnimationSequenceBuilder<THostObject> sequenceBuilder, IEnumerable<TTargetObject> targetObjects, Vector3 targetVector, float speed)
            where TTargetObject : class, IAnimatableObjectPosition
            where THostObject : class
        {
            foreach (var actTargetObject in targetObjects)
            {
                sequenceBuilder.Add(
                    new Move3DToAnimation(actTargetObject, targetVector, new MovementSpeed(speed)));
            }
            return sequenceBuilder;
        }

        /// <summary>
        /// Moves current object to the given target position.
        /// </summary>
        /// <typeparam name="THostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TTargetObject">The type of the target object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetObjects">A collection containing all target objects.</param>
        /// <param name="targetVector">The target position for the object.</param>
        /// <param name="speed">The speed for animation calculation.</param>
        public static IAnimationSequenceBuilder<THostObject> Move3DTo<THostObject, TTargetObject>(this IAnimationSequenceBuilder<THostObject> sequenceBuilder, IEnumerable<TTargetObject> targetObjects, Vector3 targetVector, MovementSpeed speed)
            where TTargetObject : class, IAnimatableObjectPosition
            where THostObject : class
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
        /// <typeparam name="THostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TTargetObject">The type of the target object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetVector">The target position for the object.</param>
        /// <param name="speed">The speed for animation calculation.</param>
        /// <param name="acceleration">The acceleration.</param>
        /// <param name="deceleration">The deceleration.</param>
        public static IAnimationSequenceBuilder<THostObject> Move3DTo<THostObject, TTargetObject>(this IAnimationSequenceBuilder<THostObject> sequenceBuilder, TTargetObject targetObject, Vector3 targetVector, float speed, float acceleration, float deceleration)
            where TTargetObject : class, IAnimatableObjectPosition
            where THostObject : class
        {
            sequenceBuilder.Add(
                new Move3DToAnimation(targetObject, targetVector, new MovementSpeed(speed, acceleration, deceleration)));
            return sequenceBuilder;
        }
    }
}