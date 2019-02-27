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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    #endregion

    public class ObjectThread
    {
        private const int STANDARD_HEARTBEAT = 500;

        #region Members for thread configuration

        private bool m_createMessenger;
        #endregion

        #region Members for thread runtime
        private volatile ObjectThreadState m_currentState;
        private Thread m_mainThread;
        private CultureInfo m_culture;
        private CultureInfo m_uiCulture;
        #endregion

        #region Threading resources
        private ObjectThreadSynchronizationContext m_syncContext;
        private ConcurrentQueue<Action> m_taskQueue;
        private SemaphoreSlim m_mainLoopSynchronizeObject;
        private SemaphoreSlim m_threadStopSynchronizeObject;
        #endregion

        /// <summary>
        /// Called when the thread ist starting.
        /// </summary>
        public event EventHandler Starting;

        /// <summary>
        /// Called on each heartbeat.
        /// </summary>
        public event EventHandler Tick;

        /// <summary>
        /// Called when the thread is stopping.
        /// </summary>
        public event EventHandler Stopping;

        /// <summary>
        /// Called when an unhandled exception occurred.
        /// </summary>
        public event EventHandler<ObjectThreadExceptionEventArgs> ThreadException;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectThread"/> class.
        /// </summary>
        public ObjectThread()
            : this(string.Empty, STANDARD_HEARTBEAT)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectThread"/> class.
        /// </summary>
        /// <param name="name">The name of the generated thread.</param>
        /// <param name="heartBeat">The initial heartbeat of the ObjectThread.</param>
        /// <param name="createMessenger">Do automatically create a messenger for this thread?</param>
        public ObjectThread(string name, int heartBeat, bool createMessenger = false)
        {
            m_taskQueue = new ConcurrentQueue<Action>();
            m_mainLoopSynchronizeObject = new SemaphoreSlim(1);

            Name = name;
            HeartBeat = heartBeat;

            m_culture = Thread.CurrentThread.CurrentCulture;
            m_uiCulture = Thread.CurrentThread.CurrentUICulture;

            Timer = new ObjectThreadTimer();
            m_createMessenger = createMessenger;
        }

        /// <summary>
        /// Starts the thread.
        /// </summary>
        public void Start()
        {
            if (m_currentState != ObjectThreadState.None) { throw new InvalidOperationException("Unable to start thread: Illegal state: " + m_currentState.ToString() + "!"); }

            //Ensure that one single pass of the main loop is made at once
            m_mainLoopSynchronizeObject.Release();

            // Create stop semaphore
            if (m_threadStopSynchronizeObject != null)
            {
                m_threadStopSynchronizeObject.Dispose();
                m_threadStopSynchronizeObject = null;
            }

            m_threadStopSynchronizeObject = new SemaphoreSlim(0);

            //Go into starting state
            m_currentState = ObjectThreadState.Starting;

            m_mainThread = new Thread(ObjectThreadMainMethod)
            {
                IsBackground = true,
                Name = Name
            };

            m_mainThread.Start();
        }

        /// <summary>
        /// Waits until this ObjectThread has stopped.
        /// </summary>
        public Task WaitUntilSoppedAsync()
        {
            switch(m_currentState)
            {
                case ObjectThreadState.None:
                case ObjectThreadState.Stopping:
                    return Task.Delay(100);

                case ObjectThreadState.Running:
                case ObjectThreadState.Starting:
                    TaskCompletionSource<object> taskSource = new TaskCompletionSource<object>();
                    this.Stopping += (sender, eArgs) =>
                    {
                        taskSource.TrySetResult(null);
                    };
                    return taskSource.Task;

                default:
                    throw new SeeingSharpException($"Unhandled {nameof(ObjectThreadState)} {m_currentState}!");
            }
        }

        /// <summary>
        /// Starts this thread. The returned task is completed when starting is finished.
        /// </summary>
        public Task StartAsync()
        {
            this.Start();

            return this.InvokeAsync(() => { });
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            if (m_currentState != ObjectThreadState.Running) { throw new InvalidOperationException("Unable to stop thread: Illegal state: " + m_currentState.ToString() + "!"); }
            m_currentState = ObjectThreadState.Stopping;

            Action dummyAction = null;
            while (m_taskQueue.TryDequeue(out dummyAction)) ;

            //Trigger next update
            this.Trigger();
        }

        /// <summary>
        /// Stops the asynchronous.
        /// </summary>
        public async Task StopAsync(int timeout)
        {
            this.Stop();

            if (m_threadStopSynchronizeObject != null)
            {
                await m_threadStopSynchronizeObject.WaitAsync(timeout);

                m_threadStopSynchronizeObject.Dispose();
                m_threadStopSynchronizeObject = null;
            }
        }

        /// <summary>
        /// Triggers a new heartbeat.
        /// </summary>
        public virtual void Trigger()
        {
            var synchronizationObject = m_mainLoopSynchronizeObject;

            if (synchronizationObject != null)
            {
                synchronizationObject.Release();
            }
        }

        /// <summary>
        /// Invokes the given delegate within the thread of this object.
        /// </summary>
        /// <param name="actionToInvoke">The delegate to invoke.</param>
        public Task InvokeAsync(Action actionToInvoke)
        {
            if (actionToInvoke == null)
            {
                throw new ArgumentNullException("actionToInvoke");
            }

            //Enqueues the given action
            TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();

            m_taskQueue.Enqueue(() =>
            {
                try
                {
                    actionToInvoke();
                    taskCompletionSource.SetResult(null);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            });

            Task result = taskCompletionSource.Task;

            //Triggers the main loop
            this.Trigger();

            //Returns the result
            return result;
        }

        /// <summary>
        /// Thread is starting.
        /// </summary>
        protected virtual void OnStarting(EventArgs eArgs)
        {
            Starting?.Invoke(this, eArgs);
        }

        /// <summary>
        /// Called on each tick.
        /// </summary>
        protected virtual void OnTick(EventArgs eArgs)
        {
            Tick?.Invoke(this, eArgs);
        }

        /// <summary>
        /// Called on each occurred exception.
        /// </summary>
        protected virtual void OnThreadException(ObjectThreadExceptionEventArgs eArgs)
        {
            ThreadException?.Invoke(this, eArgs);
        }

        /// <summary>
        /// Thread is stopping.
        /// </summary>
        protected virtual void OnStopping(EventArgs eArgs)
        {
            Stopping?.Invoke(this, eArgs);
        }

        /// <summary>
        /// The thread's main method.
        /// </summary>
        private void ObjectThreadMainMethod()
        {
            try
            {
                m_mainThread.CurrentCulture = m_culture;
                m_mainThread.CurrentUICulture = m_uiCulture;

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                //Set synchronization context for this thread
                m_syncContext = new ObjectThreadSynchronizationContext(this);
                SynchronizationContext.SetSynchronizationContext(m_syncContext);

                //Notify start process
                try
                {
                    OnStarting(EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    OnThreadException(new ObjectThreadExceptionEventArgs(m_currentState, ex));
                    m_currentState = ObjectThreadState.None;
                    return;
                }

                //Run main-thread
                if (m_currentState != ObjectThreadState.None)
                {
                    m_currentState = ObjectThreadState.Running;
                    while (m_currentState == ObjectThreadState.Running)
                    {
                        try
                        {
                            //Wait for next action to perform
                            m_mainLoopSynchronizeObject.Wait(HeartBeat);

                            //Measure current time
                            stopWatch.Stop();
                            Timer.Add(stopWatch.Elapsed);
                            stopWatch.Reset();
                            stopWatch.Start();

                            //Get current taskqueue
                            List<Action> localTaskQueue = new List<Action>();
                            Action dummyAction = null;
                            while (m_taskQueue.TryDequeue(out dummyAction))
                            {
                                localTaskQueue.Add(dummyAction);
                            }

                            //Execute all tasks
                            foreach (var actTask in localTaskQueue)
                            {
                                try
                                {
                                    actTask();
                                }
                                catch (Exception ex)
                                {
                                    OnThreadException(new ObjectThreadExceptionEventArgs(m_currentState, ex));
                                }
                            }

                            //Perfoms a tick
                            OnTick(EventArgs.Empty);
                        }
                        catch (Exception ex)
                        {
                            OnThreadException(new ObjectThreadExceptionEventArgs(m_currentState, ex));
                        }
                    }

                    //Notify stop process
                    try { OnStopping(EventArgs.Empty); }
                    catch (Exception ex)
                    {
                        OnThreadException(new ObjectThreadExceptionEventArgs(m_currentState, ex));
                    }
                }

                //Reset state to none
                m_currentState = ObjectThreadState.None;

                stopWatch.Stop();
                stopWatch = null;
            }
            catch (Exception ex)
            {
                OnThreadException(new ObjectThreadExceptionEventArgs(m_currentState, ex));
                m_currentState = ObjectThreadState.None;
            }

            // Notify thread stop event
            try { m_threadStopSynchronizeObject.Release(); }
            catch (Exception) { }
        }

        /// <summary>
        /// Gets current thread time.
        /// </summary>
        public DateTime ThreadTime
        {
            get { return Timer.Now; }
        }

        /// <summary>
        /// Gets current timer of the thread.
        /// </summary>
        public ObjectThreadTimer Timer { get; }

        /// <summary>
        /// Gets the current SynchronizationContext object.
        /// </summary>
        public SynchronizationContext SyncContext
        {
            get { return m_syncContext; }
        }

        /// <summary>
        /// Gets the name of this thread.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the thread's heartbeat.
        /// </summary>
        protected int HeartBeat { get; set; }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Synchronization object for threads within ObjectThread class.
        /// </summary>
        private class ObjectThreadSynchronizationContext : SynchronizationContext
        {
            private ObjectThread m_owner;

            /// <summary>
            /// Initializes a new instance of the <see cref="ObjectThreadSynchronizationContext"/> class.
            /// </summary>
            /// <param name="owner">The owner of this context.</param>
            public ObjectThreadSynchronizationContext(ObjectThread owner)
            {
                m_owner = owner;
            }

            /// <summary>
            /// When overridden in a derived class, dispatches an asynchronous message to a synchronization context.
            /// </summary>
            /// <param name="d">The <see cref="T:System.Threading.SendOrPostCallback"/> delegate to call.</param>
            /// <param name="state">The object passed to the delegate.</param>
            public override void Post(SendOrPostCallback d, object state)
            {
                m_owner.InvokeAsync(() => d(state));
            }

            /// <summary>
            /// When overridden in a derived class, dispatches a synchronous message to a synchronization context.
            /// </summary>
            /// <param name="d">The <see cref="T:System.Threading.SendOrPostCallback"/> delegate to call.</param>
            /// <param name="state">The object passed to the delegate.</param>
            public override void Send(SendOrPostCallback d, object state)
            {
                throw new InvalidOperationException("Synchronous messages not supported on ObjectThreads!");
            }
        }
    }
}