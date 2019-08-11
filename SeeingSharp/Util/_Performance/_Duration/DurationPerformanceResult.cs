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
    public class DurationPerformanceResult : PerformanceAnalyzeResultBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DurationPerformanceResult"/> class.
        /// </summary>
        public DurationPerformanceResult(DurationPerformanceCalculator calculator, DateTime timestampKey, long sumAvgTicks, long sumMaxTicks, long sumMinTicks)
            : base(calculator, timestampKey)
        {
            this.SumAverageTicks = sumAvgTicks;
            this.SumMaxTicks = sumMaxTicks;
            this.SumMinTicks = sumMinTicks;
        }

        public long SumMaxTicks { get; set; }

        public long SumAverageTicks { get; set; }

        public long SumMinTicks { get; set; }

        /// <summary>
        /// Gets the FPS value.
        /// </summary>
        public int Fps
        {
            get
            {
                if (this.SumAverageTicks == 0) { return 0; }
                return (int)(10000000L / this.SumAverageTicks);
            }
        }

        public TimeSpan SumMax => TimeSpan.FromTicks(this.SumMaxTicks);

        public long SumMaxMS => (long)Math.Round(this.SumMax.TotalMilliseconds);

        public TimeSpan SumMin => TimeSpan.FromTicks(this.SumMinTicks);

        public long SumMinMS => (long)Math.Round(this.SumMin.TotalMilliseconds);

        public TimeSpan SumAverage => TimeSpan.FromTicks(this.SumAverageTicks);

        public long SumAverageMS => (long)Math.Round(this.SumAverage.TotalMilliseconds);

        /// <summary>
        /// Gets the average millisecond value as a double.
        /// </summary>

        public double SumAverageMSDouble => this.SumAverage.TotalMilliseconds;
    }
}