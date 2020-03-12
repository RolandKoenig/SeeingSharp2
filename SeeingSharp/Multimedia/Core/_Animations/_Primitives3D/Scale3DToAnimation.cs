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
using System.Numerics;

namespace SeeingSharp.Multimedia.Core
{
    public class Scale3DToAnimation : AnimationBase
    {
        // Parameters
        private IAnimatableObjectScaling _targetObject;
        private Vector3 _targetScaleVector;

        // Runtime values
        private Vector3 _startScaleVector;
        private Vector3 _differenceVector;

        /// <summary>
        /// Initializes a new instance of the <see cref="Move3DByAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="scaleVector">The move vector.</param>
        /// <param name="duration">The duration.</param>
        public Scale3DToAnimation(IAnimatableObjectScaling targetObject, Vector3 scaleVector, TimeSpan duration)
            : base(targetObject, AnimationType.FixedTime, duration)
        {
            _targetObject = targetObject;
            _targetScaleVector = scaleVector;
        }

        /// <summary>
        /// Called when animation starts.
        /// </summary>
        protected override void OnStartAnimation()
        {
            _startScaleVector = _targetObject.Scaling;

            _differenceVector = _targetScaleVector - _startScaleVector;
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            var scaleFactor = this.CurrentTime.Ticks / (float)this.FixedTime.Ticks;

            _targetObject.Scaling = _startScaleVector + _differenceVector * scaleFactor;
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            _targetObject.Scaling = _targetScaleVector;
            _startScaleVector = Vector3.Zero;
            _differenceVector = Vector3.Zero;
        }
    }
}