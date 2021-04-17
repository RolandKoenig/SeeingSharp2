/*
    SeeingSharp and all applications distributed together with it. 
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

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class ViewRenderParameters : Resource
    {
        // Resource keys
        private readonly NamedOrGenericKey _keyConstantBuffer = GraphicsCore.GetNextGenericResourceKey();

        // Resources
        private TypeSafeConstantBufferResource<CBPerView> _cbPerView;
        private PostprocessEffectResource _postprocessEffect;

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => _cbPerView != null;

        /// <summary>
        /// Gets or sets the key of the postprocess effect.
        /// </summary>
        internal NamedOrGenericKey PostprocessEffectKey { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewRenderParameters" /> class.
        /// </summary>
        internal ViewRenderParameters()
        {

        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _cbPerView = resources.GetResourceAndEnsureLoaded(
                _keyConstantBuffer,
                () => new TypeSafeConstantBufferResource<CBPerView>());
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _cbPerView = null;
            _postprocessEffect = null;
        }

        /// <summary>
        /// Gets the postprocess effect with the given key.
        /// </summary>
        /// <param name="namedOrGenericKey">The key of the effect.</param>
        /// <param name="resourceDictionary">The resource dictionary where to load the effect.</param>
        internal PostprocessEffectResource GetPostprocessEffect(NamedOrGenericKey namedOrGenericKey, ResourceDictionary resourceDictionary)
        {
            this.PostprocessEffectKey = namedOrGenericKey;

            // Handle empty key
            if (namedOrGenericKey.IsEmpty)
            {
                _postprocessEffect = null;
                return null;
            }

            // Check for current effect object
            if (_postprocessEffect != null)
            {
                // Good case, return current one
                if (_postprocessEffect.Key == namedOrGenericKey) { return _postprocessEffect; }

                // Bad case, effect has changed
                _postprocessEffect = null;
            }

            _postprocessEffect = resourceDictionary.GetResourceAndEnsureLoaded<PostprocessEffectResource>(namedOrGenericKey);
            this.PostprocessEffectKey = namedOrGenericKey;
            return _postprocessEffect;
        }

        /// <summary>
        /// Updates all parameters.
        /// </summary>
        internal void UpdateValues(RenderState renderState, CBPerView cbPerView)
        {
            _cbPerView.SetData(renderState.Device.DeviceImmediateContextD3D11, cbPerView);
        }

        /// <summary>
        /// Applies all parameters.
        /// </summary>
        /// <param name="renderState">The render state on which to apply.</param>
        internal void Apply(RenderState renderState)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;

            // Apply constant buffer on shaders
            deviceContext.VertexShader.SetConstantBuffer(1, _cbPerView.ConstantBuffer);
            deviceContext.PixelShader.SetConstantBuffer(1, _cbPerView.ConstantBuffer);
        }
    }
}