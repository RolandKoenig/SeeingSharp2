using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Core
{
    internal struct RenderTargets
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargets"/> struct.
        /// </summary>
        /// <param name="colorBuffer">The color buffer.</param>
        /// <param name="depthStencilBuffer">The depth stencil buffer.</param>
        public RenderTargets(in D3D11.ID3D11RenderTargetView colorBuffer, in D3D11.ID3D11DepthStencilView depthStencilBuffer)
        {
            ColorBuffer = colorBuffer;
            DepthStencilBuffer = depthStencilBuffer;
            ObjectIdBuffer = null;
            NormalDepthBuffer = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargets"/> struct.
        /// </summary>
        /// <param name="colorBuffer">The color buffer.</param>
        /// <param name="depthStencilBuffer">The depth stencil buffer.</param>
        /// <param name="objectIdBuffer">The object identifier buffer.</param>
        public RenderTargets(in D3D11.ID3D11RenderTargetView colorBuffer, in D3D11.ID3D11DepthStencilView depthStencilBuffer, in D3D11.ID3D11RenderTargetView objectIdBuffer)
        {
            ColorBuffer = colorBuffer;
            DepthStencilBuffer = depthStencilBuffer;
            ObjectIdBuffer = objectIdBuffer;
            NormalDepthBuffer = null;
        }

        /// <summary>
        /// The default color output.
        /// </summary>
        internal D3D11.ID3D11RenderTargetView? ColorBuffer;

        /// <summary>
        /// The default depth-stencil output.
        /// </summary>
        internal D3D11.ID3D11DepthStencilView? DepthStencilBuffer;

        /// <summary>
        /// The ObjectId output buffer.
        /// </summary>
        internal D3D11.ID3D11RenderTargetView? ObjectIdBuffer;

        /// <summary>
        /// The normal/depth output buffer (processes the data for input on other postprocessing effects).
        /// </summary>
        internal D3D11.ID3D11RenderTargetView? NormalDepthBuffer;
    }
}
