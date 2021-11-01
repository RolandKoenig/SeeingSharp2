using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;

namespace SeeingSharp
{
    public class Polygon2D
    {
        private Vector2[] _vertices;
        private Lazy<BoundingBox2D> _boundingBox2D;
        private Lazy<EdgeOrder> _edgeOrder;

        /// <summary>
        /// Returns all lines defined by this polygon.
        /// </summary>
        public IEnumerable<Line2D> Lines
        {
            get
            {
                for (var loop = 0; loop < _vertices.Length; loop++)
                {
                    if (loop >= _vertices.Length - 1)
                    {
                        yield return new Line2D(_vertices[loop], _vertices[0]);
                    }
                    else
                    {
                        yield return new Line2D(_vertices[loop], _vertices[loop + 1]);
                    }
                }
            }
        }

        /// <summary>
        /// Gets all vertices defined by this polygon.
        /// </summary>
        public ReadOnlyCollection<Vector2> Vertices { get; }

        /// <summary>
        /// Gets the bounding box of this polygon.
        /// </summary>
        public BoundingBox2D BoundingBox => _boundingBox2D.Value;

        /// <summary>
        /// Gets the current edge order.
        /// </summary>
        public EdgeOrder EdgeOrder => _edgeOrder.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon2D" /> class.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <exception cref="SeeingSharpException"></exception>
        public Polygon2D(params Vector2[] vertices)
        {
            if (vertices.Length < 3) { throw new SeeingSharpException("A polygon must at least have 4 vertices!"); }

            //Apply given vertices (remove the last one if it is equal to the first one)
            _vertices = vertices;
            if (_vertices.Length > 1 &&
               _vertices[_vertices.Length - 1] == _vertices[0])
            {
                var newArray = new Vector2[_vertices.Length - 1];
                Array.Copy(_vertices, newArray, _vertices.Length - 1);
                _vertices = newArray;
            }

            this.Vertices = new ReadOnlyCollection<Vector2>(_vertices);
            _boundingBox2D = new Lazy<BoundingBox2D>(this.CalculateBoundingBox);
            _edgeOrder = new Lazy<EdgeOrder>(this.CalculateEdgeOrder);
        }

        /// <summary>
        /// Clones this polygon.
        /// </summary>
        public Polygon2D Clone()
        {
            return new Polygon2D(_vertices.Clone() as Vector2[]);
        }

        /// <summary>
        /// Merges this polygon with the given one defining a hole. The result is a new polygon.
        /// </summary>
        public Polygon2D MergeWithHole(Polygon2D actHole, Polygon2DMergeOptions mergeOptions)
        {
            return this.MergeWithHole(actHole, mergeOptions, null);
        }

