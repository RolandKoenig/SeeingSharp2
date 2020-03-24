/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/

using SeeingSharp.Multimedia.Core;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
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
