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
    public class ChangeIntByAnimation : AnimationBase
    {
        // Members for running animation
        private int _alreadyIncreased;

        // Configuration members
        private Func<int> _getValueFunc;
        private Action<int> _setValueAction;
        private int _increaseTotal;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeIntByAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="getValueFunc">The get value func.</param>
        /// <param name="setValueAction">The set value action.</param>
        /// <param name="increaseTotal">The increase total.</param>
        /// <param name="timeSpan">The timespan.</param>
        public ChangeIntByAnimation(object targetObject, Func<int> getValueFunc, Action<int> setValueAction, int increaseTotal, TimeSpan timeSpan)
            : base(targetObject, AnimationType.FixedTime, timeSpan)
        {
            _getValueFunc = getValueFunc;
            _setValueAction = setValueAction;
            _increaseTotal = increaseTotal;
        }

        /// <summary>
        /// Called when animation starts.
        /// </summary>
        protected override void OnStartAnimation()
        {
            _alreadyIncreased = 0;
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            var currentLocationPercent = (float)(this.CurrentTime.TotalMilliseconds / this.FixedTime.TotalMilliseconds);
            var toIncreaseTotal = (int)(_increaseTotal * currentLocationPercent);
            var toIncrease = toIncreaseTotal - _alreadyIncreased;

            _setValueAction(_getValueFunc() + toIncrease);

            _alreadyIncreased = toIncreaseTotal;
        }

        /// <summary>
        /// Is this animation a blocking animation?
        /// If true, all following animation have to wait for finish-event.
        /// </summary>
        public override bool IsBlockingAnimation => false;
    }
}