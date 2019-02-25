#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion

namespace SeeingSharp.Multimedia.Objects
{
    #region using

    using System;
    using Core;
    using SharpDX;

    #endregion

    public partial class VertexStructureSurface
    {
        #region Members for build-time transform
        private Vector2 m_tileSize = Vector2.Zero;
        #endregion

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
            bool result = false;

            for (int loop = 0; loop < m_indices.Count; loop += 3)
            {
                Vector3 vertex1 = m_owner.VerticesInternal[m_indices[loop]].Position;
                Vector3 vertex2 = m_owner.VerticesInternal[m_indices[loop + 1]].Position;
                Vector3 vertex3 = m_owner.VerticesInternal[m_indices[loop + 2]].Position;

                float currentDistance = 0f;
                if (pickingRay.Intersects(ref vertex1, ref vertex2, ref vertex3, out currentDistance))
                {
                    result = true;
                    if (currentDistance < distance) { distance = currentDistance; }
                }
            }

            return result;
        }

        /// <summary>
        /// Builds a column using 24 vertices.
        /// </summary>
        /// <param name="bottomMiddle">The bottom middle point.</param>
        /// <param name="size">Size on the ground.</param>
        /// <param name="height">Total height of the column.</param>
        public BuiltVerticesRange BuildColumn24V(Vector3 bottomMiddle, float size, float height)
        {
            int startVertex = m_owner.CountVertices;

            float halfSize = size / 2f;
            this.BuildCube24V(
                new Vector3(bottomMiddle.X - halfSize, bottomMiddle.Y, bottomMiddle.Z - halfSize),
                new Vector3(size, height, size));

            return new BuiltVerticesRange(m_owner, (int)startVertex, (int)(m_owner.CountVertices - startVertex));
        }

        public BuiltVerticesRange BuildHorizontalColumnX24V(Vector3 leftMiddle, float size, float width)
        {
            int startVertex = m_owner.CountVertices;

            float halfSize = size / 2f;
            this.BuildCube24V(
                new Vector3(leftMiddle.X, leftMiddle.Y - halfSize, leftMiddle.Z - halfSize),
                new Vector3(width, size, size));

            return new BuiltVerticesRange(m_owner, (int)startVertex, (int)(m_owner.CountVertices - startVertex));
        }

        public BuiltVerticesRange BuildHorizontalColumnZ24V(Vector3 frontMiddle, float size, float depth)
        {
            int startVertex = m_owner.CountVertices;

            float halfSize = size / 2f;
            this.BuildCube24V(
                new Vector3(frontMiddle.X - halfSize, frontMiddle.Y - halfSize, frontMiddle.Z),
                new Vector3(size, size, depth));

            return new BuiltVerticesRange(m_owner, (int)startVertex, (int)(m_owner.CountVertices - startVertex));
        }

        /// <summary>
        /// Builds a x-axis aligned rectangle.
        /// </summary>
        /// <param name="startPoint">Start point (left center).</param>
        /// <param name="destinationPoint">Destination point (right center).</param>
        /// <param name="size">The size of the rectangle.</param>
        public BuiltVerticesRange BuildXAxisAlignedRect(Vector3 startPoint, Vector3 destinationPoint, float size)
        {
            int startVertex = m_owner.CountVertices;

            float halfSize = size / 2f;
            this.BuildCube24V(
                new Vector3(startPoint.X, startPoint.Y - halfSize, startPoint.Z - halfSize),
                new Vector3(destinationPoint.X - startPoint.X, size, size));

            return new BuiltVerticesRange(m_owner, (int)startVertex, (int)(m_owner.CountVertices - startVertex));
        }

        /// <summary>
        /// Builds a z-axis aligned rectangle.
        /// </summary>
        /// <param name="startPoint">Start point (left center).</param>
        /// <param name="destinationPoint">Destination point (right center).</param>
        /// <param name="size">The size of the rectangle.</param>
        public BuiltVerticesRange BuildZAxisAlignedRect(Vector3 startPoint, Vector3 destinationPoint, float size)
        {
            int startVertex = m_owner.CountVertices;

            float halfSize = size / 2f;
            this.BuildCube24V(
                new Vector3(startPoint.X - halfSize, startPoint.Y - halfSize, startPoint.Z),
                new Vector3(size, size, destinationPoint.Z - startPoint.Z));

            return new BuiltVerticesRange(m_owner, (int)startVertex, (int)(m_owner.CountVertices - startVertex));
        }

