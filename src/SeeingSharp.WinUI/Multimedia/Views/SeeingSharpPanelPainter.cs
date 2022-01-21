using System;
using System.Numerics;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SeeingSharp.Core;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Core.Devices;
using SeeingSharp.Drawing3D;
using SeeingSharp.Input;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;
using SharpGen.Runtime;
using Vortice.DXGI;
using D3D11 = Vortice.Direct3D11;
using GDI = System.Drawing;
using ISwapChainPanelNative = Vortice.WinUI.ISwapChainPanelNative;
using Viewport = Vortice.Mathematics.Viewport;

namespace SeeingSharp.Views
{
    public class SeeingSharpPanelPainter : ISeeingSharpPainter, IDisposable, IInputEnabledView, IRenderLoopHost
    {
        private const double MIN_PIXEL_SIZE_HEIGHT = 100.0;
        private const double MIN_PIXEL_SIZE_WIDTH = 100.0;

        // SwapChainPanel local members
        private SwapChainPanel? _targetPanel;
        private ISwapChainPanelNative? _panelNative;
        private GDI.SizeF _lastRefreshTargetSize;
        private bool _compositionScaleChanged;
        private DateTime _lastSizeChange;

        // Resources from Direct3D 11
        private IDXGISwapChain1? _swapChain;
        private D3D11.ID3D11Texture2D? _backBuffer;
        private D3D11.ID3D11Texture2D? _backBufferMultisampled;
        private D3D11.ID3D11Texture2D? _depthBuffer;
        private D3D11.ID3D11RenderTargetView? _renderTargetView;
        private D3D11.ID3D11DepthStencilView? _renderTargetDepth;

        /// <summary>
        /// Gets the current 3D scene.
        /// </summary>
        public Scene Scene
        {
            get => this.RenderLoop.Scene;
            set => this.RenderLoop.SetScene(value);
        }

        /// <summary>
        /// Gets or sets the current 3D camera.
        /// </summary>
        public Camera3DBase Camera
        {
            get => this.RenderLoop.Camera;
            set => this.RenderLoop.Camera = value;
        }

        /// <summary>
        /// Gets current renderloop object.
        /// </summary>
        public RenderLoop RenderLoop { get; }

        public GraphicsViewConfiguration Configuration => this.RenderLoop.Configuration;

        /// <summary>
        /// Discard rendering?
        /// </summary>
        public bool DiscardRendering
        {
            get => this.RenderLoop.DiscardRendering;
            set => this.RenderLoop.DiscardRendering = value;
        }

        /// <summary>
        /// Discard presenting frames?
        /// </summary>
        public bool DiscardPresent { get; set; }

        /// <summary>
        /// Should we detach automatically if the TargetPanel gets unloaded?
        /// </summary>
        public bool DetachOnUnload { get; set; }

        /// <summary>
        /// Is this painter attached to any panel?
        /// </summary>
        public bool IsAttachedToGui => _targetPanel != null;

        /// <summary>
        /// Gets the current pixel size of the target panel.
        /// </summary>
        public GDI.Size PixelSize => this.GetTargetRenderPixelSize();

        public GDI.Size ActualSize
        {
            get
            {
                if (_targetPanel == null) { return GDI.Size.Empty; }
                else{ return new GDI.Size((int)_targetPanel.ActualWidth, (int)_targetPanel.ActualHeight); }
            }
        }

        /// <summary>
        /// Gets or sets the clear color for the 3D view.
        /// </summary>
        public Color ClearColor
        {
            get
            {
                var clearColor = this.RenderLoop.ClearColor;
                return SeeingSharpWinUIUtil.UIColorFromColor4(ref clearColor);
            }
            set => this.RenderLoop.ClearColor = SeeingSharpWinUIUtil.Color4FromUIColor(ref value);
        }

        public Panel? TargetPanel => _targetPanel;

        /// <summary>
        /// True if the control is connected with the main rendering loop.
        /// False if something went wrong.
        /// </summary>
        public bool IsOperational => this.RenderLoop.IsOperational;

        public DispatcherQueue? DispatcherQueue => _targetPanel?.DispatcherQueue;

