using System;

namespace SeeingSharp.Core.Animations
{
    public interface IAnimation
    {
        /// <summary>
        /// Is the animation finished?
        /// </summary>
        bool Finished { get; }

        /// <summary>
        /// Is the animation canceled?
        /// </summary>
        bool Canceled { get; set; }

        /// <summary>
        /// Is this animation a blocking animation?
        /// </summary>
        bool IsBlockingAnimation { get; }

        /// <summary>
        /// Should this animation ignore pause state?
        /// </summary>
        bool IgnorePauseState { get; set; }

        /// <summary>
        /// Checks if the given object is animated by this animation.
        /// </summary>
        /// <param name="targetObject">The object to check for.</param>
        bool IsObjectAnimated(object targetObject);

        /// <summary>
        /// Called for each update step of this animation.
        /// </summary>
        /// <param name="animationState">The current state of the animation.</param>
        /// <param name="updateState">The current state of the update pass.</param>
        AnimationUpdateResult Update(IAnimationUpdateState updateState, AnimationState animationState);

        /// <summary>
        /// Gets the time in milliseconds till this animation is finished.
        /// This method is relevant for event-driven processing and tells the system by what amount the clock is to be increased next.
        /// </summary>
        /// <param name="previousMinFinishTime">The minimum TimeSpan previous animations take.</param>
        /// <param name="previousMaxFinishTime">The maximum TimeSpan previous animations take.</param>
        /// <param name="defaultCycleTime">The default cycle time if we would be in continuous calculation mode.</param>
        TimeSpan GetTimeTillNextEvent(TimeSpan previousMinFinishTime, TimeSpan previousMaxFinishTime, TimeSpan defaultCycleTime);

        /// <summary>
        /// Resets this animation.
        /// </summary>
        void Reset();
    }
}
