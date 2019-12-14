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
using System.Runtime.CompilerServices;

namespace SeeingSharp
{
    public static class Matrix4x4Ex
    {
        /// <summary>
        /// Creates a left-handed, orthographic projection matrix.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void CreateOrthoLH(float width, float height, float znear, float zfar, out Matrix4x4 result)
        {
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            CreateOrthoOffCenterLH(-halfWidth, halfWidth, -halfHeight, halfHeight, znear, zfar, out result);
        }

        /// <summary>
        /// Creates a left-handed, orthographic projection matrix.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4x4 CreateOrthoLH(float width, float height, float znear, float zfar)
        {
            Matrix4x4 result;
            CreateOrthoLH(width, height, znear, zfar, out result);
            return result;
        }

        /// <summary>
        /// Creates a right-handed, orthographic projection matrix.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void CreateOrthoRH(float width, float height, float znear, float zfar, out Matrix4x4 result)
        {
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            CreateOrthoOffCenterRH(-halfWidth, halfWidth, -halfHeight, halfHeight, znear, zfar, out result);
        }

        /// <summary>
        /// Creates a right-handed, orthographic projection matrix.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4x4 CreateOrthoRH(float width, float height, float znear, float zfar)
        {
            Matrix4x4 result;
            CreateOrthoRH(width, height, znear, zfar, out result);
            return result;
        }

        /// <summary>
        /// Creates a left-handed, customized orthographic projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the viewing volume.</param>
        /// <param name="right">Maximum x-value of the viewing volume.</param>
        /// <param name="bottom">Minimum y-value of the viewing volume.</param>
        /// <param name="top">Maximum y-value of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void CreateOrthoOffCenterLH(float left, float right, float bottom, float top, float znear, float zfar, out Matrix4x4 result)
        {
            float zRange = 1.0f / (zfar - znear);

            result = Matrix4x4.Identity;
            result.M11 = 2.0f / (right - left);
            result.M22 = 2.0f / (top - bottom);
            result.M33 = zRange;
            result.M41 = (left + right) / (left - right);
            result.M42 = (top + bottom) / (bottom - top);
            result.M43 = -znear * zRange;
        }

        /// <summary>
        /// Creates a left-handed, customized orthographic projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the viewing volume.</param>
        /// <param name="right">Maximum x-value of the viewing volume.</param>
        /// <param name="bottom">Minimum y-value of the viewing volume.</param>
        /// <param name="top">Maximum y-value of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4x4 CreateOrthoOffCenterLH(float left, float right, float bottom, float top, float znear, float zfar)
        {
            Matrix4x4 result;
            CreateOrthoOffCenterLH(left, right, bottom, top, znear, zfar, out result);
            return result;
        }

        /// <summary>
        /// Creates a right-handed, customized orthographic projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the viewing volume.</param>
        /// <param name="right">Maximum x-value of the viewing volume.</param>
        /// <param name="bottom">Minimum y-value of the viewing volume.</param>
        /// <param name="top">Maximum y-value of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void CreateOrthoOffCenterRH(float left, float right, float bottom, float top, float znear, float zfar, out Matrix4x4 result)
        {
            CreateOrthoOffCenterLH(left, right, bottom, top, znear, zfar, out result);
            result.M33 *= -1.0f;
        }

        /// <summary>
        /// Creates a right-handed, customized orthographic projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the viewing volume.</param>
        /// <param name="right">Maximum x-value of the viewing volume.</param>
        /// <param name="bottom">Minimum y-value of the viewing volume.</param>
        /// <param name="top">Maximum y-value of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4x4 CreateOrthoOffCenterRH(float left, float right, float bottom, float top, float znear, float zfar)
        {
            Matrix4x4 result;
            CreateOrthoOffCenterRH(left, right, bottom, top, znear, zfar, out result);
            return result;
        }

        /// <summary>
        /// Gets a rotation matrix for the given direction vectors.
        /// </summary>
        /// <param name="upVector">The up vector (standard: y-axis).</param>
        /// <param name="forwardVector">The forward vector (standard: x-axis)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 CreateRotationDirection(Vector3 upVector, Vector3 forwardVector)
        {
            Vector3 right = Vector3.Cross(upVector, forwardVector);
            return new Matrix4x4(
                right.X, right.Y, right.Z, 0f,
                upVector.X, upVector.Y, upVector.Z, 0f,
                forwardVector.X, forwardVector.Y, forwardVector.Z, 0f,
                0f, 0f, 0f, 1f);
        }

