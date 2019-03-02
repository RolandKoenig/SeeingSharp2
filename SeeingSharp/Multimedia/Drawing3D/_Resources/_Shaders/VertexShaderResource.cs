#region License information
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
#endregion
#region using

//Some namespace mappings
using D3D11 = SharpDX.Direct3D11;

#endregion

namespace SeeingSharp.Multimedia.Drawing3D
{
    #region using

    using Core;
    using SeeingSharp.Util;

    #endregion

    public class VertexShaderResource : ShaderResource
    {
        #region Resources for Direct3D 11 rendering
        private D3D11.VertexShader m_vertexShader;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexShaderResource" /> class.
        /// </summary>
        /// <param name="shaderProfile">Shader profile used for compiling.</param>
        /// <param name="resourceLink">The resourceLink.</param>
        public VertexShaderResource(string shaderProfile, ResourceLink resourceLink)
            : base(shaderProfile, resourceLink, ShaderResourceKind.HlsFile)
        {

        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected internal override void LoadShader(EngineDevice device, byte[] shaderBytecode)
        {
            if (m_vertexShader == null)
            {
                m_vertexShader = new D3D11.VertexShader(device.DeviceD3D11_1, shaderBytecode);
            }
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected internal override void UnloadShader()
        {
            m_vertexShader = SeeingSharpTools.DisposeObject(m_vertexShader);
        }

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded
        {
            get { return m_vertexShader != null; }
        }

        /// <summary>
        /// Gets the loaded VertexShader object.
        /// </summary>
        public D3D11.VertexShader VertexShader
        {
            get { return m_vertexShader; }
        }
    }
}
