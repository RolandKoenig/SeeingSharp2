using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;

namespace SeeingSharp.Drawing3D
{
    public class ObjectRenderParameters : Resource
    {
        // Resource keys
        private readonly NamedOrGenericKey _keyConstantBuffer = GraphicsCore.GetNextGenericResourceKey();

        // Resources
        private TypeSafeConstantBufferResource<CBPerObject> _cbPerObject;

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => _cbPerObject != null;

        /// <summary>
        /// Does this object needs refreshing?
        /// </summary>
        internal bool NeedsRefresh;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectRenderParameters" /> class.
        /// </summary>
        internal ObjectRenderParameters()
        {
            NeedsRefresh = true;
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _cbPerObject = resources.GetResourceAndEnsureLoaded(
                _keyConstantBuffer,
                () => new TypeSafeConstantBufferResource<CBPerObject>());
            NeedsRefresh = true;
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _cbPerObject = null;
        }

        /// <summary>
        /// Triggers unloading of this resource.
        /// </summary>
        internal void MarkForUnloading()
        {
            this.Dictionary?.MarkForUnloading(this);
        }

        /// <summary>
        /// Updates all parameters.
        /// </summary>
        /// <param name="renderState">The render state on which to apply.</param>
        /// <param name="cbPerObject">Constant buffer data.</param>
        internal void UpdateValues(RenderState renderState, CBPerObject cbPerObject)
        {
            _cbPerObject.SetData(renderState.Device.DeviceImmediateContextD3D11, cbPerObject);
        }

        /// <summary>
        /// Applies all parameters.
        /// </summary>
        /// <param name="renderState">The render state on which to apply.</param>
        internal void Apply(RenderState renderState)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;

            // Apply constant buffer on shaders
            deviceContext.VSSetConstantBuffer(2, _cbPerObject.ConstantBuffer);
            deviceContext.PSSetConstantBuffer(2, _cbPerObject.ConstantBuffer);
        }
    }
}
