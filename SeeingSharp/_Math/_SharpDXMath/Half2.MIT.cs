using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Numerics;

namespace SeeingSharp
{
    /// <summary>
    /// Defines a two component vector, using half precision floating point coordinates.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Half2 : IEquatable<Half2>
    {
        /// <summary>
        /// Gets or sets the X component of the vector.
        /// </summary>
        /// <value>The X component of the vector.</value>
        public Half X;
        /// <summary>
        /// Gets or sets the Y component of the vector.
        /// </summary>
        /// <value>The Y component of the vector.</value>
        public Half Y;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SeeingSharp.Half2" /> structure.
        /// </summary>
        /// <param name="x">The X component.</param>
        /// <param name="y">The Y component.</param>
        public Half2(Half x, Half y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SeeingSharp.Half2" /> structure.
        /// </summary>
        /// <param name="value">The value to set for both the X and Y components.</param>
        public Half2(Half value)
        {
            this.X = value;
            this.Y = value;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="System.Numerics.Vector2"/> to <see cref="SeeingSharp.Half2"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Half2(Vector2 value)
        {
            return new Half2(value.X, value.Y);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="SeeingSharp.Half2"/> to <see cref="System.Numerics.Vector2"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Vector2(Half2 value)
        {
            return new Vector2(value.X, value.Y);
        }
        
        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left" /> has the same value as <paramref name="right" />; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Half2 left, Half2 right)
        {
            return Equals(ref left, ref right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left" /> has a different value than <paramref name="right" />; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Half2 left, Half2 right)
        {
            return !Equals(ref left, ref right);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return (this.Y.GetHashCode() + this.X.GetHashCode());
        }

        /// <summary>
        /// Determines whether the specified object instances are considered equal. 
        /// </summary>
        /// <param name="value1" />
        /// <param name="value2" />
        /// <returns>
        /// <c>true</c> if <paramref name="value1" /> is the same instance as <paramref name="value2" /> or 
        /// if both are <c>null</c> references or if <c>value1.Equals(value2)</c> returns <c>true</c>; otherwise, <c>false</c>.</returns>
        public static bool Equals(ref Half2 value1, ref Half2 value2)
        {
            return ((value1.X == value2.X) && (value1.Y == value2.Y));
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to the specified object. 
        /// </summary>
        /// <param name="other">Object to make the comparison with.</param>
        /// <returns>
        /// <c>true</c> if the current instance is equal to the specified object; <c>false</c> otherwise.</returns>
        public bool Equals(Half2 other)
        {
            return ((this.X == other.X) && (this.Y == other.Y));
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object. 
        /// </summary>
        /// <param name="obj">Object to make the comparison with.</param>
        /// <returns>
        /// <c>true</c> if the current instance is equal to the specified object; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!ReferenceEquals(obj.GetType(), typeof(Half2)))
            {
                return false;
            }
            return this.Equals((Half2)obj);
        }
    }
}
