using SeeingSharp.Core;
using SeeingSharp.Drawing3D;

namespace SeeingSharp.Views
{
    public interface ISeeingSharpPainter
    {
        RenderLoop RenderLoop { get; }

        Scene Scene { get; set; }

        Camera3DBase Camera { get; set; }
    }
}