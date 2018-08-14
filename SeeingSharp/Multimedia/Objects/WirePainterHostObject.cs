#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;

//Some namespace mappings
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Objects
{
    public class WirePainterHostObject : SceneObject
    {
        #region Configuration
        private Action<WirePainter> m_paintAction;
        #endregion

        #region Direct3D resources
        private IndexBasedDynamicCollection<LineRenderResources> m_localResources;
        #endregion

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
        /// <param name="resourceDictionary">Current resource dicionary.</param>
        public override void LoadResources(EngineDevice device, ResourceDictionary resourceDictionary)
        {
            m_localResources.AddObject(
                resourceDictionary.GetResourceAndEnsureLoaded<LineRenderResources>(
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
        /// <param name="layerViewSubset">The layer view subset wich called this update method.</param>
        protected override void UpdateForViewInternal(SceneRelatedUpdateState updateState, ViewRelatedSceneLayerSubset layerViewSubset)
        {
            if (base.CountRenderPassSubscriptions(layerViewSubset) == 0)
            {
                base.SubscribeToPass(RenderPassInfo.PASS_LINE_RENDER, layerViewSubset, RenderLines);
            }
        }

        /// <summary>
        /// Main render method for the wire object.
        /// </summary>
        /// <param name="renderState">Current render state.</param>
        private void RenderLines(RenderState renderState)
        {
            LineRenderResources resourceData = m_localResources[renderState.DeviceIndex];

            if (m_paintAction != null)
            {
                WirePainter wirePainter = new WirePainter(renderState, resourceData);
                try
                {
                    m_paintAction(wirePainter);
                }
                finally
                {
                    wirePainter.SetInvalid();
                }
            }
        }

        public Action<WirePainter> PaintAction
        {
            get { return m_paintAction; }
            set { m_paintAction = value; }
        }
    }
}
