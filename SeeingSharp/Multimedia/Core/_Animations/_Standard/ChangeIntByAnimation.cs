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
    public class ChangeIntByAnimation : AnimationBase
    {
        #region Configuration members
        private Func<int> m_getValueFunc;
        private Action<int> m_setValueAction;
        private int m_increaseTotal;
        private TimeSpan m_timeSpan;
        #endregion

        #region Members for running animation
        private int m_alreadyIncreased;
        #endregion

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
            m_getValueFunc = getValueFunc;
            m_setValueAction = setValueAction;
            m_increaseTotal = increaseTotal;
            m_timeSpan = timeSpan;
        }

        /// <summary>
        /// Called when animation starts.
        /// </summary>
        protected override void OnStartAnimation()
        {
            m_alreadyIncreased = 0;
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            float currentLocationPercent = (float)(base.CurrentTime.TotalMilliseconds / base.FixedTime.TotalMilliseconds);
            int toIncreaseTotal = (int)(m_increaseTotal * currentLocationPercent);
            int toIncrease = toIncreaseTotal - m_alreadyIncreased;

            m_setValueAction(m_getValueFunc() + toIncrease);

            m_alreadyIncreased = toIncreaseTotal;
        }

        /// <summary>
        /// Is this animation a blocking animation?
        /// If true, all following animation have to wait for finish-event.
        /// </summary>
        public override bool IsBlockingAnimation
        {
            get { return false; }
        }
    }
}