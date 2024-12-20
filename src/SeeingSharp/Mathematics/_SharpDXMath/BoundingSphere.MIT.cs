﻿using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SeeingSharp.Mathematics
{
    /// <summary>
    /// Represents a bounding sphere in three dimensional space.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public partial struct BoundingSphere : IEquatable<BoundingSphere>, IFormattable
    {
        /// <summary>
        /// The center of the sphere in three dimensional space.
        /// </summary>
        public Vector3 Center;

        /// <summary>
        /// The radious of the sphere.
        /// </summary>
        public float Radius;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBox"/> struct.
        /// </summary>
        /// <param name="center">The center of the sphere in three dimensional space.</param>
        /// <param name="radius">The radius of the sphere.</param>
        public BoundingSphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="Ray"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Ray ray)
        {
            float distance;
            return Collision.RayIntersectsSphere(ref ray, ref this, out distance);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="Ray"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Ray ray, out float distance)
        {
            return Collision.RayIntersectsSphere(ref ray, ref this, out distance);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="Ray"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="System.Numerics.Vector3.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Ray ray, out Vector3 point)
        {
            return Collision.RayIntersectsSphere(ref ray, ref this, out point);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public PlaneIntersectionType Intersects(ref Plane plane)
        {
            return Collision.PlaneIntersectsSphere(ref plane, ref this);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a triangle.
        /// </summary>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triagnle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
        {
            return Collision.SphereIntersectsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref BoundingBox box)
        {
            return Collision.BoxIntersectsSphere(ref box, ref this);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(BoundingBox box)
        {
            return this.Intersects(ref box);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref BoundingSphere sphere)
        {
            return Collision.SphereIntersectsSphere(ref this, ref sphere);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(BoundingSphere sphere)
        {
            return this.Intersects(ref sphere);
        }

        /// <summary>
        /// Determines whether the current objects contains a point.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(ref Vector3 point)
        {
            return Collision.SphereContainsPoint(ref this, ref point);
        }

        /// <summary>
        /// Determines whether the current objects contains a triangle.
        /// </summary>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triagnle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
        {
            return Collision.SphereContainsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3);
        }

        /// <summary>
        /// Determines whether the current objects contains a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(ref BoundingBox box)
        {
            return Collision.SphereContainsBox(ref this, ref box);
        }

        /// <summary>
        /// Determines whether the current objects contains a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(ref BoundingSphere sphere)
        {
            return Collision.SphereContainsSphere(ref this, ref sphere);
        }

        /// <summary>
        /// Constructs a <see cref="BoundingSphere" /> that fully contains the given points.
        /// </summary>
        /// <param name="points">The points that will be contained by the sphere.</param>
        /// <param name="start">The start index from points array to start compute the bounding sphere.</param>
        /// <param name="count">The count of points to process to compute the bounding sphere.</param>
        /// <param name="result">When the method completes, contains the newly constructed bounding sphere.</param>
        /// <exception cref="System.ArgumentNullException">points</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// start
        /// or
        /// count
        /// </exception>
        public static void FromPoints(Vector3[] points, int start, int count, out BoundingSphere result)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            // Check that start is in the correct range
            if (start < 0 || start >= points.Length)
            {
                throw new ArgumentOutOfRangeException("start", start, string.Format("Must be in the range [0, {0}]", points.Length - 1));
            }

            // Check that count is in the correct range
            if (count < 0 || start + count > points.Length)
            {
                throw new ArgumentOutOfRangeException("count", count, string.Format("Must be in the range <= {0}", points.Length));
            }

            var upperEnd = start + count;

            //Find the center of all points.
            var center = Vector3.Zero;
            for (var i = start; i < upperEnd; ++i)
            {
                center = Vector3.Add(points[i], center);
            }

            //This is the center of our sphere.
            center /= (float)count;

            //Find the radius of the sphere
            var radius = 0f;
            for (var i = start; i < upperEnd; ++i)
            {
                //We are doing a relative distance comparasin to find the maximum distance
                //from the center of our sphere.
                var distance = Vector3.DistanceSquared(center, points[i]);

                if (distance > radius)
                {
                    radius = distance;
                }
            }

            //Find the real distance from the DistanceSquared.
            radius = (float)Math.Sqrt(radius);

            //Construct the sphere.
            result.Center = center;
            result.Radius = radius;
        }

        /// <summary>
        /// Constructs a <see cref="BoundingSphere"/> that fully contains the given points.
        /// </summary>
        /// <param name="points">The points that will be contained by the sphere.</param>
        /// <param name="result">When the method completes, contains the newly constructed bounding sphere.</param>
        public static void FromPoints(Vector3[] points, out BoundingSphere result)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            FromPoints(points, 0, points.Length, out result);
        }

        /// <summary>
        /// Constructs a <see cref="BoundingSphere"/> that fully contains the given points.
        /// </summary>
        /// <param name="points">The points that will be contained by the sphere.</param>
        /// <returns>The newly constructed bounding sphere.</returns>
        public static BoundingSphere FromPoints(Vector3[] points)
        {
            BoundingSphere result;
            FromPoints(points, out result);
            return result;
        }

        /// <summary>
        /// Constructs a <see cref="BoundingSphere"/> from a given box.
        /// </summary>
        /// <param name="box">The box that will designate the extents of the sphere.</param>
        /// <param name="result">When the method completes, the newly constructed bounding sphere.</param>
        public static void FromBox(ref BoundingBox box, out BoundingSphere result)
        {
            result.Center = Vector3.Lerp(box.Minimum, box.Maximum, 0.5f);

            var x = box.Minimum.X - box.Maximum.X;
            var y = box.Minimum.Y - box.Maximum.Y;
            var z = box.Minimum.Z - box.Maximum.Z;

            var distance = (float)Math.Sqrt(x * x + y * y + z * z);
            result.Radius = distance * 0.5f;
        }

        /// <summary>
        /// Constructs a <see cref="BoundingSphere"/> from a given box.
        /// </summary>
        /// <param name="box">The box that will designate the extents of the sphere.</param>
        /// <returns>The newly constructed bounding sphere.</returns>
        public static BoundingSphere FromBox(BoundingBox box)
        {
            BoundingSphere result;
            FromBox(ref box, out result);
            return result;
        }

        /// <summary>
        /// Constructs a <see cref="BoundingSphere"/> that is the as large as the total combined area of the two specified spheres.
        /// </summary>
        /// <param name="value1">The first sphere to merge.</param>
        /// <param name="value2">The second sphere to merge.</param>
        /// <param name="result">When the method completes, contains the newly constructed bounding sphere.</param>
        public static void Merge(ref BoundingSphere value1, ref BoundingSphere value2, out BoundingSphere result)
        {
            var difference = value2.Center - value1.Center;

            var length = difference.Length();
            var radius = value1.Radius;
            var radius2 = value2.Radius;

            if (radius + radius2 >= length)
            {
                if (radius - radius2 >= length)
                {
                    result = value1;
                    return;
                }

                if (radius2 - radius >= length)
                {
                    result = value2;
                    return;
                }
            }

            var vector = difference * (1.0f / length);
            var min = Math.Min(-radius, length - radius2);
            var max = (Math.Max(radius, length + radius2) - min) * 0.5f;

            result.Center = value1.Center + vector * (max + min);
            result.Radius = max;
        }

        /// <summary>
        /// Constructs a <see cref="BoundingSphere"/> that is the as large as the total combined area of the two specified spheres.
        /// </summary>
        /// <param name="value1">The first sphere to merge.</param>
        /// <param name="value2">The second sphere to merge.</param>
        /// <returns>The newly constructed bounding sphere.</returns>
        public static BoundingSphere Merge(BoundingSphere value1, BoundingSphere value2)
        {
            BoundingSphere result;
            Merge(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator ==(BoundingSphere left, BoundingSphere right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator !=(BoundingSphere left, BoundingSphere right)
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
            return string.Format(CultureInfo.CurrentCulture, "Center:{0} Radius:{1}", Center.ToString(), Radius.ToString());
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
            if (format == null)
            {
                return this.ToString();
            }

            return string.Format(CultureInfo.CurrentCulture, "Center:{0} Radius:{1}", Center.ToString(format, CultureInfo.CurrentCulture),
                Radius.ToString(format, CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(IFormatProvider? formatProvider)
        {
            return string.Format(formatProvider, "Center:{0} Radius:{1}", Center.ToString(), Radius.ToString());
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            if (format == null)
            {
                return this.ToString(formatProvider);
            }

            return string.Format(formatProvider, "Center:{0} Radius:{1}", Center.ToString(format, formatProvider),
                Radius.ToString(format, formatProvider));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Center.GetHashCode() + Radius.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Numerics.Vector4"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="System.Numerics.Vector4"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Numerics.Vector4"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(BoundingSphere value)
        {
            return Center == value.Center && Radius == value.Radius;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? value)
        {
            if (value == null)
            {
                return false;
            }

            if (!ReferenceEquals(value.GetType(), typeof(BoundingSphere)))
            {
                return false;
            }

            return this.Equals((BoundingSphere)value);
        }
    }
}
