using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.Text;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class RenderingChunk : IDisposable
    {
        internal RenderingChunkTemplate Template;
        internal D3D11.InputLayout InputLayout;
        internal MaterialResource Material;

        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref InputLayout);
        }
    }
}
