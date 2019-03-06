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
    using System.Threading.Tasks;
    using Checking;

    #endregion

    public abstract class AnimationBase : IAnimation
    {
        #region Main properties of this animation

        #endregion

        #region Control members for AnimationTypes
        private Task m_asyncTask;
        private bool m_started;
        private bool m_canceled;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationBase"/> class.
        /// </summary>
        public AnimationBase(object targetObject)
        {
            TargetObject = targetObject;
            FixedTime = TimeSpan.Zero;
            CurrentTime = TimeSpan.Zero;
            AnimationType = AnimationType.FinishedByEvent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationBase"/> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="animationType">Type of the animation.</param>
        public AnimationBase(object targetObject, AnimationType animationType)
            : this(targetObject)
        {
            AnimationType = animationType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationBase" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="animationType">Type of the animation.</param>
        /// <param name="fixedTime">The fixed time.</param>
        public AnimationBase(object targetObject, AnimationType animationType, TimeSpan fixedTime)
            : this(targetObject)
        {
            fixedTime.EnsureLongerOrEqualZero(nameof(fixedTime));

            AnimationType = animationType;
            FixedTime = fixedTime;
        }

        /// <summary>
        /// Change the type of this animation to fixed time.
        /// </summary>
        /// <param name="fixedTime">This fixed time to be set.</param>
        protected void ChangeToFixedTime(TimeSpan fixedTime)
        {
            fixedTime.EnsureLongerOrEqualZero(nameof(fixedTime));

            if (CurrentTime > TimeSpan.Zero) { throw new InvalidOperationException("Unable to change animation type when animation was started already!"); }

            FixedTime = fixedTime;
            AnimationType = AnimationType.FixedTime;
        }

        /// <summary>
        /// Change the type of this animation to event-based (FinishedByEvent).
        /// </summary>
        protected void ChangeToEventBased()
        {
            if (CurrentTime > TimeSpan.Zero) { throw new InvalidOperationException("Unable to change animation type when animation was started already!"); }

            FixedTime = TimeSpan.Zero;
            AnimationType = AnimationType.FinishedByEvent;
        }

        /// <summary>
        /// Resets this animation.
        /// </summary>
        public void Reset()
        {
            Finished = false;
            m_started = false;
            CurrentTime = TimeSpan.Zero;

            OnReset();
        }

        /// <summary>
        /// Called for each update step.
        /// </summary>
        /// <param name="animationState">The current state of the animation.</param>
        /// <param name="updateState">The current state of the update pass.</param>
        public AnimationUpdateResult Update(IAnimationUpdateState updateState, AnimationState animationState)
        {
            // Call start animation if m_currentTime is zero
            HandleStartAnimation();

            switch (AnimationType)
            {
                // Update logic for FixedTime animations
                case AnimationType.FixedTime:
                    if (FixedTime <= TimeSpan.Zero)
                    {
                        OnStartAnimation();
                        OnFixedTimeAnimationFinished();
                        Finished = true;
                    }
                    if (CurrentTime < FixedTime)
                    {
                        CurrentTime = CurrentTime.Add(updateState.UpdateTime);
                        if (CurrentTime >= FixedTime)
                        {
                            CurrentTime = FixedTime;

                            OnCurrentTimeUpdated(updateState, animationState);
                            OnFixedTimeAnimationFinished();
                            Finished = true;
                        }
                        else
                        {
                            OnCurrentTimeUpdated(updateState, animationState);
                        }
                    }
                    break;

                // Update logic for FinishedByEvent animations
                case AnimationType.FinishedByEvent:
                    CurrentTime += updateState.UpdateTime;

                    // Call update method
                    OnCurrentTimeUpdated(updateState, animationState);
                    break;

                // Update logic for async calls
                case AnimationType.AsyncCall:
                    CurrentTime += updateState.UpdateTime;

                    if (m_asyncTask == null)
                    {
                        m_asyncTask = OnAsyncAnimationStart();
                    }
                    else if (m_asyncTask.IsFaulted)
                    {
                        throw new SeeingSharpGraphicsException("Async animation raised an exception!", m_asyncTask.Exception);
                    }
                    else if (m_asyncTask.IsCompleted || m_asyncTask.IsCanceled || m_asyncTask.IsFaulted)
                    {
                        Finished = true;
                    }
                    break;
            }

            return AnimationUpdateResult.Empty;
        }

        /// <summary>
        /// Is the given object animated by this animation?
        /// </summary>
        /// <param name="targetObject">The object to check for.</param>
        public bool IsObjectAnimated(object targetObject)
        {
            if (TargetObject == null) { return false; }
            return TargetObject == targetObject;
        }

        /// <summary>
        /// Resets this animation.
        /// </summary>
        public virtual void OnReset()
        {
            switch (AnimationType)
            {
                case AnimationType.AsyncCall:
                    m_asyncTask = null;
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
        /// Notifies this sequence that the animation is finished.
        /// </summary>
        protected void NotifyAnimationFinished()
        {
            Finished = true;
        }

        /// <summary>
        /// Call OnStartAnimation method on demand.
        /// </summary>
        private void HandleStartAnimation()
        {
            if ((CurrentTime != TimeSpan.Zero) || (Finished) || (m_started))
            {
                return;
            }

            OnStartAnimation();
            m_started = true;
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
        /// Gets the animation type.
        /// </summary>
        public AnimationType AnimationType { get; private set; }

        /// <summary>
        /// Complete duration when in FixedTime mode.
        /// </summary>
        public TimeSpan FixedTime { get; private set; }

        /// <summary>
        /// Current time in FixedTime mode.
        /// </summary>
        public TimeSpan CurrentTime { get; private set; }

        /// <summary>
        /// Gets the time in milliseconds till this animation is finished.
        /// This method is relevant for event-driven processing and tells the system by what amound the clock is to be increased next.
        /// </summary>
        /// <param name="previousMinFinishTime">The minimum TimeSpan previous animations take.</param>
        /// <param name="previousMaxFinishTime">The maximum TimeSpan previous animations take.</param>
        /// <param name="defaultCycleTime">The default cycle time if we would be in continous calculation mode.</param>
        public virtual TimeSpan GetTimeTillNextEvent(TimeSpan previousMinFinishTime, TimeSpan previousMaxFinishTime, TimeSpan defaultCycleTime)
        {
            // Trigger start logic if not done so before
            // .. => This is needed because some animations calculate their fixed time wihin the start method
            HandleStartAnimation();

            // Handle the animation type
            switch (AnimationType)
            {
                case AnimationType.AsyncCall:
                    return defaultCycleTime;

                case AnimationType.FinishedByEvent:
                    return defaultCycleTime;

                case AnimationType.FixedTime:
                    if (FixedTime > CurrentTime) { return FixedTime - CurrentTime; }
                    else { return TimeSpan.Zero; }

                default:
                    throw new SeeingSharpGraphicsException("Unhandled animation type: " + AnimationType);
            }
        }

        /// <summary>
        /// Has this animation finished executing?
        /// </summary>
        public bool Finished { get; private set; }

        /// <summary>
        /// Is this animation a blocking animation?
        /// If true, all following animation have to wait for finish-event.
        /// </summary>
        public virtual bool IsBlockingAnimation => false;

        /// <summary>
        /// Gets the target object.
        /// </summary>
        public object TargetObject { get; }

        /// <summary>
        /// Is this animation canceled?
        /// </summary>
        public bool Canceled
        {
            get => m_canceled;
            set
            {
                if (m_canceled == value)
                {
                    return;
                }

                m_canceled = value;

                if (m_canceled)
                {
                    OnCanceled();
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
    }
}