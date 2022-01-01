using SeeingSharp.Core.Devices;
using D2D = Vortice.Direct2D1;

namespace SeeingSharp.Drawing2D.Resources
{
    public abstract class BrushResource : Drawing2DResourceBase
    {
        /// <summary>
        /// Gets the brush for the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to get the brush.</param>
        internal abstract D2D.ID2D1Brush GetBrush(EngineDevice engineDevice);
    }
}
