using System;

namespace SeeingSharp.Util
{
    public class DurationPerformanceCalculator
    {
        private RingBuffer<ActivityDurationInfo> _lastDurationItems;
        private DateTime _lastReportedDurationTimestamp;

        public string ActivityName { get; }

        public int RawDataEntries => _lastDurationItems.Count;

        public DateTime LastReportedDurationTimestamp => _lastReportedDurationTimestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="DurationPerformanceCalculator"/> class.
        /// </summary>
        public DurationPerformanceCalculator(string activityName, int maxHistoricalItems)
        {
            this.ActivityName = activityName;

            _lastDurationItems = new RingBuffer<ActivityDurationInfo>(maxHistoricalItems);
        }

        /// <summary>
        /// Notifies the a done activity and it's duration.
        /// </summary>
        /// <param name="durationTicks">Total ticks the activity took.</param>
        internal void NotifyActivityDuration(long durationTicks)
        {
            var currentTimestamp = DateTime.UtcNow;
            _lastReportedDurationTimestamp = currentTimestamp;

            ref var actItem = ref _lastDurationItems.AddByRef();
            actItem.TimeStamp = currentTimestamp;
            actItem.DurationTicks = durationTicks;
        }

        /// <summary>
        /// Notifies the a done activity and it's duration.
        /// </summary>
        /// <param name="durationTicks">Total ticks the activity took.</param>
        /// <param name="timeStamp">The timestamp when the given value was measured.</param>
        internal void NotifyActivityDuration(long durationTicks, DateTime timeStamp)
        {
            if (_lastDurationItems.Count > 0)
            {
                ref var lastItem = ref _lastDurationItems.GetByRef(_lastDurationItems.Count - 1);
                if (lastItem.TimeStamp > timeStamp)
                {
                    throw new ArgumentException(
                        $"Given timestamp {timeStamp} is smaller than last one {lastItem.TimeStamp}!",
                        nameof(timeStamp));
                }
            }

            ref var actItem = ref _lastDurationItems.AddByRef();
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
            if (_lastDurationItems.Count > 0)
            {
                for (var loop = 0; loop < _lastDurationItems.Count; loop++)
                {
                    ref var actItem = ref _lastDurationItems.GetByRef(loop);
                    if (actItem.TimeStamp < minTimeStamp)
                    {
                        _lastDurationItems.RemoveFirst();
                        loop--;
                        continue;
                    }
                    if(actItem.TimeStamp >= maxTimeStamp) { break; }

                    if (minValue > actItem.DurationTicks) { minValue = actItem.DurationTicks; }
                    if (maxValue < actItem.DurationTicks) { maxValue = actItem.DurationTicks; }
                    sumValue += actItem.DurationTicks;
                    itemCount++;

                    _lastDurationItems.RemoveFirst();
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
                result = new DurationPerformanceResult(
                    this.ActivityName, maxTimeStamp, itemCount, 
                    avgValue, maxValue, minValue);
            }
            else
            {
                result.Update(
                    maxTimeStamp, itemCount, 
                    avgValue, maxValue, minValue);
            }
        }

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