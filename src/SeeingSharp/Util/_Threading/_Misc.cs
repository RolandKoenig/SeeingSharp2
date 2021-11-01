using System;

namespace SeeingSharp.Util
{
    /// <summary>
    /// Enumeration containing all possible states of a ObjectThread object.
    /// </summary>
    public enum ObjectThreadState
    {
        /// <summary>
        /// There is no thread created at the moment.
        /// </summary>
        None,

        /// <summary>
        /// The thread is starting.
        /// </summary>
        Starting,

        /// <summary>
        /// The thread is running.
        /// </summary>
        Running,

        /// <summary>
        /// The thread is stopping.
        /// </summary>
        Stopping
    }

    public class ObjectThreadExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the occurred exception.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Gets current state of the thread.
        /// </summary>
        public ObjectThreadState State { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectThreadExceptionEventArgs"/> class.
        /// </summary>
        /// <param name="threadState">The current state of the <see cref="ObjectThread"/>.</param>
        /// <param name="innerException">The inner exception.</param>
        public ObjectThreadExceptionEventArgs(ObjectThreadState threadState, Exception innerException)
        {
            this.Exception = innerException;
            this.State = threadState;
        }
    }
}