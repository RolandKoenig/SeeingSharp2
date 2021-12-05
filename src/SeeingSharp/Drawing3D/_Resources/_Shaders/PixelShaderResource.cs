using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D
{
    public class PixelShaderResource : ShaderResource
    {
        // Resources for Direct3D 11 rendering
        private D3D11.ID3D11PixelShader _pixelShader;

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => _pixelShader != null;

        /// <summary>
        /// Gets the loaded PixelShader object.
        /// </summary>
        internal D3D11.ID3D11PixelShader PixelShader => _pixelShader;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexShaderResource" /> class.
        /// </summary>
        /// <param name="shaderProfile">Shader profile used for compiling.</param>
        /// <param name="resourceLink">The resourceLink.</param>
        public PixelShaderResource(string shaderProfile, ResourceLink resourceLink)
            : base(shaderProfile, resourceLink, ShaderResourceKind.HlsFile)
        {
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected internal override void LoadShader(EngineDevice device, byte[] shaderBytecode)
        {
            if (_pixelShader == null)
            {
                _pixelShader = device.DeviceD3D11_1.CreatePixelShader(shaderBytecode);
            }
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected internal override void UnloadShader()
        {
            _pixelShader = SeeingSharpUtil.DisposeObject(_pixelShader);
        }
    }
}