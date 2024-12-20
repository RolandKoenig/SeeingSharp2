﻿using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SeeingSharp.Util
{
    // Original code from https://stackoverflow.com/questions/11232604/efficient-signaling-tasks-for-tpl-completions-on-frequently-reoccuring-events
    // Changed in some ways:
    //  - Constructor is private, the caller has to use the static Take method
    //  - The awaiter is reset automatically in GetResult method (--> After await call)
    //  - Locks are used for thread synchronization (GetResult and TrySetCompleted/TrySetException may be called from different threads)

    /// <summary>
    /// A reusable awaiter object meant for reduction object allocations on async/await.
    /// </summary>
    public sealed class ReusableAwaiter : INotifyCompletion
    {
        private static readonly ConcurrentObjectPool<ReusableAwaiter> s_freeAwaiters = new(() => new ReusableAwaiter(), 16);

        private WaitCallback _triggerContinuationCallback;
        private Exception? _exception;
        private bool _continueOnCaptureContext;
        private Action? _continuation;
        private object _stateSwitchLock;

        public bool IsCompleted { get; private set; }

        private ReusableAwaiter()
        {
            _continueOnCaptureContext = true;
            _triggerContinuationCallback = this.TriggerContinuation;
            _stateSwitchLock = new object();
        }

        public static ReusableAwaiter Take()
        {
            return s_freeAwaiters.Rent();
        }

        public ReusableAwaiter ConfigureAwait(bool continueOnCaptureContext)
        {
            _continueOnCaptureContext = continueOnCaptureContext;
            return this;
        }

        public ReusableAwaiter GetAwaiter()
        {
            return this;
        }

        public void GetResult()
        {
            try
            {
                if (_exception != null)
                {
                    throw _exception;
                }
            }
            finally
            {
                // Reset the awaiter finally
                _continuation = null;
                _exception = null;
                _continueOnCaptureContext = true;
                this.IsCompleted = false;

                s_freeAwaiters.Return(this);
            }
        }

        public void OnCompleted(Action continuation)
        {
            // Set continuation action and read IsCompleted flag
            var isCompleted = false;
            lock (_stateSwitchLock)
            {
                if (_continuation != null)
                {
                    throw new InvalidOperationException("This ReusableAwaiter instance has already been listened");
                }
                _continuation = continuation;

                isCompleted = this.IsCompleted;
            }

            // Call the continuation directly if the awaiter was completed before
            if (isCompleted)
            {
                if (_continueOnCaptureContext){ _continuation?.Invoke(); }
                else{ ThreadPool.QueueUserWorkItem(_triggerContinuationCallback); }
            }
        }

        /// <summary>
        /// Attempts to transition the completion state.
        /// </summary>
        public bool TrySetCompleted()
        {
            Action? continuation = null;
            lock (_stateSwitchLock)
            {
                if (this.IsCompleted) { return false; }

                this.IsCompleted = true;
                continuation = _continuation;
            }

            if (continuation != null)
            {
                if (_continueOnCaptureContext){ _continuation?.Invoke(); }
                else{ ThreadPool.QueueUserWorkItem(_triggerContinuationCallback); }
            }

            return true;
        }

        /// <summary>
        /// Attempts to transition the exception state.
        /// </summary>
        public bool TrySetException(Exception exception)
        {
            Action? continuation = null;
            lock (_stateSwitchLock)
            {
                if (this.IsCompleted) { return false; }

                this.IsCompleted = true;
                _exception = exception;
                continuation = _continuation;
            }

            if (continuation != null)
            {
                if (_continueOnCaptureContext){ _continuation?.Invoke(); }
                else{ ThreadPool.QueueUserWorkItem(_triggerContinuationCallback); }
            }

            return true;
        }

        private void TriggerContinuation(object? state)
        {
            var continueAction = _continuation;
            continueAction?.Invoke();
        }
    }
}
