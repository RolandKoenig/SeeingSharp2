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
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;
using SDXTK = SeeingSharp.Util.SdxTK;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class StandardTextureResource : TextureResource
    {
        // Configuration
        private ResourceLink _resourceLinkHighQuality;
        private ResourceLink _resourceLinkLowQuality;
        private MemoryMappedTexture<int> _inMemoryTexture;

        // Loaded resources
        private D3D11.Texture2D _texture;
        private D3D11.ShaderResourceView _textureView;

        // Runtime
        private bool _isCubeTexture;
        private bool _isRenderTarget;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardTextureResource" /> class.
        /// </summary>
        public StandardTextureResource(ResourceLink textureSource)
        {
            _resourceLinkHighQuality = textureSource;
            _resourceLinkLowQuality = textureSource;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardTextureResource" /> class.
        /// </summary>
        internal StandardTextureResource(MemoryMappedTexture<int> inMemoryTexture)
        {
            inMemoryTexture.EnsureNotNull(nameof(inMemoryTexture));

            _inMemoryTexture = inMemoryTexture;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardTextureResource" /> class.
        /// </summary>
        /// <param name="highQualityTextureSource">High quality version of the texture.</param>
        /// <param name="lowQualityTextureSource">Low quality version of the texture.</param>
        public StandardTextureResource(ResourceLink highQualityTextureSource, ResourceLink lowQualityTextureSource)
        {
            _resourceLinkHighQuality = highQualityTextureSource;
            _resourceLinkLowQuality = lowQualityTextureSource;
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            // Select source texture
            var source = _resourceLinkLowQuality;
            if (device.Configuration.TextureQuality == TextureQuality.High)
            {
                source = _resourceLinkHighQuality;
            }

            // Load the texture
            if (source != null)
            {
                _texture = GraphicsHelper.CreateTexture(device, source);
            }
            else if (_inMemoryTexture != null)
            {
                _texture = GraphicsHelper.Internals.LoadTexture2DFromMappedTexture(device, _inMemoryTexture);
            }

            // Create view for shaders
            _textureView = new D3D11.ShaderResourceView(device.DeviceD3D11_1, _texture);

            // Some checking..
            _isCubeTexture =
                _texture.Description.ArraySize == 6 &&
                (_texture.Description.OptionFlags & D3D11.ResourceOptionFlags.TextureCube) == D3D11.ResourceOptionFlags.TextureCube;
            _isRenderTarget =
                (_texture.Description.BindFlags & D3D11.BindFlags.RenderTarget) == D3D11.BindFlags.RenderTarget;
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _textureView = SeeingSharpUtil.DisposeObject(_textureView);
            _texture = SeeingSharpUtil.DisposeObject(_texture);

            _isCubeTexture = false;
            _isRenderTarget = false;
        }

        /// <summary>
        /// Gets the texture object.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        internal override D3D11.Texture2D Texture => _texture;

        /// <summary>
        /// Gets a ShaderResourceView targeting the texture.
        /// </summary>
        internal override D3D11.ShaderResourceView TextureView => _textureView;

        /// <summary>
        /// Is the object loaded correctly?
        /// </summary>
        public override bool IsLoaded => _textureView != null;

        /// <summary>
        /// Is this texture a cube texture?
        /// </summary>
        public bool IsCubeTexture => _isCubeTexture;

        /// <summary>
        /// Is this texture a render target texture?
        /// </summary>
        public bool IsRenderTargetTexture => _isRenderTarget;

        /// <summary>
        /// Gets the size of the texture array.
        /// 1 for normal textures.
        /// 6 for cubemap textures.
        /// </summary>
        public override int ArraySize => _texture.Description.ArraySize;
    }
}
