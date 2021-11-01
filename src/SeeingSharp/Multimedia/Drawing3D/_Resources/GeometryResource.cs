using System.Collections.Generic;
using System.Numerics;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class GeometryResource : Resource
    {
        // Constants
        private const int MAX_VERTEX_COUNT_PER_BUFFER = 1000000000;
        private const int MAX_INDEX_COUNT_PER_BUFFER = 1000000000;

        // Misc
        private BoundingBox _boundingBox;
        private GeometryFactory _geometry;

        // Loaded resources
        private RenderingChunkTemplate[] _chunkTemplates;

        /// <inheritdoc />
        public override bool IsLoaded => _chunkTemplates.Length > 0;

        /// <summary>
        /// Gets the source of geometry data.
        /// </summary>
        public GeometryFactory Geometry => _geometry;

        /// <summary>
        /// Gets the bounding box surrounding this object.
        /// </summary>
        public BoundingBox BoundingBox => _boundingBox;

        public int LoadedGeometryTriangleCount { get; private set; }

        public int LoadedGeometryRenderingChunkCount { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryResource"/> class.
        /// </summary>
        public GeometryResource(GeometryFactory geometry)
        {
            _geometry = geometry;

            _chunkTemplates = new RenderingChunkTemplate[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryResource"/> class.
        /// </summary>
        public GeometryResource(Geometry geometry)
            : this(new CustomGeometryFactory(geometry))
        {

        }

        /// <inheritdoc />
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            // Build geometries
            Geometry[] geometries = {
                _geometry.BuildGeometry(new GeometryBuildOptions(device.SupportedDetailLevel))
            };

            // Build BoundingBox around all vertices
            var vertexLocations = new List<Vector3>();
            for (var loop = 0; loop < geometries.Length; loop++)
            {
                foreach (var actVertex in geometries[loop].Vertices)
                {
                    vertexLocations.Add(actVertex.Position);
                }
            }

            _boundingBox = new BoundingBox(vertexLocations);

            // Build geometry
            _chunkTemplates = BuildBuffers(device, geometries);

            // Update counters
            this.LoadedGeometryTriangleCount = 0;
            this.LoadedGeometryRenderingChunkCount = _chunkTemplates.Length;
            for (var loop = 0; loop < _chunkTemplates.Length; loop++)
            {
                this.LoadedGeometryTriangleCount += (_chunkTemplates[loop].IndexCount / 3);
            }
        }

        /// <inheritdoc />
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            device.EnsureNotNull(nameof(device));

            for (var loop = 0; loop < _chunkTemplates.Length; loop++)
            {
                SeeingSharpUtil.DisposeObject(_chunkTemplates[loop]);
            }
            _chunkTemplates = new RenderingChunkTemplate[0];

            this.LoadedGeometryTriangleCount = 0;
            this.LoadedGeometryRenderingChunkCount = 0;
        }

        /// <summary>
        /// Stores all required data into a new <see cref="ExportGeometryInfo"/>.
        /// </summary>
        internal ExportGeometryInfo PrepareForExport()
        {
            return new ExportGeometryInfo(this.Key, this.Geometry);
        }

        /// <summary>
        /// Performs an intersection test using given picking ray and picking options.
        /// </summary>
        /// <param name="pickingRay">The given picking ray.</param>
        /// <param name="pickingOptions">The picking options.</param>
        /// <param name="distance">The distance from origin to the picking point.</param>
        internal bool Intersects(Ray pickingRay, PickingOptions pickingOptions, out float distance)
        {
            distance = float.MaxValue;
            var result = false;

            for (var loop = 0; loop < _chunkTemplates.Length; loop++)
            {
                var actLoadedGeometry = _chunkTemplates[loop].Geometry;
                if (actLoadedGeometry.Intersects(pickingRay, pickingOptions, out var currentDistance))
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
        /// Builds a collection of rendering chunks for given materials.
        /// </summary>
        /// <param name="device">The device on which to load all resources.</param>
        /// <param name="materials">The material's for later rendering.</param>
        internal RenderingChunk[] BuildRenderingChunks(EngineDevice device, MaterialResource[] materials)
        {
            materials.EnsureNotNullOrEmpty(nameof(materials));

            var result = new RenderingChunk[_chunkTemplates.Length];
            for (var loop = 0; loop < result.Length; loop++)
            {
                result[loop] = _chunkTemplates[loop].CreateChunk(
                    device,
                    materials[loop % materials.Length]);
            }

            return result;
        }

        /// <summary>
        /// Builds vertex and index buffers.
        /// </summary>
        /// <param name="device">The device on which to build all buffers.</param>
        /// <param name="geometries">All geometries to be loaded.</param>
        private static RenderingChunkTemplate[] BuildBuffers(EngineDevice device, Geometry[] geometries)
        {
            var resultTemplates = new List<RenderingChunkTemplate>(geometries.Length * 2);

            var cachedVertices = new List<StandardVertex[]>(2);
            var cachedIndices = new List<int[]>(6);

            var geometriesCount = geometries.Length;
            var actVertexCount = 0;
            var actIndexCount = 0;
            var lastFinishedVertexBufferResultIndex = -1;
            var lastFinishedIndexBufferResultIndex = -1;
            var vertexBufferId = 0;
            var indexBufferId = 0;

            // Define the action which finishes current index buffer
            void FinishIndexBuffer()
            {
                if (actIndexCount == 0) { return; }

                // Create the vertex buffer
                var indexBuffer = GraphicsHelper.Internals.CreateImmutableIndexBuffer(device, cachedIndices.ToArray());
                cachedIndices.Clear();
                actIndexCount = 0;
                indexBufferId++;

                // Do also create index buffer
                for (var loop = lastFinishedIndexBufferResultIndex + 1; loop < resultTemplates.Count; loop++)
                {
                    resultTemplates[loop].IndexBuffer = indexBuffer;
                }

                lastFinishedIndexBufferResultIndex = resultTemplates.Count - 1;
            }

            // Define the action which finishes current vertex buffer
            void FinishVertexBuffer()
            {
                // Create the vertex buffer
                var vertexBuffer = GraphicsHelper.Internals.CreateImmutableVertexBuffer(device, cachedVertices.ToArray());
                cachedVertices.Clear();
                actVertexCount = 0;
                vertexBufferId++;

                // Do also finish index buffers in this case
                FinishIndexBuffer();

                // Do also create index buffer
                for (var loop = lastFinishedVertexBufferResultIndex + 1; loop < resultTemplates.Count; loop++)
                {
                    resultTemplates[loop].VertexBuffer = vertexBuffer;
                }

                lastFinishedVertexBufferResultIndex = resultTemplates.Count - 1;
            }

            // Load all geometries into memory within a loop
            for (var loopGeometry = 0; loopGeometry < geometriesCount; loopGeometry++)
            {
                var actGeometry = geometries[loopGeometry];

                if (actGeometry.CountVertices == 0) { continue; }
                if (actGeometry.CountSurfaces == 0) { continue; }

                // Handle vertex data
                var vertexArray = StandardVertex.FromGeometry(actGeometry);
                if (actVertexCount + vertexArray.Length > MAX_VERTEX_COUNT_PER_BUFFER)
                {
                    FinishVertexBuffer();
                }

                cachedVertices.Add(vertexArray);
                var actBaseVertex = actVertexCount;
                actVertexCount += vertexArray.Length;

                // Generate one RenderingChunkTemplate per surface
                var surfaceList = new List<GeometrySurface>(actGeometry.Surfaces);
                var surfaceCount = surfaceList.Count;
                for (var loopSurface = 0; loopSurface < surfaceCount; loopSurface++)
                {
                    var actSurface = surfaceList[loopSurface];
                    if (actSurface.CountTriangles == 0)
                    {
                        continue;
                    }

                    // Handle index data
                    var indexArray = actSurface.GetIndexArray();
                    if (actBaseVertex > 0)
                    {
                        for (var loopIndex = 0; loopIndex < indexArray.Length; loopIndex++)
                        {
                            indexArray[loopIndex] = indexArray[loopIndex] + actBaseVertex;
                        }
                    }
                    if (actIndexCount + indexArray.Length > MAX_INDEX_COUNT_PER_BUFFER)
                    {
                        FinishIndexBuffer();
                    }

                    cachedIndices.Add(indexArray);
                    actIndexCount += indexArray.Length;

                    // Create the rendering chunk
                    var newChunkTemplate = new RenderingChunkTemplate
                    {
                        VertexBufferId = vertexBufferId,
                        IndexBufferId = indexBufferId,
                        SizePerVertex = StandardVertex.Size,
                        Geometry = actGeometry,
                        IndexCount = indexArray.Length,
                        StartIndex = actIndexCount - indexArray.Length,
                        VertexBuffer = null,
                        IndexBuffer = null,
                        InputElements = StandardVertex.InputElements
                    };

                    resultTemplates.Add(newChunkTemplate);
                }
            }

            // Finish all remaining buffers finally
            if (cachedVertices.Count > 0) { FinishVertexBuffer(); }
            if (cachedIndices.Count > 0) { FinishIndexBuffer(); }

            return resultTemplates.ToArray();
        }
    }
}