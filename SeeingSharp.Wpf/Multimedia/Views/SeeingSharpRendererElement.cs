/*
    Seeing# and all applications distributed together with it. 
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
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Input;
using SeeingSharp.Util;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Views
{
    public class SeeingSharpRendererElement : Image, IInputEnabledView, ISeeingSharpPainter, IRenderLoopHost, INotifyPropertyChanged
    {
        private static Duration MAX_IMAGE_LOCK_DURATION = new Duration(TimeSpan.FromMilliseconds(100.0));

        // Some members..
        private HigherD3DImageSource _d3dImageSource;
        private WriteableBitmap _fallbackWpfImageSource;
        private int _lastRecreateWidth;
        private int _lastRecreateHeight;
        private int _lockImageErrorCount;

        // All needed direct3d resources
        private D3D11.Texture2D _backBufferForWpf;
        private D3D11.Texture2D _backBufferD3D11;
        private D3D11.Texture2D _depthBuffer;
        private D3D11.RenderTargetView _renderTarget;
        private D3D11.DepthStencilView _renderTargetDepth;
        private Surface _renderTarget2DDxgi;

        // Some size related properties
        private int _renderTargetHeight;
        private int _renderTargetWidth;
        private DateTime _lastSizeChange;
        private WpfSeeingSharpCompositionMode _compositionMode;
        private bool _forceCompositionOverSoftware;

        // State members for handling rendering problems
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeeingSharpRendererElement"/> class.
        /// </summary>
        public SeeingSharpRendererElement()
        {
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;

            _compositionMode = WpfSeeingSharpCompositionMode.None;

            // Basic configuration
            this.Focusable = true;
            this.IsHitTestVisible = true;
            this.Stretch = Stretch.Fill;
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;

            // Create the RenderLoop object
            this.RenderLoop = new RenderLoop(
                SynchronizationContext.Current, this, this.IsInDesignMode());
            this.RenderLoop.DeviceChanged += this.OnRenderLoop_DeviceChanged;
            this.RenderLoop.CurrentViewSizeChanged += this.OnRenderLoop_CurrentViewSizeChanged;

            // Break here if we are in design mode
            if (this.IsInDesignMode()) { return; }

            this.RenderLoop.ClearColor = Color4.Transparent;
            this.RenderLoop.Internals.CallPresentInUIThread = true;

            // Create new scene and camera object
            this.Scene = new Scene();
            this.Camera = new PerspectiveCamera3D();
        }

        /// <summary>
        /// Gets the current DpiScaling mode.
        /// </summary>
        public DpiScaling GetDpiScaling()
        {
            var source = PresentationSource.FromVisual(this);
            var dpiScaleFactorX = 1.0;
            var dpiScaleFactorY = 1.0;

            if (source?.CompositionTarget != null)
            {
                dpiScaleFactorX = source.CompositionTarget.TransformToDevice.M11;
                dpiScaleFactorY = source.CompositionTarget.TransformToDevice.M22;
            }

            var result = DpiScaling.Default;
            result.DpiX = (float)(result.DpiX * dpiScaleFactorX);
            result.DpiY = (float)(result.DpiY * dpiScaleFactorY);
            return result;
        }

        /// <summary>
        /// Gets the pixel size of the given UIElement.
        /// </summary>
        /// <param name="minSize">The minimum size to be returned.</param>
        public Size GetPixelSize(Size minSize)
        {
            var source = PresentationSource.FromVisual(this);
            var dpiScaleFactorX = 1.0;
            var dpiScaleFactorY = 1.0;
            if (source?.CompositionTarget != null)
            {
                dpiScaleFactorX = source.CompositionTarget.TransformToDevice.M11;
                dpiScaleFactorY = source.CompositionTarget.TransformToDevice.M22;
            }

            return new Size(
                Math.Max(this.RenderSize.Width * dpiScaleFactorX, 100),
                Math.Max(this.RenderSize.Height * dpiScaleFactorY, 100));
        }

        /// <summary>
        /// Transforms the given wpf point to pure pixel coordinates.
        /// </summary>
        /// <param name="wpfPoint">The wpf point to be transformed.</param>
        public System.Windows.Point GetPixelLocation(System.Windows.Point wpfPoint)
        {
            var source = PresentationSource.FromVisual(this);
            var dpiScaleFactorX = 1.0;
            var dpiScaleFactorY = 1.0;
            if (source?.CompositionTarget != null)
            {
                dpiScaleFactorX = source.CompositionTarget.TransformToDevice.M11;
                dpiScaleFactorY = source.CompositionTarget.TransformToDevice.M22;
            }

            return new System.Windows.Point(
                wpfPoint.X * dpiScaleFactorX,
                wpfPoint.Y * dpiScaleFactorY);
        }

        /// <summary>
        /// Called when the render size has changed.
        /// </summary>
        /// <param name="sizeInfo">New size information.</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (!GraphicsCore.IsLoaded) { return; }

            // Break here if we are in design mode
            if (this.IsInDesignMode()) { return; }

            // Update render size
            this.RenderLoop.Camera.SetScreenSize((int)this.RenderSize.Width, (int)this.RenderSize.Height);

            //Resize render target only on greater size changes
            var resizeFactorWidth = sizeInfo.NewSize.Width > _renderTargetWidth ? sizeInfo.NewSize.Width / _renderTargetWidth : _renderTargetWidth / sizeInfo.NewSize.Width;
            var resizeFactorHeight = sizeInfo.NewSize.Height > _renderTargetHeight ? sizeInfo.NewSize.Height / _renderTargetHeight : _renderTargetHeight / sizeInfo.NewSize.Height;
            if (resizeFactorWidth > 1.3 || resizeFactorHeight > 1.3)
            {
                this.RenderLoop.SetCurrentViewSize((int)this.RenderSize.Width, (int)this.RenderSize.Height);
            }

            _lastSizeChange = DateTime.UtcNow;
        }

        /// <summary>
        /// Is this object in design mode?
        /// </summary>
        private bool IsInDesignMode()
        {
            return DesignerProperties.GetIsInDesignMode(this);
        }

        /// <summary>
        /// Called when the image is loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!GraphicsCore.IsLoaded) { return; }
            if (this.IsInDesignMode()) { return; }

            // Update render size
            this.RenderLoop.Camera.SetScreenSize((int)this.RenderSize.Width, (int)this.RenderSize.Height);

            // Now connect this view with the main renderloop
            this.RenderLoop.RegisterRenderLoop();
        }

        private void OnRenderLoop_DeviceChanged(object sender, EventArgs e)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.SelectedDevice)));
        }

        private void OnRenderLoop_CurrentViewSizeChanged(object sender, EventArgs e)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CurrentViewSize)));
        }

        /// <summary>
        /// Called when the image is unloaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (!GraphicsCore.IsLoaded) { return; }
            if (this.IsInDesignMode()) { return; }

            this.RenderLoop.DeregisterRenderLoop();
        }

        /// <summary>
        /// Disposes all loaded view resources.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_DisposeViewResources(EngineDevice engineDevice)
        {
            this.Source = null;
            if (_d3dImageSource != null)
            {
                // Dispose the render target
                _d3dImageSource.SetRenderTarget(null);
                _d3dImageSource.IsFrontBufferAvailableChanged -= this.OnD3DImageSource_IsFrontBufferAvailableChanged;
                _d3dImageSource.Dispose();
                _d3dImageSource = null;
            }

            // Dispose all other resources
            _renderTarget2DDxgi = SeeingSharpUtil.DisposeObject(_renderTarget2DDxgi);
            _renderTargetDepth = SeeingSharpUtil.DisposeObject(_renderTargetDepth);
            _depthBuffer = SeeingSharpUtil.DisposeObject(_depthBuffer);
            _renderTarget = SeeingSharpUtil.DisposeObject(_renderTarget);
            _backBufferForWpf = SeeingSharpUtil.DisposeObject(_backBufferForWpf);
            _backBufferD3D11 = SeeingSharpUtil.DisposeObject(_backBufferD3D11);

            // Reset composition mode
            this.CompositionMode = WpfSeeingSharpCompositionMode.None;
        }

        /// <summary>
        /// Create all view resources.
        /// </summary>
        Tuple<D3D11.Texture2D, D3D11.RenderTargetView, D3D11.Texture2D, D3D11.DepthStencilView, RawViewportF, Size2, DpiScaling> IRenderLoopHost.OnRenderLoop_CreateViewResources(EngineDevice engineDevice)
        {
            SeeingSharpWpfUtil.GetDpiScalingFactor(this, out var dpiScaleFactorX, out var dpiScaleFactorY);

            _lockImageErrorCount = 0;

            // Calculate pixel with and high of this visual
            var pixelSize = new Size(
                Math.Max(this.RenderSize.Width * dpiScaleFactorX, 100),
                Math.Max(this.RenderSize.Height * dpiScaleFactorY, 100));
            var width = (int)pixelSize.Width;
            var height = (int)pixelSize.Height;

            // Get references to current render device
            D3D11.Device renderDevice = engineDevice.Internals.DeviceD3D11_1;
            var viewPort = default(RawViewportF);

            var initializedSuccessfully = false;
            var forceFallbackSolution = _forceCompositionOverSoftware;
            var tryCount = 0;

            do
            {
                tryCount++;
                if (tryCount > 2)
                {
                    this.Source = null;
                    this.CompositionMode = WpfSeeingSharpCompositionMode.None;
                    throw new SeeingSharpException($"Unable to initialize view with device {engineDevice}: Too much tries, also fallback solution does not work!");
                }

                // Try to create the object for surface sharing
                if (!engineDevice.IsSoftware &&
                    !forceFallbackSolution &&
                    RenderOptions.ProcessRenderMode != RenderMode.SoftwareOnly &&
                    RenderCapability.Tier >> 16 >= 2)
                {
                    var handlerD3D9 = engineDevice.TryGetAdditionalDeviceHandler<DeviceHandlerD3D9>();
                    if (handlerD3D9?.Device != null)
                    {
                        try
                        {
                            _d3dImageSource = new HigherD3DImageSource(engineDevice, handlerD3D9);
                            _d3dImageSource.IsFrontBufferAvailableChanged += this.OnD3DImageSource_IsFrontBufferAvailableChanged;
                        }
                        catch (Exception)
                        {
                            // Unable to enable surface sharing
                        }
                    }
                }

                // Switch to fallback method if we can't create the HigherD3DImageSource
                if (_d3dImageSource == null)
                {
                    _fallbackWpfImageSource = new WriteableBitmap(width, height, 96.0 * dpiScaleFactorX, 96.0 * dpiScaleFactorY, PixelFormats.Bgra32, BitmapPalettes.WebPaletteTransparent);
                }

                // Create the swap chain and the render target
                _backBufferD3D11 = GraphicsHelper.Internals.CreateRenderTargetTexture(engineDevice, width, height, this.RenderLoop.Configuration);
                _backBufferForWpf = GraphicsHelper.Internals.CreateSharedTexture(engineDevice, width, height);
                _renderTarget = new D3D11.RenderTargetView(renderDevice, _backBufferD3D11);

                // Create the depth buffer
                _depthBuffer = GraphicsHelper.Internals.CreateDepthBufferTexture(engineDevice, width, height, this.RenderLoop.Configuration);
                _renderTargetDepth = new D3D11.DepthStencilView(renderDevice, _depthBuffer);

                // Apply render target size values
                _renderTargetWidth = width;
                _renderTargetHeight = height;

                // Define the viewport for rendering
                viewPort = GraphicsHelper.Internals.CreateDefaultViewport(width, height);
                if (_d3dImageSource != null)
                {
                    // Create and apply the image source object
                    try
                    {
                        _d3dImageSource.SetRenderTarget(_backBufferForWpf);
                        this.Source = _d3dImageSource;
                        initializedSuccessfully = true;
                        this.CompositionMode = WpfSeeingSharpCompositionMode.OverHardware;
                    }
                    catch (Exception)
                    {
                        initializedSuccessfully = false;
                        forceFallbackSolution = true;

                        SeeingSharpUtil.SafeDispose(ref _d3dImageSource);
                        SeeingSharpUtil.SafeDispose(ref _renderTargetDepth);
                        SeeingSharpUtil.SafeDispose(ref _depthBuffer);
                        SeeingSharpUtil.SafeDispose(ref _renderTarget);
                        SeeingSharpUtil.SafeDispose(ref _backBufferForWpf);
                        SeeingSharpUtil.SafeDispose(ref _backBufferD3D11);
                    }
                }
                else
                {
                    // Set a dummy image source
                    this.Source = _fallbackWpfImageSource;
                    initializedSuccessfully = true;
                    this.CompositionMode = WpfSeeingSharpCompositionMode.FallbackOverSoftware;
                }
            }
            while (!initializedSuccessfully);

            _lastRecreateWidth = width;
            _lastRecreateHeight = height;

            // Return all generated objects
            return Tuple.Create(_backBufferD3D11, _renderTarget, _depthBuffer, _renderTargetDepth, viewPort, new Size2(width, height), this.GetDpiScaling());
        }

        /// <summary>
        /// Called when RenderLoop object checks whether it is possible to render.
        /// </summary>
        bool IRenderLoopHost.OnRenderLoop_CheckCanRender(EngineDevice engineDevice)
        {
            if (_d3dImageSource != null)
            {
                if (!_d3dImageSource.IsFrontBufferAvailable) { return false; }
                if (!_d3dImageSource.HasRenderTarget) { return false; }
            }
            else if (_fallbackWpfImageSource == null) { return false; }

            if (this.Width <= 0) { return false; }
            if (this.Height <= 0) { return false; }

            return true;
        }

        /// <summary>
        /// Called when the render loop prepares rendering.
        /// </summary>
        /// <param name="engineDevice">The engine device.</param>
        void IRenderLoopHost.OnRenderLoop_PrepareRendering(EngineDevice engineDevice)
        {
            if (_lastSizeChange != DateTime.MinValue &&
                DateTime.UtcNow - _lastSizeChange > SeeingSharpConstantsWinWpf.THROTTLED_VIEW_RECREATION_TIME_ON_RESIZE)
            {
                _lastSizeChange = DateTime.MinValue;

                SeeingSharpWpfUtil.GetDpiScalingFactor(this, out var dpiScaleFactorX, out var dpiScaleFactorY);

                // Calculate pixel with and high of this visual
                var pixelSize = new Size(
                    Math.Max(this.RenderSize.Width * dpiScaleFactorX, 100),
                    Math.Max(this.RenderSize.Height * dpiScaleFactorY, 100));
                var width = (int)pixelSize.Width;
                var height = (int)pixelSize.Height;

                if (width > 0 && height > 0)
                {
                    this.RenderLoop.SetCurrentViewSize(width, height);
                }
            }
        }

        /// <summary>
        /// Called when RenderLoop wants to present its results.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_Present(EngineDevice engineDevice)
        {
            if (!this.IsLoaded) { return; }

            if (_d3dImageSource != null)
            {
                // Final checks
                if (!_d3dImageSource.IsFrontBufferAvailable) { return; }
                if (!_d3dImageSource.HasRenderTarget) { return; }

                // Try to lock the D3DImage
                var isLocked = false;
                using (GraphicsCore.Current.PerformanceAnalyzer.Internals.BeginMeasureActivityDuration("Render.Lock"))
                {
                    isLocked = _d3dImageSource.TryLock(MAX_IMAGE_LOCK_DURATION);
                }
                if (!isLocked)
                {
                    _lockImageErrorCount++;
                    if (_lockImageErrorCount > 5) { this.RenderLoop.ForceViewReload(); }
                    return;
                }
                _lockImageErrorCount = 0;

                try
                {
                    // Draw current 3d scene to wpf
                    var deviceContext = engineDevice.Internals.DeviceImmediateContextD3D11;
                    deviceContext.ResolveSubresource(_backBufferD3D11, 0, _backBufferForWpf, 0, Format.B8G8R8A8_UNorm);
                    deviceContext.Flush();
                    deviceContext.ClearState();

                    // Apply true background texture if a cached bitmap was applied before
                    if (this.Source != _d3dImageSource)
                    {
                        this.Source = _d3dImageSource;
                    }

                    // Invalidate the D3D image
                    _d3dImageSource.InvalidateD3DImage();
                }
                finally
                {
                    _d3dImageSource.Unlock();
                }
            }
            else if (_fallbackWpfImageSource != null)
            {
                // Get and read data from the gpu (create copy helper texture on demand)
                if (this.RenderLoop.Internals.CopyHelperTextureStaging == null)
                {
                    this.RenderLoop.Internals.CopyHelperTextureStaging =
                        GraphicsHelper.Internals.CreateStagingTexture(engineDevice, _lastRecreateWidth,
                            _lastRecreateHeight);
                    this.RenderLoop.Internals.CopyHelperTextureStandard =
                        GraphicsHelper.Internals.CreateTexture(engineDevice, _lastRecreateWidth, _lastRecreateHeight);
                }

                // Copy resources
                engineDevice.Internals.DeviceImmediateContextD3D11.ResolveSubresource(
                    this.RenderLoop.Internals.RenderTarget, 0, this.RenderLoop.Internals.CopyHelperTextureStandard, 0,
                    GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT);
                engineDevice.Internals.DeviceImmediateContextD3D11.CopyResource(
                    this.RenderLoop.Internals.CopyHelperTextureStandard,
                    this.RenderLoop.Internals.CopyHelperTextureStaging);

                // Copy texture into wpf bitmap
                GraphicsHelperWpf.LoadBitmapFromStagingTexture(
                    engineDevice, this.RenderLoop.Internals.CopyHelperTextureStaging,
                    _fallbackWpfImageSource,
                    _lastRecreateWidth, _lastRecreateHeight,
                    TimeSpan.FromMilliseconds(500.0));
            }
        }

        /// <summary>
        /// Called when RenderLoop has finished rendering.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_AfterRendering(EngineDevice engineDevice)
        {

        }

        private void OnD3DImageSource_IsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                // Recreate view resources if we got the frontbuffer back
                this.RenderLoop.ForceViewReload();
            }
        }

        /// <summary>
        /// Gets the RenderLoop that is currently in use.
        /// </summary>
        [Browsable(false)]
        public RenderLoop RenderLoop { get; }

        /// <summary>
        /// Does the target control have focus?
        /// </summary>
        public bool Focused => this.IsFocused;

        /// <summary>
        /// Gets or sets the currently applied scene.
        /// </summary>
        public Scene Scene
        {
            get => this.RenderLoop.Scene;
            set => this.RenderLoop.SetScene(value);
        }

        /// <summary>
        /// Gets or sets the camera.
        /// </summary>
        public Camera3DBase Camera
        {
            get => this.RenderLoop.Camera;
            set => this.RenderLoop.Camera = value;
        }

        /// <summary>
        /// Discard rendering?
        /// </summary>
        public bool DiscardRendering
        {
            get => this.RenderLoop.DiscardRendering;
            set => this.RenderLoop.DiscardRendering = value;
        }

        /// <summary>
        /// Gets or sets the clear color of this 3D view.
        /// </summary>
        public System.Windows.Media.Color ClearColor
        {
            get
            {
                var clearColor = this.RenderLoop.ClearColor;
                var result = new System.Windows.Media.Color();
                SeeingSharpWpfUtil.WpfColorFromColor4(ref clearColor, ref result);
                return result;
            }
            set
            {
                var clearColor = new Color4();
                SeeingSharpWpfUtil.Color4FromWpfColor(ref value, ref clearColor);
                this.RenderLoop.ClearColor = clearColor;
            }
        }

        /// <summary>
        /// True if the control is connected with the main rendering loop.
        /// False if something went wrong.
        /// </summary>
        [Browsable(false)]
        public bool IsOperational => this.RenderLoop.IsOperational;

        public EngineDevice SelectedDevice
        {
            get => this.RenderLoop.Device;
            set => this.RenderLoop.SetRenderingDevice(value);
        }

        public GraphicsViewConfiguration Configuration => this.RenderLoop.Configuration;

        public Size CurrentViewSize
        {
            get
            {
                var currentViewSize = this.RenderLoop.CurrentViewSize;

                var result = new Size
                {
                    Width = currentViewSize.Width,
                    Height = currentViewSize.Height
                };

                return result;
            }
        }

        public IEnumerable<EngineDevice> PossibleDevices
        {
            get
            {
                if (!GraphicsCore.IsLoaded)
                {
                    return new EngineDevice[0];
                }

                return GraphicsCore.Current.Devices;
            }
        }

        public WpfSeeingSharpCompositionMode CompositionMode
        {
            get => _compositionMode;
            private set
            {
                if (_compositionMode != value)
                {
                    _compositionMode = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CompositionMode)));
                }
            }
        }

        public bool ForceCompositionOverSoftware
        {
            get => _forceCompositionOverSoftware;
            set
            {
                if (_forceCompositionOverSoftware != value)
                {
                    _forceCompositionOverSoftware = value;
                    this.RenderLoop.ForceViewReload();

                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ForceCompositionOverSoftware)));
                }
            }
        }
    }
}