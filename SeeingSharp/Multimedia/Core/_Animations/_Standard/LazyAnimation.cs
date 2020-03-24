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

namespace SeeingSharp.Multimedia.Core
{
    public class LazyAnimation : IAnimation
    {
        private IAnimation _animation;
        private Func<IAnimation> _animationCreator;

        /// <summary>
        /// Is the animation finished?
        /// </summary>
        public bool Finished
        {
            get
            {
                if (_animation == null) { _animation = _animationCreator(); }
                if (_animation == null) { return true; }
                return _animation.Finished;
            }
        }

        /// <summary>
        /// Is this animation a blocking animation?
        /// </summary>
        public bool IsBlockingAnimation
        {
            get
            {
                if (_animation == null) { _animation = _animationCreator(); }
                if (_animation == null) { return false; }
                return _animation.IsBlockingAnimation;
            }
        }

        /// <summary>
        /// Is this animation canceled?
        /// </summary>
        public bool Canceled
        {
            get;
            set;
        }

        public bool IgnorePauseState
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyAnimation"/> class.
        /// </summary>
        /// <param name="animationCreator">The animation creator.</param>
        public LazyAnimation(Func<IAnimation> animationCreator)
        {
            _animationCreator = animationCreator;
            _animation = null;
        }

        /// <summary>
        /// Checks if the given object is animated by this animation.
        /// </summary>
        /// <param name="targetObject">The object to check for.</param>
        public bool IsObjectAnimated(object targetObject)
        {
            if (_animation == null) { _animation = _animationCreator(); }
            if (_animation == null) { return false; }

            return _animation.IsObjectAnimated(targetObject);
        }

        /// <summary>
        /// Called for each update step of this animation.
        /// </summary>
        /// <param name="updateState">The current state of the update pass.</param>
        /// <param name="animationState">The current state of the animation.</param>
        public AnimationUpdateResult Update(IAnimationUpdateState updateState, AnimationState animationState)
        {
            if (_animation == null) { _animation = _animationCreator(); }
            if (_animation == null) { return AnimationUpdateResult.EMPTY; }

            return _animation.Update(updateState, animationState);
        }

        /// <summary>
        /// Resets this animation.
        /// </summary>
        public void Reset()
        {
            if (_animation == null) { _animation = _animationCreator(); }

            _animation?.Reset();
        }

        /// <summary>
        /// Gets the time in milliseconds till this animation is finished.
        /// This method is relevant for event-driven processing and tells the system by what amount the clock is to be increased next.
        /// </summary>
        /// <param name="previousMinFinishTime">The minimum TimeSpan previous animations take.</param>
        /// <param name="previousMaxFinishTime">The maximum TimeSpan previous animations take.</param>
        /// <param name="defaultCycleTime">The default cycle time if we would be in continuous calculation mode.</param>
        public TimeSpan GetTimeTillNextEvent(TimeSpan previousMinFinishTime, TimeSpan previousMaxFinishTime, TimeSpan defaultCycleTime)
        {
            if (_animation == null) { _animation = _animationCreator(); }
            if (_animation == null) { return TimeSpan.Zero; }

            return _animation.GetTimeTillNextEvent(previousMinFinishTime, previousMaxFinishTime, defaultCycleTime);
        }
    }
}
