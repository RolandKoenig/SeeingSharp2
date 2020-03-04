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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace SeeingSharp.Util
{
    public class PerformanceAnalyzer
    {
        private const int INIT_COUNT_CACHED_MEASURE_TOKENS = 32;
        private static readonly TimeSpan CALCULATOR_THROWAWAY_TIMEOUT = TimeSpan.FromSeconds(5.0);

        // Current state
        private ConcurrentObjectPool<DurationMeasureToken> m_cachedMeasureTokens;
        private UnsafeList<CalculatorInfo> m_cachedCalculators;
        private ConcurrentDictionary<string, CalculatorInfo> m_calculatorsDict;
        private int m_isCalculating;

        // Members for time ticks
        private DateTime m_lastValueTimestamp;
        private DateTime m_startupTimestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceAnalyzer"/> class.
        /// </summary>
        public PerformanceAnalyzer(TimeSpan valueInterval, int maxResultCountPerCalculator = 50)
        {
            m_lastValueTimestamp = DateTime.UtcNow;
            m_startupTimestamp = m_lastValueTimestamp;

            this.ValueInterval = valueInterval;
            this.MaxResultCountPerCalculator = maxResultCountPerCalculator;

            m_calculatorsDict = new ConcurrentDictionary<string, CalculatorInfo>();
            m_cachedCalculators = new UnsafeList<CalculatorInfo>(16);

            this.Internals = new PerformanceAnalyzerInternals(this);

            m_cachedMeasureTokens = new ConcurrentObjectPool<DurationMeasureToken>(
                () => new DurationMeasureToken(this),
                INIT_COUNT_CACHED_MEASURE_TOKENS);
        }

        /// <summary>
        /// Notifies that the given activityName took the given count of ticks.
        /// </summary>
        /// <param name="activity">The Activity to report to.</param>
        /// <param name="durationTicks">Total count of ticks to be notified.</param>
        internal void NotifyActivityDuration(string activity, long durationTicks)
        {
            if (m_isCalculating != 0)
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
            if (m_isCalculating != 0)
            {
                throw new InvalidOperationException($"{nameof(PerformanceAnalyzer)} is calculating currently, call to {nameof(this.ExecuteAndMeasureActivityDuration)} not allowed!");
            }

            var calculator = this.GetCalculator<DurationPerformanceCalculator>(activity);
            using (var token = m_cachedMeasureTokens.Rent())
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
            if (m_isCalculating != 0)
            {
                throw new InvalidOperationException($"{nameof(PerformanceAnalyzer)} is calculating currently, call to {nameof(this.BeginMeasureActivityDuration)} not allowed!");
            }

            var result = m_cachedMeasureTokens.Rent();
            result.Activity = activity;
            result.Start();

            return result;
        }

        internal void EndMeasureActivityDuration(DurationMeasureToken measureToken)
        {
            if (m_isCalculating != 0)
            {
                throw new InvalidOperationException($"{nameof(PerformanceAnalyzer)} is calculating currently, call to {nameof(this.EndMeasureActivityDuration)} not allowed!");
            }

            measureToken.Stop();

            this.NotifyActivityDuration(measureToken.Activity, measureToken.ElapsedTicks);

            m_cachedMeasureTokens.Return(measureToken);
        }

        /// <summary>
        /// Triggers calculation of
        /// </summary>
        internal void CalculateResults()
        {
            // Guard this object for changes during calculation
            // Reason is because we may delete some calculator objects when they are outdated
            if (Interlocked.Exchange(ref m_isCalculating, 1) != 0)
            {
                throw new InvalidOperationException($"{nameof(PerformanceAnalyzer)} is already calculating!");
            }

            try
            {
                var utcNow = DateTime.UtcNow;
                if (utcNow - this.ValueInterval < m_lastValueTimestamp)
                {
                    return;
                }

                // Trigger calculation
                var actKeyTimestamp = m_lastValueTimestamp + this.ValueInterval;
                while (actKeyTimestamp < utcNow)
                {
                    var actMaxTimestamp = actKeyTimestamp;
                    var actMinTimestamp = actKeyTimestamp - this.ValueInterval;
                    if (actMinTimestamp < m_startupTimestamp)
                    {
                        actMinTimestamp = m_startupTimestamp;
                    }

                    // Calculate reporting values
                    var calculatorsLength = m_cachedCalculators.Count;
                    var calculatorsArray = m_cachedCalculators.BackingArray;
                    for (var loop = 0; loop < calculatorsLength; loop++)
                    {
                        var actCalculatorInfo = calculatorsArray[loop];
                        if(actCalculatorInfo == null){ continue; }

                        // Remove this calculator if the last reported value came to long ago
                        if (utcNow - actCalculatorInfo.Calculator.LastReportedDurationTimestamp >
                            CALCULATOR_THROWAWAY_TIMEOUT)
                        {
                            m_cachedCalculators.RemoveAt(loop);
                            m_calculatorsDict.TryRemove(actCalculatorInfo.Calculator.ActivityName, out _);
                            loop--;
                            continue;
                        }

                        // Perform calculation
                        ref var actResult = ref actCalculatorInfo.Results.AddByRef();
                        actCalculatorInfo.Calculator.Calculate(ref actResult, actMinTimestamp, actMaxTimestamp);
                        actCalculatorInfo.CurrentResult = actResult;
                    }

                    // Handle next value timestamp
                    m_lastValueTimestamp = actKeyTimestamp;
                    actKeyTimestamp = m_lastValueTimestamp + this.ValueInterval;
                }
            }
            finally
            {
                Interlocked.Exchange(ref m_isCalculating, 0);
            }
        }

        /// <summary>
        /// Gets all current results.
        /// </summary>
        public IEnumerable<DurationPerformanceResult> GetCurrentResults()
        {
            // This call is not synchronized and may be called at any time (even when m_isCalculating is set)
            // Worst cases are that we
            //  a) miss new new calculation result
            //  b) get a result which was deleted from the list before
            // No real problem actually...

            var calculatorsLength = m_cachedCalculators.Count;
            var calculatorsArray = m_cachedCalculators.BackingArray;
            for (var loop = 0; loop < calculatorsLength; loop++)
            {
                var actCalculatorInfo = calculatorsArray[loop];

                if(actCalculatorInfo?.CurrentResult == null){ continue; }
                yield return actCalculatorInfo.CurrentResult;
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
                m_cachedCalculators.Add(calcInfo);
                return calcInfo;
            }

            var newCalculatorInfo = m_calculatorsDict.GetOrAdd(activityName, CreateCalculator);
            if (newCalculatorInfo?.Calculator == null)
            {
                throw new SeeingSharpException("Unable to create a calculator of type " + typeof(T) + " for activityName " + activityName + "!");
            }

            return newCalculatorInfo.Calculator;
        }

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

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Helper class for holding relevant information of the value calculators.
        /// </summary>
        private class CalculatorInfo
        {
            public CalculatorInfo(DurationPerformanceCalculator calculator, int maxResultCount)
            {
                Calculator = calculator;
                Results = new RingBuffer<DurationPerformanceResult>(maxResultCount);
            }

            public DurationPerformanceCalculator Calculator;
            public RingBuffer<DurationPerformanceResult> Results;
            public DurationPerformanceResult CurrentResult;
        }

        /// <summary>
        /// Accessors to internal methods.
        /// </summary>
        public class PerformanceAnalyzerInternals
        {
            private PerformanceAnalyzer m_owner;

            internal PerformanceAnalyzerInternals(PerformanceAnalyzer owner)
            {
                m_owner = owner;
            }

            public void NotifyActivityDuration(string activity, long durationTicks)
            {
                m_owner.NotifyActivityDuration(activity, durationTicks);
            }

            public void ExecuteAndMeasureActivityDuration(string activity, Action actionToExecute)
            {
                m_owner.ExecuteAndMeasureActivityDuration(activity, actionToExecute);
            }

            public IDisposable BeginMeasureActivityDuration(string activity)
            {
                return m_owner.BeginMeasureActivityDuration(activity);
            }
        }
    }
}