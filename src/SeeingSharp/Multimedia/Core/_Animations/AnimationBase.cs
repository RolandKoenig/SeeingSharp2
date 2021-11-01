using System;
using System.Threading.Tasks;
using SeeingSharp.Checking;

namespace SeeingSharp.Multimedia.Core
{
    public abstract class AnimationBase : IAnimation
    {
        // Main properties of this animation
        private AnimationType _animationType;

        // Control members for AnimationTypes
        private Task _asyncTask;
        private TimeSpan _fixedTime;
        private TimeSpan _currentTime;
        private bool _finished;
        private bool _started;
        private bool _canceled;

        /// <summary>
        /// Has this animation finished executing?
        /// </summary>
        public bool Finished => _finished;

        /// <summary>
        /// Is this animation a blocking animation?
        /// If true, all following animation have to wait for finish-event.
        /// </summary>
        public virtual bool IsBlockingAnimation => false;

        /// <summary>
        /// Is this animation canceled?
        /// </summary>
        public bool Canceled
        {
            get => _canceled;
            set
            {
                if (_canceled != value)
                {
                    _canceled = value;
                    if (_canceled)
                    {
                        this.OnCanceled();
                    }
                }
            }
        }

        /// <summary>
        /// Should this animation ignore pause state?
        /// </summary>
        public bool IgnorePauseState
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the animation type.
        /// </summary>
        public AnimationType AnimationType => _animationType;

        /// <summary>
        /// Complete duration when in FixedTime mode.
        /// </summary>
        public TimeSpan FixedTime => _fixedTime;

        /// <summary>
        /// Current time in FixedTime mode.
        /// </summary>
        public TimeSpan CurrentTime => _currentTime;

