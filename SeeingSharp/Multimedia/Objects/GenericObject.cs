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
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;
using SharpDX;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Objects
{
    public class GenericObject : SceneSpacialObject
    {
        // Resources
        private IndexBasedDynamicCollection<GeometryResource> m_localResources;
        private float m_opacity;
        private bool m_passRelevantValuesChanged;

        // Configuration members
        private NamedOrGenericKey m_resGeometryKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericObject"/> class.
        /// </summary>
        /// <param name="geometryResource">The geometry resource.</param>
        public GenericObject(NamedOrGenericKey geometryResource)
        {
            m_localResources = new IndexBasedDynamicCollection<GeometryResource>();

            m_resGeometryKey = geometryResource;

            //m_opacity = 1f;
            m_passRelevantValuesChanged = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericObject" /> class.
        /// </summary>
        /// <param name="geometryResource">The geometry resource.</param>
        /// <param name="position">The initial position.</param>
        public GenericObject(NamedOrGenericKey geometryResource, Vector3 position)
            : this(geometryResource)
        {
            Position = position;
        }

        /// <summary>
        /// Tries to get the bounding box for the given render-loop.
        /// Returns BoundingBox.Empty if it is not available.
        /// </summary>
        /// <param name="viewInfo">The ViewInformation for which to get the BoundingBox.</param>
        public override BoundingBox TryGetBoundingBox(ViewInformation viewInfo)
        {
            var geometryResource = m_localResources[viewInfo.Device.DeviceIndex];

            if (geometryResource != null &&
                geometryResource.IsLoaded)
            {
                var result = geometryResource.BoundingBox;
                result.Transform(Transform);
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
            var geometryResource = m_localResources[viewInfo.Device.DeviceIndex];

            if (geometryResource != null &&
                geometryResource.IsLoaded)
            {
                // Get BoundingBox object
                var boundingBox = geometryResource.BoundingBox;

                // Calculate bounding sphare
                BoundingSphere result;
                BoundingSphere.FromBox(ref boundingBox, out result);

                result.Transform(Transform);

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
            return "GenericObject (Geometry: " + m_resGeometryKey + ")";
        }

        /// <summary>
        /// Changes the geometry to the given one.
        /// </summary>
        /// <param name="newGeometry">The new geometry to set.</param>
        public void ChangeGeometry(NamedOrGenericKey newGeometry)
        {
            m_resGeometryKey = newGeometry;
        }

        /// <summary>
        /// Loads all resources of the object.
        /// </summary>
        /// <param name="device">Current graphics device.</param>
        /// <param name="resourceDictionary">Current resource dicionary.</param>
        public override void LoadResources(EngineDevice device, ResourceDictionary resourceDictionary)
        {
            m_localResources.AddObject(
                resourceDictionary.GetResourceAndEnsureLoaded<GeometryResource>(m_resGeometryKey),
                device.DeviceIndex,
                false);
        }

        /// <summary>
        /// Are resources loaded for the given device?
        /// </summary>
        /// <param name="device">The device to check for.</param>
        public override bool IsLoaded(EngineDevice device)
        {
            if (!m_localResources.HasObjectAt(device.DeviceIndex))
            {
                return false;
            }

            var geoResource = m_localResources[device.DeviceIndex];

            if (geoResource.Key != m_resGeometryKey)
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

            m_localResources.Clear();
        }

        /// <summary>
        /// This methode stores all data related to this object into the given <see cref="ExportModelContainer" />.
        /// </summary>
        /// <param name="modelContainer">The target container.</param>
        /// <param name="exportOptions">Options for export.</param>
        protected override void PrepareForExportInternal(
            ExportModelContainer modelContainer, ExportOptions exportOptions)
        {
            modelContainer.EnsureNotNull(nameof(modelContainer));
            exportOptions.EnsureNotNull(nameof(exportOptions));

            // Get the device and ensure that we've an instance
            var exportDevice = exportOptions.ExportDevice;
            exportOptions.EnsureNotNull(nameof(exportDevice));

            var geometryResource = m_localResources[exportDevice.DeviceIndex];

            if (geometryResource != null)
            {
                // Ensure that we have geometry infos for the exporter
                if(!modelContainer.ContainsExportGeometry(geometryResource.Key))
                {
                    modelContainer.AddExportGeometry(geometryResource.PrepareForExport());

                    foreach(var actMaterial in geometryResource.GetReferencedMaterials())
                    {
                        if(!modelContainer.ContainsExportMaterial(actMaterial.Key))
                        {

                        }
                    }
                }

                //base.UpdateAndApplyRenderParameters(renderState);
                //geometryResource.Render(renderState);
            }
        }

        /// <summary>
        /// Updates this object for the given view.
        /// </summary>
        /// <param name="updateState">Current state of the update pass.</param>
        /// <param name="layerViewSubset">The layer view subset wich called this update method.</param>
        protected override void UpdateForViewInternal(SceneRelatedUpdateState updateState, ViewRelatedSceneLayerSubset layerViewSubset)
        {
            //Subscribe to render passes
            if (m_passRelevantValuesChanged ||
                CountRenderPassSubscriptions(layerViewSubset) == 0)
            {
                //Unsubscribe from all passes first
                UnsubsribeFromAllPasses(layerViewSubset);

                //Now subscribe to needed pass
                if (Opacity < 1f)
                {
                    SubscribeToPass(
                        RenderPassInfo.PASS_TRANSPARENT_RENDER,
                        layerViewSubset, OnRenderTransparent);
                }
                else
                {
                    SubscribeToPass(
                        RenderPassInfo.PASS_PLAIN_RENDER,
                        layerViewSubset, OnRenderPlain);
                }

                //Update local flag
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
            var geometryResource = m_localResources[viewInfo.Device.DeviceIndex];

            if (geometryResource != null &&
                geometryResource.IsLoaded)
            {
                var boundingBox = geometryResource.BoundingBox;

                if (!boundingBox.IsEmpty())
                {
                    // Transform picking ray to local space
                    var pickingRay = new Ray(rayStart, rayDirection);
                    Matrix temp;
                    var localTransform = Transform;
                    Matrix.Invert(ref localTransform, out temp);
                    pickingRay.Transform(temp);

                    // Check for intersection on the bounding box
                    var distance = 0f;

                    if (pickingRay.Intersects(ref boundingBox, out distance))
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
            var boundingSphere = TryGetBoundingSphere(viewInfo);

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
        private void OnRenderPlain(RenderState renderState)
        {
            var geometryResource = m_localResources[renderState.DeviceIndex];

            if (geometryResource != null)
            {
                UpdateAndApplyRenderParameters(renderState);
                geometryResource.Render(renderState);
            }
        }

        /// <summary>
        /// Renders the object.
        /// </summary>
        /// <param name="renderState">Current render state.</param>
        private void OnRenderTransparent(RenderState renderState)
        {
            var geometryResource = m_localResources[renderState.DeviceIndex];

            if (geometryResource != null)
            {
                UpdateAndApplyRenderParameters(renderState);
                geometryResource.Render(renderState);
            }
        }

        /// <summary>
        /// Gets the key of the geometry resources used by this object.
        /// </summary>
        public NamedOrGenericKey GeometryResourceKey => m_resGeometryKey;

        public override bool IsExportable => true;
    }
}