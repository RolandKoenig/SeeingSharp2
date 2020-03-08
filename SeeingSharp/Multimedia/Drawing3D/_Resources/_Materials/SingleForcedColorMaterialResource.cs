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
        private static readonly NamedOrGenericKey RES_KEY_VERTEX_SHADER = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey RES_KEY_PIXEL_SHADER = GraphicsCore.GetNextGenericResourceKey();

        // Instance resource keys
        private static NamedOrGenericKey KEY_CONSTANT_BUFFER = GraphicsCore.GetNextGenericResourceKey();

        // Resource members
        private VertexShaderResource m_vertexShader;
        private PixelShaderResource m_pixelShader;
        private TypeSafeConstantBufferResource<CBPerMaterial> m_cbPerMaterial;

        // Shader parameters
        private float m_fadeIntensity;
        private bool m_cbPerMaterialDataChanged;

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            // Load all required shaders and constant buffers
            m_vertexShader = resources.GetResourceAndEnsureLoaded(
                RES_KEY_VERTEX_SHADER,
                () => GraphicsHelper.Internals.GetVertexShaderResource(device, "Common", "SingleForcedColorVertexShader"));
            m_pixelShader = resources.GetResourceAndEnsureLoaded(
                RES_KEY_PIXEL_SHADER,
                () => GraphicsHelper.Internals.GetPixelShaderResource(device, "Common", "SingleForcedColorPixelShader"));

            m_cbPerMaterial = resources.GetResourceAndEnsureLoaded(
                KEY_CONSTANT_BUFFER,
                () => new TypeSafeConstantBufferResource<CBPerMaterial>());
            m_cbPerMaterialDataChanged = true;
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            m_pixelShader = null;
            m_vertexShader = null;
            m_cbPerMaterial = null;
        }

        /// <summary>
        /// Generates the requested input layout.
        /// </summary>
        /// <param name="device">The device on which to create the input layout.</param>
        /// <param name="inputElements">An array of InputElements describing vertex input structure.</param>
        internal override D3D11.InputLayout GetInputLayout(EngineDevice device, D3D11.InputElement[] inputElements)
        {
            return m_vertexShader.GetInputLayout(device, inputElements);
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
            if (m_cbPerMaterialDataChanged)
            {
                m_cbPerMaterial.SetData(
                    deviceContext,
                    new CBPerMaterial
                    {
                        FadeIntensity = m_fadeIntensity
                    });
                m_cbPerMaterialDataChanged = false;
            }

            // Apply constants and shader resources
            deviceContext.VertexShader.Set(m_vertexShader.VertexShader);
            deviceContext.PixelShader.Set(m_pixelShader.PixelShader);
            deviceContext.PixelShader.SetConstantBuffer(3, m_cbPerMaterial.ConstantBuffer);
            deviceContext.VertexShader.SetConstantBuffer(3, m_cbPerMaterial.ConstantBuffer);
        }

        public override bool IsLoaded =>
            m_vertexShader != null &&
            m_pixelShader != null;

        public float FadeIntensity
        {
            get => m_fadeIntensity;
            set
            {
                if (!EngineMath.EqualsWithTolerance(m_fadeIntensity, value))
                {
                    m_fadeIntensity = value;
                    m_cbPerMaterialDataChanged = true;
                }
            }
        }
    }
}
