using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Numerics;

namespace SeeingSharp
{
    /// <summary>
    /// Represents a three dimensional line based on a point in space and a direction.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public partial struct Ray : IEquatable<Ray>, IFormattable
    {
        /// <summary>
        /// The position in three dimensional space where the ray starts.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The normalized direction in which the ray points.
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeeingSharp.Ray"/> struct.
        /// </summary>
        /// <param name="position">The position in three dimensional space of the origin of the ray.</param>
        /// <param name="direction">The normalized direction of the ray.</param>
        public Ray(Vector3 position, Vector3 direction)
        {
            this.Position = position;
            this.Direction = direction;
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a point.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Vector3 point)
        {
            return Collision.RayIntersectsPoint(ref this, ref point);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="SeeingSharp.Ray"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Ray ray)
        {
            Vector3 point;
            return Collision.RayIntersectsRay(ref this, ref ray, out point);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="SeeingSharp.Ray"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="System.Numerics.Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Ray ray, out Vector3 point)
        {
            return Collision.RayIntersectsRay(ref this, ref ray, out point);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="SeeingSharp.Plane"/>.
        /// </summary>
        /// <param name="plane">The plane to test</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Plane plane)
        {
            float distance;
            return Collision.RayIntersectsPlane(ref this, ref plane, out distance);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="SeeingSharp.Plane"/>.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Plane plane, out float distance)
        {
            return Collision.RayIntersectsPlane(ref this, ref plane, out distance);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="SeeingSharp.Plane"/>.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="System.Numerics.Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Plane plane, out Vector3 point)
        {
            return Collision.RayIntersectsPlane(ref this, ref plane, out point);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a triangle.
        /// </summary>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
        {
            float distance;
            return Collision.RayIntersectsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3, out distance);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a triangle.
        /// </summary>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out float distance)
        {
            return Collision.RayIntersectsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3, out distance);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a triangle.
        /// </summary>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="System.Numerics.Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out Vector3 point)
        {
            return Collision.RayIntersectsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3, out point);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="SeeingSharp.BoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref BoundingBox box)
        {
            float distance;
            return Collision.RayIntersectsBox(ref this, ref box, out distance);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="SeeingSharp.BoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(BoundingBox box)
        {
            return Intersects(ref box);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="SeeingSharp.BoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref BoundingBox box, out float distance)
        {
            return Collision.RayIntersectsBox(ref this, ref box, out distance);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="SeeingSharp.BoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="System.Numerics.Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref BoundingBox box, out Vector3 point)
        {
            return Collision.RayIntersectsBox(ref this, ref box, out point);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="SeeingSharp.BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref BoundingSphere sphere)
        {
            float distance;
            return Collision.RayIntersectsSphere(ref this, ref sphere, out distance);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="SeeingSharp.BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(BoundingSphere sphere)
        {
            return Intersects(ref sphere);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="SeeingSharp.BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref BoundingSphere sphere, out float distance)
        {
            return Collision.RayIntersectsSphere(ref this, ref sphere, out distance);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="SeeingSharp.BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="System.Numerics.Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref BoundingSphere sphere, out Vector3 point)
        {
            return Collision.RayIntersectsSphere(ref this, ref sphere, out point);
        }

        /// <summary>
        /// Calculates a world space <see cref="SeeingSharp.Ray"/> from 2d screen coordinates.
        /// </summary>
        /// <param name="x">X coordinate on 2d screen.</param>
        /// <param name="y">Y coordinate on 2d screen.</param>
        /// <param name="viewport"><see cref="SeeingSharp.ViewportF"/>.</param>
        /// <param name="worldViewProjection">Transformation <see cref="System.Numerics.Matrix4x4"/>.</param>
        /// <returns>Resulting <see cref="SeeingSharp.Ray"/>.</returns>
        public static Ray GetPickRay(int x, int y, ViewportF viewport, Matrix4x4 worldViewProjection)
        {
            var nearPoint = new Vector3(x, y, 0);
            var farPoint = new Vector3(x, y, 1);

            nearPoint = Vector3Ex.Unproject(nearPoint, viewport.X, viewport.Y, viewport.Width, viewport.Height, viewport.MinDepth,
                                        viewport.MaxDepth, worldViewProjection);
            farPoint = Vector3Ex.Unproject(farPoint, viewport.X, viewport.Y, viewport.Width, viewport.Height, viewport.MinDepth,
                                        viewport.MaxDepth, worldViewProjection);

            Vector3 direction = Vector3.Normalize(farPoint - nearPoint);

            return new Ray(nearPoint, direction);
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Ray left, Ray right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Ray left, Ray right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "Position:{0} Direction:{1}", Position.ToString(), Direction.ToString());
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            return string.Format(CultureInfo.CurrentCulture, "Position:{0} Direction:{1}", Position.ToString(format, CultureInfo.CurrentCulture),
                Direction.ToString(format, CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "Position:{0} Direction:{1}", Position.ToString(), Direction.ToString());
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "Position:{0} Direction:{1}", Position.ToString(format, formatProvider),
                Direction.ToString(format, formatProvider));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Position.GetHashCode() + Direction.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Numerics.Vector4"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="System.Numerics.Vector4"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Numerics.Vector4"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Ray value)
        {
            return Position == value.Position && Direction == value.Direction;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (!ReferenceEquals(value.GetType(), typeof(Ray)))
                return false;

            return Equals((Ray)value);
        }
    }
}
