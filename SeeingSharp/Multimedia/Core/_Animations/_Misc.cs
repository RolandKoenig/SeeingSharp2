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

namespace SeeingSharp.Multimedia.Core
{
    /// <summary>
    /// Describes the component which is relevant for rotation calculation.
    /// </summary>
    [Flags]
    public enum RotationCalculationComponent : int
    {
        All = RotationCalculationComponent.Pitch | RotationCalculationComponent.Yaw | RotationCalculationComponent.Roll,

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
        /// An animation changes the state of an object by a given transition vlaue (e. g. move by vector).
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
        public static readonly AnimationUpdateResult Empty = new AnimationUpdateResult();

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
        /// Remove the failed part from the animation and continue with remaining.
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
        /// Initializes a new instance of the <see cref="AnimationFailedEventArgs"/> class.
        /// </summary>
        /// <param name="animation">The animation.</param>
        /// <param name="exception">The exception.</param>
        internal AnimationFailedEventArgs(IAnimation animation, Exception exception)
        {
            this.Animation = animation;
            this.Exception = exception;
        }

        /// <summary>
        /// Gets the animation that has failed.
        /// </summary>
        public IAnimation Animation
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the exception occurred during execution of the animation.
        /// </summary>
        public Exception Exception
        {
            get;
            private set;
        }
    }
}