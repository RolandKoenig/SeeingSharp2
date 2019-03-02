#region License information
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
#endregion
#region using

// Some namespace mappings
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

#endregion

namespace SeeingSharp.Multimedia.Drawing3D
{
    #region using
    #endregion

    public class ViewRenderParameters : Resource
    {
        #region Resource keys
        internal NamedOrGenericKey KEY_CONSTANT_BUFFER = GraphicsCore.GetNextGenericResourceKey();
        #endregion

        /// <summary>
        /// Gets the postrocess effect with the given key.
        /// </summary>
        /// <param name="namedOrGenericKey">The key of the effect.</param>
        /// <param name="resourceDictionary">The resource dictionary where to load the effect.</param>
        internal PostprocessEffectResource GetPostprocessEffect(NamedOrGenericKey namedOrGenericKey, ResourceDictionary resourceDictionary)
        {
            PostprocessEffectKey = namedOrGenericKey;

            // Handle empty key
            if (namedOrGenericKey.IsEmpty)
            {
                m_postprocessEffect = null;
                return null;
            }

            // Check for current effect object
            if (m_postprocessEffect != null)
            {
                // Good case, return current one
                if (m_postprocessEffect.Key == namedOrGenericKey) { return m_postprocessEffect; }

                // Bad case, effect has changed
                m_postprocessEffect = null;
            }

            m_postprocessEffect = resourceDictionary.GetResourceAndEnsureLoaded<PostprocessEffectResource>(namedOrGenericKey);
            PostprocessEffectKey = namedOrGenericKey;
            return m_postprocessEffect;
        }

        /// <summary>
        /// Updates all parameters.
        /// </summary>
        internal void UpdateValues(RenderState renderState, CBPerView cbPerView)
        {
            m_cbPerView.SetData(renderState.Device.DeviceImmediateContextD3D11, cbPerView);
        }

        /// <summary>
        /// Applies all parameters.
        /// </summary>
        /// <param name="renderState">The render state on which to apply.</param>
        internal void Apply(RenderState renderState)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;

            // Apply constant buffer on shaders
            deviceContext.VertexShader.SetConstantBuffer(1, m_cbPerView.ConstantBuffer);
            deviceContext.PixelShader.SetConstantBuffer(1, m_cbPerView.ConstantBuffer);
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            m_cbPerView = resources.GetResourceAndEnsureLoaded(
                KEY_CONSTANT_BUFFER,
                () => new TypeSafeConstantBufferResource<CBPerView>());
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            m_cbPerView = null;
            m_postprocessEffect = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewRenderParameters" /> class.
        /// </summary>
        internal ViewRenderParameters()
        {

        }

        /// <summary>
        /// Gets or sets the key of the postprocess effect.
        /// </summary>
        internal NamedOrGenericKey PostprocessEffectKey { get; set; }

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => m_cbPerView != null;

        #region Configuration
        #endregion

        #region Resources
        private TypeSafeConstantBufferResource<CBPerView> m_cbPerView;
        private PostprocessEffectResource m_postprocessEffect;
        #endregion
    }
}