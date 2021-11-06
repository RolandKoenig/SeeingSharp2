using System;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Drawing3D
{
    public class DefaultResources : Resource
    {
        public static readonly NamedOrGenericKey RESOURCE_KEY = new NamedOrGenericKey(typeof(DefaultResources));

        // Blend states
        private Lazy<D3D11.BlendState> _defaultBlendState;
        private Lazy<D3D11.BlendState> _alphaBlendingBlendState;

        // Depth stencil states
        private Lazy<D3D11.DepthStencilState> _depthStencilStateDefault;
        private Lazy<D3D11.DepthStencilState> _depthStencilStateDisableZWrites;
        private Lazy<D3D11.DepthStencilState> _depthStencilStateInvertedZTest;
        private Lazy<D3D11.DepthStencilState> _depthStencilStateAlwaysPass;

        // Rasterizer states
        private Lazy<D3D11.RasterizerState> _rasterStateLines;
        private Lazy<D3D11.RasterizerState> _rasterStateDefault;
        private Lazy<D3D11.RasterizerState> _rasterStateBiased;
        private Lazy<D3D11.RasterizerState> _rasterStateWireframe;

        // Sample states
        private Lazy<D3D11.SamplerState> _samplerStateLow;
        private Lazy<D3D11.SamplerState> _samplerStateMedium;
        private Lazy<D3D11.SamplerState> _samplerStateHigh;

        /// <summary>
        /// Are resources loaded?
        /// </summary>
        public bool ResourcesLoaded => _defaultBlendState != null;

        public override bool IsLoaded => _defaultBlendState != null;

        internal D3D11.BlendState DefaultBlendState => _defaultBlendState?.Value;

        internal D3D11.BlendState AlphaBlendingBlendState => _alphaBlendingBlendState?.Value;

        internal D3D11.DepthStencilState DepthStencilStateDefault => _depthStencilStateDefault?.Value;

        internal D3D11.DepthStencilState DepthStencilStateDisableZWrites => _depthStencilStateDisableZWrites?.Value;

        internal D3D11.DepthStencilState DepthStencilStateAlwaysPassDepth => _depthStencilStateAlwaysPass?.Value;

        internal D3D11.DepthStencilState DepthStencilStateInvertedZTest => _depthStencilStateInvertedZTest?.Value;

        internal D3D11.RasterizerState RasterStateDefault => _rasterStateDefault?.Value;

        internal D3D11.RasterizerState RasterStateBiased => _rasterStateBiased?.Value;

        internal D3D11.RasterizerState RasterStateWireframe => _rasterStateWireframe?.Value;

        internal D3D11.RasterizerState RasterStateLines => _rasterStateLines?.Value;

        internal D3D11.SamplerState SamplerStateDefault => _samplerStateMedium?.Value;

        /// <summary>
        /// Loads the resource.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            // Create default blend state
            _defaultBlendState = new Lazy<D3D11.BlendState>(() =>
            {
                var blendDesc = D3D11.BlendStateDescription.Default();
                return new D3D11.BlendState(device.DeviceD3D11_1, blendDesc);
            });

            // Create alpha blending blend state
            _alphaBlendingBlendState = new Lazy<D3D11.BlendState>(() =>
            {
                //Define the blend state (based on http://www.rastertek.com/dx11tut26.html)
                var blendDesc = D3D11.BlendStateDescription.Default();
                blendDesc.RenderTarget[0].IsBlendEnabled = true;
                blendDesc.RenderTarget[0].SourceBlend = D3D11.BlendOption.SourceAlpha;
                blendDesc.RenderTarget[0].DestinationBlend = D3D11.BlendOption.InverseSourceAlpha;
                blendDesc.RenderTarget[0].BlendOperation = D3D11.BlendOperation.Add;
                blendDesc.RenderTarget[0].DestinationAlphaBlend = D3D11.BlendOption.One;
                blendDesc.RenderTarget[0].SourceAlphaBlend = D3D11.BlendOption.One;
                blendDesc.RenderTarget[0].AlphaBlendOperation = D3D11.BlendOperation.Maximum;
                blendDesc.RenderTarget[0].RenderTargetWriteMask = D3D11.ColorWriteMaskFlags.All;

                //Create the BlendState object
                return new D3D11.BlendState(device.DeviceD3D11_1, blendDesc);
            });

            // Create default depth stencil state
            _depthStencilStateDefault = new Lazy<D3D11.DepthStencilState>(() =>
            {
                var stateDesc = D3D11.DepthStencilStateDescription.Default();
                stateDesc.DepthComparison = D3D11.Comparison.LessEqual;
                return new D3D11.DepthStencilState(device.DeviceD3D11_1, stateDesc);
            });

            // Create the depth stencil state for disabling z writes
            _depthStencilStateDisableZWrites = new Lazy<D3D11.DepthStencilState>(() =>
            {
                var stateDesc = D3D11.DepthStencilStateDescription.Default();
                stateDesc.DepthWriteMask = D3D11.DepthWriteMask.Zero;
                stateDesc.DepthComparison = D3D11.Comparison.LessEqual;
                return new D3D11.DepthStencilState(device.DeviceD3D11_1, stateDesc);
            });

            _depthStencilStateAlwaysPass = new Lazy<D3D11.DepthStencilState>(() =>
            {
                var stateDesc = D3D11.DepthStencilStateDescription.Default();
                stateDesc.DepthWriteMask = D3D11.DepthWriteMask.Zero;
                stateDesc.DepthComparison = D3D11.Comparison.Always;
                stateDesc.IsDepthEnabled = false;
                return new D3D11.DepthStencilState(device.DeviceD3D11_1, stateDesc);
            });

            // Create the depth stencil state for inverting z logic
            _depthStencilStateInvertedZTest = new Lazy<D3D11.DepthStencilState>(() =>
            {
                var stateDesc = D3D11.DepthStencilStateDescription.Default();
                stateDesc.DepthComparison = D3D11.Comparison.Greater;
                stateDesc.DepthWriteMask = D3D11.DepthWriteMask.Zero;
                return new D3D11.DepthStencilState(device.DeviceD3D11_1, stateDesc);
            });

            // Create default rasterizer state
            _rasterStateDefault = new Lazy<D3D11.RasterizerState>(
                () =>
                {
                    var stateDesc = D3D11.RasterizerStateDescription.Default();
                    stateDesc.IsAntialiasedLineEnabled = true;
                    stateDesc.IsMultisampleEnabled = true;
                    stateDesc.FillMode = D3D11.FillMode.Solid;
                    return new D3D11.RasterizerState(device.DeviceD3D11_1, stateDesc);
                });

            // Create a raster state with depth bias
            _rasterStateBiased = new Lazy<D3D11.RasterizerState>(() =>
            {
                var rasterDesc = D3D11.RasterizerStateDescription.Default();
                rasterDesc.DepthBias = GraphicsHelper.Internals.GetDepthBiasValue(device, -0.00003f);
                return new D3D11.RasterizerState(device.DeviceD3D11_1, rasterDesc);
            });

            // Create a raster state for wireframe rendering
            _rasterStateWireframe = new Lazy<D3D11.RasterizerState>(() =>
            {
                var rasterDesc = D3D11.RasterizerStateDescription.Default();
                rasterDesc.FillMode = D3D11.FillMode.Wireframe;
                rasterDesc.IsAntialiasedLineEnabled = true;
                rasterDesc.IsMultisampleEnabled = true;
                return new D3D11.RasterizerState(device.DeviceD3D11_1, rasterDesc);
            });

            // Create the rasterizer state for line rendering
            _rasterStateLines = new Lazy<D3D11.RasterizerState>(() =>
            {
                var stateDesc = D3D11.RasterizerStateDescription.Default();
                stateDesc.CullMode = D3D11.CullMode.None;
                stateDesc.IsAntialiasedLineEnabled = true;
                stateDesc.IsMultisampleEnabled = true;
                stateDesc.FillMode = D3D11.FillMode.Solid;
                return new D3D11.RasterizerState(device.DeviceD3D11_1, stateDesc);
            });

            // Create sampler states
            _samplerStateLow = new Lazy<D3D11.SamplerState>(
                () => GraphicsHelper.Internals.CreateDefaultTextureSampler(device, TextureSamplerQualityLevel.Low));
            _samplerStateMedium = new Lazy<D3D11.SamplerState>(
                () => GraphicsHelper.Internals.CreateDefaultTextureSampler(device, TextureSamplerQualityLevel.Medium));
            _samplerStateHigh = new Lazy<D3D11.SamplerState>(
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
        internal D3D11.SamplerState GetSamplerState(TextureSamplerQualityLevel qualityLevel)
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
