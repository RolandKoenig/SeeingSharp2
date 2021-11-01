using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeeingSharp.Multimedia.Core
{
    internal class AnimationSequenceBuilder<TTargetType> : IAnimationSequenceBuilder<TTargetType>
        where TTargetType : class
    {
        private bool _applied;
        private List<IAnimation> _sequenceList;

        /// <summary>
        /// Gets the corresponding animation handler.
        /// </summary>
        public AnimationHandler AnimationHandler { get; }

        /// <summary>
        /// Gets the target object of this animation
        /// </summary>
        public TTargetType TargetObject { get; }

        /// <summary>
        /// Gets the item count.
        /// </summary>
        public int ItemCount => _sequenceList.Count;

        /// <summary>
        /// Was apply called already?
        /// </summary>
        public bool Applied => _applied;

        /// <summary>
        /// Initializes a new instance of the AnimationSequenceBuilder class.
        /// </summary>
        /// <param name="owner">The owner object.</param>
        /// <exception cref="System.ArgumentException">Unable to cast target object of this AnimationSequenceBuilder to the generic type parameter!</exception>
        internal AnimationSequenceBuilder(AnimationHandler owner)
            : this()
        {
            this.AnimationHandler = owner;
            this.TargetObject = owner.Owner as TTargetType;
        }

        /// <summary>
        /// Initializes a new instance of the AnimationSequenceBuilder class.
        /// </summary>
        /// <param name="owner">The owner object.</param>
        /// <param name="animatedObject">The object which gets animated.</param>
        /// <exception cref="System.ArgumentException">Unable to cast target object of this AnimationSequenceBuilder to the generic type parameter!</exception>
        internal AnimationSequenceBuilder(AnimationHandler owner, TTargetType animatedObject)
            : this()
        {
            this.AnimationHandler = owner;
            this.TargetObject = animatedObject;
        }

        /// <summary>
        /// Initializes a new instance of the AnimationSequenceBuilder class.
        /// </summary>
        private AnimationSequenceBuilder()
        {
            _sequenceList = new List<IAnimation>();
        }

        /// <summary>
        /// Adds an AnimationSequence to the builder.
        /// </summary>
        public IAnimationSequenceBuilder<TTargetType> Add(IAnimation animationSequence)
        {
            if (_applied) { throw new SeeingSharpGraphicsException("Unable to add a new AnimationSequence to a finished AnimationSequenceBuilder!"); }
            _sequenceList.Add(animationSequence);

            return this;
        }

        /// <summary>
        /// Finishes the AnimationSequence and adds it to the AnimationHandler it was created with.
        /// </summary>
        /// <param name="actionToCall">The action to be called after animation has finished.</param>
        /// <param name="cancelAction">The action to be called when the animation gets canceled.</param>
        /// <param name="ignorePause">Should this animation ignore pause state</param>
        public void Apply(Action actionToCall = null, Action cancelAction = null, bool? ignorePause = null)
        {
            if (this.AnimationHandler == null)
            {
                throw new SeeingSharpGraphicsException("Unable to finish AnimationSequenceBuilder: No default AnimationHandler found!");
            }

            // Append 'CallAction' on demand
            if (actionToCall != null || cancelAction != null)
            {
                this.WaitFinished()
                    .CallAction(actionToCall, cancelAction);
            }

            // Change the 'Ignore pause state'
            if (ignorePause != null)
            {
                foreach (var actAnimation in _sequenceList)
                {
                    actAnimation.IgnorePauseState = ignorePause.Value;
                }
            }

            this.AnimationHandler.BeginAnimation(_sequenceList);
            _applied = true;
        }

        /// <summary>
        /// Finishes the AnimationSequence and adds it to the AnimationHandler it was created with.
        /// </summary>
        /// <param name="actionToCall">The action to be called after animation has finished.</param>
        /// <param name="cancelAction">The action to be called when the animation gets canceled.</param>
        /// <param name="ignorePause">Should this animation ignore pause state</param>
        public void ApplyAsSecondary(Action actionToCall, Action cancelAction, bool? ignorePause = null)
        {
            if (this.AnimationHandler == null) { throw new SeeingSharpGraphicsException("Unable to finish AnimationSequenceBuilder: No default AnimationHandler found!"); }

            // Append 'CallAction' on demand
            if (actionToCall != null || cancelAction != null)
            {
                this.WaitFinished()
                    .CallAction(actionToCall, cancelAction);
            }

            // Change the 'Ignore pause state'
            if (ignorePause != null)
            {
                foreach (var actAnimation in _sequenceList)
                {
                    actAnimation.IgnorePauseState = ignorePause.Value;
                }
            }

            this.AnimationHandler.BeginSecondaryAnimation(_sequenceList);
            _applied = true;
        }

        /// <summary>
        /// Finishes the AnimationSequence and adds it to the AnimationHandler it was created with.
        /// The caller can await the finish of this animation using the returned task object.
        /// </summary>
        public Task ApplyAsync()
        {
            var taskComplSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            this.Apply(
                () => taskComplSource.TrySetResult(true),
                () => taskComplSource.TrySetCanceled());

            return taskComplSource.Task;
        }

        /// <summary>
        /// Finishes the AnimationSequence and adds it to the AnimationHandler it was created with.
        /// The caller can await the finish of this animation using the returned task object.
        /// </summary>
        public Task ApplyAsSecondaryAsync()
        {
            var taskComplSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            this.ApplyAsSecondary(
                () => taskComplSource.TrySetResult(true),
                () => taskComplSource.TrySetCanceled());

            return taskComplSource.Task;
        }

        /// <summary>
        /// Finishes the animation and starts from beginning.
        /// </summary>
        /// <param name="ignorePause">Should this animation ignore pause state</param>
        public void ApplyAndRewind(bool? ignorePause = null)
        {
            if (this.AnimationHandler == null) { throw new SeeingSharpGraphicsException("Unable to finish AnimationSequenceBuilder: No default AnimationHandler found!"); }

            // Define rewind action
            //  a bit complicated because there a problems with the finished flag
            void RewindAction()
            {
                var newAnimationList = new List<IAnimation>(_sequenceList.Count);
                foreach (var actAnimationStep in _sequenceList)
                {
                    actAnimationStep.Reset();
                    newAnimationList.Add(actAnimationStep);
                }
                newAnimationList[newAnimationList.Count - 1] = new CallActionAnimation(RewindAction);
                this.AnimationHandler.BeginAnimation(newAnimationList);
            }

            // Append rewind action to the sequence
            this.WaitFinished()
                .CallAction(RewindAction);

            // Change the 'Ignore pause state'
            if (ignorePause != null)
            {
                foreach (var actAnimation in _sequenceList)
                {
                    actAnimation.IgnorePauseState = ignorePause.Value;
                }
            }

            // Start the animation
            this.AnimationHandler.BeginAnimation(_sequenceList);
            _applied = true;
        }

        /// <summary>
        /// Finishes the animation and starts from beginning.
        /// </summary>
        public void ApplyAsSecondaryAndRewind()
        {
            if (this.AnimationHandler == null) { throw new SeeingSharpGraphicsException("Unable to finish AnimationSequenceBuilder: No default AnimationHandler found!"); }

            // Define rewind action
            //  a bit complicated because there a problems with the finished flag
            void RewindAction()
            {
                var newAnimationList = new List<IAnimation>(_sequenceList.Count);
                foreach (var actAnimationStep in _sequenceList)
                {
                    actAnimationStep.Reset();
                    newAnimationList.Add(actAnimationStep);
                }
                newAnimationList[newAnimationList.Count - 1] = new CallActionAnimation(RewindAction);
                this.AnimationHandler.BeginSecondaryAnimation(newAnimationList);
            }

            // Append rewind action to the sequence
            this.WaitFinished()
                .CallAction(RewindAction);

            // Start the animation
            this.AnimationHandler.BeginSecondaryAnimation(_sequenceList);
            _applied = true;
        }
    }
}