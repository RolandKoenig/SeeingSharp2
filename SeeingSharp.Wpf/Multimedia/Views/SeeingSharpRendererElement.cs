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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Input;
using SeeingSharp.Util;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using Color = System.Windows.Media.Color;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Views
{
    public class SeeingSharpRendererElement : Image, IInputEnabledView, ISeeingSharpPainter, IRenderLoopHost, INotifyPropertyChanged
    {
        // Dependency properties
        public static readonly DependencyProperty SceneProperty =
            DependencyProperty.Register(nameof(Scene), typeof(Scene), typeof(SeeingSharpRendererElement), new PropertyMetadata(new Scene()));
        public static readonly DependencyProperty CameraProperty =
            DependencyProperty.Register(nameof(Camera), typeof(Camera3DBase), typeof(SeeingSharpRendererElement), new PropertyMetadata(new PerspectiveCamera3D()));
        public static readonly DependencyProperty DrawingLayer2DProperty =
            DependencyProperty.Register(nameof(DrawingLayer2D), typeof(Custom2DDrawingLayer), typeof(SeeingSharpRendererElement), new PropertyMetadata(null));

        private static Duration MAX_IMAGE_LOCK_DURATION = new Duration(TimeSpan.FromMilliseconds(100.0));

        // Some members..
        private HigherD3DImageSource m_d3dImageSource;
        private WriteableBitmap m_fallbackWpfImageSource;
        private int m_lastRecreateWidth;
        private int m_lastRecreateHeight;

        // All needed direct3d resources
        private D3D11.Texture2D m_backBufferForWpf;
        private D3D11.Texture2D m_backBufferD3D11;
        private D3D11.Texture2D m_depthBuffer;
        private D3D11.RenderTargetView m_renderTarget;
        private D3D11.DepthStencilView m_renderTargetDepth;
        private Surface m_renderTarget2DDxgi;

        // Some size related properties
        private int m_renderTargetHeight;
        private int m_renderTargetWidth;
        private int m_viewportHeight;
        private int m_viewportWidth;
        private DateTime m_lastSizeChange;
        private WpfSeeingSharpCompositionMode m_compositionMode;
        private bool m_forceCompositionOverSoftware;

        // State members for handling rendering problems
        private int m_isDirtyCount;
        public event EventHandler CameraChanged;
        public event EventHandler DrawingLayer2DChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        // Change events
        public event EventHandler SceneChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeeingSharpRendererElement"/> class.
        /// </summary>
        public SeeingSharpRendererElement()
        {
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;

            m_compositionMode = WpfSeeingSharpCompositionMode.None;

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

            this.RenderLoop.ClearColor = Color4Ex.Transparent;
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

            if (source != null)
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
            PresentationSource source = PresentationSource.FromVisual(this);
            double dpiScaleFactorX = 1.0;
            double dpiScaleFactorY = 1.0;
            if (source != null)
            {
                dpiScaleFactorX = source.CompositionTarget.TransformToDevice.M11;
                dpiScaleFactorY = source.CompositionTarget.TransformToDevice.M22;
            }

            return new Size(
                Math.Max(this.RenderSize.Width * dpiScaleFactorX, 100),
                Math.Max(this.RenderSize.Height * dpiScaleFactorY, 100));
        }

        /// <summary>
        /// Transporms the given wpf point to pure pixel coordinates.
        /// </summary>
        /// <param name="wpfPoint">The wpf point to be transformed.</param>
        public System.Windows.Point GetPixelLocation(System.Windows.Point wpfPoint)
        {
            PresentationSource source = PresentationSource.FromVisual(this);
            double dpiScaleFactorX = 1.0;
            double dpiScaleFactorY = 1.0;
            if (source != null)
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
            this.RenderLoop.Camera.SetScreenSize((int) this.RenderSize.Width, (int) this.RenderSize.Height);

            //Resize render target only on greater size changes
            var resizeFactorWidth = sizeInfo.NewSize.Width > m_renderTargetWidth ? sizeInfo.NewSize.Width / m_renderTargetWidth : m_renderTargetWidth / sizeInfo.NewSize.Width;
            var resizeFactorHeight = sizeInfo.NewSize.Height > m_renderTargetHeight ? sizeInfo.NewSize.Height / m_renderTargetHeight : m_renderTargetHeight / sizeInfo.NewSize.Height;
            if (resizeFactorWidth > 1.3 || resizeFactorHeight > 1.3)
            {
                this.RenderLoop.SetCurrentViewSize((int) this.RenderSize.Width, (int) this.RenderSize.Height);
            }

            m_lastSizeChange = DateTime.UtcNow;
        }

        /// <summary>
        /// Called when one of the dependency properties has changed.
        /// </summary>
        protected override async void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (!GraphicsCore.IsLoaded) { return; }
            if (this.IsInDesignMode()) { return; }

            if (e.Property == SceneProperty)
            {
                this.RenderLoop.SetScene(this.Scene);
                this.SceneChanged?.Invoke(this, EventArgs.Empty);
            }
            else if (e.Property == CameraProperty)
            {
                this.RenderLoop.Camera = this.Camera;
                this.CameraChanged?.Invoke(this, EventArgs.Empty);
            }
            else if (e.Property == DrawingLayer2DProperty)
            {
                if (e.OldValue != null) { await this.RenderLoop.Deregister2DDrawingLayerAsync(e.OldValue as Custom2DDrawingLayer); }
                if (e.NewValue != null) { await this.RenderLoop.Register2DDrawingLayerAsync(e.NewValue as Custom2DDrawingLayer); }
                this.DrawingLayer2DChanged?.Invoke(this, EventArgs.Empty);
            }
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
            this.RenderLoop.Camera.SetScreenSize((int) this.RenderSize.Width, (int) this.RenderSize.Height);

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
        /// Called when the current session was switched.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SessionSwitchEventArgs"/> instance containing the event data.</param>
        private void OnSystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (this.RenderLoop == null) { return; }
            if (this.IsInDesignMode()) { return; }

            switch (e.Reason)
            {
                    // Handle session lock/unload events
                    //  => Force recreation of view resources in that case
                case SessionSwitchReason.SessionUnlock:
                case SessionSwitchReason.SessionLock:
                case SessionSwitchReason.SessionLogon:
                    if (this.RenderLoop.IsRegisteredOnMainLoop)
                    {
                        this.RenderLoop.ViewConfiguration.ViewNeedsRefresh = true;
                    }
                    break;
            }
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
            if (m_d3dImageSource != null)
            {
                // Dispose the render target
                m_d3dImageSource.SetRenderTarget(null);
                m_d3dImageSource.Dispose();
                m_d3dImageSource = null;
            }

            // Dispose all other resources
            m_renderTarget2DDxgi = SeeingSharpUtil.DisposeObject(m_renderTarget2DDxgi);
            m_renderTargetDepth = SeeingSharpUtil.DisposeObject(m_renderTargetDepth);
            m_depthBuffer = SeeingSharpUtil.DisposeObject(m_depthBuffer);
            m_renderTarget = SeeingSharpUtil.DisposeObject(m_renderTarget);
            m_backBufferForWpf = SeeingSharpUtil.DisposeObject(m_backBufferForWpf);
            m_backBufferD3D11 = SeeingSharpUtil.DisposeObject(m_backBufferD3D11);

            // Reset composition mode
            this.CompositionMode = WpfSeeingSharpCompositionMode.None;
        }

        /// <summary>
        /// Create all view resources.
        /// </summary>
        Tuple<D3D11.Texture2D, D3D11.RenderTargetView, D3D11.Texture2D, D3D11.DepthStencilView, RawViewportF, Size2, DpiScaling> IRenderLoopHost.OnRenderLoop_CreateViewResources(EngineDevice engineDevice)
        {
            SeeingSharpWpfUtil.GetDpiScalingFactor(this, out var dpiScaleFactorX, out var dpiScaleFactorY);

            // Calculate pixel with and high of this visual
            var pixelSize = new Size(
                Math.Max(this.RenderSize.Width * dpiScaleFactorX, 100),
                Math.Max(this.RenderSize.Height * dpiScaleFactorY, 100));
            var width = (int)pixelSize.Width;
            var height = (int)pixelSize.Height;

            // Get references to current render device
            D3D11.Device renderDevice = engineDevice.DeviceD3D11_1;
            var renderDeviceContext = renderDevice.ImmediateContext;
            var viewPort = default(RawViewportF);

            var initializedSuccessfully = false;
            var forceFallbackSolution = m_forceCompositionOverSoftware;
            var tryCount = 0;

            do
            {
                tryCount++;
                if(tryCount > 2)
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
                            m_d3dImageSource = new HigherD3DImageSource(engineDevice, handlerD3D9);
                        }
                        catch (Exception)
                        {
                            // Unable to enable surface sharing
                        }
                    }
                }

                // Switch to fallback method if we can't create the HigherD3DImageSource
                if (m_d3dImageSource == null)
                {
                    m_d3dImageSource = null;
                    m_fallbackWpfImageSource = new WriteableBitmap(width, height, 96.0 * dpiScaleFactorX, 96.0 * dpiScaleFactorY, PixelFormats.Bgra32, BitmapPalettes.WebPaletteTransparent);
                }

                // Create the swap chain and the render target
                m_backBufferD3D11 = GraphicsHelper.CreateRenderTargetTexture(engineDevice, width, height, this.RenderLoop.ViewConfiguration);
                m_backBufferForWpf = GraphicsHelper.CreateSharedTexture(engineDevice, width, height);
                m_renderTarget = new D3D11.RenderTargetView(renderDevice, m_backBufferD3D11);

                // Create the depth buffer
                m_depthBuffer = GraphicsHelper.CreateDepthBufferTexture(engineDevice, width, height, this.RenderLoop.ViewConfiguration);
                m_renderTargetDepth = new D3D11.DepthStencilView(renderDevice, m_depthBuffer);

                // Apply render target size values
                m_renderTargetWidth = width;
                m_renderTargetHeight = height;

                // Define the viewport for rendering
                viewPort = GraphicsHelper.CreateDefaultViewport(width, height);

                // Apply new width and height values of the viewport
                m_viewportWidth = width;
                m_viewportHeight = height;

                if (m_d3dImageSource != null)
                {
                    // Create and apply the image source object
                    try
                    {
                        m_d3dImageSource.SetRenderTarget(m_backBufferForWpf);
                        this.Source = m_d3dImageSource;
                        initializedSuccessfully = true;
                        this.CompositionMode = WpfSeeingSharpCompositionMode.OverHardware;
                    }
                    catch(Exception)
                    {
                        initializedSuccessfully = false;
                        forceFallbackSolution = true;

                        SeeingSharpUtil.SafeDispose(ref m_d3dImageSource);
                        SeeingSharpUtil.SafeDispose(ref m_renderTargetDepth);
                        SeeingSharpUtil.SafeDispose(ref m_depthBuffer);
                        SeeingSharpUtil.SafeDispose(ref m_renderTarget);
                        SeeingSharpUtil.SafeDispose(ref m_backBufferForWpf);
                        SeeingSharpUtil.SafeDispose(ref m_backBufferD3D11);
                    }
                }
                else
                {
                    // Set a dummy image source
                    this.Source = m_fallbackWpfImageSource;
                    initializedSuccessfully = true;
                    this.CompositionMode = WpfSeeingSharpCompositionMode.FallbackOverSoftware;
                }
            }
            while (!initializedSuccessfully);

            m_lastRecreateWidth = width;
            m_lastRecreateHeight = height;

            // Return all generated objects
            return Tuple.Create(m_backBufferD3D11, m_renderTarget, m_depthBuffer, m_renderTargetDepth, viewPort, new Size2(width, height), this.GetDpiScaling());
        }

        /// <summary>
        /// Called when RenderLoop object checks whether it is possible to render.
        /// </summary>
        bool IRenderLoopHost.OnRenderLoop_CheckCanRender(EngineDevice engineDevice)
        {
            if (m_d3dImageSource != null)
            {
                if (!m_d3dImageSource.IsFrontBufferAvailable) { return false; }
                if (!m_d3dImageSource.HasRenderTarget) { return false; }
            }
            else if(m_fallbackWpfImageSource == null) { return false; }

            if (this.Width <= 0) { return false; }
            if (this.Height <= 0) { return false; }

            if (m_d3dImageSource != null)
            {
                if (SeeingSharpWpfUtil.ReadPrivateMember<bool, D3DImage>(m_d3dImageSource, "_isDirty") ||
                    SeeingSharpWpfUtil.ReadPrivateMember<IntPtr, D3DImage>(m_d3dImageSource, "_pUserSurfaceUnsafe") == IntPtr.Zero)
                {
                    m_isDirtyCount++;
                    if (m_isDirtyCount > 20)
                    {
                        this.RenderLoop.ViewConfiguration.ViewNeedsRefresh = true;
                        return true;
                    }
                    return false;
                }
                m_isDirtyCount = 0;
            }

            return true;
        }

        /// <summary>
        /// Called when the render loop prepares rendering.
        /// </summary>
        /// <param name="engineDevice">The engine device.</param>
        void IRenderLoopHost.OnRenderLoop_PrepareRendering(EngineDevice engineDevice)
        {
            if (m_lastSizeChange != DateTime.MinValue &&
                DateTime.UtcNow - m_lastSizeChange > SeeingSharpConstantsWinWpf.THROTTLED_VIEW_RECREATION_TIME_ON_RESIZE)
            {
                m_lastSizeChange = DateTime.MinValue;

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

            if (m_d3dImageSource != null)
            {
                var isLocked = false;
                GraphicsCore.Current.PerformanceAnalyzer.ExecuteAndMeasureActivityDuration(
                    "Render.Lock",
                    () => isLocked = m_d3dImageSource.TryLock(MAX_IMAGE_LOCK_DURATION));

                if (!isLocked)
                {
                    return;
                }

                try
                {
                    // Draw current 3d scene to wpf
                    var deviceContext = engineDevice.DeviceImmediateContextD3D11;
                    deviceContext.ResolveSubresource(m_backBufferD3D11, 0, m_backBufferForWpf, 0, Format.B8G8R8A8_UNorm);
                    deviceContext.Flush();
                    deviceContext.ClearState();

                    // Apply true background texture if a cached bitmap was applied before
                    if (this.Source != m_d3dImageSource)
                    {
                        this.Source = m_d3dImageSource;
                    }

                    // Invalidate the D3D image
                    m_d3dImageSource.InvalidateD3DImage();
                }
                finally
                {
                    m_d3dImageSource.Unlock();
                }
            }
            else if(m_fallbackWpfImageSource != null)
            {
                // Get and read data from the gpu (create copy helper texture on demand)
                if (this.RenderLoop.Internals.CopyHelperTextureStaging == null)
                {
                    this.RenderLoop.Internals.CopyHelperTextureStaging = GraphicsHelper.CreateStagingTexture(engineDevice, m_lastRecreateWidth, m_lastRecreateHeight);
                    this.RenderLoop.Internals.CopyHelperTextureStandard = GraphicsHelper.CreateTexture(engineDevice, m_lastRecreateWidth, m_lastRecreateHeight);
                }

                // Copy resources
                engineDevice.DeviceImmediateContextD3D11.ResolveSubresource(this.RenderLoop.Internals.RenderTarget, 0, this.RenderLoop.Internals.CopyHelperTextureStandard, 0,
                    GraphicsHelper.DEFAULT_TEXTURE_FORMAT);
                engineDevice.DeviceImmediateContextD3D11.CopyResource(this.RenderLoop.Internals.CopyHelperTextureStandard, this.RenderLoop.Internals.CopyHelperTextureStaging);

                // Copy texture into wpf bitmap
                GraphicsHelperWpf.LoadBitmapFromStagingTexture(
                    engineDevice, this.RenderLoop.Internals.CopyHelperTextureStaging,
                    m_fallbackWpfImageSource,
                    TimeSpan.FromMilliseconds(500.0));
            }
        }

        /// <summary>
        /// Called when RenderLoop has finished rendering.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_AfterRendering(EngineDevice engineDevice)
        {

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
            get => (Scene) this.GetValue(SceneProperty);
            set => this.SetValue(SceneProperty, value);
        }

        /// <summary>
        /// Gets or sets the camera.
        /// </summary>
        public Camera3DBase Camera
        {
            get => (Camera3DBase) this.GetValue(CameraProperty);
            set => this.SetValue(CameraProperty, value);
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
        public Color ClearColor
        {
            get
            {
                var clearColor = this.RenderLoop.ClearColor;
                var result = new Color();
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
            get => m_compositionMode;
            private set
            {
                if(m_compositionMode != value)
                {
                    m_compositionMode = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CompositionMode)));
                }
            }
        }

        public bool ForceCompositionOverSoftware
        {
            get => m_forceCompositionOverSoftware;
            set
            {
                if (m_forceCompositionOverSoftware != value)
                {
                    m_forceCompositionOverSoftware = value;
                    this.RenderLoop.ForceViewReload();

                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ForceCompositionOverSoftware)));
                }
            }
        }

        /// <summary>
        /// Gets or sets the custom layer for 2D rendering.
        /// </summary>
        public Custom2DDrawingLayer DrawingLayer2D
        {
            get => (Custom2DDrawingLayer) this.GetValue(DrawingLayer2DProperty);
            set => this.SetValue(DrawingLayer2DProperty, value);
        }
    }
}