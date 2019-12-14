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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SeeingSharp
{
    public class Polygon2D
    {
        private Vector2[] m_vertices;
        private ReadOnlyCollection<Vector2> m_verticesPublic;
        private Lazy<BoundingBox2D> m_boundingBox2D;
        private Lazy<EdgeOrder> m_edgeOrder;

        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon2D" /> class.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <exception cref="SeeingSharpException"></exception>
        public Polygon2D(params Vector2[] vertices)
        {
            if (vertices.Length < 3) { throw new SeeingSharpException("A plygon must at least have 4 vertices!"); }

            //Apply given vertices (remove the last one if it is equal to the first one)
            m_vertices = vertices;
            if ((m_vertices.Length > 1) &&
               (m_vertices[m_vertices.Length - 1] == m_vertices[0]))
            {
                Vector2[] newArray = new Vector2[m_vertices.Length - 1];
                Array.Copy(m_vertices, newArray, m_vertices.Length - 1);
                m_vertices = newArray;
            }

            m_verticesPublic = new ReadOnlyCollection<Vector2>(m_vertices);
            m_boundingBox2D = new Lazy<BoundingBox2D>(CalculateBoundingBox);
            m_edgeOrder = new Lazy<EdgeOrder>(CalculateEdgeOrder);
        }

        /// <summary>
        /// Clones this polygon.
        /// </summary>
        public Polygon2D Clone()
        {
            return new Polygon2D(m_vertices.Clone() as Vector2[]);
        }

        /// <summary>
        /// Merges this polygon with the given one defining a hole. The result is a new polygon.
        /// </summary>
        public Polygon2D MergeWithHole(Polygon2D actHole, Polygon2DMergeOptions mergeOptions)
        {
            return MergeWithHole(actHole, mergeOptions, null);
        }

        /// <summary>
        /// Merges the with hole.
        /// </summary>
         public Polygon2D MergeWithHole(Polygon2D actHole, Polygon2DMergeOptions mergeOptions, List<Vector2> cutPoints)
        {
            //This algorithm uses the method described in http://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf

            //Find the hole vertex with the highest x value
            Vector2 holeVertexWithHighestX = new Vector2(float.MinValue, 0f);
            int holeVertexIndexWithHighestX = -1;
            for (int loopVertex = 0; loopVertex < actHole.m_vertices.Length; loopVertex++)
            {
                if (actHole.m_vertices[loopVertex].X > holeVertexWithHighestX.X)
                {
                    holeVertexWithHighestX = actHole.m_vertices[loopVertex];
                    holeVertexIndexWithHighestX = loopVertex;
                }
            }
            if (cutPoints != null) { cutPoints.Add(holeVertexWithHighestX); }

            //Define a ray from the found vertex pointing in x direction
            Ray2D ray2D = new Ray2D(holeVertexWithHighestX, new Vector2(1f, 0f));

            //Find the line on current filling polygon with intersects first with the created ray
            Tuple<int, float, Vector2> foundLine = null;
            int actLineIndex = 0;
            foreach (Line2D actLine in this.Lines)
            {
                var actIntersection = actLine.Intersect(ray2D);
                if (actIntersection.Item1)
                {
                    Ray2D rayToIntersectionPoint = new Ray2D(
                        ray2D.Origin,
                        Vector2.Normalize(actIntersection.Item2 - ray2D.Origin));
                    float lengthToIntersectionPoint = Vector2.Distance(actIntersection.Item2, ray2D.Origin);
                    if ((lengthToIntersectionPoint > 0f) &&
                        (rayToIntersectionPoint.EqualsWithTolerance(ray2D)))
                    {
                        if (foundLine == null)
                        {
                            //First found intersection
                            foundLine = Tuple.Create(actLineIndex, lengthToIntersectionPoint, actIntersection.Item2);
                        }
                        else if (lengthToIntersectionPoint < foundLine.Item2)
                        {
                            //More intersections found.. take the one with smalles distance to intersection point
                            foundLine = Tuple.Create(actLineIndex, lengthToIntersectionPoint, actIntersection.Item2);
                        }
                    }
                }
                actLineIndex++;
            }
            if (cutPoints != null) { cutPoints.Add(foundLine.Item3); }

            //Check for found intersection
            if (foundLine == null)
            {
                throw new SeeingSharpException("No point found on which given polygons can be combinded!");
            }

            //Now generate result polygon 
            List<Vector2> resultBuilder = new List<Vector2>(this.m_vertices.Length + actHole.m_vertices.Length + 2);
            for (int loopFillVertex = 0; loopFillVertex < this.m_vertices.Length; loopFillVertex++)
            {
                //Add current vertex from filling polygon first
                resultBuilder.Add(m_vertices[loopFillVertex]);

                //Do special logic on cut point
                if (loopFillVertex == foundLine.Item1)
                {
                    //Cut point.. place here the hole polygon
                    if (!m_vertices[loopFillVertex].Equals(foundLine.Item3))
                    {
                        resultBuilder.Add(foundLine.Item3);
                    }

                    //Add all vertices from the hole polygon
                    resultBuilder.Add(actHole.m_vertices[holeVertexIndexWithHighestX]);
                    int loopHoleVertex = holeVertexIndexWithHighestX + 1;
                    while (loopHoleVertex != holeVertexIndexWithHighestX)
                    {
                        if (loopHoleVertex >= actHole.m_vertices.Length) { loopHoleVertex = 0; }

                        resultBuilder.Add(actHole.m_vertices[loopHoleVertex]);

                        loopHoleVertex++;
                        if (loopHoleVertex >= actHole.m_vertices.Length) { loopHoleVertex = 0; }
                    }

                    //Add cutpoints again to continue with main polygon
                    if (mergeOptions.MakeMergepointSpaceForTriangulation)
                    {
                        resultBuilder.Add(actHole.m_vertices[holeVertexIndexWithHighestX] + new Vector2(0f, 0.001f));

                        //Add the cutpoint again
                        resultBuilder.Add(foundLine.Item3 + new Vector2(0f, 0.001f));
                    }
                    else
                    {
                        resultBuilder.Add(actHole.m_vertices[holeVertexIndexWithHighestX]);

                        //Add the cutpoint again
                        resultBuilder.Add(foundLine.Item3);
                    }

                    //Handle the case in which next vertex would equal current cut point
                    if ((m_vertices.Length > loopFillVertex + 1) &&
                        (m_vertices[loopFillVertex + 1].Equals(foundLine.Item3)))
                    {
                        loopFillVertex++;
                    }
                }
            }

            //Return new generate polygon
            return new Polygon2D(resultBuilder.ToArray());
        }

        /// <summary>
        /// Calculates the bounding box of this polygon.
        /// </summary>
        private BoundingBox2D CalculateBoundingBox()
        {
            if (m_vertices.Length == 0)
            {
                return BoundingBox2D.Empty;
            }
            else
            {
                Vector2 minimum = new Vector2(float.MaxValue, float.MaxValue);
                Vector2 maximum = new Vector2(float.MinValue, float.MinValue);

                for (int loopVertex = 0; loopVertex < m_vertices.Length; loopVertex++)
                {
                    if (m_vertices[loopVertex].X < minimum.X) { minimum.X = m_vertices[loopVertex].X; }
                    if (m_vertices[loopVertex].Y < minimum.Y) { minimum.Y = m_vertices[loopVertex].Y; }
                    if (m_vertices[loopVertex].X > maximum.X) { maximum.X = m_vertices[loopVertex].X; }
                    if (m_vertices[loopVertex].Y > maximum.Y) { maximum.Y = m_vertices[loopVertex].Y; }
                }

                return new BoundingBox2D(minimum, maximum - minimum);
            }
        }

        /// <summary>
        /// Calculates the edge order of this polygon.
        /// </summary>
        private EdgeOrder CalculateEdgeOrder()
        {
            //Calculation method taken from http://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
            //Formula: 
            // For each Edge: (x2-x1)(y2+y1)
            // Take sum from each result
            // If result is positiv, vertices are aligned clockwise, otherwhiese counter-clockwise

            if (m_vertices.Length < 2) { return EdgeOrder.Unknown; }

            float currentSum = 0f;
            for (int loopVertex = 0; loopVertex < m_vertices.Length; loopVertex++)
            {
                //Get index of following vertex
                int loopNext = loopVertex + 1;
                if (loopNext >= m_vertices.Length) { loopNext = 0; }

                //Calculate sum
                currentSum +=
                    (m_vertices[loopNext].X - m_vertices[loopVertex].X) *
                    (m_vertices[loopNext].Y + m_vertices[loopVertex].Y);
            }

            //Return result depending on sum
            if (currentSum > 0f) { return EdgeOrder.Clockwise; }
            else { return EdgeOrder.CounterClockwise; }
        }

        /// <summary>
        /// Returns all lines defined by this polygon.
        /// </summary>
        public IEnumerable<Line2D> Lines
        {
            get
            {
                for (int loop = 0; loop < m_vertices.Length; loop++)
                {
                    if (loop >= m_vertices.Length - 1)
                    {
                        yield return new Line2D(m_vertices[loop], m_vertices[0]);
                    }
                    else
                    {
                        yield return new Line2D(m_vertices[loop], m_vertices[loop + 1]);
                    }
                }
            }
        }

        /// <summary>
        /// Triangulates this polygon using the cutting ears triangulator.
        /// </summary>
        public IEnumerable<int> TriangulateUsingCuttingEars()
        {
            return CuttingEarsTriangulator.Triangulate(m_vertices);
        }

        /// <summary>
        /// Gets all vertices defined by this polygon.
        /// </summary>
        public ReadOnlyCollection<Vector2> Vertices
        {
            get { return m_verticesPublic; }
        }

        /// <summary>
        /// Gets the bounding box of this polygon.
        /// </summary>
        public BoundingBox2D BoundingBox
        {
            get { return m_boundingBox2D.Value; }
        }

        /// <summary>
        /// Gets the current edge order.
        /// </summary>
        public EdgeOrder EdgeOrder
        {
            get { return m_edgeOrder.Value; }
        }
    }
}
