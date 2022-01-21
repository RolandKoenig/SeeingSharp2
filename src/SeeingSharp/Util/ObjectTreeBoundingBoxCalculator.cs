using System.Numerics;
using SeeingSharp.Mathematics;

namespace SeeingSharp.Util
{
    public class ObjectTreeBoundingBoxCalculator
    {
        private static readonly Vector3 s_vectorMinInitial = new(float.MaxValue, float.MaxValue, float.MaxValue);
        private static readonly Vector3 s_vectorMaxInitial = new(float.MinValue, float.MinValue, float.MinValue);

        private Matrix4Stack _mStack;
        private Vector3 _minCoord;
        private Vector3 _maxCoord;

        public bool CanCreateBoundingBox
        {
            get
            {
                if (_minCoord == s_vectorMinInitial) { return false; }
                if (_maxCoord == s_vectorMaxInitial) { return false; }
                if (Vector3Ex.EqualsWithTolerance(_minCoord, _maxCoord)){ return false; }

                return true;
            }
        }

        public ObjectTreeBoundingBoxCalculator()
        {
            _mStack = new Matrix4Stack();
            _minCoord = s_vectorMinInitial;
            _maxCoord = s_vectorMaxInitial;
        }

        public void PushTransform(ref Matrix4x4 matrix)
        {
            _mStack.Push();
            _mStack.TransformLocal(ref matrix);
        }

        public void PopTransform()
        {
            _mStack.Pop();
        }

        public void AddCoordinate(Vector3 coordinate)
        {
            this.AddCoordinate(ref coordinate);
        }

        public void AddCoordinate(ref Vector3 coordinate)
        {
            ref var topReference = ref _mStack.GetTopByRef();
            var transformedCoord = Vector3.Transform(coordinate, topReference);

            if (_minCoord.X > transformedCoord.X) { _minCoord.X = transformedCoord.X; }
            if (_minCoord.Y > transformedCoord.Y) { _minCoord.Y = transformedCoord.Y; }
            if (_minCoord.Z > transformedCoord.Z) { _minCoord.Z = transformedCoord.Z; }

            if (_maxCoord.X < transformedCoord.X) { _maxCoord.X = transformedCoord.X; }
            if (_maxCoord.Y < transformedCoord.Y) { _maxCoord.Y = transformedCoord.Y; }
            if (_maxCoord.Z < transformedCoord.Z) { _maxCoord.Z = transformedCoord.Z; }
        }

        public BoundingBox CreateBoundingBox()
        {
            if (!this.CanCreateBoundingBox)
            {
                throw new SeeingSharpException($"Unable to create {nameof(BoundingBox)}: Not enough coordinates given!");
            }

            return new BoundingBox(_minCoord, _maxCoord);
        }
    }
}
