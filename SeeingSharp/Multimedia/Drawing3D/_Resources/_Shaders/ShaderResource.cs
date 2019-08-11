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
using SharpDX.D3DCompiler;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public abstract class ShaderResource : Resource
    {
        // Generic members
        private string m_shaderProfile;
        private byte[] m_shaderBytecode;
        private ResourceLink m_resourceLink;
        private ShaderResourceKind m_resourceKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderResource"/> class.
        /// </summary>
        /// <param name="shaderProfile">Shader profile used for compilation.</param>
        /// <param name="resourceLink">The source of the resource.</param>
        /// <param name="resourceKind">Kind of the shader resource.</param>
        protected ShaderResource(string shaderProfile, ResourceLink resourceLink, ShaderResourceKind resourceKind)
        {
            m_resourceKind = resourceKind;
            m_shaderProfile = shaderProfile;
            m_resourceLink = resourceLink;
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
            if(m_shaderBytecode == null)
            {
                m_shaderBytecode = GetShaderBytecode(device, m_resourceLink, m_resourceKind, m_shaderProfile);
            }

            this.LoadShader(device, m_shaderBytecode);
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
            switch(resourceKind)
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
                        if(compileResult.HasErrors)
                        {
                            throw new SeeingSharpGraphicsException($"Unable to compile shader from {resourceLink}: {compileResult.ResultCode} - {compileResult.Message}");
                        }
                        return compileResult.Bytecode.Data;
                    }

                default:
                    throw new SeeingSharpException($"Unhandled {nameof(ShaderResourceKind)}: {resourceKind}");
            }
        }

        /// <summary>
        /// Gets the shaders raw bytecode.
        /// </summary>
        public byte[] ShaderBytecode => m_shaderBytecode;
    }
}
