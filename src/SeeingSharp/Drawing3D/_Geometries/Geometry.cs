using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using SeeingSharp.Checking;
using SeeingSharp.Core;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.Drawing3D
{
    /// <summary>
    /// Describes a 3D geometry in memory.
    /// </summary>
    public class Geometry
    {
        // Geometry
        private List<GeometrySurface> _surfaces;
        private UnsafeList<VertexBasic> _verticesBasic;
        private UnsafeList<VertexBinormalTangent> _verticesBinormalTangents;

        // Members for build time transform
        private bool _buildTimeTransformEnabled;
        private Matrix4x4 _buildTransformMatrix;
        private Func<VertexBasic, VertexBasic> _buildTimeTransformFunc;

        /// <summary>
        /// Gets the first surface of this geometry.
        /// It there is no surface, then one gets created automatically.
        /// </summary>
        public GeometrySurface FirstSurface
        {
            get
            {
                if (_surfaces.Count == 0)
                {
                    this.CreateSurface();
                }
                return _surfaces[0];
            }
        }

        /// <summary>
        /// Retrieves total count of all vertices within this geometry
        /// </summary>
        public int CountVertices => _verticesBasic.Count;

        /// <summary>
        /// A short description for the use of this geometry
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Is this geometry empty?
        /// </summary>
        public bool IsEmpty => _verticesBasic.Count == 0 && _surfaces.Count == 0;

        /// <summary>
        /// Gets a collection of vertices.
        /// </summary>
        public VertexCollection Vertices { get; }

        public SurfaceCollection Surfaces { get; }

        public int CountSurfaces => _surfaces.Count;

        public int CountTriangles
        {
            get
            {
                var sum = 0;
                for (var loop = 0; loop < _surfaces.Count; loop++)
                {
                    sum += _surfaces[loop].CountTriangles;
                }
                return sum;
            }
        }

        public int CountIndices
        {
            get
            {
                var sum = 0;
                for (var loop = 0; loop < _surfaces.Count; loop++)
                {
                    sum += _surfaces[loop].CountIndices;
                }
                return sum;
            }
        }

        public bool HasBinormalsTangents => _verticesBinormalTangents != null;

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
            _verticesBasic = new UnsafeList<VertexBasic>(verticesCapacity);
            if (hasBinormalsTangents) { _verticesBinormalTangents = new UnsafeList<VertexBinormalTangent>(verticesCapacity); }

            // Triangle data
            _surfaces = new List<GeometrySurface>();

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
            _surfaces.Add(newSurface);
            return newSurface;
        }

        /// <summary>
        /// Gets the reference to the vertex at the given index.
        /// </summary>
        /// <param name="vertexIndex">The index of the vertex.</param>
        public ref VertexBasic GetVertexBasicRef(int vertexIndex)
        {
            return ref _verticesBasic.BackingArray[vertexIndex];
        }

        /// <summary>
        /// Gets the reference to the vertex at the given index.
        /// </summary>
        /// <param name="vertexIndex">The index of the vertex.</param>
        public ref VertexBinormalTangent GetVertexBinormalTangentRef(int vertexIndex)
        {
            if(_verticesBinormalTangents == null){ throw new SeeingSharpException("Unable to get binormal/tangent information: This geometry doesn't provide them!");}
            return ref _verticesBinormalTangents.BackingArray[vertexIndex];
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

            this.UpdateVerticesUsingTranslation(moveToTargetCenter);
        }

        public void RealignToFloorCenter()
        {
            var fullBoundingBox = this.GenerateBoundingBox();
            var fullCenter = fullBoundingBox.GetMiddleCenter();
            var targetCenter = new Vector3(0f, fullBoundingBox.GetSize().Y / 2f, 0f);
            var moveToTargetCenter = targetCenter - fullCenter;

            this.UpdateVerticesUsingTranslation(moveToTargetCenter);
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
            this.UpdateVerticesUsingTranslation(Vector3.Negate(boundingBox.CornerA));
            this.UpdateVerticesUsingTranslation(actPosition => new Vector3(
                actPosition.X * resizeFactorX,
                actPosition.Y * resizeFactorY,
                actPosition.Z * resizeFactorZ));
            this.UpdateVerticesUsingTranslation(targetCornerALocation);
        }

        public void RemoveSurface(GeometrySurface surface)
        {
            _surfaces.Remove(surface);
        }

        /// <summary>
        /// Enables build-time transform using the given matrix.
        /// </summary>
        /// <param name="transformMatrix">Transform matrix.</param>
        public void EnableBuildTimeTransform(Matrix4x4 transformMatrix)
        {
            _buildTimeTransformEnabled = true;
            _buildTransformMatrix = transformMatrix;
            _buildTimeTransformFunc = null;
        }

        /// <summary>
        /// Enables build-time transform using given transform method.
        /// </summary>
        public void EnableBuildTimeTransform(Func<VertexBasic, VertexBasic> transformFunc)
        {
            _buildTimeTransformEnabled = true;
            _buildTransformMatrix = Matrix4x4.Identity;
            _buildTimeTransformFunc = transformFunc;
        }

        /// <summary>
        /// Disables build-time transform.
        /// </summary>
        public void DisableBuildTimeTransform()
        {
            _buildTimeTransformEnabled = false;
            _buildTransformMatrix = Matrix4x4.Identity;
        }

        /// <summary>
        /// Performs a simple picking test against all triangles of this object.
        /// </summary>
        /// <param name="pickingRay">The picking ray.</param>
        /// <param name="distance">Additional picking options.</param>
        /// <param name="pickingOptions">The distance if picking succeeds.</param>
        public bool Intersects(Ray pickingRay, PickingOptions pickingOptions, out float distance)
        {
            var surfaceCount = _surfaces.Count;
            for (var loop = 0; loop < surfaceCount; loop++)
            {
                if (_surfaces[loop].Intersects(pickingRay, pickingOptions, out distance))
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
            var baseIndex = _verticesBasic.Count;

            this.AddVertices(otherGeometry);
            var otherGeometrySurfaceCount = otherGeometry._surfaces.Count;

            for (var loopSurface = 0; loopSurface < otherGeometrySurfaceCount; loopSurface++)
            {
                _surfaces.Add(otherGeometry._surfaces[loopSurface].Clone(
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
            var startVertex = _verticesBasic.Count;

            // Prepare vertex array for new capacity
            _verticesBasic.Capacity = startVertex + otherGeometry.CountVertices;

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
            if (_buildTimeTransformEnabled)
            {
                if (_buildTimeTransformFunc != null) { vertex = _buildTimeTransformFunc(vertex); }
                else
                {
                    vertex.Position = Vector3.Transform(vertex.Position, _buildTransformMatrix);
                    vertex.Normal = Vector3.TransformNormal(vertex.Normal, _buildTransformMatrix);
                }
            }

            // AddObject the vertex and return the index
            _verticesBasic.Add(vertex);
            _verticesBinormalTangents?.Add(new VertexBinormalTangent());
            return _verticesBasic.Count - 1;
        }

        /// <summary>
        /// Calculates tangents for all vectors.
        /// </summary>
        public void CalculateTangentsAndBinormals()
        {
            foreach (var actSurface in _surfaces)
            {
                actSurface.CalculateTangentsAndBinormals();
            }
        }

        /// <summary>
        /// Recalculates all normals
        /// </summary>
        public void CalculateNormalsFlat()
        {
            foreach (var actSurface in _surfaces)
            {
                actSurface.CalculateNormalsFlat();
            }
        }

        /// <summary>
        /// Calculates normals for all triangles of this geometry.
        /// </summary>
        public void CalculateNormals()
        {
            this.CalculateNormals(0, _verticesBasic.Count);
        }

        /// <summary>
        /// Calculates normals for all triangles specified by the given parameters.
        /// </summary>
        /// <param name="startVertex">The vertex index on which to start.</param>
        /// <param name="vertexCount">Total count of vertices to be updated.</param>
        public void CalculateNormals(int startVertex, int vertexCount)
        {
            if (startVertex < 0 || startVertex >= _verticesBasic.Count) { throw new ArgumentException("startVertex"); }
            if (vertexCount + startVertex > _verticesBasic.Count) { throw new ArgumentException("vertexCount"); }

            for (var actVertexIndex = startVertex; actVertexIndex < startVertex + vertexCount; actVertexIndex++)
            {
                // Find all triangles connected to this vertex and get normals from them
                var finalNormalHelper = Vector3.Zero;
                var normalCount = 0;

                var surfaceCount = _surfaces.Count;

                for (var actSurfaceIndex = 0; actSurfaceIndex < surfaceCount; actSurfaceIndex++)
                {
                    var actSurface = _surfaces[actSurfaceIndex];
                    var triangleCount = actSurface.CountTriangles;

                    for (var loopTriangle = 0; loopTriangle < triangleCount; loopTriangle++)
                    {
                        var triangleStartIndex = loopTriangle * 3;

                        if (actSurface.Indices[triangleStartIndex] == actVertexIndex ||
                            actSurface.Indices[triangleStartIndex + 1] == actVertexIndex ||
                            actSurface.Indices[triangleStartIndex + 2] == actVertexIndex)
                        {
                            ref var v1 = ref _verticesBasic.BackingArray[actSurface.Indices[triangleStartIndex]];
                            ref var v2 = ref _verticesBasic.BackingArray[actSurface.Indices[triangleStartIndex + 1]];
                            ref var v3 = ref _verticesBasic.BackingArray[actSurface.Indices[triangleStartIndex + 2]];

                            finalNormalHelper += Vector3Ex.CalculateTriangleNormal(v1.Position, v2.Position, v3.Position, false);

                            normalCount++;
                        }
                    }
                }

                // Calculate final normal
                if (normalCount > 0)
                {
                    ref var actVertex = ref _verticesBasic.BackingArray[actVertexIndex];
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
            var vertexCount = _verticesBasic.Count;
            var hasBinormalsTangents = this.HasBinormalsTangents;
            var result = new Geometry(
                vertexCount * capacityMultiplier,
                hasBinormalsTangents);

            // Copy geometry
            if (copyGeometryData)
            {
                for (var loop = 0; loop < vertexCount; loop++)
                {
                    result._verticesBasic.Add(_verticesBasic[loop]);
                    if(hasBinormalsTangents){ result._verticesBinormalTangents.Add(_verticesBinormalTangents[loop]); }
                }
            }

            // Copy surfaces
            foreach (var actSurface in _surfaces)
            {
                result._surfaces.Add(actSurface.Clone(result, copyGeometryData, capacityMultiplier));
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

            foreach (var actVertex in _verticesBasic)
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
            for (var loop = 0; loop < _surfaces.Count; loop++)
            {
                _surfaces[loop].ToggleTriangleIndexOrder();
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
            this.UpdateVerticesUsingTranslation(givenVector => new Vector3(
                givenVector.X, givenVector.Y,
                centerCoord.Z + (centerCoord.Z - givenVector.Z)));

            // Now change index ordering
            foreach (var actSurface in _surfaces)
            {
                actSurface.ToggleCoordinateSystemInternal();
            }
        }

        /// <summary>
        /// Transforms positions and normals of all vertices using the given transform matrix
        /// </summary>
        public void TransformVertices(Matrix4x4 transformMatrix)
        {
            var length = _verticesBasic.Count;
            for (var loop = 0; loop < length; loop++)
            {
                ref var actVertex = ref _verticesBasic.BackingArray[loop];
                actVertex.Position = Vector3.Transform(actVertex.Position, transformMatrix);
                actVertex.Normal = Vector3.TransformNormal(actVertex.Normal, transformMatrix);
            }
        }

        /// <summary>
        /// Relocates all vertices by the given vector
        /// </summary>
        public void UpdateVerticesUsingTranslation(Vector3 relocateVector)
        {
            var length = _verticesBasic.Count;
            for (var loop = 0; loop < length; loop++)
            {
                ref var actVertex = ref _verticesBasic.BackingArray[loop];
                actVertex.Position += relocateVector;
            }
        }

        /// <summary>
        /// Relocates all vertices by the given relocation function (executed for each position vector).
        /// </summary>
        /// <param name="calculatePositionFunc">The function to be applied to each coordinate.</param>
        public void UpdateVerticesUsingTranslation(Func<Vector3, Vector3> calculatePositionFunc)
        {
            var length = _verticesBasic.Count;
            for (var loop = 0; loop < length; loop++)
            {
                ref var actVertex = ref _verticesBasic.BackingArray[loop];
                actVertex.Position = calculatePositionFunc(actVertex.Position);
            }
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Contains all vertices of a Geometry object
        /// </summary>
        public class VertexCollection : IEnumerable<VertexBasic>
        {
            private Geometry _owner;

            /// <summary>
            /// Returns the vertex at ghe given index
            /// </summary>
            public VertexBasic this[int index] => _owner._verticesBasic[index];

            internal VertexCollection(Geometry owner)
            {
                _owner = owner;
            }

            /// <summary>
            /// Adds a vertex to the geometry
            /// </summary>
            public void Add(VertexBasic vertex)
            {
                _owner.AddVertex(vertex);
            }

            public IEnumerator<VertexBasic> GetEnumerator()
            {
                return _owner._verticesBasic.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _owner._verticesBasic.GetEnumerator();
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
            private Geometry _owner;

            /// <summary>
            /// Returns the surface at ghe given index
            /// </summary>
            public GeometrySurface this[int index]
            {
                get => _owner._surfaces[index];
                internal set => _owner._surfaces[index] = value;
            }

            internal SurfaceCollection(Geometry owner)
            {
                _owner = owner;
            }

            public void Add(GeometrySurface surface)
            {
                _owner._surfaces.Add(surface);
            }

            public IEnumerator<GeometrySurface> GetEnumerator()
            {
                return _owner._surfaces.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _owner._surfaces.GetEnumerator();
            }
        }
    }
}