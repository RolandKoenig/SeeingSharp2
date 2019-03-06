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

    public class Matrix3x2Stack
    {
        #region Stack data
        private Stack<Matrix3x2> m_stack;
        private int m_pushTimes;

        #endregion

        /// <summary>
        /// Cretaes a new matrix stack using 3x2 matrices
        /// </summary>
        public Matrix3x2Stack()
        {
            m_stack = new Stack<Matrix3x2>();
            Top = Matrix3x2.Identity;

            m_pushTimes = 0;
        }

        /// <summary>
        /// Creates a new matrix stack usin 3x2 matrices
        /// </summary>
        public Matrix3x2Stack(Matrix3x2 top)
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
            Top = Matrix3x2.Identity;
        }

        /// <summary>
        /// Performs translation on top matrix
        /// </summary>
        public void TranslateLocal(float transX, float transY)
        {
            Top = Matrix3x2.Translation(transX, transY) * Top;
        }

        /// <summary>
        /// Performs translation on top matrix
        /// </summary>
        public void TranslateLocal(Vector2 transVector)
        {
            Top = Matrix3x2.Translation(transVector) * Top;
        }

        /// <summary>
        /// Performs a rotation on top matrix.
        /// </summary>
        /// <param name="radians">The angle in radians.</param>
        public void RotateYawPitchRollLocal(float radians)
        {
            Top = Matrix3x2.Rotation(radians) * Top;
        }

        /// <summary>
        /// Performs a rotation on top matrix.
        /// </summary>
        /// <param name="radians">The angle in radians.</param>
        /// <param name="centerPoint">The center point of the rotation.</param>
        public void RotateYawPitchRollLocal(float radians, Vector2 centerPoint)
        {
            Top = Matrix3x2.Rotation(radians, centerPoint) * Top;
        }

        /// <summary>
        /// Performs scaling on top matrix
        /// </summary>
        public void ScaleLocal(float scaleX, float scaleY)
        {
            Top = Matrix3x2.Scaling(scaleX, scaleY) * Top;
        }

        /// <summary>
        /// Performs scaling on top matrix
        /// </summary>
        public void ScaleLocal(Vector2 scaling)
        {
            Top = Matrix3x2.Scaling(scaling) * Top;
        }

        /// <summary>
        /// Performs scaling on top matrix
        /// </summary>
        public void ScaleLocal(float scaleFactor)
        {
            Top = Matrix3x2.Scaling(scaleFactor, scaleFactor) * Top;
        }

        /// <summary>
        /// Performs a local transformation with the given matrix.
        /// </summary>
        public void TransformLocal(Matrix3x2 transformMatrix)
        {
            Top = transformMatrix * Top;
        }

        /// <summary>
        /// Clones the object
        /// </summary>
        public Object Clone()
        {
            var cloned = new Matrix3x2Stack();
            var allElements = m_stack.ToArray();

            cloned.m_stack = new Stack<Matrix3x2>();

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
        public void Push(Matrix3x2 matrixToPush)
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
        public Matrix3x2 Top { get; private set; }
    }
}