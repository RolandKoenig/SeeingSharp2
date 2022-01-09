using System;
using SeeingSharp.Core.Devices;
using SeeingSharp.Drawing3D.Geometries;
using SeeingSharp.Drawing3D.Resources;
using SeeingSharp.Util;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D
{
    public class RenderingChunkTemplate : IDisposable, ICheckDisposed
    {
        internal D3D11.ID3D11Buffer? IndexBuffer;
        internal int IndexBufferId;
        internal int IndexCount;
        internal int SizePerVertex;
        internal int StartIndex;
        internal D3D11.ID3D11Buffer? VertexBuffer;
        internal int VertexBufferId;
        internal Geometry Geometry = null!;
        internal D3D11.InputElementDescription[] InputElements = null!;

        public bool IsDisposed => IndexBuffer != null;

        public RenderingChunk CreateChunk(EngineDevice device, MaterialResource material)
        {
            var result = new RenderingChunk(
                this, 
                material.GetInputLayout(device, InputElements),
                material);

            return result;
        }

        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref IndexBuffer);
            SeeingSharpUtil.SafeDispose(ref VertexBuffer);
        }
    }
}
