using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;

namespace SeeingSharp.Multimedia.Views
{
    public interface ISeeingSharpPainter
    {
        RenderLoop RenderLoop { get; }

        Scene Scene { get; set; }

        Camera3DBase Camera { get; set; }
    }
}