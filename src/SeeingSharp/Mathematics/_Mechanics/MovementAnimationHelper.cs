using System;
using System.Numerics;

namespace SeeingSharp.Mathematics
{
    /// <summary>
    /// This class is responsible for standard movement calculation depending by
    /// maximum speed, acceleration and deceleration.
    /// 
    /// see: http://www.frustfrei-lernen.de/mechanik/gleichfoermige-bewegung-physik.html
    /// see: http://www.frustfrei-lernen.de/mechanik/gleichmaessig-beschleunigte-bewegung-physik.html
    /// </summary>
    public class MovementAnimationHelper
    {
        // Outer parameters
        private MovementSpeed _speed;
        private Vector3 _movementDistance;
        private Vector3 _movementNormal;

        // All values needed location calculation
        private double _accelerationLength;
        private double _accelerationSeconds;
        private double _decelerationSeconds;
        private double _fullSpeedLength;
        private double _fullSpeedSeconds;
        private double _reachedMaxSpeed;

        /// <summary>
        /// Gets the full movement time.
        /// </summary>
        public TimeSpan MovementTime => TimeSpan.FromSeconds(_accelerationSeconds + _fullSpeedSeconds + _decelerationSeconds);

        /// <summary>
        /// Gets the time which is needed for acceleration phase.
        /// </summary>
        public TimeSpan AccelerationTime => TimeSpan.FromSeconds(_accelerationSeconds);

        /// <summary>
        /// Gets the time which is needed for full-speed phase.
        /// </summary>
        public TimeSpan FullSpeedTime => TimeSpan.FromSeconds(_fullSpeedSeconds);

        /// <summary>
        /// Gets the time which is needed for deceleration phase.
        /// </summary>
        public TimeSpan DecelerationTime => TimeSpan.FromSeconds(_decelerationSeconds);

        /// <summary>
        /// Gets the full movement vector.
        /// </summary>
        public Vector3 MovementVector => _movementDistance;

        /// <summary>
        /// Initializes a new instance of the <see cref="MovementAnimationHelper" /> class.
        /// </summary>
        /// <param name="speed">The speed data.</param>
        /// <param name="movementDistance">The full distance for the movement.</param>
        public MovementAnimationHelper(MovementSpeed speed, Vector3 movementDistance)
        {
            // Store main parameters
            _movementDistance = movementDistance;
            _speed = speed;
            _speed.ValidateWithException();

            // Calculate length and normal
            var length = 0f;
            _movementNormal = Vector3.Normalize(movementDistance);
            length = movementDistance.Length();
            if (length <= EngineMath.TOLERANCE_DOUBLE_POSITIVE)
            {
                // No movement.. leave all values on defaults
                return;
            }

            // Calculate acceleration values
            _accelerationLength = 0f;
            _accelerationSeconds = 0f;
            if (_speed.Acceleration > EngineMath.TOLERANCE_DOUBLE_POSITIVE)
            {
                _accelerationSeconds = _speed.MaximumSpeed / _speed.Acceleration;
                _accelerationLength = 0.5 * _speed.Acceleration * Math.Pow(_accelerationSeconds, 2.0);
            }

            // Calculate deceleration values
            double decelerationLength = 0f;
            _decelerationSeconds = 0f;
            if (_speed.Deceleration < EngineMath.TOLERANCE_DOUBLE_NEGATIVE)
            {
                _decelerationSeconds = _speed.MaximumSpeed / -_speed.Deceleration;
                decelerationLength = 0.5f * -_speed.Deceleration * Math.Pow(_decelerationSeconds, 2.0);
            }

            // Handle the case where we don't reach full speed
            // => Change length values depending on percentage of these phases on whole movement
            var fullAccDecLength = _accelerationLength + decelerationLength;
            _fullSpeedLength = length;
            if (length < fullAccDecLength)
            {
                var accWeight = _accelerationLength / fullAccDecLength;
                var decWeight = decelerationLength / fullAccDecLength;
                _accelerationLength = length * accWeight;
                decelerationLength = length * decWeight;
                _fullSpeedLength = 0.0;
                _accelerationSeconds = _speed.Acceleration > EngineMath.TOLERANCE_DOUBLE_POSITIVE ? Math.Pow(_accelerationLength / (0.5 * speed.Acceleration), 0.5) : 0.0;
                _decelerationSeconds = _speed.Deceleration < EngineMath.TOLERANCE_DOUBLE_NEGATIVE ? Math.Pow(decelerationLength / (0.5 * -speed.Deceleration), 0.5) : 0.0;
                _reachedMaxSpeed = _speed.Acceleration * _accelerationSeconds;
            }
            else
            {
                _reachedMaxSpeed = _speed.MaximumSpeed;
                _fullSpeedLength = _fullSpeedLength - fullAccDecLength;
            }
            _fullSpeedSeconds = _fullSpeedLength / _speed.MaximumSpeed;
        }

        /// <summary>
        /// Gets the move distance by the given timespan.
        /// </summary>
        /// <param name="elapsedTime"></param>
        public Vector3 GetPartialMoveDistance(TimeSpan elapsedTime)
        {
            var elapsedSeconds = elapsedTime.TotalSeconds;

            var movedLength = 0.0;
            if (elapsedSeconds < _accelerationSeconds)
            {
                // We are in acceleration phase
                movedLength = 0.5 * _speed.Acceleration * Math.Pow(elapsedSeconds, 2.0);
            }
            else if (elapsedSeconds < _accelerationSeconds + _fullSpeedSeconds)
            {
                // We are in full-speed phase
                var elapsedSecondsFullSpeed = elapsedSeconds - _accelerationSeconds;
                movedLength = _accelerationLength + _speed.MaximumSpeed * elapsedSecondsFullSpeed;
            }
            else if (elapsedSeconds < _accelerationSeconds + _fullSpeedSeconds + _decelerationSeconds)
            {
                // We are in deceleration phase
                var elapsedSecondsDeceleration = elapsedSeconds - (_accelerationSeconds + _fullSpeedSeconds);
                movedLength =
                    _accelerationLength + _fullSpeedLength +
                    0.5 * _speed.Deceleration * Math.Pow(elapsedSecondsDeceleration, 2.0) + _reachedMaxSpeed * elapsedSecondsDeceleration;
            }
            else
            {
                // Movement is finished
                return _movementDistance;
            }

            // Generate the full movement vector
            return _movementNormal * (float)movedLength;
        }
    }
}
