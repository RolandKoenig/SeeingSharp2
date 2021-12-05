using System;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D
{
    public class DefaultResources : Resource
    {
        public static readonly NamedOrGenericKey RESOURCE_KEY = new NamedOrGenericKey(typeof(DefaultResources));

        // Blend states
        private Lazy<D3D11.ID3D11BlendState> _defaultBlendState;
        private Lazy<D3D11.ID3D11BlendState> _alphaBlendingBlendState;

        // Depth stencil states
        private Lazy<D3D11.ID3D11DepthStencilState> _depthStencilStateDefault;
        private Lazy<D3D11.ID3D11DepthStencilState> _depthStencilStateDisableZWrites;
        private Lazy<D3D11.ID3D11DepthStencilState> _depthStencilStateInvertedZTest;
        private Lazy<D3D11.ID3D11DepthStencilState> _depthStencilStateAlwaysPass;

        // Rasterizer states
        private Lazy<D3D11.ID3D11RasterizerState> _rasterStateLines;
        private Lazy<D3D11.ID3D11RasterizerState> _rasterStateDefault;
        private Lazy<D3D11.ID3D11RasterizerState> _rasterStateBiased;
        private Lazy<D3D11.ID3D11RasterizerState> _rasterStateWireframe;

        // Sample states
        private Lazy<D3D11.ID3D11SamplerState> _samplerStateLow;
        private Lazy<D3D11.ID3D11SamplerState> _samplerStateMedium;
        private Lazy<D3D11.ID3D11SamplerState> _samplerStateHigh;

        /// <summary>
        /// Are resources loaded?
        /// </summary>
        public bool ResourcesLoaded => _defaultBlendState != null;

        public override bool IsLoaded => _defaultBlendState != null;

        internal D3D11.ID3D11BlendState DefaultBlendState => _defaultBlendState?.Value;

        internal D3D11.ID3D11BlendState AlphaBlendingBlendState => _alphaBlendingBlendState?.Value;

        internal D3D11.ID3D11DepthStencilState DepthStencilStateDefault => _depthStencilStateDefault?.Value;

        internal D3D11.ID3D11DepthStencilState DepthStencilStateDisableZWrites => _depthStencilStateDisableZWrites?.Value;

        internal D3D11.ID3D11DepthStencilState DepthStencilStateAlwaysPassDepth => _depthStencilStateAlwaysPass?.Value;

        internal D3D11.ID3D11DepthStencilState DepthStencilStateInvertedZTest => _depthStencilStateInvertedZTest?.Value;

        internal D3D11.ID3D11RasterizerState RasterStateDefault => _rasterStateDefault?.Value;

        internal D3D11.ID3D11RasterizerState RasterStateBiased => _rasterStateBiased?.Value;

        internal D3D11.ID3D11RasterizerState RasterStateWireframe => _rasterStateWireframe?.Value;

        internal D3D11.ID3D11RasterizerState RasterStateLines => _rasterStateLines?.Value;

        internal D3D11.ID3D11SamplerState SamplerStateDefault => _samplerStateMedium?.Value;

        /// <summary>
        /// Loads the resource.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            // Create default blend state
            _defaultBlendState = new Lazy<D3D11.ID3D11BlendState>(() =>
            {
                var blendDesc = D3D11.BlendDescription.Default;
                return device.DeviceD3D11_1.CreateBlendState(blendDesc);
            });

            // Create alpha blending blend state
            _alphaBlendingBlendState = new Lazy<D3D11.ID3D11BlendState>(() =>
            {
                //Define the blend state (based on http://www.rastertek.com/dx11tut26.html)
                var blendDesc = D3D11.BlendDescription.Default;
                blendDesc.RenderTarget[0].IsBlendEnabled = true;
                blendDesc.RenderTarget[0].SourceBlend = D3D11.Blend.SourceAlpha;
                blendDesc.RenderTarget[0].DestinationBlend = D3D11.Blend.InverseSourceAlpha;
                blendDesc.RenderTarget[0].BlendOperation = D3D11.BlendOperation.Add;
                blendDesc.RenderTarget[0].DestinationBlendAlpha = D3D11.Blend.One;
                blendDesc.RenderTarget[0].SourceBlendAlpha = D3D11.Blend.One;
                blendDesc.RenderTarget[0].BlendOperationAlpha = D3D11.BlendOperation.Max;
                blendDesc.RenderTarget[0].RenderTargetWriteMask = D3D11.ColorWriteEnable.All;

                //Create the BlendState object
                return device.DeviceD3D11_1.CreateBlendState(blendDesc);
            });

            // Create default depth stencil state
            _depthStencilStateDefault = new Lazy<D3D11.ID3D11DepthStencilState>(() =>
            {
                var stateDesc = D3D11.DepthStencilDescription.Default;
                stateDesc.DepthFunc = D3D11.ComparisonFunction.LessEqual;
                return device.DeviceD3D11_1.CreateDepthStencilState(stateDesc);
            });

            // Create the depth stencil state for disabling z writes
            _depthStencilStateDisableZWrites = new Lazy<D3D11.ID3D11DepthStencilState>(() =>
            {
                var stateDesc = D3D11.DepthStencilDescription.Default;
                stateDesc.DepthWriteMask = D3D11.DepthWriteMask.Zero;
                stateDesc.DepthFunc = D3D11.ComparisonFunction.LessEqual;
                return device.DeviceD3D11_1.CreateDepthStencilState(stateDesc);
            });

            _depthStencilStateAlwaysPass = new Lazy<D3D11.ID3D11DepthStencilState>(() =>
            {
                var stateDesc = D3D11.DepthStencilDescription.Default;
                stateDesc.DepthWriteMask = D3D11.DepthWriteMask.Zero;
                stateDesc.DepthFunc = D3D11.ComparisonFunction.Always;
                stateDesc.DepthEnable = false;
                return device.DeviceD3D11_1.CreateDepthStencilState(stateDesc);
            });

            // Create the depth stencil state for inverting z logic
            _depthStencilStateInvertedZTest = new Lazy<D3D11.ID3D11DepthStencilState>(() =>
            {
                var stateDesc = D3D11.DepthStencilDescription.Default;
                stateDesc.DepthFunc = D3D11.ComparisonFunction.Greater;
                stateDesc.DepthWriteMask = D3D11.DepthWriteMask.Zero;
                return device.DeviceD3D11_1.CreateDepthStencilState(stateDesc);
            });

            // Create default rasterizer state
            _rasterStateDefault = new Lazy<D3D11.ID3D11RasterizerState>(
                () =>
                {
                    var stateDesc = new D3D11.RasterizerDescription();
                    stateDesc.AntialiasedLineEnable = true;
                    stateDesc.MultisampleEnable = true;
                    stateDesc.FillMode = D3D11.FillMode.Solid;
                    stateDesc.DepthBias = D3D11.RasterizerDescription.DefaultDepthBias;
                    stateDesc.DepthBiasClamp = D3D11.RasterizerDescription.DefaultDepthBiasClamp;
                    stateDesc.SlopeScaledDepthBias = D3D11.RasterizerDescription.DefaultSlopeScaledDepthBias;
                    return device.DeviceD3D11_1.CreateRasterizerState(stateDesc);
                });

            // Create a raster state with depth bias
            _rasterStateBiased = new Lazy<D3D11.ID3D11RasterizerState>(() =>
            {
                var rasterDesc = new D3D11.RasterizerDescription();
                rasterDesc.DepthBias = GraphicsHelper.Internals.GetDepthBiasValue(device, -0.00003f);
                rasterDesc.DepthBiasClamp = D3D11.RasterizerDescription.DefaultDepthBiasClamp;
                rasterDesc.SlopeScaledDepthBias = D3D11.RasterizerDescription.DefaultSlopeScaledDepthBias;
                return device.DeviceD3D11_1.CreateRasterizerState(rasterDesc);
            });

            // Create a raster state for wireframe rendering
            _rasterStateWireframe = new Lazy<D3D11.ID3D11RasterizerState>(() =>
            {
                var rasterDesc = new D3D11.RasterizerDescription();
                rasterDesc.FillMode = D3D11.FillMode.Wireframe;
                rasterDesc.AntialiasedLineEnable = true;
                rasterDesc.MultisampleEnable = true;
                rasterDesc.DepthBias = D3D11.RasterizerDescription.DefaultDepthBias;
                rasterDesc.DepthBiasClamp = D3D11.RasterizerDescription.DefaultDepthBiasClamp;
                rasterDesc.SlopeScaledDepthBias = D3D11.RasterizerDescription.DefaultSlopeScaledDepthBias;
                return device.DeviceD3D11_1.CreateRasterizerState(rasterDesc);
            });

            // Create the rasterizer state for line rendering
            _rasterStateLines = new Lazy<D3D11.ID3D11RasterizerState>(() =>
            {
                var rasterDesc = new D3D11.RasterizerDescription();
                rasterDesc.CullMode = D3D11.CullMode.None;
                rasterDesc.AntialiasedLineEnable = true;
                rasterDesc.MultisampleEnable = true;
                rasterDesc.FillMode = D3D11.FillMode.Solid;
                rasterDesc.DepthBias = D3D11.RasterizerDescription.DefaultDepthBias;
                rasterDesc.DepthBiasClamp = D3D11.RasterizerDescription.DefaultDepthBiasClamp;
                rasterDesc.SlopeScaledDepthBias = D3D11.RasterizerDescription.DefaultSlopeScaledDepthBias;
                return device.DeviceD3D11_1.CreateRasterizerState(rasterDesc);
            });

            // Create sampler states
            _samplerStateLow = new Lazy<D3D11.ID3D11SamplerState>(
                () => GraphicsHelper.Internals.CreateDefaultTextureSampler(device, TextureSamplerQualityLevel.Low));
            _samplerStateMedium = new Lazy<D3D11.ID3D11SamplerState>(
                () => GraphicsHelper.Internals.CreateDefaultTextureSampler(device, TextureSamplerQualityLevel.Medium));
            _samplerStateHigh = new Lazy<D3D11.ID3D11SamplerState>(
                () => GraphicsHelper.Internals.CreateDefaultTextureSampler(device, TextureSamplerQualityLevel.High));
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _defaultBlendState = SeeingSharpUtil.DisposeObjectLazy(_defaultBlendState);
            _depthStencilStateDefault = SeeingSharpUtil.DisposeObjectLazy(_depthStencilStateDefault);
            _depthStencilStateDisableZWrites = SeeingSharpUtil.DisposeObjectLazy(_depthStencilStateDisableZWrites);
            _rasterStateLines = SeeingSharpUtil.DisposeObjectLazy(_rasterStateLines);
            _rasterStateDefault = SeeingSharpUtil.DisposeObjectLazy(_rasterStateDefault);
            _samplerStateLow = SeeingSharpUtil.DisposeObjectLazy(_samplerStateLow);
            _samplerStateMedium = SeeingSharpUtil.DisposeObjectLazy(_samplerStateMedium);
            _samplerStateHigh = SeeingSharpUtil.DisposeObjectLazy(_samplerStateHigh);
        }

        /// <summary>
        /// Gets the sampler state with the given requested quality level.
        /// </summary>
        /// <param name="qualityLevel">The quality level to get the sampler state for.</param>
        internal D3D11.ID3D11SamplerState GetSamplerState(TextureSamplerQualityLevel qualityLevel)
        {
            switch (qualityLevel)
            {
                case TextureSamplerQualityLevel.High:
                    return _samplerStateHigh.Value;

                case TextureSamplerQualityLevel.Medium:
                    return _samplerStateMedium.Value;

                case TextureSamplerQualityLevel.Low:
                    return _samplerStateLow.Value;
            }

            return _samplerStateLow.Value;
        }
    }
}
