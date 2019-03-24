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
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class ObjectRenderParameters : Resource
    {
        // Resource keys
        private readonly NamedOrGenericKey KEY_CONSTANT_BUFFER = GraphicsCore.GetNextGenericResourceKey();

        // Resources
        private TypeSafeConstantBufferResource<CBPerObject> m_cbPerObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectRenderParameters" /> class.
        /// </summary>
        internal ObjectRenderParameters()
        {
            NeedsRefresh = true;
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            m_cbPerObject = resources.GetResourceAndEnsureLoaded(
                KEY_CONSTANT_BUFFER,
                () => new TypeSafeConstantBufferResource<CBPerObject>());
            this.NeedsRefresh = true;
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            m_cbPerObject = null;
        }

        /// <summary>
        /// Triggers unloading of this resource.
        /// </summary>
        internal void MarkForUnloading()
        {
            this.Dictionary?.MarkForUnloading(this);
        }

        /// <summary>
        /// Updates all parameters.
        /// </summary>
        /// <param name="renderState">The render state on which to apply.</param>
        /// <param name="cbPerObject">Constant buffer data.</param>
        internal void UpdateValues(RenderState renderState, CBPerObject cbPerObject)
        {
            m_cbPerObject.SetData(renderState.Device.DeviceImmediateContextD3D11, cbPerObject);
        }

        /// <summary>
        /// Applies all parameters.
        /// </summary>
        /// <param name="renderState">The render state on which to apply.</param>
        internal void Apply(RenderState renderState)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;

            // Apply constant buffer on shaders
            deviceContext.VertexShader.SetConstantBuffer(2, m_cbPerObject.ConstantBuffer);
            deviceContext.PixelShader.SetConstantBuffer(2, m_cbPerObject.ConstantBuffer);
        }

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => m_cbPerObject != null;

        /// <summary>
        /// Does this object needs refreshing?
        /// </summary>
        internal bool NeedsRefresh;
    }
}
