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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using SharpDX;

namespace SeeingSharp.Multimedia.Objects
{
    /// <summary>
    /// Describes a 3D geometry in memory.
    /// </summary>
    public class Geometry
    {
        // Description
        private string m_name;

        // Geometry
        private List<GeometrySurface> m_surfaces;

        // Members for build time transform
        private bool m_buildTimeTransformEnabled;
        private Matrix m_buildTransformMatrix;
        private Func<Vertex, Vertex> m_buildTimeTransformFunc;

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
        public Geometry(int verticesCapacity)
        {
            m_name = string.Empty;
            this.Description = string.Empty;

            this.VerticesInternal = new List<Vertex>(verticesCapacity);
            m_surfaces = new List<GeometrySurface>();

            this.Vertices = new VertexCollection(this.VerticesInternal);
            this.Surfaces = new SurfaceCollection(m_surfaces);
        }

        /// <summary>
        /// Creates the surface on this Geometry.
        /// </summary>
        /// <param name="triangleCapacity">The triangle capacity.</param>
        /// <param name="name">The internal name of the material.</param>
        public GeometrySurface CreateSurface(int triangleCapacity = 512, string name = "")
        {
            var newSurface = new GeometrySurface(this, triangleCapacity)
            {
                CommonMaterialProperties = {Name = name}
            };

            m_surfaces.Add(newSurface);
            return newSurface;
        }

        /// <summary>
        /// Tries to get an existing surface using given name.
        /// If none exists, then a new surface is created.
        /// </summary>
        /// <param name="name">The internal name of the material.</param>
        /// <param name="triangleCapacity">The triangle capacity.</param>
        public GeometrySurface CreateOrGetExistingSurfaceByName(string name, int triangleCapacity = 512)
        {
            foreach (var actSurface in m_surfaces)
            {
                if (actSurface.CommonMaterialProperties.Name == name)
                {
                    return actSurface;
                }
            }

            return this.CreateSurface(triangleCapacity, name);
        }

        /// <summary>
        /// Tries to get an existing surface using given CommonMaterialProperties.
        /// If none exists, then a new surface is created.
        /// </summary>
        /// <param name="matProperties">The material properties.</param>
        /// <param name="triangleCapacity">The triangle capacity.</param>
        public GeometrySurface CreateOrGetExistingSurface(CommonMaterialProperties matProperties, int triangleCapacity = 512)
        {
            foreach(var actSurface in m_surfaces)
            {
                if (actSurface.CommonMaterialProperties == matProperties)
                {
                    return actSurface;
                }
            }

            var result = this.CreateSurface(triangleCapacity);
            result.CommonMaterialProperties = matProperties;
            return result;
        }

