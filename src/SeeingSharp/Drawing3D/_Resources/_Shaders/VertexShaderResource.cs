using System.Collections.Generic;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D
{
    public class VertexShaderResource : ShaderResource
    {
        // Resources for Direct3D 11 rendering
        private D3D11.ID3D11VertexShader _vertexShader;
        private Dictionary<D3D11.InputElementDescription[], D3D11.ID3D11InputLayout> _inputLayouts;

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => _vertexShader != null;

        /// <summary>
        /// Gets the loaded VertexShader object.
        /// </summary>
        internal D3D11.ID3D11VertexShader VertexShader => _vertexShader;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexShaderResource" /> class.
        /// </summary>
        /// <param name="shaderProfile">Shader profile used for compiling.</param>
        /// <param name="resourceLink">The resourceLink.</param>
        public VertexShaderResource(string shaderProfile, ResourceLink resourceLink)
            : base(shaderProfile, resourceLink, ShaderResourceKind.HlsFile)
        {
            _inputLayouts = new Dictionary<D3D11.InputElementDescription[], D3D11.ID3D11InputLayout>();
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected internal override void LoadShader(EngineDevice device, byte[] shaderBytecode)
        {
            if (_vertexShader == null)
            {
                _vertexShader = device.DeviceD3D11_1.CreateVertexShader(shaderBytecode);
            }
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected internal override void UnloadShader()
        {
            foreach (var actInputLayout in _inputLayouts.Values)
            {
                SeeingSharpUtil.DisposeObject(actInputLayout);
            }
            _inputLayouts.Clear();

            _vertexShader = SeeingSharpUtil.DisposeObject(_vertexShader);
        }

        /// <summary>
        /// Generates the requested input layout.
        /// </summary>
        /// <param name="device">The device on which to create the input layout.</param>
        /// <param name="inputElements">An array of InputElements describing vertex input structure.</param>
        internal D3D11.ID3D11InputLayout GetInputLayout(EngineDevice device, D3D11.InputElementDescription[] inputElements)
        {
            if (_inputLayouts.TryGetValue(inputElements, out var inputLayout))
            {
                return inputLayout;
            }

            inputLayout = device.DeviceD3D11_1.CreateInputLayout(inputElements, this.ShaderBytecode);
            _inputLayouts.Add(inputElements, inputLayout);
            return inputLayout;
        }
    }
}
