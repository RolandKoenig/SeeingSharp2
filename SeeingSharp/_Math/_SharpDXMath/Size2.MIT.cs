using System;
using System.Runtime.InteropServices;
using System.Numerics;

namespace SeeingSharp
{
    /// <summary>
    /// Structure using the same layout than Size2 from System.Drawing namespace.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public partial struct Size2 : IEquatable<Size2>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Size2"/> struct.
        /// </summary>
        /// <param name="width">The x.</param>
        /// <param name="height">The y.</param>
        public Size2(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Width.
        /// </summary>
        public int Width;

        /// <summary>
        /// Height.
        /// </summary>
        public int Height;

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Size2 other)
        {
            return other.Width == Width && other.Height == Height;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Size2)) return false;
            return Equals((Size2)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Width * 397) ^ Height;
            }
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Size2 left, Size2 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Size2 left, Size2 right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", Width, Height);
        }
   
    }
}