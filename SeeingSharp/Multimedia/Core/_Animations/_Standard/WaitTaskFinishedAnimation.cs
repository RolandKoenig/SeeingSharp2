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
using System.Threading.Tasks;

namespace SeeingSharp.Multimedia.Core
{
    public class WaitTaskFinishedAnimation : AnimationBase
    {
        private Task m_taskToWaitFor;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitTaskFinishedAnimation" /> class.
        /// </summary>
        public WaitTaskFinishedAnimation(Task taskToWaitFor)
            : base(null, AnimationType.FinishedByEvent)
        {
            m_taskToWaitFor = taskToWaitFor;
        }

        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            base.OnCurrentTimeUpdated(updateState, animationState);

            if(m_taskToWaitFor.IsCanceled ||
               m_taskToWaitFor.IsCompleted ||
               m_taskToWaitFor.IsFaulted)
            {
                base.NotifyAnimationFinished();
            }
        }

        /// <summary>
        /// Is this animation a blocking animation?
        /// If true, all following animation have to wait for finish-event.
        /// </summary>
        public override bool IsBlockingAnimation
        {
            get { return true; }
        }
    }
}