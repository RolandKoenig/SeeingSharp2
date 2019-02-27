#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
using D3D = SharpDX.Direct3D;

#endregion

namespace SeeingSharp.Multimedia.Drawing3D
{
    #region using

    using System.Runtime.InteropServices;
    using Core;
    using SeeingSharp.Util;
    using SharpDX;

    #endregion

    public class LineRenderResources : Resource
    {
        #region Public constants
        internal static readonly NamedOrGenericKey RESOURCE_KEY = GraphicsCore.GetNextGenericResourceKey();
        #endregion

        #region Private constants
        private static readonly NamedOrGenericKey KEY_VERTEX_SHADER = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey KEY_PIXEL_SHADER = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey KEY_CONSTANT_BUFFER = GraphicsCore.GetNextGenericResourceKey();
        #endregion

        #region Resources
        private VertexShaderResource m_vertexShader;
        private PixelShaderResource m_pixelShader;
        private TypeSafeConstantBufferResource<ConstantBufferData> m_constantBuffer;
        private D3D11.InputLayout m_inputLayout;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="LineRenderResources" /> class.
        /// </summary>
        public LineRenderResources()
        {

        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            m_vertexShader = resources.GetResourceAndEnsureLoaded(
                KEY_VERTEX_SHADER,
                () => GraphicsHelper.GetVertexShaderResource(device, "LineRendering", "LineVertexShader"));
            m_pixelShader = resources.GetResourceAndEnsureLoaded(
                KEY_PIXEL_SHADER,
                () => GraphicsHelper.GetPixelShaderResource(device, "LineRendering", "LinePixelShader"));
            m_constantBuffer = resources.GetResourceAndEnsureLoaded(
                KEY_CONSTANT_BUFFER,
                () => new TypeSafeConstantBufferResource<ConstantBufferData>());

            m_inputLayout = new D3D11.InputLayout(
                device.DeviceD3D11_1,
                m_vertexShader.ShaderBytecode,
                LineVertex.InputElements);
        }

        /// <summary>
        /// Renders all given lines with the given parameters.
        /// </summary>
        /// <param name="renderState">The render state to be used.</param>
        /// <param name="worldViewProj">Current world-view-project transformation.</param>
        /// <param name="lineColor">The color for the line.</param>
        /// <param name="lineVertexBuffer">The vertex buffer containing all line vertices.</param>
        /// <param name="vertexCount">Total count of vertices.</param>
        internal void RenderLines(RenderState renderState, Matrix worldViewProj, Color4 lineColor, D3D11.Buffer lineVertexBuffer, int vertexCount)
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
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            SeeingSharpTools.SafeDispose(ref m_inputLayout);

            m_vertexShader = null;
            m_pixelShader = null;
            m_constantBuffer = null;
        }

        /// <summary>
        /// Is the resource loaded correctly?
        /// </summary>
        public override bool IsLoaded
        {
            get { return m_pixelShader != null; }
        }

        /// <summary>
        /// Gets the vertex shader resource.
        /// </summary>
        public VertexShaderResource VertexShader
        {
            get { return m_vertexShader; }
        }

        /// <summary>
        /// Gets the pixel shader resource.
        /// </summary>
        public PixelShaderResource PixelShader
        {
            get { return m_pixelShader; }
        }

        /// <summary>
        /// Gets the constant buffer resource.
        /// </summary>
        public ConstantBufferResource ConstantBuffer
        {
            get { return m_constantBuffer; }
        }

        /// <summary>
        /// Gets the input layout for the vertex shader.
        /// </summary>
        internal D3D11.InputLayout InputLayout
        {
            get { return m_inputLayout; }
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        [StructLayout(LayoutKind.Sequential)]
        private struct ConstantBufferData
        {
            public Matrix WorldViewProj;
            public Color4 DiffuseColor;
        }
    }
}
