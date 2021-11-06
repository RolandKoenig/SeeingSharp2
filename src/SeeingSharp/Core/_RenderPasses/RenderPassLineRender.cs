using SeeingSharp.Core.Devices;
using SeeingSharp.Drawing3D;
using D3D = SharpDX.Direct3D;

namespace SeeingSharp.Core
{
    public class RenderPassLineRender : RenderPassBase
    {
        private DefaultResources _defaultResources;
        private LineRenderResources _lineRenderResources;

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool IsLoaded => _defaultResources != null;

        /// <summary>
        /// Applies this RenderPass (called before starting rendering first objects with it).
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        public override void Apply(RenderState renderState)
        {
            var device = renderState.Device;

            device.DeviceImmediateContextD3D11.Rasterizer.State = _defaultResources.RasterStateLines;
            device.DeviceImmediateContextD3D11.InputAssembler.PrimitiveTopology = D3D.PrimitiveTopology.LineList;
            device.DeviceImmediateContextD3D11.InputAssembler.InputLayout = _lineRenderResources.InputLayout;

            device.DeviceImmediateContextD3D11.VertexShader.Set(_lineRenderResources.VertexShader.VertexShader);
            device.DeviceImmediateContextD3D11.PixelShader.Set(_lineRenderResources.PixelShader.PixelShader);
        }

        /// <summary>
        /// Discards this RenderPass (called after rendering all objects of this pass).
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        public override void Discard(RenderState renderState)
        {
            var device = renderState.Device;

            device.DeviceImmediateContextD3D11.Rasterizer.State = _defaultResources.RasterStateDefault;
            device.DeviceImmediateContextD3D11.InputAssembler.PrimitiveTopology = D3D.PrimitiveTopology.TriangleList;
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        /// <param name="device">The target device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _defaultResources = resources.GetResourceAndEnsureLoaded<DefaultResources>(DefaultResources.RESOURCE_KEY);
            _lineRenderResources = resources.GetResourceAndEnsureLoaded<LineRenderResources>(LineRenderResources.RESOURCE_KEY);
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        /// <param name="device">The target device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _defaultResources = null;
            _lineRenderResources = null;
        }
    }
}