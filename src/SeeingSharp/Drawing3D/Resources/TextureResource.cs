using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D.Resources
{
    public abstract class TextureResource : Resource
    {
        private D3D11.ID3D11SamplerState? _samplerState;
        private D3D11.SamplerDescription _samplerDescription;
        private bool _samplerDescriptionChanged;

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

        /// <summary>
        /// Filtering method to use when sampling a texture.
        /// See <see cref="SeeingSharpFilter"/>.
        /// </summary>
        public SeeingSharpFilter Filter
        {
            get => (SeeingSharpFilter) _samplerDescription.Filter;
            set
            {
                _samplerDescription.Filter = (D3D11.Filter) value;
                _samplerDescriptionChanged = true;
            }
        }

        /// <summary>
        /// Method to use for resolving a u texture coordinate that is outside the 0 to 1 range.
        /// See <see cref="SeeingSharpTextureAddressMode"/>
        /// </summary>
        public SeeingSharpTextureAddressMode AddressU
        {
            get => (SeeingSharpTextureAddressMode)_samplerDescription.AddressU;
            set
            {
                _samplerDescription.AddressU = (Vortice.Direct3D11.TextureAddressMode) value;
                _samplerDescriptionChanged = true;
            }
        }

        /// <summary>
        /// Method to use for resolving a v texture coordinate that is outside the 0 to 1 range.
        /// See <see cref="SeeingSharpTextureAddressMode"/>
        /// </summary>
        public SeeingSharpTextureAddressMode AddressV
        {
            get => (SeeingSharpTextureAddressMode)_samplerDescription.AddressV;
            set
            {
                _samplerDescription.AddressV = (Vortice.Direct3D11.TextureAddressMode) value;
                _samplerDescriptionChanged = true;
            }
        }

        /// <summary>
        /// Method to use for resolving a w texture coordinate that is outside the 0 to 1 range.
        /// See <see cref="SeeingSharpTextureAddressMode"/>
        /// </summary>
        public SeeingSharpTextureAddressMode AddressW
        {
            get => (SeeingSharpTextureAddressMode)_samplerDescription.AddressW;
            set
            {
                _samplerDescription.AddressW = (Vortice.Direct3D11.TextureAddressMode) value;
                _samplerDescriptionChanged = true;
            }
        }

        /// <summary>
        /// Border color to use if 'Border' address mode is specified for AddressU, AddressV, or AddressW.
        /// </summary>
        public Color4 BorderColor
        {
            get => MathConverter.Color4FromRaw(_samplerDescription.BorderColor);
            set
            {
                _samplerDescription.BorderColor = MathConverter.RawFromColor4(value);
                _samplerDescriptionChanged = true;
            }
        }

        /// <summary>
        /// A function that compares sampled data against existing sampled data.
        /// The function options are listed in <see cref="SeeingSharpComparisonFunction"/>.
        /// </summary>
        public SeeingSharpComparisonFunction ComparisonFunction
        {
            get => (SeeingSharpComparisonFunction) _samplerDescription.ComparisonFunction;
            set
            {
                _samplerDescription.ComparisonFunction = (D3D11.ComparisonFunction) value;
                _samplerDescriptionChanged = true;
            }
        }

        /// <summary>
        /// Clamping value used if D3D11_FILTER_ANISOTROPIC or D3D11_FILTER_COMPARISON_ANISOTROPIC is
        /// specified in Filter. Valid values are between 1 and 16.
        /// </summary>
        public int MaxAnisotropy
        {
            get => _samplerDescription.MaxAnisotropy;
            set
            {
                if (value is < 1 or > 16)
                {
                    throw new SeeingSharpGraphicsException(
                        $"Wrong value for {nameof(this.MaxAnisotropy)}. Valid are values between 1 and 16: Given: {value}");
                }

                _samplerDescription.MaxAnisotropy = value;
                _samplerDescriptionChanged = true;
            }
        }

        /// <summary>
        /// Upper end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any
        /// level higher than that is less detailed. This value must be greater than or equal to MinLOD.
        /// </summary>
        public float MaxLOD
        {
            get => _samplerDescription.MaxLOD;
            set
            {
                _samplerDescription.MaxLOD = value;
                _samplerDescriptionChanged = true;
            }
        }

        /// <summary>
        /// Lower end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level
        /// and any level higher than that is less detailed.
        /// </summary>
        public float MinLOD
        {
            get => _samplerDescription.MinLOD;
            set
            {
                _samplerDescription.MinLOD = value;
                _samplerDescriptionChanged = true;
            }
        }

        /// <summary>
        /// Offset from the calculated mipmap level. For example, if Direct3D calculates that a texture should be sampled at
        /// mipmap level 3 and MipLODBias is 2, then the texture will be sampled at mipmap level 5.
        /// </summary>
        public float MipLODBias
        {
            get => _samplerDescription.MipLODBias;
            set
            {
                _samplerDescription.MipLODBias = value;
                _samplerDescriptionChanged = true;
            }
        }

        internal void ApplySamplerOnPixelShader(EngineDevice device, D3D11.ID3D11DeviceContext deviceContext, int slot)
        {
            if (_samplerDescriptionChanged ||
                (_samplerState == null))
            {
                SeeingSharpUtil.SafeDispose(ref _samplerState);
                _samplerState = device.DeviceD3D11_1.CreateSamplerState(_samplerDescription);
            }

            deviceContext.PSSetSampler(slot, _samplerState!);
        }

        protected TextureResource()
        {
            _samplerDescription = D3D11.SamplerDescription.Default;
            _samplerDescription.AddressU = D3D11.TextureAddressMode.Wrap;
            _samplerDescription.AddressV = D3D11.TextureAddressMode.Wrap;
            _samplerDescription.Filter = D3D11.Filter.Anisotropic;
            _samplerDescription.MaxAnisotropy = 8;
        }

        /// <inheritdoc />
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            SeeingSharpUtil.SafeDispose(ref _samplerState);
        }
    }
}
