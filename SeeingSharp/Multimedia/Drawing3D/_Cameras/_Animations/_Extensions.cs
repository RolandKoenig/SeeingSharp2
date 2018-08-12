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
using SeeingSharp.Multimedia.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public static class AnimatableObjectExtensions
    {
        /// <summary>
        /// Moves the given Camera to the given ViewPoint.
        /// </summary>
        /// <param name="sequenceBuilder">AnimationSequenceBuilder building the animation.</param>
        /// <param name="targetViewPoint">The target ViewPoint object.</param>
        /// <param name="animationTime">Total time for the animation.</param>
        public static IAnimationSequenceBuilder<TargetObject> CameraStraightMoveTo<TargetObject>(this IAnimationSequenceBuilder<TargetObject> sequenceBuilder, Camera3DViewPoint targetViewPoint, TimeSpan animationTime)
            where TargetObject : Camera3DBase
        {
            sequenceBuilder.Add(
                new CameraStraightMoveAnimation(sequenceBuilder.TargetObject as Camera3DBase, targetViewPoint, animationTime));
            return sequenceBuilder;
        }
    }
}
