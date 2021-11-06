﻿using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Drawing3D
{
    public class GeometryShaderResource : ShaderResource
    {
        // Resources for Direct3D 11 rendering
        private D3D11.GeometryShader _geometryShader;

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => _geometryShader != null;

        /// <summary>
        /// Gets the loaded GeometryShader object.
        /// </summary>
        internal D3D11.GeometryShader GeometryShader => _geometryShader;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexShaderResource" /> class.
        /// </summary>
        /// <param name="shaderProfile">Shader profile used for compiling.</param>
        /// <param name="resourceLink">The resourceLink.</param>
        public GeometryShaderResource(string shaderProfile, ResourceLink resourceLink)
            : base(shaderProfile, resourceLink, ShaderResourceKind.HlsFile)
        {
           
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected internal override void LoadShader(EngineDevice device, byte[] shaderBytecode)
        {
            if (_geometryShader == null)
            {
                _geometryShader = new D3D11.GeometryShader(device.DeviceD3D11_1, shaderBytecode);
            }
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected internal override void UnloadShader()
        {
            SeeingSharpUtil.SafeDispose(ref _geometryShader);
        }
    }
}