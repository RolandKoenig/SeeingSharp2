#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion

namespace SeeingSharp.Util
{
    #region using

    using System;
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    #endregion

    public partial class PerformanceAnalyzer
    {
        private const double DEFAULT_KPI_INTERVAL_SEC = 5.0;

        // Configuration
        private TimeSpan m_valueInterval;
        private TimeSpan m_calculationInterval;
        private int m_maxCountHistoricalEntries;
        private bool m_generateHistoricalCollection;
        private bool m_generateCurrentValueCollection;

        // Members for time ticks
        private DateTime m_lastValueTimestamp;
        private DateTime m_startupTimestamp;

        // Members for threading
        private volatile int m_delayTimeMS;
        private Task m_runningTask;

        // Members for calculators
        private ConcurrentDictionary<string, CalculatorInfo> m_calculatorsDict;
        private ConcurrentBag<CalculatorInfo> m_calculatorsBag;

        // Collections for UI

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceAnalyzer"/> class.
        /// </summary>
        public PerformanceAnalyzer(TimeSpan valueInterval, TimeSpan calculationInterval)
        {
            m_lastValueTimestamp = DateTime.UtcNow;
            m_startupTimestamp = m_lastValueTimestamp;

            m_valueInterval = valueInterval;
            m_calculationInterval = calculationInterval;

            m_maxCountHistoricalEntries = 50;
            m_generateHistoricalCollection = true;
            m_generateCurrentValueCollection = true;

            SyncContext = SynchronizationContext.Current;
            m_delayTimeMS = 1000;

            m_calculatorsDict = new ConcurrentDictionary<string, CalculatorInfo>();
            m_calculatorsBag = new ConcurrentBag<CalculatorInfo>();

            UIDurationKpisHistorical = new ObservableCollection<DurationPerformanceResult>();
            UIDurationKpisCurrents = new ObservableCollection<DurationPerformanceResult>();
            UIFlowRateKpisHistorical = new ObservableCollection<FlowRatePerformanceResult>();
            UIFlowRateKpisCurrents = new ObservableCollection<FlowRatePerformanceResult>();
        }

        /// <summary>
        /// Notifies one occurrence of the FlowRate measurenemt with the given name.
        /// </summary>
        /// <param name="calculatorName">The name of the calculator this occurrence belongs to.</param>
        public void NotifyFlowRateOccurrence(string calculatorName)
        {
            var kpiCalculator = GetKpiCalculator<FlowRatePerformanceCalculator>(calculatorName);
            kpiCalculator.NotifyOccurrence();
        }

        /// <summary>
        /// Notifies that the given activity took the given count of ticks.
        /// </summary>
        /// <param name="activity">The Activity to report to.</param>
        /// <param name="durationTicks">Total count of ticks to be notified.</param>
        public void NotifyActivityDuration(string activity, long durationTicks)
        {
            var kpiCalculator = GetKpiCalculator<DurationPerformanceCalculator>(activity);
            kpiCalculator.NotifyActivityDuration(durationTicks);
        }

        /// <summary>
        /// Executes the given action and measures the time it took in total.
        /// </summary>
        /// <param name="activity">The Activity to report to.</param>
        /// <param name="actionToExecute">The action to be executed and measured.</param>
        public void ExecuteAndMeasureActivityDuration(string activity, Action actionToExecute)
        {
            var kpiCalculator = GetKpiCalculator<DurationPerformanceCalculator>(activity);
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                actionToExecute();
            }
            finally
            {
                kpiCalculator.NotifyActivityDuration(stopwatch.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Begins measuring the duration of the given activity (end of the duration is when Dispose gets called on the result).
        /// </summary>
        /// <param name="activity">The activity name to be measured.</param>
        public IDisposable BeginMeasureActivityDuration(string activity)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            return new DummyDisposable(() =>
            {
                NotifyActivityDuration(activity, stopwatch.Elapsed.Ticks);
            });
        }

        /// <summary>
        /// Starts the main loop of this OnlineKpiContainer.
        /// </summary>
        public Task RunAsync(CancellationToken cancelToken)
        {
            // Check for currently running task
            if ((m_runningTask != null) &&
                (m_runningTask.Status == TaskStatus.Running))
            {
                throw new SeeingSharpException("Unable to start OnlineKpiContainer: Main loop is already running!");
            }

            // Trigger new main loop
            m_runningTask = Task.Factory.StartNew(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(m_delayTimeMS);

                        // Do we have anything to do?
                        var utcNow = DateTime.UtcNow;

                        if (utcNow - m_valueInterval < m_lastValueTimestamp)
                        {
                            continue;
                        }

                        // Calculate values now
                        CalculateValuesAsync(utcNow);

                        // Trigger refresh of ui collections
                        await RefreshUICollectionsAsync();
                    }
                    catch(Exception)
                    {
                        // TODO: What to do in case of an exception?
                    }
                }
            });

