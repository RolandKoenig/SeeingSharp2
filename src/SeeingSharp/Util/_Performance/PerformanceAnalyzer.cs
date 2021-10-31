/*
    SeeingSharp and all applications distributed together with it. 
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace SeeingSharp.Util
{
    public class PerformanceAnalyzer
    {
        private const int INIT_COUNT_CACHED_MEASURE_TOKENS = 32;
        private static readonly TimeSpan s_calculatorThrowawayTimeout = TimeSpan.FromSeconds(5.0);

        // Current state
        private ConcurrentObjectPool<DurationMeasureToken> _cachedMeasureTokens;
        private UnsafeList<CalculatorInfo> _cachedCalculators;
        private ConcurrentDictionary<string, CalculatorInfo> _calculatorsDict;
        private int _isCalculating;

        // Members for time ticks
        private DateTime _lastValueTimestamp;
        private DateTime _startupTimestamp;

        /// <summary>
        /// The interval for which values are produced.
        /// (Calculating is triggered after value interval).
        /// </summary>
        public TimeSpan ValueInterval { get; }

        /// <summary>
        /// The Maximum count of historical entries.
        /// </summary>
        public int MaxResultCountPerCalculator { get; }

        /// <summary>
        /// Accessor to internal methods/members.
        /// </summary>
        public PerformanceAnalyzerInternals Internals { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceAnalyzer"/> class.
        /// </summary>
        public PerformanceAnalyzer(TimeSpan valueInterval, int maxResultCountPerCalculator = 50)
        {
            _lastValueTimestamp = DateTime.UtcNow;
            _startupTimestamp = _lastValueTimestamp;

            this.ValueInterval = valueInterval;
            this.MaxResultCountPerCalculator = maxResultCountPerCalculator;

            _calculatorsDict = new ConcurrentDictionary<string, CalculatorInfo>();
            _cachedCalculators = new UnsafeList<CalculatorInfo>(16);

            this.Internals = new PerformanceAnalyzerInternals(this);

            _cachedMeasureTokens = new ConcurrentObjectPool<DurationMeasureToken>(
                () => new DurationMeasureToken(this),
                INIT_COUNT_CACHED_MEASURE_TOKENS);
        }

        /// <summary>
        /// Gets all current results.
        /// </summary>
        public IEnumerable<DurationPerformanceResult> GetCurrentResults()
        {
            // This call is not synchronized and may be called at any time (even when _isCalculating is set)
            // Worst cases are that we
            //  a) miss new new calculation result
            //  b) get a result which was deleted from the list before
            // No real problem actually...

            var calculatorsLength = _cachedCalculators.Count;
            var calculatorsArray = _cachedCalculators.BackingArray;
            for (var loop = 0; loop < calculatorsLength; loop++)
            {
                var actCalculatorInfo = calculatorsArray[loop];

                if(actCalculatorInfo?.CurrentResult == null){ continue; }
                yield return actCalculatorInfo.CurrentResult;
            }
        }

        /// <summary>
        /// Notifies that the given activityName took the given count of ticks.
        /// </summary>
        /// <param name="activity">The Activity to report to.</param>
        /// <param name="durationTicks">Total count of ticks to be notified.</param>
        internal void NotifyActivityDuration(string activity, long durationTicks)
        {
            if (_isCalculating != 0)
            {
                throw new InvalidOperationException($"{nameof(PerformanceAnalyzer)} is calculating currently, call to {nameof(this.NotifyActivityDuration)} not allowed!");
            }

            var calculator = this.GetCalculator<DurationPerformanceCalculator>(activity);
            calculator.NotifyActivityDuration(durationTicks);
        }

        /// <summary>
        /// Executes the given action and measures the time it took in total.
        /// </summary>
        /// <param name="activity">The Activity to report to.</param>
        /// <param name="actionToExecute">The action to be executed and measured.</param>
        internal void ExecuteAndMeasureActivityDuration(string activity, Action actionToExecute)
        {
            if (_isCalculating != 0)
            {
                throw new InvalidOperationException($"{nameof(PerformanceAnalyzer)} is calculating currently, call to {nameof(this.ExecuteAndMeasureActivityDuration)} not allowed!");
            }

            var calculator = this.GetCalculator<DurationPerformanceCalculator>(activity);
            using (var token = _cachedMeasureTokens.Rent())
            {
                token.Start();

                actionToExecute();

                token.Stop();
                calculator.NotifyActivityDuration(token.ElapsedTicks);
            }
        }

        /// <summary>
        /// Begins measuring the duration of the given activityName (end of the duration is when Dispose gets called on the result).
        /// </summary>
        /// <param name="activity">The activityName name to be measured.</param>
        internal DurationMeasureToken BeginMeasureActivityDuration(string activity)
        {
            if (_isCalculating != 0)
            {
                throw new InvalidOperationException($"{nameof(PerformanceAnalyzer)} is calculating currently, call to {nameof(this.BeginMeasureActivityDuration)} not allowed!");
            }

            var result = _cachedMeasureTokens.Rent();
            result.Activity = activity;
            result.Start();

            return result;
        }

        internal void EndMeasureActivityDuration(DurationMeasureToken measureToken)
        {
            if (_isCalculating != 0)
            {
                throw new InvalidOperationException($"{nameof(PerformanceAnalyzer)} is calculating currently, call to {nameof(this.EndMeasureActivityDuration)} not allowed!");
            }

            measureToken.Stop();

            this.NotifyActivityDuration(measureToken.Activity, measureToken.ElapsedTicks);

            _cachedMeasureTokens.Return(measureToken);
        }

        /// <summary>
        /// Triggers calculation of
        /// </summary>
        internal void CalculateResults()
        {
            // Guard this object for changes during calculation
            // Reason is because we may delete some calculator objects when they are outdated
            if (Interlocked.Exchange(ref _isCalculating, 1) != 0)
            {
                throw new InvalidOperationException($"{nameof(PerformanceAnalyzer)} is already calculating!");
            }

            try
            {
                var utcNow = DateTime.UtcNow;
                if (utcNow - this.ValueInterval < _lastValueTimestamp)
                {
                    return;
                }

                // Trigger calculation
                var actKeyTimestamp = _lastValueTimestamp + this.ValueInterval;
                while (actKeyTimestamp < utcNow)
                {
                    var actMaxTimestamp = actKeyTimestamp;
                    var actMinTimestamp = actKeyTimestamp - this.ValueInterval;
                    if (actMinTimestamp < _startupTimestamp)
                    {
                        actMinTimestamp = _startupTimestamp;
                    }

                    // Calculate reporting values
                    var calculatorsLength = _cachedCalculators.Count;
                    var calculatorsArray = _cachedCalculators.BackingArray;
                    for (var loop = 0; loop < calculatorsLength; loop++)
                    {
                        var actCalculatorInfo = calculatorsArray[loop];
                        if(actCalculatorInfo == null){ continue; }

                        // Remove this calculator if the last reported value came to long ago
                        if (utcNow - actCalculatorInfo.Calculator.LastReportedDurationTimestamp >
                            s_calculatorThrowawayTimeout)
                        {
                            _cachedCalculators.RemoveAt(loop);
                            _calculatorsDict.TryRemove(actCalculatorInfo.Calculator.ActivityName, out _);
                            loop--;
                            continue;
                        }

                        // Perform calculation
                        ref var actResult = ref actCalculatorInfo.Results.AddByRef();
                        actCalculatorInfo.Calculator.Calculate(ref actResult, actMinTimestamp, actMaxTimestamp);
                        actCalculatorInfo.CurrentResult = actResult;
                    }

                    // Handle next value timestamp
                    _lastValueTimestamp = actKeyTimestamp;
                    actKeyTimestamp = _lastValueTimestamp + this.ValueInterval;
                }
            }
            finally
            {
                Interlocked.Exchange(ref _isCalculating, 0);
            }
        }

        /// <summary>
        /// Gets the calculator for the given activityName.
        /// </summary>
        /// <typeparam name="T">The type of the calculator to get.</typeparam>
        /// <param name="activityName">The name of the activityName.</param>
        private DurationPerformanceCalculator GetCalculator<T>(string activityName)
        {
            CalculatorInfo CreateCalculator(string key)
            {
                var newCalculator = new DurationPerformanceCalculator(activityName, 1000);

                var calcInfo = new CalculatorInfo(newCalculator, this.MaxResultCountPerCalculator);
                _cachedCalculators.Add(calcInfo);
                return calcInfo;
            }

            var newCalculatorInfo = _calculatorsDict.GetOrAdd(activityName, CreateCalculator);
            if (newCalculatorInfo?.Calculator == null)
            {
                throw new SeeingSharpException("Unable to create a calculator of type " + typeof(T) + " for activityName " + activityName + "!");
            }

            return newCalculatorInfo.Calculator;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Helper class for holding relevant information of the value calculators.
        /// </summary>
        private class CalculatorInfo
        {
            public DurationPerformanceCalculator Calculator;
            public RingBuffer<DurationPerformanceResult> Results;
            public DurationPerformanceResult CurrentResult;

            public CalculatorInfo(DurationPerformanceCalculator calculator, int maxResultCount)
            {
                Calculator = calculator;
                Results = new RingBuffer<DurationPerformanceResult>(maxResultCount);
            }
        }

        /// <summary>
        /// Accessors to internal methods.
        /// </summary>
        public class PerformanceAnalyzerInternals
        {
            private PerformanceAnalyzer _owner;

            internal PerformanceAnalyzerInternals(PerformanceAnalyzer owner)
            {
                _owner = owner;
            }

            public void NotifyActivityDuration(string activity, long durationTicks)
            {
                _owner.NotifyActivityDuration(activity, durationTicks);
            }

            public void ExecuteAndMeasureActivityDuration(string activity, Action actionToExecute)
            {
                _owner.ExecuteAndMeasureActivityDuration(activity, actionToExecute);
            }

            public IDisposable BeginMeasureActivityDuration(string activity)
            {
                return _owner.BeginMeasureActivityDuration(activity);
            }
        }
    }
}