using System;
using System.Diagnostics;

namespace SeeingSharp.Util
{
    public class DurationMeasureToken : IDisposable
    {
        private PerformanceAnalyzer _owner;
        private Stopwatch _stopwatch;

        public string Activity { get; internal set; }

        public long ElapsedTicks => _stopwatch.Elapsed.Ticks;

        public DurationMeasureToken(PerformanceAnalyzer owner)
        {
            _owner = owner;
            _stopwatch = new Stopwatch();
        }

        public void Dispose()
        {
            _owner.EndMeasureActivityDuration(this);
        }

        internal void Start()
        {
            _stopwatch.Reset();
            _stopwatch.Start();
        }

        internal void Stop()
        {
            _stopwatch.Stop();
        }
    }
}
