using System.Numerics;
using System.Runtime.InteropServices;
using SeeingSharp.Checking;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.Drawing3D.Resources
{
    public class FocusPostprocessEffectResource : PostprocessEffectResource
    {
        // Resource keys
        private static readonly NamedOrGenericKey s_resKeyPixelShaderBlur = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey s_keyMaterial = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey s_keyRenderTarget = GraphicsCore.GetNextGenericResourceKey();
        private readonly NamedOrGenericKey _keyCbPass01 = GraphicsCore.GetNextGenericResourceKey();
        private readonly NamedOrGenericKey _keyCbPass02 = GraphicsCore.GetNextGenericResourceKey();

        // Configuration
        private bool _forceSimpleMethod;
        private float _fadeIntensity;

        // Resources
        private SingleForcedColorMaterialResource _singleForcedColor;
        private RenderTargetTextureResource _renderTarget;
        private DefaultResources _defaultResources;
        private PixelShaderResource _pixelShaderBlur;
        private TypeSafeConstantBufferResource<CbPerObject> _cbFirstPass;
        private TypeSafeConstantBufferResource<CbPerObject> _cbSecondPass;

        /// <inheritdoc />
        public override bool IsLoaded =>
            _renderTarget != null &&
            _singleForcedColor != null &&
            _singleForcedColor.IsLoaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="FocusPostprocessEffectResource"/> class.
        /// </summary>
        /// <param name="forceSimpleMethod">Force simple mode. Default to false.</param>
        /// <param name="fadeIntensity">Intensity of the fade effect.</param>
        public FocusPostprocessEffectResource(bool forceSimpleMethod = false, float fadeIntensity = 1f)
        {
            _fadeIntensity.EnsureInRange(0f, 1f, nameof(fadeIntensity));

            _forceSimpleMethod = forceSimpleMethod;
            _fadeIntensity = fadeIntensity;
        }

        /// <inheritdoc />
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            base.LoadResourceInternal(device, resources);

            // Load graphics resources
            _pixelShaderBlur = resources.GetResourceAndEnsureLoaded(
                s_resKeyPixelShaderBlur,
                () => GraphicsHelper.Internals.GetPixelShaderResource(device, "Postprocessing", "PostprocessBlur"));
            _singleForcedColor = resources.GetResourceAndEnsureLoaded(
                s_keyMaterial,
                () => new SingleForcedColorMaterialResource { FadeIntensity = _fadeIntensity });
            _renderTarget = resources.GetResourceAndEnsureLoaded(
                s_keyRenderTarget,
                () => new RenderTargetTextureResource(RenderTargetCreationMode.Color));
            _defaultResources = resources.DefaultResources;

            // Load constant buffers
            _cbFirstPass = resources.GetResourceAndEnsureLoaded(
                _keyCbPass01,
                () => new TypeSafeConstantBufferResource<CbPerObject>(new CbPerObject
                {
                    BlurIntensity = 0.0f,
                    BlurOpacity = 0.1f
                }));
            _cbSecondPass = resources.GetResourceAndEnsureLoaded(
                _keyCbPass02,
                () => new TypeSafeConstantBufferResource<CbPerObject>(new CbPerObject
                {
                    BlurIntensity = 0.8f,
                    BlurOpacity = 0.5f
                }));
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            base.UnloadResourceInternal(device, resources);

            _pixelShaderBlur = null;
            _singleForcedColor = null;
            _renderTarget = null;
            _defaultResources = null;
            _cbFirstPass = null;
            _cbSecondPass = null;
        }

        /// <inheritdoc />
        internal override void NotifyBeforeRender(RenderState renderState, string layerName, int passId)
        {
            if (renderState.Device.IsHighDetailSupported && !_forceSimpleMethod)
            {
                switch (passId)
                {
                    //******************************
                    // 1. Pass: Draw all pixels that ly behind other already rendered elements
                    case 0:
                        // Force the single color material
                        renderState.ForceMaterial(_singleForcedColor);

                        // Apply current render target size an push render target texture on current rendering stack
                        _renderTarget.ApplySize(renderState);
                        _renderTarget.PushOnRenderState(renderState, PushRenderTargetMode.Default_OwnColor_PrevDepthObjectIdNormalDepth);

                        // Clear current render target
                        renderState.ClearCurrentColorBuffer(new Color4(1f, 1f, 1f, 0f));

                        // ConfigureLoading stencil state(invert z logic, disable z writes)
                        renderState.Device.DeviceImmediateContextD3D11.OMSetDepthStencilState(_defaultResources.DepthStencilStateInvertedZTest);
                        break;

                    //******************************
                    // 2. Pass: Draw all visible pixels with some blur effect
                    case 1:
                        // Force the single color material
                        renderState.ForceMaterial(_singleForcedColor);

                        // Push render target texture on current rendering stack again
                        _renderTarget.PushOnRenderState(renderState, PushRenderTargetMode.Default_OwnColor_PrevDepthObjectIdNormalDepth);

                        // Clear current render target
                        renderState.ClearCurrentColorBuffer(new Color4(1f, 1f, 1f, 0f));

                        // Change raster state
                        renderState.Device.DeviceImmediateContextD3D11.RSSetState(_defaultResources.RasterStateBiased);
                        renderState.Device.DeviceImmediateContextD3D11.OMSetDepthStencilState(_defaultResources.DepthStencilStateDisableZWrites);
                        break;
                }
            }
            else
            {
                renderState.ForceMaterial(_singleForcedColor);

                // Change raster state
                renderState.Device.DeviceImmediateContextD3D11.RSSetState(_defaultResources.RasterStateBiased);
            }
        }

        /// <inheritdoc />
        internal override void NotifyAfterRenderPlain(RenderState renderState, string layerName, int passId)
        {
            // Nothing to be done here
        }

        /// <inheritdoc />
        internal override bool NotifyAfterRender(RenderState renderState, string layerName, int passId)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;

            if (renderState.Device.IsHighDetailSupported && !_forceSimpleMethod)
            {
                // Reset settings made on render state (needed for all passes)
                deviceContext.RSSetState(_defaultResources.RasterStateDefault);
                deviceContext.OMSetDepthStencilState(_defaultResources.DepthStencilStateDefault);
                renderState.ForceMaterial(null);

                renderState.DumpCurrentRenderTargetsIfActivated(layerName, passId, "FocusAfterRender");

                // Reset render target (needed for all passes)
                _renderTarget.PopFromRenderState(renderState);

                // Clear cached material resource because we work with shaders directly here
                renderState.ClearCachedAppliedMaterial();

                // Render result of current pass to the main render target
                switch (passId)
                {
                    case 0:
                        this.ApplyAlphaBasedSpriteRendering(deviceContext);
                        try
                        {
                            deviceContext.PSSetShaderResource(0, _renderTarget.TextureView);
                            deviceContext.PSSetSampler(0, _defaultResources.GetSamplerState(TextureSamplerQualityLevel.Low));
                            deviceContext.PSSetConstantBuffer(2, _cbFirstPass.ConstantBuffer);
                            deviceContext.PSSetShader(_pixelShaderBlur.PixelShader);
                            deviceContext.Draw(3, 0);
                        }
                        finally
                        {
                            this.DiscardAlphaBasedSpriteRendering(deviceContext);
                        }
                        return true;

                    case 1:
                        this.ApplyAlphaBasedSpriteRendering(deviceContext);
                        try
                        {
                            deviceContext.PSSetShaderResource(0, _renderTarget.TextureView);
                            deviceContext.PSSetSampler(0, _defaultResources.GetSamplerState(TextureSamplerQualityLevel.Low));
                            deviceContext.PSSetConstantBuffer(2, _cbSecondPass.ConstantBuffer);
                            deviceContext.PSSetShader(_pixelShaderBlur.PixelShader);
                            deviceContext.Draw(3, 0);

                        }
                        finally
                        {
                            this.DiscardAlphaBasedSpriteRendering(deviceContext);
                        }
                        return false;
                }
            }
            else
            {
                // Reset changes from before
                deviceContext.RSSetState(_defaultResources.RasterStateDefault);
                renderState.ForceMaterial(null);

                // Now we ware finished
                return false;
            }

            return false;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        [StructLayout(LayoutKind.Sequential)]
        private struct CbPerObject
        {
            public float BlurIntensity;

            public float BlurOpacity;

            /// <summary>
            /// A dummy field to ensure a 4-based size of this structure.
            /// </summary>
            public Vector2 Dummy;
        }
    }
}