        public BuiltVerticesRange BuildCircleFullV(Vector3 middle, float radius, float width, float height, int countOfSegments, Color4 Color)
        {
            int startVertex = m_owner.CountVertices;
            if (countOfSegments < 3) { throw new ArgumentException("Segment count of " + countOfSegments + " is too small!", "coundOfSegments"); }

            Matrix3x2 rotationMatrix;

            float halfWidth = width / 2f;
            float halfHeight = height / 2f;
            Vector2 nearVector = new Vector2(0f, radius - halfWidth);
            Vector2 farVector = new Vector2(0f, radius + halfWidth);
            Vector2 lastNearVector = nearVector;
            Vector2 lastFarVector = farVector;
            Vector2 nextNearVector;
            Vector2 nextFarVector;
            for (int loop = 1; loop <= countOfSegments; loop++)
            {
                float actPercent = (float)loop / (float)countOfSegments;
                float actAngle = EngineMath.RAD_360DEG * actPercent;

                // Calculate next points
                Matrix3x2.Rotation(actAngle, out rotationMatrix);
                Matrix3x2.TransformPoint(ref rotationMatrix, ref nearVector, out nextNearVector);
                Matrix3x2.TransformPoint(ref rotationMatrix, ref farVector, out nextFarVector);

                // Build current segment
                this.BuildRect4V(
                    middle + new Vector3(lastNearVector.X, halfHeight, lastNearVector.Y),
                    middle + new Vector3(lastFarVector.X, halfHeight, lastFarVector.Y),
                    middle + new Vector3(nextFarVector.X, halfHeight, nextFarVector.Y),
                    middle + new Vector3(nextNearVector.X, halfHeight, nextNearVector.Y),
                    Vector3.Up,
                    Color4Ex.Transparent);
                this.BuildRect4V(
                    middle + new Vector3(lastFarVector.X, halfHeight, lastFarVector.Y),
                    middle + new Vector3(lastFarVector.X, -halfHeight, lastFarVector.Y),
                    middle + new Vector3(nextFarVector.X, -halfHeight, nextFarVector.Y),
                    middle + new Vector3(nextFarVector.X, halfHeight, nextFarVector.Y),
                    Vector3.Normalize(new Vector3(lastFarVector.X, 0, lastFarVector.Y)),
                    Color4Ex.Transparent);
                this.BuildRect4V(
                    middle + new Vector3(lastFarVector.X, -halfHeight, lastFarVector.Y),
                    middle + new Vector3(lastNearVector.X, -halfHeight, lastNearVector.Y),
                    middle + new Vector3(nextNearVector.X, -halfHeight, nextNearVector.Y),
                    middle + new Vector3(nextFarVector.X, -halfHeight, nextFarVector.Y),
                    -Vector3.Up,
                    Color4Ex.Transparent);
                this.BuildRect4V(
                    middle + new Vector3(lastNearVector.X, halfHeight, lastNearVector.Y),
                    middle + new Vector3(nextNearVector.X, halfHeight, nextNearVector.Y),
                    middle + new Vector3(nextNearVector.X, -halfHeight, nextNearVector.Y),
                    middle + new Vector3(lastNearVector.X, -halfHeight, lastNearVector.Y),
                    Vector3.Normalize(new Vector3(lastFarVector.X, 0, lastFarVector.Y)),
                    Color4Ex.Transparent);

                lastNearVector = nextNearVector;
                lastFarVector = nextFarVector;
            }

            return new BuiltVerticesRange(m_owner, (int)startVertex, (int)(m_owner.CountVertices - startVertex));
        }

        /// <summary>
        /// Builds a cone into the structure with correct texture coordinates and normals.
        /// </summary>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cone.</param>
        /// <param name="height">The height of the cone.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        /// <param name="color">The color for the generated vertices.</param>
        public BuiltVerticesRange BuildConeFullV(Vector3 bottomMiddle, float radius, float height, int countOfSegments, Color4 color)
        {
            int startVertex = m_owner.CountVertices;

            if (countOfSegments < 5) { throw new ArgumentException("Segment count of " + countOfSegments + " is too small!", "coundOfSegments"); }
            float diameter = radius * 2f;

            //Get texture offsets
            float texX = 1f;
            float texY = 1f;
            if (m_tileSize != Vector2.Zero)
            {
                texX = diameter / m_tileSize.X;
                texY = diameter / m_tileSize.Y;
            }

            //Specify bottom and top middle coordinates
            Vector3 bottomCoordinate = bottomMiddle;
            Vector3 topCoordinate = new Vector3(bottomMiddle.X, bottomMiddle.Y + height, bottomMiddle.Z);

            //Create bottom and top vertices
            Vertex bottomVertex = new Vertex(bottomCoordinate, color, new Vector2(texX / 2f, texY / 2f), new Vector3(0f, -1f, 0f));

            //Add bottom and top vertices to the structure
            int bottomVertexIndex = m_owner.AddVertex(bottomVertex);

            //Generate all segments
            float fullRadian = EngineMath.RAD_360DEG;
            float countOfSegmentsF = (float)countOfSegments;
            for (int loop = 0; loop < countOfSegments; loop++)
            {
                //Calculate rotation values for each segment border
                float startRadian = fullRadian * ((float)loop / (float)countOfSegmentsF);
                float targetRadian = fullRadian * ((float)(loop + 1) / (float)countOfSegmentsF);
                float normalRadian = startRadian + (targetRadian - startRadian) / 2f;

                //Generate all normals
                Vector3 sideNormal = Vector3Ex.NormalFromHVRotation(normalRadian, 0f);
                Vector3 sideLeftNormal = Vector3Ex.NormalFromHVRotation(startRadian, 0f);
                Vector3 sideRightNormal = Vector3Ex.NormalFromHVRotation(targetRadian, 0f);

                //Calculate border texture coordinates
                Vector2 sideLeftTexCoord = new Vector2(0.5f + sideLeftNormal.X * radius, 0.5f + sideLeftNormal.Z * radius);
                Vector2 sideRightTexCoord = new Vector2(0.5f + sideRightNormal.X * radius, 0.5f + sideRightNormal.Z * radius);

                //Generate all points
                Vector3 sideLeftBottomCoord = bottomCoordinate + sideLeftNormal * radius;
                Vector3 sideRighBottomtCoord = bottomCoordinate + sideRightNormal * radius;
                Vector3 sideMiddleBottomCoord = bottomCoordinate + sideNormal * radius;

                //Add segment bottom triangle
                Vertex segmentBottomLeft = bottomVertex.Copy(sideLeftBottomCoord);
                Vertex segmentBottomRight = bottomVertex.Copy(sideRighBottomtCoord);
                AddTriangle(
                    bottomVertexIndex,
                    m_owner.AddVertex(segmentBottomLeft),
                    m_owner.AddVertex(segmentBottomRight));

                //Generate side normal
                Vector3 vectorToTop = topCoordinate - sideMiddleBottomCoord;
                Vector2 vectorToTopRotation = Vector3Ex.ToHVRotation(vectorToTop);
                vectorToTopRotation.Y = vectorToTopRotation.Y + EngineMath.RAD_90DEG;
                Vector3 topSideNormal = Vector3Ex.NormalFromHVRotation(vectorToTopRotation);

                //Add segment top triangle
                Vertex topVertex = new Vertex(topCoordinate, color, new Vector2(texX / 2f, texY / 2f), topSideNormal);
                Vertex segmentTopLeft = topVertex.Copy(sideLeftBottomCoord);
                Vertex segmentTopRight = topVertex.Copy(sideRighBottomtCoord);
                AddTriangle(
                    m_owner.AddVertex(topVertex),
                    m_owner.AddVertex(segmentTopRight),
                    m_owner.AddVertex(segmentTopLeft));
            }

            return new BuiltVerticesRange(m_owner, (int)startVertex, (int)(m_owner.CountVertices - startVertex));
        }

