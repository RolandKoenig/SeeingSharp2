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
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Objects
{
    public class FullscreenTextureObject : SceneObject, IAnimatableObjectAccentuation, IAnimatableObjectOpacity
    {
        // Configuration
        private NamedOrGenericKey m_resTexture;
        

        // Device dependent resources
        private IndexBasedDynamicCollection<TexturePainterHelper> m_texturePainterHelpers;
        

        /// <summary>
        /// Loads all resources of the object.
        /// </summary>
        public override void LoadResources(EngineDevice device, ResourceDictionary resourceDictionary)
        {
            var newHelper = new TexturePainterHelper(m_resTexture);

            m_texturePainterHelpers.AddObject(
                newHelper,
                device.DeviceIndex);

            newHelper.LoadResources(resourceDictionary);
        }

        /// <summary>
        /// Are resources loaded for the given device?
        /// </summary>
        /// <param name="device"></param>
        public override bool IsLoaded(EngineDevice device)
        {
            return m_texturePainterHelpers.HasObjectAt(device.DeviceIndex);
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
            //Subscribe to render passes
            if (CountRenderPassSubscriptions(layerViewSubset) == 0)
            {
                SubscribeToPass(
                    RenderPassInfo.PASS_PLAIN_RENDER,
                    layerViewSubset, OnRenderPlain);
                SubscribeToPass(
                    RenderPassInfo.PASS_TRANSPARENT_RENDER,
                    layerViewSubset, OnRenderTransparent);
            }
        }

        /// <summary>
        /// Renders the object.
        /// </summary>
        /// <param name="renderState">Current render state.</param>
        private void OnRenderPlain(RenderState renderState)
        {
            if (Opacity >= 1f)
            {
                // Get and configure helper object
                var actHelper = m_texturePainterHelpers[renderState.DeviceIndex];
                actHelper.Scaling = Scaling;
                actHelper.Opacity = Opacity;
                actHelper.AccentuationFactor = AccentuationFactor;
                actHelper.AlphaBlendMode = AlphaBlendMode;

                // Render the object
                actHelper.RenderPlain(renderState);
            }
        }

        /// <summary>
        /// Renders the object.
        /// </summary>
        /// <param name="renderState">Current render state.</param>
        private void OnRenderTransparent(RenderState renderState)
        {
            if (Opacity < 1f)
            {
                // Get and configure helper object
                var actHelper = m_texturePainterHelpers[renderState.DeviceIndex];
                actHelper.Scaling = Scaling;
                actHelper.Opacity = Opacity;
                actHelper.AccentuationFactor = AccentuationFactor;
                actHelper.AlphaBlendMode = AlphaBlendMode;

                // Render the object
                actHelper.RenderPlain(renderState);
            }
        }

        /// <summary>
        /// Unloads all resources of the object.
        /// </summary>
        public override void UnloadResources()
        {
            base.UnloadResources();

            foreach(var actHelper in m_texturePainterHelpers)
            {
                actHelper.UnloadResources();
            }

            m_texturePainterHelpers.Clear();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FullscreenTextureObject"/> class.
        /// </summary>
        /// <param name="texture">The texture.</param>
        public FullscreenTextureObject(NamedOrGenericKey texture)
        {
            m_resTexture = texture;
            Scaling = 1f;
            Opacity = 1f;
            AccentuationFactor = 0f;
            AlphaBlendMode = TexturePainterAlphaBlendMode.AlphaBlend;

            m_texturePainterHelpers = new IndexBasedDynamicCollection<TexturePainterHelper>();
        }

        public float AccentuationFactor { get; set; }

        public float Opacity { get; set; }

        /// <summary>
        /// Gets or sets the scaling factor.
        /// </summary>
        public float Scaling { get; set; }

        /// <summary>
        /// Gets or sets the alpha blend mode.
        /// </summary>
        public TexturePainterAlphaBlendMode AlphaBlendMode { get; set; }
    }
}