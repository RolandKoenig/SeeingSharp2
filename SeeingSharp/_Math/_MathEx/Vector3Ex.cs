#region License information
/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
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
namespace SeeingSharp
{
    #region using

    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using SharpDX;

    #endregion

    public static class Vector3Ex
    {
        /// <summary>
        /// Generates a normal out of given horizontal and vertical rotation.
        /// </summary>
        /// <param name="horizontalRotation">Horizontal rotation value.</param>
        /// <param name="verticalRotation">Vertical rotation value.</param>
        public static Vector3 NormalFromHVRotation(float horizontalRotation, float verticalRotation)
        {
            var result = Vector3.Zero;

            //Generate vector
            result.X = (float)(1f * Math.Cos(verticalRotation) * Math.Cos(horizontalRotation));
            result.Y = (float)(1f * Math.Sin(verticalRotation));
            result.Z = (float)(1f * Math.Cos(verticalRotation) * Math.Sin(horizontalRotation));

            //Normalize the generated vector
            result = Vector3.Normalize(result);

            return result;
        }

        /// <summary>
        /// Generates a normal out of given horizontal and vertical rotation.
        /// </summary>
        /// <param name="rotation">Vector containing horizontal and vertical rotations.</param>
        public static Vector3 NormalFromHVRotation(Vector2 rotation)
        {
            return NormalFromHVRotation(rotation.X, rotation.Y);
        }

        /// <summary>
        /// Gets an average vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Average(params Vector3[] vectors)
        {
            if (vectors.Length == 0)
            {
                return Vector3.Zero;
            }

            var result = Vector3Ex.Sum(vectors);

            result.X = result.X / (float)vectors.Length;
            result.Y = result.Y / (float)vectors.Length;
            result.Z = result.Z / (float)vectors.Length;

            return result;
        }

        /// <summary>
        /// Gets an average vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Average(List<Vector3> vectors)
        {
            if (vectors.Count == 0)
            {
                return Vector3.Zero;
            }

            var result = Vector3Ex.Sum(vectors);

            result.X = result.X / (float)vectors.Count;
            result.Y = result.Y / (float)vectors.Count;
            result.Z = result.Z / (float)vectors.Count;

            return result;
        }

        /// <summary>
        /// Converts this vector to a vector containing horizontal and vertical rotation values.
        /// </summary>
        /// <param name="vector">The vector to be converted.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToHVRotation(Vector3 vector)
        {
            var normal = Vector3.Normalize(vector);

            var result = new Vector2
            {
                X = (float) Math.Atan2(normal.Z, normal.X),
                Y = (float) Math.Atan2(normal.Y, new Vector2(normal.Z, normal.X).Length())
            };

            return result;
        }

        /// <summary>
        /// Gets the a vector containing the sum of each given vector.
        /// </summary>
        /// <param name="vectors">The vectors to add one by one.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Sum(params Vector3[] vectors)
        {
            var result = Vector3.Zero;

            for (var loop = 0; loop < vectors.Length; loop++)
            {
                result = result + vectors[loop];
            }

            return result;
        }

        /// <summary>
        /// Gets the a vector containing the sum of each given vector.
        /// </summary>
        /// <param name="vectors">The vectors to add one by one.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Sum(List<Vector3> vectors)
        {
            var result = Vector3.Zero;

            for (var loop = 0; loop < vectors.Count; loop++)
            {
                result = result + vectors[loop];
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetXY(Vector3 vector3)
        {
            return new Vector2(vector3.X, vector3.Y);
        }

        /// <summary>
        /// Writes horizontal and vertical rotation values to given parameters.
        /// </summary>
        /// <param name="vector">The vector to be converted.</param>
        /// <param name="hRotation">Parameter for horizontal rotation.</param>
        /// <param name="vRotation">Parameter for vertical rotation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToHVRotation(Vector3 vector, out float hRotation, out float vRotation)
        {
            var normal = Vector3.Normalize(vector);

            hRotation = (float)Math.Atan2(normal.Z, normal.X);
            vRotation = (float)Math.Atan2(normal.Y, new Vector2(normal.Z, normal.X).Length());
        }

        /// <summary>
        /// Calculates the normal of the given triangle
        /// </summary>
        /// <param name="p0">First point of the triangle.</param>
        /// <param name="p1">Second point of the triangle.</param>
        /// <param name="p2">Third point of the triangle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 CalculateTriangleNormal(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            return CalculateTriangleNormal(p0, p1, p2, true);
        }

        /// <summary>
        /// Calculates the normal of the given triangle
        /// </summary>
        /// <param name="p0">First point of the triangle.</param>
        /// <param name="p1">Second point of the triangle.</param>
        /// <param name="p2">Third point of the triangle.</param>
        /// <param name="doNormalize">Setting this parameter to false causes the result normal to be not normalized after calculation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 CalculateTriangleNormal(Vector3 p0, Vector3 p1, Vector3 p2, bool doNormalize)
        {
            var result = new Vector3();

            // Calculation of the normal based on 'Mathematics for 3D Game Programming and Computer Graphics (Eric Lengyel, 2012)
            //  Page 175: 7.7.1 Calculating Normal Vectors
            //
            //  We have two modes: Normalized and unnormalized form of the result
            if (doNormalize)
            {
                var crossProductVector = Vector3.Cross(p1 - p0, p2 - p0);
                result = crossProductVector / crossProductVector.Length();
            }
            else
            {
                result = Vector3.Cross(p1 - p0, p2 - p0);
            }

            return result;
        }

        /// <summary>
        /// Is this vector empty?
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty(this Vector3 vector)
        {
            return vector.Equals(Vector3.Zero);
        }

        /// <summary>
        /// Returns the reflection of a vector off a surface that has the specified normal.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="normal">Normal of the surface.</param>
        /// <param name="result">When the method completes, contains the reflected vector.</param>
        /// <remarks>Reflect only gives the direction of a reflection off a surface, it does not determine
        /// whether the original vector was close enough to the surface to hit it.</remarks>
        public static void Reflect(ref Vector3 vector, ref Vector3 normal, out Vector3 result)
        {
            float dot = (vector.X * normal.X) + (vector.Y * normal.Y) + (vector.Z * normal.Z);

            result.X = vector.X - ((2.0f * dot) * normal.X);
            result.Y = vector.Y - ((2.0f * dot) * normal.Y);
            result.Z = vector.Z - ((2.0f * dot) * normal.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetValue(this Vector3 vector, int index)
        {
            switch(index)
            {
                case 1: return vector.X;
                case 2: return vector.Y;
                case 3: return vector.Z;
                default: throw new ArgumentException("Invalid index!");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue(this Vector3 vector, int index, float value)
        {
            switch(index)
            {
                case 1: vector.X = value; break;
                case 2: vector.Y = value; break;
                case 3: vector.Z = value; break;
                default: throw new ArgumentException("Invalid index!");
            }
        }

        public static Vector3 MinValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector3(float.MinValue, float.MinValue, float.MinValue); }
        }

        public static Vector3 MaxValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector3(float.MaxValue, float.MaxValue, float.MaxValue); }
        }
    }
}
