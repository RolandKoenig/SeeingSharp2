using System;
using System.Numerics;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class Mesh : SceneSpacialObject
    {
        // Resources
        private readonly IndexBasedDynamicCollection<GeometryResource> _localResGeometry;
        private readonly IndexBasedDynamicCollection<MaterialResource[]> _localResMaterials;
        private readonly IndexBasedDynamicCollection<RenderingChunk[]> _localChunks;
        private bool _passRelevantValuesChanged;

        // Configuration members
        private NamedOrGenericKey _resGeometryKey;
        private NamedOrGenericKey[] _resMaterialResourceKeys;

        public override bool IsExportable => false;

        public NamedOrGenericKey GeometryResourceKey => _resGeometryKey;

        public ReadOnlySpan<NamedOrGenericKey> MaterialResourceKeys => new ReadOnlySpan<NamedOrGenericKey>(_resMaterialResourceKeys);

        /// <summary>
        /// Initializes a new instance of the <see cref="Mesh"/> class.
        /// </summary>
        /// <param name="geometryResourceKey">The geometry resource.</param>
        /// <param name="materialResourceKeys">The material resources to apply on this Mesh.</param>
        public Mesh(NamedOrGenericKey geometryResourceKey, params NamedOrGenericKey[] materialResourceKeys)
        {
            _localResGeometry = new IndexBasedDynamicCollection<GeometryResource>();
            _localResMaterials = new IndexBasedDynamicCollection<MaterialResource[]>();
            _localChunks = new IndexBasedDynamicCollection<RenderingChunk[]>();

            _resGeometryKey = geometryResourceKey;
            _resMaterialResourceKeys = materialResourceKeys;

            _passRelevantValuesChanged = true;
        }

        /// <summary>
        /// Tries to get the count of rendering chunks for this mesh on the given device.
        /// Be careful: Chunks are generated on first render. It this have not happened yet, then this method returns 0.
        /// </summary>
        /// <param name="device">The device for which to get the value.</param>
        public int TryGetRenderingChunkCount(EngineDevice device)
        {
            device.EnsureNotNull(nameof(device));

            var renderingChunks = _localChunks[device.DeviceIndex];
            if (renderingChunks == null) { return 0; }

            return renderingChunks.Length;
        }

        /// <summary>
        /// Tries to get the <see cref="GeometryResource"/> object for the given device.
        /// Be careful: Resource objects are created on first render. If this have not happened yet, then this method returns null.
        /// </summary>
        /// <param name="device">The device for which to get the <see cref="GeometryResource"/></param>
        public GeometryResource TryGetGeometryResource(EngineDevice device)
        {
            device.EnsureNotNull(nameof(device));

            return _localResGeometry[device.DeviceIndex];
        }

        /// <summary>
        /// Tries to get the <see cref="MaterialResource"/> objects for the given device.
        /// Be careful: Resource objects are created on first render. If this have not happened yet, then this method returns null.
        /// </summary>
        /// <param name="device">The device for which to get the <see cref="GeometryResource"/></param>
        public MaterialResource[] TryGetMaterialResources(EngineDevice device)
        {
            device.EnsureNotNull(nameof(device));

            var materialResources = _localResMaterials[device.DeviceIndex];
            if (materialResources == null) { return null; }
            if (materialResources.Length == 0) { return null; }

            var result = new MaterialResource[materialResources.Length];
            materialResources.CopyTo(result, 0);
            return result;
        }

        /// <summary>
        /// Tries to get the bounding box for the given render-loop.
        /// Returns BoundingBox.Empty if it is not available.
        /// </summary>
        /// <param name="viewInfo">The ViewInformation for which to get the BoundingBox.</param>
        public override BoundingBox TryGetBoundingBox(ViewInformation viewInfo)
        {
            viewInfo.EnsureNotNull(nameof(viewInfo));

            var geometryResource = this.TryGetGeometryResource(viewInfo.Device);

            if (geometryResource != null &&
                geometryResource.IsLoaded)
            {
                var result = geometryResource.BoundingBox;
                result.Transform(this.Transform);
                return result;
            }

            return default;
        }

        /// <summary>
        /// Tries to get the bounding sphere for the given render-loop.
        /// Returns BoundingSphere.Empty, if it is not available.
        /// </summary>
        /// <param name="viewInfo">The ViewInformation for which to get the BoundingSphere.</param>
        public override BoundingSphere TryGetBoundingSphere(ViewInformation viewInfo)
        {
            viewInfo.EnsureNotNull(nameof(viewInfo));

            var geometryResource = this.TryGetGeometryResource(viewInfo.Device);

            if (geometryResource != null &&
                geometryResource.IsLoaded)
            {
                // Get BoundingBox object
                var boundingBox = geometryResource.BoundingBox;

                // Calculate bounding sphere
                BoundingSphere.FromBox(ref boundingBox, out var result);

                result.Transform(this.Transform);

                return result;
            }

            return default;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"GenericObject (Geometry: {_resGeometryKey.ToString()})";
        }

        /// <summary>
        /// Loads all resources of the object.
        /// </summary>
        /// <param name="device">Current graphics device.</param>
        /// <param name="resourceDictionary">Current resource dictionary.</param>
        public override void LoadResources(EngineDevice device, ResourceDictionary resourceDictionary)
        {
            // Load geometry
            var geoResource = resourceDictionary.GetResourceAndEnsureLoaded<GeometryResource>(_resGeometryKey);
            _localResGeometry.AddObject(geoResource, device.DeviceIndex, false);

            // Load materials
            MaterialResource[] matResources;
            if (_resMaterialResourceKeys.Length > 0)
            {
                matResources = new MaterialResource[_resMaterialResourceKeys.Length];
                for (var loop = 0; loop < matResources.Length; loop++)
                {
                    matResources[loop] =
                        resourceDictionary
                            .GetResourceAndEnsureLoaded<MaterialResource>(_resMaterialResourceKeys[loop]);
                }
                _localResMaterials.AddObject(matResources, device.DeviceIndex, false);
            }
            else
            {
                matResources = new MaterialResource[1];
                matResources[0] = resourceDictionary.GetOrCreateDefaultMaterialResource();
                _localResMaterials.AddObject(matResources, device.DeviceIndex, false);

            }

            // Load chunks
            _localChunks.AddObject(
                geoResource.BuildRenderingChunks(device, matResources),
                device.DeviceIndex,
                false);
        }

        /// <summary>
        /// Are resources loaded for the given device?
        /// </summary>
        /// <param name="device">The device to check for.</param>
        public override bool IsLoaded(EngineDevice device)
        {
            // Handle geometry
            if (!_localResGeometry.HasObjectAt(device.DeviceIndex))
            {
                return false;
            }
            if (!_localResMaterials.HasObjectAt(device.DeviceIndex))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Unloads all resources of the object.
        /// </summary>
        public override void UnloadResources()
        {
            base.UnloadResources();

            _localResGeometry.Clear();
            _localResMaterials.Clear();

            _localChunks.Clear();
        }

        /// <summary>
        /// Updates this object for the given view.
        /// </summary>
        /// <param name="updateState">Current state of the update pass.</param>
        /// <param name="layerViewSubset">The layer view subset which called this update method.</param>
        protected override void UpdateForViewInternal(SceneRelatedUpdateState updateState, ViewRelatedSceneLayerSubset layerViewSubset)
        {
            // Subscribe to render passes
            var countSubscriptions = 0;
            if (_passRelevantValuesChanged || (countSubscriptions = this.CountRenderPassSubscriptions(layerViewSubset)) == 0)
            {
                // Unsubscribe from all passes first
                if (countSubscriptions > 0)
                {
                    this.UnsubsribeFromAllPasses(layerViewSubset);
                }

                // Now subscribe to needed pass
                if (this.Opacity < 1f)
                {
                    this.SubscribeToPass(
                        RenderPassInfo.PASS_TRANSPARENT_RENDER,
                        layerViewSubset, this.OnRender);
                }
                else
                {
                    this.SubscribeToPass(
                        RenderPassInfo.PASS_PLAIN_RENDER,
                        layerViewSubset, this.OnRender);
                }

                // Update local flag
                _passRelevantValuesChanged = false;
            }
        }

        /// <summary>
        /// Called when opacity has changed.
        /// </summary>
        protected override void OnOpacityChanged()
        {
            base.OnOpacityChanged();

            _passRelevantValuesChanged = true;
        }

        /// <summary>
        /// Picks an object in 3D-World.
        /// </summary>
        /// <param name="rayStart">Start of picking ray.</param>
        /// <param name="rayDirection"></param>
        /// <param name="viewInfo">Information about the view that triggered picking.</param>
        /// <param name="pickingOptions">Some additional options for picking calculations.</param>
        /// <returns>
        /// Returns the distance to the object or float.NaN if object is not picked.
        /// </returns>
        internal override float Pick(Vector3 rayStart, Vector3 rayDirection, ViewInformation viewInfo, PickingOptions pickingOptions)
        {
            var geometryResource = _localResGeometry[viewInfo.Device.DeviceIndex];
            if (geometryResource != null &&
                geometryResource.IsLoaded)
            {
                var boundingBox = geometryResource.BoundingBox;
                if (!boundingBox.IsEmpty())
                {
                    // Transform picking ray to local space
                    var pickingRay = new Ray(rayStart, rayDirection);
                    var localTransform = this.Transform;
                    Matrix4x4.Invert(localTransform, out var temp);
                    pickingRay.Transform(temp);

                    // Check for intersection on the bounding box
                    if (pickingRay.Intersects(ref boundingBox, out float distance))
                    {
                        if (pickingOptions.OnlyCheckBoundingBoxes)
                        {
                            return distance;
                        }

                        // Perform picking on polygon level
                        if (geometryResource.Intersects(pickingRay, pickingOptions, out distance))
                        {
                            return distance;
                        }
                    }
                }
            }

            return float.NaN;
        }

        /// <summary>
        /// Is this object visible currently?
        /// </summary>
        /// <param name="viewInfo">Information about the view that triggered bounding volume testing.</param>
        /// <param name="boundingFrustum">The bounding frustum to check.</param>
        /// <returns></returns>
        internal override bool IsInBoundingFrustum(ViewInformation viewInfo, ref BoundingFrustum boundingFrustum)
        {
            var boundingSphere = this.TryGetBoundingSphere(viewInfo);

            if (boundingSphere != default)
            {
                return boundingFrustum.Contains(ref boundingSphere) != ContainmentType.Disjoint;
            }

            return true;
        }

        /// <summary>
        /// Renders the object.
        /// </summary>
        /// <param name="renderState">Current render state.</param>
        private void OnRender(RenderState renderState)
        {
            this.UpdateAndApplyRenderParameters(renderState);

            renderState.RenderChunks(
                _localChunks[renderState.DeviceIndex]);
        }
    }
}