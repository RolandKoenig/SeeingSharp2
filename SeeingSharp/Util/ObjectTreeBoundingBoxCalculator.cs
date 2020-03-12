/*
    Seeing# and all applications distributed together with it. 
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
using System.Numerics;

namespace SeeingSharp.Util
{
    public class ObjectTreeBoundingBoxCalculator
    {
        private static readonly Vector3 s_vectorMinInitial = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        private static readonly Vector3 s_vectorMaxInitial = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        private Matrix4Stack _mStack;
        private Vector3 _minCoord;
        private Vector3 _maxCoord;

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
    }
}
