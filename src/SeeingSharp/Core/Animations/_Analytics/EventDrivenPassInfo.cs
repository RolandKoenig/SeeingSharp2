using System;
using System.Collections.Generic;

namespace SeeingSharp.Core.Animations
{
    /// <summary>
    /// Holds some detail information about event-driven animation calculation.
    /// </summary>
    public class EventDrivenPassInfo
    {
        public List<EventDrivenStepInfo> Steps
        {
            get;
        }

        /// <summary>
        /// Gets the total time the animation took (simulation time).
        /// </summary>
        public TimeSpan TotalTime
        {
            get
            {
                var totalTime = TimeSpan.Zero;
                foreach (var actAnimStep in this.Steps)
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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventDrivenPassInfo"/> class.
        /// </summary>
        /// <param name="steps">All steps performed in this calculation.</param>
        internal EventDrivenPassInfo(List<EventDrivenStepInfo> steps)
        {
            this.CountSteps = steps.Count;
            this.Steps = steps;
        }
    }
}
