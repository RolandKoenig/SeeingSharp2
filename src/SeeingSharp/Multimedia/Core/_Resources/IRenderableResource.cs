namespace SeeingSharp.Multimedia.Core
{
    public interface IRenderableResource
    {
        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Triggers internal rendering within the resource (e. g. Render to Texture).
        /// </summary>
        /// <param name="renderState">Current render state.</param>
        void Render(RenderState renderState);
    }
}
