using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D.Resources
{
    public abstract class MaterialResource : Resource
    {
        public virtual bool IsExportable => false;

        /// <summary>
        /// Applies the material to the given render state.
        /// </summary>
        /// <param name="renderState">Current render state</param>
        /// <param name="previousMaterial">The previously applied material.</param>
        internal virtual void Apply(RenderState renderState, MaterialResource? previousMaterial) { }

        /// <summary>
        /// Discards the material in current render state.
        /// </summary>
        /// <param name="renderState">Current render state.</param>
        internal virtual void Discard(RenderState renderState) { }

        /// <summary>
        /// Generates the requested input layout.
        /// </summary>
        /// <param name="device">The device on which to create the input layout.</param>
        /// <param name="inputElements">An array of InputElements describing vertex input structure.</param>
        internal abstract D3D11.ID3D11InputLayout GetInputLayout(EngineDevice device, D3D11.InputElementDescription[] inputElements);
    }
}
