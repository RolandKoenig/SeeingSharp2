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
        private static readonly Vector3 VECTOR_MIN_INITIAL = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        private static readonly Vector3 VECTOR_MAX_INITIAL = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        private Matrix4Stack m_mStack;
        private Vector3 m_minCoord;
        private Vector3 m_maxCoord;

        public ObjectTreeBoundingBoxCalculator()
        {
            m_mStack = new Matrix4Stack();
            m_minCoord = VECTOR_MIN_INITIAL;
            m_maxCoord = VECTOR_MAX_INITIAL;
        }

        public void PushTransform(ref Matrix4x4 matrix)
        {
            m_mStack.Push();
            m_mStack.TransformLocal(ref matrix);
        }

        public void PopTransform()
        {
            m_mStack.Pop();
        }

        public void AddCoordinate(Vector3 coordinate)
        {
            this.AddCoordinate(ref coordinate);
        }

        public void AddCoordinate(ref Vector3 coordinate)
        {
            ref var topReference = ref m_mStack.GetTopByRef();
            var transformedCoord = Vector3.Transform(coordinate, topReference);

            if (m_minCoord.X > transformedCoord.X) { m_minCoord.X = transformedCoord.X; }
            if (m_minCoord.Y > transformedCoord.Y) { m_minCoord.Y = transformedCoord.Y; }
            if (m_minCoord.Z > transformedCoord.Z) { m_minCoord.Z = transformedCoord.Z; }

            if (m_maxCoord.X < transformedCoord.X) { m_maxCoord.X = transformedCoord.X; }
            if (m_maxCoord.Y < transformedCoord.Y) { m_maxCoord.Y = transformedCoord.Y; }
            if (m_maxCoord.Z < transformedCoord.Z) { m_maxCoord.Z = transformedCoord.Z; }
        }

        public BoundingBox CreateBoundingBox()
        {
            if (!this.CanCreateBoundingBox)
            {
                throw new SeeingSharpException($"Unable to create {nameof(BoundingBox)}: Not enough coordinates given!");
            }

            return new BoundingBox(m_minCoord, m_maxCoord);
        }

        public bool CanCreateBoundingBox
        {
            get
            {
                if (m_minCoord == VECTOR_MIN_INITIAL) { return false; }
                if (m_maxCoord == VECTOR_MAX_INITIAL) { return false; }
                if (Vector3Ex.EqualsWithTolerance(m_minCoord, m_maxCoord)){ return false; }

                return true;
            }
        }
    }
}
