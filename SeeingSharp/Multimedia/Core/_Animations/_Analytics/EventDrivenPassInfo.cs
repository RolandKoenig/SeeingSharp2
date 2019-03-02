#region License information
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
#endregion
namespace SeeingSharp.Multimedia.Core
{
    #region using

    using System;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// Holds some detail information about event-driven animation calculation.
    /// </summary>
    public class EventDrivenPassInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventDrivenPassInfo"/> class.
        /// </summary>
        /// <param name="steps">All steps performed in this calculation.</param>
        internal EventDrivenPassInfo(List<EventDrivenStepInfo> steps)
        {
            this.CountSteps = steps.Count;
            this.Steps = steps;
        }

        public List<EventDrivenStepInfo> Steps
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the total time the animation took (simulation time).
        /// </summary>
        public TimeSpan TotalTime
        {
            get
            {
                if (Steps == null)
                {
                    return TimeSpan.Zero;
                }

                var totalTime = TimeSpan.Zero;

                foreach(var actAnimStep in this.Steps)
                {
                    totalTime = totalTime + actAnimStep.UpdateTime;
                }

                return totalTime;
            }
        }

        /// <summary>
        /// Total count of calculation steps.
        /// </summary>
        public int CountSteps
        {
            get;
            private set;
        }
    }
}
