using SeeingSharp.Drawing3D.Resources;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D
{
    /// <summary>
    /// Describes on rendering chunk.
    /// </summary>
    public class RenderingChunk
    {
        internal RenderingChunkTemplate Template;
        internal D3D11.ID3D11InputLayout InputLayout;
        internal MaterialResource Material;

        internal RenderingChunk(
            RenderingChunkTemplate template,
            D3D11.ID3D11InputLayout inputLayout,
            MaterialResource material)
        {
            this.Template = template;
            this.InputLayout = inputLayout;
            this.Material = material;
        }
    }
}
