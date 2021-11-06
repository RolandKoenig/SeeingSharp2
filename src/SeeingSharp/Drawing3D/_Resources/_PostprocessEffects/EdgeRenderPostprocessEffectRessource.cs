using System.Numerics;
using System.Runtime.InteropServices;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.Drawing3D
{
    public class EdgeDetectPostprocessEffectResource : PostprocessEffectResource
    {
        // Static resource keys
        private static readonly NamedOrGenericKey s_resKeyPixelShaderBlur = GraphicsCore.GetNextGenericResourceKey();

        // Instance resource keys
        private readonly NamedOrGenericKey _keyRenderTarget = GraphicsCore.GetNextGenericResourceKey();
        private readonly NamedOrGenericKey _keyConstantBuffer = GraphicsCore.GetNextGenericResourceKey();

        // Configuration
        private Color4 _borderColor;

        // Resources
        private RenderTargetTextureResource _renderTarget;
        private DefaultResources _defaultResources;
        private PixelShaderResource _pixelShaderBlur;
        private CBPerObject _constantBufferData;
        private TypeSafeConstantBufferResource<CBPerObject> _constantBuffer;

        /// <inheritdoc />
        public override bool IsLoaded =>
            _renderTarget != null &&
            _renderTarget.IsLoaded;

        public float Thickness { get; set; }

        public Color4 BorderColor
        {
            get => _borderColor;
            set => _borderColor = value;
        }

        public bool DrawOriginalObject { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FocusPostprocessEffectResource"/> class.
        /// </summary>
        public EdgeDetectPostprocessEffectResource()
        {
            this.Thickness = 2f;
            _borderColor = Color4.BlueColor;
            this.DrawOriginalObject = true;
        }

        /// <inheritdoc />
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            base.LoadResourceInternal(device, resources);

            // Load graphics resources
            _pixelShaderBlur = resources.GetResourceAndEnsureLoaded(
                s_resKeyPixelShaderBlur,
                () => GraphicsHelper.Internals.GetPixelShaderResource(device, "Postprocessing", "PostprocessEdgeDetect"));
            _renderTarget = resources.GetResourceAndEnsureLoaded(
                _keyRenderTarget,
                () => new RenderTargetTextureResource(RenderTargetCreationMode.Color));
            _defaultResources = resources.DefaultResources;

            // Load constant buffer
            _constantBufferData = new CBPerObject();
            _constantBuffer = resources.GetResourceAndEnsureLoaded(
                _keyConstantBuffer,
                () => new TypeSafeConstantBufferResource<CBPerObject>(_constantBufferData));
        }

        /// <inheritdoc />
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            base.UnloadResourceInternal(device, resources);

            _pixelShaderBlur = null;
            _renderTarget = null;
            _defaultResources = null;
            _constantBuffer = null;
        }

        /// <inheritdoc />
        internal override void NotifyBeforeRender(RenderState renderState, string layerName, int passId)
        {
            switch (passId)
            {
                //******************************
                // 1. Pass: Draw all pixels that ly behind other already rendered elements
                case 0:
                    // Apply current render target size an push render target texture on current rendering stack
                    _renderTarget.ApplySize(renderState);
                    _renderTarget.PushOnRenderState(renderState, PushRenderTargetMode.Default_OwnColor_PrevDepthObjectIdNormalDepth);

                    // Clear current render target
                    renderState.ClearCurrentColorBuffer(new Color(0f, 0f, 0f, 0f));
                    break;
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

            // Reset settings made on render state (needed for all passes)
            deviceContext.Rasterizer.State = _defaultResources.RasterStateDefault;

            // Reset render target (needed for all passes)
            _renderTarget.PopFromRenderState(renderState);

            // Update constant buffer data
            var currentViewSize = renderState.ViewInformation.CurrentViewSize;
            _constantBufferData.ScreenPixelSize = currentViewSize.ToVector2();
            _constantBufferData.Opacity = 0.9f;
            _constantBufferData.Threshold = 0.2f;
            _constantBufferData.Thickness = this.Thickness;
            _constantBufferData.BorderColor = _borderColor.ToVector3();
            _constantBufferData.OriginalColorAlpha = this.DrawOriginalObject ? 1f : 0f;
            _constantBuffer.SetData(deviceContext, _constantBufferData);

            // Render result of current pass to the main render target
            switch (passId)
            {
                case 0:
                    this.ApplyAlphaBasedSpriteRendering(deviceContext);
                    try
                    {
                        deviceContext.PixelShader.SetShaderResource(0, _renderTarget.TextureView);
                        deviceContext.PixelShader.SetSampler(0, _defaultResources.GetSamplerState(TextureSamplerQualityLevel.Low));
                        deviceContext.PixelShader.SetConstantBuffer(2, _constantBuffer.ConstantBuffer);
                        deviceContext.PixelShader.Set(_pixelShaderBlur.PixelShader);
                        deviceContext.Draw(3, 0);
                    }
                    finally
                    {
                        this.DiscardAlphaBasedSpriteRendering(deviceContext);
                    }
                    return false;
            }

            return false;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        [StructLayout(LayoutKind.Sequential)]
        private struct CBPerObject
        {
            public float Opacity;
            public Vector2 ScreenPixelSize;
            public float Thickness;
            public float Threshold;
            public Vector3 BorderColor;
            public float OriginalColorAlpha;
            public Vector3 Dummy;
        }
    }
}
