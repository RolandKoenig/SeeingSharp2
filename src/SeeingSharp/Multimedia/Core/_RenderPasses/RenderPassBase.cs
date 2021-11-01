namespace SeeingSharp.Multimedia.Core
{
    public abstract class RenderPassBase : Resource
    {
        /// <summary>
        /// Applies this RenderPass (called before starting rendering first objects with it).
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        public abstract void Apply(RenderState renderState);

        /// <summary>
        /// Discards this RenderPass (called after rendering all objects of this pass).
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        public abstract void Discard(RenderState renderState);
    }
}
