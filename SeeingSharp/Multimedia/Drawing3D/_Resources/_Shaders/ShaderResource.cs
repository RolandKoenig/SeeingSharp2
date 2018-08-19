#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
#endregion
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using System.IO;
using SharpDX.D3DCompiler;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public abstract class ShaderResource : Resource
    {
        #region Generic members
        private string m_shaderProfile;
        private byte[] m_shaderBytecode;
        private ResourceLink m_resourceLink;
        private ShaderResourceKind m_resourceKind;
        #endregion

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
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            if(m_shaderBytecode == null) { GetShaderBytecode(m_resourceLink, m_resourceKind); }

            LoadShader(device, m_shaderBytecode);
        }

        private static byte[] GetShaderBytecode(ResourceLink resourceLink, ShaderResourceKind resourceKind)
        {
            switch(resourceKind)
            {
                case ShaderResourceKind.Bytecode:
                    using (Stream inStream = resourceLink.OpenInputStream())
                    {
                        return inStream.ReadAllBytes();
                    }

                case ShaderResourceKind.HlsFile:
                    var cResult = SharpDX.D3DCompiler.ShaderBytecode.Compile("", "", ShaderFlags.Debug, EffectFlags.None, "test.txt", SecondaryDataFlags.None, null);
               
                    return new byte[0];
            }

            throw new SeeingSharpException($"Unhanbled {nameof(ShaderResourceKind)}: {resourceKind}");
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            UnloadShader();
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
        /// Gets the shader's raw bytecode.
        /// </summary>
        public byte[] ShaderBytecode
        {
            get { return m_shaderBytecode; }
        }
    }
}
