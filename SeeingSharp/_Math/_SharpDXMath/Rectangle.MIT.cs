﻿using System;
using System.Runtime.InteropServices;
using System.Numerics;

namespace SeeingSharp
{
    /// <summary>
    /// Define a Rectangle. This structure is slightly different from System.Drawing.Rectangle as It is 
    /// internally storing Left,Top,Right,Bottom instead of Left,Top,Width,Height.
    /// Although automatic casting from a to System.Drawing.Rectangle is provided by this class.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Rectangle : IEquatable<Rectangle>
    {
        private int _left;
        private int _top;
        private int _right;
        private int _bottom;

        /// <summary>
        /// An empty rectangle.
        /// </summary>
        public static readonly Rectangle Empty;

        static Rectangle()
        {
            Empty = new Rectangle();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="x">The left.</param>
        /// <param name="y">The top.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public Rectangle(int x, int y, int width, int height)
        {
            _left = x;
            _top = y;
            _right = x + width;
            _bottom = y + height;
        }

        /// <summary>
        /// Checks, if specified point is inside <see cref="SeeingSharp.Rectangle"/>.
        /// </summary>
        /// <param name="x">X point coordinate.</param>
        /// <param name="y">Y point coordinate.</param>
        /// <returns><c>true</c> if point is inside <see cref="SeeingSharp.Rectangle"/>, otherwise <c>false</c>.</returns>
        public bool Contains(float x, float y)
        {
            return (x >= _left && x <= _right && y >= _top && y <= _bottom);
        }

        /// <summary>
        /// Checks, if specified <see cref="System.Numerics.Vector2"/> is inside <see cref="SeeingSharp.Rectangle"/>. 
        /// </summary> 
        /// <param name="vector2D">Coordinate <see cref="System.Numerics.Vector2"/>.</param>
        /// <returns><c>true</c> if <see cref="System.Numerics.Vector2"/> is inside <see cref="SeeingSharp.Rectangle"/>, otherwise <c>false</c>.</returns>
        public bool Contains(Vector2 vector2D)
        {
            return (vector2D.X >= _left && vector2D.X <= _right && vector2D.Y >= _top && vector2D.Y <= _bottom);
        }

        /// <summary>
        /// Gets or sets the left.
        /// </summary>
        /// <value>The left.</value>
        public int Left
        {
            get { return _left; }
            set
            {
                _right = value + Width;
                _left = value;
            }
        }

        /// <summary>
        /// Gets or sets the top.
        /// </summary>
        /// <value>The top.</value>
        public int Top
        {
            get { return _top; }
            set
            {
                _bottom = value + Height;
                _top = value;
            }
        }

        /// <summary>
        /// Gets or sets the right.
        /// </summary>
        /// <value>The right.</value>
        public int Right
        {
            get { return _right; }
            set { _right = value; }
        }

        /// <summary>
        /// Gets or sets the bottom.
        /// </summary>
        /// <value>The bottom.</value>
        public int Bottom
        {
            get { return _bottom; }
            set { _bottom = value; }
        }

        /// <summary>
        /// Gets the left position.
        /// </summary>
        /// <value>The left position.</value>
        public int X
        {
            get { return _left; }
            set
            {
                _right = value + Width;
                _left = value;
            }
        }

        /// <summary>
        /// Gets the top position.
        /// </summary>
        /// <value>The top position.</value>
        public int Y
        {
            get { return _top; }
            set
            {
                _bottom = value + Height;
                _top = value;
            }
        }

        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>The width.</value>
        public int Width
        {
            get { return _right - _left; }
            set
            {
                _right = _left + value;
            }
        }

        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <value>The height.</value>
        public int Height
        {
            get { return _bottom - _top; }
            set
            {
                _bottom = _top + value;
            }
        }

        /// <summary>Gets or sets the upper-left value of the Rectangle.</summary>
        public Point Location
        {
            get
            {
                return new Point(X, Y);
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }
        /// <summary>Gets the Point that specifies the center of the rectangle.</summary>
        public Point Center
        {
            get
            {
                return new Point(X + (Width / 2), Y + (Height / 2));
            }
        }

        /// <summary>Gets a value that indicates whether the Rectangle is empty.</summary>
        public bool IsEmpty
        {
            get
            {
                return ((((Width == 0) && (Height == 0)) && (X == 0)) && (Y == 0));
            }
        }

        /// <summary>Changes the position of the Rectangle.</summary>
        /// <param name="amount">The values to adjust the position of the Rectangle by.</param>
        public void Offset(Point amount)
        {
            X += amount.X;
            Y += amount.Y;
        }

        /// <summary>Changes the position of the Rectangle.</summary>
        /// <param name="offsetX">Change in the x-position.</param>
        /// <param name="offsetY">Change in the y-position.</param>
        public void Offset(int offsetX, int offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        /// <summary>Pushes the edges of the Rectangle out by the horizontal and vertical values specified.</summary>
        /// <param name="horizontalAmount">Value to push the sides out by.</param>
        /// <param name="verticalAmount">Value to push the top and bottom out by.</param>
        public void Inflate(int horizontalAmount, int verticalAmount)
        {
            X -= horizontalAmount;
            Y -= verticalAmount;
            Width += horizontalAmount * 2;
            Height += verticalAmount * 2;
        }

        /// <summary>Determines whether this Rectangle contains a specified point represented by its x- and y-coordinates.</summary>
        /// <param name="x">The x-coordinate of the specified point.</param>
        /// <param name="y">The y-coordinate of the specified point.</param>
        public bool Contains(int x, int y)
        {
            return ((((X <= x) && (x < Right)) && (Y <= y)) && (y < Bottom));
        }

        /// <summary>Determines whether this Rectangle contains a specified Point.</summary>
        /// <param name="value">The Point to evaluate.</param>
        public bool Contains(Point value)
        {
            return ((((X <= value.X) && (value.X < Right)) && (Y <= value.Y)) && (value.Y < Bottom));
        }

        /// <summary>Determines whether this Rectangle contains a specified Point.</summary>
        /// <param name="value">The Point to evaluate.</param>
        /// <param name="result">[OutAttribute] true if the specified Point is contained within this Rectangle; false otherwise.</param>
        public void Contains(ref Point value, out bool result)
        {
            result = (((X <= value.X) && (value.X < Right)) && (Y <= value.Y)) && (value.Y < Bottom);
        }

        /// <summary>Determines whether this Rectangle entirely contains a specified Rectangle.</summary>
        /// <param name="value">The Rectangle to evaluate.</param>
        public bool Contains(Rectangle value)
        {
            return ((((X <= value.X) && (value.Right <= Right)) && (Y <= value.Y)) && (value.Bottom <= Bottom));
        }

        /// <summary>Determines whether this Rectangle entirely contains a specified Rectangle.</summary>
        /// <param name="value">The Rectangle to evaluate.</param>
        /// <param name="result">[OutAttribute] On exit, is true if this Rectangle entirely contains the specified Rectangle, or false if not.</param>
        public void Contains(ref Rectangle value, out bool result)
        {
            result = (((X <= value.X) && (value.Right <= Right)) && (Y <= value.Y)) && (value.Bottom <= Bottom);
        }

        /// <summary>Determines whether a specified Rectangle intersects with this Rectangle.</summary>
        /// <param name="value">The Rectangle to evaluate.</param>
        public bool Intersects(Rectangle value)
        {
            return ((((value.X < Right) && (X < value.Right)) && (value.Y < Bottom)) && (Y < value.Bottom));
        }

        /// <summary>
        /// Determines whether a specified Rectangle intersects with this Rectangle.
        /// </summary>
        /// <param name="value">The Rectangle to evaluate</param>
        /// <param name="result">[OutAttribute] true if the specified Rectangle intersects with this one; false otherwise.</param>
        public void Intersects(ref Rectangle value, out bool result)
        {
            result = (((value.X < Right) && (X < value.Right)) && (value.Y < Bottom)) && (Y < value.Bottom);
        }

        /// <summary>
        /// Creates a Rectangle defining the area where one rectangle overlaps with another rectangle.
        /// </summary>
        /// <param name="value1">The first Rectangle to compare.</param>
        /// <param name="value2">The second Rectangle to compare.</param>
        /// <returns>Rectangle.</returns>
        public static Rectangle Intersect(Rectangle value1, Rectangle value2)
        {
            int newLeft = (value1.X > value2.X) ? value1.X : value2.X;
            int newTop = (value1.Y > value2.Y) ? value1.Y : value2.Y;
            int newRight = (value1.Right < value2.Right) ? value1.Right : value2.Right;
            int newBottom = (value1.Bottom < value2.Bottom) ? value1.Bottom : value2.Bottom;
            if ((newRight > newLeft) && (newBottom > newTop))
            {
                return new Rectangle(newLeft, newTop, newRight - newLeft, newBottom - newTop);
            }
            return Empty;
        }

        /// <summary>Creates a Rectangle defining the area where one rectangle overlaps with another rectangle.</summary>
        /// <param name="value1">The first Rectangle to compare.</param>
        /// <param name="value2">The second Rectangle to compare.</param>
        /// <param name="result">[OutAttribute] The area where the two first parameters overlap.</param>
        public static void Intersect(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
        {
            int newLeft = (value1.X > value2.X) ? value1.X : value2.X;
            int newTop = (value1.Y > value2.Y) ? value1.Y : value2.Y;
            int newRight = (value1.Right < value2.Right) ? value1.Right : value2.Right;
            int newBottom = (value1.Bottom < value2.Bottom) ? value1.Bottom : value2.Bottom;
            if ((newRight > newLeft) && (newBottom > newTop))
            {
                result = new Rectangle(newLeft, newTop, newRight - newLeft, newBottom - newTop);
            }
            else
            {
                result = Empty;
            }
        }

        /// <summary>
        /// Creates a new Rectangle that exactly contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first Rectangle to contain.</param>
        /// <param name="value2">The second Rectangle to contain.</param>
        /// <returns>Rectangle.</returns>
        public static Rectangle Union(Rectangle value1, Rectangle value2)
        {
            int num6 = value1.X + value1.Width;
            int num5 = value2.X + value2.Width;
            int num4 = value1.Y + value1.Height;
            int num3 = value2.Y + value2.Height;
            int num2 = (value1.X < value2.X) ? value1.X : value2.X;
            int num = (value1.Y < value2.Y) ? value1.Y : value2.Y;
            int num8 = (num6 > num5) ? num6 : num5;
            int num7 = (num4 > num3) ? num4 : num3;
            return new Rectangle(num2, num, num8 - num2, num7 - num);
        }

        /// <summary>
        /// Creates a new Rectangle that exactly contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first Rectangle to contain.</param>
        /// <param name="value2">The second Rectangle to contain.</param>
        /// <param name="result">[OutAttribute] The Rectangle that must be the union of the first two rectangles.</param>
        public static void Union(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
        {
            int num6 = value1.X + value1.Width;
            int num5 = value2.X + value2.Width;
            int num4 = value1.Y + value1.Height;
            int num3 = value2.Y + value2.Height;
            int num2 = (value1.X < value2.X) ? value1.X : value2.X;
            int num = (value1.Y < value2.Y) ? value1.Y : value2.Y;
            int num8 = (num6 > num5) ? num6 : num5;
            int num7 = (num4 > num3) ? num4 : num3;
            result = new Rectangle(num2, num, num8 - num2, num7 - num);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Rectangle)) return false;
            return Equals((Rectangle)obj);
        }

        /// <summary>
        /// Determines whether the specified <see cref="SeeingSharp.Rectangle"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="SeeingSharp.Rectangle"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="SeeingSharp.Rectangle"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Rectangle other)
        {
            return other._left == _left && other._top == _top && other._right == _right && other._bottom == _bottom;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = _left;
                result = (result * 397) ^ _top;
                result = (result * 397) ^ _right;
                result = (result * 397) ^ _bottom;
                return result;
            }
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Rectangle left, Rectangle right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Rectangle left, Rectangle right)
        {
            return !(left == right);
        }

        internal void MakeXYAndWidthHeight()
        {
            _right = (_right - _left);
            _bottom = (_bottom - _top);
        }
    }
}