        /// <summary>
        /// Gets the target object.
        /// </summary>
        public object TargetObject { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationBase"/> class.
        /// </summary>
        protected AnimationBase(object targetObject)
        {
            this.TargetObject = targetObject;
            _fixedTime = TimeSpan.Zero;
            _currentTime = TimeSpan.Zero;
            _animationType = AnimationType.FinishedByEvent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationBase"/> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="animationType">Type of the animation.</param>
        protected AnimationBase(object targetObject, AnimationType animationType)
            : this(targetObject)
        {
            _animationType = animationType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationBase" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="animationType">Type of the animation.</param>
        /// <param name="fixedTime">The fixed time.</param>
        protected AnimationBase(object targetObject, AnimationType animationType, TimeSpan fixedTime)
            : this(targetObject)
        {
            fixedTime.EnsureLongerOrEqualZero(nameof(fixedTime));

            _animationType = animationType;
            _fixedTime = fixedTime;
        }

        /// <summary>
        /// Resets this animation.
        /// </summary>
        public virtual void OnReset()
        {
            switch (_animationType)
            {
                case AnimationType.AsyncCall:
                    _asyncTask = null;
                    break;
            }
        }

        /// <summary>
        /// Called when this animation got canceled.
        /// </summary>
        public virtual void OnCanceled()
        {
        }

        /// <summary>
        /// Resets this animation.
        /// </summary>
        public void Reset()
        {
            _finished = false;
            _started = false;
            _currentTime = TimeSpan.Zero;

            this.OnReset();
        }

        /// <summary>
        /// Called for each update step.
        /// </summary>
        /// <param name="animationState">The current state of the animation.</param>
        /// <param name="updateState">The current state of the update pass.</param>
        public AnimationUpdateResult Update(IAnimationUpdateState updateState, AnimationState animationState)
        {
            // Call start animation if _currentTime is zero
            this.HandleStartAnimation();

            switch (_animationType)
            {
                // Update logic for FixedTime animations
                case AnimationType.FixedTime:
                    if (_fixedTime <= TimeSpan.Zero)
                    {
                        this.OnStartAnimation();
                        this.OnFixedTimeAnimationFinished();
                        _finished = true;
                    }
                    if (_currentTime < _fixedTime)
                    {
                        _currentTime = _currentTime.Add(updateState.UpdateTime);
                        if (_currentTime >= _fixedTime)
                        {
                            _currentTime = _fixedTime;

                            this.OnCurrentTimeUpdated(updateState, animationState);
                            this.OnFixedTimeAnimationFinished();
                            _finished = true;
                        }
                        else
                        {
                            this.OnCurrentTimeUpdated(updateState, animationState);
                        }
                    }
                    break;

                // Update logic for FinishedByEvent animations
                case AnimationType.FinishedByEvent:
                    _currentTime += updateState.UpdateTime;

                    //Call update method
                    this.OnCurrentTimeUpdated(updateState, animationState);
                    break;

                // Update logic for async calls
                case AnimationType.AsyncCall:
                    _currentTime += updateState.UpdateTime;
                    if (_asyncTask == null)
                    {
                        _asyncTask = this.OnAsyncAnimationStart();
                    }
                    else if (_asyncTask.IsFaulted)
                    {
                        throw new SeeingSharpGraphicsException("Async animation raised an exception!", _asyncTask.Exception);
                    }
                    else if (_asyncTask.IsCompleted || _asyncTask.IsCanceled || _asyncTask.IsFaulted)
                    {
                        _finished = true;
                    }
                    break;
            }

            return AnimationUpdateResult.EMPTY;
        }

        /// <summary>
        /// Is the given object animated by this animation?
        /// </summary>
        /// <param name="targetObject">The object to check for.</param>
        public bool IsObjectAnimated(object targetObject)
        {
            if (this.TargetObject == null) { return false; }
            return this.TargetObject == targetObject;
        }

        /// <summary>
        /// Gets the time in milliseconds till this animation is finished.
        /// This method is relevant for event-driven processing and tells the system by what amount the clock is to be increased next.
        /// </summary>
        /// <param name="previousMinFinishTime">The minimum TimeSpan previous animations take.</param>
        /// <param name="previousMaxFinishTime">The maximum TimeSpan previous animations take.</param>
        /// <param name="defaultCycleTime">The default cycle time if we would be in continuous calculation mode.</param>
        public virtual TimeSpan GetTimeTillNextEvent(TimeSpan previousMinFinishTime, TimeSpan previousMaxFinishTime, TimeSpan defaultCycleTime)
        {
            // Trigger start logic if not done so before
            // .. => This is needed because some animations calculate their fixed time within the start method
            this.HandleStartAnimation();

            // Handle the animation type
            switch (this.AnimationType)
            {
                case AnimationType.AsyncCall:
                    return defaultCycleTime;

                case AnimationType.FinishedByEvent:
                    return defaultCycleTime;

                case AnimationType.FixedTime:
                    if (_fixedTime > _currentTime) { return _fixedTime - _currentTime; }
                    else { return TimeSpan.Zero; }

                default:
                    throw new SeeingSharpGraphicsException("Unhandled animation type: " + this.AnimationType);
            }
        }

        /// <summary>
        /// Change the type of this animation to fixed time.
        /// </summary>
        /// <param name="fixedTime">This fixed time to be set.</param>
        protected void ChangeToFixedTime(TimeSpan fixedTime)
        {
            fixedTime.EnsureLongerOrEqualZero(nameof(fixedTime));

            if (_currentTime > TimeSpan.Zero) { throw new InvalidOperationException("Unable to change animation type when animation was started already!"); }

            _fixedTime = fixedTime;
            _animationType = AnimationType.FixedTime;
        }

        /// <summary>
        /// Change the type of this animation to event-based (FinishedByEvent).
        /// </summary>
        protected void ChangeToEventBased()
        {
            if (_currentTime > TimeSpan.Zero) { throw new InvalidOperationException("Unable to change animation type when animation was started already!"); }

            _fixedTime = TimeSpan.Zero;
            _animationType = AnimationType.FinishedByEvent;
        }

        /// <summary>
        /// Notifies this sequence that the animation is finished.
        /// </summary>
        protected void NotifyAnimationFinished()
        {
            _finished = true;
        }

        /// <summary>
        /// Called when animation starts.
        /// (Checks the target object for compatibility and initializes runtime values).
        /// </summary>
        protected virtual void OnStartAnimation()
        {
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        protected virtual void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// (Sets final state to the target object and clears all runtime values).
        /// </summary>
        protected virtual void OnFixedTimeAnimationFinished()
        {
        }

        /// <summary>
        /// Called when an async animation starts.
        /// </summary>
        protected virtual Task OnAsyncAnimationStart()
        {
            return null;
        }

        /// <summary>
        /// Call OnStartAnimation method on demand.
        /// </summary>
        private void HandleStartAnimation()
        {
            if (_currentTime == TimeSpan.Zero &&
                !_finished &&
                !_started)
            {
                this.OnStartAnimation();
                _started = true;
            }
        }
    }
}