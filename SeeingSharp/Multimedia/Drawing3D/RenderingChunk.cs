using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class RenderingChunk
    {
        internal D3D11.Buffer IndexBuffer;
        internal int IndexBufferID;
        internal int IndexCount;
        internal D3D11.InputLayout InputLayout;
        internal MaterialResource Material;
        internal int SizePerVertex;
        internal int StartIndex;
        internal D3D11.Buffer VertexBuffer;
        internal int VertexBufferID;
        internal Geometry Geometry;
        internal D3D11.InputElement[] InputElements;

        public RenderingChunk CopyForMaterial(EngineDevice device, MaterialResource material)
        {
            var result = this.MemberwiseClone() as RenderingChunk;
            result.Material = material;
            result.InputLayout = material.GenerateInputLayout(device, this.InputElements);

            return result;
        }
    }
}
