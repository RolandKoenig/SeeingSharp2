using System.Numerics;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
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
        private bool _forceReloadLineData;
        private Line[] _lineData;

        // Bounding volumes
        private BoundingBox _boundingBox;
        private BoundingSphere _boundingSphere;

        // Direct3D resources
        private IndexBasedDynamicCollection<LocalResourceData> _localResources;

        /// <summary>
        /// Gets or sets current line data.
        /// </summary>
        public Line[] LineData
        {
            get => _lineData;
            set
            {
                if (_lineData != value)
                {
                    _lineData = value;
                    _forceReloadLineData = true;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WireObject" /> class.
        /// </summary>
        public WireObject()
        {
            this.Color = Color4.Black;
            _localResources = new IndexBasedDynamicCollection<LocalResourceData>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WireObject" /> class.
        /// </summary>
        public WireObject(params Line[] lines)
            : this()
        {
            _lineData = lines;

            this.UpdateBoundingVolumes();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WireObject" /> class.
        /// </summary>
        public WireObject(Color4 lineColor, params Line[] lines)
            : this()
        {
            this.Color = lineColor;
            _lineData = lines;

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
            _lineData = new[]
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
                new Line(dBottom, dTop)
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
            _localResources.AddObject(
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
            return _localResources.HasObjectAt(device.DeviceIndex);
        }

        /// <summary>
        /// Tries to get the bounding box for the given render-loop.
        /// Returns BoundingBox.Empty, if it is not available.
        /// </summary>
        /// <param name="viewInfo">The ViewInformation for which to get the BoundingBox.</param>
        public override BoundingBox TryGetBoundingBox(ViewInformation viewInfo)
        {
            var result = _boundingBox;
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
            var result = _boundingSphere;
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
            foreach (var actLocalResource in _localResources)
            {
                if (actLocalResource == null) { continue; }

                SeeingSharpUtil.SafeDispose(ref actLocalResource.LineVertexBuffer);
            }

            _localResources.Clear();
        }

        /// <summary>
        /// Updates the object.
        /// </summary>
        /// <param name="updateState">Current update state.</param>
        protected override void UpdateInternal(SceneRelatedUpdateState updateState)
        {
            base.UpdateInternal(updateState);

            // Handle line data reloading flag
            if (_forceReloadLineData)
            {
                _localResources.ForEachInEnumeration(actItem => actItem.LineDataLoaded = false);
                _forceReloadLineData = false;

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
                _boundingBox = boundBoxCalc.CreateBoundingBox();
                BoundingSphere.FromBox(ref _boundingBox, out _boundingSphere);
            }
            else
            {
                _boundingBox = BoundingBox.Empty;
                _boundingSphere = BoundingSphere.Empty;
            }
        }

        /// <summary>
        /// Main render method for the wire object.
        /// </summary>
        /// <param name="renderState">Current render state.</param>
        private void RenderLines(RenderState renderState)
        {
            this.UpdateAndApplyRenderParameters(renderState);

            var resourceData = _localResources[renderState.DeviceIndex];

            // Load line data to memory if needed
            if (!resourceData.LineDataLoaded)
            {
                if (_lineData == null ||
                    _lineData.Length == 0)
                {
                    return;
                }

                // Loading of line data
                SeeingSharpUtil.SafeDispose(ref resourceData.LineVertexBuffer);
                resourceData.LineVertexBuffer = GraphicsHelper.Internals.CreateImmutableVertexBuffer(renderState.Device, _lineData);
                resourceData.LineDataLoaded = true;
            }

            // Apply vertex buffer and draw lines
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;
            deviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(resourceData.LineVertexBuffer, LineVertex.Size, 0));
            deviceContext.Draw(_lineData.Length * 2, 0);
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