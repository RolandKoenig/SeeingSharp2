using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    /// <summary>
    /// Describes on rendering chunk.
    /// </summary>
    public class RenderingChunk
    {
        internal RenderingChunkTemplate Template;
        internal D3D11.InputLayout InputLayout;
        internal MaterialResource Material;
    }
}
