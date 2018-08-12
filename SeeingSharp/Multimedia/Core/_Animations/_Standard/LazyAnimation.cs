#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.Multimedia.Core
{
    public class LazyAnimation : IAnimation
    {
        private Func<IAnimation> m_animationCreator;
        private IAnimation m_animation;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyAnimation"/> class.
        /// </summary>
        /// <param name="animationCreator">The animation creator.</param>
        public LazyAnimation(Func<IAnimation> animationCreator)
        {
            m_animationCreator = animationCreator;
            m_animation = null;
        }

        /// <summary>
        /// Checks if the given object is animated by this animation.
        /// </summary>
        /// <param name="targetObject">The object to check for.</param>
        public bool IsObjectAnimated(object targetObject)
        {
            if (m_animation == null) { m_animation = m_animationCreator(); }
            if (m_animation == null) { return false; }

            return m_animation.IsObjectAnimated(targetObject);
        }

        /// <summary>
        /// Called for each update step of this animation.
        /// </summary>
        /// <param name="updateState">The current state of the update pass.</param>
        /// <param name="animationState">The current state of the animation.</param>
        public AnimationUpdateResult Update(IAnimationUpdateState updateState, AnimationState animationState)
        {
            if (m_animation == null) { m_animation = m_animationCreator(); }
            if (m_animation == null) { return AnimationUpdateResult.Empty; }

            return m_animation.Update(updateState, animationState); 
        }

        /// <summary>
        /// Resets this animation.
        /// </summary>
        public void Reset()
        {
            if (m_animation == null) { m_animation = m_animationCreator(); }
            if (m_animation == null) { return; }

            m_animation.Reset();
        }

        /// <summary>
        /// Gets the time in milliseconds till this animation is finished.
        /// This method is relevant for event-driven processing and tells the system by what amound the clock is to be increased next.
        /// </summary>
        /// <param name="previousMinFinishTime">The minimum TimeSpan previous animations take.</param>
        /// <param name="previousMaxFinishTime">The maximum TimeSpan previous animations take.</param>
        /// <param name="defaultCycleTime">The default cycle time if we would be in continous calculation mode.</param>
        public TimeSpan GetTimeTillNextEvent(TimeSpan previousMinFinishTime, TimeSpan previousMaxFinishTime, TimeSpan defaultCycleTime)
        {
            if (m_animation == null) { m_animation = m_animationCreator(); }
            if (m_animation == null) { return TimeSpan.Zero; }

            return m_animation.GetTimeTillNextEvent(previousMinFinishTime, previousMaxFinishTime, defaultCycleTime);
        }

        /// <summary>
        /// Is the animation finished?
        /// </summary>
        public bool Finished
        {
            get 
            {
                if (m_animation == null) { m_animation = m_animationCreator(); }
                if (m_animation == null) { return true; }
                return m_animation.Finished;
            }
        }

        /// <summary>
        /// Is this animation a blocking animation?
        /// </summary>
        public bool IsBlockingAnimation
        {
            get 
            {
                if (m_animation == null) { m_animation = m_animationCreator(); }
                if (m_animation == null) { return false; }
                return m_animation.IsBlockingAnimation;
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
    }
}
