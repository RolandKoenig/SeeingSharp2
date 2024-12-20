﻿using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using SeeingSharp.Checking;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.Drawing3D.Geometries
{
    /// <summary>
    /// A set of triangles of a Geometry which share the
    /// same material settings.
    /// </summary>
    public partial class GeometrySurface
    {
        private UnsafeList<TriangleCorner> _corners;

        /// <summary>
        /// Retrieves a collection of triangles
        /// </summary>
        public TriangleCollection Triangles { get; }

        /// <summary>
        /// Gets a collection containing all indexes.
        /// </summary>
        public IndexCollection Indices { get; }

        /// <summary>
        /// Gets a collection containing all corners.
        /// </summary>
        public CornerCollection Corners { get; }

        /// <summary>
        /// Retrieves total count of all triangles within this geometry
        /// </summary>
        public int CountTriangles => _corners.Count / 3;

        /// <summary>
        /// Gets the owner of this surface.
        /// </summary>
        public Geometry Owner { get; }

        /// <summary>
        /// Retrieves total count of all indexes within this geometry
        /// </summary>
        internal int CountIndices => _corners.Count;

        internal GeometrySurface(Geometry owner, int triangleCapacity)
        {
            this.Owner = owner;
            _corners = new UnsafeList<TriangleCorner>(triangleCapacity * 3);

            this.Indices = new IndexCollection(this);
            this.Corners = new CornerCollection(this);
            this.Triangles = new TriangleCollection(this);
        }

        /// <summary>
        /// Clones this object.
        /// </summary>
        public GeometrySurface Clone(
            Geometry newOwner,
            bool copyGeometryData = true, int capacityMultiplier = 1,
            int baseIndex = 0)
        {
            newOwner.EnsureNotNull(nameof(newOwner));

            // Create new Geometry object
            var indexCount = _corners.Count;
            var result = new GeometrySurface(newOwner, indexCount / 3 * capacityMultiplier);

            // Copy geometry
            if (copyGeometryData)
            {
                for (var loop = 0; loop < indexCount; loop++)
                {
                    var cornerToAdd = _corners[loop];
                    cornerToAdd.Index = cornerToAdd.Index + baseIndex;
                    result._corners.Add(cornerToAdd);
                }
            }

            return result;
        }

        /// <summary>
        /// Adds all vertices and surfaces of the given geometry to this one.
        /// All surfaces of the given geometry are merged to this single surface.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        public void AddGeometry(Geometry geometry)
        {
            var baseIndex = this.Owner.CountVertices;

            // AddObject all vertices to local geometry
            this.Owner.AddVertices(geometry);

            // AddObject all corners to local surface
            foreach (var actSurface in geometry.Surfaces)
            {
                var corners = actSurface._corners;
                var cornerCount = corners.Count;

                for (var loop = 0; loop < cornerCount; loop++)
                {
                    var cornerToAdd = corners[loop];
                    cornerToAdd.Index = cornerToAdd.Index + baseIndex;
                    _corners.Add(cornerToAdd);
                }
            }
        }

        /// <summary>
        /// Subdivides all triangles of this surface.
        /// </summary>
        public void Subdivide()
        {
            this.Subdivide(0, this.CountTriangles);
        }

        /// <summary>
        /// Subdivides the given collection of triangles of this surface.
        /// </summary>
        /// <param name="startTriangleIndex">The triangle where to start.</param>
        public void Subdivide(int startTriangleIndex)
        {
            this.Subdivide(startTriangleIndex, this.CountTriangles - startTriangleIndex);
        }

        /// <summary>
        /// Subdivides the given collection of triangles of this surface.
        /// </summary>
        /// <param name="startTriangleIndex">The index of the triangle to start with.</param>
        /// <param name="countTriangles">The total count of triangles to subdivide.</param>
        public void Subdivide(int startTriangleIndex, int countTriangles)
        {
            var fullTriangleCount = this.CountTriangles;
            startTriangleIndex.EnsureInRange(0, fullTriangleCount - 1, nameof(startTriangleIndex));
            countTriangles.EnsureInRange(1, this.CountTriangles - startTriangleIndex, nameof(countTriangles));

            var endingTriangle = startTriangleIndex + countTriangles;
            for (var actTriangleIndex = startTriangleIndex; actTriangleIndex < endingTriangle; actTriangleIndex++)
            {
                // Subdivide the triangle like this:
                //       v1                   v1
                //       *                    *
                //      / \                  / \
                //     /   \                /   \
                //    /     \    -->     m0*-----*m1
                //   /       \            / \   / \
                //  /         \          /   \ /   \
                // *-----------*        *-----*-----*
                // v0    m2     v2      v0    m2     v2  

                // Get all three vertices of the current triangle
                var i0 = actTriangleIndex * 3;
                var i1 = i0 + 1;
                var i2 = i0 + 2;
                var vertexIndex0 = _corners[i0].Index;
                var vertexIndex1 = _corners[i1].Index;
                var vertexIndex2 = _corners[i2].Index;
                ref var v0 = ref this.Owner.GetVertexBasicRef(vertexIndex0);
                ref var v1 = ref this.Owner.GetVertexBasicRef(vertexIndex1);
                ref var v2 = ref this.Owner.GetVertexBasicRef(vertexIndex2);

                // Generate new vertices
                VertexBasic.SubdivideVertices(ref v0, ref v1, out var m0);
                VertexBasic.SubdivideVertices(ref v1, ref v2, out var m1);
                VertexBasic.SubdivideVertices(ref v2, ref v0, out var m2);

                // Add those newly generated vertices
                var vertexIndex3 = this.Owner.AddVertex(m0);
                var vertexIndex4 = this.Owner.AddVertex(m1);
                var vertexIndex5 = this.Owner.AddVertex(m2);

                // Now add new triangles
                _corners[i0] = new TriangleCorner(vertexIndex0);
                _corners[i1] = new TriangleCorner(vertexIndex3);
                _corners[i2] = new TriangleCorner(vertexIndex5);

                _corners.Add(new TriangleCorner(vertexIndex3));
                _corners.Add(new TriangleCorner(vertexIndex4));
                _corners.Add(new TriangleCorner(vertexIndex5));

                _corners.Add(new TriangleCorner(vertexIndex5));
                _corners.Add(new TriangleCorner(vertexIndex4));
                _corners.Add(new TriangleCorner(vertexIndex2));

                _corners.Add(new TriangleCorner(vertexIndex3));
                _corners.Add(new TriangleCorner(vertexIndex1));
                _corners.Add(new TriangleCorner(vertexIndex4));
            }
        }

        /// <summary>
        /// Adds a triangle fan.
        /// </summary>
        /// <param name="vertexIndices">The vertex indices of the triangle fan.</param>
        public void AddTriangleFan(IList<int> vertexIndices)
        {
            for (var loop = 0; loop + 2 < vertexIndices.Count; loop++)
            {
                this.AddTriangle(
                    vertexIndices[0],
                    vertexIndices[loop + 1],
                    vertexIndices[loop + 2]);
            }
        }

        /// <summary>
        /// Adds a triangle
        /// </summary>
        /// <param name="index1">Index of the first vertex</param>
        /// <param name="index2">Index of the second vertex</param>
        /// <param name="index3">Index of the third vertex</param>
        public void AddTriangle(int index1, int index2, int index3)
        {
            _corners.Add(new TriangleCorner(index1));
            _corners.Add(new TriangleCorner(index2));
            _corners.Add(new TriangleCorner(index3));
        }

        /// <summary>
        /// Adds a triangle
        /// </summary>
        /// <param name="v1">First vertex</param>
        /// <param name="v2">Second vertex</param>
        /// <param name="v3">Third vertex</param>
        public void AddTriangle(VertexBasic v1, VertexBasic v2, VertexBasic v3)
        {
            _corners.Add(new TriangleCorner(this.Owner.AddVertex(v1)));
            _corners.Add(new TriangleCorner(this.Owner.AddVertex(v2)));
            _corners.Add(new TriangleCorner(this.Owner.AddVertex(v3)));
        }

        /// <summary>
        /// Adds a triangle and calculates normals for it.
        /// </summary>
        /// <param name="index1">Index of the first vertex</param>
        /// <param name="index2">Index of the second vertex</param>
        /// <param name="index3">Index of the third vertex</param>
        public void AddTriangleAndCalculateNormalsFlat(int index1, int index2, int index3)
        {
            this.AddTriangle(index1, index2, index3);
            this.CalculateNormalsFlat(new Triangle(index1, index2, index3));
        }

        /// <summary>
        /// Adds a triangle
        /// </summary>
        /// <param name="v1">First vertex</param>
        /// <param name="v2">Second vertex</param>
        /// <param name="v3">Third vertex</param>
        public void AddTriangleAndCalculateNormalsFlat(VertexBasic v1, VertexBasic v2, VertexBasic v3)
        {
            var index1 = this.Owner.AddVertex(v1);
            var index2 = this.Owner.AddVertex(v2);
            var index3 = this.Owner.AddVertex(v3);

            this.AddTriangleAndCalculateNormalsFlat(index1, index2, index3);
        }

        /// <summary>
        /// Adds the given polygon using the cutting ears algorithm for triangulation.
        /// </summary>
        /// <param name="vertices">The vertices to add.</param>
        public void AddPolygonByCuttingEars(IEnumerable<VertexBasic> vertices)
        {
            //AddObject vertices first
            var indices = new List<int>();

            foreach (var actVertex in vertices)
            {
                indices.Add(this.Owner.AddVertex(actVertex));
            }

            //Calculate cutting ears
            this.AddPolygonByCuttingEars(indices);
        }

        /// <summary>
        /// Adds the given polygon using the cutting ears algorithm for triangulation.
        /// </summary>
        /// <param name="indices">The indices of the polygon's corners.</param>
        /// <param name="twoSided">The indexes for front- and backside?</param>
        public void AddPolygonByCuttingEars(IEnumerable<int> indices, bool twoSided = false)
        {
            this.AddPolygonByCuttingEarsInternal(new List<int>(indices), twoSided);
        }

        /// <summary>
        /// Adds the given polygon using the cutting ears algorithm for triangulation.
        /// </summary>
        /// <param name="vertices">The vertices to add.</param>
        /// <param name = "twoSided" > The indexes for front- and backside?</param>
        public void AddPolygonByCuttingEarsAndCalculateNormals(IEnumerable<VertexBasic> vertices, bool twoSided = false)
        {
            //AddObject vertices first
            var indices = new List<int>();

            foreach (var actVertex in vertices)
            {
                indices.Add(this.Owner.AddVertex(actVertex));
            }

            //Calculate cutting ears and normals
            this.AddPolygonByCuttingEarsAndCalculateNormals(indices, twoSided);
        }

        /// <summary>
        /// Adds the given polygon using the cutting ears algorithm for triangulation.
        /// </summary>
        /// <param name="indices">The indices of the polygon's corners.</param>
        /// <param name="twoSided">The indexes for front- and backside?</param>
        public void AddPolygonByCuttingEarsAndCalculateNormals(IEnumerable<int> indices, bool twoSided)
        {
            //AddObject the triangles using cutting ears algorithm
            var addedIndices = this.AddPolygonByCuttingEarsInternal(new List<int>(indices), twoSided);

            //Calculate all normals
            var indexEnumerator = addedIndices.GetEnumerator();
            try
            {
                while (indexEnumerator.MoveNext())
                {
                    var index1 = indexEnumerator.Current;
                    var index2 = 0;
                    var index3 = 0;

                    if (indexEnumerator.MoveNext())
                    {
                        index2 = indexEnumerator.Current;
                    }
                    else
                    {
                        break;
                    }

                    if (indexEnumerator.MoveNext())
                    {
                        index3 = indexEnumerator.Current;
                    }
                    else
                    {
                        break;
                    }

                    this.CalculateNormalsFlat(new Triangle(index1, index2, index3));
                }
            }
            finally
            {
                indexEnumerator.Dispose();
            }
        }

        /// <summary>
        /// Builds a line list containing a line for each face binormal.
        /// </summary>
        public List<Vector3> BuildLineListForFaceBinormals()
        {
            var result = new List<Vector3>();

            //Generate all lines
            foreach (var actTriangle in this.Triangles)
            {
                //Get all vertices of current face
                ref var vertex1 = ref this.Owner.GetVertexBasicRef(actTriangle.Index1);
                ref var vertex2 = ref this.Owner.GetVertexBasicRef(actTriangle.Index2);
                ref var vertex3 = ref this.Owner.GetVertexBasicRef(actTriangle.Index3);
                ref var vertexEx1 = ref this.Owner.GetVertexBinormalTangentRef(actTriangle.Index1);
                ref var vertexEx2 = ref this.Owner.GetVertexBinormalTangentRef(actTriangle.Index2);
                ref var vertexEx3 = ref this.Owner.GetVertexBinormalTangentRef(actTriangle.Index3);

                //Get average values for current face
                var averageBinormal = Vector3.Normalize(Vector3Ex.Average(vertexEx1.Binormal, vertexEx2.Binormal, vertexEx3.Binormal));
                var averagePosition = Vector3Ex.Average(vertex1.Position, vertex2.Position, vertex3.Position);
                averageBinormal *= 0.2f;

                //Generate a line
                if (averageBinormal.Length() > 0.1f)
                {
                    result.Add(averagePosition);
                    result.Add(averagePosition + averageBinormal);
                }
            }

            return result;
        }

        /// <summary>
        /// Builds a line list containing a line for each face normal.
        /// </summary>
        public List<Vector3> BuildLineListForFaceNormals()
        {
            var result = new List<Vector3>();

            //Generate all lines
            foreach (var actTriangle in this.Triangles)
            {
                //Get all vertices of current face
                ref var vertex1 = ref this.Owner.GetVertexBasicRef(actTriangle.Index1);
                ref var vertex2 = ref this.Owner.GetVertexBasicRef(actTriangle.Index2);
                ref var vertex3 = ref this.Owner.GetVertexBasicRef(actTriangle.Index3);

                //Get average values for current face
                var averageNormal = Vector3.Normalize(Vector3Ex.Average(vertex1.Normal, vertex2.Normal, vertex3.Normal));
                var averagePosition = Vector3Ex.Average(vertex1.Position, vertex2.Position, vertex3.Position);
                averageNormal *= 0.2f;

                //Generate a line
                if (averageNormal.Length() > 0.1f)
                {
                    result.Add(averagePosition);
                    result.Add(averagePosition + averageNormal);
                }
            }

            return result;
        }

        /// <summary>
        /// Builds a line list containing a line for each face tangent.
        /// </summary>
        public List<Vector3> BuildLineListForFaceTangents()
        {
            var result = new List<Vector3>();

            //Generate all lines
            foreach (var actTriangle in this.Triangles)
            {
                //Get all vertices of current face
                ref var vertex1 = ref this.Owner.GetVertexBasicRef(actTriangle.Index1);
                ref var vertex2 = ref this.Owner.GetVertexBasicRef(actTriangle.Index2);
                ref var vertex3 = ref this.Owner.GetVertexBasicRef(actTriangle.Index3);
                ref var vertexEx1 = ref this.Owner.GetVertexBinormalTangentRef(actTriangle.Index1);
                ref var vertexEx2 = ref this.Owner.GetVertexBinormalTangentRef(actTriangle.Index2);
                ref var vertexEx3 = ref this.Owner.GetVertexBinormalTangentRef(actTriangle.Index3);

                //Get average values for current face
                var averageTangent = Vector3.Normalize(Vector3Ex.Average(vertexEx1.Tangent, vertexEx2.Tangent, vertexEx3.Tangent));
                var averagePosition = Vector3Ex.Average(vertex1.Position, vertex2.Position, vertex3.Position);
                averageTangent *= 0.2f;

                //Generate a line
                if (averageTangent.Length() > 0.1f)
                {
                    result.Add(averagePosition);
                    result.Add(averagePosition + averageTangent);
                }
            }

            return result;
        }

        /// <summary>
        /// Builds a list list containing a list for each vertex binormal.
        /// </summary>
        public List<Vector3> BuildLineListForVertexBinormals()
        {
            var result = new List<Vector3>();

            //Generate all lines
            for (var loop = 0; loop < this.Owner.CountVertices; loop++)
            {
                ref var actVertexEx = ref this.Owner.GetVertexBinormalTangentRef(loop);
                if (actVertexEx.Binormal.Length() > 0.1f)
                {
                    ref var actVertex = ref this.Owner.GetVertexBasicRef(loop);

                    result.Add(actVertex.Position);
                    result.Add(actVertex.Position + actVertexEx.Binormal * 0.2f);
                }
            }

            return result;
        }

        /// <summary>
        /// Builds a list list containing a list for each vertex normal.
        /// </summary>
        public List<Vector3> BuildLineListForVertexNormals()
        {
            var result = new List<Vector3>();

            // Generate all lines
            for (var loop = 0; loop < this.Owner.CountVertices; loop++)
            {
                ref var actVertex = ref this.Owner.GetVertexBasicRef(loop);
                if (actVertex.Normal.Length() > 0.1f)
                {
                    result.Add(actVertex.Position);
                    result.Add(actVertex.Position + actVertex.Normal * 0.2f);
                }
            }

            return result;
        }

        /// <summary>
        /// Builds a list list containing a list for each vertex tangent.
        /// </summary>
        public List<Vector3> BuildLineListForVertexTangents()
        {
            var result = new List<Vector3>();

            // Generate all lines
            for (var loop = 0; loop < this.Owner.CountVertices; loop++)
            {
                ref var actVertexEx = ref this.Owner.GetVertexBinormalTangentRef(loop);
                if (actVertexEx.Tangent.Length() > 0.1f)
                {
                    ref var actVertex = ref this.Owner.GetVertexBasicRef(loop);

                    result.Add(actVertex.Position);
                    result.Add(actVertex.Position + actVertexEx.Tangent * 0.2f);
                }
            }

            return result;
        }

        /// <summary>
        /// Build a line list containing all lines for wireframe display.
        /// </summary>
        public List<Vector3> BuildLineListForWireframeView()
        {
            var result = new List<Vector3>();

            //Generate all lines
            foreach (var actTriangle in this.Triangles)
            {
                //Get all vertices of current face
                ref var vertex1 = ref this.Owner.GetVertexBasicRef(actTriangle.Index1);
                ref var vertex2 = ref this.Owner.GetVertexBasicRef(actTriangle.Index2);
                ref var vertex3 = ref this.Owner.GetVertexBasicRef(actTriangle.Index3);

                //first line (c)
                result.Add(vertex1.Position);
                result.Add(vertex2.Position);

                //second line (a)
                result.Add(vertex2.Position);
                result.Add(vertex3.Position);

                //third line (b)
                result.Add(vertex3.Position);
                result.Add(vertex1.Position);
            }

            return result;
        }

        /// <summary>
        /// Gets an index array
        /// </summary>
        public int[] GetIndexArray()
        {
            var result = new int[_corners.Count];
            for (var loop = 0; loop < _corners.Count; loop++)
            {
                result[loop] = _corners[loop].Index;
            }
            return result;
        }

        /// <summary>
        /// Recalculates all normals
        /// </summary>
        public void CalculateNormalsFlat()
        {
            foreach (var actTriangle in this.Triangles)
            {
                this.CalculateNormalsFlat(actTriangle);
            }
        }

        /// <summary>
        /// Calculates normals for the given triangle.
        /// </summary>
        /// <param name="actTriangle">The triangle for which to calculate the normal (flat).</param>
        public void CalculateNormalsFlat(Triangle actTriangle)
        {
            ref var v1 = ref this.Owner.GetVertexBasicRef(actTriangle.Index1);
            ref var v2 = ref this.Owner.GetVertexBasicRef(actTriangle.Index2);
            ref var v3 = ref this.Owner.GetVertexBasicRef(actTriangle.Index3);

            var normal = Vector3Ex.CalculateTriangleNormal(v1.Position, v2.Position, v3.Position);
            v1.Normal = normal;
            v2.Normal = normal;
            v3.Normal = normal;
        }

        /// <summary>
        /// Calculates normals for the given triangle.
        /// </summary>
        /// <param name="countTriangles">Total count of triangles.</param>
        /// <param name="startTriangleIndex">The triangle on which to start.</param>
        public void CalculateNormalsFlat(int startTriangleIndex, int countTriangles)
        {
            var startIndex = startTriangleIndex * 3;
            var indexCount = countTriangles * 3;

            if (startIndex < 0) { throw new ArgumentException("startTriangleIndex"); }
            if (startIndex >= _corners.Count) { throw new ArgumentException("startTriangleIndex"); }
            if (startIndex + indexCount > _corners.Count) { throw new ArgumentException("countTriangles"); }

            for (var loop = 0; loop < indexCount; loop += 3)
            {
                this.CalculateNormalsFlat(new Triangle(
                    _corners[startIndex + loop].Index,
                    _corners[startIndex + loop + 1].Index,
                    _corners[startIndex + loop + 2].Index));
            }
        }

        /// <summary>
        /// Calculates tangents for all vectors.
        /// </summary>
        public void CalculateTangentsAndBinormals()
        {
            for (var loop = 0; loop < this.CountTriangles; loop += 1)
            {
                var actTriangle = this.Triangles[loop];

                //Get all vertices of current face
                ref var vertex1 = ref this.Owner.GetVertexBasicRef(actTriangle.Index1);
                ref var vertex2 = ref this.Owner.GetVertexBasicRef(actTriangle.Index2);
                ref var vertex3 = ref this.Owner.GetVertexBasicRef(actTriangle.Index3);
                ref var vertexEx1 = ref this.Owner.GetVertexBinormalTangentRef(actTriangle.Index1);
                ref var vertexEx2 = ref this.Owner.GetVertexBinormalTangentRef(actTriangle.Index2);
                ref var vertexEx3 = ref this.Owner.GetVertexBinormalTangentRef(actTriangle.Index3);

                // Perform some precalculations
                var w1 = vertex1.TexCoord1;
                var w2 = vertex2.TexCoord1;
                var w3 = vertex3.TexCoord1;
                var x1 = vertex2.Position.X - vertex1.Position.X;
                var x2 = vertex3.Position.X - vertex1.Position.X;
                var y1 = vertex2.Position.Y - vertex1.Position.Y;
                var y2 = vertex3.Position.Y - vertex1.Position.Y;
                var z1 = vertex2.Position.Z - vertex1.Position.Z;
                var z2 = vertex3.Position.Z - vertex1.Position.Z;
                var s1 = w2.X - w1.X;
                var s2 = w3.X - w1.X;
                var t1 = w2.Y - w1.Y;
                var t2 = w3.Y - w1.Y;
                var r = 1f / (s1 * t2 - s2 * t1);
                var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                // Create the tangent vector (assumes that each vertex normal within the face are equal)
                var tangent = Vector3.Normalize(sdir - vertex1.Normal * Vector3.Dot(vertex1.Normal, sdir));

                // Create the binormal using the tangent
                var tangentDir = Vector3.Dot(Vector3.Cross(vertex1.Normal, sdir), tdir) >= 0.0f ? 1f : -1f;
                var binormal = Vector3.Cross(vertex1.Normal, tangent) * tangentDir;

                // Setting binormals and tangents to each vertex of current face
                vertexEx1.Tangent = tangent;
                vertexEx1.Binormal = binormal;
                vertexEx2.Tangent = tangent;
                vertexEx2.Binormal = binormal;
                vertexEx3.Tangent = tangent;
                vertexEx3.Binormal = binormal;
            }
        }

        /// <summary>
        /// Toggles all vertices and indexes from left to right handed or right to left handed system.
        /// </summary>
        internal void ToggleCoordinateSystemInternal()
        {
            for (var loopTriangle = 0; loopTriangle + 3 <= _corners.Count; loopTriangle += 3)
            {
                var corner1 = _corners[loopTriangle];
                var corner2 = _corners[loopTriangle + 1];
                var corner3 = _corners[loopTriangle + 2];
                _corners[loopTriangle] = corner3;
                _corners[loopTriangle + 1] = corner2;
                _corners[loopTriangle + 2] = corner1;
            }
        }

        private IEnumerable<int> AddPolygonByCuttingEarsInternal(IList<int> vertexIndices, bool twoSided)
        {
            //Get all coordinates
            var coordinates = new Vector3[vertexIndices.Count];

            for (var loop = 0; loop < vertexIndices.Count; loop++)
            {
                ref var actVertex = ref this.Owner.GetVertexBasicRef(vertexIndices[loop]);
                coordinates[loop] = actVertex.Position;
            }

            //Triangulate all data
            var polygon = new Polygon(coordinates);
            var triangleIndices = polygon.TriangulateUsingCuttingEars();
            if (triangleIndices == null)
            {
                throw new SeeingSharpGraphicsException("Unable to triangulate given polygon!");
            }

            //AddObject all triangle data
            var indexEnumerator = triangleIndices.GetEnumerator();
            try
            {
                while (indexEnumerator.MoveNext())
                {
                    var index1 = indexEnumerator.Current;
                    var index2 = 0;
                    var index3 = 0;

                    if (indexEnumerator.MoveNext())
                    {
                        index2 = indexEnumerator.Current;
                    }
                    else
                    {
                        break;
                    }
                    if (indexEnumerator.MoveNext())
                    {
                        index3 = indexEnumerator.Current;
                    }
                    else
                    {
                        break;
                    }

                    this.AddTriangle(vertexIndices[index3], vertexIndices[index2], vertexIndices[index1]);
                    if (twoSided)
                    {
                        this.AddTriangle(vertexIndices[index1], vertexIndices[index2], vertexIndices[index3]);
                    }

                }
            }
            finally
            {
                indexEnumerator.Dispose();
            }

            //Return found corners
            return triangleIndices;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Contains all triangles of a Geometry object
        /// </summary>
        public class TriangleCollection : IEnumerable<Triangle>
        {
            private GeometrySurface _owner;

            /// <summary>
            /// Retrieves the triangle at the given index
            /// </summary>
            public Triangle this[int index]
            {
                get
                {
                    var startIndex = index * 3;
                    return new Triangle(
                        _owner._corners[startIndex].Index, 
                        _owner._corners[startIndex + 1].Index, 
                        _owner._corners[startIndex + 2].Index);
                }
            }

            internal TriangleCollection(GeometrySurface owner)
            {
                _owner = owner;
            }

            /// <summary>
            /// Adds a triangle to this geometry
            /// </summary>
            /// <param name="index1">Index of the first vertex</param>
            /// <param name="index2">Index of the second vertex</param>
            /// <param name="index3">Index of the third vertex</param>
            public int Add(int index1, int index2, int index3)
            {
                var result = _owner._corners.Count / 3;

                _owner._corners.Add(new TriangleCorner(index1));
                _owner._corners.Add(new TriangleCorner(index2));
                _owner._corners.Add(new TriangleCorner(index3));

                return result;
            }

            /// <summary>
            /// Adds a triangle to this geometry
            /// </summary>
            /// <param name="triangle"></param>
            public int Add(Triangle triangle)
            {
                return this.Add(triangle.Index1, triangle.Index2, triangle.Index3);
            }

            public IEnumerator<Triangle> GetEnumerator()
            {
                var triangleCount = _owner._corners.Count / 3;
                for (var loop = 0; loop < triangleCount; loop++)
                {
                    yield return this[loop];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                var triangleCount = _owner._corners.Count / 3;
                for (var loop = 0; loop < triangleCount; loop++)
                {
                    yield return this[loop];
                }
            }
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Contains all indexes of a Geometry object.
        /// </summary>
        public class IndexCollection : IEnumerable<int>
        {
            private GeometrySurface _owner;

            /// <summary>
            /// Returns the index at ghe given index
            /// </summary>
            public int this[int index] => _owner._corners[index].Index;

            public int Count => _owner._corners.Count;

            internal IndexCollection(GeometrySurface owner)
            {
                _owner = owner;
            }

            public IEnumerator<int> GetEnumerator()
            {
                return _owner._corners.Select(actCorner => actCorner.Index)
                    .GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _owner._corners.Select(actCorner => actCorner.Index)
                    .GetEnumerator();
            }
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Contains all corners of a Geometry object.
        /// </summary>
        public class CornerCollection : IEnumerable<TriangleCorner>
        {
            private GeometrySurface _owner;

            /// <summary>
            /// Returns the index at ghe given index
            /// </summary>
            public TriangleCorner this[int index] => _owner._corners[index];

            public int Count => _owner._corners.Count;

            internal CornerCollection(GeometrySurface owner)
            {
                _owner = owner;
            }

            public IEnumerator<TriangleCorner> GetEnumerator()
            {
                return _owner._corners.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _owner._corners.GetEnumerator();
            }
        }
    }
}