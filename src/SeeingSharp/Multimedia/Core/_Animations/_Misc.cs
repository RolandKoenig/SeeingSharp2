using System;

namespace SeeingSharp.Multimedia.Core
{
    /// <summary>
    /// Describes the component which is relevant for rotation calculation.
    /// </summary>
    [Flags]
    public enum RotationCalculationComponent
    {
        All = Pitch | Yaw | Roll,

        Pitch = 1,

        Yaw = 2,

        Roll = 4
    }

    /// <summary>
    /// Describes the mode of an animation.
    /// </summary>
    public enum AnimationStateChangeMode
    {
        /// <summary>
        /// An animation changes the state of an object to a given target state (e. g. fixed target location).
        /// </summary>
        ChangeStateTo,

        /// <summary>
        /// An animation changes the state of an object by a given transition value (e. g. move by vector).
        /// </summary>
        ChangeStateBy
    }

    /// <summary>
    /// Defines the type of an animation.
    /// </summary>
    public enum AnimationType
    {
        /// <summary>
        /// The animation finishes after a fixed period of time.
        /// </summary>
        FixedTime,

        /// <summary>
        /// The animation finishes after a event occurs (e. g. an object reaches its target position)
        /// </summary>
        FinishedByEvent,

        /// <summary>
        /// The animation defines an asynchronous call
        /// </summary>
        AsyncCall
    }

    /// <summary>
    /// Describes the result of an update pass in the animation system.
    /// </summary>
    public struct AnimationUpdateResult
    {
        public static readonly AnimationUpdateResult EMPTY = new AnimationUpdateResult();

        /// <summary>
        /// Total count of finished animations.
        /// </summary>
        public int CountFinishedAnimations;
    }

    /// <summary>
    /// Tells the system what to do if an animation has failed.
    /// </summary>
    public enum AnimationFailedReaction
    {
        /// <summary>
        /// RemoveObject the failed part from the animation and continue with remaining.
        /// </summary>
        RemoveAndContinue,

        /// <summary>
        /// Throw an exception and therefore stop the animation.
        /// </summary>
        ThrowException
    }

    public class AnimationFailedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the animation that has failed.
        /// </summary>
        public IAnimation Animation
        {
            get;
        }

        /// <summary>
        /// Gets the exception occurred during execution of the animation.
        /// </summary>
        public Exception Exception
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationFailedEventArgs"/> class.
        /// </summary>
        /// <param name="animation">The animation.</param>
        /// <param name="exception">The exception.</param>
        internal AnimationFailedEventArgs(IAnimation animation, Exception exception)
        {
            this.Animation = animation;
            this.Exception = exception;
        }
    }
}