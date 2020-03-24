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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;

namespace SeeingSharp
{
    public class Polygon
    {
        private Vector3[] _vertices;
        private Lazy<Vector3> _normal;

        /// <summary>
        /// Gets a collection containing all vertices.
        /// </summary>
        public ReadOnlyCollection<Vector3> Vertices { get; }

        /// <summary>
        /// Gets the normal of this polygon.
        /// </summary>
        public Vector3 Normal => _normal.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon" /> class.
        /// </summary>
        public Polygon(params Vector3[] vertices)
        {
            if (vertices.Length < 3) { throw new SeeingSharpException("A polygon must at least have 4 vertices!"); }

            _vertices = vertices;
            this.Vertices = new ReadOnlyCollection<Vector3>(_vertices);

            //Define normal calculation method
            _normal = new Lazy<Vector3>(() => Vector3Ex.CalculateTriangleNormal(_vertices[0], _vertices[1], _vertices[2]));
        }

        /// <summary>
        /// Flatterns this polygon.
        /// </summary>
        public Polygon2D Flattern()
        {
            //Inspired by implementation of the Helix Toolkit from codeplex (http://helixtoolkit.codeplex.com/)
            //Original sources:
            // http://forums.xna.com/forums/p/16529/86802.aspx
            // http://stackoverflow.com/questions/1023948/rotate-normal-vector-onto-axis-plane

            //Calculate transform matrix
            var upVector = _normal.Value;
            var right = Vector3.Cross(
                upVector, Math.Abs(upVector.X) > Math.Abs(upVector.Z) ? new Vector3(0, 0, 1) : new Vector3(1, 0, 0));
            var backward = Vector3.Cross(right, upVector);
            var m = new Matrix4x4(
                backward.X, right.X, upVector.X, 0, backward.Y, right.Y, upVector.Y, 0, backward.Z, right.Z, upVector.Z, 0, 0, 0, 0, 1);

            //Make first point origin
            var offs = Vector3.Transform(_vertices[0], m);
            m.M41 = -offs.X;
            m.M42 = -offs.Y;

            //Calculate 2D surface
            var resultVertices = new Vector2[_vertices.Length];
            for (var loopVertex = 0; loopVertex < _vertices.Length; loopVertex++)
            {
                var pp = Vector3.Transform(_vertices[loopVertex], m);
                resultVertices[loopVertex] = new Vector2(pp.X, pp.Y);
            }

            return new Polygon2D(resultVertices);
        }

        /// <summary>
        /// Triangulates this surface using the cutting ears algorithm.
        /// </summary>
        public IEnumerable<int> TriangulateUsingCuttingEars()
        {
            var surface2D = this.Flattern();
            return surface2D.TriangulateUsingCuttingEars();
        }
    }
}
