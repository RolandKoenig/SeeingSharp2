using System;
using System.Threading.Tasks;

namespace SeeingSharp.Core
{
    public interface IAnimationSequenceBuilder<out TTargetType>
        where TTargetType : class
    {
        /// <summary>
        /// Gets the target object.
        /// </summary>
        TTargetType TargetObject
        {
            get;
        }

        /// <summary>
        /// Gets the corresponding animation handler.
        /// </summary>
        AnimationHandler AnimationHandler
        {
            get;
        }

        /// <summary>
        /// Gets the current count of items within this SequenceBuilder object.
        /// </summary>
        int ItemCount
        {
            get;
        }

        /// <summary>
        /// Is Apply already called?
        /// </summary>
        bool Applied
        {
            get;
        }

        /// <summary>
        /// Adds an AnimationSequence to this builder.
        /// </summary>
        /// <param name="animationSequence">The animation sequence to be added.</param>
        IAnimationSequenceBuilder<TTargetType> Add(IAnimation animationSequence);

        /// <summary>
        /// Finishes the AnimationSequence and adds it to the AnimationHandler it was created with.
        /// </summary>
        /// <param name="actionToCall">The action to be called after animation has finished.</param>
        /// <param name="cancelAction">The action to be called when the animation gets canceled.</param>
        /// <param name="ignorePause">Should this animation ignore pause state</param>
        void Apply(Action actionToCall = null, Action cancelAction = null, bool? ignorePause = null);

        /// <summary>
        /// Finishes the AnimationSequence and adds it to the AnimationHandler it was created with.
        /// The caller can await the finish of this animation using the returned task object.
        /// </summary>
        Task ApplyAsync();

        /// <summary>
        /// Finishes the animation and starts from beginning.
        /// </summary>
        /// <param name="ignorePause">Should this animation ignore pause state</param>
        void ApplyAndRewind(bool? ignorePause = null);

        /// <summary>
        /// Finishes the AnimationSequence and adds it to the AnimationHandler it was created with.
        /// </summary>
        /// <param name="actionToCall">The action to be called after animation has finished.</param>
        /// <param name="cancelAction">The action to be called when the animation gets canceled.</param>
        /// <param name="ignorePause">Should this animation ignore pause state</param>
        void ApplyAsSecondary(Action actionToCall = null, Action cancelAction = null, bool? ignorePause = null);

        /// <summary>
        /// Finishes the AnimationSequence and adds it to the AnimationHandler it was created with.
        /// The caller can await the finish of this animation using the returned task object.
        /// </summary>
        Task ApplyAsSecondaryAsync();

        /// <summary>
        /// Finishes the animation and starts from beginning.
        /// </summary>
        void ApplyAsSecondaryAndRewind();
    }
}
