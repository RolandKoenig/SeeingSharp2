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

using SDXTK = SeeingSharp.Multimedia.Util.SdxTK;
using D3D11 = SharpDX.Direct3D11;

#endregion

namespace SeeingSharp.Multimedia.Drawing3D
{
    #region using

    using Checking;
    using Core;
    using SeeingSharp.Util;

    #endregion

    public class StandardTextureResource : TextureResource
    {
        #region configuration
        private ResourceLink m_resourceLinkHighQuality;
        private ResourceLink m_resourceLinkLowQuality;
        private MemoryMappedTexture32bpp m_inMemoryTexture;
        #endregion

        #region Loaded resources
        private D3D11.Texture2D m_texture;
        private D3D11.ShaderResourceView m_textureView;
        #endregion

        #region Runtime

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardTextureResource" /> class.
        /// </summary>
        public StandardTextureResource(ResourceLink textureSource)
        {
            m_resourceLinkHighQuality = textureSource;
            m_resourceLinkLowQuality = textureSource;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardTextureResource" /> class.
        /// </summary>
        internal StandardTextureResource(MemoryMappedTexture32bpp inMemoryTexture)
        {
            inMemoryTexture.EnsureNotNull(nameof(inMemoryTexture));

            m_inMemoryTexture = inMemoryTexture;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardTextureResource" /> class.
        /// </summary>
        /// <param name="highQualityTextureSource">High quality version of the texture.</param>
        /// <param name="lowQualityTextureSource">Low quality version of the texture.</param>
        public StandardTextureResource(ResourceLink highQualityTextureSource, ResourceLink lowQualityTextureSource)
        {
            m_resourceLinkHighQuality = highQualityTextureSource;
            m_resourceLinkLowQuality = lowQualityTextureSource;
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            // Select source texture
            var source = m_resourceLinkLowQuality;

            if (device.Configuration.TextureQuality == TextureQuality.Hight)
            {
                source = m_resourceLinkHighQuality;
            }

            // Load the texture
            if (source != null)
            {
                using (var inStream = source.OpenInputStream())
                using (var rawImage = SDXTK.Image.Load(inStream))
                {
                    m_texture = GraphicsHelper.CreateTexture(device, rawImage);
                }
            }
            else if (m_inMemoryTexture != null)
            {
                m_texture = GraphicsHelper.LoadTexture2DFromMappedTexture(device, m_inMemoryTexture);
            }

            // Create view for shaders
            m_textureView = new D3D11.ShaderResourceView(device.DeviceD3D11_1, m_texture);

            // Some checking..
            IsCubeTexture =
                (m_texture.Description.ArraySize == 6) &&
                ((m_texture.Description.OptionFlags & D3D11.ResourceOptionFlags.TextureCube) == D3D11.ResourceOptionFlags.TextureCube);
            IsRenderTargetTexture =
                (m_texture.Description.BindFlags & D3D11.BindFlags.RenderTarget) == D3D11.BindFlags.RenderTarget;
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            m_textureView = SeeingSharpTools.DisposeObject(m_textureView);
            m_texture = SeeingSharpTools.DisposeObject(m_texture);

            IsCubeTexture = false;
            IsRenderTargetTexture = false;
        }

        /// <summary>
        /// Gets the texture object.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public override D3D11.Texture2D Texture => m_texture;

        /// <summary>
        /// Gets a ShaderResourceView targeting the texture.
        /// </summary>
        public override D3D11.ShaderResourceView TextureView => m_textureView;

        /// <summary>
        /// Is the object loaded correctly?
        /// </summary>
        public override bool IsLoaded => m_textureView != null;

        /// <summary>
        /// Is this texture a cube texture?
        /// </summary>
        public bool IsCubeTexture { get; private set; }

        /// <summary>
        /// Is this texture a render target texture?
        /// </summary>
        public bool IsRenderTargetTexture { get; private set; }

        /// <summary>
        /// Gets the size of the texture array.
        /// 1 for normal textures.
        /// 6 for cubemap textures.
        /// </summary>
        public override int ArraySize => m_texture.Description.ArraySize;
    }
}