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

namespace SeeingSharp.Util
{
    public class FlowRatePerformanceCalculator : PerformanceCalculatorBase
    {
        // Values used for calculation
        private ThreadSaveQueue<DateTime> m_lastReportedTimestamps;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowRatePerformanceCalculator"/> class.
        /// </summary>
        /// <param name="calculatorName">Name of the calculator.</param>
        public FlowRatePerformanceCalculator(string calculatorName)
            : base(calculatorName)
        {
            m_lastReportedTimestamps = new ThreadSaveQueue<DateTime>();
        }

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
            else
            {
                // Throw away all items which are too old
                foreach (var dummy in m_lastReportedTimestamps.DequeueWhile((actItem) => actItem < minTimeStamp)) { }

                // Check again wether we have any items
                if (!m_lastReportedTimestamps.HasAny())
                {
                    return new FlowRatePerformanceResult(this, keyTimeStamp, 0.0);
                }

                // Counts all relevant items
                double resultValue = (double)(m_lastReportedTimestamps
                    .PeekWhile((actTuple) => actTuple < maxTimeStamp)
                    .Count());
              
                // Handle case where measured timespan in more less than the calculation timespan
                TimeSpan currentValueTimespan = maxTimeStamp - minTimeStamp;
                if(currentValueTimespan != calculationInterval)
                {
                    double timespanFactor = (double)calculationInterval.Ticks / (double)currentValueTimespan.Ticks;
                    resultValue *= timespanFactor;
                }

                // Generate the result object
                return new FlowRatePerformanceResult(this, keyTimeStamp, resultValue);
            }
        }
    }
}