            return m_runningTask;
        }


        /// <summary>
        /// Refreshes the gui using the configured SynchronizationContext.
        /// </summary>
        public async Task RefreshUICollectionsAsync()
        {
            // Trigger UI synchronization
            await SyncContext.PostAlsoIfNullAsync(
                () =>
                {
                    RefreshUICollections();
                },
                ActionIfSyncContextIsNull.InvokeSynchronous);
        }

        /// <summary>
        /// Refreshes the gui in a sync call.
        /// </summary>
        public void RefreshUICollections()
        {
            UIDurationKpisCurrents.Clear();

            foreach (var actCalculatorInfo in m_calculatorsBag)
            {
                PerformanceAnalyzeResultBase actResult = null;

                while (actCalculatorInfo.Results.TryTake(out actResult))
                {
                    HandleResultForUI(actResult);
                }
            }
        }

        /// <summary>
        /// Triggers calculation of
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        internal void CalculateValuesAsync(DateTime timestamp)
        {
            // Trigger kpi calculation
            var actKeyTimestamp = m_lastValueTimestamp + m_valueInterval;

            while (actKeyTimestamp < timestamp)
            {
                var actMaxTimestamp = actKeyTimestamp;
                var actMinTimestamp = actKeyTimestamp - m_calculationInterval;

                if (actMinTimestamp < m_startupTimestamp)
                {
                    actMinTimestamp = m_startupTimestamp;
                }

                // Calculate reporting values
                foreach (var actCalculatorInfo in m_calculatorsBag)
                {
                    var actResult = actCalculatorInfo.Calculator.Calculate(
                        actKeyTimestamp,
                        actMinTimestamp, actMaxTimestamp,
                        m_calculationInterval);
                    actCalculatorInfo.Results.Add(actResult);
                }

                // Handle next value timestamp
                m_lastValueTimestamp = actKeyTimestamp;
                actKeyTimestamp = m_lastValueTimestamp + m_valueInterval;
            }
        }

        /// <summary>
        /// Gets the calculator for the given activity.
        /// </summary>
        /// <typeparam name="T">The type of the calculator to get.</typeparam>
        /// <param name="activity">The name of the activity.</param>
        private T GetKpiCalculator<T>(string activity)
            where T : PerformanceCalculatorBase
        {
            var newCalculatorInfo = m_calculatorsDict.GetOrAdd(
                activity,
                (key) =>
                {
                    var newCalculator = Activator.CreateInstance(typeof(T), activity) as PerformanceCalculatorBase;
                    newCalculator.Parent = this;

                    var calcInfo = new CalculatorInfo(newCalculator);
                    m_calculatorsBag.Add(calcInfo);
                    return calcInfo;
                });


            if ((newCalculatorInfo == null) ||
                (newCalculatorInfo.Calculator == null))
            {
                throw new SeeingSharpException("Unable to create a calculator of type " + typeof(T) + " for activity " + activity + "!");
            }

            if(!(newCalculatorInfo.Calculator is T result))
            {
                throw new SeeingSharpException("Unable to create a calculator of type " + typeof(T) + " for activity " + activity + "!");
            }

            return result;
        }

        /// <summary>
        /// Check method used for setter methods.
        /// </summary>
        private void EnsureNotRunning()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("Unable to perform this operation when OnlineKpiContainer is running!");
            }
        }

        /// <summary>
        /// Gets or sets the delay time (milliseconds) of the kpi calculate loop.
        /// </summary>
        public int DelayTimeMS
        {
            get => m_delayTimeMS;
            set => m_delayTimeMS = value;
        }

        /// <summary>
        /// Gets or sets the current SynchronizationContext object.
        /// </summary>
        public SynchronizationContext SyncContext { get; set; }

        /// <summary>
        /// Is the main loop currently running?
        /// </summary>
        public bool IsRunning
        {
            get
            {
                // Check for currently running task
                if ((m_runningTask != null) &&
                    (m_runningTask.Status == TaskStatus.Running))
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// The interval for which values are produced.
        /// (Calculating is triggered after value interval).
        /// </summary>
        public TimeSpan ValueInterval
        {
            get => m_valueInterval;
            set
            {
                EnsureNotRunning();
                m_valueInterval = value;
            }
        }

        /// <summary>
        /// Values collected over this time interval are used for calculation.
        /// (Calculation uses values of this time interval).
        /// </summary>
        public TimeSpan CalculationInterval
        {
            get => m_calculationInterval;
            set
            {
                EnsureNotRunning();
                m_calculationInterval = value;
            }
        }

        /// <summary>
        /// The Maximum count of historical entries.
        /// </summary>
        public int MaxCountHistoricalEntries
        {
            get => m_maxCountHistoricalEntries;
            set
            {
                EnsureNotRunning();
                m_maxCountHistoricalEntries = value;
            }
        }

        /// <summary>
        /// Should this class fill the collection containing historical entries?
        /// </summary>
        public bool GenerateHistoricalCollection
        {
            get => m_generateHistoricalCollection;
            set
            {
                EnsureNotRunning();
                m_generateHistoricalCollection = value;
            }
        }

        /// <summary>
        /// Should this class fill the collection containing current entries?
        /// </summary>
        public bool GenerateCurrentValueCollection
        {
            get => m_generateCurrentValueCollection;
            set
            {
                EnsureNotRunning();
                m_generateCurrentValueCollection = value;
            }
        }

        /// <summary>
        /// Gets historical duration results (if activated).
        /// </summary>
        public ObservableCollection<DurationPerformanceResult> UIDurationKpisHistorical { get; }

        /// <summary>
        /// Gets current duration results (if activated).
        /// </summary>
        public ObservableCollection<DurationPerformanceResult> UIDurationKpisCurrents { get; }

        /// <summary>
        /// Gets historical flowrate results (if activated).
        /// </summary>
        public ObservableCollection<FlowRatePerformanceResult> UIFlowRateKpisHistorical { get; }

        /// <summary>
        /// Gets current flowrate results (if activated).
        /// </summary>
        public ObservableCollection<FlowRatePerformanceResult> UIFlowRateKpisCurrents { get; }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Helper class for holding relevant information of the value calculators.
        /// </summary>
        private class CalculatorInfo
        {
            public CalculatorInfo(PerformanceCalculatorBase calculator)
            {
                Calculator = calculator;
                Results = new BlockingCollection<PerformanceAnalyzeResultBase>();
            }

            public PerformanceCalculatorBase Calculator;
            public BlockingCollection<PerformanceAnalyzeResultBase> Results;
        }
    }
}