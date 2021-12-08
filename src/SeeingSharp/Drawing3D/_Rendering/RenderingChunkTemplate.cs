using System;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D
{
    public class RenderingChunkTemplate : IDisposable, ICheckDisposed
    {
        internal D3D11.ID3D11Buffer IndexBuffer;
        internal int IndexBufferId;
        internal int IndexCount;
        internal int SizePerVertex;
        internal int StartIndex;
        internal D3D11.ID3D11Buffer VertexBuffer;
        internal int VertexBufferId;
        internal Geometry Geometry;
        internal D3D11.InputElementDescription[] InputElements;

        public bool IsDisposed => IndexBuffer != null;

        public RenderingChunk CreateChunk(EngineDevice device, MaterialResource material)
        {
            var result = new RenderingChunk();
            result.Template = this;
            result.Material = material;
            result.InputLayout = material.GetInputLayout(device, InputElements);

            return result;
        }

        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref IndexBuffer);
            SeeingSharpUtil.SafeDispose(ref VertexBuffer);
        }
    }
}
