using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D
{
    internal class RenderTargetTextureResource : TextureResource
    {
        // Runtime variables
        private bool _shaderResourceCreated;

        // Configuration
        private RenderTargetCreationMode _creationMode;
        private int _width;
        private int _height;
        private bool _antialiasingEnabled;
        private AntialiasingQualityLevel _antialiasingQuality;
        private Vortice.Mathematics.Viewport _viewportF;
        private bool _forceRecreateResources;

        // Resources for depth buffer
        private D3D11.ID3D11Texture2D _depthBuffer;
        private D3D11.ID3D11DepthStencilView _depthBufferView;

        // Resources for color buffer
        private D3D11.ID3D11Texture2D _colorBuffer;
        private D3D11.ID3D11RenderTargetView _colorBufferRenderTargetView;
        private D3D11.ID3D11Texture2D _colorBufferShaderResource;
        private D3D11.ID3D11ShaderResourceView _colorBufferShaderResourceView;

        // Resources for ObjectId buffer
        private D3D11.ID3D11Texture2D _objectIdBuffer;
        private D3D11.ID3D11RenderTargetView _objectIdBufferRenderTargetView;

        // Resources for normal/depth buffer
        private D3D11.ID3D11Texture2D _normalDepthBuffer;
        private D3D11.ID3D11RenderTargetView _normalDepthBufferRenderTargetView;
        private D3D11.ID3D11Texture2D _normalDepthBufferShaderResource;
        private D3D11.ID3D11ShaderResourceView _normalDepthBufferShaderResourceView;

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => true;

        /// <summary>
        /// Gets the texture itself.
        /// </summary>
        internal override D3D11.ID3D11Texture2D Texture => _colorBuffer;

        /// <summary>
        /// Gets the shader resource view to the texture.
        /// </summary>
        internal override D3D11.ID3D11ShaderResourceView TextureView => _colorBufferShaderResourceView;

        public override int ArraySize => 1;

        internal D3D11.ID3D11Texture2D TextureColor => _colorBuffer;

        internal D3D11.ID3D11ShaderResourceView TextureViewColor => _colorBufferShaderResourceView;

        /// <summary>
        /// Gets the shader resource view to the normal-depth texture.
        /// </summary>
        internal D3D11.ID3D11ShaderResourceView TextureViewNormalDepth => _normalDepthBufferShaderResourceView;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetTextureResource" /> class.
        /// </summary>
        /// <param name="creationMode">Tells this object which texture to create.</param>
        public RenderTargetTextureResource(RenderTargetCreationMode creationMode)
        {
            _creationMode = creationMode;
            _width = -1;
            _height = -1;
            _viewportF = new Vortice.Mathematics.Viewport();
            _shaderResourceCreated = false;
        }

        /// <summary>
        /// Applies the given size.
        /// </summary>
        /// <param name="renderState">The render state used for creating all resources.</param>
        public void ApplySize(RenderState renderState)
        {
            var viewInfo = renderState.ViewInformation;
            var viewConfig = viewInfo.ViewConfiguration;

            // Get current view size and antialiasing settings
            var currentViewSize = viewInfo.CurrentViewSize;
            var currentAntialiasingEnabled = viewConfig.AntialiasingEnabled;
            var currentAntialiasingQuality = viewConfig.AntialiasingQuality;

            if (_width != currentViewSize.Width ||
                _height != currentViewSize.Height ||
                _antialiasingEnabled != currentAntialiasingEnabled ||
                _antialiasingQuality != currentAntialiasingQuality ||
                _forceRecreateResources)
            {
                _forceRecreateResources = false;

                // Dispose color-buffer resources
                SeeingSharpUtil.SafeDispose(ref _colorBuffer);
                SeeingSharpUtil.SafeDispose(ref _colorBufferRenderTargetView);
                SeeingSharpUtil.SafeDispose(ref _colorBufferShaderResourceView);
                if (_shaderResourceCreated) { SeeingSharpUtil.SafeDispose(ref _colorBufferShaderResource); }

                // Dispose depth-buffer resources
                SeeingSharpUtil.SafeDispose(ref _depthBufferView);
                SeeingSharpUtil.SafeDispose(ref _depthBuffer);

                // Dispose object-id buffer
                SeeingSharpUtil.SafeDispose(ref _objectIdBufferRenderTargetView);
                SeeingSharpUtil.SafeDispose(ref _objectIdBuffer);

                // Dispose normal-depth resources
                SeeingSharpUtil.SafeDispose(ref _normalDepthBuffer);
                SeeingSharpUtil.SafeDispose(ref _normalDepthBufferRenderTargetView);
                SeeingSharpUtil.SafeDispose(ref _normalDepthBufferShaderResourceView);
                if (_shaderResourceCreated) { SeeingSharpUtil.SafeDispose(ref _normalDepthBufferShaderResource); }

                // Create color-buffer resources
                if (_creationMode.HasFlag(RenderTargetCreationMode.Color))
                {
                    _colorBuffer = GraphicsHelper.Internals.CreateRenderTargetTexture(
                        renderState.Device, currentViewSize.Width, currentViewSize.Height, renderState.ViewInformation.ViewConfiguration);
                    _colorBufferShaderResource = _colorBuffer;
                    if (renderState.ViewInformation.ViewConfiguration.AntialiasingEnabled)
                    {
                        _colorBufferShaderResource = GraphicsHelper.Internals.CreateTexture(renderState.Device, currentViewSize.Width, currentViewSize.Height);
                        _shaderResourceCreated = true;
                    }
                    else
                    {
                        _shaderResourceCreated = false;
                    }
                    _colorBufferRenderTargetView = renderState.Device.DeviceD3D11_1.CreateRenderTargetView(_colorBuffer);
                    _colorBufferShaderResourceView = renderState.Device.DeviceD3D11_1.CreateShaderResourceView(_colorBufferShaderResource);
                }

                // Create depth-buffer resources
                if (_creationMode.HasFlag(RenderTargetCreationMode.Depth))
                {
                    _depthBuffer = GraphicsHelper.Internals.CreateDepthBufferTexture(
                        renderState.Device, currentViewSize.Width, currentViewSize.Height, renderState.ViewInformation.ViewConfiguration);
                    _depthBufferView = GraphicsHelper.Internals.CreateDepthBufferView(renderState.Device, _depthBuffer);
                }

                // Create object-id resources
                if (_creationMode.HasFlag(RenderTargetCreationMode.ObjectId))
                {
                    _objectIdBuffer = GraphicsHelper.Internals.CreateRenderTargetTextureObjectIds(
                        renderState.Device, currentViewSize.Width, currentViewSize.Height, renderState.ViewInformation.ViewConfiguration);
                    _objectIdBufferRenderTargetView = renderState.Device.DeviceD3D11_1.CreateRenderTargetView(_objectIdBuffer);
                }

                // Create normal-depth buffer resources
                if (_creationMode.HasFlag(RenderTargetCreationMode.NormalDepth))
                {
                    _normalDepthBuffer = GraphicsHelper.Internals.CreateRenderTargetTextureNormalDepth(
                        renderState.Device, currentViewSize.Width, currentViewSize.Height, renderState.ViewInformation.ViewConfiguration);
                    _normalDepthBufferShaderResource = _normalDepthBuffer;
                    if (_shaderResourceCreated)
                    {
                        _normalDepthBufferShaderResource = GraphicsHelper.Internals.CreateTexture(
                            renderState.Device, currentViewSize.Width, currentViewSize.Height, GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT_NORMAL_DEPTH);
                    }
                    _normalDepthBufferRenderTargetView = renderState.Device.DeviceD3D11_1.CreateRenderTargetView(_normalDepthBuffer);
                    _normalDepthBufferShaderResourceView = renderState.Device.DeviceD3D11_1.CreateShaderResourceView(_normalDepthBufferShaderResource);
                }

                // Remember values
                _width = currentViewSize.Width;
                _height = currentViewSize.Height;
                _antialiasingEnabled = currentAntialiasingEnabled;
                _antialiasingQuality = currentAntialiasingQuality;
                _viewportF = renderState.Viewport;
            }
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _forceRecreateResources = true;
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            SeeingSharpUtil.SafeDispose(ref _depthBufferView);
            SeeingSharpUtil.SafeDispose(ref _colorBufferRenderTargetView);
            SeeingSharpUtil.SafeDispose(ref _colorBufferShaderResourceView);
            SeeingSharpUtil.SafeDispose(ref _depthBuffer);
            SeeingSharpUtil.SafeDispose(ref _colorBuffer);
            SeeingSharpUtil.SafeDispose(ref _normalDepthBufferRenderTargetView);
            SeeingSharpUtil.SafeDispose(ref _normalDepthBufferShaderResourceView);
            SeeingSharpUtil.SafeDispose(ref _normalDepthBuffer);

            // Unload shader resource if it was created explicitly
            if (_shaderResourceCreated)
            {
                SeeingSharpUtil.SafeDispose(ref _colorBufferShaderResource);
                SeeingSharpUtil.SafeDispose(ref _normalDepthBufferShaderResource);
                _shaderResourceCreated = false;
            }
        }

        /// <summary>
        /// Pushes this render target on the given render state.
        /// </summary>
        /// <param name="renderState">The render state to push to.</param>
        /// <param name="mode"></param>
        internal void PushOnRenderState(RenderState renderState, PushRenderTargetMode mode)
        {
            // Store RenderTargets structures
            var prevRenderTargets = renderState.CurrentRenderTargets;
            var newRenderTargets = new RenderTargets();

            // Handle color buffer
            if (mode.HasFlag(PushRenderTargetMode.UseOwnColorBuffer))
            {
                newRenderTargets.ColorBuffer = _colorBufferRenderTargetView;
            }
            else if (mode.HasFlag(PushRenderTargetMode.OvertakeColorBuffer))
            {
                newRenderTargets.ColorBuffer = prevRenderTargets.ColorBuffer;
            }

            // Handle depth-stencil buffer
            if (mode.HasFlag(PushRenderTargetMode.UseOwnDepthBuffer))
            {
                newRenderTargets.DepthStencilBuffer = _depthBufferView;
            }
            else if (mode.HasFlag(PushRenderTargetMode.OvertakeDepthBuffer))
            {
                newRenderTargets.DepthStencilBuffer = prevRenderTargets.DepthStencilBuffer;
            }

            // Handle object-id buffer
            if (mode.HasFlag(PushRenderTargetMode.UseOwnObjectIdBuffer))
            {
                newRenderTargets.ObjectIdBuffer = _objectIdBufferRenderTargetView;
            }
            else if (mode.HasFlag(PushRenderTargetMode.OvertakeObjectIdBuffer))
            {
                newRenderTargets.ObjectIdBuffer = prevRenderTargets.ObjectIdBuffer;
            }

            // Handle normal-depth buffer
            if (mode.HasFlag(PushRenderTargetMode.UseOwnNormalDepthBuffer))
            {
                newRenderTargets.NormalDepthBuffer = _normalDepthBufferRenderTargetView;
            }
            else if (mode.HasFlag(PushRenderTargetMode.OvertakeNormalDepthBuffer))
            {
                newRenderTargets.NormalDepthBuffer = prevRenderTargets.NormalDepthBuffer;
            }

            // Push new RenderTargets structure onto the rendering stack
            renderState.PushRenderTarget(
                newRenderTargets,
                _viewportF, renderState.Camera, renderState.ViewInformation);
        }

        /// <summary>
        /// Pops the render target from the given render state.
        /// </summary>
        /// <param name="renderState">The render state.</param>
        internal void PopFromRenderState(RenderState renderState)
        {
            renderState.PopRenderTarget();

            // Copy texture data when in antialiasing ode
            if (_antialiasingEnabled)
            {
                // Resolve color buffer
                if (_creationMode.HasFlag(RenderTargetCreationMode.Color))
                {
                    renderState.Device.DeviceImmediateContextD3D11.ResolveSubresource(
                        _colorBuffer, 0, _colorBufferShaderResource, 0, GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT);
                }

                // Resolve normal-depth buffer
                if (_creationMode.HasFlag(RenderTargetCreationMode.NormalDepth))
                {
                    renderState.Device.DeviceImmediateContextD3D11.ResolveSubresource(
                        _normalDepthBuffer, 0, _normalDepthBufferShaderResource, 0, GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT_NORMAL_DEPTH);
                }
            }
        }
    }
}
