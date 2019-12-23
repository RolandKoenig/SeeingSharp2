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
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using System.Numerics;

namespace SeeingSharp.Multimedia.Drawing3D
{
    internal class TexturePainterHelper
    {
        // Instance resource keys
        private readonly NamedOrGenericKey KEY_GEOMETRY = GraphicsCore.GetNextGenericResourceKey();
        private readonly NamedOrGenericKey KEY_RENDER_PARAMETERS = GraphicsCore.GetNextGenericResourceKey();
        private readonly NamedOrGenericKey KEY_MATERIAL = GraphicsCore.GetNextGenericResourceKey();

        // Configuration
        private NamedOrGenericKey m_texture;

        // Used resources
        private TextureResource m_textureResource;
        private GeometryResource m_geometryResource;
        private SpriteMaterialResource m_materialResource;
        private DefaultResources m_defaultResources;
        private ObjectRenderParameters m_renderParameters;
        private RenderingChunk[] m_renderingChunks;

        /// <summary>
        /// Initializes a new instance of the <see cref="TexturePainterHelper"/> class.
        /// </summary>
        /// <param name="textureKey">The texture key.</param>
        internal TexturePainterHelper(NamedOrGenericKey textureKey)
        {
            m_texture = textureKey;
            this.Scaling = 1f;
            this.Opacity = 1f;
            this.AccentuationFactor = 0f;
        }

        /// <summary>
        /// Loads all resources of this texture painter object.
        /// </summary>
        /// <param name="resources">The target resource dictionary.</param>
        internal void LoadResources(ResourceDictionary resources)
        {
            // Load material
            m_materialResource = resources.GetResourceAndEnsureLoaded(
                KEY_MATERIAL,
                () => new SpriteMaterialResource(m_texture));

            // Load geometry resource
            m_geometryResource = resources.GetResourceAndEnsureLoaded(
                KEY_GEOMETRY,
                () =>
                {
                    var geometry = new Geometry();
                    geometry.FirstSurface.BuildRect4V(
                        new Vector3(-1f, -1f, 0f),
                        new Vector3(1f, -1f, 0f),
                        new Vector3(1f, 1f, 0f),
                        new Vector3(-1f, 1f, 0f),
                        Color4.Transparent);
                    return new GeometryResource(geometry);
                });

            // Load the texture resource
            m_textureResource = resources.GetResourceAndEnsureLoaded<TextureResource>(m_texture);

            // Generate rendering chunks
            m_renderingChunks = m_geometryResource.BuildRenderingChunks(
                resources.Device, new MaterialResource[] { m_materialResource });

            // Get default resources
            m_defaultResources = resources.GetResourceAndEnsureLoaded(
                DefaultResources.RESOURCE_KEY,
                () => new DefaultResources());

            m_renderParameters = resources.GetResourceAndEnsureLoaded(
                KEY_RENDER_PARAMETERS,
                () => new ObjectRenderParameters());
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        internal void UnloadResources()
        {
            SeeingSharpUtil.DisposeObjects(m_renderingChunks);
            m_renderingChunks = null;

            m_geometryResource = null;
            m_textureResource = null;
            m_defaultResources = null;
            m_renderParameters = null;
            m_materialResource = null;
        }

        /// <summary>
        /// Renders the texture without using any effect.
        /// </summary>
        /// <param name="renderState">The render state to be used.</param>
        internal void RenderPlain(RenderState renderState)
        {
            // Apply rendering parameters
            m_renderParameters.UpdateValues(renderState, new CBPerObject
            {
                AccentuationFactor = this.AccentuationFactor,
                BorderMultiplier = 0f,
                BorderPart = 0f,
                Color = Vector4.Zero,
                Opacity = this.Opacity,
                SpriteScaling = this.Scaling,
                World = Matrix4x4.Identity
            });

            // Render using current configuration
            this.RenderInternal(renderState);
        }

        /// <summary>
        /// Renders the texture finally.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        private void RenderInternal(RenderState renderState)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;

            // Apply all current rendering parameters
            m_renderParameters.Apply(renderState);

            // Render the object
            deviceContext.OutputMerger.DepthStencilState = m_defaultResources.DepthStencilStateDisableZWrites;

            if (this.AlphaBlendMode == TexturePainterAlphaBlendMode.AlphaBlend)
            {
                deviceContext.OutputMerger.BlendState = m_defaultResources.AlphaBlendingBlendState;
            }

            renderState.RenderChunks(m_renderingChunks);

            if (this.AlphaBlendMode == TexturePainterAlphaBlendMode.AlphaBlend)
            {
                deviceContext.OutputMerger.BlendState = m_defaultResources.DefaultBlendState;
            }

            deviceContext.OutputMerger.DepthStencilState = m_defaultResources.DepthStencilStateDefault;
        }

        /// <summary>
        /// Gets or sets the scaling.
        /// </summary>
        public float Scaling { get; set; }

        public float AccentuationFactor { get; set; }

        public float Opacity { get; set; }

        /// <summary>
        /// Are resources loaded?
        /// </summary>
        public bool IsLoaded => m_geometryResource != null;

        /// <summary>
        /// Gets or sets the alpha blend mode.
        /// </summary>
        public TexturePainterAlphaBlendMode AlphaBlendMode
        {
            get; set;
        }
    }
}