        /// <summary>
        /// Does the target control have focus?
        /// (Return true here if rendering runs, because in WinRT we are every time at fullscreen)
        /// </summary>
        bool IInputEnabledView.Focused => this.RenderLoop.IsRegisteredOnMainLoop;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeeingSharpPanelPainter" /> class.
        /// </summary>
        private SeeingSharpPanelPainter()
        {
            _lastSizeChange = DateTime.MinValue;

            var syncContext = SynchronizationContext.Current;
            if (syncContext == null)
            {
                throw new SeeingSharpException("Unable to determine the SynchronizationContext of the current thread!");
            }

            // Create the RenderLoop object
            this.RenderLoop = new RenderLoop(syncContext, this)
            {
                ClearColor = Color4.Transparent
            };

            this.RenderLoop.Internals.CallPresentInUIThread = true;
            this.DetachOnUnload = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SeeingSharpPanelPainter"/> class.
        /// </summary>
        /// <param name="targetPanel">The target panel.</param>
        public SeeingSharpPanelPainter(SwapChainPanel targetPanel)
            : this()
        {
            this.Attach(targetPanel);
        }

        /// <summary>
        /// Attaches the renderer to the given SwapChainBackgroundPanel.
        /// </summary>
        /// <param name="targetPanel">The target panel to attach to.</param>
        public void Attach(SwapChainPanel targetPanel)
        {
            if (_targetPanel != null) { throw new InvalidOperationException("Unable to attach to new SwapChainBackgroundPanel: Renderer is already attached to another one!"); }

            _lastRefreshTargetSize = new GDI.SizeF(0f, 0f);
            _targetPanel = targetPanel;

            _panelNative = ComObject.As<ISwapChainPanelNative>(_targetPanel);

            _targetPanel.SizeChanged += this.OnTargetPanel_SizeChanged;
            _targetPanel.Loaded += this.OnTargetPanel_Loaded;
            _targetPanel.Unloaded += this.OnTargetPanel_Unloaded;
            _targetPanel.CompositionScaleChanged += this.OnTargetPanel_CompositionScaleChanged;

            this.UpdateRenderLoopViewSize();

            // Define unloading behavior
            if (VisualTreeHelper.GetParent(_targetPanel) != null)
            {
                this.RenderLoop.RegisterRenderLoop();
            }
        }

        /// <summary>
        /// Detaches the renderer from the current target panel.
        /// </summary>
        public void Detach()
        {
            try
            {
                if (_targetPanel == null) { return; }

                // Clear view resources
                this.RenderLoop.Internals.UnloadViewResources();
                this.RenderLoop.DeregisterRenderLoop();

                // Clear event registrations
                _targetPanel.SizeChanged -= this.OnTargetPanel_SizeChanged;
                _targetPanel.Loaded -= this.OnTargetPanel_Loaded;
                _targetPanel.Unloaded -= this.OnTargetPanel_Unloaded;
                _targetPanel.CompositionScaleChanged -= this.OnTargetPanel_CompositionScaleChanged;

                // Clear created references
                if (_targetPanel != null)
                {
                    this.SetSwapChain(null);
                    SeeingSharpUtil.SafeDispose(ref _panelNative);
                    _panelNative = null;
                    _targetPanel = null;
                }
            }
            catch (Exception ex)
            {
                throw new SeeingSharpException($"Error while detaching from view: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Detach();
        }

        /// <summary>
        /// Called when the size of the target panel has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Microsoft.UI.Xaml.SizeChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnTargetPanelThrottled_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (!GraphicsCore.IsLoaded)
                {
                    return;
                }

                // Ignore event, if nothing has changed..
                var actSize = this.GetTargetRenderPixelSize();

                if ((int)_lastRefreshTargetSize.Width == actSize.Width &&
                    (int)_lastRefreshTargetSize.Height == actSize.Height)
                {
                    return;
                }

                this.UpdateRenderLoopViewSize();
            }
            catch (Exception ex)
            {
                throw new SeeingSharpException($"Error during resize of the view: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the current target pixel size for the render panel.
        /// </summary>
        private GDI.Size GetTargetRenderPixelSize()
        {
            if (_targetPanel == null) { return new GDI.Size((int)MIN_PIXEL_SIZE_WIDTH, (int)MIN_PIXEL_SIZE_HEIGHT); }

            var currentWidth = _targetPanel.ActualWidth * _targetPanel.CompositionScaleX;
            var currentHeight = _targetPanel.ActualHeight * _targetPanel.CompositionScaleY;

            return new GDI.Size(
                (int)(currentWidth > MIN_PIXEL_SIZE_WIDTH ? currentWidth : MIN_PIXEL_SIZE_WIDTH),
                (int)(currentHeight > MIN_PIXEL_SIZE_HEIGHT ? currentHeight : MIN_PIXEL_SIZE_HEIGHT));
        }

        /// <summary>
        /// Update current configured view size on the render loop.
        /// </summary>
        private void UpdateRenderLoopViewSize()
        {
            var viewSize = this.GetTargetRenderPixelSize();
            this.RenderLoop.Camera.SetScreenSize(viewSize.Width, viewSize.Height);
            this.RenderLoop.SetCurrentViewSize(
                viewSize.Width,
                viewSize.Height);
        }

        private void SetSwapChain(IDXGISwapChain1? swapChain)
        {
            if (_panelNative != null)
            {
                _panelNative.SetSwapChain(swapChain!);
            }
            else
            {
                throw new SeeingSharpException(
                    "Unable to change SwapChain of target SwapChainPanel: Interface ISwapChainPanelNative not available!");
            }
        }

        /// <summary>
        /// Called when the target panel gets unloaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnTargetPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            this.RenderLoop.DeregisterRenderLoop();

            // Trigger detach if requested
            if (this.DetachOnUnload)
            {
                this.Detach();
            }
        }

        /// <summary>
        /// Called when the target panel gets loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnTargetPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.RenderLoop.IsRegisteredOnMainLoop)
            {
                this.RenderLoop.RegisterRenderLoop();
            }
        }

        /// <summary>
        /// Called when the size of the target panel has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
        private void OnTargetPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!GraphicsCore.IsLoaded)
            {
                return;
            }

            //Resize render target only on greater size changes
            var viewSize = this.GetTargetRenderPixelSize();
            var resizeFactorWidth = (double)viewSize.Width > _lastRefreshTargetSize.Width ? viewSize.Width / _lastRefreshTargetSize.Width : _lastRefreshTargetSize.Width / viewSize.Width;
            var resizeFactorHeight = (double)viewSize.Height > _lastRefreshTargetSize.Height ? viewSize.Height / _lastRefreshTargetSize.Height : _lastRefreshTargetSize.Height / viewSize.Height;

            if (resizeFactorWidth > 1.3 || resizeFactorHeight > 1.3)
            {
                this.UpdateRenderLoopViewSize();
            }

            _lastSizeChange = DateTime.UtcNow;
        }