        /// <summary>
        /// Creates a left-handed, perspective projection matrix.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void CreatePerspectiveLH(float width, float height, float znear, float zfar, out Matrix4x4 result)
        {
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            CreatePerspectiveOffCenterLH(-halfWidth, halfWidth, -halfHeight, halfHeight, znear, zfar, out result);
        }

        /// <summary>
        /// Creates a left-handed, perspective projection matrix.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4x4 CreatePerspectiveLH(float width, float height, float znear, float zfar)
        {
            Matrix4x4 result;
            CreatePerspectiveLH(width, height, znear, zfar, out result);
            return result;
        }

        /// <summary>
        /// Creates a right-handed, perspective projection matrix.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void CreatePerspectiveRH(float width, float height, float znear, float zfar, out Matrix4x4 result)
        {
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            CreatePerspectiveOffCenterRH(-halfWidth, halfWidth, -halfHeight, halfHeight, znear, zfar, out result);
        }

        /// <summary>
        /// Creates a right-handed, perspective projection matrix.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4x4 CreatePerspectiveRH(float width, float height, float znear, float zfar)
        {
            Matrix4x4 result;
            CreatePerspectiveRH(width, height, znear, zfar, out result);
            return result;
        }

        /// <summary>
        /// Creates a left-handed, perspective projection matrix based on a field of view.
        /// </summary>
        /// <param name="fov">Field of view in the y direction, in radians.</param>
        /// <param name="aspect">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void CreatePerspectiveFovLH(float fov, float aspect, float znear, float zfar, out Matrix4x4 result)
        {
            float yScale = (float)(1.0 / Math.Tan(fov * 0.5f));
            float xScale = yScale / aspect;

            float halfWidth = znear / xScale;
            float halfHeight = znear / yScale;

            CreatePerspectiveOffCenterLH(-halfWidth, halfWidth, -halfHeight, halfHeight, znear, zfar, out result);
        }

        /// <summary>
        /// Creates a left-handed, perspective projection matrix based on a field of view.
        /// </summary>
        /// <param name="fov">Field of view in the y direction, in radians.</param>
        /// <param name="aspect">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4x4 CreatePerspectiveFovLH(float fov, float aspect, float znear, float zfar)
        {
            Matrix4x4 result;
            CreatePerspectiveFovLH(fov, aspect, znear, zfar, out result);
            return result;
        }

        /// <summary>
        /// Creates a right-handed, perspective projection matrix based on a field of view.
        /// </summary>
        /// <param name="fov">Field of view in the y direction, in radians.</param>
        /// <param name="aspect">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void CreatePerspectiveFovRH(float fov, float aspect, float znear, float zfar, out Matrix4x4 result)
        {
            float yScale = (float)(1.0 / Math.Tan(fov * 0.5f));
            float xScale = yScale / aspect;

            float halfWidth = znear / xScale;
            float halfHeight = znear / yScale;

            CreatePerspectiveOffCenterRH(-halfWidth, halfWidth, -halfHeight, halfHeight, znear, zfar, out result);
        }

        /// <summary>
        /// Creates a right-handed, perspective projection matrix based on a field of view.
        /// </summary>
        /// <param name="fov">Field of view in the y direction, in radians.</param>
        /// <param name="aspect">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4x4 CreatePerspectiveFovRH(float fov, float aspect, float znear, float zfar)
        {
            Matrix4x4 result;
            CreatePerspectiveFovRH(fov, aspect, znear, zfar, out result);
            return result;
        }

        /// <summary>
        /// Creates a left-handed, customized perspective projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the viewing volume.</param>
        /// <param name="right">Maximum x-value of the viewing volume.</param>
        /// <param name="bottom">Minimum y-value of the viewing volume.</param>
        /// <param name="top">Maximum y-value of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void CreatePerspectiveOffCenterLH(float left, float right, float bottom, float top, float znear, float zfar, out Matrix4x4 result)
        {
            float zRange = zfar / (zfar - znear);

            result = new Matrix4x4();
            result.M11 = 2.0f * znear / (right - left);
            result.M22 = 2.0f * znear / (top - bottom);
            result.M31 = (left + right) / (left - right);
            result.M32 = (top + bottom) / (bottom - top);
            result.M33 = zRange;
            result.M34 = 1.0f;
            result.M43 = -znear * zRange;
        }

