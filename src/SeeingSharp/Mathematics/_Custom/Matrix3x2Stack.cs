using System.Collections.Generic;
using System.Numerics;

namespace SeeingSharp.Mathematics
{
    public class Matrix3x2Stack
    {
        private Stack<Matrix3x2> _stack;
        private int _pushTimes;
        private Matrix3x2 _top;

        /// <summary>
        /// Gets the top matrix
        /// </summary>
        public Matrix3x2 Top => _top;

        /// <summary>
        /// Creates a new matrix stack using 3x2 matrices
        /// </summary>
        public Matrix3x2Stack()
        {
            _stack = new Stack<Matrix3x2>();
            _top = Matrix3x2.Identity;

            _pushTimes = 0;
        }

        /// <summary>
        /// Creates a new matrix stack usin 3x2 matrices
        /// </summary>
        public Matrix3x2Stack(Matrix3x2 top)
            : this()
        {
            _top = top;
        }

        /// <summary>
        /// Resets this object to single identity matrix.
        /// </summary>
        public void ResetStackToIdentity()
        {
            _stack.Clear();
            _pushTimes = 0;
            _top = Matrix3x2.Identity;
        }

        /// <summary>
        /// Performs translation on top matrix
        /// </summary>
        public void TranslateLocal(float transX, float transY)
        {
            _top = Matrix3x2.CreateTranslation(transX, transY) * _top;
        }

        /// <summary>
        /// Performs translation on top matrix
        /// </summary>
        public void TranslateLocal(Vector2 transVector)
        {
            _top = Matrix3x2.CreateTranslation(transVector) * _top;
        }

        /// <summary>
        /// Performs a rotation on top matrix.
        /// </summary>
        /// <param name="radians">The angle in radians.</param>
        public void RotateYawPitchRollLocal(float radians)
        {
            _top = Matrix3x2.CreateRotation(radians) * _top;
        }

        /// <summary>
        /// Performs a rotation on top matrix.
        /// </summary>
        /// <param name="radians">The angle in radians.</param>
        /// <param name="centerPoint">The center point of the rotation.</param>
        public void RotateYawPitchRollLocal(float radians, Vector2 centerPoint)
        {
            _top = Matrix3x2.CreateRotation(radians, centerPoint) * _top;
        }

        /// <summary>
        /// Performs scaling on top matrix
        /// </summary>
        public void ScaleLocal(float scaleX, float scaleY)
        {
            _top = Matrix3x2.CreateScale(scaleX, scaleY) * _top;
        }

        /// <summary>
        /// Performs scaling on top matrix
        /// </summary>
        public void ScaleLocal(Vector2 scaling)
        {
            _top = Matrix3x2.CreateScale(scaling) * _top;
        }

        /// <summary>
        /// Performs scaling on top matrix
        /// </summary>
        public void ScaleLocal(float scaleFactor)
        {
            _top = Matrix3x2.CreateScale(scaleFactor, scaleFactor) * _top;
        }

        /// <summary>
        /// Performs a local transformation with the given matrix.
        /// </summary>
        public void TransformLocal(Matrix3x2 transformMatrix)
        {
            _top = transformMatrix * _top;
        }

        /// <summary>
        /// Clones the object
        /// </summary>
        public object Clone()
        {
            var cloned = new Matrix3x2Stack();

            var allElements = _stack.ToArray();

            cloned._stack = new Stack<Matrix3x2>();
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
        public void Push(Matrix3x2 matrixToPush)
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
