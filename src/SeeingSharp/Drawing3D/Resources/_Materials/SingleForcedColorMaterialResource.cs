using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D.Resources
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
        internal override D3D11.ID3D11InputLayout GetInputLayout(EngineDevice device, D3D11.InputElementDescription[] inputElements)
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
            deviceContext.VSSetShader(_vertexShader.VertexShader);
            deviceContext.PSSetShader(_pixelShader.PixelShader);
            deviceContext.PSSetConstantBuffer(3, _cbPerMaterial.ConstantBuffer);
            deviceContext.VSSetConstantBuffer(3, _cbPerMaterial.ConstantBuffer);
        }
    }
}
