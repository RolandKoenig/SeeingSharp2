#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion

#region using

using D3D11 = SharpDX.Direct3D11;

#endregion

namespace SeeingSharp.Multimedia.Drawing3D
{
    #region using

    using Checking;
    using Core;
    using SeeingSharp.Util;

    #endregion

    public class BitmapTextureResource : TextureResource
    {
        #region Member for Direct3D 11 rendering
        private D3D11.Texture2D m_texture;
        private D3D11.ShaderResourceView m_textureView;
        #endregion

        #region Generic members
        private MemoryMappedTexture32bpp m_mappedTexture;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapTextureResource"/> class.
        /// </summary>
        /// <param name="mappedTexture">The mapped texture.</param>
        public BitmapTextureResource(MemoryMappedTexture32bpp mappedTexture)
        {
            mappedTexture.EnsureNotNull(nameof(mappedTexture));

            m_mappedTexture = mappedTexture;
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            if (m_texture != null)
            {
                return;
            }

            // Load from mapped texture
            if (m_mappedTexture == null)
            {
                throw new SeeingSharpException("Unable to load BitmapTextureResource: No resource loader implemented!");
            }

            m_texture = GraphicsHelper.LoadTexture2DFromMappedTexture(device, m_mappedTexture);
            m_textureView = new D3D11.ShaderResourceView(device.DeviceD3D11_1, m_texture);

            return;
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            if (m_texture == null)
            {
                return;
            }

            m_textureView = SeeingSharpTools.DisposeObject(m_textureView);
            m_texture = SeeingSharpTools.DisposeObject(m_texture);
        }

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => m_texture != null;

        /// <summary>
        /// Gets the texture.
        /// </summary>
        public override D3D11.Texture2D Texture => m_texture;

        /// <summary>
        /// Gets a ShaderResourceView targeting the texture.
        /// </summary>
        public override D3D11.ShaderResourceView TextureView => m_textureView;

        /// <summary>
        /// Gets the size of the texture array.
        /// 1 for normal textures.
        /// 6 for cubemap textures.
        /// </summary>
        public override int ArraySize => m_texture.Description.ArraySize;
    }
}