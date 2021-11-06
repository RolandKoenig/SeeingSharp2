using System;
using System.Numerics;

namespace SeeingSharp.Mathematics
{
    public struct MovementSpeed : IEquatable<MovementSpeed>
    {
        public static readonly MovementSpeed Empty = new MovementSpeed();

        /// <summary>
        /// The maximum speed in m/s.
        /// </summary>
        public float MaximumSpeed;

        /// <summary>
        /// The acceleration in m/s².
        /// </summary>
        public float Acceleration;

        /// <summary>
        /// The deceleration in m/s².
        /// </summary>
        public float Deceleration;

        /// <summary>
        /// Initializes a new instance of the <see cref="MovementSpeed"/> struct.
        /// </summary>
        /// <param name="movementVector">The total movement vector.</param>
        /// <param name="timeSpan">The total timespan the movement should take.</param>
        public MovementSpeed(Vector3 movementVector, TimeSpan timeSpan)
        {
            var totalLength = movementVector.Length();

            MaximumSpeed = (float)(totalLength / timeSpan.TotalSeconds);
            Acceleration = 0f;
            Deceleration = 0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MovementSpeed"/> struct.
        /// </summary>
        /// <param name="maxSpeed">The maximum speed in m/s.</param>
        public MovementSpeed(float maxSpeed)
        {
            MaximumSpeed = maxSpeed;
            Acceleration = 0f;
            Deceleration = 0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MovementSpeed"/> struct.
        /// </summary>
        /// <param name="maxSpeed">The maximum speed in m/s.</param>
        /// <param name="acceleration">The acceleration in m/s².</param>
        public MovementSpeed(float maxSpeed, float acceleration)
        {
            MaximumSpeed = maxSpeed;
            Acceleration = EngineMath.ForcePositive(acceleration);
            Deceleration = 0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MovementSpeed"/> struct.
        /// </summary>
        /// <param name="maxSpeed">The maximum speed in m/s.</param>
        /// <param name="acceleration">The acceleration in m/s².</param>
        /// <param name="deceleration">The deceleration in m/s².</param>
        public MovementSpeed(float maxSpeed, float acceleration, float deceleration)
        {
            MaximumSpeed = EngineMath.ForcePositive(maxSpeed);
            Acceleration = EngineMath.ForcePositive(acceleration);
            Deceleration = EngineMath.ForceNegative(deceleration);
        }

        public bool Equals(MovementSpeed other)
        {
            return Math.Abs(other.Acceleration - Acceleration) < MathUtil.ZeroTolerance &&
                   Math.Abs(other.Deceleration - Deceleration) < MathUtil.ZeroTolerance &&
                   Math.Abs(other.MaximumSpeed - MaximumSpeed) < MathUtil.ZeroTolerance;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (value == null)
            {
                return false;
            }

            if (!ReferenceEquals(value.GetType(), typeof(MovementSpeed)))
            {
                return false;
            }

            return this.Equals((MovementSpeed)value);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return Acceleration.GetHashCode() + Deceleration.GetHashCode() + MaximumSpeed.GetHashCode();
        }

        /// <summary>
        /// Validates the contents of this class.
        /// Throws an InvalidOperationException if something is not correct.
        /// </summary>
        public void ValidateWithException()
        {
            if (MaximumSpeed <= EngineMath.TOLERANCE_FLOAT_POSITIVE) { throw new InvalidOperationException("Invalid value for MaximumSpeed (must be positive)!"); }
            if (Acceleration < EngineMath.TOLERANCE_FLOAT_NEGATIVE) { throw new InvalidOperationException("Invalid value for acceleration (must be positive or zero)!"); }
            if (Deceleration > EngineMath.TOLERANCE_FLOAT_POSITIVE) { throw new InvalidOperationException("Invalid value for deceleration (must be negative or zero)!"); }
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator ==(MovementSpeed left, MovementSpeed right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator !=(MovementSpeed left, MovementSpeed right)
        {
            return !left.Equals(right);
        }
    }
}