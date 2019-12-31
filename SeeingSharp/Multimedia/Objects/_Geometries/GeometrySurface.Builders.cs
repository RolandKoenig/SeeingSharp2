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
using SeeingSharp.Multimedia.Core;
using System;
using System.Numerics;

namespace SeeingSharp.Multimedia.Objects
{
    public partial class GeometrySurface
    {
        // Members for build-time transform
        private Vector2 m_tileSize = Vector2.Zero;

        /// <summary>
        /// Enables texture tile mode.
        /// </summary>
        public void EnableTextureTileMode(Vector2 tileSize)
        {
            m_tileSize = tileSize;
        }

        /// <summary>
        /// Disables texture tile mode.
        /// </summary>
        public void DisableTextureTileMode()
        {
            m_tileSize = Vector2.Zero;
        }

        /// <summary>
        /// Performs a simple picking test against all triangles of this object.
        /// </summary>
        /// <param name="pickingRay">The picking ray.</param>
        /// <param name="distance">Additional picking options.</param>
        /// <param name="pickingOptions">The distance if picking succeeds.</param>
        public bool Intersects(Ray pickingRay, PickingOptions pickingOptions, out float distance)
        {
            distance = float.MaxValue;
            var result = false;

            for (var loop = 0; loop < m_corners.Count; loop += 3)
            {
                var vertex1 = this.Owner.VerticesInternal[m_corners[loop].Index].Position;
                var vertex2 = this.Owner.VerticesInternal[m_corners[loop + 1].Index].Position;
                var vertex3 = this.Owner.VerticesInternal[m_corners[loop + 2].Index].Position;

                if (pickingRay.Intersects(ref vertex1, ref vertex2, ref vertex3, out float currentDistance))
                {
                    result = true;
                    if (currentDistance < distance)
                    {
                        distance = currentDistance;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Builds a plain polygon using the given coordinates.
        /// </summary>
        /// <param name="coordinates">The coordinates to build the polygon from.</param>
        public void BuildPlainPolygon(Vector3[] coordinates)
        {
            // Build the polygon
            var polygon = new Polygon(coordinates);

            // Try to triangulate it
            var indices = polygon.TriangulateUsingCuttingEars();
            if (indices == null)
            {
                throw new SeeingSharpGraphicsException("Unable to triangulate given polygon!");
            }

            // Append all vertices
            var baseIndex = this.Owner.CountVertices;

            for (var loopCoordinates = 0; loopCoordinates < coordinates.Length; loopCoordinates++)
            {
                this.Owner.AddVertex(new Vertex(coordinates[loopCoordinates]));
            }

            // Append all indices
            using (var indexEnumerator = indices.GetEnumerator())
            {
                while (indexEnumerator.MoveNext())
                {
                    var index1 = indexEnumerator.Current;
                    var index2 = 0;
                    var index3 = 0;

                    if (indexEnumerator.MoveNext()) { index2 = indexEnumerator.Current; } else { break; }
                    if (indexEnumerator.MoveNext()) { index3 = indexEnumerator.Current; } else { break; }

                    this.AddTriangle(index1 + baseIndex, index2 + baseIndex, index3 + baseIndex);
                }
            }
        }

        /// <summary>
        /// Builds a column using 24 vertices.
        /// </summary>
        /// <param name="bottomMiddle">The bottom middle point.</param>
        /// <param name="size">Size on the ground.</param>
        /// <param name="height">Total height of the column.</param>
        public BuiltVerticesRange BuildColumn24V(Vector3 bottomMiddle, float size, float height)
        {
            var startVertex = this.Owner.CountVertices;

            var halfSize = size / 2f;
            this.BuildCube24V(
                new Vector3(bottomMiddle.X - halfSize, bottomMiddle.Y, bottomMiddle.Z - halfSize),
                new Vector3(size, height, size));

            return new BuiltVerticesRange(this.Owner, startVertex, this.Owner.CountVertices - startVertex);
        }

        public BuiltVerticesRange BuildHorizontalColumnX24V(Vector3 leftMiddle, float size, float width)
        {
            var startVertex = this.Owner.CountVertices;

            var halfSize = size / 2f;
            this.BuildCube24V(
                new Vector3(leftMiddle.X, leftMiddle.Y - halfSize, leftMiddle.Z - halfSize),
                new Vector3(width, size, size));

            return new BuiltVerticesRange(this.Owner, startVertex, this.Owner.CountVertices - startVertex);
        }

        public BuiltVerticesRange BuildHorizontalColumnZ24V(Vector3 frontMiddle, float size, float depth)
        {
            var startVertex = this.Owner.CountVertices;

            var halfSize = size / 2f;
            this.BuildCube24V(
                new Vector3(frontMiddle.X - halfSize, frontMiddle.Y - halfSize, frontMiddle.Z),
                new Vector3(size, size, depth));

            return new BuiltVerticesRange(this.Owner, startVertex, this.Owner.CountVertices - startVertex);
        }

        /// <summary>
        /// Builds a x-axis aligned rectangle.
        /// </summary>
        /// <param name="startPoint">Start point (left center).</param>
        /// <param name="destinationPoint">Destination point (right center).</param>
        /// <param name="size">The size of the rectangle.</param>
        public BuiltVerticesRange BuildXAxisAlignedRect(Vector3 startPoint, Vector3 destinationPoint, float size)
        {
            var startVertex = this.Owner.CountVertices;

            var halfSize = size / 2f;
            this.BuildCube24V(
                new Vector3(startPoint.X, startPoint.Y - halfSize, startPoint.Z - halfSize),
                new Vector3(destinationPoint.X - startPoint.X, size, size));

            return new BuiltVerticesRange(this.Owner, startVertex, this.Owner.CountVertices - startVertex);
        }

        /// <summary>
        /// Builds a z-axis aligned rectangle.
        /// </summary>
        /// <param name="startPoint">Start point (left center).</param>
        /// <param name="destinationPoint">Destination point (right center).</param>
        /// <param name="size">The size of the rectangle.</param>
        public BuiltVerticesRange BuildZAxisAlignedRect(Vector3 startPoint, Vector3 destinationPoint, float size)
        {
            var startVertex = this.Owner.CountVertices;

            var halfSize = size / 2f;
            this.BuildCube24V(
                new Vector3(startPoint.X - halfSize, startPoint.Y - halfSize, startPoint.Z),
                new Vector3(size, size, destinationPoint.Z - startPoint.Z));

            return new BuiltVerticesRange(this.Owner, startVertex, this.Owner.CountVertices - startVertex);
        }

        public BuiltVerticesRange BuildCircleFullV(Vector3 middle, float radius, float width, float height, int countOfSegments)
        {
            var startVertex = this.Owner.CountVertices;
            if (countOfSegments < 3) { throw new ArgumentException("Segment count of " + countOfSegments + " is too small!", nameof(countOfSegments)); }

            var halfWidth = width / 2f;
            var halfHeight = height / 2f;
            var nearVector = new Vector2(0f, radius - halfWidth);
            var farVector = new Vector2(0f, radius + halfWidth);
            var lastNearVector = nearVector;
            var lastFarVector = farVector;
            for (var loop = 1; loop <= countOfSegments; loop++)
            {
                var actPercent = loop / (float)countOfSegments;
                var actAngle = EngineMath.RAD_360DEG * actPercent;

                // Calculate next points
                var rotationMatrix = Matrix3x2.CreateRotation(actAngle);
                var nextNearVector = Vector2.Transform(nearVector, rotationMatrix);
                var nextFarVector = Vector2.Transform(farVector, rotationMatrix);

                // Build current segment
                this.BuildRect4V(
                    middle + new Vector3(lastNearVector.X, halfHeight, lastNearVector.Y),
                    middle + new Vector3(lastFarVector.X, halfHeight, lastFarVector.Y),
                    middle + new Vector3(nextFarVector.X, halfHeight, nextFarVector.Y),
                    middle + new Vector3(nextNearVector.X, halfHeight, nextNearVector.Y),
                    Vector3.UnitY);
                this.BuildRect4V(
                    middle + new Vector3(lastFarVector.X, halfHeight, lastFarVector.Y),
                    middle + new Vector3(lastFarVector.X, -halfHeight, lastFarVector.Y),
                    middle + new Vector3(nextFarVector.X, -halfHeight, nextFarVector.Y),
                    middle + new Vector3(nextFarVector.X, halfHeight, nextFarVector.Y),
                    Vector3.Normalize(new Vector3(lastFarVector.X, 0, lastFarVector.Y)));
                this.BuildRect4V(
                    middle + new Vector3(lastFarVector.X, -halfHeight, lastFarVector.Y),
                    middle + new Vector3(lastNearVector.X, -halfHeight, lastNearVector.Y),
                    middle + new Vector3(nextNearVector.X, -halfHeight, nextNearVector.Y),
                    middle + new Vector3(nextFarVector.X, -halfHeight, nextFarVector.Y),
                    -Vector3.UnitY);
                this.BuildRect4V(
                    middle + new Vector3(lastNearVector.X, halfHeight, lastNearVector.Y),
                    middle + new Vector3(nextNearVector.X, halfHeight, nextNearVector.Y),
                    middle + new Vector3(nextNearVector.X, -halfHeight, nextNearVector.Y),
                    middle + new Vector3(lastNearVector.X, -halfHeight, lastNearVector.Y),
                    Vector3.Normalize(new Vector3(lastFarVector.X, 0, lastFarVector.Y)));

                lastNearVector = nextNearVector;
                lastFarVector = nextFarVector;
            }

            return new BuiltVerticesRange(this.Owner, startVertex, this.Owner.CountVertices - startVertex);
        }

        /// <summary>
        /// Create a 4 Side Pyramid
        /// </summary>
        public BuiltVerticesRange BuildPyramidFullV(Vector3 lowerMiddle, float width, float height)
        {
            var halfWidth = width / 2f;
            var start = new Vector3(lowerMiddle.X - halfWidth, lowerMiddle.Y, lowerMiddle.Z - halfWidth);
            var dest = start + new Vector3(width, 0f, width);
            var centerTopCoordination = new Vector3((dest.X + start.X) / 2, start.Y + height, (dest.Z + start.Z) / 2);

            this.BuildRect4V(
                new Vector3(start.X, dest.Y, start.Z),
                new Vector3(start.X, dest.Y, dest.Z),
                new Vector3(dest.X, dest.Y, dest.Z),
                new Vector3(dest.X, dest.Y, start.Z),
                new Vector3(0f, -1f, 0f));

            this.BuildTriangleV(
                new Vector3(start.X, dest.Y, start.Z),
                new Vector3(dest.X, dest.Y, start.Z),
                centerTopCoordination);
            this.BuildTriangleV(
                new Vector3(start.X, dest.Y, dest.Z),
                new Vector3(start.X, dest.Y, start.Z),
                centerTopCoordination);
            this.BuildTriangleV(
                new Vector3(dest.X, dest.Y, dest.Z),
                new Vector3(start.X, dest.Y, dest.Z),
                centerTopCoordination);
            this.BuildTriangleV(
                new Vector3(dest.X, dest.Y, start.Z),
                new Vector3(dest.X, dest.Y, dest.Z),
                centerTopCoordination);

            return new BuiltVerticesRange(this.Owner, this.Owner.CountVertices - 4, 4);
        }

        /// <summary>
        /// Build a Triangle.
        /// </summary>
        public BuiltVerticesRange BuildTriangleV(Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            var texX = 1f;
            var texY = 1f;

            if (m_tileSize != Vector2.Zero)
            {
                texX = (pointB - pointA).Length() / m_tileSize.X;
                texY = (pointC - pointB).Length() / m_tileSize.Y;
            }

            var vertex = new Vertex(pointA, new Vector2(0f, texY));

            var a = this.Owner.AddVertex(vertex);
            var b = this.Owner.AddVertex(vertex.Copy(pointB, new Vector2(texX, texY)));
            var c = this.Owner.AddVertex(vertex.Copy(pointC, new Vector2(texX, 0f)));

            this.AddTriangleAndCalculateNormalsFlat(a, c, b);

            return new BuiltVerticesRange(this.Owner, this.Owner.CountVertices - 4, 4);
        }

        /// <summary>
        /// Builds a cone into the geometry with correct texture coordinates and normals.
        /// </summary>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cone.</param>
        /// <param name="height">The height of the cone.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        public BuiltVerticesRange BuildConeFullV(Vector3 bottomMiddle, float radius, float height, int countOfSegments)
        {
            var startVertex = this.Owner.CountVertices;

            if (countOfSegments < 5)
            {
                throw new ArgumentException("Segment count of " + countOfSegments + " is too small!", nameof(countOfSegments));
            }

            var diameter = radius * 2f;

            // Get texture offsets
            var texX = 1f;
            var texY = 1f;

            if (m_tileSize != Vector2.Zero)
            {
                texX = diameter / m_tileSize.X;
                texY = diameter / m_tileSize.Y;
            }

            // Specify bottom and top middle coordinates
            var bottomCoordinate = bottomMiddle;
            var topCoordinate = new Vector3(bottomMiddle.X, bottomMiddle.Y + height, bottomMiddle.Z);

            // Create bottom and top vertices
            var bottomVertex = new Vertex(bottomCoordinate, new Vector2(texX / 2f, texY / 2f), new Vector3(0f, -1f, 0f));

            // AddObject bottom and top vertices to the geometry
            var bottomVertexIndex = this.Owner.AddVertex(bottomVertex);

            // Generate all segments
            var countOfSegmentsF = (float)countOfSegments;
            for (var loop = 0; loop < countOfSegments; loop++)
            {
                // Calculate rotation values for each segment border
                var startRadian = EngineMath.RAD_360DEG * (loop / countOfSegmentsF);
                var targetRadian = EngineMath.RAD_360DEG * ((loop + 1) / countOfSegmentsF);
                var normalRadian = startRadian + (targetRadian - startRadian) / 2f;

                // Generate all normals
                var sideNormal = Vector3Ex.NormalFromHVRotation(normalRadian, 0f);
                var sideLeftNormal = Vector3Ex.NormalFromHVRotation(startRadian, 0f);
                var sideRightNormal = Vector3Ex.NormalFromHVRotation(targetRadian, 0f);

                //Generate all points
                var sideLeftBottomCoord = bottomCoordinate + sideLeftNormal * radius;
                var sideRightBottomCoord = bottomCoordinate + sideRightNormal * radius;
                var sideMiddleBottomCoord = bottomCoordinate + sideNormal * radius;

                //AddObject segment bottom triangle
                var segmentBottomLeft = bottomVertex.Copy(sideLeftBottomCoord);
                var segmentBottomRight = bottomVertex.Copy(sideRightBottomCoord);
                this.AddTriangle(
                    bottomVertexIndex, this.Owner.AddVertex(segmentBottomLeft), this.Owner.AddVertex(segmentBottomRight));

                //Generate side normal
                var vectorToTop = topCoordinate - sideMiddleBottomCoord;
                var vectorToTopRotation = Vector3Ex.ToHVRotation(vectorToTop);
                vectorToTopRotation.Y = vectorToTopRotation.Y + EngineMath.RAD_90DEG;
                var topSideNormal = Vector3Ex.NormalFromHVRotation(vectorToTopRotation);

                //AddObject segment top triangle
                var topVertex = new Vertex(topCoordinate, new Vector2(texX / 2f, texY / 2f), topSideNormal);
                var segmentTopLeft = topVertex.Copy(sideLeftBottomCoord);
                var segmentTopRight = topVertex.Copy(sideRightBottomCoord);

                this.AddTriangle(this.Owner.AddVertex(topVertex), this.Owner.AddVertex(segmentTopRight), this.Owner.AddVertex(segmentTopLeft));
            }

            return new BuiltVerticesRange(this.Owner, startVertex, this.Owner.CountVertices - startVertex);
        }

        /// <summary>
        /// Builds a cylinder into the geometry with correct texture coordinates and normals.
        /// </summary>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        public BuiltVerticesRange BuildCylinderFullV(Vector3 bottomMiddle, float radius, float height, int countOfSegments)
        {
            return this.BuildCylinderV(bottomMiddle, radius, height, countOfSegments, true, true, true);
        }

        /// <summary>
        /// Builds a cylinder into the geometry with correct texture coordinates and normals.
        /// </summary>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        public BuiltVerticesRange BuildCylinderTopV(Vector3 bottomMiddle, float radius, float height, int countOfSegments)
        {
            return this.BuildCylinderV(bottomMiddle, radius, height, countOfSegments, false, false, true);
        }

        /// <summary>
        /// Builds a cylinder into the geometry with correct texture coordinates and normals.
        /// </summary>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        public BuiltVerticesRange BuildCylinderSidesV(Vector3 bottomMiddle, float radius, float height, int countOfSegments)
        {
            return this.BuildCylinderV(bottomMiddle, radius, height, countOfSegments, true, false, false);
        }

        /// <summary>
        /// Builds a cylinder into the geometry with correct texture coordinates and normals.
        /// </summary>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        public BuiltVerticesRange BuildCylinderBottomV(Vector3 bottomMiddle, float radius, float height, int countOfSegments)
        {
            return this.BuildCylinderV(bottomMiddle, radius, height, countOfSegments, false, true, false);
        }

        /// <summary>
        /// Builds a cylinder into the geometry with correct texture coordinates and normals.
        /// </summary>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        /// <param name="buildBottom">Build bottom of the cylinder.</param>
        /// <param name="buildSides">Build sides of the cylinder.</param>
        /// <param name="buildTop">Build top side of the cylinder.</param>
        public BuiltVerticesRange BuildCylinderV(
            Vector3 bottomMiddle, float radius, float height, int countOfSegments,
            bool buildSides, bool buildBottom, bool buildTop)
        {
            var startVertex = this.Owner.CountVertices;

            if (countOfSegments < 5) { throw new ArgumentException("Segment count of " + countOfSegments + " is too small!", nameof(countOfSegments)); }
            var diameter = radius * 2f;

            // Get texture offsets
            var texX = 1f;
            var texY = 1f;
            var texSegmentY = 1f;
            var texSegmentX = 1f;
            if (m_tileSize != Vector2.Zero)
            {
                texX = diameter / m_tileSize.X;
                texY = diameter / m_tileSize.Y;
                texSegmentY = height / m_tileSize.Y;
                texSegmentX = EngineMath.RAD_180DEG * diameter / m_tileSize.X;
            }

            // Specify bottom and top middle coordinates
            var bottomCoordinate = bottomMiddle;
            var topCoordinate = new Vector3(bottomMiddle.X, bottomMiddle.Y + height, bottomMiddle.Z);

            // Create bottom and top vertices
            var bottomVertex = new Vertex(bottomCoordinate, new Vector2(texX / 2f, texY / 2f), new Vector3(0f, -1f, 0f));
            var topVertex = new Vertex(topCoordinate, new Vector2(texX / 2f, texY / 2f), new Vector3(0f, 1f, 0f));

            // AddObject bottom and top vertices to the geometry
            var bottomVertexIndex = this.Owner.AddVertex(bottomVertex);
            var topVertexIndex = this.Owner.AddVertex(topVertex);

            // Generate all segments
            var fullRadian = EngineMath.RAD_360DEG;
            var countOfSegmentsF = (float)countOfSegments;

            for (var loop = 0; loop < countOfSegments; loop++)
            {
                // Calculate rotation values for each segment border
                var startRadian = fullRadian * (loop / countOfSegmentsF);
                var targetRadian = fullRadian * ((loop + 1) / countOfSegmentsF);
                var normalRadian = startRadian + (targetRadian - startRadian) / 2f;

                // Generate all normals
                var sideNormal = Vector3Ex.NormalFromHVRotation(normalRadian, 0f);
                var sideLeftNormal = Vector3Ex.NormalFromHVRotation(startRadian, 0f);
                var sideRightNormal = Vector3Ex.NormalFromHVRotation(targetRadian, 0f);

                //
                var sideLeftTexCoord = new Vector2(0.5f + sideLeftNormal.X * radius, 0.5f + sideLeftNormal.Z * radius);
                var sideRightTexCoord = new Vector2(0.5f + sideRightNormal.X * radius, 0.5f + sideRightNormal.Z * radius);

                // Generate all points
                var sideLeftBottomCoord = bottomCoordinate + sideLeftNormal * radius;
                var sideRightBottomCoord = bottomCoordinate + sideRightNormal * radius;
                var sideLeftTopCoord = new Vector3(sideLeftBottomCoord.X, sideLeftBottomCoord.Y + height, sideLeftBottomCoord.Z);
                var sideRightTopCoord = new Vector3(sideRightBottomCoord.X, sideRightBottomCoord.Y + height, sideRightBottomCoord.Z);

                // AddObject segment bottom triangle
                if (buildBottom)
                {
                    var segmentBottomLeft = bottomVertex.Copy(sideLeftBottomCoord, sideLeftTexCoord);
                    var segmentBottomRight = bottomVertex.Copy(sideRightBottomCoord, sideRightTexCoord);
                    this.AddTriangle(
                        bottomVertexIndex, this.Owner.AddVertex(segmentBottomLeft), this.Owner.AddVertex(segmentBottomRight));
                }

                // AddObject segment top triangle
                if (buildTop)
                {
                    var segmentTopLeft = topVertex.Copy(sideLeftTopCoord, sideLeftTexCoord);
                    var segmentTopRight = topVertex.Copy(sideRightTopCoord, sideRightTexCoord);
                    this.AddTriangle(
                        topVertexIndex, this.Owner.AddVertex(segmentTopRight), this.Owner.AddVertex(segmentTopLeft));
                }

                if (buildSides)
                {
                    // Calculate texture coords for side segment
                    var texCoordSegmentStart = new Vector2(texSegmentX * (loop / (float)countOfSegments), 0f);
                    var texCoordSegmentTarget = new Vector2(texSegmentX * ((loop + 1) / (float)countOfSegments), texSegmentY);

                    // AddObject segment side
                    this.BuildRect4V(sideLeftBottomCoord, sideRightBottomCoord, sideRightTopCoord, sideLeftTopCoord, sideNormal, texCoordSegmentStart, texCoordSegmentTarget);
                }
            }

            return new BuiltVerticesRange(this.Owner, startVertex, this.Owner.CountVertices - startVertex);
        }

        /// <summary>
        /// Builds a sphere geometry.
        /// </summary>
        public BuiltVerticesRange BuildShpere(int tDiv, int pDiv, double radius)
        {
            tDiv = Math.Max(tDiv, 3);
            pDiv = Math.Max(pDiv, 2);
            radius = Math.Max(Math.Abs(radius), EngineMath.TOLERANCE_FLOAT_POSITIVE);

            Vector3 SphereGetPosition(double theta, double phi)
            {
                var x = radius * Math.Sin(theta) * Math.Sin(phi);
                var y = radius * Math.Cos(phi);
                var z = radius * Math.Cos(theta) * Math.Sin(phi);

                return new Vector3((float)x, (float)y, (float)z);
            }
            Vector2 SphereGetTextureCoordinate(double theta, double phi)
            {
                return new Vector2(
                    (float)(theta / (2 * Math.PI)),
                    (float)(phi / Math.PI));
            }

            var startVertex = this.Owner.CountVertices;
            var dt = Math.PI * 2 / tDiv;
            var dp = Math.PI / pDiv;

            for (var pi = 0; pi <= pDiv; pi++)
            {
                var phi = pi * dp;

                for (var ti = 0; ti <= tDiv; ti++)
                {
                    // we want to start the mesh on the x axis
                    var theta = ti * dt;

                    var position = SphereGetPosition(theta, phi);
                    var vertex = new Vertex(
                        position,
                        SphereGetTextureCoordinate(theta, phi),
                        Vector3.Normalize(position));
                    this.Owner.Vertices.Add(vertex);
                }
            }

            for (var pi = 0; pi < pDiv; pi++)
            {
                for (var ti = 0; ti < tDiv; ti++)
                {
                    var x0 = ti;
                    var x1 = ti + 1;
                    var y0 = pi * (tDiv + 1);
                    var y1 = (pi + 1) * (tDiv + 1);

                    this.Triangles.Add(
                        x0 + y0,
                        x0 + y1,
                        x1 + y0);

                    this.Triangles.Add(
                        x1 + y0,
                        x0 + y1,
                        x1 + y1);
                }
            }

            return new BuiltVerticesRange(this.Owner, startVertex, this.Owner.CountVertices - startVertex);
        }

        /// <summary>
        /// Builds a geosphere geometry.
        /// </summary>
        public BuiltVerticesRange BuildGeosphere(float radius, int countSubdivisions)
        {
            // Implemented with sample code from http://www.d3dcoder.net/d3d11.htm, Source Code Set II

            countSubdivisions = Math.Max(countSubdivisions, 0);
            radius = Math.Max(Math.Abs(radius), EngineMath.TOLERANCE_FLOAT_POSITIVE); // <-- this one prevents device by zero

            var startVertex = this.Owner.CountVertices;
            var startTriangle = this.Owner.CountTriangles;

            // Build an icosahedron
            const float X = 0.525731f;
            const float Z = 0.850651f;
            var pos = new[]
            {
                new Vector3(-X, 0f, Z),    new Vector3(X, 0f, Z), 
                new Vector3(-X, 0f, -Z), new Vector3(X, 0f, -Z),
                new Vector3(0f, Z, X),   new Vector3(0f, Z, -X),
                new Vector3(0f, -Z, X),  new Vector3(0f, -Z, -X),
                new Vector3(Z, X, 0f),   new Vector3(-Z, X, 0f),
                new Vector3(Z, -X, 0f),  new Vector3(-Z, -X, 0f),  
            };
            var k = new[]
            {
                1, 4, 0,   4, 9, 0,   4, 5, 9,   8, 5, 4,   1, 8, 4,
                1, 10, 8,  10, 3, 8,  8, 3, 5,   3, 2, 5,   3, 7, 2,
                3, 10, 7,  10, 6, 7,  6, 11, 7,  6, 0, 11,  6, 1, 0,
                10, 1, 6,  11, 0, 9,  2, 11, 9,  5, 2, 9,   11, 2, 7
            };
            foreach (var actPosition in pos)
            {
                this.Owner.AddVertex(new Vertex(actPosition));
            }
            for (var loop = 0; loop < k.Length; loop += 3)
            {
                this.AddTriangle(k[loop], k[loop +1], k[loop + 2]);
            }

            // Subdivide it n times
            for (var loop = 0; loop < countSubdivisions; loop++)
            {
                this.Subdivide(startTriangle);
            }

            // Project vertices onto sphere and scale
            var vertexCount = this.Owner.CountVertices;
            for (var actVertexIndex = startVertex; actVertexIndex < vertexCount; actVertexIndex++)
            {
                var actVertex = this.Owner.VerticesInternal[actVertexIndex];
                actVertex.Normal = Vector3.Normalize(actVertex.Position);
                actVertex.Position = actVertex.Normal * radius;

                var theta = EngineMath.AngleFromXY(actVertex.Position.X, actVertex.Position.Z);
                var phi = (float)Math.Acos(actVertex.Position.Y / radius);
                actVertex.TexCoord = new Vector2(
                    theta / EngineMath.PI_2,
                    phi / EngineMath.PI);

                this.Owner.VerticesInternal[actVertexIndex] = actVertex;
            }

            return new BuiltVerticesRange(this.Owner, startVertex, this.Owner.CountVertices - startVertex);
        }

        /// <summary>
        /// Builds a cube into a geometry (this cube is built up of just 8 vertices, so no texturing is supported)
        /// </summary>
        /// <param name="start">Start point of the cube (left-lower-front point)</param>
        /// <param name="size">Size of the cube</param>
        public BuiltVerticesRange BuildCube8V(Vector3 start, Vector3 size)
        {
            var startVertex = this.Owner.CountVertices;

            var dest = start + size;
            var vertex = new Vertex(start);

            var a = this.Owner.AddVertex(vertex);
            var b = this.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, start.Z)));
            var c = this.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, dest.Z)));
            var d = this.Owner.AddVertex(vertex.Copy(new Vector3(start.X, start.Y, dest.Z)));
            var e = this.Owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, start.Z)));
            var f = this.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, start.Z)));
            var g = this.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, dest.Z)));
            var h = this.Owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, dest.Z)));

            this.AddTriangle(a, e, f);  // front side
            this.AddTriangle(f, b, a);
            this.AddTriangle(b, f, g);  // right side
            this.AddTriangle(g, c, b);
            this.AddTriangle(c, g, h);  // back side
            this.AddTriangle(h, d, c);
            this.AddTriangle(d, h, e);  // left side
            this.AddTriangle(e, a, d);
            this.AddTriangle(e, h, g);  // top side
            this.AddTriangle(g, f, e);
            this.AddTriangle(a, b, c);  // bottom side
            this.AddTriangle(c, d, a);

            return new BuiltVerticesRange(this.Owner, startVertex, this.Owner.CountVertices - startVertex);
        }

        /// <summary>
        /// Builds a cube into this Geometry (this cube is built up of 24 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="start">Start point of the cube</param>
        /// <param name="size">Size of the cube</param>
        public BuiltVerticesRange BuildCube24V(Vector3 start, Vector3 size)
        {
            var result = new BuiltVerticesRange(this.Owner);

            result.Merge(this.BuildCubeSides16V(start, size));
            result.Merge(this.BuildCubeTop4V(start, size));
            result.Merge(this.BuildCubeBottom4V(start, size));

            return result;
        }

        /// <summary>
        /// Builds a cube of 4 vertices and a defined height.
        /// </summary>
        public BuiltVerticesRange BuildCube24V(Vector3 topA, Vector3 topB, Vector3 topC, Vector3 topD, float height)
        {
            var result = new BuiltVerticesRange(this.Owner)
            {
                StartVertex = this.Owner.CountVertices
            };

            var startTriangleIndex = this.CountTriangles;

            // Calculate texture coordinates
            var size = new Vector3(
                (topB - topA).Length(),
                Math.Abs(height),
                (topC - topB).Length());
            var texX = 1f;
            var texY = 1f;
            var texZ = 1f;

            if (m_tileSize != Vector2.Zero)
            {
                texX = size.X / m_tileSize.X;
                texY = size.Y / m_tileSize.Y;
                texZ = size.Z / m_tileSize.X;
            }

            // Calculate bottom vectors
            var bottomA = new Vector3(topA.X, topA.Y - height, topA.Z);
            var bottomB = new Vector3(topB.X, topB.Y - height, topB.Z);
            var bottomC = new Vector3(topC.X, topC.Y - height, topC.Z);
            var bottomD = new Vector3(topD.X, topD.Y - height, topD.Z);

            // Build Top side
            var vertex = new Vertex(topA, new Vector2(texX, 0f), new Vector3(0f, 1f, 0f));
            var a = this.Owner.AddVertex(vertex);
            var b = this.Owner.AddVertex(vertex.Copy(topB, new Vector2(texX, texY)));
            var c = this.Owner.AddVertex(vertex.Copy(topC, new Vector2(0f, texY)));
            var d = this.Owner.AddVertex(vertex.Copy(topD, new Vector2(0f, 0f)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            // Build Bottom side
            vertex = new Vertex(topA, new Vector2(0f, 0f), new Vector3(0f, -1f, 0f));
            a = this.Owner.AddVertex(vertex);
            b = this.Owner.AddVertex(vertex.Copy(topD, new Vector2(texX, 0f)));
            c = this.Owner.AddVertex(vertex.Copy(topC, new Vector2(texX, texY)));
            d = this.Owner.AddVertex(vertex.Copy(topB, new Vector2(0f, texY)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            // Build Front side
            vertex = new Vertex(topA, new Vector2(0f, texY), new Vector3(0f, 0f, -1f));
            a = this.Owner.AddVertex(vertex);
            b = this.Owner.AddVertex(vertex.Copy(topB, new Vector2(texX, texY)));
            c = this.Owner.AddVertex(vertex.Copy(bottomB, new Vector2(texX, 0f)));
            d = this.Owner.AddVertex(vertex.Copy(bottomA, new Vector2(0f, 0f)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            // Build Right side
            a = this.Owner.AddVertex(vertex.Copy(topB, new Vector3(1f, 0f, 0f), new Vector2(0f, texY)));
            b = this.Owner.AddVertex(vertex.Copy(topC, new Vector3(1f, 0f, 0f), new Vector2(texZ, texY)));
            c = this.Owner.AddVertex(vertex.Copy(bottomC, new Vector3(1f, 0f, 0f), new Vector2(texZ, 0f)));
            d = this.Owner.AddVertex(vertex.Copy(bottomB, new Vector3(1f, 0f, 0f), new Vector2(0f, 0f)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            // Build Back side
            a = this.Owner.AddVertex(vertex.Copy(topC, new Vector3(0f, 0f, 1f), new Vector2(0f, texY)));
            b = this.Owner.AddVertex(vertex.Copy(topD, new Vector3(0f, 0f, 1f), new Vector2(texX, texY)));
            c = this.Owner.AddVertex(vertex.Copy(bottomD, new Vector3(0f, 0f, 1f), new Vector2(texX, 0f)));
            d = this.Owner.AddVertex(vertex.Copy(bottomC, new Vector3(0f, 0f, 1f), new Vector2(0f, 0f)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            // Build Left side
            a = this.Owner.AddVertex(vertex.Copy(topD, new Vector3(-1f, 0f, 0f), new Vector2(0f, texY)));
            b = this.Owner.AddVertex(vertex.Copy(topA, new Vector3(-1f, 0f, 0f), new Vector2(texZ, texY)));
            c = this.Owner.AddVertex(vertex.Copy(bottomA, new Vector3(-1f, 0f, 0f), new Vector2(texZ, 0f)));
            d = this.Owner.AddVertex(vertex.Copy(bottomD, new Vector3(-1f, 0f, 0f), new Vector2(0f, 0f)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            // Calculate normals finally
            this.CalculateNormalsFlat(startTriangleIndex, this.CountTriangles - startTriangleIndex);

            result.VertexCount = this.Owner.CountVertices - result.StartVertex;
            return result;
        }

        /// <summary>
        /// Builds a cube into this Geometry (this cube is built up of 24 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="box">Box defining bounds of generated cube.</param>
        public BuiltVerticesRange BuildCube24V(BoundingBox box)
        {
            return this.BuildCube24V(box.Minimum, box.GetSize());
        }

        /// <summary>
        /// Builds a cube on the given point with the given color.
        /// </summary>
        /// <param name="centerLocation">The location to draw the cube at.</param>
        /// <param name="sideLength">The side length of the cube.</param>
        public BuiltVerticesRange BuildCube24V(Vector3 centerLocation, float sideLength)
        {
            return this.BuildCube24V(
                centerLocation - new Vector3(sideLength / 2f, sideLength / 2f, sideLength / 2f),
                new Vector3(sideLength, sideLength, sideLength));
        }

        /// <summary>
        /// Builds a cube into this Geometry (this cube is built up of 24 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="bottomCenter">Bottom center point of the cube.</param>
        /// <param name="width">Width (and depth) of the cube.</param>
        /// <param name="height">Height of the cube.</param>
        public BuiltVerticesRange BuildCube24V(Vector3 bottomCenter, float width, float height)
        {
            var start = new Vector3(
                bottomCenter.X - width / 2f,
                bottomCenter.Y,
                bottomCenter.Z - width / 2f);
            var size = new Vector3(width, height, width);
            return this.BuildCube24V(start, size);
        }

        /// <summary>
        /// Build a single rectangle into the geometry (Supports texturing and normal vectors)
        /// </summary>
        public BuiltVerticesRange BuildRect4V(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, Vector3 normal, TextureCoordinateCalculationAlignment uCoordAlignment, TextureCoordinateCalculationAlignment vCoordAlignment, float coordRepeatUnit)
        {
            var startVertex = this.Owner.CountVertices;

            //Define texture coordinate calculation functions
            float CalculateU(Vector3 actPosition)
            {
                switch (uCoordAlignment)
                {
                    case TextureCoordinateCalculationAlignment.XAxis:
                        return actPosition.X / coordRepeatUnit;

                    case TextureCoordinateCalculationAlignment.YAxis:
                        return actPosition.Y / coordRepeatUnit;

                    case TextureCoordinateCalculationAlignment.ZAxis:
                        return actPosition.Z / coordRepeatUnit;
                }
                return 0f;
            }

            float CalculateV(Vector3 actPosition)
            {
                switch (vCoordAlignment)
                {
                    case TextureCoordinateCalculationAlignment.XAxis:
                        return actPosition.X / coordRepeatUnit;

                    case TextureCoordinateCalculationAlignment.YAxis:
                        return actPosition.Y / coordRepeatUnit;

                    case TextureCoordinateCalculationAlignment.ZAxis:
                        return actPosition.Z / coordRepeatUnit;
                }
                return 0f;
            }

            var textureCoordinate = new Vector2(CalculateU(pointA), CalculateV(pointA));
            var vertex = new Vertex(pointA, textureCoordinate, normal);

            var a = this.Owner.AddVertex(vertex);
            var b = this.Owner.AddVertex(vertex.Copy(pointB, new Vector2(CalculateU(pointB), CalculateV(pointB))));
            var c = this.Owner.AddVertex(vertex.Copy(pointC, new Vector2(CalculateU(pointC), CalculateV(pointC))));
            var d = this.Owner.AddVertex(vertex.Copy(pointD, new Vector2(CalculateU(pointD), CalculateV(pointD))));

            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            return new BuiltVerticesRange(this.Owner, startVertex, this.Owner.CountVertices - startVertex);
        }

        /// <summary>
        /// Builds the top side of a cube into this Geometry (Built up of 4 vertices, so texture coordinates and normals are set)
        /// </summary>
        public BuiltVerticesRange BuildCubeTop4V(Vector3 start, Vector3 size)
        {
            var dest = start + size;

            return this.BuildRect4V(
                new Vector3(start.X, dest.Y, start.Z),
                new Vector3(dest.X, dest.Y, start.Z),
                new Vector3(dest.X, dest.Y, dest.Z),
                new Vector3(start.X, dest.Y, dest.Z),
                new Vector3(0f, 1f, 0f));
        }

        /// <summary>
        /// Builds the bottom side of a cube into this Geometry (Built up of 4 vertices, so texture coordinates and normals are set)
        /// </summary>
        public BuiltVerticesRange BuildCubeBottom4V(Vector3 start, Vector3 size)
        {
            var dest = start + size;

            return this.BuildRect4V(
                new Vector3(start.X, start.Y, dest.Z),
                new Vector3(dest.X, start.Y, dest.Z),
                new Vector3(dest.X, start.Y, start.Z),
                new Vector3(start.X, start.Y, start.Z),
                new Vector3(0f, -1f, 0f));
        }

        /// <summary>
        /// Builds cube sides into this Geometry (these sides are built up of  16 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="start">Start point of the cube</param>
        /// <param name="size">Size of the cube</param>
        public BuiltVerticesRange BuildCubeSides16V(Vector3 start, Vector3 size)
        {
            var result = new BuiltVerticesRange(this.Owner)
            {
                StartVertex = this.Owner.CountVertices
            };

            var dest = start + size;

            var texX = 1f;
            var texY = 1f;
            var texZ = 1f;

            if (m_tileSize != Vector2.Zero)
            {
                texX = size.X / m_tileSize.X;
                texY = size.Y / m_tileSize.Y;
                texZ = size.Z / m_tileSize.X;
            }

            //Front side
            var vertex = new Vertex(start, new Vector2(0f, texY), new Vector3(0f, 0f, -1f));
            var a = this.Owner.AddVertex(vertex);
            var b = this.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, start.Z), new Vector2(texX, texY)));
            var c = this.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, start.Z), new Vector2(texX, 0f)));
            var d = this.Owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, start.Z), new Vector2(0f, 0f)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            //Right side
            a = this.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, start.Z), new Vector3(1f, 0f, 0f), new Vector2(0f, texY)));
            b = this.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, dest.Z), new Vector3(1f, 0f, 0f), new Vector2(texZ, texY)));
            c = this.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, dest.Z), new Vector3(1f, 0f, 0f), new Vector2(texZ, 0f)));
            d = this.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, start.Z), new Vector3(1f, 0f, 0f), new Vector2(0f, 0f)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            //Back side
            a = this.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, dest.Z), new Vector3(0f, 0f, 1f), new Vector2(0f, texY)));
            b = this.Owner.AddVertex(vertex.Copy(new Vector3(start.X, start.Y, dest.Z), new Vector3(0f, 0f, 1f), new Vector2(texX, texY)));
            c = this.Owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, dest.Z), new Vector3(0f, 0f, 1f), new Vector2(texX, 0f)));
            d = this.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, dest.Z), new Vector3(0f, 0f, 1f), new Vector2(0f, 0f)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            //Left side
            a = this.Owner.AddVertex(vertex.Copy(new Vector3(start.X, start.Y, dest.Z), new Vector3(-1f, 0f, 0f), new Vector2(0f, texY)));
            b = this.Owner.AddVertex(vertex.Copy(new Vector3(start.X, start.Y, start.Z), new Vector3(-1f, 0f, 0f), new Vector2(texZ, texY)));
            c = this.Owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, start.Z), new Vector3(-1f, 0f, 0f), new Vector2(texZ, 0f)));
            d = this.Owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, dest.Z), new Vector3(-1f, 0f, 0f), new Vector2(0f, 0f)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            result.VertexCount = this.Owner.CountVertices - result.StartVertex;
            return result;
        }

        /// <summary>
        /// Build a single rectangle into the geometry (Supports texturing)
        /// </summary>
        public BuiltVerticesRange BuildRect4V(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD)
        {
            var texX = 1f;
            var texY = 1f;

            if (m_tileSize != Vector2.Zero)
            {
                texX = (pointB - pointA).Length() / m_tileSize.X;
                texY = (pointC - pointB).Length() / m_tileSize.Y;
            }

            var vertex = new Vertex(pointA, new Vector2(0f, texY));

            var a = this.Owner.AddVertex(vertex);
            var b = this.Owner.AddVertex(vertex.Copy(pointB, new Vector2(texX, texY)));
            var c = this.Owner.AddVertex(vertex.Copy(pointC, new Vector2(texX, 0f)));
            var d = this.Owner.AddVertex(vertex.Copy(pointD, new Vector2(0f, 0f)));

            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            return new BuiltVerticesRange(this.Owner, this.Owner.CountVertices - 4, 4);
        }

        /// <summary>
        /// Build a single rectangle into the geometry (Supports texturing and normal vectors)
        /// </summary>
        public BuiltVerticesRange BuildRect4V(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, Vector3 normal)
        {
            var texX = 1f;
            var texY = 1f;

            if (m_tileSize != Vector2.Zero)
            {
                texX = (pointB - pointA).Length() / m_tileSize.X;
                texY = (pointC - pointB).Length() / m_tileSize.Y;
            }

            var vertex = new Vertex(pointA, new Vector2(0f, texY), normal);

            var a = this.Owner.AddVertex(vertex);
            var b = this.Owner.AddVertex(vertex.Copy(pointB, new Vector2(texX, texY)));
            var c = this.Owner.AddVertex(vertex.Copy(pointC, new Vector2(texX, 0f)));
            var d = this.Owner.AddVertex(vertex.Copy(pointD, new Vector2(0f, 0f)));

            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            return new BuiltVerticesRange(this.Owner, this.Owner.CountVertices - 4, 4);
        }

        /// <summary>
        /// Build a single rectangle into the geometry (Supports texturing and normal vectors)
        /// </summary>
        public BuiltVerticesRange BuildRect4V(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, Vector3 normal, Vector2 minTexCoord, Vector2 maxTexCoord)
        {
            var vertex = new Vertex(pointA, new Vector2(minTexCoord.X, maxTexCoord.Y), normal);

            var a = this.Owner.AddVertex(vertex);
            var b = this.Owner.AddVertex(vertex.Copy(pointB, new Vector2(maxTexCoord.X, maxTexCoord.Y)));
            var c = this.Owner.AddVertex(vertex.Copy(pointC, new Vector2(maxTexCoord.X, minTexCoord.Y)));
            var d = this.Owner.AddVertex(vertex.Copy(pointD, new Vector2(minTexCoord.X, minTexCoord.Y)));

            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            return new BuiltVerticesRange(this.Owner, this.Owner.CountVertices - 4, 4);
        }

        /// <summary>
        /// Changes the index order of each triangle.
        /// </summary>
        public void ToggleTriangleIndexOrder()
        {
            for (var loop = 2; loop < m_corners.Count; loop += 3)
            {
                var edge1 = m_corners[loop - 2];
                var edge2 = m_corners[loop - 1];
                var edge3 = m_corners[loop];
                m_corners[loop] = edge1;
                m_corners[loop - 1] = edge2;
                m_corners[loop - 2] = edge3;
            }
        }
    }
}