        /// <summary>
        /// Creates a left-handed, customized perspective projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the viewing volume.</param>
        /// <param name="right">Maximum x-value of the viewing volume.</param>
        /// <param name="bottom">Minimum y-value of the viewing volume.</param>
        /// <param name="top">Maximum y-value of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4x4 CreatePerspectiveOffCenterLH(float left, float right, float bottom, float top, float znear, float zfar)
        {
            Matrix4x4 result;
            CreatePerspectiveOffCenterLH(left, right, bottom, top, znear, zfar, out result);
            return result;
        }

        /// <summary>
        /// Creates a right-handed, customized perspective projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the viewing volume.</param>
        /// <param name="right">Maximum x-value of the viewing volume.</param>
        /// <param name="bottom">Minimum y-value of the viewing volume.</param>
        /// <param name="top">Maximum y-value of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void CreatePerspectiveOffCenterRH(float left, float right, float bottom, float top, float znear, float zfar, out Matrix4x4 result)
        {
            CreatePerspectiveOffCenterLH(left, right, bottom, top, znear, zfar, out result);
            result.M31 *= -1.0f;
            result.M32 *= -1.0f;
            result.M33 *= -1.0f;
            result.M34 *= -1.0f;
        }

        /// <summary>
        /// Creates a right-handed, customized perspective projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the viewing volume.</param>
        /// <param name="right">Maximum x-value of the viewing volume.</param>
        /// <param name="bottom">Minimum y-value of the viewing volume.</param>
        /// <param name="top">Maximum y-value of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4x4 CreatePerspectiveOffCenterRH(float left, float right, float bottom, float top, float znear, float zfar)
        {
            Matrix4x4 result;
            CreatePerspectiveOffCenterRH(left, right, bottom, top, znear, zfar, out result);
            return result;
        }

        /// <summary>
        /// Creates a left-handed, look-at matrix.
        /// </summary>
        /// <param name="eye">The position of the viewer's eye.</param>
        /// <param name="target">The camera look-at target.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <param name="result">When the method completes, contains the created look-at matrix.</param>
        public static void CreateLookAtLH(ref Vector3 eye, ref Vector3 target, ref Vector3 up, out Matrix4x4 result)
        {
            Vector3 xaxis, yaxis, zaxis;
            zaxis = Vector3.Normalize(Vector3.Subtract(target, eye));
            xaxis = Vector3.Normalize(Vector3.Cross(up, zaxis));
            yaxis = Vector3.Cross(zaxis, xaxis);

            result = Matrix4x4.Identity;
            result.M11 = xaxis.X; result.M21 = xaxis.Y; result.M31 = xaxis.Z;
            result.M12 = yaxis.X; result.M22 = yaxis.Y; result.M32 = yaxis.Z;
            result.M13 = zaxis.X; result.M23 = zaxis.Y; result.M33 = zaxis.Z;

            result.M41 = Vector3.Dot(xaxis, eye);
            result.M42 = Vector3.Dot(yaxis, eye);
            result.M43 = Vector3.Dot(zaxis, eye);

            result.M41 = -result.M41;
            result.M42 = -result.M42;
            result.M43 = -result.M43;
        }

        /// <summary>
        /// Creates a left-handed, look-at matrix.
        /// </summary>
        /// <param name="eye">The position of the viewer's eye.</param>
        /// <param name="target">The camera look-at target.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <returns>The created look-at matrix.</returns>
        public static Matrix4x4 CreateLookAtLH(Vector3 eye, Vector3 target, Vector3 up)
        {
            Matrix4x4 result;
            CreateLookAtLH(ref eye, ref target, ref up, out result);
            return result;
        }

        /// <summary>
        /// Creates a right-handed, look-at matrix.
        /// </summary>
        /// <param name="eye">The position of the viewer's eye.</param>
        /// <param name="target">The camera look-at target.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <param name="result">When the method completes, contains the created look-at matrix.</param>
        public static void CreateLookAtRH(ref Vector3 eye, ref Vector3 target, ref Vector3 up, out Matrix4x4 result)
        {
            Vector3 xaxis, yaxis, zaxis;
            zaxis = Vector3.Normalize(Vector3.Subtract(eye, target));
            xaxis = Vector3.Normalize(Vector3.Cross(up, zaxis));
            yaxis = Vector3.Cross(zaxis, xaxis);

            result = Matrix4x4.Identity;
            result.M11 = xaxis.X; result.M21 = xaxis.Y; result.M31 = xaxis.Z;
            result.M12 = yaxis.X; result.M22 = yaxis.Y; result.M32 = yaxis.Z;
            result.M13 = zaxis.X; result.M23 = zaxis.Y; result.M33 = zaxis.Z;

            result.M41 = Vector3.Dot(xaxis, eye);
            result.M42 = Vector3.Dot(yaxis, eye);
            result.M43 = Vector3.Dot(zaxis, eye);

            result.M41 = -result.M41;
            result.M42 = -result.M42;
            result.M43 = -result.M43;
        }

