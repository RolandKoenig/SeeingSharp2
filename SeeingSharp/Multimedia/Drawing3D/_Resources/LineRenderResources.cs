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
using System.Numerics;
using System.Runtime.InteropServices;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class LineRenderResources : Resource
    {
        internal static readonly NamedOrGenericKey RESOURCE_KEY = GraphicsCore.GetNextGenericResourceKey();

        // Private constants
        private static readonly NamedOrGenericKey KEY_VERTEX_SHADER = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey KEY_PIXEL_SHADER = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey KEY_CONSTANT_BUFFER = GraphicsCore.GetNextGenericResourceKey();

        // Resources
        private VertexShaderResource m_vertexShader;
        private PixelShaderResource m_pixelShader;
        private D3D11.InputLayout m_inputLayout;

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            m_vertexShader = resources.GetResourceAndEnsureLoaded(
                KEY_VERTEX_SHADER,
                () => GraphicsHelper.Internals.GetVertexShaderResource(device, "LineRendering", "LineVertexShader"));
            m_pixelShader = resources.GetResourceAndEnsureLoaded(
                KEY_PIXEL_SHADER,
                () => GraphicsHelper.Internals.GetPixelShaderResource(device, "LineRendering", "LinePixelShader"));

            m_inputLayout = new D3D11.InputLayout(
                device.DeviceD3D11_1,
                m_vertexShader.ShaderBytecode,
                LineVertex.InputElements);
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            SeeingSharpUtil.SafeDispose(ref m_inputLayout);

            m_vertexShader = null;
            m_pixelShader = null;
        }

        /// <summary>
        /// Is the resource loaded correctly?
        /// </summary>
        public override bool IsLoaded => m_pixelShader != null;

        /// <summary>
        /// Gets the vertex shader resource.
        /// </summary>
        public VertexShaderResource VertexShader => m_vertexShader;

        /// <summary>
        /// Gets the pixel shader resource.
        /// </summary>
        public PixelShaderResource PixelShader => m_pixelShader;

        /// <summary>
        /// Gets the input layout for the vertex shader.
        /// </summary>
        internal D3D11.InputLayout InputLayout => m_inputLayout;
    }
}