        /// <summary>
        /// Some configuration like
        /// </summary>
        private void OnTargetPanel_CompositionScaleChanged(SwapChainPanel sender, object args)
        {
            this.UpdateRenderLoopViewSize();

            _compositionScaleChanged = true;
        }

        /// <summary>
        /// Disposes all loaded view resources.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_DisposeViewResources(EngineDevice engineDevice)
        {
            if (_targetPanel != null)
            {
                this.SetSwapChain(null);
            }

            _renderTargetDepth = SeeingSharpUtil.DisposeObject(_renderTargetDepth);
            _depthBuffer = SeeingSharpUtil.DisposeObject(_depthBuffer);
            _renderTargetView = SeeingSharpUtil.DisposeObject(_renderTargetView);
            _backBuffer = SeeingSharpUtil.DisposeObject(_backBuffer);
            _backBufferMultisampled = SeeingSharpUtil.DisposeObject(_backBufferMultisampled);
            _swapChain = SeeingSharpUtil.DisposeObject(_swapChain);
        }

        /// <summary>
        /// Create all view resources.
        /// </summary>
        Tuple<D3D11.ID3D11Texture2D, D3D11.ID3D11RenderTargetView, D3D11.ID3D11Texture2D, D3D11.ID3D11DepthStencilView, Viewport, GDI.Size, DpiScaling> IRenderLoopHost.OnRenderLoop_CreateViewResources(EngineDevice engineDevice)
        {
            _backBufferMultisampled = null;

            var viewSize = this.GetTargetRenderPixelSize();

            // Create the SwapChain and associate it with the SwapChainBackgroundPanel
            _swapChain = GraphicsHelperWinUI.CreateSwapChainForComposition(engineDevice, viewSize.Width, viewSize.Height, this.RenderLoop.Configuration);
            this.SetSwapChain(_swapChain);
            _compositionScaleChanged = true;

            // Get the backbuffer from the SwapChain
            _backBuffer = _swapChain.GetBuffer<D3D11.ID3D11Texture2D>(0);

            // Define the render target (in case of multisample an own render target)
            D3D11.ID3D11Texture2D? backBufferForRenderloop = null;
            if (this.RenderLoop.Configuration.AntialiasingEnabled)
            {
                _backBufferMultisampled = GraphicsHelper.Internals.CreateRenderTargetTexture(engineDevice, viewSize.Width, viewSize.Height, this.RenderLoop.Configuration);
                _renderTargetView = engineDevice.Internals.DeviceD3D11_1.CreateRenderTargetView(_backBufferMultisampled);
                backBufferForRenderloop = _backBufferMultisampled;
            }
            else
            {
                _renderTargetView = engineDevice.Internals.DeviceD3D11_1.CreateRenderTargetView(_backBuffer);
                backBufferForRenderloop = _backBuffer;
            }

            //Create the depth buffer
            _depthBuffer = GraphicsHelper.Internals.CreateDepthBufferTexture(engineDevice, viewSize.Width, viewSize.Height, this.RenderLoop.Configuration);
            _renderTargetDepth = engineDevice.Internals.DeviceD3D11_1.CreateDepthStencilView(_depthBuffer);

            //Define the viewport for rendering
            var viewPort = GraphicsHelper.Internals.CreateDefaultViewport(viewSize.Width, viewSize.Height);
            _lastRefreshTargetSize = new GDI.SizeF(viewSize.Width, viewSize.Height);

            var dpiScaling = new DpiScaling
            {
                DpiX = (float)(96.0 * _targetPanel!.CompositionScaleX),
                DpiY = (float)(96.0 * _targetPanel!.CompositionScaleY)
            };

            return Tuple.Create(backBufferForRenderloop, _renderTargetView, _depthBuffer, _renderTargetDepth, viewPort, viewSize, dpiScaling);
        }

