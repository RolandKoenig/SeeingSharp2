using SeeingSharp.Core;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D
{
    public abstract class TextureResource : Resource
    {
        /// <summary>
        /// Gets the texture object.
        /// </summary>
        internal abstract D3D11.ID3D11Texture2D Texture
        {
            get;
        }

        /// <summary>
        /// Gets a ShaderResourceView targeting the texture.
        /// </summary>
        internal abstract D3D11.ID3D11ShaderResourceView TextureView
        {
            get;
        }

        /// <summary>
        /// Gets the size of the texture array.
        /// 1 for normal textures.
        /// 6 for cubemap textures.
        /// </summary>
        public abstract int ArraySize
        {
            get;
        }
    }
}
