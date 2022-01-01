using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D.Resources
{
    public class SpriteMaterialResource : MaterialResource
    {
        // Static Resource keys
        private static readonly NamedOrGenericKey s_resKeyVertexShader = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey s_resKeyPixelShader = GraphicsCore.GetNextGenericResourceKey();

        // Resource members
        private TextureResource _textureResource;
        private DefaultResources _defaultResources;
        private PixelShaderResource _pixelShader;
        private VertexShaderResource _vertexShader;

        /// <summary>
        /// Gets the key of the texture resource.
        /// </summary>
        public NamedOrGenericKey TextureKey { get; }

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => _vertexShader != null;

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
            _vertexShader = resources.GetResourceAndEnsureLoaded(
                s_resKeyVertexShader,
                () => GraphicsHelper.Internals.GetVertexShaderResource(device, "Sprite", "SpriteVertexShader"));
            _pixelShader = resources.GetResourceAndEnsureLoaded(
                s_resKeyPixelShader,
                () => GraphicsHelper.Internals.GetPixelShaderResource(device, "Sprite", "SpritePixelShader"));

            // Get a reference to default resource object
            _defaultResources = resources.GetResourceAndEnsureLoaded<DefaultResources>(DefaultResources.RESOURCE_KEY);

            //Load the texture if any configured.
            if (!this.TextureKey.IsEmpty)
            {
                //Get texture resource
                _textureResource = resources.GetResourceAndEnsureLoaded<TextureResource>(this.TextureKey);
            }
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _defaultResources = null;
            _vertexShader = null;
            _pixelShader = null;
            _textureResource = null;
        }

        /// <summary>
        /// Generates the requested input layout.
        /// </summary>
        /// <param name="device">The device on which to create the input layout.</param>
        /// <param name="inputElements">An array of InputElements describing vertex input structure.</param>
        internal override D3D11.ID3D11InputLayout GetInputLayout(EngineDevice device, D3D11.InputElementDescription[] inputElements)
        {
            return _vertexShader.GetInputLayout(device, inputElements);
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
                deviceContext.PSSetSampler(0, _defaultResources.GetSamplerState(TextureSamplerQualityLevel.Low));

                deviceContext.VSSetShader(_vertexShader.VertexShader);
                deviceContext.PSSetShader(_pixelShader.PixelShader);
            }

            // Set texture resource (if set)
            if (_textureResource != null)
            {
                deviceContext.PSSetShaderResource(0, _textureResource.TextureView);
            }
            else
            {
                deviceContext.PSSetShaderResource(0, null);
            }
        }
    }
}