        /// <summary>
        /// Creates a right-handed, look-at matrix.
        /// </summary>
        /// <param name="eye">The position of the viewer's eye.</param>
        /// <param name="target">The camera look-at target.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <returns>The created look-at matrix.</returns>
        public static Matrix4x4 LookAtRH(Vector3 eye, Vector3 target, Vector3 up)
        {
            Matrix4x4 result;
            CreateLookAtRH(ref eye, ref target, ref up, out result);
            return result;
        }

        /// <summary>
        /// Gets the value at the given index of the given matrix.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetValue(Matrix4x4 matrix, int index)
        {
            switch (index)
            {
                case 0: return matrix.M11;
                case 1: return matrix.M12;
                case 2: return matrix.M13;
                case 3: return matrix.M14;
                case 4: return matrix.M21;
                case 5: return matrix.M22;
                case 6: return matrix.M23;
                case 7: return matrix.M24;
                case 8: return matrix.M31;
                case 9: return matrix.M32;
                case 10: return matrix.M33;
                case 11: return matrix.M34;
                case 12: return matrix.M41;
                case 13: return matrix.M42;
                case 14: return matrix.M43;
                case 15: return matrix.M44;
            }

            throw new ArgumentOutOfRangeException("index", "Indices for Matrix run from 0 to 15, inclusive.");
        }

        /// <summary>
        /// Sets the value at the given index of the given matrix.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue(Matrix4x4 matrix, int index, float value)
        {
            switch (index)
            {
                case 0: matrix.M11 = value; break;
                case 1: matrix.M12 = value; break;
                case 2: matrix.M13 = value; break;
                case 3: matrix.M14 = value; break;
                case 4: matrix.M21 = value; break;
                case 5: matrix.M22 = value; break;
                case 6: matrix.M23 = value; break;
                case 7: matrix.M24 = value; break;
                case 8: matrix.M31 = value; break;
                case 9: matrix.M32 = value; break;
                case 10: matrix.M33 = value; break;
                case 11: matrix.M34 = value; break;
                case 12: matrix.M41 = value; break;
                case 13: matrix.M42 = value; break;
                case 14: matrix.M43 = value; break;
                case 15: matrix.M44 = value; break;
                default: throw new ArgumentOutOfRangeException("index", "Indices for Matrix run from 0 to 15, inclusive.");
            }
        }

        /// <summary>
        /// Gets the component at the specified index.
        /// </summary>
        /// <param name="matrix">The matrix from which to get the value.</param>
        /// <param name="row">The row of the matrix to access.</param>
        /// <param name="column">The column of the matrix to access.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="row"/> or <paramref name="column"/>is out of the range [0, 3].</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetValue(Matrix4x4 matrix, int row, int column)
        {
            if (row < 0 || row > 3)
                throw new ArgumentOutOfRangeException("row", "Rows and columns for matrices run from 0 to 3, inclusive.");
            if (column < 0 || column > 3)
                throw new ArgumentOutOfRangeException("column", "Rows and columns for matrices run from 0 to 3, inclusive.");

            return GetValue(matrix,(row * 4) + column);
        }

        /// <summary>
        /// Sets the component at the specified index.
        /// </summary>
        /// <param name="matrix">The matrix on which to set the value.</param>
        /// <param name="row">The row of the matrix to access.</param>
        /// <param name="column">The column of the matrix to access.</param>
        /// <param name="value">The value to be set.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="row"/> or <paramref name="column"/>is out of the range [0, 3].</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue(Matrix4x4 matrix, int row, int column, float value)
        {
            if (row < 0 || row > 3)
                throw new ArgumentOutOfRangeException("row", "Rows and columns for matrices run from 0 to 3, inclusive.");
            if (column < 0 || column > 3)
                throw new ArgumentOutOfRangeException("column", "Rows and columns for matrices run from 0 to 3, inclusive.");

            SetValue(matrix, (row * 4) + column, value);
        }
    }
}
