﻿/*
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
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class SpriteMaterialResource : MaterialResource
    {
        // Static Resource keys
        private static readonly NamedOrGenericKey RES_KEY_VERTEX_SHADER = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey RES_KEY_PIXEL_SHADER = GraphicsCore.GetNextGenericResourceKey();

        // Resource members
        private TextureResource m_textureResource;
        private DefaultResources m_defaultResources;
        private PixelShaderResource m_pixelShader;
        private VertexShaderResource m_vertexShader;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteMaterialResource"/> class.
        /// </summary>
        /// <param name="textureKey">The name of the texture to be rendered.</param>
        public SpriteMaterialResource(NamedOrGenericKey textureKey)
        {
            this.TextureKey = textureKey;
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            //Load all required shaders and constant buffers
            m_vertexShader = resources.GetResourceAndEnsureLoaded(
                RES_KEY_VERTEX_SHADER,
                () => GraphicsHelper.Internals.GetVertexShaderResource(device, "Sprite", "SpriteVertexShader"));
            m_pixelShader = resources.GetResourceAndEnsureLoaded(
                RES_KEY_PIXEL_SHADER,
                () => GraphicsHelper.Internals.GetPixelShaderResource(device, "Sprite", "SpritePixelShader"));

            // Get a reference to default resource object
            m_defaultResources = resources.GetResourceAndEnsureLoaded<DefaultResources>(DefaultResources.RESOURCE_KEY);

            //Load the texture if any configured.
            if (!this.TextureKey.IsEmpty)
            {
                //Get texture resource
                m_textureResource = resources.GetResourceAndEnsureLoaded<TextureResource>(this.TextureKey);
            }
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            m_defaultResources = null;
            m_vertexShader = null;
            m_pixelShader = null;
            m_textureResource = null;
        }

        /// <summary>
        /// Generates the requested input layout.
        /// </summary>
        /// <param name="device">The device on which to create the input layout.</param>
        /// <param name="inputElements">An array of InputElements describing vertex input structure.</param>
        internal override D3D11.InputLayout GetInputLayout(EngineDevice device, D3D11.InputElement[] inputElements)
        {
            return m_vertexShader.GetInputLayout(device, inputElements);
        }

        /// <summary>
        /// Applies the material to the given render state.
        /// </summary>
        /// <param name="renderState">Current render state</param>
        /// <param name="previousMaterial">The previously applied material.</param>
        /// <exception cref="SeeingSharpGraphicsException">Effect  + this.Effect +  not supported!</exception>
        internal override void Apply(RenderState renderState, MaterialResource previousMaterial)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;
            var isResourceSameType =
                previousMaterial != null &&
                previousMaterial.ResourceType == this.ResourceType;
            if (!isResourceSameType)
            {
                deviceContext.PixelShader.SetSampler(0, m_defaultResources.GetSamplerState(TextureSamplerQualityLevel.Low));

                deviceContext.VertexShader.Set(m_vertexShader.VertexShader);
                deviceContext.PixelShader.Set(m_pixelShader.PixelShader);
            }

            // Set texture resource (if set)
            if (m_textureResource != null)
            {
                deviceContext.PixelShader.SetShaderResource(0, m_textureResource.TextureView);
            }
            else
            {
                deviceContext.PixelShader.SetShaderResource(0, null);
            }
        }

        /// <summary>
        /// Gets the key of the texture resource.
        /// </summary>
        public NamedOrGenericKey TextureKey { get; }

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => m_vertexShader != null;
    }
}