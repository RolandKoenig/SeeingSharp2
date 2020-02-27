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
using SeeingSharp.Util;
using System.Numerics;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    /// <summary>
    /// This class is responsible for rendering simple lines into the 3d scene.
    /// Use the LineData property to define all points of the line.
    /// </summary>
    public class WireObject : SceneSpacialObject
    {
        // Configuration
        private bool m_forceReloadLineData;
        private Line[] m_lineData;
        
        // Bounding volumes
        private BoundingBox m_boundingBox;
        private BoundingSphere m_boundingSphere;

        // Direct3D resources
        private IndexBasedDynamicCollection<LocalResourceData> m_localResources;

        /// <summary>
        /// Initializes a new instance of the <see cref="WireObject" /> class.
        /// </summary>
        public WireObject()
        {
            this.Color = Color4.Black;
            m_localResources = new IndexBasedDynamicCollection<LocalResourceData>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WireObject" /> class.
        /// </summary>
        public WireObject(params Line[] lines)
            : this()
        {
            m_lineData = lines;

            this.UpdateBoundingVolumes();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WireObject" /> class.
        /// </summary>
        public WireObject(Color4 lineColor, params Line[] lines)
            : this()
        {
            this.Color = lineColor;
            m_lineData = lines;

            this.UpdateBoundingVolumes();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WireObject" /> class.
        /// </summary>
        public WireObject(Color4 lineColor, BoundingBox boundingBox)
            : this()
        {
            this.Color = lineColor;

            var aBottom = new Vector3(boundingBox.Minimum.X, boundingBox.Minimum.Y, boundingBox.Minimum.Z);
            var bBottom = new Vector3(boundingBox.Maximum.X, boundingBox.Minimum.Y, boundingBox.Minimum.Z);
            var cBottom = new Vector3(boundingBox.Maximum.X, boundingBox.Minimum.Y, boundingBox.Maximum.Z);
            var dBottom = new Vector3(boundingBox.Minimum.X, boundingBox.Minimum.Y, boundingBox.Maximum.Z);
            var aTop = new Vector3(boundingBox.Minimum.X, boundingBox.Maximum.Y, boundingBox.Minimum.Z);
            var bTop = new Vector3(boundingBox.Maximum.X, boundingBox.Maximum.Y, boundingBox.Minimum.Z);
            var cTop = new Vector3(boundingBox.Maximum.X, boundingBox.Maximum.Y, boundingBox.Maximum.Z);
            var dTop = new Vector3(boundingBox.Minimum.X, boundingBox.Maximum.Y, boundingBox.Maximum.Z);
            m_lineData = new[]
            {
                new Line(aBottom, bBottom),
                new Line(bBottom, cBottom),
                new Line(cBottom, dBottom),
                new Line(dBottom, aBottom),

                new Line(aTop, bTop),
                new Line(bTop, cTop),
                new Line(cTop, dTop),
                new Line(dTop, aTop),

                new Line(aBottom, aTop),
                new Line(bBottom, bTop),
                new Line(cBottom, cTop),
                new Line(dBottom, dTop),
            };

            this.UpdateBoundingVolumes();
        }

        /// <summary>
        /// Loads all resources of the object.
        /// </summary>
        /// <param name="device">Current graphics device.</param>
        /// <param name="resourceDictionary">Current resource dictionary.</param>
        public override void LoadResources(EngineDevice device, ResourceDictionary resourceDictionary)
        {
            m_localResources.AddObject(
                new LocalResourceData
                {
                    LineVertexBuffer = null
                },
                device.DeviceIndex);
        }

        /// <summary>
        /// Are resources loaded for the given device?
        /// </summary>
        /// <param name="device">The device for which to check.</param>
        public override bool IsLoaded(EngineDevice device)
        {
            return m_localResources.HasObjectAt(device.DeviceIndex);
        }

        /// <summary>
        /// Tries to get the bounding box for the given render-loop.
        /// Returns BoundingBox.Empty, if it is not available.
        /// </summary>
        /// <param name="viewInfo">The ViewInformation for which to get the BoundingBox.</param>
        public override BoundingBox TryGetBoundingBox(ViewInformation viewInfo)
        {
            var result = m_boundingBox;
            result.Transform(this.Transform);
            return result;
        }

        /// <summary>
        /// Tries to get the bounding sphere for the given render-loop.
        /// Returns BoundingSphere.Empty, if it is not available.
        /// </summary>
        /// <param name="viewInfo">The ViewInformation for which to get the BoundingSphere.</param>
        public override BoundingSphere TryGetBoundingSphere(ViewInformation viewInfo)
        {
            var result = m_boundingSphere;
            result.Transform(this.Transform);
            return result;
        }

        /// <summary>
        /// Unloads all resources of the object.
        /// </summary>
        public override void UnloadResources()
        {
            base.UnloadResources();

            // Dispose all locally created resources
            foreach (var actLocalResource in m_localResources)
            {
                if (actLocalResource == null) { continue; }

                SeeingSharpUtil.SafeDispose(ref actLocalResource.LineVertexBuffer);
            }

            m_localResources.Clear();
        }

        /// <summary>
        /// Updates the object.
        /// </summary>
        /// <param name="updateState">Current update state.</param>
        protected override void UpdateInternal(SceneRelatedUpdateState updateState)
        {
            base.UpdateInternal(updateState);

            // Handle line data reloading flag
            if (m_forceReloadLineData)
            {
                m_localResources.ForEachInEnumeration(actItem => actItem.LineDataLoaded = false);
                m_forceReloadLineData = false;

                this.UpdateBoundingVolumes();
            }
        }

        /// <summary>
        /// Updates this object for the given view.
        /// </summary>
        /// <param name="updateState">Current state of the update pass.</param>
        /// <param name="layerViewSubset">The layer view subset which called this update method.</param>
        protected override void UpdateForViewInternal(SceneRelatedUpdateState updateState, ViewRelatedSceneLayerSubset layerViewSubset)
        {
            if (this.CountRenderPassSubscriptions(layerViewSubset) == 0)
            {
                this.SubscribeToPass(RenderPassInfo.PASS_LINE_RENDER, layerViewSubset, this.RenderLines);
            }
        }

        /// <summary>
        /// Updates bounding volumes.
        /// </summary>
        private void UpdateBoundingVolumes()
        {
            var lines = this.LineData;

            var boundBoxCalc = new ObjectTreeBoundingBoxCalculator();
            for(var loop=0; loop<lines.Length; loop++)
            {
                ref var actLine = ref lines[loop];
                boundBoxCalc.AddCoordinate(ref actLine.StartPosition);
                boundBoxCalc.AddCoordinate(ref actLine.EndPosition);
            }

            if (boundBoxCalc.CanCreateBoundingBox)
            {
                m_boundingBox = boundBoxCalc.CreateBoundingBox();
                BoundingSphere.FromBox(ref m_boundingBox, out m_boundingSphere);
            }
            else
            {
                m_boundingBox = BoundingBox.Empty;
                m_boundingSphere = BoundingSphere.Empty;
            }
        }

        /// <summary>
        /// Main render method for the wire object.
        /// </summary>
        /// <param name="renderState">Current render state.</param>
        private void RenderLines(RenderState renderState)
        {
            this.UpdateAndApplyRenderParameters(renderState);

            var resourceData = m_localResources[renderState.DeviceIndex];

            // Load line data to memory if needed
            if (!resourceData.LineDataLoaded)
            {
                if (m_lineData == null ||
                    m_lineData.Length == 0)
                {
                    return;
                }

                // Loading of line data
                SeeingSharpUtil.SafeDispose(ref resourceData.LineVertexBuffer);
                resourceData.LineVertexBuffer = GraphicsHelper.Internals.CreateImmutableVertexBuffer(renderState.Device, m_lineData);
                resourceData.LineDataLoaded = true;
            }

            // Apply vertex buffer and draw lines
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;
            deviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(resourceData.LineVertexBuffer, LineVertex.Size, 0));
            deviceContext.Draw(m_lineData.Length * 2, 0);
        }

        /// <summary>
        /// Gets or sets current line data.
        /// </summary>
        public Line[] LineData
        {
            get => m_lineData;
            set
            {
                if (m_lineData != value)
                {
                    m_lineData = value;
                    m_forceReloadLineData = true;
                }
            }
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class LocalResourceData
        {
            public bool LineDataLoaded;
            public D3D11.Buffer LineVertexBuffer;
        }
    }
}