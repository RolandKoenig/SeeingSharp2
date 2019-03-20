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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Objects
{
    public class WirePainterHostObject : SceneObject
    {
        // Direct3D resources
        private IndexBasedDynamicCollection<LineRenderResources> m_localResources;

        /// <summary>
        /// Initializes a new instance of the <see cref="WireObject" /> class.
        /// </summary>
        public WirePainterHostObject()
        {
            m_localResources = new IndexBasedDynamicCollection<LineRenderResources>();
        }

        /// <summary>
        /// Loads all resources of the object.
        /// </summary>
        /// <param name="device">Current graphics device.</param>
        /// <param name="resourceDictionary">Current resource dictionary.</param>
        public override void LoadResources(EngineDevice device, ResourceDictionary resourceDictionary)
        {
            m_localResources.AddObject(
                resourceDictionary.GetResourceAndEnsureLoaded(
                    LineRenderResources.RESOURCE_KEY,
                    () => new LineRenderResources()),
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
        /// Unloads all resources of the object.
        /// </summary>
        public override void UnloadResources()
        {
            base.UnloadResources();

            m_localResources.Clear();
        }

        /// <summary>
        /// Updates the object.
        /// </summary>
        /// <param name="updateState">Current update state.</param>
        protected override void UpdateInternal(SceneRelatedUpdateState updateState)
        {

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
        /// Main render method for the wire object.
        /// </summary>
        /// <param name="renderState">Current render state.</param>
        private void RenderLines(RenderState renderState)
        {
            var resourceData = m_localResources[renderState.DeviceIndex];

            if (this.PaintAction != null)
            {
                var wirePainter = new WirePainter(renderState, resourceData);

                try
                {
                    this.PaintAction(wirePainter);
                }
                finally
                {
                    wirePainter.SetInvalid();
                }
            }
        }

        public Action<WirePainter> PaintAction { get; set; }
    }
}