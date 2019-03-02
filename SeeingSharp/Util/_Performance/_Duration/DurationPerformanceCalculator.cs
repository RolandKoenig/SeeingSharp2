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

namespace SeeingSharp.Util
{
    public class DurationPerformanceCalculator : PerformanceCalculatorBase
    {
        //Values used for calculation
        private ThreadSaveQueue<Tuple<DateTime, long>> m_lastDurationItems;

        /// <summary>
        /// Notifies the a done activity and it's duration.
        /// </summary>
        /// <param name="durationTicks">Total ticks the activity took.</param>
        internal void NotifyActivityDuration(long durationTicks)
        {
            m_lastDurationItems.Enqueue(Tuple.Create(DateTime.UtcNow, durationTicks));
        }

        /// <summary>
        /// Calculates a new kpi value.
        /// </summary>
        /// <param name="keyTimeStamp">The timestamp which is used for the result object.</param>
        /// <param name="minTimeStamp">The timestamp which is the minimum for current calculation step.</param>
        /// <param name="maxTimeStamp">The maximum timestamp up to which to calculate the next kpi.</param>
        /// <param name="calculationInterval">The interval from which to take all values from.</param>
        internal override PerformanceAnalyzeResultBase Calculate(
            DateTime keyTimeStamp,
            DateTime minTimeStamp, DateTime maxTimeStamp,
            TimeSpan calculationInterval)
        {
            if (!m_lastDurationItems.HasAny())
            {
                return null;
            }
            // Throw away all items which are too old
            foreach (var dummy in m_lastDurationItems.DequeueWhile(actItem => actItem.Item1 < minTimeStamp)) { }

            // Check again wether we have any items
            if(!m_lastDurationItems.HasAny())
            {
                return null;
            }

            // Calculate result values
            var minValue = long.MaxValue;
            var maxValue = long.MinValue;
            long sumValue = 0;
            long itemCount = 0;
            foreach (var actItem in m_lastDurationItems.PeekWhile(actTuple => actTuple.Item1 < maxTimeStamp))
            {
                if (minValue > actItem.Item2) { minValue = actItem.Item2; }
                if (maxValue < actItem.Item2) { maxValue = actItem.Item2; }
                sumValue += actItem.Item2;
                itemCount++;
            }

            // Check again wether we have any items
            if(itemCount == 0)
            {
                return null;
            }

            // Calculate average time value
            var avgValue = sumValue / itemCount;

            // Create result object
            return new DurationPerformanceResult(this, keyTimeStamp, avgValue, maxValue, minValue);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DurationPerformanceCalculator"/> class.
        /// </summary>
        public DurationPerformanceCalculator(string calculatorName)
            : base(calculatorName)
        {
            m_lastDurationItems = new ThreadSaveQueue<Tuple<DateTime, long>>();
        }
    }
}