﻿/*
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SeeingSharp.Checking;
using SeeingSharp.Util;
using SharpDX;

namespace SeeingSharp.Multimedia.Objects
{
    /// <summary>
    /// A set of triangles of a Geometry which share the
    /// same material settings.
    /// </summary>
    public partial class GeometrySurface
    {
        private Dictionary<Type, object> m_materialPropertiesExtended;
        private List<TriangleCorner> m_corners;

        internal GeometrySurface(Geometry owner, int triangleCapacity)
        {
            Owner = owner;
            m_corners = new List<TriangleCorner>(triangleCapacity * 3);

            Indices = new IndexCollection(m_corners);
            Corners = new CornerCollection(m_corners);
            Triangles = new TriangleCollection(m_corners);
            MaterialProperties = new MaterialProperties();
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
            var indexCount = m_corners.Count;
            var result = new GeometrySurface(newOwner, indexCount / 3 * capacityMultiplier);

            // Copy geometry
            if (copyGeometryData)
            {
                for (var loop = 0; loop < indexCount; loop++)
                {
                    var cornerToAdd = m_corners[loop];
                    cornerToAdd.Index = cornerToAdd.Index + baseIndex;
                    result.m_corners.Add(cornerToAdd);
                }
            }

            // Copy metadata
            result.MaterialProperties = MaterialProperties.Clone();

            return result;
        }

        /// <summary>
        /// Adds all vertices and surfaces of the given geometry to this one.
        /// All surfaces of the given geometry are merged to this single surface.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        public void AddGeometry(Geometry geometry)
        {
            var baseIndex = Owner.VerticesInternal.Count;

            // Add all vertices to local geometry
            Owner.VerticesInternal.AddRange(geometry.VerticesInternal);

            // Add all corners to local surface
            foreach(var actSurface in geometry.Surfaces)
            {
                var corners = actSurface.m_corners;
                var cornerCount = corners.Count;

                for(var loop=0; loop<cornerCount; loop++)
                {
                    var cornerToAdd = corners[loop];
                    cornerToAdd.Index = cornerToAdd.Index + baseIndex;
                    m_corners.Add(cornerToAdd);
                }
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
            m_corners.Add(new TriangleCorner(index1));
            m_corners.Add(new TriangleCorner(index2));
            m_corners.Add(new TriangleCorner(index3));
        }

        /// <summary>
        /// Adds a triangle
        /// </summary>
        /// <param name="v1">First vertex</param>
        /// <param name="v2">Second vertex</param>
        /// <param name="v3">Third vertex</param>
        public void AddTriangle(Vertex v1, Vertex v2, Vertex v3)
        {
            m_corners.Add(new TriangleCorner(Owner.AddVertex(v1)));
            m_corners.Add(new TriangleCorner(Owner.AddVertex(v2)));
            m_corners.Add(new TriangleCorner(Owner.AddVertex(v3)));
        }

        /// <summary>
        /// Adds a triangle and calculates normals for it.
        /// </summary>
        /// <param name="index1">Index of the first vertex</param>
        /// <param name="index2">Index of the second vertex</param>
        /// <param name="index3">Index of the third vertex</param>
        public void AddTriangleAndCalculateNormalsFlat(int index1, int index2, int index3)
        {
            AddTriangle(index1, index2, index3);
            CalculateNormalsFlat(new Triangle(index1, index2, index3));
        }

        /// <summary>
        /// Adds a triangle
        /// </summary>
        /// <param name="v1">First vertex</param>
        /// <param name="v2">Second vertex</param>
        /// <param name="v3">Third vertex</param>
        public void AddTriangleAndCalculateNormalsFlat(Vertex v1, Vertex v2, Vertex v3)
        {
            var index1 = Owner.AddVertex(v1);
            var index2 = Owner.AddVertex(v2);
            var index3 = Owner.AddVertex(v3);

            AddTriangleAndCalculateNormalsFlat(index1, index2, index3);
        }

        /// <summary>
        /// Adds the given polygon using the cutting ears algorithm for triangulation.
        /// </summary>
        /// <param name="vertices">The vertices to add.</param>
        public void AddPolygonByCuttingEars(IEnumerable<Vertex> vertices)
        {
            //Add vertices first
            var indices = new List<int>();

            foreach (var actVertex in vertices)
            {
                indices.Add(Owner.AddVertex(actVertex));
            }

            //Calculate cutting ears
            AddPolygonByCuttingEars(indices);
        }

        /// <summary>
        /// Adds the given polygon using the cutting ears algorithm for triangulation.
        /// </summary>
        /// <param name="indices">The indices of the polygon's corners.</param>
        /// <param name="twoSided">The indexes for front- and backside?</param>
        public void AddPolygonByCuttingEars(IEnumerable<int> indices, bool twoSided = false)
        {
            AddPolygonByCuttingEarsInternal(new List<int>(indices), twoSided);
        }

        /// <summary>
        /// Adds the given polygon using the cutting ears algorithm for triangulation.
        /// </summary>
        /// <param name="vertices">The vertices to add.</param>
        /// <param name = "twoSided" > The indexes for front- and backside?</param>
        public void AddPolygonByCuttingEarsAndCalculateNormals(IEnumerable<Vertex> vertices, bool twoSided = false)
        {
            //Add vertices first
            var indices = new List<int>();

            foreach (var actVertex in vertices)
            {
                indices.Add(Owner.AddVertex(actVertex));
            }

            //Calculate cutting ears and normals
            AddPolygonByCuttingEarsAndCalculateNormals(indices, twoSided);
        }

        /// <summary>
        /// Adds the given polygon using the cutting ears algorithm for triangulation.
        /// </summary>
        /// <param name="indices">The indices of the polygon's corners.</param>
        /// <param name="twoSided">The indexes for front- and backside?</param>
        public void AddPolygonByCuttingEarsAndCalculateNormals(IEnumerable<int> indices, bool twoSided)
        {
            //Add the triangles using cutting ears algorithm
            var addedIndices = AddPolygonByCuttingEarsInternal(new List<int>(indices), twoSided);

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

                    CalculateNormalsFlat(new Triangle(index1, index2, index3));
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
            foreach (var actTriangle in Triangles)
            {
                //Get all vertices of current face
                var vertex1 = Owner.VerticesInternal[actTriangle.Index1];
                var vertex2 = Owner.VerticesInternal[actTriangle.Index2];
                var vertex3 = Owner.VerticesInternal[actTriangle.Index3];

                //Get average values for current face
                var averageBinormal = Vector3.Normalize(Vector3Ex.Average(vertex1.Binormal, vertex2.Binormal, vertex3.Binormal));
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
            foreach (var actTriangle in Triangles)
            {
                //Get all vertices of current face
                var vertex1 = Owner.VerticesInternal[actTriangle.Index1];
                var vertex2 = Owner.VerticesInternal[actTriangle.Index2];
                var vertex3 = Owner.VerticesInternal[actTriangle.Index3];

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
            foreach (var actTriangle in Triangles)
            {
                //Get all vertices of current face
                var vertex1 = Owner.VerticesInternal[actTriangle.Index1];
                var vertex2 = Owner.VerticesInternal[actTriangle.Index2];
                var vertex3 = Owner.VerticesInternal[actTriangle.Index3];

                //Get average values for current face
                var averageTangent = Vector3.Normalize(Vector3Ex.Average(vertex1.Tangent, vertex2.Tangent, vertex3.Tangent));
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
            foreach (var actVertex in Owner.VerticesInternal)
            {
                if (actVertex.Binormal.Length() > 0.1f)
                {
                    result.Add(actVertex.Position);
                    result.Add(actVertex.Position + actVertex.Binormal * 0.2f);
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

            //Generate all lines
            foreach (var actVertex in Owner.VerticesInternal)
            {
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

            //Generate all lines
            foreach (var actVertex in Owner.VerticesInternal)
            {
                if (actVertex.Tangent.Length() > 0.1f)
                {
                    result.Add(actVertex.Position);
                    result.Add(actVertex.Position + actVertex.Tangent * 0.2f);
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
            foreach (var actTriangle in Triangles)
            {
                //Get all vertices of current face
                var vertex1 = Owner.VerticesInternal[actTriangle.Index1];
                var vertex2 = Owner.VerticesInternal[actTriangle.Index2];
                var vertex3 = Owner.VerticesInternal[actTriangle.Index3];

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
        /// Sets the extended material properties.
        /// </summary>
        /// <typeparam name="T">The type of properties to set.</typeparam>
        /// <param name="properties">The properties to set.</param>
        public void SetExtendedMaterialProperties<T>(T properties)
            where T : class
        {
            if (m_materialPropertiesExtended == null)
            {
                m_materialPropertiesExtended = new Dictionary<Type, object>();
            }

            if (properties == null)
            {
                var propertiesType = typeof(T);

                if (m_materialPropertiesExtended.ContainsKey(propertiesType))
                {
                    m_materialPropertiesExtended.Remove(propertiesType);
                }

                if (m_materialPropertiesExtended.Count == 0)
                {
                    m_materialPropertiesExtended = null;
                }
            }
            else
            {
                m_materialPropertiesExtended[typeof(T)] = properties;
            }
        }

        /// <summary>
        /// Gets extended material properties of the given type.
        /// </summary>
        /// <typeparam name="T">The type of properties to get.</typeparam>
        public T GetExtendedMaterialProperties<T>()
            where T : class
        {
            if (m_materialPropertiesExtended == null)
            {
                return null;
            }

            var propertiesType = typeof(T);

            if (m_materialPropertiesExtended.ContainsKey(propertiesType))
            {
                return m_materialPropertiesExtended[propertiesType] as T;
            }

            return null;
        }

        /// <summary>
        /// Gets an index array
        /// </summary>
        public int[] GetIndexArray()
        {
            var result = new int[m_corners.Count];
            for (var loop = 0; loop < m_corners.Count; loop++)
            {
                result[loop] = m_corners[loop].Index;
            }
            return result;
        }

        /// <summary>
        /// Recalculates all normals
        /// </summary>
        public void CalculateNormalsFlat()
        {
            foreach (var actTriangle in Triangles)
            {
                CalculateNormalsFlat(actTriangle);
            }
        }

        /// <summary>
        /// Calculates normals for the given triangle.
        /// </summary>
        /// <param name="actTriangle">The triangle for which to calculate the normal (flat).</param>
        public void CalculateNormalsFlat(Triangle actTriangle)
        {
            var v1 = Owner.VerticesInternal[actTriangle.Index1];
            var v2 = Owner.VerticesInternal[actTriangle.Index2];
            var v3 = Owner.VerticesInternal[actTriangle.Index3];

            var normal = Vector3Ex.CalculateTriangleNormal(v1.Geometry.Position, v2.Geometry.Position, v3.Geometry.Position);

            v1 = v1.Copy(v1.Geometry.Position, normal);
            v2 = v2.Copy(v2.Geometry.Position, normal);
            v3 = v3.Copy(v3.Geometry.Position, normal);

            Owner.VerticesInternal[actTriangle.Index1] = v1;
            Owner.VerticesInternal[actTriangle.Index2] = v2;
            Owner.VerticesInternal[actTriangle.Index3] = v3;
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
            if (startIndex >= m_corners.Count) { throw new ArgumentException("startTriangleIndex"); }
            if (startIndex + indexCount > m_corners.Count) { throw new ArgumentException("countTriangles"); }

            for (var loop = 0; loop < indexCount; loop += 3)
            {
                CalculateNormalsFlat(new Triangle(
                    m_corners[startIndex + loop].Index,
                    m_corners[startIndex + loop + 1].Index,
                    m_corners[startIndex + loop + 2].Index));
            }
        }

        /// <summary>
        /// Calculates tangents for all vectors.
        /// </summary>
        public void CalculateTangentsAndBinormals()
        {
            for (var loop = 0; loop < CountTriangles; loop += 1)
            {
                var actTriangle = Triangles[loop];

                //Get all vertices of current face
                var vertex1 = Owner.VerticesInternal[actTriangle.Index1];
                var vertex2 = Owner.VerticesInternal[actTriangle.Index2];
                var vertex3 = Owner.VerticesInternal[actTriangle.Index3];

                // Perform some precalculations
                var w1 = vertex1.TexCoord;
                var w2 = vertex2.TexCoord;
                var w3 = vertex3.TexCoord;
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
                vertex1.Tangent = tangent;
                vertex1.Binormal = binormal;
                vertex2.Tangent = tangent;
                vertex2.Binormal = binormal;
                vertex3.Tangent = tangent;
                vertex3.Binormal = binormal;

                // Overtake changes made in geometry
                Owner.VerticesInternal[actTriangle.Index1] = vertex1;
                Owner.VerticesInternal[actTriangle.Index2] = vertex2;
                Owner.VerticesInternal[actTriangle.Index3] = vertex3;
            }
        }

        /// <summary>
        /// Toggles all vertices and indexes from left to right handed or right to left handed system.
        /// </summary>
        internal void ToggleCoordinateSystemInternal()
        {
            for (var loopTriangle = 0; loopTriangle + 3 <= m_corners.Count; loopTriangle += 3)
            {
                var corner1 = m_corners[loopTriangle];
                var corner2 = m_corners[loopTriangle + 1];
                var corner3 = m_corners[loopTriangle + 2];
                m_corners[loopTriangle] = corner3;
                m_corners[loopTriangle + 1] = corner2;
                m_corners[loopTriangle + 2] = corner1;
            }
        }

        private IEnumerable<int> AddPolygonByCuttingEarsInternal(IList<int> vertexIndices, bool twoSided)
        {
            //Get all coordinates
            var coordinates = new Vector3[vertexIndices.Count];

            for (var loop = 0; loop < vertexIndices.Count; loop++)
            {
                coordinates[loop] = Owner.VerticesInternal[vertexIndices[loop]].Position;
            }

            //Triangulate all data
            var polygon = new Polygon(coordinates);
            var triangleIndices = polygon.TriangulateUsingCuttingEars();

            if (triangleIndices == null)
            {
                throw new SeeingSharpGraphicsException("Unable to triangulate given polygon!");
            }

            //Add all triangle data
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

                    AddTriangle(vertexIndices[index3], vertexIndices[index2], vertexIndices[index1]);
                    if (twoSided)
                    {
                        AddTriangle(vertexIndices[index1], vertexIndices[index2], vertexIndices[index3]);
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

        /// <summary>
        /// Gets the name of the material.
        /// </summary>
        public NamedOrGenericKey Material
        {
            get => MaterialProperties.Key;
            set => MaterialProperties.Key = value;
        }

        /// <summary>
        /// Gets or sets the diffuse color of the material.
        /// </summary>
        public Color4 DiffuseColor
        {
            get => MaterialProperties.DiffuseColor;
            set => MaterialProperties.DiffuseColor = value;
        }

        /// <summary>
        /// Gets or sets the name of the texture (used for the NamedOrGenericKey geometry behind).
        /// </summary>
        public string TextureName
        {
            get => MaterialProperties.TextureKey.NameKey;
            set
            {
                if (string.IsNullOrEmpty(value)) { MaterialProperties.TextureKey = NamedOrGenericKey.Empty; }
                else { MaterialProperties.TextureKey = new NamedOrGenericKey(value); }
            }
        }

        /// <summary>
        /// Gets or sets the key of the texture.
        /// </summary>
        public NamedOrGenericKey TextureKey
        {
            get => MaterialProperties.TextureKey;
            set => MaterialProperties.TextureKey = value;
        }

        /// <summary>
        /// Gets or sets the material properties object.
        /// </summary>
        public MaterialProperties MaterialProperties { get; internal set; }

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
        public int CountTriangles => m_corners.Count / 3;

        /// <summary>
        /// Gets or sets the resource source assembly.
        /// </summary>
        public Assembly ResourceSourceAssembly => Owner.ResourceSourceAssembly;

        /// <summary>
        /// Gets or sets the original source of this geometry.
        /// </summary>
        public ResourceLink ResourceLink => Owner.ResourceLink;

        public Geometry Owner { get; }

        /// <summary>
        /// Retrieves total count of all indexes within this geometry
        /// </summary>
        internal int CountIndices => m_corners.Count;

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Contains all triangles of a Geometry object
        /// </summary>
        public class TriangleCollection : IEnumerable<Triangle>
        {
            private List<TriangleCorner> m_corners;

            internal TriangleCollection(List<TriangleCorner> corners)
            {
                m_corners = corners;
            }

            /// <summary>
            /// Adds a triangle to this geometry
            /// </summary>
            /// <param name="index1">Index of the first vertex</param>
            /// <param name="index2">Index of the second vertex</param>
            /// <param name="index3">Index of the third vertex</param>
            public int Add(int index1, int index2, int index3)
            {
                var result = m_corners.Count / 3;

                m_corners.Add(new TriangleCorner(index1));
                m_corners.Add(new TriangleCorner(index2));
                m_corners.Add(new TriangleCorner(index3));

                return result;
            }

            /// <summary>
            /// Adds a triangle to this geometry
            /// </summary>
            /// <param name="triangle"></param>
            public int Add(Triangle triangle)
            {
                return Add(triangle.Index1, triangle.Index2, triangle.Index3);
            }

            public IEnumerator<Triangle> GetEnumerator()
            {
                var triangleCount = m_corners.Count / 3;
                for (var loop = 0; loop < triangleCount; loop++)
                {
                    yield return this[loop];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                var triangleCount = m_corners.Count / 3;
                for (var loop = 0; loop < triangleCount; loop++)
                {
                    yield return this[loop];
                }
            }

            /// <summary>
            /// Retrieves the triangle at the given index
            /// </summary>
            public Triangle this[int index]
            {
                get
                {
                    var startIndex = index * 3;
                    return new Triangle(m_corners[startIndex].Index, m_corners[startIndex + 1].Index, m_corners[startIndex + 2].Index);
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
            private List<TriangleCorner> m_corners;

            internal IndexCollection(List<TriangleCorner> corners)
            {
                m_corners = corners;
            }

            public IEnumerator<int> GetEnumerator()
            {
                return m_corners.Select(actCorner => actCorner.Index)
                    .GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return m_corners.Select(actCorner => actCorner.Index)
                    .GetEnumerator();
            }

            /// <summary>
            /// Returns the index at ghe given index
            /// </summary>
            public int this[int index] => m_corners[index].Index;

            public int Count => m_corners.Count;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Contains all corners of a Geometry object.
        /// </summary>
        public class CornerCollection : IEnumerable<TriangleCorner>
        {
            private List<TriangleCorner> m_corners;

            internal CornerCollection(List<TriangleCorner> corners)
            {
                m_corners = corners;
            }

            public IEnumerator<TriangleCorner> GetEnumerator()
            {
                return m_corners.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return m_corners.GetEnumerator();
            }

            /// <summary>
            /// Returns the index at ghe given index
            /// </summary>
            public TriangleCorner this[int index] => m_corners[index];

            public int Count => m_corners.Count;
        }
    }
}