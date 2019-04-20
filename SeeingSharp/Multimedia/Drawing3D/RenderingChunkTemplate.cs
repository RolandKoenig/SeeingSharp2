using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.Text;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class RenderingChunkTemplate : IDisposable
    {
        internal D3D11.Buffer IndexBuffer;
        internal int IndexBufferID;
        internal int IndexCount;
        internal int SizePerVertex;
        internal int StartIndex;
        internal D3D11.Buffer VertexBuffer;
        internal int VertexBufferID;
        internal Geometry Geometry;
        internal D3D11.InputElement[] InputElements;

        public RenderingChunk CreateChunk(EngineDevice device, MaterialResource material)
        {
            var result = new RenderingChunk();
            result.Template = this;
            result.Material = material;
            result.InputLayout = material.GenerateInputLayout(device, this.InputElements);

            return result;
        }

        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref IndexBuffer);
            SeeingSharpUtil.SafeDispose(ref VertexBuffer);
        }
    }
}
