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

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class Mesh : SceneSpacialObject
    {
        // Resources
        private IndexBasedDynamicCollection<GeometryResource> m_localResGeometry;
        private IndexBasedDynamicCollection<MaterialResource[]> m_localResMaterials;
        private IndexBasedDynamicCollection<RenderingChunk[]> m_localChunks;
        private bool m_passRelevantValuesChanged;

        // Configuration members
        private NamedOrGenericKey m_resGeometryKey;
        private NamedOrGenericKey[] m_resMaterialResourceKeys;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mesh"/> class.
        /// </summary>
        /// <param name="geometryResourceKey">The geometry resource.</param>
        /// <param name="materialResourceKeys">The material resources to apply on this Mesh.</param>
        public Mesh(NamedOrGenericKey geometryResourceKey, params NamedOrGenericKey[] materialResourceKeys)
        {
            m_localResGeometry = new IndexBasedDynamicCollection<GeometryResource>();
            m_localResMaterials = new IndexBasedDynamicCollection<MaterialResource[]>();
            m_localChunks = new IndexBasedDynamicCollection<RenderingChunk[]>();

            m_resGeometryKey = geometryResourceKey;
            m_resMaterialResourceKeys = materialResourceKeys;

            m_passRelevantValuesChanged = true;
        }

        /// <summary>
        /// Tries to get the bounding box for the given render-loop.
        /// Returns BoundingBox.Empty if it is not available.
        /// </summary>
        /// <param name="viewInfo">The ViewInformation for which to get the BoundingBox.</param>
        public override BoundingBox TryGetBoundingBox(ViewInformation viewInfo)
        {
            var geometryResource = m_localResGeometry[viewInfo.Device.DeviceIndex];

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
            var geometryResource = m_localResGeometry[viewInfo.Device.DeviceIndex];

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
            return $"GenericObject (Geometry: {m_resGeometryKey.ToString()})";
        }

        /// <summary>
        /// Loads all resources of the object.
        /// </summary>
        /// <param name="device">Current graphics device.</param>
        /// <param name="resourceDictionary">Current resource dictionary.</param>
        public override void LoadResources(EngineDevice device, ResourceDictionary resourceDictionary)
        {
            // Load geometry
            var geoResource = resourceDictionary.GetResourceAndEnsureLoaded<GeometryResource>(m_resGeometryKey);
            m_localResGeometry.AddObject(geoResource, device.DeviceIndex, false);

            // Load materials
            MaterialResource[] matResources;
            if (m_resMaterialResourceKeys.Length > 0)
            {
                matResources = new MaterialResource[m_resMaterialResourceKeys.Length];
                for (var loop = 0; loop < matResources.Length; loop++)
                {
                    matResources[loop] =
                        resourceDictionary
                            .GetResourceAndEnsureLoaded<MaterialResource>(m_resMaterialResourceKeys[loop]);
                }
                m_localResMaterials.AddObject(matResources, device.DeviceIndex, false);
            }
            else
            {
                matResources = new MaterialResource[1];
                matResources[0] = resourceDictionary.GetOrCreateDefaultMaterialResource();
                m_localResMaterials.AddObject(matResources, device.DeviceIndex, false);

            }

            // Load chunks
            m_localChunks.AddObject(
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
            if (!m_localResGeometry.HasObjectAt(device.DeviceIndex))
            {
                return false;
            }
            if (!m_localResMaterials.HasObjectAt(device.DeviceIndex))
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

            m_localResGeometry.Clear();
            m_localResMaterials.Clear();

            m_localChunks.Clear();
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
            if (m_passRelevantValuesChanged || (countSubscriptions = this.CountRenderPassSubscriptions(layerViewSubset)) == 0)
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
                m_passRelevantValuesChanged = false;
            }
        }

        /// <summary>
        /// Called when opacity has changed.
        /// </summary>
        protected override void OnOpacityChanged()
        {
            base.OnOpacityChanged();

            m_passRelevantValuesChanged = true;
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
            var geometryResource = m_localResGeometry[viewInfo.Device.DeviceIndex];
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
                m_localChunks[renderState.DeviceIndex]);
        }

        public override bool IsExportable => false;
    }
}