using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;

namespace SeeingSharp.Drawing3D
{
    public class SceneRenderParameters : Resource
    {
        // Resource keys
        private readonly NamedOrGenericKey _keyConstantBuffer = GraphicsCore.GetNextGenericResourceKey();

        // Resources
        private TypeSafeConstantBufferResource<CBPerFrame> _cbPerFrame;

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => _cbPerFrame != null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneRenderParameters" /> class.
        /// </summary>
        internal SceneRenderParameters()
        {

        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _cbPerFrame = resources.GetResourceAndEnsureLoaded(
                _keyConstantBuffer,
                () => new TypeSafeConstantBufferResource<CBPerFrame>());
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _cbPerFrame = null;

            //resources.RemoveResource(KEY_CONSTANT_BUFFER);
        }

        /// <summary>
        /// Updates all parameters.
        /// </summary>
        /// <param name="renderState">The render state on which to apply.</param>
        /// <param name="cbPerFrame">Per frame data.</param>
        internal void UpdateValues(RenderState renderState, CBPerFrame cbPerFrame)
        {
            _cbPerFrame.SetData(renderState.Device.DeviceImmediateContextD3D11, cbPerFrame);
        }

        /// <summary>
        /// Applies all parameters.
        /// </summary>
        /// <param name="renderState">The render state on which to apply.</param>
        internal void Apply(RenderState renderState)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;

            // Apply constant buffer on shaders
            deviceContext.VertexShader.SetConstantBuffer(0, _cbPerFrame.ConstantBuffer);
            deviceContext.PixelShader.SetConstantBuffer(0, _cbPerFrame.ConstantBuffer);
        }
    }
}
