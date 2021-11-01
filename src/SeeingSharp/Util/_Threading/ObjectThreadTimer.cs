using System;

namespace SeeingSharp.Util
{
    public class ObjectThreadTimer
    {
        private double _speedFactor;
        private DateTime _startTimeStamp;
        private TimeSpan _timeSinceStart;

        /// <summary>
        /// Gets current time (thread-time, not pc-time!).
        /// </summary>
        public DateTime Now => _startTimeStamp.Add(_timeSinceStart);

        /// <summary>
        /// Gets or sets current speed factor of the timer (default: 1.0).
        /// </summary>
        public double SpeedFactor
        {
            get => _speedFactor;
            set
            {
                if (value < 0.0) { throw new ArgumentException("SpeedFactor can not be less than zero!", "value"); }
                _speedFactor = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectThreadTimer"/> class.
        /// </summary>
        public ObjectThreadTimer()
        {
            _speedFactor = 1.0;
            _startTimeStamp = DateTime.MinValue;
            _timeSinceStart = TimeSpan.Zero;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectThreadTimer"/> class.
        /// </summary>
        /// <param name="startTimeStamp">The start time stamp.</param>
        public ObjectThreadTimer(DateTime startTimeStamp)
        {
            _startTimeStamp = startTimeStamp;
            _timeSinceStart = TimeSpan.Zero;
            _speedFactor = 1.0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectThreadTimer"/> class.
        /// </summary>
        /// <param name="startTimeStamp">The start time stamp.</param>
        /// <param name="speedFactor">Speed factor (standard: 1.0).</param>
        public ObjectThreadTimer(DateTime startTimeStamp, double speedFactor)
        {
            _startTimeStamp = startTimeStamp;
            _timeSinceStart = TimeSpan.Zero;
            _speedFactor = speedFactor;
        }

        /// <summary>
        /// Adds the given timespan to the timer.
        /// </summary>
        internal void Add(TimeSpan timeSpan)
        {
            if (EngineMath.EqualsWithTolerance(_speedFactor, 1.0))
            {
                _timeSinceStart = _timeSinceStart.Add(timeSpan);
            }
            else
            {
                _timeSinceStart = _timeSinceStart.Add(TimeSpan.FromTicks((long)(timeSpan.Ticks * _speedFactor)));
            }
        }
    }
}