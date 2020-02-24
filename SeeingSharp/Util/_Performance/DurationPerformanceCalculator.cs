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
    public class DurationPerformanceCalculator
    {
        private RingBuffer<ActivityDurationInfo> m_lastDurationItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="DurationPerformanceCalculator"/> class.
        /// </summary>
        public DurationPerformanceCalculator(string activityName, int maxHistoricalItems)
        {
            this.ActivityName = activityName;

            m_lastDurationItems = new RingBuffer<ActivityDurationInfo>(maxHistoricalItems);
        }

        /// <summary>
        /// Notifies the a done activity and it's duration.
        /// </summary>
        /// <param name="durationTicks">Total ticks the activity took.</param>
        internal void NotifyActivityDuration(long durationTicks)
        {
            ref var actItem = ref m_lastDurationItems.AddByRef();
            actItem.TimeStamp = DateTime.UtcNow;
            actItem.DurationTicks = durationTicks;
        }

        /// <summary>
        /// Notifies the a done activity and it's duration.
        /// </summary>
        /// <param name="durationTicks">Total ticks the activity took.</param>
        /// <param name="timeStamp">The timestamp when the given value was measured.</param>
        internal void NotifyActivityDuration(long durationTicks, DateTime timeStamp)
        {
            if (m_lastDurationItems.Count > 0)
            {
                ref var lastItem = ref m_lastDurationItems.GetByRef(m_lastDurationItems.Count - 1);
                if (lastItem.TimeStamp > timeStamp)
                {
                    throw new ArgumentException(
                        $"Given timestamp {timeStamp} is smaller than last one {lastItem.TimeStamp}!",
                        nameof(timeStamp));
                }
            }

            ref var actItem = ref m_lastDurationItems.AddByRef();
            actItem.TimeStamp = timeStamp;
            actItem.DurationTicks = durationTicks;
        }

        /// <summary>
        /// Calculates a new value.
        /// </summary>
        /// <param name="result">The result that should be set during calculation.</param>
        /// <param name="minTimeStamp">The timestamp which is the minimum for current calculation step.</param>
        /// <param name="maxTimeStamp">The maximum timestamp up to which to calculate the next result value.</param>
        internal void Calculate(ref DurationPerformanceResult result, DateTime minTimeStamp, DateTime maxTimeStamp)
        {
            // Calculate result values
            var minValue = long.MaxValue;
            var maxValue = long.MinValue;
            long sumValue = 0;
            long itemCount = 0;
            if (m_lastDurationItems.Count > 0)
            {
                for (var loop = 0; loop < m_lastDurationItems.Count; loop++)
                {
                    ref var actItem = ref m_lastDurationItems.GetByRef(loop);
                    if (actItem.TimeStamp < minTimeStamp)
                    {
                        m_lastDurationItems.RemoveFirst();
                        loop--;
                        continue;
                    }
                    if(actItem.TimeStamp >= maxTimeStamp) { break; }

                    if (minValue > actItem.DurationTicks) { minValue = actItem.DurationTicks; }
                    if (maxValue < actItem.DurationTicks) { maxValue = actItem.DurationTicks; }
                    sumValue += actItem.DurationTicks;
                    itemCount++;

                    m_lastDurationItems.RemoveFirst();
                    loop--;
                }
            }

            // Calculate average time value
            var avgValue = 0L;
            if (itemCount > 0)
            {
                avgValue = sumValue / itemCount;
            }

            // Create result object
            if (result == null)
            {
                result = new DurationPerformanceResult(this.ActivityName, maxTimeStamp, avgValue, maxValue, minValue);
            }
            else
            {
                result.Update(maxTimeStamp, avgValue, maxValue, minValue);
            }
        }

        public string ActivityName { get; }

        public int RawDataEntries => m_lastDurationItems.Count;

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private struct ActivityDurationInfo
        {
            public DateTime TimeStamp;
            public long DurationTicks;
        }
    }
}