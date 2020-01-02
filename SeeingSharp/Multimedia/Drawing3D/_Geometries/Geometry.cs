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
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Drawing3D
{
    /// <summary>
    /// Describes a 3D geometry in memory.
    /// </summary>
    public class Geometry
    {
        // Geometry
        private List<GeometrySurface> m_surfaces;
        private UnsafeList<VertexBasic> m_verticesBasic;
        private UnsafeList<VertexBinormalTangent> m_verticesBinormalTangents;

        // Members for build time transform
        private bool m_buildTimeTransformEnabled;
        private Matrix4x4 m_buildTransformMatrix;
        private Func<VertexBasic, VertexBasic> m_buildTimeTransformFunc;

        /// <summary>
        /// Creates a new <see cref="Geometry"/> object
        /// </summary>
        public Geometry()
            : this(512)
        {
        }

        /// <summary>
        /// Creates a new Geometry object
        /// </summary>
        public Geometry(int verticesCapacity, bool hasBinormalsTangents = false)
        {
            this.Description = string.Empty;

            // Vertex data
            m_verticesBasic = new UnsafeList<VertexBasic>(verticesCapacity);
            if (hasBinormalsTangents) { m_verticesBinormalTangents = new UnsafeList<VertexBinormalTangent>(verticesCapacity); }

            // Triangle data
            m_surfaces = new List<GeometrySurface>();

            // Public accessors
            this.Vertices = new VertexCollection(this);
            this.Surfaces = new SurfaceCollection(this);
        }

        /// <summary>
        /// Creates the surface on this Geometry.
        /// </summary>
        /// <param name="triangleCapacity">The triangle capacity.</param>
        public GeometrySurface CreateSurface(int triangleCapacity = 512)
        {
            var newSurface = new GeometrySurface(this, triangleCapacity);
            m_surfaces.Add(newSurface);
            return newSurface;
        }

        /// <summary>
        /// Gets the reference to the vertex at the given index.
        /// </summary>
        /// <param name="vertexIndex">The index of the vertex.</param>
        public ref VertexBasic GetVertexBasicRef(int vertexIndex)
        {
            return ref m_verticesBasic.BackingArray[vertexIndex];
        }

        /// <summary>
        /// Gets the reference to the vertex at the given index.
        /// </summary>
        /// <param name="vertexIndex">The index of the vertex.</param>
        public ref VertexBinormalTangent GetVertexBinormalTangentRef(int vertexIndex)
        {
            if(m_verticesBinormalTangents == null){ throw new SeeingSharpException("Unable to get binormal/tangent information: This geometry doesn't provide them!");}
            return ref m_verticesBinormalTangents.BackingArray[vertexIndex];
        }

        /// <summary>
        /// Realigns all given geometries to their center coordinate.
        /// </summary>
        public void RealignToCenter()
        {
            var fullBoundingBox = this.GenerateBoundingBox();
            var fullCenter = fullBoundingBox.GetMiddleCenter();
            var targetCenter = new Vector3(0f, 0f, 0f);
            var moveToTargetCenter = targetCenter - fullCenter;

            this.UpdateVerticesUsingRelocationBy(moveToTargetCenter);
        }

        public void RealignToFloorCenter()
        {
            var fullBoundingBox = this.GenerateBoundingBox();
            var fullCenter = fullBoundingBox.GetMiddleCenter();
            var targetCenter = new Vector3(0f, fullBoundingBox.GetSize().Y / 2f, 0f);
            var moveToTargetCenter = targetCenter - fullCenter;

            this.UpdateVerticesUsingRelocationBy(moveToTargetCenter);
        }

        /// <summary>
        /// Fits to centered cube.
        /// </summary>
        public void FitToCenteredCube()
        {
            this.FitToCenteredCuboid(1f, 1f, 1f, FitToCuboidMode.MaintainAspectRatio, SpacialOriginLocation.Center);
        }

        /// <summary>
        /// Fits to centered cube.
        /// </summary>
        public void FitToCenteredCube(float cubeSideLength)
        {
            this.FitToCenteredCuboid(cubeSideLength, cubeSideLength, cubeSideLength, FitToCuboidMode.MaintainAspectRatio, SpacialOriginLocation.Center);
        }

        /// <summary>
        /// Fits to centered cube.
        /// </summary>
        public void FitToCenteredCube(float cubeSideLength, FitToCuboidMode mode)
        {
            this.FitToCenteredCuboid(cubeSideLength, cubeSideLength, cubeSideLength, mode, SpacialOriginLocation.Center);
        }

        /// <summary>
        /// Fits to centered cube.
        /// </summary>
        public void FitToCenteredCube(float cubeSideLength, FitToCuboidMode mode, SpacialOriginLocation fitOrigin)
        {
            this.FitToCenteredCuboid(cubeSideLength, cubeSideLength, cubeSideLength, mode, fitOrigin);
        }

        /// <summary>
        /// Fits to centered cube.
        /// </summary>
        public void FitToCenteredCuboid(float cubeSideLengthX, float cubeSideLengthY, float cubeSideLengthZ, FitToCuboidMode fitMode, SpacialOriginLocation fitOrigin)
        {
            //Get whole bounding box
            var boundingBox = this.GenerateBoundingBox();
            var boundingBoxSize = boundingBox.GetSize();

            if (boundingBox.IsEmpty()) { return; }
            if (boundingBoxSize.X <= 0f) { return; }
            if (boundingBoxSize.Y <= 0f) { return; }
            if (boundingBoxSize.Z <= 0f) { return; }

            var targetCornerALocation = new Vector3(
                -boundingBoxSize.X / 2f,
                -boundingBoxSize.Y / 2f,
                -boundingBoxSize.Z / 2f);

            // Calculate resize factors
            var resizeFactorX = cubeSideLengthX / boundingBoxSize.X;
            var resizeFactorY = cubeSideLengthY / boundingBoxSize.Y;
            var resizeFactorZ = cubeSideLengthZ / boundingBoxSize.Z;

            if (fitMode == FitToCuboidMode.MaintainAspectRatio)
            {
                resizeFactorX = Math.Min(resizeFactorX, Math.Min(resizeFactorY, resizeFactorZ));
                resizeFactorY = resizeFactorX;
                resizeFactorZ = resizeFactorX;
            }

            targetCornerALocation.X = targetCornerALocation.X * resizeFactorX;
            targetCornerALocation.Y = targetCornerALocation.Y * resizeFactorY;
            targetCornerALocation.Z = targetCornerALocation.Z * resizeFactorZ;
            switch (fitOrigin)
            {
                case SpacialOriginLocation.LowerCenter:
                    targetCornerALocation.Y = 0f;
                    break;
            }

            // Bring the geometry to origin based location and then scale it
            this.UpdateVerticesUsingRelocationBy(Vector3.Negate(boundingBox.CornerA));
            this.UpdateVerticesUsingRelocationFunc(actPosition => new Vector3(
                actPosition.X * resizeFactorX,
                actPosition.Y * resizeFactorY,
                actPosition.Z * resizeFactorZ));
            this.UpdateVerticesUsingRelocationBy(targetCornerALocation);
        }

        public void RemoveSurface(GeometrySurface surface)
        {
            m_surfaces.Remove(surface);
        }

        /// <summary>
        /// Enables build-time transform using the given matrix.
        /// </summary>
        /// <param name="transformMatrix">Transform matrix.</param>
        public void EnableBuildTimeTransform(Matrix4x4 transformMatrix)
        {
            m_buildTimeTransformEnabled = true;
            m_buildTransformMatrix = transformMatrix;
            m_buildTimeTransformFunc = null;
        }

        /// <summary>
        /// Enables build-time transform using given transform method.
        /// </summary>
        public void EnableBuildTimeTransform(Func<VertexBasic, VertexBasic> transformFunc)
        {
            m_buildTimeTransformEnabled = true;
            m_buildTransformMatrix = Matrix4x4.Identity;
            m_buildTimeTransformFunc = transformFunc;
        }

        /// <summary>
        /// Disables build-time transform.
        /// </summary>
        public void DisableBuildTimeTransform()
        {
            m_buildTimeTransformEnabled = false;
            m_buildTransformMatrix = Matrix4x4.Identity;
        }

        /// <summary>
        /// Performs a simple picking test against all triangles of this object.
        /// </summary>
        /// <param name="pickingRay">The picking ray.</param>
        /// <param name="distance">Additional picking options.</param>
        /// <param name="pickingOptions">The distance if picking succeeds.</param>
        public bool Intersects(Ray pickingRay, PickingOptions pickingOptions, out float distance)
        {
            var surfaceCount = m_surfaces.Count;
            for (var loop = 0; loop < surfaceCount; loop++)
            {
                if (m_surfaces[loop].Intersects(pickingRay, pickingOptions, out distance))
                {
                    return true;
                }
            }

            distance = 0f;
            return false;
        }

        /// <summary>
        /// Gets a vector to the bottom center of given geometries.
        /// </summary>
        public static Vector3 GetBottomCenter(Geometry[] geometries)
        {
            var box = GetBoundingBox(geometries);
            return box.GetBottomCenter();
        }

        /// <summary>
        /// Gets a bounding box for given geometry array.
        /// </summary>
        /// <param name="geometries">Array of geometries.</param>
        public static BoundingBox GetBoundingBox(Geometry[] geometries)
        {
            var result = new BoundingBox();

            if (geometries != null)
            {
                for (var loop = 0; loop < geometries.Length; loop++)
                {
                    if (geometries[loop] != null)
                    {
                        result.MergeWith(geometries[loop].GenerateBoundingBox());
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a vector to the middle center of given geometries.
        /// </summary>
        public static Vector3 GetMiddleCenter(Geometry[] geometries)
        {
            var box = GetBoundingBox(geometries);
            return box.GetMiddleCenter();
        }

        /// <summary>
        /// Adds all triangles of the given Geometry to this one.
        /// </summary>
        /// <param name="otherGeometry">The geometry to add to this one.</param>
        public void AddGeometry(Geometry otherGeometry)
        {
            var baseIndex = m_verticesBasic.Count;

            this.AddVertices(otherGeometry);
            var otherGeometrySurfaceCount = otherGeometry.m_surfaces.Count;

            for (var loopSurface = 0; loopSurface < otherGeometrySurfaceCount; loopSurface++)
            {
                m_surfaces.Add(otherGeometry.m_surfaces[loopSurface].Clone(
                    this,
                    baseIndex: baseIndex));
            }
        }

        /// <summary>
        /// Adds all vertices from the given geometry to this geometry.
        /// </summary>
        /// <param name="otherGeometry">The Geometry from which to copy all vertices.</param>
        public BuiltVerticesRange AddVertices(Geometry otherGeometry)
        {
            var startVertex = m_verticesBasic.Count;

            // Prepare vertex array for new capacity
            m_verticesBasic.Capacity = startVertex + otherGeometry.CountVertices;

            // Copy all vertices
            var otherGeometryVertexCount = otherGeometry.CountVertices;
            for (var loop = 0; loop < otherGeometryVertexCount; loop++)
            {
                ref var actVertex = ref otherGeometry.GetVertexBasicRef(loop);
                this.AddVertex(actVertex);
            }

            return new BuiltVerticesRange(this, startVertex, this.CountVertices - startVertex);
        }

        /// <summary>
        /// Adds a vertex to the structure
        /// </summary>
        public int AddVertex()
        {
            return this.AddVertex(VertexBasic.Empty);
        }

        /// <summary>
        /// Adds a vertex to the geometry.
        /// </summary>
        public int AddVertex(VertexBasic vertex)
        {
            // Transform vertex on build-time
            if (m_buildTimeTransformEnabled)
            {
                if (m_buildTimeTransformFunc != null) { vertex = m_buildTimeTransformFunc(vertex); }
                else
                {
                    vertex.Position = Vector3.Transform(vertex.Position, m_buildTransformMatrix);
                    vertex.Normal = Vector3.TransformNormal(vertex.Normal, m_buildTransformMatrix);
                }
            }

            // AddObject the vertex and return the index
            m_verticesBasic.Add(vertex);
            m_verticesBinormalTangents?.Add(new VertexBinormalTangent());
            return m_verticesBasic.Count - 1;
        }

        /// <summary>
        /// Calculates tangents for all vectors.
        /// </summary>
        public void CalculateTangentsAndBinormals()
        {
            foreach (var actSurface in m_surfaces)
            {
                actSurface.CalculateTangentsAndBinormals();
            }
        }

        /// <summary>
        /// Recalculates all normals
        /// </summary>
        public void CalculateNormalsFlat()
        {
            foreach (var actSurface in m_surfaces)
            {
                actSurface.CalculateNormalsFlat();
            }
        }

        /// <summary>
        /// Calculates normals for all triangles of this geometry.
        /// </summary>
        public void CalculateNormals()
        {
            this.CalculateNormals(0, m_verticesBasic.Count);
        }

        /// <summary>
        /// Calculates normals for all triangles specified by the given parameters.
        /// </summary>
        /// <param name="startVertex">The vertex index on which to start.</param>
        /// <param name="vertexCount">Total count of vertices to be updated.</param>
        public void CalculateNormals(int startVertex, int vertexCount)
        {
            if (startVertex < 0 || startVertex >= m_verticesBasic.Count) { throw new ArgumentException("startVertex"); }
            if (vertexCount + startVertex > m_verticesBasic.Count) { throw new ArgumentException("vertexCount"); }

            for (var actVertexIndex = startVertex; actVertexIndex < startVertex + vertexCount; actVertexIndex++)
            {
                // Find all triangles connected to this vertex and get normals from them
                var finalNormalHelper = Vector3.Zero;
                var normalCount = 0;

                var surfaceCount = m_surfaces.Count;

                for (var actSurfaceIndex = 0; actSurfaceIndex < surfaceCount; actSurfaceIndex++)
                {
                    var actSurface = m_surfaces[actSurfaceIndex];
                    var triangleCount = actSurface.CountTriangles;

                    for (var loopTriangle = 0; loopTriangle < triangleCount; loopTriangle++)
                    {
                        var triangleStartIndex = loopTriangle * 3;

                        if (actSurface.Indices[triangleStartIndex] == actVertexIndex ||
                            actSurface.Indices[triangleStartIndex + 1] == actVertexIndex ||
                            actSurface.Indices[triangleStartIndex + 2] == actVertexIndex)
                        {
                            ref var v1 = ref m_verticesBasic.BackingArray[actSurface.Indices[triangleStartIndex]];
                            ref var v2 = ref m_verticesBasic.BackingArray[actSurface.Indices[triangleStartIndex + 1]];
                            ref var v3 = ref m_verticesBasic.BackingArray[actSurface.Indices[triangleStartIndex + 2]];

                            finalNormalHelper += Vector3Ex.CalculateTriangleNormal(v1.Position, v2.Position, v3.Position, false);

                            normalCount++;
                        }
                    }
                }

                // Calculate final normal
                if (normalCount > 0)
                {
                    ref var actVertex = ref m_verticesBasic.BackingArray[actVertexIndex];
                    actVertex.Normal = finalNormalHelper / finalNormalHelper.Length();
                }
            }
        }

        /// <summary>
        /// Clones this object
        /// </summary>
        public Geometry Clone(bool copyGeometryData = true, int capacityMultiplier = 1)
        {
            capacityMultiplier.EnsurePositiveAndNotZero(nameof(capacityMultiplier));

            // Create new Geometry object
            var vertexCount = m_verticesBasic.Count;
            var hasBinormalsTangents = this.HasBinormalsTangents;
            var result = new Geometry(
                verticesCapacity: vertexCount * capacityMultiplier,
                hasBinormalsTangents: hasBinormalsTangents);

            // Copy geometry
            if (copyGeometryData)
            {
                for (var loop = 0; loop < vertexCount; loop++)
                {
                    result.m_verticesBasic.Add(m_verticesBasic[loop]);
                    if(hasBinormalsTangents){ result.m_verticesBinormalTangents.Add(m_verticesBinormalTangents[loop]); }
                }
            }

            // Copy surfaces
            foreach (var actSurface in m_surfaces)
            {
                result.m_surfaces.Add(actSurface.Clone(result, copyGeometryData, capacityMultiplier));
            }

            // Copy metadata
            result.Description = this.Description;

            return result;
        }

        /// <summary>
        /// Generates a <see cref="BoundingBox"/> around this geometry
        /// </summary>
        public BoundingBox GenerateBoundingBox()
        {
            var maximum = Vector3Ex.MinValue;
            var minimum = Vector3Ex.MaxValue;

            foreach (var actVertex in m_verticesBasic)
            {
                var actPosition = actVertex.Position;

                //Update minimum vector
                if (actPosition.X < minimum.X) { minimum.X = actPosition.X; }
                if (actPosition.Y < minimum.Y) { minimum.Y = actPosition.Y; }
                if (actPosition.Z < minimum.Z) { minimum.Z = actPosition.Z; }

                //Update maximum vector
                if (actPosition.X > maximum.X) { maximum.X = actPosition.X; }
                if (actPosition.Y > maximum.Y) { maximum.Y = actPosition.Y; }
                if (actPosition.Z > maximum.Z) { maximum.Z = actPosition.Z; }
            }

            return new BoundingBox(minimum, maximum);
        }

        /// <summary>
        /// Changes the index order of each triangle.
        /// </summary>
        public void ToggleTriangleIndexOrder()
        {
            for (var loop = 0; loop < m_surfaces.Count; loop++)
            {
                m_surfaces[loop].ToggleTriangleIndexOrder();
            }
        }

        /// <summary>
        /// Toggles all vertices and indexes from left to right handed or right to left handed system.
        /// </summary>
        public void ToggleCoordinateSystem()
        {
            // Calculate the center coordinate of this geometry
            var boundingBox = this.GenerateBoundingBox();
            var centerCoord = boundingBox.GetMiddleCenter();

            // Update each vertex coordinate
            this.UpdateVerticesUsingRelocationFunc(givenVector => new Vector3(
                givenVector.X, givenVector.Y,
                centerCoord.Z + (centerCoord.Z - givenVector.Z)));

            // Now change index ordering
            foreach (var actSurface in m_surfaces)
            {
                actSurface.ToggleCoordinateSystemInternal();
            }
        }

        /// <summary>
        /// Transforms positions and normals of all vertices using the given transform matrix
        /// </summary>
        public void TransformVertices(Matrix4x4 transformMatrix)
        {
            var length = m_verticesBasic.Count;
            for (var loop = 0; loop < length; loop++)
            {
                ref var actVertex = ref m_verticesBasic.BackingArray[loop];
                actVertex.Position = Vector3.Transform(actVertex.Position, transformMatrix);
                actVertex.Normal = Vector3.TransformNormal(actVertex.Normal, transformMatrix);
            }
        }

        /// <summary>
        /// Relocates all vertices by the given vector
        /// </summary>
        public void UpdateVerticesUsingRelocationBy(Vector3 relocateVector)
        {
            var length = m_verticesBasic.Count;
            for (var loop = 0; loop < length; loop++)
            {
                ref var actVertex = ref m_verticesBasic.BackingArray[loop];
                actVertex.Position += relocateVector;
            }
        }

        /// <summary>
        /// Relocates all vertices by the given relocation function (executed for each position vector).
        /// </summary>
        /// <param name="calculatePositionFunc">The function to be applied to each coordinate.</param>
        public void UpdateVerticesUsingRelocationFunc(Func<Vector3, Vector3> calculatePositionFunc)
        {
            var length = m_verticesBasic.Count;
            for (var loop = 0; loop < length; loop++)
            {
                ref var actVertex = ref m_verticesBasic.BackingArray[loop];
                actVertex.Position = calculatePositionFunc(actVertex.Position);
            }
        }

        /// <summary>
        /// Gets the first surface of this geometry.
        /// It there is no surface, then one gets created automatically.
        /// </summary>
        public GeometrySurface FirstSurface
        {
            get
            {
                if (m_surfaces.Count == 0)
                {
                    this.CreateSurface();
                }
                return m_surfaces[0];
            }
        }

        /// <summary>
        /// Retrieves total count of all vertices within this geometry
        /// </summary>
        public int CountVertices => m_verticesBasic.Count;

        /// <summary>
        /// A short description for the use of this geometry
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Is this geometry empty?
        /// </summary>
        public bool IsEmpty => m_verticesBasic.Count == 0 && m_surfaces.Count == 0;

        /// <summary>
        /// Gets a collection of vertices.
        /// </summary>
        public VertexCollection Vertices { get; }

        public SurfaceCollection Surfaces { get; }

        public int CountSurfaces => m_surfaces.Count;

        public int CountTriangles
        {
            get
            {
                var sum = 0;
                for (var loop = 0; loop < m_surfaces.Count; loop++)
                {
                    sum += m_surfaces[loop].CountTriangles;
                }
                return sum;
            }
        }

        public int CountIndices
        {
            get
            {
                var sum = 0;
                for (var loop = 0; loop < m_surfaces.Count; loop++)
                {
                    sum += m_surfaces[loop].CountIndices;
                }
                return sum;
            }
        }

        public bool HasBinormalsTangents => m_verticesBinormalTangents != null;

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Contains all vertices of a Geometry object
        /// </summary>
        public class VertexCollection : IEnumerable<VertexBasic>
        {
            private Geometry m_owner;

            internal VertexCollection(Geometry owner)
            {
                m_owner = owner;
            }

            /// <summary>
            /// Adds a vertex to the geometry
            /// </summary>
            public void Add(VertexBasic vertex)
            {
                m_owner.AddVertex(vertex);
            }

            public IEnumerator<VertexBasic> GetEnumerator()
            {
                return m_owner.m_verticesBasic.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return m_owner.m_verticesBasic.GetEnumerator();
            }

            /// <summary>
            /// Returns the vertex at ghe given index
            /// </summary>
            public VertexBasic this[int index]
            {
                get => m_owner.m_verticesBasic[index];
            }
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Contains all vertices of a Geometry object
        /// </summary>
        public class SurfaceCollection : IEnumerable<GeometrySurface>
        {
            private Geometry m_owner;

            internal SurfaceCollection(Geometry owner)
            {
                m_owner = owner;
            }

            public void Add(GeometrySurface surface)
            {
                m_owner.m_surfaces.Add(surface);
            }

            public IEnumerator<GeometrySurface> GetEnumerator()
            {
                return m_owner.m_surfaces.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return m_owner.m_surfaces.GetEnumerator();
            }

            /// <summary>
            /// Returns the surface at ghe given index
            /// </summary>
            public GeometrySurface this[int index]
            {
                get => m_owner.m_surfaces[index];
                internal set => m_owner.m_surfaces[index] = value;
            }
        }
    }
}