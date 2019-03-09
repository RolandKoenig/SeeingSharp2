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
        private LoadedStructureInfo[] m_loadedStructures;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryResource"/> class.
        /// </summary>
        public GeometryResource(GeometryFactory geometry)
        {
            m_geometry = geometry;

            m_loadedStructures = new LoadedStructureInfo[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryResource"/> class.
        /// </summary>
        /// <param name="vertexStructure">The vertex structures.</param>
        public GeometryResource(VertexStructure vertexStructure)
            : this(new GenericGeometryFactory(vertexStructure))
        {

        }

        /// <summary>
        /// Stores all required data into a new <see cref="ExportGeometryInfo"/>.
        /// </summary>
        public ExportGeometryInfo PrepareForExport()
        {
            return new ExportGeometryInfo(
                Key,
                Geometry);
        }

        /// <summary>
        /// Gets a collection containing all referenced materials.
        /// </summary>
        public IEnumerable<MaterialResource> GetReferencedMaterials()
        {
            var loadedStructures = m_loadedStructures;
            for(var loop=0; loop<loadedStructures.Length; loop++)
            {
                var actLoadedStructure = loadedStructures[loop];

                if(actLoadedStructure?.Material != null) { yield return actLoadedStructure.Material; }
            }
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

            for (var loop = 0; loop < m_loadedStructures.Length; loop++)
            {
                var actLoadedStructure = m_loadedStructures[loop].VertexStructure;
                if (actLoadedStructure.Intersects(pickingRay, pickingOptions, out var currentDistance))
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
            var wasLoaded = IsLoaded;
            if (wasLoaded)
            {
                UnloadResource();
            }

            // Update members
            m_geometry = objectType;
            m_boundingBox = new BoundingBox();

            // Reload resources again if they where loaded before
            if (wasLoaded)
            {
                LoadResource();
            }
        }

        /// <summary>
        /// Redefines the content of this geometry resource.
        /// </summary>
        public void Redefine(ResourceDictionary resources, VertexStructure vertexStructures)
        {
            Redefine(resources, new GenericGeometryFactory(vertexStructures));
        }

        /// <summary>
        /// Performs a simple picking-test.
        /// </summary>
        /// <param name="pickInformation">The that gathers picking information.</param>
        /// <param name="pickingRay">The picking ray.</param>
        public void Pick(PickingInformation pickInformation, Ray pickingRay)
        {
            if (pickingRay.Intersects(m_boundingBox))
            {

            }
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

            for (var loop = 0; loop < m_loadedStructures.Length; loop++)
            {
                var structureToDraw = m_loadedStructures[loop];

                // Apply VertexBuffer
                if (lastVertexBufferID != structureToDraw.VertexBufferID)
                {
                    lastVertexBufferID = structureToDraw.VertexBufferID;
                    deviceContext.InputAssembler.InputLayout = structureToDraw.InputLayout;
                    deviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(structureToDraw.VertexBuffer, structureToDraw.SizePerVertex, 0));
                }

                // Apply IndexBuffer
                if (lastIndexBufferID != structureToDraw.IndexBufferID)
                {
                    deviceContext.InputAssembler.SetIndexBuffer(structureToDraw.IndexBuffer, indexBufferFormat, 0);
                }

                // Apply material
                renderState.ApplyMaterial(structureToDraw.Material);
                D3D11.InputLayout newInputLayout = null;
                if(renderState.ForcedMaterial != null)
                {
                    newInputLayout = renderState.ForcedMaterial.GenerateInputLayout(
                        renderState.Device,
                        StandardVertex.InputElements,
                        MaterialApplyInstancingMode.SingleObject);
                    deviceContext.InputAssembler.InputLayout = newInputLayout;
                }
                try
                {
                    // Draw current rener block
                    deviceContext.DrawIndexed(
                        structureToDraw.IndexCount,
                        structureToDraw.StartIndex,
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
            // Build structures
            VertexStructure[] structures = {
                m_geometry.BuildStructure(new GeometryBuildOptions(device.SupportedDetailLevel))
            };

            // Build BoundingBox around all vertices
            var vertexLocations = new List<Vector3>();
            for (var loop = 0; loop < structures.Length; loop++)
            {
                foreach (var actVertex in structures[loop].Vertices)
                {
                    vertexLocations.Add(actVertex.Position);
                }
            }

            m_boundingBox = BoundingBoxEx.Create(vertexLocations);

            // Build geometry
            m_loadedStructures = BuildBuffers(device, structures, resources);
        }

        /// <inheritdoc />
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            device.EnsureNotNull(nameof(device));

            for (var loop = 0; loop < m_loadedStructures.Length; loop++)
            {
                m_loadedStructures[loop].InputLayout = SeeingSharpUtil.DisposeObject(m_loadedStructures[loop].InputLayout);
                m_loadedStructures[loop].VertexBuffer = SeeingSharpUtil.DisposeObject(m_loadedStructures[loop].VertexBuffer);
                m_loadedStructures[loop].IndexBuffer = SeeingSharpUtil.DisposeObject(m_loadedStructures[loop].IndexBuffer);
            }
            m_loadedStructures = new LoadedStructureInfo[0];

            device = null;
        }

        /// <summary>
        /// Builds vertex and index buffers.
        /// </summary>
        /// <param name="device">The device on which to build all buffers.</param>
        /// <param name="structures">All structure to be loaded.</param>
        /// <param name="resources">The current resource dictionary</param>
        protected virtual LoadedStructureInfo[] BuildBuffers(EngineDevice device, VertexStructure[] structures, ResourceDictionary resources)
        {
            var result = new List<LoadedStructureInfo>(structures.Length * 2);
            var cachedVertices = new List<StandardVertex[]>(2);
            var cachedIndices = new List<int[]>(6);

            var structuresCount = structures.Length;
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
                for (var loop = lastFinishedIndexBufferResultIndex + 1; loop < result.Count; loop++)
                {
                    result[loop].IndexBuffer = indexBuffer;
                }

                lastFinishedIndexBufferResultIndex = result.Count - 1;
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
                for (var loop = lastFinishedVertexBufferResultIndex + 1; loop < result.Count; loop++)
                {
                    result[loop].VertexBuffer = vertexBuffer;
                }

                lastFinishedVertexBufferResultIndex = result.Count - 1;
            }

            // Load all structures into memory within a loop
            for(var loopStruct = 0; loopStruct<structuresCount; loopStruct++)
            {
                var actStructure = structures[loopStruct];

                if(actStructure.CountVertices == 0) { continue; }
                if(actStructure.CountSurfaces == 0) { continue; }

                // Handle vertex data
                var vertexArray = StandardVertex.FromVertexStructure(actStructure);
                if(actVertexCount + vertexArray.Length > MAX_VERTEX_COUNT_PER_BUFFER)
                {
                    FinishVertexBuffer();
                }

                cachedVertices.Add(vertexArray);
                var actBaseVertex = actVertexCount;
                actVertexCount += vertexArray.Length;

                // Sort all surfaces by material
                var surfaceList = new List<VertexStructureSurface>(actStructure.Surfaces);
                surfaceList.Sort((left, right) => left.MaterialProperties.GetHashCode().CompareTo(right.MaterialProperties.GetHashCode()));

                var surfaceCount = surfaceList.Count;

                for(var loopSurface =0; loopSurface<surfaceCount; loopSurface++)
                {
                    var actSurface = surfaceList[loopSurface];

                    if (actSurface.CountTriangles == 0)
                    {
                        continue;
                    }

                    // Handle index data
                    var indexArray = actSurface.GetIndexArray();

                    if(actBaseVertex > 0)
                    {
                        for(var loopIndex =0; loopIndex<indexArray.Length; loopIndex++)
                        {
                            indexArray[loopIndex] = indexArray[loopIndex] + actBaseVertex;
                        }
                    }

                    if(actIndexCount + indexArray.Length > MAX_INDEX_COUNT_PER_BUFFER)
                    {
                        FinishIndexBuffer();
                    }

                    cachedIndices.Add(indexArray);
                    actIndexCount += indexArray.Length;

                    // Get or create the material
                    var lastStructureInfo = result.Count > 0 ? result[result.Count - 1] : null;

                    if (lastStructureInfo != null &&
                        lastStructureInfo.IndexBuffer == null &&
                        actSurface.MaterialProperties.Equals(lastStructureInfo.MaterialProperties))
                    {
                        var actStructureInfo = result[result.Count - 1];
                        actStructureInfo.IndexCount = actStructureInfo.IndexCount + indexArray.Length;
                    }
                    else
                    {
                        var actMaterialResource = resources.GetOrCreateMaterialResourceAndEnsureLoaded(actSurface);

                        // Create some information about the loaded structures
                        var newStructureInfo = new LoadedStructureInfo
                        {
                            VertexBufferID = vertexBufferID,
                            IndexBufferID = indexBufferID,
                            SizePerVertex = StandardVertex.Size,
                            VertexStructure = actStructure,
                            IndexCount = indexArray.Length,
                            StartIndex = actIndexCount - indexArray.Length,
                            Material = actMaterialResource,
                            MaterialProperties = actSurface.MaterialProperties,
                            VertexBuffer = null,
                            IndexBuffer = null
                        };

                        newStructureInfo.InputLayout = newStructureInfo.Material.GenerateInputLayout(
                            device, StandardVertex.InputElements, MaterialApplyInstancingMode.SingleObject);
                        result.Add(newStructureInfo);
                    }
                }
            }

            // Finish all remaining buffers finally
            if(cachedVertices.Count > 0) { FinishVertexBuffer(); }
            if(cachedIndices.Count > 0) { FinishIndexBuffer(); }

            return result.ToArray();
        }

        /// <inheritdoc />
        public override bool IsLoaded => m_loadedStructures.Length > 0;

        /// <summary>
        /// Gets the source of geometry data.
        /// </summary>
        public GeometryFactory Geometry => m_geometry;

        /// <summary>
        /// Gets the bounding box surrounding this object.
        /// </summary>
        public BoundingBox BoundingBox => m_boundingBox;

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        protected class LoadedStructureInfo
        {
            public D3D11.Buffer IndexBuffer;
            public int IndexBufferID;
            public int IndexCount;
            public D3D11.InputLayout InputLayout;
            public MaterialResource Material;
            public MaterialProperties MaterialProperties;
            public int SizePerVertex;
            public int StartIndex;
            public D3D11.Buffer VertexBuffer;
            public int VertexBufferID;
            public VertexStructure VertexStructure;
        }
    }
}