        /// <summary>
        /// Called when RenderLoop object checks whether it is possible to render.
        /// </summary>
        bool IRenderLoopHost.OnRenderLoop_CheckCanRender(EngineDevice engineDevice)
        {
            if (_targetPanel == null) { return false; }
            if (!_targetPanel.IsLoaded) { return false;}
            if (_targetPanel.ActualWidth <= 0) { return false; }
            if (_targetPanel.ActualHeight <= 0) { return false; }
            if (_targetPanel.Visibility != Visibility.Visible) { return false; }

            return true;
        }

        void IRenderLoopHost.OnRenderLoop_PrepareRendering(EngineDevice engineDevice)
        {
            if (_targetPanel == null) { return; }

            // Update swap chain scaling (only relevant for SwapChainPanel targets)
            //  see https://www.packtpub.com/books/content/integrating-direct3d-xaml-and-windows-81
            if (_swapChain != null)
            {
                if (_compositionScaleChanged)
                {
                    _compositionScaleChanged = false;
                    var swapChain2 = _swapChain.QueryInterfaceOrNull<IDXGISwapChain2>();
                    if (swapChain2 != null)
                    {
                        try
                        {
                            var inverseScale = new Matrix3x2
                            {
                                M11 = 1.0f / _targetPanel.CompositionScaleX,
                                M22 = 1.0f / _targetPanel.CompositionScaleY
                            };

                            swapChain2.MatrixTransform = inverseScale;
                        }
                        finally
                        {
                            swapChain2.Dispose();
                        }
                    }
                }
            }

            // Handle throttled resizing of view resources
            if (_lastSizeChange != DateTime.MinValue &&
                DateTime.UtcNow - _lastSizeChange > SeeingSharpConstantsUwp.THROTTLED_VIEW_RECREATION_TIME_ON_RESIZE)
            {
                _lastSizeChange = DateTime.MinValue;
                this.UpdateRenderLoopViewSize();
            }
        }

        /// <summary>
        /// Called when RenderLoop wants to present its results.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_Present(EngineDevice engineDevice)
        {
            if (this.DiscardPresent) { return; }

            // Copy contents of the backbuffer if in multisampling mode
            if (_backBufferMultisampled != null)
            {
                engineDevice.Internals.DeviceImmediateContextD3D11.ResolveSubresource(
                    _backBuffer, 0, 
                    _backBufferMultisampled, 0,
                    GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT);
            }

            // Present all rendered stuff on screen
            // First parameter indicates synchronization with vertical blank
            //  see http://msdn.microsoft.com/en-us/library/windows/desktop/bb174576(v=vs.85).aspx
            //  see example http://msdn.microsoft.com/en-us/library/windows/apps/hh825871.aspx
            _swapChain!.Present(1, PresentFlags.None);
        }

        /// <summary>
        /// Called when RenderLoop has finished rendering.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_AfterRendering(EngineDevice engineDevice)
        {

        }
    }
}