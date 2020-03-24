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