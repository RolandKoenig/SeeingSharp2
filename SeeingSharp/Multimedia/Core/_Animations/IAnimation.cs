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
    public interface IAnimation
    {
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
        /// This method is relevant for event-driven processing and tells the system by what amound the clock is to be increased next.
        /// </summary>
        /// <param name="previousMinFinishTime">The minimum TimeSpan previous animations take.</param>
        /// <param name="previousMaxFinishTime">The maximum TimeSpan previous animations take.</param>
        /// <param name="defaultCycleTime">The default cycle time if we would be in continous calculation mode.</param>
        TimeSpan GetTimeTillNextEvent(TimeSpan previousMinFinishTime, TimeSpan previousMaxFinishTime, TimeSpan defaultCycleTime);

        /// <summary>
        /// Resets this animation.
        /// </summary>
        void Reset();

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
    }
}