        /// <summary>
        /// Builds a cylinder into the structure with correct texture coordinates and normals.
        /// </summary>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        /// <param name="color">The color to be applied on the vertices.</param>
        public BuiltVerticesRange BuildCylinderFullV(Vector3 bottomMiddle, float radius, float height, int countOfSegments, Color4 color)
        {
            return BuildCylinderV(bottomMiddle, radius, height, countOfSegments, color, true, true, true);
        }

        /// <summary>
        /// Builds a cylinder into the structure with correct texture coordinates and normals.
        /// </summary>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        /// <param name="color">The color to be applied on the vertices.</param>
        public BuiltVerticesRange BuildCylinderTopV(Vector3 bottomMiddle, float radius, float height, int countOfSegments, Color4 color)
        {
            return BuildCylinderV(bottomMiddle, radius, height, countOfSegments, color, false, false, true);
        }

        /// <summary>
        /// Builds a cylinder into the structure with correct texture coordinates and normals.
        /// </summary>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        /// <param name="color">The color to be applied on the vertices.</param>
        public BuiltVerticesRange BuildCylinderSidesV(Vector3 bottomMiddle, float radius, float height, int countOfSegments, Color4 color)
        {
            return BuildCylinderV(bottomMiddle, radius, height, countOfSegments, color, true, false, false);
        }

        /// <summary>
        /// Builds a cylinder into the structure with correct texture coordinates and normals.
        /// </summary>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        /// <param name="color">The color to be applied on the vertices.</param>
        public BuiltVerticesRange BuildCylinderBottomV(Vector3 bottomMiddle, float radius, float height, int countOfSegments, Color4 color)
        {
            return BuildCylinderV(bottomMiddle, radius, height, countOfSegments, color, false, true, false);
        }

