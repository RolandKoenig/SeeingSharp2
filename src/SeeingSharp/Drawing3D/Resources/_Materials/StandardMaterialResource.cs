using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D.Resources
{
    public class StandardMaterialResource : MaterialResource
    {
        // Resource keys
        private static readonly NamedOrGenericKey s_resKeyVertexShader = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey s_resKeyPixelShader = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey s_resKeyPixelShaderOrtho = GraphicsCore.GetNextGenericResourceKey();
        private readonly NamedOrGenericKey _keyConstantBuffer = GraphicsCore.GetNextGenericResourceKey();

        // Some configuration
        private Color4 _materialDiffuseColor;
        private float _clipFactor;
        private float _maxClipDistance;
        private float _addToAlpha;
        private bool _adjustTextureCoordinates;
        private bool _cbPerMaterialDataChanged;
        private bool _useVertexColors;
        private float _borderPart;
        private float _borderMultiplier;

        // Resource members
        private TextureResource? _textureResource;
        private VertexShaderResource? _vertexShader;
        private PixelShaderResource? _pixelShader;
        private PixelShaderResource? _pixelShaderOrtho;
        private TypeSafeConstantBufferResource<CBPerMaterial>? _cbPerMaterial;
        private DefaultResources? _defaultResources;

        /// <summary>
        /// Gets the key of the texture resource.
        /// </summary>
        public NamedOrGenericKey TextureKey { get; }

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => _vertexShader != null;

        /// <summary>
        /// Gets or sets the ClipFactor.
        /// Pixel are clipped up to an alpha value defined by this Clipfactor within the pixel shader.
        /// </summary>
        public float ClipFactor
        {
            get => _clipFactor;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_clipFactor, value))
                {
                    _clipFactor = value;
                    _cbPerMaterialDataChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum distance on which to apply pixel clipping (defined by ClipFactor property).
        /// </summary>
        public float MaxClipDistance
        {
            get => _maxClipDistance;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_maxClipDistance, value))
                {
                    _maxClipDistance = value;
                    _cbPerMaterialDataChanged = true;
                }
            }
        }

        /// <summary>
        /// Interpolate texture coordinate based on xy-scaling.
        /// </summary>
        public bool AdjustTextureCoordinates
        {
            get => _adjustTextureCoordinates;
            set
            {
                if (_adjustTextureCoordinates != value)
                {
                    _adjustTextureCoordinates = value;
                    _cbPerMaterialDataChanged = true;
                }
            }
        }

        /// <summary>
        /// Needed for video rendering (Frames from the MF SourceReader have alpha always to zero).
        /// </summary>
        public float AddToAlpha
        {
            get => _addToAlpha;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_addToAlpha, value))
                {
                    _addToAlpha = value;
                    _cbPerMaterialDataChanged = true;
                }
            }
        }

        public Color4 MaterialDiffuseColor
        {
            get => _materialDiffuseColor;
            set
            {
                if (_materialDiffuseColor != value)
                {
                    _materialDiffuseColor = value;
                    _cbPerMaterialDataChanged = true;
                }
            }
        }

        public bool UseVertexColors
        {
            get => _useVertexColors;
            set
            {
                if (_useVertexColors != value)
                {
                    _useVertexColors = value;
                    _cbPerMaterialDataChanged = true;
                }
            }
        }

        public float BorderPart
        {
            get => _borderPart;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_borderPart, value))
                {
                    _borderPart = value;
                    _cbPerMaterialDataChanged = true;
                }
            }
        }

        public float BorderMultiplier
        {
            get => _borderMultiplier;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_borderMultiplier, value))
                {
                    _borderMultiplier = value;
                    _cbPerMaterialDataChanged = true;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardMaterialResource"/> class.
        /// </summary>
        /// <param name="textureKey">The name of the texture to be rendered.</param>
        /// <param name="enableShaderGeneratedBorder">Enable drawing of borders which are generated by the pixel shader?</param>
        public StandardMaterialResource(NamedOrGenericKey textureKey = new NamedOrGenericKey(), bool enableShaderGeneratedBorder = false)
        {
            this.TextureKey = textureKey;
            _adjustTextureCoordinates = false;
            _addToAlpha = 0f;
            _materialDiffuseColor = Color4.White;
            _useVertexColors = true;
            _clipFactor = 0.1f;
            _maxClipDistance = float.MaxValue;

            if(enableShaderGeneratedBorder){ this.EnableShaderGeneratedBorder(); }
            else{ this.DisableShaderGeneratedBorder(); }
        }

        /// <summary>
        /// Enables a shader generated border.
        /// </summary>
        public void EnableShaderGeneratedBorder(float borderThickness = 1f)
        {
            this.BorderMultiplier = 50f;
            this.BorderPart = 0.01f * borderThickness;
        }

        /// <summary>
        /// Disables shader generated border.
        /// </summary>
        public void DisableShaderGeneratedBorder()
        {
            this.BorderMultiplier = 0f;
            this.BorderPart = 0f;
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            // Load all required shaders and constant buffers
            _vertexShader = resources.GetResourceAndEnsureLoaded(
                s_resKeyVertexShader,
                () => GraphicsHelper.Internals.GetVertexShaderResource(device, "Common", "CommonVertexShader"));
            _pixelShader = resources.GetResourceAndEnsureLoaded(
                s_resKeyPixelShader,
                () => GraphicsHelper.Internals.GetPixelShaderResource(device, "Common", "CommonPixelShader"));
            _pixelShaderOrtho = resources.GetResourceAndEnsureLoaded(
                s_resKeyPixelShaderOrtho,
                () => GraphicsHelper.Internals.GetPixelShaderResource(device, "Common", "CommonPixelShader.Ortho"));
            _cbPerMaterial = resources.GetResourceAndEnsureLoaded(
                _keyConstantBuffer,
                () => new TypeSafeConstantBufferResource<CBPerMaterial>());
            _cbPerMaterialDataChanged = true;

            // Get a reference to default resource object
            _defaultResources = resources.GetResourceAndEnsureLoaded<DefaultResources>(DefaultResources.RESOURCE_KEY);

            //Load the texture if any configured.
            if (!this.TextureKey.IsEmpty)
            {
                // Get texture resource
                _textureResource = resources.GetResourceAndEnsureLoaded<TextureResource>(this.TextureKey);
            }
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _vertexShader = null;
            _pixelShader = null;
            _pixelShaderOrtho = null;
            _textureResource = null;
            _cbPerMaterial = null;
        }

        /// <summary>
        /// Generates the requested input layout.
        /// </summary>
        /// <param name="device">The device on which to create the input layout.</param>
        /// <param name="inputElements">An array of InputElements describing vertex input structure.</param>
        internal override D3D11.ID3D11InputLayout GetInputLayout(EngineDevice device, D3D11.InputElementDescription[] inputElements)
        {
            return _vertexShader!.GetInputLayout(device, inputElements);
        }

        /// <summary>
        /// Applies the material to the given render state.
        /// </summary>
        /// <param name="renderState">Current render state</param>
        /// <param name="previousMaterial">The previously applied material.</param>
        internal override void Apply(RenderState renderState, MaterialResource? previousMaterial)
        {
            var device = renderState.Device;
            var deviceContext = device.DeviceImmediateContextD3D11;
            var isResourceSameType =
                previousMaterial != null &&
                previousMaterial.ResourceType == this.ResourceType;

            // Apply local shader configuration
            if (_cbPerMaterialDataChanged)
            {
                _cbPerMaterial!.SetData(
                    deviceContext,
                    new CBPerMaterial
                    {
                        ClipFactor = _clipFactor,
                        MaxClipDistance = _maxClipDistance,
                        Texture0Factor = _textureResource != null ? 1f : 0f,
                        AdjustTextureCoordinates = _adjustTextureCoordinates ? 1f : 0f,
                        AddToAlpha = _addToAlpha,
                        MaterialDiffuseColor = _materialDiffuseColor,
                        DiffuseColorFactor = _useVertexColors ? 0f : 1f,
                        BorderPart = _borderPart,
                        BorderMultiplier = _borderMultiplier
                    });
                _cbPerMaterialDataChanged = false;
            }

            // Set shaders, sampler and constants
            if (!isResourceSameType)
            {
                deviceContext.VSSetShader(_vertexShader!.VertexShader);
                if (renderState.Camera!.IsOrthopraphicInternal)
                {
                    deviceContext.PSSetShader(_pixelShaderOrtho!.PixelShader);
                }
                else
                {
                    deviceContext.PSSetShader(_pixelShader!.PixelShader);
                }
            }
            deviceContext.PSSetConstantBuffer(3, _cbPerMaterial!.ConstantBuffer);
            deviceContext.VSSetConstantBuffer(3, _cbPerMaterial!.ConstantBuffer);

            // Set texture resource (if set)
            if (_textureResource != null &&
                renderState.ViewInformation!.ViewConfiguration.ShowTexturesInternal)
            {
                _textureResource.ApplySamplerOnPixelShader(device, deviceContext, 0);
                deviceContext.PSSetShaderResource(0, _textureResource.TextureView);
            }
            else
            {
                deviceContext.PSSetSampler(0, null!);
                deviceContext.PSSetShaderResource(0, null!);
            }
        }
    }
}
