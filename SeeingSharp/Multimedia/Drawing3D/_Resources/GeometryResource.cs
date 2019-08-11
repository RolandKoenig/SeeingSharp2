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
using System.Collections.Generic;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using SharpDX;
using SharpDX.DXGI;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using Resource = SeeingSharp.Multimedia.Core.Resource;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class GeometryResource : Resource
    {
        // Constants
        private const int MAX_VERTEX_COUNT_PER_BUFFER = 1000000000;
        private const int MAX_INDEX_COUNT_PER_BUFFER = 1000000000;

        // Misc
        private BoundingBox m_boundingBox;
        private GeometryFactory m_geometry;

        // Loaded resources
        private RenderingChunkTemplate[] m_chunkTemplates;
        private RenderingChunk[] m_chunks;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryResource"/> class.
        /// </summary>
        public GeometryResource(GeometryFactory geometry)
        {
            m_geometry = geometry;

            m_chunkTemplates = new RenderingChunkTemplate[0];
            m_chunks = new RenderingChunk[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryResource"/> class.
        /// </summary>
        public GeometryResource(Geometry geometry)
            : this(new CustomGeometryFactory(geometry))
        {

        }

        /// <summary>
        /// Stores all required data into a new <see cref="ExportGeometryInfo"/>.
        /// </summary>
        public ExportGeometryInfo PrepareForExport()
        {
            return new ExportGeometryInfo(this.Key, this.Geometry);
        }

        /// <summary>
        /// Performs an intersection test using given picking ray and picking options.
        /// </summary>
        /// <param name="pickingRay">The given picking ray.</param>
        /// <param name="pickingOptions">The picking options.</param>
        /// <param name="distance">The distance from origin to the picking point.</param>
        public bool Intersects(Ray pickingRay, PickingOptions pickingOptions, out float distance)
        {
            distance = float.MaxValue;
            var result = false;

            for (var loop = 0; loop < m_chunkTemplates.Length; loop++)
            {
                var actLoadedGeometry = m_chunkTemplates[loop].Geometry;
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
        /// Redefines the content of this geometry resource.
        /// </summary>
        public void Redefine(ResourceDictionary resources, GeometryFactory objectType)
        {
            // Unload resource first if it was loaded
            var wasLoaded = this.IsLoaded;
            if (wasLoaded)
            {
                this.UnloadResource();
            }

            // Update members
            m_geometry = objectType;
            m_boundingBox = new BoundingBox();

            // Reload resources again if they where loaded before
            if (wasLoaded)
            {
                this.LoadResource();
            }
        }

        /// <summary>
        /// Redefines the content of this geometry resource.
        /// </summary>
        public void Redefine(ResourceDictionary resources, Geometry geometry)
        {
            this.Redefine(resources, new CustomGeometryFactory(geometry));
        }

        internal RenderingChunk[] BuildRenderingChunks(EngineDevice device, MaterialResource[] materials)
        {
            materials.EnsureNotNullOrEmpty(nameof(materials));

            var result = new RenderingChunk[m_chunkTemplates.Length];
            for (var loop = 0; loop < result.Length; loop++)
            {
                result[loop] = m_chunkTemplates[loop].CreateChunk(
                    device,
                    materials[loop % materials.Length]);
            }

            return result;
        }

        /// <summary>
        /// Renders this GeometryResource.
        /// </summary>
        /// <param name="renderState">Current render state.</param>
        public void Render(RenderState renderState)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;
            var indexBufferFormat = renderState.Device.SupportsOnly16BitIndexBuffer ? Format.R16_UInt : Format.R32_UInt;

            var lastVertexBufferID = -1;
            var lastIndexBufferID = -1;
            for (var loop = 0; loop < m_chunks.Length; loop++)
            {
                var actChunk = m_chunks[loop];

                // Apply VertexBuffer
                if (lastVertexBufferID != actChunk.Template.VertexBufferID)
                {
                    lastVertexBufferID = actChunk.Template.VertexBufferID;
                    deviceContext.InputAssembler.InputLayout = actChunk.InputLayout;
                    deviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(actChunk.Template.VertexBuffer, actChunk.Template.SizePerVertex, 0));
                }

                // Apply IndexBuffer
                if (lastIndexBufferID != actChunk.Template.IndexBufferID)
                {
                    deviceContext.InputAssembler.SetIndexBuffer(actChunk.Template.IndexBuffer, indexBufferFormat, 0);
                }

                // Apply material
                renderState.ApplyMaterial(actChunk.Material);
                D3D11.InputLayout newInputLayout = null;
                if(renderState.ForcedMaterial != null)
                {
                    newInputLayout = renderState.ForcedMaterial.GenerateInputLayout(
                        renderState.Device,
                        StandardVertex.InputElements);
                    deviceContext.InputAssembler.InputLayout = newInputLayout;
                }
                try
                {
                    // Draw current rener block
                    deviceContext.DrawIndexed(
                        actChunk.Template.IndexCount,
                        actChunk.Template.StartIndex,
                        0);
                }
                finally
                {
                    if (newInputLayout != null)
                    {
                        deviceContext.InputAssembler.InputLayout = null;
                        SeeingSharpUtil.SafeDispose(ref newInputLayout);
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            // Build geometries
            Geometry[] geometries = {
                m_geometry.BuildGeometry(new GeometryBuildOptions(device.SupportedDetailLevel))
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

            m_boundingBox = BoundingBoxEx.Create(vertexLocations);

            // Build geometry
            var builtChunks = this.BuildBuffers(device, geometries, resources);
            m_chunkTemplates = builtChunks.Item1;
            m_chunks = builtChunks.Item2;
        }

        /// <inheritdoc />
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            device.EnsureNotNull(nameof(device));

            for (var loop = 0; loop < m_chunkTemplates.Length; loop++)
            {
                SeeingSharpUtil.DisposeObject(m_chunkTemplates[loop]);
                SeeingSharpUtil.DisposeObject(m_chunks[loop]);
            }
            m_chunkTemplates = new RenderingChunkTemplate[0];
            m_chunks = new RenderingChunk[0];
        }

        /// <summary>
        /// Builds vertex and index buffers.
        /// </summary>
        /// <param name="device">The device on which to build all buffers.</param>
        /// <param name="geometries">All geometries to be loaded.</param>
        /// <param name="resources">The current resource dictionary</param>
        private (RenderingChunkTemplate[], RenderingChunk[]) BuildBuffers(EngineDevice device, Geometry[] geometries, ResourceDictionary resources)
        {
            var resultTemplates = new List<RenderingChunkTemplate>(geometries.Length * 2);
            var resultChunks = new List<RenderingChunk>(resultTemplates.Capacity);

            var cachedVertices = new List<StandardVertex[]>(2);
            var cachedIndices = new List<int[]>(6);

            var geometriesCount = geometries.Length;
            var actVertexCount = 0;
            var actIndexCount = 0;
            var lastFinishedVertexBufferResultIndex = -1;
            var lastFinishedIndexBufferResultIndex = -1;
            var vertexBufferID = 0;
            var indexBufferID = 0;

            // Define the action which finishes current index buffer
            void FinishIndexBuffer()
            {
                // Create the vertex buffer
                var indexBuffer = GraphicsHelper.CreateImmutableIndexBuffer(device, cachedIndices.ToArray());
                cachedIndices.Clear();
                actIndexCount = 0;
                indexBufferID++;

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
                var vertexBuffer = GraphicsHelper.CreateImmutableVertexBuffer(device, cachedVertices.ToArray());
                cachedVertices.Clear();
                actVertexCount = 0;
                vertexBufferID++;

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
            for(var loopGeometry = 0; loopGeometry<geometriesCount; loopGeometry++)
            {
                var actGeometry = geometries[loopGeometry];

                if(actGeometry.CountVertices == 0) { continue; }
                if(actGeometry.CountSurfaces == 0) { continue; }

                // Handle vertex data
                var vertexArray = StandardVertex.FromGeometry(actGeometry);
                if(actVertexCount + vertexArray.Length > MAX_VERTEX_COUNT_PER_BUFFER)
                {
                    FinishVertexBuffer();
                }

                cachedVertices.Add(vertexArray);
                var actBaseVertex = actVertexCount;
                actVertexCount += vertexArray.Length;

                // Sort all surfaces by material
                var surfaceList = new List<GeometrySurface>(actGeometry.Surfaces);
                surfaceList.Sort((left, right) => left.CommonMaterialProperties.GetHashCode().CompareTo(right.CommonMaterialProperties.GetHashCode()));

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

                    // Get the material resource for the given surface
                    var actMaterialResource = resources.GetOrCreateMaterialResourceAndEnsureLoaded(actSurface);

                    // Create the rendering chunk
                    var newChunkTemplate = new RenderingChunkTemplate()
                    {
                        VertexBufferID = vertexBufferID,
                        IndexBufferID = indexBufferID,
                        SizePerVertex = StandardVertex.Size,
                        Geometry = actGeometry,
                        IndexCount = indexArray.Length,
                        StartIndex = actIndexCount - indexArray.Length,
                        VertexBuffer = null,
                        IndexBuffer = null,
                        InputElements = StandardVertex.InputElements
                    };
                    var newChunk = new RenderingChunk()
                    {
                        Template = newChunkTemplate,
                        Material = actMaterialResource,
                        InputLayout = actMaterialResource.GenerateInputLayout(device, StandardVertex.InputElements)
                    };
                
                    resultTemplates.Add(newChunkTemplate);
                    resultChunks.Add(newChunk);
                }
            }

            // Finish all remaining buffers finally
            if(cachedVertices.Count > 0) { FinishVertexBuffer(); }
            if(cachedIndices.Count > 0) { FinishIndexBuffer(); }

            return (resultTemplates.ToArray(), resultChunks.ToArray());
        }

        /// <inheritdoc />
        public override bool IsLoaded => m_chunkTemplates.Length > 0;

        /// <summary>
        /// Gets the source of geometry data.
        /// </summary>
        public GeometryFactory Geometry => m_geometry;

        /// <summary>
        /// Gets the bounding box surrounding this object.
        /// </summary>
        public BoundingBox BoundingBox => m_boundingBox;
    }
}