using SeeingSharp.Core.Devices;
using SeeingSharp.Drawing3D;

namespace SeeingSharp.Core
{
    public class RenderPassDefaultTransparent : RenderPassBase
    {
        private DefaultResources _defaultResources;

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => _defaultResources != null;

        /// <summary>
        /// Applies this RenderPass (called before starting rendering first objects with it).
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        public override void Apply(RenderState renderState)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;

            deviceContext.OMSetBlendState(_defaultResources.AlphaBlendingBlendState);
            deviceContext.OMSetDepthStencilState(_defaultResources.DepthStencilStateDisableZWrites);
        }

        /// <summary>
        /// Discards this RenderPass (called after rendering all objects of this pass).
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        public override void Discard(RenderState renderState)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;

            deviceContext.OMSetBlendState(_defaultResources.DefaultBlendState);
            deviceContext.OMSetDepthStencilState(_defaultResources.DepthStencilStateDefault);
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        /// <param name="device">The target device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            //Get default resources
            _defaultResources = resources.GetResourceAndEnsureLoaded(
                DefaultResources.RESOURCE_KEY,
                () => new DefaultResources());
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        /// <param name="device">The target device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _defaultResources = null;
        }
    }
}