        /// <summary>
        /// Builds a cylinder into the structure with correct texture coordinates and normals.
        /// </summary>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        /// <param name="color">The color to be applied on the vertices.</param>
        /// <param name="buildBottom">Build bottom of the cylinder.</param>
        /// <param name="buildSides">Build sides of the cylinder.</param>
        /// <param name="buildTop">Build top side of the cylinder.</param>
        public BuiltVerticesRange BuildCylinderV(
            Vector3 bottomMiddle, float radius, float height, int countOfSegments, Color4 color,
            bool buildSides, bool buildBottom, bool buildTop)
        {
            int startVertex = m_owner.CountVertices;

            if (countOfSegments < 5) { throw new ArgumentException("Segment count of " + countOfSegments + " is too small!", "coundOfSegments"); }
            float diameter = radius * 2f;

            //Get texture offsets
            float texX = 1f;
            float texY = 1f;
            float texSegmentY = 1f;
            float texSegmentX = 1f;
            if (m_tileSize != Vector2.Zero)
            {
                texX = diameter / m_tileSize.X;
                texY = diameter / m_tileSize.Y;
                texSegmentY = height / m_tileSize.Y;
                texSegmentX = (EngineMath.RAD_180DEG * diameter) / m_tileSize.X;
            }

            //Specify bottom and top middle coordinates
            Vector3 bottomCoordinate = bottomMiddle;
            Vector3 topCoordinate = new Vector3(bottomMiddle.X, bottomMiddle.Y + height, bottomMiddle.Z);

            //Create bottom and top vertices
            Vertex bottomVertex = new Vertex(bottomCoordinate, color, new Vector2(texX / 2f, texY / 2f), new Vector3(0f, -1f, 0f));
            Vertex topVertex = new Vertex(topCoordinate, color, new Vector2(texX / 2f, texY / 2f), new Vector3(0f, 1f, 0f));

            //Add bottom and top vertices to the structure
            int bottomVertexIndex = m_owner.AddVertex(bottomVertex);
            int topVertexIndex = m_owner.AddVertex(topVertex);

            //Generate all segments
            float fullRadian = EngineMath.RAD_360DEG;
            float countOfSegmentsF = (float)countOfSegments;
            for (int loop = 0; loop < countOfSegments; loop++)
            {
                //Calculate rotation values for each segment border
                float startRadian = fullRadian * ((float)loop / (float)countOfSegmentsF);
                float targetRadian = fullRadian * ((float)(loop + 1) / (float)countOfSegmentsF);
                float normalRadian = startRadian + (targetRadian - startRadian) / 2f;

                //Generate all normals
                Vector3 sideNormal = Vector3Ex.NormalFromHVRotation(normalRadian, 0f);
                Vector3 sideLeftNormal = Vector3Ex.NormalFromHVRotation(startRadian, 0f);
                Vector3 sideRightNormal = Vector3Ex.NormalFromHVRotation(targetRadian, 0f);

                //
                Vector2 sideLeftTexCoord = new Vector2(0.5f + sideLeftNormal.X * radius, 0.5f + sideLeftNormal.Z * radius);
                Vector2 sideRightTexCoord = new Vector2(0.5f + sideRightNormal.X * radius, 0.5f + sideRightNormal.Z * radius);

                //Generate all points
                Vector3 sideLeftBottomCoord = bottomCoordinate + sideLeftNormal * radius;
                Vector3 sideRighBottomtCoord = bottomCoordinate + sideRightNormal * radius;
                Vector3 sideLeftTopCoord = new Vector3(sideLeftBottomCoord.X, sideLeftBottomCoord.Y + height, sideLeftBottomCoord.Z);
                Vector3 sideRightTopCoord = new Vector3(sideRighBottomtCoord.X, sideRighBottomtCoord.Y + height, sideRighBottomtCoord.Z);

                //Add segment bottom triangle
                if (buildBottom)
                {
                    Vertex segmentBottomLeft = bottomVertex.Copy(sideLeftBottomCoord, sideLeftTexCoord);
                    Vertex segmentBottomRight = bottomVertex.Copy(sideRighBottomtCoord, sideRightTexCoord);
                    AddTriangle(
                        bottomVertexIndex,
                        m_owner.AddVertex(segmentBottomLeft),
                        m_owner.AddVertex(segmentBottomRight));
                }

                //Add segment top triangle
                if (buildTop)
                {
                    Vertex segmentTopLeft = topVertex.Copy(sideLeftTopCoord, sideLeftTexCoord);
                    Vertex segmentTopRight = topVertex.Copy(sideRightTopCoord, sideRightTexCoord);
                    AddTriangle(
                        topVertexIndex,
                        m_owner.AddVertex(segmentTopRight),
                        m_owner.AddVertex(segmentTopLeft));
                }

                if (buildSides)
                {
                    //Calculate texture coords for side segment
                    Vector2 texCoordSegmentStart = new Vector2(texSegmentX * ((float)loop / (float)countOfSegments), 0f);
                    Vector2 texCoordSegmentTarget = new Vector2(texSegmentX * ((float)(loop + 1) / (float)countOfSegments), texSegmentY);

                    //Add segment side
                    BuildRect4V(sideLeftBottomCoord, sideRighBottomtCoord, sideRightTopCoord, sideLeftTopCoord, sideNormal, color, texCoordSegmentStart, texCoordSegmentTarget);
                }
            }

            return new BuiltVerticesRange(m_owner, (int)startVertex, (int)(m_owner.CountVertices - startVertex));
        }

        /// <summary>
        /// Builds a sphere geometry.
        /// </summary>
        public BuiltVerticesRange BuildShpere(int tDiv, int pDiv, double radius, Color4 color)
        {
            int startVertex = m_owner.CountVertices;

            double dt = (Math.PI * 2) / tDiv;
            double dp = Math.PI / pDiv;

            for (int pi = 0; pi <= pDiv; pi++)
            {
                double phi = pi * dp;

                for (int ti = 0; ti <= tDiv; ti++)
                {
                    // we want to start the mesh on the x axis
                    double theta = ti * dt;

                    Vector3 position = SphereGetPosition(theta, phi, radius);
                    Vertex vertex = new Vertex(
                        position,
                        color,
                        SphereGetTextureCoordinate(theta, phi),
                        Vector3.Normalize(position));
                    m_owner.Vertices.Add(vertex);
                }
            }

            for (int pi = 0; pi < pDiv; pi++)
            {
                for (int ti = 0; ti < tDiv; ti++)
                {
                    int x0 = ti;
                    int x1 = (ti + 1);
                    int y0 = pi * (tDiv + 1);
                    int y1 = (pi + 1) * (tDiv + 1);

                    this.Triangles.Add(
                        (int)(x0 + y0),
                        (int)(x0 + y1),
                        (int)(x1 + y0));

                    this.Triangles.Add(
                        (int)(x1 + y0),
                        (int)(x0 + y1),
                        (int)(x1 + y1));
                }
            }

            return new BuiltVerticesRange(m_owner, (int)startVertex, (int)(m_owner.CountVertices - startVertex));
        }

