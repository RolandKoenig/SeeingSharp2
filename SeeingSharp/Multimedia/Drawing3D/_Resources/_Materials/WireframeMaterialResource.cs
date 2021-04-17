/*
    SeeingSharp and all applications distributed together with it. 
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
    public class WireframeMaterialResource : MaterialResource
    {
        // Resource keys
        private static readonly NamedOrGenericKey s_resKeyVertexShader = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey s_resKeyPixelShader = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey s_resKeyGeoShader = GraphicsCore.GetNextGenericResourceKey();
        private readonly NamedOrGenericKey _keyConstantBuffer = GraphicsCore.GetNextGenericResourceKey();

        // Some configuration
        private Color4 _materialDiffuseColor;
        private float _clipFactor;
        private float _maxClipDistance;
        private float _addToAlpha;
        private bool _adjustTextureCoordinates;
        private bool _cbPerMaterialDataChanged;
        private bool _useVertexColors;

        // Resource members
        private VertexShaderResource _vertexShader;
        private GeometryShaderResource _geoShader;
        private PixelShaderResource _pixelShader;
        private TypeSafeConstantBufferResource<CBPerMaterial> _cbPerMaterial;

        /// <inheritdoc />
        public override bool IsLoaded => _vertexShader != null;

        /// <summary>
        /// Gets or sets the ClipFactor.
        /// Pixel are clipped up to an alpha value defined by this ClipFactor within the pixel shader.
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

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardMaterialResource"/> class.
        /// </summary>
        public WireframeMaterialResource()
        {
            _maxClipDistance = 1000f;
            _adjustTextureCoordinates = false;
            _addToAlpha = 0f;
            _materialDiffuseColor = Color4.White;
            _useVertexColors = true;
        }

        /// <inheritdoc />
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            // Load all required shaders and constant buffers
            _vertexShader = resources.GetResourceAndEnsureLoaded(
                s_resKeyVertexShader,
                () => GraphicsHelper.Internals.GetVertexShaderResource(device, "Common", "WireframeVertexShader"));
            _geoShader = resources.GetResourceAndEnsureLoaded(
                s_resKeyGeoShader,
                () => GraphicsHelper.Internals.GetGeometryShaderResource(device, "Common", "WireframeGeometryShader"));
            _pixelShader = resources.GetResourceAndEnsureLoaded(
                s_resKeyPixelShader,
                () => GraphicsHelper.Internals.GetPixelShaderResource(device, "Common", "WireframePixelShader"));
            _cbPerMaterial = resources.GetResourceAndEnsureLoaded(
                _keyConstantBuffer,
                () => new TypeSafeConstantBufferResource<CBPerMaterial>());
            _cbPerMaterialDataChanged = true;
        }

        /// <inheritdoc />
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _vertexShader = null;
            _geoShader = null;
            _pixelShader = null;
            _cbPerMaterial = null;
        }

        /// <inheritdoc />
        internal override D3D11.InputLayout GetInputLayout(EngineDevice device, D3D11.InputElement[] inputElements)
        {
            return _vertexShader.GetInputLayout(device, inputElements);
        }

        /// <inheritdoc />
        internal override void Apply(RenderState renderState, MaterialResource previousMaterial)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;
            var isResourceSameType =
                previousMaterial != null &&
                previousMaterial.ResourceType == this.ResourceType;

            // Apply local shader configuration
            if (_cbPerMaterialDataChanged)
            {
                _cbPerMaterial.SetData(
                    deviceContext,
                    new CBPerMaterial
                    {
                        ClipFactor = _clipFactor,
                        MaxClipDistance = _maxClipDistance,
                        Texture0Factor = 0f,
                        AdjustTextureCoordinates = _adjustTextureCoordinates ? 1f : 0f,
                        AddToAlpha = _addToAlpha,
                        MaterialDiffuseColor = _materialDiffuseColor,
                        DiffuseColorFactor = _useVertexColors ? 0f : 1f
                    });
                _cbPerMaterialDataChanged = false;
            }

            // Apply sampler and constants
            deviceContext.PixelShader.SetConstantBuffer(3, _cbPerMaterial.ConstantBuffer);
            deviceContext.VertexShader.SetConstantBuffer(3, _cbPerMaterial.ConstantBuffer);

            // Set texture resource (if set)
            deviceContext.PixelShader.SetShaderResource(0, null);

            // Set shader resources
            if (!isResourceSameType)
            {
                deviceContext.VertexShader.Set(_vertexShader.VertexShader);
                deviceContext.GeometryShader.Set(_geoShader.GeometryShader);
                deviceContext.PixelShader.Set(_pixelShader.PixelShader);
            }
        }

        /// <inheritdoc />
        internal override void Discard(RenderState renderState)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;

            deviceContext.GeometryShader.Set(null);
        }
    }
}
