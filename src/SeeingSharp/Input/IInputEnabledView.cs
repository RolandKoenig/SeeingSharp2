using SeeingSharp.Core;

namespace SeeingSharp.Input
{
    /// <summary>
    /// This interfaces is used by SeeingSharp internally to abstract input relevant
    /// information of a view.
    /// </summary>
    public interface IInputEnabledView
    {
        /// <summary>
        /// Gets the RenderLoop that is currently in use.
        /// </summary>
        RenderLoop RenderLoop { get; }

        /// <summary>
        /// Does the target control have focus?
        /// </summary>
        bool Focused { get; }
    }
}