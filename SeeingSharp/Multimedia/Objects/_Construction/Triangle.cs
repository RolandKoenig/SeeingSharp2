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

namespace SeeingSharp.Multimedia.Objects
{
    /// <summary>
    /// A Treangle inside a VertexStructure object
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
        /// <param name="sourceStructure">The source structure.</param>
        public Line[] GetEdges(VertexStructure sourceStructure)
        {
            return new Line[]
            {
                new Line(
                    sourceStructure.Vertices[this.Index1].Position,
                    sourceStructure.Vertices[this.Index2].Position),
                new Line(
                    sourceStructure.Vertices[this.Index2].Position,
                    sourceStructure.Vertices[this.Index3].Position),
                new Line(
                    sourceStructure.Vertices[this.Index3].Position,
                    sourceStructure.Vertices[this.Index1].Position)
            };
        }
    }
}