        /// <summary>
        /// Tries to get an existing surface using given CommonMaterialProperties.
        /// If none exists, then a new surface is created.
        /// </summary>
        /// <param name="name">The internal name of the material.</param>
        /// <param name="triangleCapacity">The triangle capacity.</param>
        public GeometrySurface CreateOrGetExistingSurface(string name, int triangleCapacity = 512)
        {
            foreach (var actSurface in m_surfaces)
            {
                if (actSurface.CommonMaterialProperties.Name == name)
                {
                    return actSurface;
                }
            }

            var result = this.CreateSurface(triangleCapacity);
            result.CommonMaterialProperties.Name = name;
            return result;
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
            var targetCenter = new Vector3(0f, fullBoundingBox.Size.Y / 2f, 0f);
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
            var boundingBoxSize = boundingBox.Size;

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
            this.UpdateVerticesUsingRelocationBy(Vector3.Negate(boundingBox.GetCornerA()));
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
        /// Ensures that there is a vertex at the given index and returns it.
        /// </summary>
        /// <param name="index">The index to get the vertex from.</param>
        public Vertex EnsureVertexAt(int index)
        {
            while (this.VerticesInternal.Count <= index) {
                this.AddVertex(); }
            return this.VerticesInternal[index];
        }

        /// <summary>
        /// Enables build-time transform using the given matrix.
        /// </summary>
        /// <param name="transformMatrix">Transform matrix.</param>
        public void EnableBuildTimeTransform(Matrix transformMatrix)
        {
            m_buildTimeTransformEnabled = true;
            m_buildTransformMatrix = transformMatrix;
            m_buildTimeTransformFunc = null;
        }

        /// <summary>
        /// Enables build-time transform using given transform method.
        /// </summary>
        public void EnableBuildTimeTransform(Func<Vertex, Vertex> transformFunc)
        {
            m_buildTimeTransformEnabled = true;
            m_buildTransformMatrix = Matrix.Identity;
            m_buildTimeTransformFunc = transformFunc;
        }

        /// <summary>
        /// Disables build-time transform.
        /// </summary>
        public void DisableBuildTimeTransform()
        {
            m_buildTimeTransformEnabled = false;
            m_buildTransformMatrix = Matrix.Identity;
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
            for(var loop=0; loop<surfaceCount; loop++)
            {
                if(m_surfaces[loop].Intersects(pickingRay, pickingOptions, out distance))
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
            var baseIndex = this.VerticesInternal.Count;

            this.VerticesInternal.AddRange(otherGeometry.Vertices);
            var otherGeometrySurfaceCount = otherGeometry.m_surfaces.Count;

            for (var loopSurface = 0; loopSurface < otherGeometrySurfaceCount; loopSurface++)
            {
                m_surfaces.Add(otherGeometry.m_surfaces[loopSurface].Clone(
                    this,
                    baseIndex: baseIndex));
            }
        }

        /// <summary>
        /// Adds a vertex to the structure
        /// </summary>
        public int AddVertex()
        {
            return this.AddVertex(Vertex.Empty);
        }

        /// <summary>
        /// Adds a vertex to the geometry.
        /// </summary>
        public int AddVertex(Vertex vertex)
        {
            //Transform vertex on build-time
            if (m_buildTimeTransformEnabled)
            {
                if (m_buildTimeTransformFunc != null) { vertex = m_buildTimeTransformFunc(vertex); }
                else
                {
                    vertex.Position = Vector3.Transform(vertex.Position, m_buildTransformMatrix).ToXYZ();
                    vertex.Normal = Vector3.TransformNormal(vertex.Normal, m_buildTransformMatrix);
                }
            }

            //AddObject the vertex and return the index
            this.VerticesInternal.Add(vertex);
            return this.VerticesInternal.Count - 1;
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
            foreach(var actSurface in m_surfaces)
            {
                actSurface.CalculateNormalsFlat();
            }
        }

        /// <summary>
        /// Calculates normals for all triangles of this geometry.
        /// </summary>
        public void CalculateNormals()
        {
            this.CalculateNormals(0, this.VerticesInternal.Count);
        }

        /// <summary>
        /// Calculates normals for all triangles specified by the given parameters.
        /// </summary>
        /// <param name="startVertex">The vertex index on which to start.</param>
        /// <param name="vertexCount">Total count of vertices to be updated.</param>
        public void CalculateNormals(int startVertex, int vertexCount)
        {
            if (startVertex < 0 || startVertex >= this.VerticesInternal.Count) { throw new ArgumentException("startVertex"); }
            if (vertexCount + startVertex > this.VerticesInternal.Count) { throw new ArgumentException("vertexCount"); }

            for (var actVertexIndex = startVertex; actVertexIndex < startVertex + vertexCount; actVertexIndex++)
            {
                // Find all triangles connected to this vertex and get normals from them
                var finalNormalHelper = Vector3.Zero;
                var finalNormalHelper2 = Vector3.Zero;
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
                            var v1 = this.VerticesInternal[actSurface.Indices[triangleStartIndex]];
                            var v2 = this.VerticesInternal[actSurface.Indices[triangleStartIndex + 1]];
                            var v3 = this.VerticesInternal[actSurface.Indices[triangleStartIndex + 2]];

                            finalNormalHelper += Vector3Ex.CalculateTriangleNormal(v1.Geometry.Position, v2.Geometry.Position, v3.Geometry.Position, false);

                            normalCount++;
                        }
                    }
                }

                // Calculate final normal
                if (normalCount > 0)
                {
                    var actVertex = this.VerticesInternal[actVertexIndex];
                    actVertex.Normal = finalNormalHelper / finalNormalHelper.Length();
                    this.VerticesInternal[actVertexIndex] = actVertex;
                    normalCount = 0;
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
            var vertexCount = this.VerticesInternal.Count;
            var result = new Geometry(
                vertexCount * capacityMultiplier);

            // Copy geometry
            if (copyGeometryData)
            {
                for (var loop = 0; loop < vertexCount; loop++)
                {
                    result.VerticesInternal.Add(this.VerticesInternal[loop]);
                }
            }

            // Copy surfaces
            foreach(var actSurface in m_surfaces)
            {
                result.m_surfaces.Add(actSurface.Clone(result, copyGeometryData, capacityMultiplier));
            }

            // Copy metadata
            result.Description = this.Description;
            result.m_name = m_name;

            return result;
        }

        /// <summary>
        /// Generates a <see cref="BoundingBox"/> around this geometry
        /// </summary>
        public BoundingBox GenerateBoundingBox()
        {
            var maximum = Vector3Ex.MinValue;
            var minimum = Vector3Ex.MaxValue;

            foreach (var actVertex in this.VerticesInternal)
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
        /// Changes the color on each vertex.
        /// </summary>
        /// <param name="Color4">The new color.</param>
        public void SetColorOnEachVertex(Color4 Color4)
        {
            for (var loop = 0; loop < this.VerticesInternal.Count; loop++)
            {
                this.VerticesInternal[loop] = this.VerticesInternal[loop].Copy(Color4);
            }
        }

        /// <summary>
        /// Changes the index order of each triangle.
        /// </summary>
        public void ToggleTriangleIndexOrder()
        {
            for(var loop=0; loop<m_surfaces.Count; loop++)
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
            foreach(var actSurface in m_surfaces)
            {
                actSurface.ToggleCoordinateSystemInternal();
            }
        }

        /// <summary>
        /// Transforms positions and normals of all vertices using the given transform matrix
        /// </summary>
        /// <param name="transformMatrix"></param>
        public void TransformVertices(Matrix transformMatrix)
        {
            var length = this.VerticesInternal.Count;
            for (var loop = 0; loop < length; loop++)
            {
                this.VerticesInternal[loop] = this.VerticesInternal[loop].Copy(
                    Vector3.Transform(this.VerticesInternal[loop].Position, transformMatrix).ToXYZ(),
                    Vector3.TransformNormal(this.VerticesInternal[loop].Normal, transformMatrix));
            }
        }

        /// <summary>
        /// Gets an array with this object as a single item.
        /// </summary>
        public Geometry[] ToSingleItemArray()
        {
            return new[] { this };
        }

        /// <summary>
        /// Relocates all vertices by the given vector
        /// </summary>
        public void UpdateVerticesUsingRelocationBy(Vector3 relocateVector)
        {
            var length = this.VerticesInternal.Count;
            for (var loop = 0; loop < length; loop++)
            {
                this.VerticesInternal[loop] = this.VerticesInternal[loop].Copy(Vector3.Add(this.VerticesInternal[loop].Geometry.Position, relocateVector));
            }
        }

        /// <summary>
        /// Relocates all vertices by the given relocation function (executed for each position vector).
        /// </summary>
        /// <param name="calculatePositionFunc">The function to be applied to each coordinate.</param>
        public void UpdateVerticesUsingRelocationFunc(Func<Vector3, Vector3> calculatePositionFunc)
        {
            var length = this.VerticesInternal.Count;
            for (var loop = 0; loop < length; loop++)
            {
                this.VerticesInternal[loop] = this.VerticesInternal[loop].Copy(calculatePositionFunc(this.Vertices[loop].Position));
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
                if(m_surfaces.Count == 0) {
                    this.CreateSurface(); }
                return m_surfaces[0];
            }
        }

        /// <summary>
        /// Retrieves total count of all vertices within this geometry
        /// </summary>
        public int CountVertices => this.VerticesInternal.Count;

        /// <summary>
        /// A short description for the use of this geometry
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Is this geometry empty?
        /// </summary>
        public bool IsEmpty => this.VerticesInternal.Count == 0 && m_surfaces.Count == 0;

        /// <summary>
        /// The name of this geometry
        /// </summary>
        public string Name
        {
            get => m_name;
            set
            {
                m_name = value;
                if (m_name == null) { m_name = string.Empty; }
            }
        }

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
                for(var loop=0; loop<m_surfaces.Count; loop++)
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

        /// <summary>
        /// Gets or sets the resource source assembly.
        /// </summary>
        public Assembly ResourceSourceAssembly { get; set; }

        /// <summary>
        /// Gets or sets the original source of this geometry.
        /// </summary>
        public ResourceLink ResourceLink { get; set; }

        /// <summary>
        /// Gets a collection of vertices.
        /// </summary>
        internal List<Vertex> VerticesInternal { get; }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Contains all vertices of a Geometry object
        /// </summary>
        public class VertexCollection : IEnumerable<Vertex>
        {
            private List<Vertex> m_vertices;

            internal VertexCollection(List<Vertex> vertices)
            {
                m_vertices = vertices;
            }

            /// <summary>
            /// Adds a vertex to the geometry
            /// </summary>
            public void Add(Vertex vertex)
            {
                m_vertices.Add(vertex);
            }

            /// <summary>
            ///
            /// </summary>
            public IEnumerator<Vertex> GetEnumerator()
            {
                return m_vertices.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return m_vertices.GetEnumerator();
            }

            /// <summary>
            /// Returns the vertex at ghe given index
            /// </summary>
            public Vertex this[int index]
            {
                get => m_vertices[index];
                internal set => m_vertices[index] = value;
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
            private List<GeometrySurface> m_surfaces;

            internal SurfaceCollection(List<GeometrySurface> surfaces)
            {
                m_surfaces = surfaces;
            }

            public void Add(GeometrySurface surface)
            {
                m_surfaces.Add(surface);
            }

            public IEnumerator<GeometrySurface> GetEnumerator()
            {
                return m_surfaces.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return m_surfaces.GetEnumerator();
            }

            /// <summary>
            /// Returns the surface at ghe given index
            /// </summary>
            public GeometrySurface this[int index]
            {
                get => m_surfaces[index];
                internal set => m_surfaces[index] = value;
            }
        }
    }
}