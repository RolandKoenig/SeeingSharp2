﻿using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using SharpDX.D3DCompiler;

namespace SeeingSharp.Drawing3D
{
    public abstract class ShaderResource : Resource
    {
        // Generic members
        private string _shaderProfile;
        private byte[] _shaderBytecode;
        private ResourceLink _resourceLink;
        private ShaderResourceKind _resourceKind;

        /// <summary>
        /// Gets the shaders raw bytecode.
        /// </summary>
        public byte[] ShaderBytecode => _shaderBytecode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderResource"/> class.
        /// </summary>
        /// <param name="shaderProfile">Shader profile used for compilation.</param>
        /// <param name="resourceLink">The source of the resource.</param>
        /// <param name="resourceKind">Kind of the shader resource.</param>
        protected ShaderResource(string shaderProfile, ResourceLink resourceLink, ShaderResourceKind resourceKind)
        {
            _resourceKind = resourceKind;
            _shaderProfile = shaderProfile;
            _resourceLink = resourceLink;
        }

        /// <summary>
        /// Loads a shader using the given bytecode.
        /// </summary>
        protected internal abstract void LoadShader(EngineDevice device, byte[] shaderBytecode);

        /// <summary>
        /// Unloads the shader.
        /// </summary>
        protected internal abstract void UnloadShader();

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            if (_shaderBytecode == null)
            {
                _shaderBytecode = GetShaderBytecode(device, _resourceLink, _resourceKind, _shaderProfile);
            }

            this.LoadShader(device, _shaderBytecode);
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            this.UnloadShader();
        }

        private static byte[] GetShaderBytecode(EngineDevice device, ResourceLink resourceLink, ShaderResourceKind resourceKind, string shaderModel)
        {
            switch (resourceKind)
            {
                case ShaderResourceKind.Bytecode:
                    using (var inStream = resourceLink.OpenInputStream())
                    {
                        return inStream.ReadAllBytes();
                    }

                case ShaderResourceKind.HlsFile:
                    using (ReusableStringBuilders.Current.UseStringBuilder(out var singleShaderSourceBuilder, 10024))
                    {
                        SingleShaderFileBuilder.ReadShaderFileAndResolveIncludes(
                            resourceLink,
                            singleShaderSourceBuilder);

                        var shaderSource = singleShaderSourceBuilder.ToString();
                        var compileResult = SharpDX.D3DCompiler.ShaderBytecode.Compile(
                            shaderSource,
                            "main",
                            shaderModel,
                            device.DebugEnabled ? ShaderFlags.Debug : ShaderFlags.None,
                            sourceFileName: resourceLink.ToString());
                        if (compileResult.HasErrors)
                        {
                            throw new SeeingSharpGraphicsException($"Unable to compile shader from {resourceLink}: {compileResult.ResultCode} - {compileResult.Message}");
                        }
                        return compileResult.Bytecode.Data;
                    }

                default:
                    throw new SeeingSharpException($"Unhandled {nameof(ShaderResourceKind)}: {resourceKind}");
            }
        }
    }
}