        /// <summary>
        /// Merges the with hole.
        /// </summary>
        public Polygon2D MergeWithHole(Polygon2D actHole, Polygon2DMergeOptions mergeOptions, List<Vector2> cutPoints)
        {
            //This algorithm uses the method described in http://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf

            //Find the hole vertex with the highest x value
            var holeVertexWithHighestX = new Vector2(float.MinValue, 0f);
            var holeVertexIndexWithHighestX = -1;
            for (var loopVertex = 0; loopVertex < actHole._vertices.Length; loopVertex++)
            {
                if (actHole._vertices[loopVertex].X > holeVertexWithHighestX.X)
                {
                    holeVertexWithHighestX = actHole._vertices[loopVertex];
                    holeVertexIndexWithHighestX = loopVertex;
                }
            }
            cutPoints?.Add(holeVertexWithHighestX);

            //Define a ray from the found vertex pointing in x direction
            var ray2D = new Ray2D(holeVertexWithHighestX, new Vector2(1f, 0f));

            //Find the line on current filling polygon with intersects first with the created ray
            Tuple<int, float, Vector2> foundLine = null;
            var actLineIndex = 0;
            foreach (var actLine in this.Lines)
            {
                var actIntersection = actLine.Intersect(ray2D);
                if (actIntersection.Item1)
                {
                    var rayToIntersectionPoint = new Ray2D(
                        ray2D.Origin,
                        Vector2.Normalize(actIntersection.Item2 - ray2D.Origin));
                    var lengthToIntersectionPoint = Vector2.Distance(actIntersection.Item2, ray2D.Origin);
                    if (lengthToIntersectionPoint > 0f &&
                        rayToIntersectionPoint.EqualsWithTolerance(ray2D))
                    {
                        if (foundLine == null)
                        {
                            //First found intersection
                            foundLine = Tuple.Create(actLineIndex, lengthToIntersectionPoint, actIntersection.Item2);
                        }
                        else if (lengthToIntersectionPoint < foundLine.Item2)
                        {
                            //More intersections found.. take the one with smallest distance to intersection point
                            foundLine = Tuple.Create(actLineIndex, lengthToIntersectionPoint, actIntersection.Item2);
                        }
                    }
                }
                actLineIndex++;
            }
            if (cutPoints != null && foundLine != null) { cutPoints.Add(foundLine.Item3); }

            // Check for found intersection
            // Return a duplicate of this polygon as the result if no intersection found
            if (foundLine == null)
            {
                var newPolygonVertices = new Vector2[_vertices.Length];
                Array.Copy(_vertices, newPolygonVertices, _vertices.Length);
                return new Polygon2D(newPolygonVertices);
            }

            //Now generate result polygon 
            var resultBuilder = new List<Vector2>(_vertices.Length + actHole._vertices.Length + 2);
            for (var loopFillVertex = 0; loopFillVertex < _vertices.Length; loopFillVertex++)
            {
                //Add current vertex from filling polygon first
                resultBuilder.Add(_vertices[loopFillVertex]);

                //Do special logic on cut point
                if (loopFillVertex == foundLine.Item1)
                {
                    //Cut point.. place here the hole polygon
                    if (!_vertices[loopFillVertex].Equals(foundLine.Item3))
                    {
                        resultBuilder.Add(foundLine.Item3);
                    }

                    //Add all vertices from the hole polygon
                    resultBuilder.Add(actHole._vertices[holeVertexIndexWithHighestX]);
                    var loopHoleVertex = holeVertexIndexWithHighestX + 1;
                    while (loopHoleVertex != holeVertexIndexWithHighestX)
                    {
                        if (loopHoleVertex >= actHole._vertices.Length) { loopHoleVertex = 0; }

                        resultBuilder.Add(actHole._vertices[loopHoleVertex]);

                        loopHoleVertex++;
                        if (loopHoleVertex >= actHole._vertices.Length) { loopHoleVertex = 0; }
                    }

                    //Add cutpoints again to continue with main polygon
                    if (mergeOptions.MakeMergepointSpaceForTriangulation)
                    {
                        resultBuilder.Add(actHole._vertices[holeVertexIndexWithHighestX] + new Vector2(0f, 0.001f));

                        //Add the cutpoint again
                        resultBuilder.Add(foundLine.Item3 + new Vector2(0f, 0.001f));
                    }
                    else
                    {
                        resultBuilder.Add(actHole._vertices[holeVertexIndexWithHighestX]);

                        //Add the cutpoint again
                        resultBuilder.Add(foundLine.Item3);
                    }

                    //Handle the case in which next vertex would equal current cut point
                    if (_vertices.Length > loopFillVertex + 1 &&
                        _vertices[loopFillVertex + 1].Equals(foundLine.Item3))
                    {
                        loopFillVertex++;
                    }
                }
            }

            //Return new generate polygon
            return new Polygon2D(resultBuilder.ToArray());
        }

        /// <summary>
        /// Triangulates this polygon using the cutting ears triangulator.
        /// </summary>
        public IEnumerable<int> TriangulateUsingCuttingEars()
        {
            return CuttingEarsTriangulator.Triangulate(_vertices);
        }

        /// <summary>
        /// Calculates the bounding box of this polygon.
        /// </summary>
        private BoundingBox2D CalculateBoundingBox()
        {
            if (_vertices.Length == 0)
            {
                return BoundingBox2D.Empty;
            }
            var minimum = new Vector2(float.MaxValue, float.MaxValue);
            var maximum = new Vector2(float.MinValue, float.MinValue);

            for (var loopVertex = 0; loopVertex < _vertices.Length; loopVertex++)
            {
                if (_vertices[loopVertex].X < minimum.X) { minimum.X = _vertices[loopVertex].X; }
                if (_vertices[loopVertex].Y < minimum.Y) { minimum.Y = _vertices[loopVertex].Y; }
                if (_vertices[loopVertex].X > maximum.X) { maximum.X = _vertices[loopVertex].X; }
                if (_vertices[loopVertex].Y > maximum.Y) { maximum.Y = _vertices[loopVertex].Y; }
            }

            return new BoundingBox2D(minimum, maximum - minimum);
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
            // If result is positive, vertices are aligned clockwise, otherwise counter-clockwise

            if (_vertices.Length < 2) { return EdgeOrder.Unknown; }

            var currentSum = 0f;
            for (var loopVertex = 0; loopVertex < _vertices.Length; loopVertex++)
            {
                //Get index of following vertex
                var loopNext = loopVertex + 1;
                if (loopNext >= _vertices.Length) { loopNext = 0; }

                //Calculate sum
                currentSum +=
                    (_vertices[loopNext].X - _vertices[loopVertex].X) *
                    (_vertices[loopNext].Y + _vertices[loopVertex].Y);
            }

            //Return result depending on sum
            if (currentSum > 0f) { return EdgeOrder.Clockwise; }
            return EdgeOrder.CounterClockwise;
        }
    }
}
