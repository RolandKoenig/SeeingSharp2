using SeeingSharp.Core;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Drawing3D
{
    public abstract class MaterialResource : Resource
    {
        public virtual bool IsExportable => false;

        /// <summary>
        /// Stores all required data into a new <see cref="ExportMaterialInfo"/>.
        /// </summary>
        public virtual ExportMaterialInfo PrepareForExport()
        {
            return null;
        }

        /// <summary>
        /// Applies the material to the given render state.
        /// </summary>
        /// <param name="renderState">Current render state</param>
        /// <param name="previousMaterial">The previously applied material.</param>
        internal virtual void Apply(RenderState renderState, MaterialResource previousMaterial) { }

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
        internal abstract D3D11.InputLayout GetInputLayout(EngineDevice device, D3D11.InputElement[] inputElements);
    }
}
