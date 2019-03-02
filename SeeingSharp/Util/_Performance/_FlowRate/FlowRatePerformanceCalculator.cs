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

using System;
using System.Linq;

namespace SeeingSharp.Util
{
    public class FlowRatePerformanceCalculator : PerformanceCalculatorBase
    {
        // Values used for calculation
        private ThreadSaveQueue<DateTime> m_lastReportedTimestamps;

        /// <summary>
        /// Notifies a new occurrence of the activity.
        /// </summary>
        internal void NotifyOccurrence()
        {
            m_lastReportedTimestamps.Enqueue(DateTime.UtcNow);
        }

        /// <summary>
        /// Notifies a new occurrence of the avitivity.
        /// </summary>
        /// <param name="timestamp">The timestamp of the occurrence.</param>
        internal void NotifyOccurrence(DateTime timestamp)
        {
            m_lastReportedTimestamps.Enqueue(timestamp);
        }

        /// <summary>
        /// Calculates a new kpi value based on given timestamp parameters.
        /// </summary>
        /// <param name="keyTimeStamp">The timestamp which is used for the result object.</param>
        /// <param name="minTimeStamp">The timestamp which is the minimum for current calculation step.</param>
        /// <param name="maxTimeStamp">The maximum timestamp up to which to calculate the next kpi.</param>
        /// <param name="calculationInterval">The interval from which to take all values from.</param>
        /// <returns></returns>
        internal override PerformanceAnalyzeResultBase Calculate(
            DateTime keyTimeStamp,
            DateTime minTimeStamp, DateTime maxTimeStamp,
            TimeSpan calculationInterval)
        {
            if (!m_lastReportedTimestamps.HasAny())
            {
                return new FlowRatePerformanceResult(this, keyTimeStamp, 0.0);
            }
            // Throw away all items which are too old
            foreach (var dummy in m_lastReportedTimestamps.DequeueWhile(actItem => actItem < minTimeStamp)) { }

            // Check again wether we have any items
            if (!m_lastReportedTimestamps.HasAny())
            {
                return new FlowRatePerformanceResult(this, keyTimeStamp, 0.0);
            }

            // Counts all relevant items
            double resultValue = m_lastReportedTimestamps
                .PeekWhile(actTuple => actTuple < maxTimeStamp)
                .Count();

            // Handle case where measured timespan in more less than the calculation timespan
            var currentValueTimespan = maxTimeStamp - minTimeStamp;

            if(currentValueTimespan != calculationInterval)
            {
                var timespanFactor = calculationInterval.Ticks / (double)currentValueTimespan.Ticks;
                resultValue *= timespanFactor;
            }

            // Generate the result object
            return new FlowRatePerformanceResult(this, keyTimeStamp, resultValue);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowRatePerformanceCalculator"/> class.
        /// </summary>
        /// <param name="calculatorName">Name of the calculator.</param>
        public FlowRatePerformanceCalculator(string calculatorName)
            : base(calculatorName)
        {
            m_lastReportedTimestamps = new ThreadSaveQueue<DateTime>();
        }
    }
}