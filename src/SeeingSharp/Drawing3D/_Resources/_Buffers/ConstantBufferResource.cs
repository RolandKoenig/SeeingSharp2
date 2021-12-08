using System;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D
{
    public class ConstantBufferResource : Resource
    {
        // Direct3D resources
        private D3D11.ID3D11Buffer _constantBuffer;

        /// <summary>
        /// Is the buffer loaded correctly?
        /// </summary>
        public override bool IsLoaded => _constantBuffer != null;

        /// <summary>
        /// Gets the total size of the constant buffer.
        /// </summary>
        public int BufferSize { get; }

        /// <summary>
        /// Gets the buffer object.
        /// </summary>
        internal D3D11.ID3D11Buffer ConstantBuffer => _constantBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantBufferResource" /> class.
        /// </summary>
        public ConstantBufferResource(int bufferSize)
        {
            if (bufferSize < 1) { throw new ArgumentException("Invalid value for buffer size!", nameof(bufferSize)); }
            this.BufferSize = bufferSize;
        }

        /// <summary>
        /// Creates the constant buffer object.
        /// </summary>
        protected internal virtual D3D11.ID3D11Buffer CreateConstantBuffer(EngineDevice device)
        {
            return device.DeviceD3D11_1.CreateBuffer(
                D3D11.BindFlags.ConstantBuffer,
                new byte[this.BufferSize],
                this.BufferSize,
                D3D11.ResourceUsage.Dynamic,
                D3D11.CpuAccessFlags.Write,
                D3D11.ResourceOptionFlags.None,
                0);
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _constantBuffer = this.CreateConstantBuffer(device);
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _constantBuffer = SeeingSharpUtil.DisposeObject(_constantBuffer);
        }
    }
}