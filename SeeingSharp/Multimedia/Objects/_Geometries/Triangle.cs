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
namespace SeeingSharp.Multimedia.Objects
{
    /// <summary>
    /// A Triangle inside a Geometry object
    /// </summary>
    public struct Triangle
    {
        public int Index1;
        public int Index2;
        public int Index3;

        /// <summary>
        /// Creates a new triangle
        /// </summary>
        /// <param name="index1">Index of the first vertex</param>
        /// <param name="index2">Index of the second vertex</param>
        /// <param name="index3">Index of the third vertex</param>
        public Triangle(int index1, int index2, int index3)
        {
            Index1 = index1;
            Index2 = index2;
            Index3 = index3;
        }

        /// <summary>
        /// Gets all edges defined by this triangle.
        /// </summary>
        /// <param name="sourceGeometry">The source geometry.</param>
        public Line[] GetEdges(Geometry sourceGeometry)
        {
            return new[]
            {
                new Line(
                    sourceGeometry.Vertices[Index1].Position,
                    sourceGeometry.Vertices[Index2].Position),
                new Line(
                    sourceGeometry.Vertices[Index2].Position,
                    sourceGeometry.Vertices[Index3].Position),
                new Line(
                    sourceGeometry.Vertices[Index3].Position,
                    sourceGeometry.Vertices[Index1].Position)
            };
        }
    }
}