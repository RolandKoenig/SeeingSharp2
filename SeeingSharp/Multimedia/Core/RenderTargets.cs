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

using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Core
{
    internal struct RenderTargets
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargets"/> struct.
        /// </summary>
        /// <param name="colorBuffer">The color buffer.</param>
        /// <param name="depthStencilBuffer">The depth stencil buffer.</param>
        public RenderTargets(D3D11.RenderTargetView colorBuffer, D3D11.DepthStencilView depthStencilBuffer)
        {
            ColorBuffer = colorBuffer;
            DepthStencilBuffer = depthStencilBuffer;
            ObjectIDBuffer = null;
            NormalDepthBuffer = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargets"/> struct.
        /// </summary>
        /// <param name="colorBuffer">The color buffer.</param>
        /// <param name="depthStencilBuffer">The depth stencil buffer.</param>
        /// <param name="objectIDBuffer">The object identifier buffer.</param>
        public RenderTargets(D3D11.RenderTargetView colorBuffer, D3D11.DepthStencilView depthStencilBuffer, D3D11.RenderTargetView objectIDBuffer)
        {
            ColorBuffer = colorBuffer;
            DepthStencilBuffer = depthStencilBuffer;
            ObjectIDBuffer = objectIDBuffer;
            NormalDepthBuffer = null;
        }

        /// <summary>
        /// The default color output.
        /// </summary>
        internal D3D11.RenderTargetView ColorBuffer;

        /// <summary>
        /// The default depth-stencil output.
        /// </summary>
        internal D3D11.DepthStencilView DepthStencilBuffer;

        /// <summary>
        /// The ObjectID output buffer.
        /// </summary>
        internal D3D11.RenderTargetView ObjectIDBuffer;

        /// <summary>
        /// The normal/depth output buffer (processes the data for input on other postprocessing effects).
        /// </summary>
        internal D3D11.RenderTargetView NormalDepthBuffer;
    }
}
