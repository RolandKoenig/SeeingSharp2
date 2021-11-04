using System;
using System.Collections.Generic;
using System.Numerics;

namespace SeeingSharp.Core
{
    public static class Drawing2DAnimationExtensions
    {
        /// <summary>
        /// Moves current object by the given move vector.
        /// </summary>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="moveVector">The move vector.</param>
        /// <param name="animationTime">Total time for the animation.</param>
        public static IAnimationSequenceBuilder<TTargetObject> Move2DBy<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, Vector2 moveVector, TimeSpan animationTime)
            where TTargetObject : class, IAnimatableObjectPosition2D
        {
            sequenceBuilder.Add(
                new Move2DByAnimation(sequenceBuilder.TargetObject, moveVector, animationTime));
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
        public static IAnimationSequenceBuilder<THostObject> Move2DBy<THostObject, TTargetObject>(this IAnimationSequenceBuilder<THostObject> sequenceBuilder, TTargetObject targetObject, Vector2 moveVector, TimeSpan animationTime)
            where THostObject : class
            where TTargetObject : class, IAnimatableObjectPosition2D
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
        public static IAnimationSequenceBuilder<TTargetObject> Move2DBy<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, Vector2 moveVector, MovementSpeed speed)
            where TTargetObject : class, IAnimatableObjectPosition2D
        {
            sequenceBuilder.Add(
                new Move2DByAnimation(sequenceBuilder.TargetObject, moveVector, speed));
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
        public static IAnimationSequenceBuilder<THostObject> Move2DBy<THostObject, TTargetObject>(this IAnimationSequenceBuilder<THostObject> sequenceBuilder, TTargetObject targetObject, Vector2 moveVector, MovementSpeed speed)
            where THostObject : class
            where TTargetObject : class, IAnimatableObjectPosition2D
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
        public static IAnimationSequenceBuilder<TTargetObject> Move2DBy<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, Vector2 moveVector, float speed)
            where TTargetObject : class, IAnimatableObjectPosition2D
        {
            sequenceBuilder.Add(
                new Move2DByAnimation(sequenceBuilder.TargetObject, moveVector, new MovementSpeed(speed)));
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
        public static IAnimationSequenceBuilder<THostObject> Move2DBy<THostObject, TTargetObject>(this IAnimationSequenceBuilder<THostObject> sequenceBuilder, TTargetObject targetObject, Vector2 moveVector, float speed)
            where THostObject : class
            where TTargetObject : class, IAnimatableObjectPosition2D
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
        public static IAnimationSequenceBuilder<TTargetObject> Move2DTo<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, Vector2 targetVector, TimeSpan animationTime)
            where TTargetObject : class, IAnimatableObjectPosition2D
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
        public static IAnimationSequenceBuilder<TTargetObject> Move2DTo<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, Vector2 targetVector, MovementSpeed speed)
            where TTargetObject : class, IAnimatableObjectPosition2D
        {
            sequenceBuilder.Add(
                new Move2DToAnimation(sequenceBuilder.TargetObject, targetVector, speed));
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
        public static IAnimationSequenceBuilder<TTargetObject> Move2DTo<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, Vector2 targetVector, float speed)
            where TTargetObject : class, IAnimatableObjectPosition2D
        {
            sequenceBuilder.Add(
                new Move2DToAnimation(sequenceBuilder.TargetObject, targetVector, new MovementSpeed(speed)));
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
        public static IAnimationSequenceBuilder<TTargetObject> Move2DTo<TTargetObject>(this IAnimationSequenceBuilder<TTargetObject> sequenceBuilder, Vector2 targetVector, float speed, float acceleration, float deceleration)
            where TTargetObject : class, IAnimatableObjectPosition2D
        {
            sequenceBuilder.Add(
                new Move2DToAnimation(sequenceBuilder.TargetObject, targetVector, new MovementSpeed(speed, acceleration, deceleration)));
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
        public static IAnimationSequenceBuilder<THostObject> Move2DTo<THostObject, TTargetObject>(this IAnimationSequenceBuilder<THostObject> sequenceBuilder, TTargetObject targetObject, Vector2 targetVector, float speed)
            where TTargetObject : class, IAnimatableObjectPosition2D
            where THostObject : class
        {
            sequenceBuilder.Add(
                new Move2DToAnimation(targetObject, targetVector, new MovementSpeed(speed)));
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
        public static IAnimationSequenceBuilder<THostObject> Move2DTo<THostObject, TTargetObject>(this IAnimationSequenceBuilder<THostObject> sequenceBuilder, TTargetObject targetObject, Vector2 targetVector, MovementSpeed speed)
            where TTargetObject : class, IAnimatableObjectPosition2D
            where THostObject : class
        {
            sequenceBuilder.Add(
                new Move2DToAnimation(targetObject, targetVector, speed));
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
        public static IAnimationSequenceBuilder<THostObject> Move2DTo<THostObject, TTargetObject>(this IAnimationSequenceBuilder<THostObject> sequenceBuilder, IEnumerable<TTargetObject> targetObjects, Vector2 targetVector, float speed)
            where TTargetObject : class, IAnimatableObjectPosition2D
            where THostObject : class
        {
            foreach (var actTargetObject in targetObjects)
            {
                sequenceBuilder.Add(
                    new Move2DToAnimation(actTargetObject, targetVector, new MovementSpeed(speed)));
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
        public static IAnimationSequenceBuilder<THostObject> Move2DTo<THostObject, TTargetObject>(this IAnimationSequenceBuilder<THostObject> sequenceBuilder, IEnumerable<TTargetObject> targetObjects, Vector2 targetVector, MovementSpeed speed)
            where TTargetObject : class, IAnimatableObjectPosition2D
            where THostObject : class
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
        /// <typeparam name="THostObject">The type of the ost object.</typeparam>
        /// <typeparam name="TTargetObject">The type of the target object.</typeparam>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetVector">The target position for the object.</param>
        /// <param name="speed">The speed for animation calculation.</param>
        /// <param name="acceleration">The acceleration.</param>
        /// <param name="deceleration">The deceleration.</param>
        public static IAnimationSequenceBuilder<THostObject> Move2DTo<THostObject, TTargetObject>(this IAnimationSequenceBuilder<THostObject> sequenceBuilder, TTargetObject targetObject, Vector2 targetVector, float speed, float acceleration, float deceleration)
            where TTargetObject : class, IAnimatableObjectPosition2D
            where THostObject : class
        {
            sequenceBuilder.Add(
                new Move2DToAnimation(targetObject, targetVector, new MovementSpeed(speed, acceleration, deceleration)));
            return sequenceBuilder;
        }
    }
}