        /// <summary>
        /// Builds a cube into a vertex structure (this cube is built up of just 8 vertices, so not texturing is supported)
        /// </summary>
        /// <param name="start">Start point of the cube (left-lower-front point)</param>
        /// <param name="size">Size of the cube</param>
        public BuiltVerticesRange BuildCube8V(Vector3 start, Vector3 size)
        {
            return BuildCube8V(start, size, Color4.White);
        }

        /// <summary>
        /// Builds a cube into a vertex structure (this cube is built up of just 8 vertices, so no texturing is supported)
        /// </summary>
        /// <param name="start">Start point of the cube (left-lower-front point)</param>
        /// <param name="size">Size of the cube</param>
        /// <param name="color">Color of the cube</param>
        public BuiltVerticesRange BuildCube8V(Vector3 start, Vector3 size, Color4 color)
        {
            int startVertex = m_owner.CountVertices;

            Vector3 dest = start + size;
            Vertex vertex = new Vertex(start, color, new Vector2());

            int a = m_owner.AddVertex(vertex);
            int b = m_owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, start.Z)));
            int c = m_owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, dest.Z)));
            int d = m_owner.AddVertex(vertex.Copy(new Vector3(start.X, start.Y, dest.Z)));
            int e = m_owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, start.Z)));
            int f = m_owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, start.Z)));
            int g = m_owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, dest.Z)));
            int h = m_owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, dest.Z)));

            this.AddTriangle(a, e, f);  //front side
            this.AddTriangle(f, b, a);
            this.AddTriangle(b, f, g);  //right side
            this.AddTriangle(g, c, b);
            this.AddTriangle(c, g, h);  //back side
            this.AddTriangle(h, d, c);
            this.AddTriangle(d, h, e);  //left side
            this.AddTriangle(e, a, d);
            this.AddTriangle(e, h, g);  //top side
            this.AddTriangle(g, f, e);
            this.AddTriangle(a, b, c);  //botton side
            this.AddTriangle(c, d, a);

            return new BuiltVerticesRange(m_owner, (int)startVertex, (int)(m_owner.CountVertices - startVertex));
        }

        /// <summary>
        /// Builds a cube into this VertexStructure (this cube is built up of 24 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="start">Start point of the cube</param>
        /// <param name="size">Size of the cube</param>
        public BuiltVerticesRange BuildCube24V(Vector3 start, Vector3 size)
        {
            return BuildCube24V(start, size, Color4.White);
        }

        /// <summary>
        /// Builds a cube into this VertexStructure (this cube is built up of 24 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="start">Start point of the cube</param>
        /// <param name="size">Size of the cube</param>
        /// <param name="color">Color of the cube</param>
        public BuiltVerticesRange BuildCube24V(Vector3 start, Vector3 size, Color4 color)
        {
            BuiltVerticesRange result = new BuiltVerticesRange(m_owner);

            result.Merge(this.BuildCubeSides16V(start, size, color));
            result.Merge(this.BuildCubeTop4V(start, size, color));
            result.Merge(this.BuildCubeBottom4V(start, size, color));

            return result;
        }

        /// <summary>
        /// Builds a cube of 4 vertices and a defined hight.
        /// </summary>
        /// <param name="topA"></param>
        /// <param name="topB"></param>
        /// <param name="topC"></param>
        /// <param name="topD"></param>
        /// <param name="heigh"></param>
        /// <param name="color"></param>
        public BuiltVerticesRange BuildCube24V(Vector3 topA, Vector3 topB, Vector3 topC, Vector3 topD, float heigh, Color4 color)
        {
            BuiltVerticesRange result = new BuiltVerticesRange(m_owner);
            result.StartVertex = m_owner.CountVertices;
            int startTriangleIndex = this.CountTriangles;

            // Calculate texture coordinates
            Vector3 size = new Vector3(
                (topB - topA).Length(),
                Math.Abs(heigh),
                (topC - topB).Length());
            float texX = 1f;
            float texY = 1f;
            float texZ = 1f;
            if (m_tileSize != Vector2.Zero)
            {
                texX = size.X / m_tileSize.X;
                texY = size.Y / m_tileSize.Y;
                texZ = size.Z / m_tileSize.X;
            }

            // Calculate bottom vectors
            Vector3 bottomA = new Vector3(topA.X, topA.Y - heigh, topA.Z);
            Vector3 bottomB = new Vector3(topB.X, topB.Y - heigh, topB.Z);
            Vector3 bottomC = new Vector3(topC.X, topC.Y - heigh, topC.Z);
            Vector3 bottomD = new Vector3(topD.X, topD.Y - heigh, topD.Z);

            // Build Top side
            Vertex vertex = new Vertex(topA, color, new Vector2(texX, 0f), new Vector3(0f, 1f, 0f));
            int a = m_owner.AddVertex(vertex);
            int b = m_owner.AddVertex(vertex.Copy(topB, new Vector2(texX, texY)));
            int c = m_owner.AddVertex(vertex.Copy(topC, new Vector2(0f, texY)));
            int d = m_owner.AddVertex(vertex.Copy(topD, new Vector2(0f, 0f)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            // Build Bottom side
            vertex = new Vertex(topA, color, new Vector2(0f, 0f), new Vector3(0f, -1f, 0f));
            a = m_owner.AddVertex(vertex);
            b = m_owner.AddVertex(vertex.Copy(topD, new Vector2(texX, 0f)));
            c = m_owner.AddVertex(vertex.Copy(topC, new Vector2(texX, texY)));
            d = m_owner.AddVertex(vertex.Copy(topB, new Vector2(0f, texY)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            // Build Front side
            vertex = new Vertex(topA, color, new Vector2(0f, texY), new Vector3(0f, 0f, -1f));
            a = m_owner.AddVertex(vertex);
            b = m_owner.AddVertex(vertex.Copy(topB, new Vector2(texX, texY)));
            c = m_owner.AddVertex(vertex.Copy(bottomB, new Vector2(texX, 0f)));
            d = m_owner.AddVertex(vertex.Copy(bottomA, new Vector2(0f, 0f)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            // Build Right side
            a = m_owner.AddVertex(vertex.Copy(topB, new Vector3(1f, 0f, 0f), new Vector2(0f, texY)));
            b = m_owner.AddVertex(vertex.Copy(topC, new Vector3(1f, 0f, 0f), new Vector2(texZ, texY)));
            c = m_owner.AddVertex(vertex.Copy(bottomC, new Vector3(1f, 0f, 0f), new Vector2(texZ, 0f)));
            d = m_owner.AddVertex(vertex.Copy(bottomB, new Vector3(1f, 0f, 0f), new Vector2(0f, 0f)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            // Build Back side
            a = m_owner.AddVertex(vertex.Copy(topC, new Vector3(0f, 0f, 1f), new Vector2(0f, texY)));
            b = m_owner.AddVertex(vertex.Copy(topD, new Vector3(0f, 0f, 1f), new Vector2(texX, texY)));
            c = m_owner.AddVertex(vertex.Copy(bottomD, new Vector3(0f, 0f, 1f), new Vector2(texX, 0f)));
            d = m_owner.AddVertex(vertex.Copy(bottomC, new Vector3(0f, 0f, 1f), new Vector2(0f, 0f)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            // Build Left side
            a = m_owner.AddVertex(vertex.Copy(topD, new Vector3(-1f, 0f, 0f), new Vector2(0f, texY)));
            b = m_owner.AddVertex(vertex.Copy(topA, new Vector3(-1f, 0f, 0f), new Vector2(texZ, texY)));
            c = m_owner.AddVertex(vertex.Copy(bottomA, new Vector3(-1f, 0f, 0f), new Vector2(texZ, 0f)));
            d = m_owner.AddVertex(vertex.Copy(bottomD, new Vector3(-1f, 0f, 0f), new Vector2(0f, 0f)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            // Calculate normals finally
            this.CalculateNormalsFlat(startTriangleIndex, this.CountTriangles - startTriangleIndex);

            result.VertexCount = (int)(m_owner.CountVertices - result.StartVertex);
            return result;
        }

        /// <summary>
        /// Builds a cube into this VertexStructure (this cube is built up of 24 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="box">Box defining bounds of generated cube.</param>
        /// <param name="color">Color of generated vertices.</param>
        public BuiltVerticesRange BuildCube24V(BoundingBox box, Color4 color)
        {
            return BuildCube24V(box.Minimum, box.Size, color);
        }

        /// <summary>
        /// Builds a cube into this VertexStructure (this cube is built up of 24 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="box">Box defining bounds of generated cube.</param>
        public BuiltVerticesRange BuildCube24V(BoundingBox box)
        {
            return BuildCube24V(box, Color4.White);
        }

        /// <summary>
        /// Builds a cube on the given point with the given color.
        /// </summary>
        /// <param name="centerLocation">The location to draw the cube at.</param>
        /// <param name="sideLength">The side length of the cube.</param>
        /// <param name="color">The color to be used.</param>
        public BuiltVerticesRange BuildCube24V(Vector3 centerLocation, float sideLength, Color4 color)
        {
            return BuildCube24V(
                centerLocation - new Vector3(sideLength / 2f, sideLength / 2f, sideLength / 2f),
                new Vector3(sideLength, sideLength, sideLength),
                color);
        }

        /// <summary>
        /// Builds a cube into this VertexStructure (this cube is built up of 24 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="bottomCenter">Bottom center point of the cube.</param>
        /// <param name="width">Width (and depth) of the cube.</param>
        /// <param name="height">Height of the cube.</param>
        /// <param name="color">Color of the cube</param>
        public BuiltVerticesRange BuildCube24V(Vector3 bottomCenter, float width, float height, Color4 color)
        {
            Vector3 start = new Vector3(
                bottomCenter.X - width / 2f,
                bottomCenter.Y,
                bottomCenter.Z - width / 2f);
            Vector3 size = new Vector3(width, height, width);
            return BuildCube24V(start, size, color);
        }

        /// <summary>
        /// Build a single rectangle into the vertex structure (Supports texturing and normal vectors)
        /// </summary>
        public BuiltVerticesRange BuildRect4V(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, Vector3 normal, TextureCoordinateCalculationAlignment uCoordAlignment, TextureCoordinateCalculationAlignment vCoordAlignment, float coordRepeatUnit)
        {
            int startVertex = m_owner.CountVertices;

            //Define texture coordinate calculation functions
            Func<Vector3, float> caluclateU = (actPosition) =>
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
            };
            Func<Vector3, float> calculateV = (actPosition) =>
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
            };

            Vector2 textureCoordinate = new Vector2(caluclateU(pointA), calculateV(pointA));
            Vertex vertex = new Vertex(pointA, Color4.White, textureCoordinate, normal);

            int a = m_owner.AddVertex(vertex);
            int b = m_owner.AddVertex(vertex.Copy(pointB, new Vector2(caluclateU(pointB), calculateV(pointB))));
            int c = m_owner.AddVertex(vertex.Copy(pointC, new Vector2(caluclateU(pointC), calculateV(pointC))));
            int d = m_owner.AddVertex(vertex.Copy(pointD, new Vector2(caluclateU(pointD), calculateV(pointD))));

            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            return new BuiltVerticesRange(m_owner, (int)startVertex, (int)(m_owner.CountVertices - startVertex));
        }

        /// <summary>
        /// Builds the top side of a cube into this VertexStructure (Built up of 4 vertices, so texture coordinates and normals are set)
        /// </summary>
        public BuiltVerticesRange BuildCubeTop4V(Vector3 start, Vector3 size, Color4 color)
        {
            Vector3 dest = start + size;

            return this.BuildRect4V(
                new Vector3(start.X, dest.Y, start.Z),
                new Vector3(dest.X, dest.Y, start.Z),
                new Vector3(dest.X, dest.Y, dest.Z),
                new Vector3(start.X, dest.Y, dest.Z),
                new Vector3(0f, 1f, 0f),
                color);
        }

        /// <summary>
        /// Builds the bottom side of a cube into this VertexStructure (Built up of 4 vertices, so texture coordinates and normals are set)
        /// </summary>
        public BuiltVerticesRange BuildCubeBottom4V(Vector3 start, Vector3 size, Color4 color)
        {
            Vector3 dest = start + size;

            return this.BuildRect4V(
                new Vector3(start.X, start.Y, dest.Z),
                new Vector3(dest.X, start.Y, dest.Z),
                new Vector3(dest.X, start.Y, start.Z),
                new Vector3(start.X, start.Y, start.Z),
                new Vector3(0f, -1f, 0f),
                color);
        }

        /// <summary>
        /// Builds cube sides into this VertexStructure (these sides are built up of  16 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="start">Start poiint of the cube</param>
        /// <param name="size">Size of the cube</param>
        /// <param name="color">Color of the cube</param>
        public BuiltVerticesRange BuildCubeSides16V(Vector3 start, Vector3 size, Color4 color)
        {
            BuiltVerticesRange result = new BuiltVerticesRange(m_owner);
            result.StartVertex = m_owner.CountVertices;

            Vector3 dest = start + size;

            float texX = 1f;
            float texY = 1f;
            float texZ = 1f;
            if (m_tileSize != Vector2.Zero)
            {
                texX = size.X / m_tileSize.X;
                texY = size.Y / m_tileSize.Y;
                texZ = size.Z / m_tileSize.X;
            }

            //Front side
            Vertex vertex = new Vertex(start, color, new Vector2(0f, texY), new Vector3(0f, 0f, -1f));
            int a = m_owner.AddVertex(vertex);
            int b = m_owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, start.Z), new Vector2(texX, texY)));
            int c = m_owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, start.Z), new Vector2(texX, 0f)));
            int d = m_owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, start.Z), new Vector2(0f, 0f)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            //Right side
            a = m_owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, start.Z), new Vector3(1f, 0f, 0f), new Vector2(0f, texY)));
            b = m_owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, dest.Z), new Vector3(1f, 0f, 0f), new Vector2(texZ, texY)));
            c = m_owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, dest.Z), new Vector3(1f, 0f, 0f), new Vector2(texZ, 0f)));
            d = m_owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, start.Z), new Vector3(1f, 0f, 0f), new Vector2(0f, 0f)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            //Back side
            a = m_owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, dest.Z), new Vector3(0f, 0f, 1f), new Vector2(0f, texY)));
            b = m_owner.AddVertex(vertex.Copy(new Vector3(start.X, start.Y, dest.Z), new Vector3(0f, 0f, 1f), new Vector2(texX, texY)));
            c = m_owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, dest.Z), new Vector3(0f, 0f, 1f), new Vector2(texX, 0f)));
            d = m_owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, dest.Z), new Vector3(0f, 0f, 1f), new Vector2(0f, 0f)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            //Left side
            a = m_owner.AddVertex(vertex.Copy(new Vector3(start.X, start.Y, dest.Z), new Vector3(-1f, 0f, 0f), new Vector2(0f, texY)));
            b = m_owner.AddVertex(vertex.Copy(new Vector3(start.X, start.Y, start.Z), new Vector3(-1f, 0f, 0f), new Vector2(texZ, texY)));
            c = m_owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, start.Z), new Vector3(-1f, 0f, 0f), new Vector2(texZ, 0f)));
            d = m_owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, dest.Z), new Vector3(-1f, 0f, 0f), new Vector2(0f, 0f)));
            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            result.VertexCount = (int)(m_owner.CountVertices - result.StartVertex);
            return result;
        }

        /// <summary>
        /// Build a single rectangle into the vertex structure
        /// </summary>
        public BuiltVerticesRange BuildRect4V(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD)
        {
            return BuildRect4V(pointA, pointB, pointC, pointD, Color4.White);
        }

        /// <summary>
        /// Build a single rectangle into the vertex structure (Supports texturing)
        /// </summary>
        public BuiltVerticesRange BuildRect4V(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, Color4 color)
        {
            float texX = 1f;
            float texY = 1f;
            if (m_tileSize != Vector2.Zero)
            {
                texX = (pointB - pointA).Length() / m_tileSize.X;
                texY = (pointC - pointB).Length() / m_tileSize.Y;
            }

            Vertex vertex = new Vertex(pointA, color, new Vector2(0f, texY));

            int a = m_owner.AddVertex(vertex);
            int b = m_owner.AddVertex(vertex.Copy(pointB, new Vector2(texX, texY)));
            int c = m_owner.AddVertex(vertex.Copy(pointC, new Vector2(texX, 0f)));
            int d = m_owner.AddVertex(vertex.Copy(pointD, new Vector2(0f, 0f)));

            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            return new BuiltVerticesRange(m_owner, (int)(m_owner.CountVertices - 4), 4);
        }

        /// <summary>
        /// Build a single rectangle into the vertex structure (Supports texturing and normal vectors)
        /// </summary>
        public BuiltVerticesRange BuildRect4V(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, Vector3 normal)
        {
            return BuildRect4V(pointA, pointB, pointC, pointD, normal, Color4.White);
        }

        /// <summary>
        /// Build a single rectangle into the vertex structure (Supports texturing and normal vectors)
        /// </summary>
        public BuiltVerticesRange BuildRect4V(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, Vector3 normal, Color4 color)
        {
            float texX = 1f;
            float texY = 1f;
            if (m_tileSize != Vector2.Zero)
            {
                texX = (pointB - pointA).Length() / m_tileSize.X;
                texY = (pointC - pointB).Length() / m_tileSize.Y;
            }

            Vertex vertex = new Vertex(pointA, color, new Vector2(0f, texY), normal);

            int a = m_owner.AddVertex(vertex);
            int b = m_owner.AddVertex(vertex.Copy(pointB, new Vector2(texX, texY)));
            int c = m_owner.AddVertex(vertex.Copy(pointC, new Vector2(texX, 0f)));
            int d = m_owner.AddVertex(vertex.Copy(pointD, new Vector2(0f, 0f)));

            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            return new BuiltVerticesRange(m_owner, (int)(m_owner.CountVertices - 4), 4);
        }

        /// <summary>
        /// Build a single rectangle into the vertex structure (Supports texturing and normal vectors)
        /// </summary>
        public BuiltVerticesRange BuildRect4V(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, Vector3 normal, Color4 color, Vector2 minTexCoord, Vector2 maxTexCoord)
        {
            Vertex vertex = new Vertex(pointA, color, new Vector2(minTexCoord.X, maxTexCoord.Y), normal);

            int a = m_owner.AddVertex(vertex);
            int b = m_owner.AddVertex(vertex.Copy(pointB, new Vector2(maxTexCoord.X, maxTexCoord.Y)));
            int c = m_owner.AddVertex(vertex.Copy(pointC, new Vector2(maxTexCoord.X, minTexCoord.Y)));
            int d = m_owner.AddVertex(vertex.Copy(pointD, new Vector2(minTexCoord.X, minTexCoord.Y)));

            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            return new BuiltVerticesRange(m_owner, (int)(m_owner.CountVertices - 4), 4);
        }

        /// <summary>
        /// Changes the index order of each triangle.
        /// </summary>
        public void ToggleTriangleIndexOrder()
        {
            for (int loop = 2; loop < m_indices.Count; loop += 3)
            {
                int index1 = m_indices[loop - 2];
                int index2 = m_indices[loop - 1];
                int index3 = m_indices[loop];
                m_indices[loop] = index1;
                m_indices[loop - 1] = index2;
                m_indices[loop - 2] = index3;
            }
        }

        /// <summary>
        /// Helper method for spehere creation.
        /// </summary>
        private Vector3 SphereGetPosition(double theta, double phi, double radius)
        {
            double x = radius * Math.Sin(theta) * Math.Sin(phi);
            double y = radius * Math.Cos(phi);
            double z = radius * Math.Cos(theta) * Math.Sin(phi);

            return new Vector3((float)x, (float)y, (float)z);
        }

        /// <summary>
        /// Helper method for spehere creation.
        /// </summary>
        private Vector2 SphereGetTextureCoordinate(double theta, double phi)
        {
            return new Vector2(
                (float)(theta / (2 * Math.PI)),
                (float)(phi / Math.PI));
        }
    }
}