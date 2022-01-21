using System;

namespace SeeingSharp.Util
{
    public class DurationPerformanceResult
    {
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

        public DurationPerformanceResult(string activityName)
        {
            this.ActivityName = activityName;
        }

        public void Update(DateTime timestampKey, long itemCount, long sumAvgTicks, long sumMaxTicks, long sumMinTicks)
        {
            this.KeyTimestamp = timestampKey;
            this.ItemCount = itemCount;
            this.SumAverageTicks = sumAvgTicks;
            this.SumMaxTicks = sumMaxTicks;
            this.SumMinTicks = sumMinTicks;
        }
    }
}