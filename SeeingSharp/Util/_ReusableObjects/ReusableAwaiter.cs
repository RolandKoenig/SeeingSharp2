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
using System.Runtime.CompilerServices;
using System.Threading;

namespace SeeingSharp.Util
{
    // Original code from https://stackoverflow.com/questions/11232604/efficient-signaling-tasks-for-tpl-completions-on-frequently-reoccuring-events
    // Changed in two ways:
    //  - Constructor is private, the caller has to use the static Take method

    public sealed class ReusableAwaiter : INotifyCompletion
    {
        private static ConcurrentBag<ReusableAwaiter> s_freeAwaiters;

        private WaitCallback m_triggerContinuationCallback;
        private Exception m_exception;
        private bool m_continueOnCaptureContext;
        private Action m_continuation;
        private object m_stateSwitchLock;

        static ReusableAwaiter()
        {
            s_freeAwaiters = new ConcurrentBag<ReusableAwaiter>();
        }

        private ReusableAwaiter()
        {
            m_continueOnCaptureContext = true;
            m_triggerContinuationCallback = this.TriggerContinuation;
            m_stateSwitchLock = new object();
        }

        public static ReusableAwaiter Take()
        {
            if (s_freeAwaiters.TryTake(out var result))
            {
                return result;
            }
            return new ReusableAwaiter(); 
        }

        public ReusableAwaiter ConfigureAwait(bool continueOnCaptureContext)
        {
            m_continueOnCaptureContext = continueOnCaptureContext;
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
                if (m_exception != null)
                {
                    throw m_exception;
                }
            }
            finally
            {
                // Reset the awaiter finally
                m_continuation = null;
                m_exception = null;
                m_continueOnCaptureContext = true;
                this.IsCompleted = false;

                s_freeAwaiters.Add(this);
            }
        }

        public void OnCompleted(Action continuation)
        {
            // Set continuation action and read IsCompleted flag
            var isCompleted = false;
            lock (m_stateSwitchLock)
            {
                if (m_continuation != null)
                {
                    throw new InvalidOperationException("This ReusableAwaiter instance has already been listened");
                }
                m_continuation = continuation;

                isCompleted = this.IsCompleted;
            }

            // Call the continuation directly if the awaiter was completed before
            if (isCompleted)
            {
                if (m_continueOnCaptureContext){ m_continuation?.Invoke(); }
                else{ ThreadPool.QueueUserWorkItem(m_triggerContinuationCallback); }
            }
        }

        /// <summary>
        /// Attempts to transition the completion state.
        /// </summary>
        public bool TrySetCompleted()
        {
            Action continuation = null;
            lock (m_stateSwitchLock)
            {
                if (this.IsCompleted) { return false; }

                this.IsCompleted = true;
                continuation = m_continuation;
            }

            if (continuation != null)
            {
                if (m_continueOnCaptureContext){ m_continuation?.Invoke(); }
                else{ ThreadPool.QueueUserWorkItem(m_triggerContinuationCallback); }
            }

            return true;
        }

        /// <summary>
        /// Attempts to transition the exception state.
        /// </summary>
        public bool TrySetException(Exception exception)
        {
            Action continuation = null;
            lock (m_stateSwitchLock)
            {
                if (this.IsCompleted) { return false; }

                this.IsCompleted = true;
                m_exception = exception;
                continuation = m_continuation;
            }

            if (continuation != null)
            {
                if (m_continueOnCaptureContext){ m_continuation?.Invoke(); }
                else{ ThreadPool.QueueUserWorkItem(m_triggerContinuationCallback); }
            }

            return true;
        }

        private void TriggerContinuation(object state)
        {
            var continueAction = m_continuation;
            continueAction?.Invoke();
        }

        public bool IsCompleted { get; private set; }
    }
}
