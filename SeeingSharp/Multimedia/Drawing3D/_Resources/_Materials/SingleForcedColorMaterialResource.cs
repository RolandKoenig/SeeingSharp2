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
    public class SingleForcedColorMaterialResource : MaterialResource
    {
        // Static resource keys
        private static readonly NamedOrGenericKey s_resKeyVertexShader = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey s_resKeyPixelShader = GraphicsCore.GetNextGenericResourceKey();

        // Instance resource keys
        private static NamedOrGenericKey s_keyConstantBuffer = GraphicsCore.GetNextGenericResourceKey();

        // Resource members
        private VertexShaderResource _vertexShader;
        private PixelShaderResource _pixelShader;
        private TypeSafeConstantBufferResource<CBPerMaterial> _cbPerMaterial;

        // Shader parameters
        private float _fadeIntensity;
        private bool _cbPerMaterialDataChanged;

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            // Load all required shaders and constant buffers
            _vertexShader = resources.GetResourceAndEnsureLoaded(
                s_resKeyVertexShader,
                () => GraphicsHelper.Internals.GetVertexShaderResource(device, "Common", "SingleForcedColorVertexShader"));
            _pixelShader = resources.GetResourceAndEnsureLoaded(
                s_resKeyPixelShader,
                () => GraphicsHelper.Internals.GetPixelShaderResource(device, "Common", "SingleForcedColorPixelShader"));

            _cbPerMaterial = resources.GetResourceAndEnsureLoaded(
                s_keyConstantBuffer,
                () => new TypeSafeConstantBufferResource<CBPerMaterial>());
            _cbPerMaterialDataChanged = true;
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _pixelShader = null;
            _vertexShader = null;
            _cbPerMaterial = null;
        }

        /// <summary>
        /// Generates the requested input layout.
        /// </summary>
        /// <param name="device">The device on which to create the input layout.</param>
        /// <param name="inputElements">An array of InputElements describing vertex input structure.</param>
        internal override D3D11.InputLayout GetInputLayout(EngineDevice device, D3D11.InputElement[] inputElements)
        {
            return _vertexShader.GetInputLayout(device, inputElements);
        }

        /// <summary>
        /// Applies the material to the given render state.
        /// </summary>
        /// <param name="renderState">Current render state</param>
        /// <param name="previousMaterial">The previously applied material.</param>
        internal override void Apply(RenderState renderState, MaterialResource previousMaterial)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;

            // Apply local shader configuration
            if (_cbPerMaterialDataChanged)
            {
                _cbPerMaterial.SetData(
                    deviceContext,
                    new CBPerMaterial
                    {
                        FadeIntensity = _fadeIntensity
                    });
                _cbPerMaterialDataChanged = false;
            }

            // Apply constants and shader resources
            deviceContext.VertexShader.Set(_vertexShader.VertexShader);
            deviceContext.PixelShader.Set(_pixelShader.PixelShader);
            deviceContext.PixelShader.SetConstantBuffer(3, _cbPerMaterial.ConstantBuffer);
            deviceContext.VertexShader.SetConstantBuffer(3, _cbPerMaterial.ConstantBuffer);
        }

        public override bool IsLoaded =>
            _vertexShader != null &&
            _pixelShader != null;

        public float FadeIntensity
        {
            get => _fadeIntensity;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_fadeIntensity, value))
                {
                    _fadeIntensity = value;
                    _cbPerMaterialDataChanged = true;
                }
            }
        }
    }
}
