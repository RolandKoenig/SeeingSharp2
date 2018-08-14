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
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Util;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Objects;

// Some namespace mappings
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class ViewRenderParameters : Resource
    {
        #region Resource keys
        internal NamedOrGenericKey KEY_CONSTANT_BUFFER = GraphicsCore.GetNextGenericResourceKey();
        #endregion

        #region Configuration
        private NamedOrGenericKey m_postprocessEffectKey;
        #endregion

        #region Resources
        private TypeSafeConstantBufferResource<CBPerView> m_cbPerView;
        private PostprocessEffectResource m_postprocessEffect;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewRenderParameters" /> class.
        /// </summary>
        internal ViewRenderParameters()
        {

        }

        /// <summary>
        /// Gets the postrocess effect with the given key.
        /// </summary>
        /// <param name="namedOrGenericKey">The key of the effect.</param>
        /// <param name="resourceDictionary">The resource dictionary where to load the effect.</param>
        internal PostprocessEffectResource GetPostprocessEffect(NamedOrGenericKey namedOrGenericKey, ResourceDictionary resourceDictionary)
        {
            m_postprocessEffectKey = namedOrGenericKey;

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
            m_postprocessEffectKey = namedOrGenericKey;
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
            D3D11.DeviceContext deviceContext = renderState.Device.DeviceImmediateContextD3D11;

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
        /// Gets or sets the key of the postprocess effect.
        /// </summary>
        internal NamedOrGenericKey PostprocessEffectKey
        {
            get { return m_postprocessEffectKey; }
            set { m_postprocessEffectKey = value; }
        }

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded
        {
            get
            {
                return m_cbPerView != null;
            }
        }
    }
}
