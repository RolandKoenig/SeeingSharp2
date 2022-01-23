using System.Drawing;
using SeeingSharp.Checking;
using SeeingSharp.Core;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Core.Devices;
using SeeingSharp.Drawing2D;
using SeeingSharp.Util;
using SharpGen.Runtime;
using D2D = Vortice.Direct2D1;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D.Resources
{
    public class Direct2DTextureResource : TextureResource, IRenderableResource, IUpdatableResource
    {
        private Custom2DDrawingLayer _drawingLayer;
        private int _width;
        private int _height;
        private bool _notLoadedDueToMissingSupport;

        // Resources for Direct3D
        private D3D11.ID3D11Texture2D? _renderTargetTexture;
        private D3D11.ID3D11ShaderResourceView? _renderTargetTextureView;

        // Resources for Direct2D
        private Graphics2D? _graphics2D;
        private Direct2DOverlayRenderer? _overlayRenderer;

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => (_graphics2D != null) || _notLoadedDueToMissingSupport;

        /// <summary>
        /// Gets the texture object.
        /// </summary>
        internal override D3D11.ID3D11Texture2D Texture
        {
            get
            {
                _renderTargetTexture.EnsureResourceLoaded(typeof(Direct2DTextureResource));
                return _renderTargetTexture!;
            }
        }

        /// <summary>
        /// Gets a ShaderResourceView targeting the texture.
        /// </summary>
        internal override D3D11.ID3D11ShaderResourceView TextureView
        {
            get
            {
                _renderTargetTextureView.EnsureResourceLoaded(typeof(Direct2DTextureResource));
                return _renderTargetTextureView!;
            }
        }

        /// <summary>
        /// Gets the size of the texture array.
        /// 1 for normal textures.
        /// 6 for cubemap textures.
        /// </summary>
        public override int ArraySize => 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Direct2DTextureResource"/> class.
        /// </summary>
        /// <param name="drawingLayer">The drawing layer.</param>
        /// <param name="height">The width of the generated texture.</param>
        /// <param name="width">The height of the generated texture.</param>
        public Direct2DTextureResource(Custom2DDrawingLayer drawingLayer, int width, int height)
        {
            drawingLayer.EnsureNotNull(nameof(drawingLayer));
            width.EnsurePositiveOrZero(nameof(width));
            height.EnsurePositiveOrZero(nameof(height));

            _drawingLayer = drawingLayer;
            _width = width;
            _height = height;
        }

        /// <summary>
        /// Triggers internal update within the resource (e. g. Render to Texture).
        /// </summary>
        /// <param name="updateState">Current state of update process.</param>
        public void Update(UpdateState updateState)
        {
            _drawingLayer.UpdateInternal(updateState);
        }

        /// <summary>
        /// Triggers internal rendering within the resource (e. g. Render to Texture).
        /// </summary>
        /// <param name="renderState">Current render state.</param>
        public void Render(RenderState renderState)
        {
            if (renderState.Device.IsLost) { return; }
            if (_notLoadedDueToMissingSupport) { return; }
            if (_overlayRenderer!.IsRenderTargetDisposed) { return; }

            _overlayRenderer!.BeginDraw();
            try
            {
                if (_graphics2D != null)
                {
                    _drawingLayer.Draw2DInternal(_graphics2D);
                }
            }
            finally
            {
                try
                {
                    _overlayRenderer!.EndDraw();
                }
                catch (SharpGenException dxException)
                {
                    if (dxException.ResultCode == D2D.ResultCode.RecreateTarget)
                    {
                        // Mark the device as lost
                        renderState.Device.IsLost = true;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Loads all resource.
        /// </summary>
        /// <param name="device">The device on which to load all resources.</param>
        /// <param name="resources">The current ResourceDictionary.</param>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _renderTargetTexture = GraphicsHelper.Internals.CreateRenderTargetTexture(
                device, _width, _height, new GraphicsViewConfiguration { AntialiasingEnabled = false });
            _renderTargetTextureView = device.DeviceD3D11_1.CreateShaderResourceView(_renderTargetTexture);

            if (device.Supports2D)
            {
                _overlayRenderer = new Direct2DOverlayRenderer(
                    device,
                    _renderTargetTexture,
                    _width, _height,
                    DpiScaling.Default);
                _graphics2D = new Graphics2D(device, _overlayRenderer.RenderTarget2D, new SizeF(_width, _height));
            }
            else
            {
                _notLoadedDueToMissingSupport = true;
            }
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        /// <param name="device">The device on which the resources where loaded.</param>
        /// <param name="resources">The current ResourceDictionary.</param>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            if (device.Supports2D)
            {
                _graphics2D = null;
                SeeingSharpUtil.SafeDispose(ref _overlayRenderer);

            }
            else
            {
                _notLoadedDueToMissingSupport = false;
                return;
            }

            SeeingSharpUtil.SafeDispose(ref _renderTargetTextureView);
            SeeingSharpUtil.SafeDispose(ref _renderTargetTexture);
        }
    }
}
