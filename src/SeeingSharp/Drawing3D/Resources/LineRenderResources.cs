﻿using SeeingSharp.Checking;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D.Resources
{
    public class LineRenderResources : Resource
    {
        public static readonly NamedOrGenericKey RESOURCE_KEY = GraphicsCore.GetNextGenericResourceKey();

        // Private constants
        private static readonly NamedOrGenericKey s_keyVertexShader = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey s_keyPixelShader = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey s_keyConstantBuffer = GraphicsCore.GetNextGenericResourceKey();

        // Resources
        private VertexShaderResource? _vertexShader;
        private PixelShaderResource? _pixelShader;
        private D3D11.ID3D11InputLayout? _inputLayout;

        /// <summary>
        /// Is the resource loaded correctly?
        /// </summary>
        public override bool IsLoaded => _pixelShader != null;

        /// <summary>
        /// Gets the vertex shader resource.
        /// </summary>
        internal VertexShaderResource VertexShader
        {
            get
            {
                _vertexShader.EnsureResourceLoaded(typeof(LineRenderResources));
                return _vertexShader!;
            }
        }

        /// <summary>
        /// Gets the pixel shader resource.
        /// </summary>
        internal PixelShaderResource PixelShader
        {
            get
            {
                _pixelShader.EnsureResourceLoaded(typeof(LineRenderResources));
                return _pixelShader!;
            }
        }

        /// <summary>
        /// Gets the input layout for the vertex shader.
        /// </summary>
        internal D3D11.ID3D11InputLayout InputLayout
        {
            get
            {
                _inputLayout.EnsureResourceLoaded(typeof(LineRenderResources));
                return _inputLayout!;
            }
        }

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

            _inputLayout = device.DeviceD3D11_1.CreateInputLayout(
                LineVertex.InputElements, _vertexShader.ShaderBytecode);
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
