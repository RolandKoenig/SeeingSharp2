using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Drawing3D
{
    public class LineRenderResources : Resource
    {
        public static readonly NamedOrGenericKey RESOURCE_KEY = GraphicsCore.GetNextGenericResourceKey();

        // Private constants
        private static readonly NamedOrGenericKey s_keyVertexShader = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey s_keyPixelShader = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey s_keyConstantBuffer = GraphicsCore.GetNextGenericResourceKey();

        // Resources
        private VertexShaderResource _vertexShader;
        private PixelShaderResource _pixelShader;
        private D3D11.InputLayout _inputLayout;

        /// <summary>
        /// Is the resource loaded correctly?
        /// </summary>
        public override bool IsLoaded => _pixelShader != null;

        /// <summary>
        /// Gets the vertex shader resource.
        /// </summary>
        public VertexShaderResource VertexShader => _vertexShader;

        /// <summary>
        /// Gets the pixel shader resource.
        /// </summary>
        public PixelShaderResource PixelShader => _pixelShader;

        /// <summary>
        /// Gets the input layout for the vertex shader.
        /// </summary>
        internal D3D11.InputLayout InputLayout => _inputLayout;

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _vertexShader = resources.GetResourceAndEnsureLoaded(
                s_keyVertexShader,
                () => GraphicsHelper.Internals.GetVertexShaderResource(device, "LineRendering", "LineVertexShader"));
            _pixelShader = resources.GetResourceAndEnsureLoaded(
                s_keyPixelShader,
                () => GraphicsHelper.Internals.GetPixelShaderResource(device, "LineRendering", "LinePixelShader"));

            _inputLayout = new D3D11.InputLayout(
                device.DeviceD3D11_1,
                _vertexShader.ShaderBytecode,
                LineVertex.InputElements);
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            SeeingSharpUtil.SafeDispose(ref _inputLayout);

            _vertexShader = null;
            _pixelShader = null;
        }
    }
}
