﻿using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SeeingSharp.Mathematics
{
    /// <summary>
    /// Represents a plane in three dimensional space.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public partial struct Plane : IEquatable<Plane>, IFormattable
    {
        /// <summary>
        /// The normal vector of the plane.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// The distance of the plane along its normal from the origin.
        /// </summary>
        public float D;

        /// <summary>
        /// Initializes a new instance of the <see cref="Plane"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public Plane(float value)
        {
            Normal.X = Normal.Y = Normal.Z = D = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Plane"/> struct.
        /// </summary>
        /// <param name="a">The X component of the normal.</param>
        /// <param name="b">The Y component of the normal.</param>
        /// <param name="c">The Z component of the normal.</param>
        /// <param name="d">The distance of the plane along its normal from the origin.</param>
        public Plane(float a, float b, float c, float d)
        {
            Normal.X = a;
            Normal.Y = b;
            Normal.Z = c;
            D = d;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SeeingSharp.Plane" /> class.
        /// </summary>
        /// <param name="point">Any point that lies along the plane.</param>
        /// <param name="normal">The normal vector to the plane.</param>
        public Plane(Vector3 point, Vector3 normal)
        {
            Normal = normal;
            D = -Vector3.Dot(normal, point);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Plane"/> struct.
        /// </summary>
        /// <param name="value">The normal of the plane.</param>
        /// <param name="d">The distance of the plane along its normal from the origin</param>
        public Plane(Vector3 value, float d)
        {
            Normal = value;
            D = d;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Plane"/> struct.
        /// </summary>
        /// <param name="point1">First point of a triangle defining the plane.</param>
        /// <param name="point2">Second point of a triangle defining the plane.</param>
        /// <param name="point3">Third point of a triangle defining the plane.</param>
        public Plane(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            var x1 = point2.X - point1.X;
            var y1 = point2.Y - point1.Y;
            var z1 = point2.Z - point1.Z;
            var x2 = point3.X - point1.X;
            var y2 = point3.Y - point1.Y;
            var z2 = point3.Z - point1.Z;
            var yz = y1 * z2 - z1 * y2;
            var xz = z1 * x2 - x1 * z2;
            var xy = x1 * y2 - y1 * x2;
            var invPyth = 1.0f / (float)Math.Sqrt(yz * yz + xz * xz + xy * xy);

            Normal.X = yz * invPyth;
            Normal.Y = xz * invPyth;
            Normal.Z = xy * invPyth;
            D = -(Normal.X * point1.X + Normal.Y * point1.Y + Normal.Z * point1.Z);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Plane"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the A, B, C, and D components of the plane. This must be an array with four elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
        public Plane(float[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            if (values.Length != 4)
            {
                throw new ArgumentOutOfRangeException("values", "There must be four and only four input values for Plane.");
            }

            Normal.X = values[0];
            Normal.Y = values[1];
            Normal.Z = values[2];
            D = values[3];
        }

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the A, B, C, or D component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the A component, 1 for the B component, 2 for the C component, and 3 for the D component.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 3].</exception>
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return Normal.X;
                    case 1: return Normal.Y;
                    case 2: return Normal.Z;
                    case 3: return D;
                }

                throw new ArgumentOutOfRangeException("index", "Indices for Plane run from 0 to 3, inclusive.");
            }

            set
            {
                switch (index)
                {
                    case 0: Normal.X = value; break;
                    case 1: Normal.Y = value; break;
                    case 2: Normal.Z = value; break;
                    case 3: D = value; break;
                    default: throw new ArgumentOutOfRangeException("index", "Indices for Plane run from 0 to 3, inclusive.");
                }
            }
        }

        /// <summary>
        /// Changes the coefficients of the normal vector of the plane to make it of unit length.
        /// </summary>
        public void Normalize()
        {
            var magnitude = 1.0f / (float)Math.Sqrt(Normal.X * Normal.X + Normal.Y * Normal.Y + Normal.Z * Normal.Z);

            Normal.X *= magnitude;
            Normal.Y *= magnitude;
            Normal.Z *= magnitude;
            D *= magnitude;
        }

        /// <summary>
        /// Creates an array containing the elements of the plane.
        /// </summary>
        /// <returns>A four-element array containing the components of the plane.</returns>
        public float[] ToArray()
        {
            return new[] { Normal.X, Normal.Y, Normal.Z, D };
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a point.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public PlaneIntersectionType Intersects(ref Vector3 point)
        {
            return Collision.PlaneIntersectsPoint(ref this, ref point);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="Ray"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Ray ray)
        {
            float distance;
            return Collision.RayIntersectsPlane(ref ray, ref this, out distance);
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
            return Collision.RayIntersectsPlane(ref ray, ref this, out distance);
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
            return Collision.RayIntersectsPlane(ref ray, ref this, out point);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Plane plane)
        {
            return Collision.PlaneIntersectsPlane(ref this, ref plane);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="line">When the method completes, contains the line of intersection
        /// as a <see cref="Ray"/>, or a zero ray if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Plane plane, out Ray line)
        {
            return Collision.PlaneIntersectsPlane(ref this, ref plane, out line);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a triangle.
        /// </summary>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triagnle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public PlaneIntersectionType Intersects(ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
        {
            return Collision.PlaneIntersectsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public PlaneIntersectionType Intersects(ref BoundingBox box)
        {
            return Collision.PlaneIntersectsBox(ref this, ref box);
        }

        /// <summary>
        /// Determines if there is an intersection between the current object and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public PlaneIntersectionType Intersects(ref BoundingSphere sphere)
        {
            return Collision.PlaneIntersectsSphere(ref this, ref sphere);
        }

        /// <summary>
        /// Scales the plane by the given scaling factor.
        /// </summary>
        /// <param name="value">The plane to scale.</param>
        /// <param name="scale">The amount by which to scale the plane.</param>
        /// <param name="result">When the method completes, contains the scaled plane.</param>
        public static void Multiply(ref Plane value, float scale, out Plane result)
        {
            result.Normal.X = value.Normal.X * scale;
            result.Normal.Y = value.Normal.Y * scale;
            result.Normal.Z = value.Normal.Z * scale;
            result.D = value.D * scale;
        }

        /// <summary>
        /// Scales the plane by the given scaling factor.
        /// </summary>
        /// <param name="value">The plane to scale.</param>
        /// <param name="scale">The amount by which to scale the plane.</param>
        /// <returns>The scaled plane.</returns>
        public static Plane Multiply(Plane value, float scale)
        {
            return new Plane(value.Normal.X * scale, value.Normal.Y * scale, value.Normal.Z * scale, value.D * scale);
        }

        /// <summary>
        /// Calculates the dot product of the specified vector and plane.
        /// </summary>
        /// <param name="left">The source plane.</param>
        /// <param name="right">The source vector.</param>
        /// <param name="result">When the method completes, contains the dot product of the specified plane and vector.</param>
        public static void Dot(ref Plane left, ref Vector4 right, out float result)
        {
            result = left.Normal.X * right.X + left.Normal.Y * right.Y + left.Normal.Z * right.Z + left.D * right.W;
        }

        /// <summary>
        /// Calculates the dot product of the specified vector and plane.
        /// </summary>
        /// <param name="left">The source plane.</param>
        /// <param name="right">The source vector.</param>
        /// <returns>The dot product of the specified plane and vector.</returns>
        public static float Dot(Plane left, Vector4 right)
        {
            return left.Normal.X * right.X + left.Normal.Y * right.Y + left.Normal.Z * right.Z + left.D * right.W;
        }

        /// <summary>
        /// Calculates the dot product of a specified vector and the normal of the plane plus the distance value of the plane.
        /// </summary>
        /// <param name="left">The source plane.</param>
        /// <param name="right">The source vector.</param>
        /// <param name="result">When the method completes, contains the dot product of a specified vector and the normal of the Plane plus the distance value of the plane.</param>
        public static void DotCoordinate(ref Plane left, ref Vector3 right, out float result)
        {
            result = left.Normal.X * right.X + left.Normal.Y * right.Y + left.Normal.Z * right.Z + left.D;
        }

        /// <summary>
        /// Calculates the dot product of a specified vector and the normal of the plane plus the distance value of the plane.
        /// </summary>
        /// <param name="left">The source plane.</param>
        /// <param name="right">The source vector.</param>
        /// <returns>The dot product of a specified vector and the normal of the Plane plus the distance value of the plane.</returns>
        public static float DotCoordinate(Plane left, Vector3 right)
        {
            return left.Normal.X * right.X + left.Normal.Y * right.Y + left.Normal.Z * right.Z + left.D;
        }

        /// <summary>
        /// Calculates the dot product of the specified vector and the normal of the plane.
        /// </summary>
        /// <param name="left">The source plane.</param>
        /// <param name="right">The source vector.</param>
        /// <param name="result">When the method completes, contains the dot product of the specified vector and the normal of the plane.</param>
        public static void DotNormal(ref Plane left, ref Vector3 right, out float result)
        {
            result = left.Normal.X * right.X + left.Normal.Y * right.Y + left.Normal.Z * right.Z;
        }

        /// <summary>
        /// Calculates the dot product of the specified vector and the normal of the plane.
        /// </summary>
        /// <param name="left">The source plane.</param>
        /// <param name="right">The source vector.</param>
        /// <returns>The dot product of the specified vector and the normal of the plane.</returns>
        public static float DotNormal(Plane left, Vector3 right)
        {
            return left.Normal.X * right.X + left.Normal.Y * right.Y + left.Normal.Z * right.Z;
        }

        /// <summary>
        /// Changes the coefficients of the normal vector of the plane to make it of unit length.
        /// </summary>
        /// <param name="plane">The source plane.</param>
        /// <param name="result">When the method completes, contains the normalized plane.</param>
        public static void Normalize(ref Plane plane, out Plane result)
        {
            var magnitude = 1.0f / (float)Math.Sqrt(plane.Normal.X * plane.Normal.X + plane.Normal.Y * plane.Normal.Y + plane.Normal.Z * plane.Normal.Z);

            result.Normal.X = plane.Normal.X * magnitude;
            result.Normal.Y = plane.Normal.Y * magnitude;
            result.Normal.Z = plane.Normal.Z * magnitude;
            result.D = plane.D * magnitude;
        }

        /// <summary>
        /// Changes the coefficients of the normal vector of the plane to make it of unit length.
        /// </summary>
        /// <param name="plane">The source plane.</param>
        /// <returns>The normalized plane.</returns>
        public static Plane Normalize(Plane plane)
        {
            var magnitude = 1.0f / (float)Math.Sqrt(plane.Normal.X * plane.Normal.X + plane.Normal.Y * plane.Normal.Y + plane.Normal.Z * plane.Normal.Z);
            return new Plane(plane.Normal.X * magnitude, plane.Normal.Y * magnitude, plane.Normal.Z * magnitude, plane.D * magnitude);
        }

        /// <summary>
        /// Transforms a normalized plane by a quaternion rotation.
        /// </summary>
        /// <param name="plane">The normalized source plane.</param>
        /// <param name="rotation">The quaternion rotation.</param>
        /// <param name="result">When the method completes, contains the transformed plane.</param>
        public static void Transform(ref Plane plane, ref Quaternion rotation, out Plane result)
        {
            var x2 = rotation.X + rotation.X;
            var y2 = rotation.Y + rotation.Y;
            var z2 = rotation.Z + rotation.Z;
            var wx = rotation.W * x2;
            var wy = rotation.W * y2;
            var wz = rotation.W * z2;
            var xx = rotation.X * x2;
            var xy = rotation.X * y2;
            var xz = rotation.X * z2;
            var yy = rotation.Y * y2;
            var yz = rotation.Y * z2;
            var zz = rotation.Z * z2;

            var x = plane.Normal.X;
            var y = plane.Normal.Y;
            var z = plane.Normal.Z;

            result.Normal.X = x * (1.0f - yy - zz) + y * (xy - wz) + z * (xz + wy);
            result.Normal.Y = x * (xy + wz) + y * (1.0f - xx - zz) + z * (yz - wx);
            result.Normal.Z = x * (xz - wy) + y * (yz + wx) + z * (1.0f - xx - yy);
            result.D = plane.D;
        }

        /// <summary>
        /// Transforms a normalized plane by a quaternion rotation.
        /// </summary>
        /// <param name="plane">The normalized source plane.</param>
        /// <param name="rotation">The quaternion rotation.</param>
        /// <returns>The transformed plane.</returns>
        public static Plane Transform(Plane plane, Quaternion rotation)
        {
            Plane result;
            var x2 = rotation.X + rotation.X;
            var y2 = rotation.Y + rotation.Y;
            var z2 = rotation.Z + rotation.Z;
            var wx = rotation.W * x2;
            var wy = rotation.W * y2;
            var wz = rotation.W * z2;
            var xx = rotation.X * x2;
            var xy = rotation.X * y2;
            var xz = rotation.X * z2;
            var yy = rotation.Y * y2;
            var yz = rotation.Y * z2;
            var zz = rotation.Z * z2;

            var x = plane.Normal.X;
            var y = plane.Normal.Y;
            var z = plane.Normal.Z;

            result.Normal.X = x * (1.0f - yy - zz) + y * (xy - wz) + z * (xz + wy);
            result.Normal.Y = x * (xy + wz) + y * (1.0f - xx - zz) + z * (yz - wx);
            result.Normal.Z = x * (xz - wy) + y * (yz + wx) + z * (1.0f - xx - yy);
            result.D = plane.D;

            return result;
        }

        /// <summary>
        /// Transforms an array of normalized planes by a quaternion rotation.
        /// </summary>
        /// <param name="planes">The array of normalized planes to transform.</param>
        /// <param name="rotation">The quaternion rotation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="planes"/> is <c>null</c>.</exception>
        public static void Transform(Plane[] planes, ref Quaternion rotation)
        {
            if (planes == null)
            {
                throw new ArgumentNullException("planes");
            }

            var x2 = rotation.X + rotation.X;
            var y2 = rotation.Y + rotation.Y;
            var z2 = rotation.Z + rotation.Z;
            var wx = rotation.W * x2;
            var wy = rotation.W * y2;
            var wz = rotation.W * z2;
            var xx = rotation.X * x2;
            var xy = rotation.X * y2;
            var xz = rotation.X * z2;
            var yy = rotation.Y * y2;
            var yz = rotation.Y * z2;
            var zz = rotation.Z * z2;

            for (var i = 0; i < planes.Length; ++i)
            {
                var x = planes[i].Normal.X;
                var y = planes[i].Normal.Y;
                var z = planes[i].Normal.Z;

                /*
                 * Note:
                 * Factor common arithmetic out of loop.
                */
                planes[i].Normal.X = x * (1.0f - yy - zz) + y * (xy - wz) + z * (xz + wy);
                planes[i].Normal.Y = x * (xy + wz) + y * (1.0f - xx - zz) + z * (yz - wx);
                planes[i].Normal.Z = x * (xz - wy) + y * (yz + wx) + z * (1.0f - xx - yy);
            }
        }

        /// <summary>
        /// Transforms a normalized plane by a matrix.
        /// </summary>
        /// <param name="plane">The normalized source plane.</param>
        /// <param name="transformation">The transformation matrix.</param>
        /// <param name="result">When the method completes, contains the transformed plane.</param>
        public static void Transform(ref Plane plane, ref Matrix4x4 transformation, out Plane result)
        {
            var x = plane.Normal.X;
            var y = plane.Normal.Y;
            var z = plane.Normal.Z;
            var d = plane.D;

            Matrix4x4 inverse;
            Matrix4x4.Invert(transformation, out inverse);

            result.Normal.X = x * inverse.M11 + y * inverse.M12 + z * inverse.M13 + d * inverse.M14;
            result.Normal.Y = x * inverse.M21 + y * inverse.M22 + z * inverse.M23 + d * inverse.M24;
            result.Normal.Z = x * inverse.M31 + y * inverse.M32 + z * inverse.M33 + d * inverse.M34;
            result.D = x * inverse.M41 + y * inverse.M42 + z * inverse.M43 + d * inverse.M44;
        }

        /// <summary>
        /// Transforms a normalized plane by a matrix.
        /// </summary>
        /// <param name="plane">The normalized source plane.</param>
        /// <param name="transformation">The transformation matrix.</param>
        /// <returns>When the method completes, contains the transformed plane.</returns>
        public static Plane Transform(Plane plane, Matrix4x4 transformation)
        {
            Plane result;
            var x = plane.Normal.X;
            var y = plane.Normal.Y;
            var z = plane.Normal.Z;
            var d = plane.D;

            Matrix4x4 inverted;
            Matrix4x4.Invert(transformation, out inverted);
            result.Normal.X = x * inverted.M11 + y * inverted.M12 + z * inverted.M13 + d * inverted.M14;
            result.Normal.Y = x * inverted.M21 + y * inverted.M22 + z * inverted.M23 + d * inverted.M24;
            result.Normal.Z = x * inverted.M31 + y * inverted.M32 + z * inverted.M33 + d * inverted.M34;
            result.D = x * inverted.M41 + y * inverted.M42 + z * inverted.M43 + d * inverted.M44;

            return result;
        }

        /// <summary>
        /// Transforms an array of normalized planes by a matrix.
        /// </summary>
        /// <param name="planes">The array of normalized planes to transform.</param>
        /// <param name="transformation">The transformation matrix.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="planes"/> is <c>null</c>.</exception>
        public static void Transform(Plane[] planes, ref Matrix4x4 transformation)
        {
            if (planes == null)
            {
                throw new ArgumentNullException("planes");
            }

            Matrix4x4 inverse;
            Matrix4x4.Invert(transformation, out inverse);

            for (var i = 0; i < planes.Length; ++i)
            {
                Transform(ref planes[i], ref transformation, out planes[i]);
            }
        }

        /// <summary>
        /// Scales a plane by the given value.
        /// </summary>
        /// <param name="scale">The amount by which to scale the plane.</param>
        /// <param name="plane">The plane to scale.</param>
        /// <returns>The scaled plane.</returns>
        public static Plane operator *(float scale, Plane plane)
        {
            return new Plane(plane.Normal.X * scale, plane.Normal.Y * scale, plane.Normal.Z * scale, plane.D * scale);
        }

        /// <summary>
        /// Scales a plane by the given value.
        /// </summary>
        /// <param name="plane">The plane to scale.</param>
        /// <param name="scale">The amount by which to scale the plane.</param>
        /// <returns>The scaled plane.</returns>
        public static Plane operator *(Plane plane, float scale)
        {
            return new Plane(plane.Normal.X * scale, plane.Normal.Y * scale, plane.Normal.Z * scale, plane.D * scale);
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Plane left, Plane right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Plane left, Plane right)
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
            return string.Format(CultureInfo.CurrentCulture, "A:{0} B:{1} C:{2} D:{3}", Normal.X, Normal.Y, Normal.Z, D);
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
            return string.Format(CultureInfo.CurrentCulture, "A:{0} B:{1} C:{2} D:{3}", Normal.X.ToString(format, CultureInfo.CurrentCulture),
                Normal.Y.ToString(format, CultureInfo.CurrentCulture), Normal.Z.ToString(format, CultureInfo.CurrentCulture), D.ToString(format, CultureInfo.CurrentCulture));
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
            return string.Format(formatProvider, "A:{0} B:{1} C:{2} D:{3}", Normal.X, Normal.Y, Normal.Z, D);
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
            return string.Format(formatProvider, "A:{0} B:{1} C:{2} D:{3}", Normal.X.ToString(format, formatProvider),
                Normal.Y.ToString(format, formatProvider), Normal.Z.ToString(format, formatProvider), D.ToString(format, formatProvider));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Normal.GetHashCode() + D.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Numerics.Vector4"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="System.Numerics.Vector4"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Numerics.Vector4"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Plane value)
        {
            return Normal == value.Normal && D == value.D;
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

            if (!ReferenceEquals(value.GetType(), typeof(Plane)))
            {
                return false;
            }

            return this.Equals((Plane)value);
        }
    }
}
