/*
    SeeingSharp and all applications distributed together with it. 
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
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
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
