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
    public interface IAnimationSequenceBuilder<out TargetType>
        where TargetType : class
    {
        /// <summary>
        /// Adds an AnimationSequence to this builder.
        /// </summary>
        /// <param name="animationSequence">The animation sequence to be added.</param>
        IAnimationSequenceBuilder<TargetType> Add(IAnimation animationSequence);

        /// <summary>
        /// Finishes the AnimationSequence and adds it to the AninationHandler it was created with.
        /// </summary>
        /// <param name="actionToCall">The action to be called after animation has finished.</param>
        /// <param name="cancelAction">The action to be called when the animation gets canceled.</param>
        /// <param name="ignorePause">Should this animation ignore pause stateß</param>
        void Apply(Action actionToCall = null, Action cancelAction = null, bool? ignorePause = null);

        /// <summary>
        /// Finishes the AnimationSequence and adds it to the AninationHandler it was created with.
        /// The caller can await the finish of this animation using the returned task object.
        /// </summary>
        Task ApplyAsync();

        /// <summary>
        /// Finishes the animation and starts from beginning.
        /// </summary>
        /// <param name="ignorePause">Should this animation ignore pause stateß</param>
        void ApplyAndRewind(bool? ignorePause = null);

        /// <summary>
        /// Finishes the AnimationSequence and adds it to the AninationHandler it was created with.
        /// </summary>
        /// <param name="actionToCall">The action to be called after animation has finished.</param>
        /// <param name="cancelAction">The action to be called when the animation gets canceled.</param>
        /// <param name="ignorePause">Should this animation ignore pause stateß</param>
        void ApplyAsSecondary(Action actionToCall = null, Action cancelAction = null, bool? ignorePause = null);

        /// <summary>
        /// Finishes the AnimationSequence and adds it to the AninationHandler it was created with.
        /// The caller can await the finish of this animation using the returned task object.
        /// </summary>
        Task ApplyAsSecondaryAsync();

        /// <summary>
        /// Finishes the animation and starts from beginning.
        /// </summary>
        void ApplyAsSecondaryAndRewind();

        /// <summary>
        /// Gets the target object.
        /// </summary>
        TargetType TargetObject
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
    }
}
