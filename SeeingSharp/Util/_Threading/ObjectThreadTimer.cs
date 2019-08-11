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
    public class ObjectThreadTimer
    {
        private double m_speedFactor;
        private DateTime m_startTimeStamp;
        private TimeSpan m_timeSinceStart;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectThreadTimer"/> class.
        /// </summary>
        public ObjectThreadTimer()
        {
            m_speedFactor = 1.0;
            m_startTimeStamp = DateTime.MinValue;
            m_timeSinceStart = TimeSpan.Zero;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectThreadTimer"/> class.
        /// </summary>
        /// <param name="startTimeStamp">The start time stamp.</param>
        public ObjectThreadTimer(DateTime startTimeStamp)
        {
            m_startTimeStamp = startTimeStamp;
            m_timeSinceStart = TimeSpan.Zero;
            m_speedFactor = 1.0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectThreadTimer"/> class.
        /// </summary>
        /// <param name="startTimeStamp">The start time stamp.</param>
        /// <param name="speedFactor">Speed factor (standard: 1.0).</param>
        public ObjectThreadTimer(DateTime startTimeStamp, double speedFactor)
        {
            m_startTimeStamp = startTimeStamp;
            m_timeSinceStart = TimeSpan.Zero;
            m_speedFactor = speedFactor;
        }

        /// <summary>
        /// Adds the given timespan to the timer.
        /// </summary>
        internal void Add(TimeSpan timeSpan)
        {
            if (EngineMath.EqualsWithTolerance(m_speedFactor, 1.0))
            {
                m_timeSinceStart = m_timeSinceStart.Add(timeSpan);
            }
            else
            {
                m_timeSinceStart = m_timeSinceStart.Add(TimeSpan.FromTicks((long)(timeSpan.Ticks * m_speedFactor)));
            }
        }

        /// <summary>
        /// Gets current time (thread-time, not pc-time!).
        /// </summary>
        public DateTime Now => m_startTimeStamp.Add(m_timeSinceStart);

        /// <summary>
        /// Gets or sets current speed factor of the timer (default: 1.0).
        /// </summary>
        public double SpeedFactor
        {
            get => m_speedFactor;
            set
            {
                if (value < 0.0) { throw new ArgumentException("SpeedFactor can not be less than zero!", "value"); }
                m_speedFactor = value;
            }
        }
    }
}