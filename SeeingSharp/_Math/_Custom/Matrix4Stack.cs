/*
    SeeingSharp and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
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
using System.Collections.Generic;
using System.Numerics;

namespace SeeingSharp
{
    public class Matrix4Stack
    {
        private Stack<Matrix4x4> _stack;
        private int _pushTimes;

        private Matrix4x4 _top;

        /// <summary>
        /// Gets the top matrix
        /// </summary>
        public Matrix4x4 Top => _top;

        /// <summary>
        /// Creates a new matrix stack using 4x4 matrices
        /// </summary>
        public Matrix4Stack()
        {
            _stack = new Stack<Matrix4x4>();
            _top = Matrix4x4.Identity;

            _pushTimes = 0;
        }

        /// <summary>
        /// Creates a new matrix stack using 4x4 matrices
        /// </summary>
        public Matrix4Stack(Matrix4x4 top)
            : this()
        {
            _top = top;
        }

        /// <summary>
        /// Gets the top matrix by reference.
        /// </summary>
        public ref Matrix4x4 GetTopByRef()
        {
            return ref _top;
        }

        /// <summary>
        /// Resets this object to single identity matrix.
        /// </summary>
        public void ResetStackToIdentity()
        {
            _stack.Clear();
            _pushTimes = 0;
            _top = Matrix4x4.Identity;
        }

        /// <summary>
        /// Performs translation on top matrix
        /// </summary>
        public void TranslateLocal(float transX, float transY, float transZ)
        {
            _top = Matrix4x4.CreateTranslation(transX, transY, transZ) * _top;
        }

        /// <summary>
        /// Performs translation on top matrix
        /// </summary>
        public void TranslateLocal(Vector3 transVector)
        {
            _top = Matrix4x4.CreateTranslation(transVector) * _top;
        }

        /// <summary>
        /// Performs translation on top matrix
        /// </summary>
        public void TranslateLocal(ref Vector3 transVector)
        {
            _top = Matrix4x4.CreateTranslation(transVector) * _top;
        }

        /// <summary>
        /// Performs a rotation on top matrix.
        /// </summary>
        /// <param name="yaw">Yaw around y-axis.</param>
        /// <param name="pitch">Pitch around x-axis.</param>
        /// <param name="roll">Roll around z-axis.</param>
        public void RotateYawPitchRollLocal(float yaw, float pitch, float roll)
        {
            _top = Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll) * _top;
        }

        /// <summary>
        /// Performs a rotation on top matrix using horizontal and vertical rotation angles.
        /// </summary>
        /// <param name="hRotation">The horizontal rotation angle.</param>
        /// <param name="vRotation">The vertical rotation angle.</param>
        public void RotateHVLocal(float hRotation, float vRotation)
        {
            _top = Matrix4x4.CreateFromYawPitchRoll(vRotation, hRotation, 0f) * _top; //Matrix.RotationHV(hRotation, vRotation) * _top;
        }

        /// <summary>
        /// Performs a rotation on top matrix using horizontal and vertical rotation angles.
        /// </summary>
        /// <param name="rotation">Vector containing horizontal and vertical rotations.</param>
        public void RotateHVLocal(Vector2 rotation)
        {
            _top = Matrix4x4.CreateFromYawPitchRoll(rotation.X, rotation.Y, 0f) * _top;
        }

        /// <summary>
        /// Performs a rotation on top matrix using horizontal and vertical rotation angles.
        /// </summary>
        /// <param name="rotation">Vector containing horizontal and vertical rotations.</param>
        public void RotateHVLocal(ref Vector2 rotation)
        {
            _top = Matrix4x4.CreateFromYawPitchRoll(rotation.X, rotation.Y, 0f) * _top;
        }

        /// <summary>
        /// Performs scaling on top matrix
        /// </summary>
        public void ScaleLocal(float scaleX, float scaleY, float scaleZ)
        {
            _top = Matrix4x4.CreateScale(scaleX, scaleY, scaleZ) * _top;
        }

        /// <summary>
        /// Performs scaling on top matrix
        /// </summary>
        public void ScaleLocal(Vector3 scaling)
        {
            _top = Matrix4x4.CreateScale(scaling) * _top;
        }

        /// <summary>
        /// Performs scaling on top matrix
        /// </summary>
        public void ScaleLocal(ref Vector3 scaling)
        {
            _top = Matrix4x4.CreateScale(scaling) * _top;
        }

        /// <summary>
        /// Performs scaling on top matrix
        /// </summary>
        public void ScaleLocal(float scaleFactor)
        {
            _top = Matrix4x4.CreateScale(scaleFactor, scaleFactor, scaleFactor) * _top;
        }

        /// <summary>
        /// performs rotation around x-axis on top matrix
        /// </summary>
        public void RotateXLocal(float angle)
        {
            _top = Matrix4x4.CreateRotationX(angle) * _top;
        }

        /// <summary>
        /// performs rotation around y-axis on top matrix
        /// </summary>
        public void RotateYLocal(float angle)
        {
            _top = Matrix4x4.CreateRotationY(angle) * _top;
        }

        /// <summary>
        /// performs rotation around z-axis on top matrix
        /// </summary>
        public void RotateZLocal(float angle)
        {
            _top = Matrix4x4.CreateRotationZ(angle) * _top;
        }

        /// <summary>
        /// Performs a local transformation with the given matrix.
        /// </summary>
        public void TransformLocal(Matrix4x4 transformMatrix)
        {
            _top = transformMatrix * _top;
        }

        /// <summary>
        /// Performs a local transformation with the given matrix.
        /// </summary>
        public void TransformLocal(ref Matrix4x4 transformMatrix)
        {
            _top = transformMatrix * _top;
        }

        /// <summary>
        /// Clones the object
        /// </summary>
        public object Clone()
        {
            var cloned = new Matrix4Stack();

            var allElements = _stack.ToArray();

            cloned._stack = new Stack<Matrix4x4>();
            for (var loop = 0; loop < allElements.Length; loop++)
            {
                cloned._stack.Push(allElements[loop]);
                cloned._pushTimes++;
            }
            cloned._top = _top;

            return cloned;
        }

        /// <summary>
        /// Inserts a new matrix on top of the collection
        /// </summary>
        public void Push()
        {
            _stack.Push(_top);
            _pushTimes++;
        }

        /// <summary>
        /// Inserts a new matrix on top of the collection
        /// </summary>
        public void Push(Matrix4x4 matrixToPush)
        {
            _stack.Push(_top);
            _top = matrixToPush;
            _pushTimes++;
        }

        /// <summary>
        /// Inserts a new matrix on top of the collection
        /// </summary>
        public void Push(ref Matrix4x4 matrixToPush)
        {
            _stack.Push(_top);
            _top = matrixToPush;
            _pushTimes++;
        }

        /// <summary>
        /// Removes the lastly created matrix
        /// </summary>
        public void Pop()
        {
            if (_pushTimes > 0)
            {
                _top = _stack.Pop();
                _pushTimes--;
            }
        }
    }
}
