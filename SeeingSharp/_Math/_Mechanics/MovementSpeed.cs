#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SeeingSharp
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
        public float Decelration;

        /// <summary>
        /// Initializes a new instance of the <see cref="MovementSpeed"/> struct.
        /// </summary>
        /// <param name="movementVector">The total movement vector.</param>
        /// <param name="timeSpan">The total timespan the movement should take.</param>
        public MovementSpeed(Vector3 movementVector, TimeSpan timeSpan)
        {
            float totalLength = movementVector.Length();

            this.MaximumSpeed = (float)((double)totalLength / timeSpan.TotalSeconds);
            this.Acceleration = 0f;
            this.Decelration = 0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MovementSpeed"/> struct.
        /// </summary>
        /// <param name="maxSpeed">The maximum speed in m/s.</param>
        public MovementSpeed(float maxSpeed)
        {
            this.MaximumSpeed = maxSpeed;
            this.Acceleration = 0f;
            this.Decelration = 0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MovementSpeed"/> struct.
        /// </summary>
        /// <param name="maxSpeed">The maximum speed in m/s.</param>
        /// <param name="acceleration">The acceleration in m/s².</param>
        public MovementSpeed(float maxSpeed, float acceleration)
        {
            this.MaximumSpeed = maxSpeed;
            this.Acceleration = EngineMath.ForcePositive(acceleration);
            this.Decelration = 0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MovementSpeed"/> struct.
        /// </summary>
        /// <param name="maxSpeed">The maximum speed in m/s.</param>
        /// <param name="acceleration">The acceleration in m/s².</param>
        /// <param name="deceleration">The deceleration in m/s².</param>
        public MovementSpeed(float maxSpeed, float acceleration, float deceleration)
        {
            this.MaximumSpeed = EngineMath.ForcePositive(maxSpeed);
            this.Acceleration = EngineMath.ForcePositive(acceleration);
            this.Decelration = EngineMath.ForceNegative(deceleration);
        }

        /// <summary>
        /// Gibt an, ob das aktuelle Objekt einem anderen Objekt des gleichen Typs entspricht.
        /// </summary>
        /// <param name="other">Ein Objekt, das mit diesem Objekt verglichen werden soll.</param>
        /// <returns>
        /// true, wenn das aktuelle Objekt gleich dem <paramref name="other" />-Parameter ist, andernfalls false.
        /// </returns>
        public bool Equals(MovementSpeed other)
        {
            return (Math.Abs(other.Acceleration - Acceleration) < MathUtil.ZeroTolerance &&
                    Math.Abs(other.Decelration - Decelration) < MathUtil.ZeroTolerance &&
                    Math.Abs(other.MaximumSpeed - MaximumSpeed) < MathUtil.ZeroTolerance);
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
                return false;

            if (!ReferenceEquals(value.GetType(), typeof(MovementSpeed)))
                return false;

            return Equals((MovementSpeed)value);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return Acceleration.GetHashCode() + Decelration.GetHashCode() + MaximumSpeed.GetHashCode();
        }

        /// <summary>
        /// Validates the contents of this class.
        /// Throws an InvalidOperationException if something is not correct.
        /// </summary>
        public void ValidateWithException()
        {
            if (this.MaximumSpeed <= EngineMath.TOLERANCE_FLOAT_POSITIVE) { throw new InvalidOperationException("Invalid value for MaximumSpeed (musst be positive)!"); }
            if (this.Acceleration < EngineMath.TOLERANCE_FLOAT_NEGATIVE) { throw new InvalidOperationException("Invalid value for acceleration (musst be possitive or zero)!"); }
            if (this.Decelration > EngineMath.TOLERANCE_FLOAT_POSITIVE) { throw new InvalidOperationException("Invalid value for deceleration (musst be negative or zero)!"); }
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