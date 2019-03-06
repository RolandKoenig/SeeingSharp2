#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
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
    using SharpDX;

    #endregion

    public class Matrix4Stack
    {
        private Stack<Matrix> m_stack;
        private int m_pushTimes;

        /// <summary>
        /// Cretaes a new matrix stack using 4x4 matrices
        /// </summary>
        public Matrix4Stack()
        {
            m_stack = new Stack<Matrix>();
            Top = Matrix.Identity;

            m_pushTimes = 0;
        }

        /// <summary>
        /// Creates a new matrix stack usin 4x4 matrices
        /// </summary>
        public Matrix4Stack(Matrix top)
            : this()
        {
            Top = top;
        }

        /// <summary>
        /// Resets this object to single identity matrix.
        /// </summary>
        public void ResetStackToIdentity()
        {
            m_stack.Clear();
            m_pushTimes = 0;
            Top = Matrix.Identity;
        }

        /// <summary>
        /// Performs translation on top matrix
        /// </summary>
        public void TranslateLocal(float transX, float transY, float transZ)
        {
            Top = Matrix.Translation(transX, transY, transZ) * Top;
        }

        /// <summary>
        /// Performs translation on top matrix
        /// </summary>
        public void TranslateLocal(Vector3 transVector)
        {
            Top = Matrix.Translation(transVector) * Top;
        }

        /// <summary>
        /// Performs a rotation on top matrix.
        /// </summary>
        /// <param name="yaw">Yaw around y-axis.</param>
        /// <param name="pitch">Pitch around x-axis.</param>
        /// <param name="roll">Roll around z-axis.</param>
        public void RotateYawPitchRollLocal(float yaw, float pitch, float roll)
        {
            Top = Matrix.RotationYawPitchRoll(yaw, pitch, roll) * Top;
        }

        /// <summary>
        /// Performs a rotation on top matrix using horizontal and vertical rotation angles.
        /// </summary>
        /// <param name="hRotation">The horizontal rotation angle.</param>
        /// <param name="vRotation">The vertical rotation angle.</param>
        public void RotateHVLocal(float hRotation, float vRotation)
        {
            Top = Matrix.RotationYawPitchRoll(vRotation, hRotation, 0f) * Top; // Matrix.RotationHV(hRotation, vRotation) * m_top;
        }

        /// <summary>
        /// Performs a rotation on top matrix using horizontal and vertical rotation angles.
        /// </summary>
        /// <param name="rotation">Vector containing horizontal and vertical rotations.</param>
        public void RotateHVLocal(Vector2 rotation)
        {
            Top = Matrix.RotationYawPitchRoll(rotation.X, rotation.Y, 0f) * Top;
        }

        /// <summary>
        /// Performs scaling on top matrix
        /// </summary>
        public void ScaleLocal(float scaleX, float scaleY, float scaleZ)
        {
            Top = Matrix.Scaling(scaleX, scaleY, scaleZ) * Top;
        }

        /// <summary>
        /// Performs scaling on top matrix
        /// </summary>
        public void ScaleLocal(Vector3 scaling)
        {
            Top = Matrix.Scaling(scaling) * Top;
        }

        /// <summary>
        /// Performs scaling on top matrix
        /// </summary>
        public void ScaleLocal(float scaleFactor)
        {
            Top = Matrix.Scaling(scaleFactor, scaleFactor, scaleFactor) * Top;
        }

        /// <summary>
        /// performs rotation around x-axis on top matrix
        /// </summary>
        public void RotateXLocal(float angle)
        {
            Top = Matrix.RotationX(angle) * Top;
        }

        /// <summary>
        /// performs rotation around y-axis on top matrix
        /// </summary>
        public void RotateYLocal(float angle)
        {
            Top = Matrix.RotationY(angle) * Top;
        }

        /// <summary>
        /// performs rotation around z-axis on top matrix
        /// </summary>
        public void RotateZLocal(float angle)
        {
            Top = Matrix.RotationZ(angle) * Top;
        }

        /// <summary>
        /// Performs a local transformation with the given matrix.
        /// </summary>
        public void TransformLocal(Matrix transformMatrix)
        {
            Top = transformMatrix * Top;
        }

        /// <summary>
        /// Clones the object
        /// </summary>
        public Object Clone()
        {
            var cloned = new Matrix4Stack();
            var allElements = m_stack.ToArray();
            cloned.m_stack = new Stack<Matrix>();

            for (var loop = 0; loop < allElements.Length; loop++)
            {
                cloned.m_stack.Push(allElements[loop]);
                cloned.m_pushTimes++;
            }

            cloned.Top = Top;

            return cloned;
        }

        /// <summary>
        /// Inserts a new matrix on top of the collection
        /// </summary>
        public void Push()
        {
            m_stack.Push(Top);
            m_pushTimes++;
        }

        /// <summary>
        /// Inserts a new matrix on top of the collection
        /// </summary>
        public void Push(Matrix matrixToPush)
        {
            m_stack.Push(Top);
            Top = matrixToPush;
            m_pushTimes++;
        }

        /// <summary>
        /// Removes the lastly created matrix
        /// </summary>
        public void Pop()
        {
            if (m_pushTimes <= 0)
            {
                return;
            }

            Top = m_stack.Pop();
            m_pushTimes--;
        }

        /// <summary>
        /// Gets the top matrix
        /// </summary>
        public Matrix Top { get; private set; }
    }
}