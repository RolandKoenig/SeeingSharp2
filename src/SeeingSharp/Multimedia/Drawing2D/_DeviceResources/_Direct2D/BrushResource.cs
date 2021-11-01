using SeeingSharp.Multimedia.Core;
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public abstract class BrushResource : Drawing2DResourceBase
    {
        /// <summary>
        /// Gets the brush for the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to get the brush.</param>
        internal abstract D2D.Brush GetBrush(EngineDevice engineDevice);
    }
}
