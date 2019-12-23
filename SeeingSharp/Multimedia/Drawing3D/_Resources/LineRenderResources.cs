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
        private TypeSafeConstantBufferResource<ConstantBufferData> m_constantBuffer;
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
            m_constantBuffer = resources.GetResourceAndEnsureLoaded(
                KEY_CONSTANT_BUFFER,
                () => new TypeSafeConstantBufferResource<ConstantBufferData>());

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
            m_constantBuffer = null;
        }

        /// <summary>
        /// Renders all given lines with the given parameters.
        /// </summary>
        /// <param name="renderState">The render state to be used.</param>
        /// <param name="worldViewProj">Current world-view-project transformation.</param>
        /// <param name="lineColor">The color for the line.</param>
        /// <param name="lineVertexBuffer">The vertex buffer containing all line vertices.</param>
        /// <param name="vertexCount">Total count of vertices.</param>
        internal void RenderLines(RenderState renderState, Matrix4x4 worldViewProj, Color4 lineColor, D3D11.Buffer lineVertexBuffer, int vertexCount)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;

            //Apply constant buffer data
            var constantData = new ConstantBufferData
            {
                DiffuseColor = lineColor,
                WorldViewProj = worldViewProj
            };

            m_constantBuffer.SetData(deviceContext, constantData);

            //Apply vertex buffer and draw lines
            deviceContext.VertexShader.SetConstantBuffer(4, m_constantBuffer.ConstantBuffer);
            deviceContext.PixelShader.SetConstantBuffer(4, m_constantBuffer.ConstantBuffer);
            deviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(lineVertexBuffer, LineVertex.Size, 0));
            deviceContext.Draw(vertexCount, 0);
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
        /// Gets the constant buffer resource.
        /// </summary>
        public ConstantBufferResource ConstantBuffer => m_constantBuffer;

        /// <summary>
        /// Gets the input layout for the vertex shader.
        /// </summary>
        internal D3D11.InputLayout InputLayout => m_inputLayout;

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        [StructLayout(LayoutKind.Sequential)]
        private struct ConstantBufferData
        {
            public Matrix4x4 WorldViewProj;
            public Color4 DiffuseColor;
        }
    }
}
