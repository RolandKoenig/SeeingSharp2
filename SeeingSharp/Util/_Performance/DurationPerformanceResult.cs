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
    public class DurationPerformanceResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DurationPerformanceResult"/> class.
        /// </summary>
        public DurationPerformanceResult(string activityName, DateTime timestampKey, long itemCount, long sumAvgTicks, long sumMaxTicks, long sumMinTicks)
        {
            this.ActivityName = activityName;

            this.Update(timestampKey, itemCount, sumAvgTicks, sumMaxTicks, sumMinTicks);
        }

        public void Update(DateTime timestampKey, long itemCount, long sumAvgTicks, long sumMaxTicks, long sumMinTicks)
        {
            this.KeyTimestamp = timestampKey;
            this.ItemCount = itemCount;
            this.SumAverageTicks = sumAvgTicks;
            this.SumMaxTicks = sumMaxTicks;
            this.SumMinTicks = sumMinTicks;
        }

        public string ActivityName { get; }

        /// <summary>
        /// Gets the key of this value.
        /// </summary>
        public DateTime KeyTimestamp { get; private set; }

        public long ItemCount { get; private set; }

        public long SumMaxTicks { get; private set; }

        public long SumAverageTicks { get; private set; }

        public long SumMinTicks { get; private set; }

        public TimeSpan SumMax => TimeSpan.FromTicks(this.SumMaxTicks);

        public long SumMaxMs => (long)Math.Round(this.SumMax.TotalMilliseconds);

        public TimeSpan SumMin => TimeSpan.FromTicks(this.SumMinTicks);

        public long SumMinMs => (long)Math.Round(this.SumMin.TotalMilliseconds);

        public TimeSpan SumAverage => TimeSpan.FromTicks(this.SumAverageTicks);

        public long SumAverageMs => (long)Math.Round(this.SumAverage.TotalMilliseconds);

        /// <summary>
        /// Gets the average millisecond value as a double.
        /// </summary>

        public double SumAverageMsDouble => this.SumAverage.TotalMilliseconds;
    }
}