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
using System.Numerics;

namespace SeeingSharp
{
    public class Matrix4Stack
    {
        private Stack<Matrix4x4> m_stack;
        private int m_pushTimes;

        private Matrix4x4 m_top;

        /// <summary>
        /// Cretaes a new matrix stack using 4x4 matrices
        /// </summary>
        public Matrix4Stack()
        {
            m_stack = new Stack<Matrix4x4>();
            m_top = Matrix4x4.Identity;

            m_pushTimes = 0;
        }

        /// <summary>
        /// Creates a new matrix stack usin 4x4 matrices
        /// </summary>
        public Matrix4Stack(Matrix4x4 top)
            : this()
        {
            m_top = top;
        }

        /// <summary>
        /// Resets this object to single identity matrix.
        /// </summary>
        public void ResetStackToIdentity()
        {
            m_stack.Clear();
            m_pushTimes = 0;
            m_top = Matrix4x4.Identity;
        }

        /// <summary>
        /// Performs translation on top matrix
        /// </summary>
        public void TranslateLocal(float transX, float transY, float transZ)
        {
            m_top = Matrix4x4.CreateTranslation(transX, transY, transZ) * m_top;
        }

        /// <summary>
        /// Performs translation on top matrix
        /// </summary>
        public void TranslateLocal(Vector3 transVector)
        {
            m_top = Matrix4x4.CreateTranslation(transVector) * m_top;
        }

        /// <summary>
        /// Performs a rotation on top matrix.
        /// </summary>
        /// <param name="yaw">Yaw around y-axis.</param>
        /// <param name="pitch">Pitch around x-axis.</param>
        /// <param name="roll">Roll around z-axis.</param>
        public void RotateYawPitchRollLocal(float yaw, float pitch, float roll)
        {
            m_top = Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll) * m_top;
        }

        /// <summary>
        /// Performs a rotation on top matrix using horizontal and vertical rotation angles.
        /// </summary>
        /// <param name="hRotation">The horizontal rotation angle.</param>
        /// <param name="vRotation">The vertical rotation angle.</param>
        public void RotateHVLocal(float hRotation, float vRotation)
        {
            m_top = Matrix4x4.CreateFromYawPitchRoll(vRotation, hRotation, 0f) * m_top; //Matrix.RotationHV(hRotation, vRotation) * m_top;
        }

        /// <summary>
        /// Performs a rotation on top matrix using horizontal and vertical rotation angles.
        /// </summary>
        /// <param name="rotation">Vector containing horizontal and vertical rotations.</param>
        public void RotateHVLocal(Vector2 rotation)
        {
            m_top = Matrix4x4.CreateFromYawPitchRoll(rotation.X, rotation.Y, 0f) * m_top;
        }

        /// <summary>
        /// Performs scaling on top matrix
        /// </summary>
        public void ScaleLocal(float scaleX, float scaleY, float scaleZ)
        {
            m_top = Matrix4x4.CreateScale(scaleX, scaleY, scaleZ) * m_top;
        }

        /// <summary>
        /// Performs scaling on top matrix
        /// </summary>
        public void ScaleLocal(Vector3 scaling)
        {
            m_top = Matrix4x4.CreateScale(scaling) * m_top;
        }

        /// <summary>
        /// Performs scaling on top matrix
        /// </summary>
        public void ScaleLocal(float scaleFactor)
        {
            m_top = Matrix4x4.CreateScale(scaleFactor, scaleFactor, scaleFactor) * m_top;
        }

        /// <summary>
        /// performs rotation around x-axis on top matrix
        /// </summary>
        public void RotateXLocal(float angle)
        {
            m_top = Matrix4x4.CreateRotationX(angle) * m_top;
        }

        /// <summary>
        /// performs rotation around y-axis on top matrix
        /// </summary>
        public void RotateYLocal(float angle)
        {
            m_top = Matrix4x4.CreateRotationY(angle) * m_top;
        }

        /// <summary>
        /// performs rotation around z-axis on top matrix
        /// </summary>
        public void RotateZLocal(float angle)
        {
            m_top = Matrix4x4.CreateRotationZ(angle) * m_top;
        }

        /// <summary>
        /// Performs a local transformation with the given matrix.
        /// </summary>
        public void TransformLocal(Matrix4x4 transformMatrix)
        {
            m_top = transformMatrix * m_top;
        }

        ///// <summary>
        ///// Performs a local transformation with the given matrix.
        ///// </summary>
        ///// <param name="matrix3">The matrix to transform this one with.</param>
        //public void TransformLocal(Matrix3 matrix3)
        //{
        //    m_top = new Matrix4(matrix3) * m_top;
        //}

        /// <summary>
        /// Clones the object
        /// </summary>
        public Object Clone()
        {
            Matrix4Stack cloned = new Matrix4Stack();

            Matrix4x4[] allElements = m_stack.ToArray();

            cloned.m_stack = new Stack<Matrix4x4>();
            for (int loop = 0; loop < allElements.Length; loop++)
            {
                cloned.m_stack.Push(allElements[loop]);
                cloned.m_pushTimes++;
            }
            cloned.m_top = m_top;

            return cloned;
        }

        /// <summary>
        /// Inserts a new matrix on top of the collection
        /// </summary>
        public void Push()
        {
            m_stack.Push(m_top);
            m_pushTimes++;
        }

        /// <summary>
        /// Inserts a new matrix on top of the collection
        /// </summary>
        public void Push(Matrix4x4 matrixToPush)
        {
            m_stack.Push(m_top);
            m_top = matrixToPush;
            m_pushTimes++;
        }

        /// <summary>
        /// Removes the lastly created matrix
        /// </summary>
        public void Pop()
        {
            if (m_pushTimes > 0)
            {
                m_top = m_stack.Pop();
                m_pushTimes--;
            }
        }

        /// <summary>
        /// Gets the top matrix
        /// </summary>
        public Matrix4x4 Top
        {
            get { return m_top; }
        }
    }
}
