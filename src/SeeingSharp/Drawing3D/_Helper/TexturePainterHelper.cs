using System.Numerics;
using SeeingSharp.Core;
using SeeingSharp.Drawing3D.Resources;
using SeeingSharp.Util;

namespace SeeingSharp.Drawing3D
{
    internal class TexturePainterHelper
    {
        // Instance resource keys
        private readonly NamedOrGenericKey _keyGeometry = GraphicsCore.GetNextGenericResourceKey();
        private readonly NamedOrGenericKey _keyRenderParameters = GraphicsCore.GetNextGenericResourceKey();
        private readonly NamedOrGenericKey _keyMaterial = GraphicsCore.GetNextGenericResourceKey();

        // Configuration
        private NamedOrGenericKey _texture;

        // Used resources
        private GeometryResource _geometryResource;
        private SpriteMaterialResource _materialResource;
        private DefaultResources _defaultResources;
        private ObjectRenderParameters _renderParameters;
        private RenderingChunk[] _renderingChunks;

        /// <summary>
        /// Gets or sets the scaling.
        /// </summary>
        public float Scaling { get; set; }

        public float AccentuationFactor { get; set; }

        public float Opacity { get; set; }

        /// <summary>
        /// Are resources loaded?
        /// </summary>
        public bool IsLoaded => _geometryResource != null;

        /// <summary>
        /// Gets or sets the alpha blend mode.
        /// </summary>
        public TexturePainterAlphaBlendMode AlphaBlendMode
        {
            get; set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TexturePainterHelper"/> class.
        /// </summary>
        /// <param name="textureKey">The texture key.</param>
        internal TexturePainterHelper(NamedOrGenericKey textureKey)
        {
            _texture = textureKey;
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
            _materialResource = resources.GetResourceAndEnsureLoaded(
                _keyMaterial,
                () => new SpriteMaterialResource(_texture));

            // Load geometry resource
            _geometryResource = resources.GetResourceAndEnsureLoaded(
                _keyGeometry,
                () =>
                {
                    var geometry = new Geometry();
                    geometry.FirstSurface.BuildRect(
                        new Vector3(-1f, -1f, 0f),
                        new Vector3(1f, -1f, 0f),
                        new Vector3(1f, 1f, 0f),
                        new Vector3(-1f, 1f, 0f));
                    return new GeometryResource(geometry);
                });

            // Generate rendering chunks
            _renderingChunks = _geometryResource.BuildRenderingChunks(
                resources.Device, new MaterialResource[] { _materialResource });

            // Get default resources
            _defaultResources = resources.GetResourceAndEnsureLoaded(
                DefaultResources.RESOURCE_KEY,
                () => new DefaultResources());

            _renderParameters = resources.GetResourceAndEnsureLoaded(
                _keyRenderParameters,
                () => new ObjectRenderParameters());
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        internal void UnloadResources()
        {
            _renderingChunks = null;
            _geometryResource = null;
            _defaultResources = null;
            _renderParameters = null;
            _materialResource = null;
        }

        /// <summary>
        /// Renders the texture without using any effect.
        /// </summary>
        /// <param name="renderState">The render state to be used.</param>
        internal void RenderPlain(RenderState renderState)
        {
            // Apply rendering parameters
            _renderParameters.UpdateValues(renderState, new CBPerObject
            {
                AccentuationFactor = this.AccentuationFactor,
                Color = Vector4.One,
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
            _renderParameters.Apply(renderState);

            deviceContext.OMSetDepthStencilState(_defaultResources.DepthStencilStateDisableZWrites);
            if (this.AlphaBlendMode == TexturePainterAlphaBlendMode.AlphaBlend)
            {
                deviceContext.OMSetBlendState(_defaultResources.AlphaBlendingBlendState);
            }

            // Render the object
            renderState.RenderChunks(_renderingChunks);

            if (this.AlphaBlendMode == TexturePainterAlphaBlendMode.AlphaBlend)
            {
                deviceContext.OMSetBlendState(_defaultResources.DefaultBlendState);
            }
            deviceContext.OMSetDepthStencilState(_defaultResources.DepthStencilStateDefault);
        }
    }
}