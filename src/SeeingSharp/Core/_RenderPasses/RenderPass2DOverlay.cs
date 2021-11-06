using SeeingSharp.Core.Devices;

namespace SeeingSharp.Core
{
    public class RenderPass2DOverlay : RenderPassBase
    {
        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => true;

        /// <summary>
        /// Applies this RenderPass (called before starting rendering first objects with it).
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        public override void Apply(RenderState renderState)
        {
        }

        /// <summary>
        /// Discards this RenderPass (called after rendering all objects of this pass).
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        public override void Discard(RenderState renderState)
        {
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
        }
    }
}