#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion

#region using

//Some namespace mappings
using D3D11 = SharpDX.Direct3D11;

#endregion

namespace SeeingSharp.Multimedia.Views
{
    #region using

    using System;
    using System.Threading;
    using Windows.Foundation;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;
    using Core;
    using Drawing3D;
    using Input;
    using SeeingSharp.Util;
    using SharpDX;

    #endregion

    // Using SwapChainBackgroundPanel to render to the background of the WinRT app
    //  see http://msdn.microsoft.com/en-us/library/windows/apps/hh825871.aspx
    public partial class SeeingSharpPanelPainter : ISeeingSharpPainter, IDisposable, IInputEnabledView, IRenderLoopHost
    {
        private double MIN_PIXEL_SIZE_WIDTH = 100.0;
        private double MIN_PIXEL_SIZE_HEIGHT = 100.0;

        #region Global members
        private RenderLoop m_renderLoop;
        private bool m_detachOnUnload;
        #endregion

        #region SwapChainBackgroundPanel local members
        private SwapChainPanelWrapper m_targetPanel;
        private Size m_lastRefreshTargetSize;
        private IDisposable m_observerSizeChanged;
        private bool m_compositionScaleChanged;
        private DateTime m_lastSizeChange;
        #endregion

        #region Resources from Direct3D 11
        private SharpDX.DXGI.SwapChain1 m_swapChain;
        private D3D11.Texture2D m_backBuffer;
        private D3D11.Texture2D m_backBufferMultisampled;
        private D3D11.Texture2D m_depthBuffer;
        private D3D11.RenderTargetView m_renderTargetView;
        private D3D11.DepthStencilView m_renderTargetDepth;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SeeingSharpPanelPainter" /> class.
        /// </summary>
        public SeeingSharpPanelPainter()
        {
            m_lastSizeChange = DateTime.MinValue;

            // Create the RenderLoop object
            m_renderLoop = new Core.RenderLoop(SynchronizationContext.Current, this);
            m_renderLoop.ClearColor = Color4Ex.White;
            m_renderLoop.Internals.CallPresentInUIThread = false;

            m_detachOnUnload = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SeeingSharpPanelPainter"/> class.
        /// </summary>
        /// <param name="targetPanel">The target panel.</param>
        public SeeingSharpPanelPainter(SwapChainBackgroundPanel targetPanel)
            : this()
        {
            this.Attach(targetPanel);
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
            this.AttachInternal(new SwapChainPanelWrapper(targetPanel));
        }

        /// <summary>
        /// Attaches the renderer to the given SwapChainBackgroundPanel.
        /// </summary>
        /// <param name="targetPanel">The target panel to attach to.</param>
        public void Attach(SwapChainBackgroundPanel targetPanel)
        {
            this.AttachInternal(new SwapChainPanelWrapper(targetPanel));
        }

        /// <summary>
        /// Detaches the renderer from the current target panel.
        /// </summary>
        public void Detach()
        {
            try
            {
                if (m_targetPanel == null) { return; }

                // Clear view resources
                m_renderLoop.Internals.UnloadViewResources();
                m_renderLoop.DeregisterRenderLoop();

                // Clear event registrations
                m_targetPanel.SizeChanged -= OnTargetPanel_SizeChanged;
                m_targetPanel.Loaded -= OnTargetPanel_Loaded;
                m_targetPanel.Unloaded -= OnTargetPanel_Unloaded;
                m_targetPanel.CompositionScaleChanged -= OnTargetPanel_CompositionScaleChanged;

                //Clear created references
                m_observerSizeChanged.Dispose();
                m_observerSizeChanged = null;
                m_targetPanel.Dispose();
                m_targetPanel = null;
            }
            catch(Exception ex)
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
        /// Attaches the renderer to the given SwapChainBackgroundPanel.
        /// </summary>
        /// <param name="targetPanel">The target panel to attach to.</param>
        private void AttachInternal(SwapChainPanelWrapper targetPanel)
        {
            if (m_targetPanel != null) { throw new InvalidOperationException("Unable to attach to new SwapChainBackgroundPanel: Renderer is already attached to another one!"); }

            m_lastRefreshTargetSize = new Size(0.0, 0.0);
            m_targetPanel = targetPanel;

            //// Attach to SizeChanged event (refresh view resources only after a specific time)
            //m_observerSizeChanged = Observable.FromEventPattern<SizeChangedEventArgs>(m_targetPanel, "SizeChanged")
            //    .Throttle(TimeSpan.FromSeconds(0.5))
            //    .ObserveOn(SynchronizationContext.Current)
            //    .Subscribe((eArgs) => OnTargetPanelThrottled_SizeChanged(eArgs.Sender, eArgs.EventArgs));

            m_targetPanel.SizeChanged += OnTargetPanel_SizeChanged;
            m_targetPanel.Loaded += OnTargetPanel_Loaded;
            m_targetPanel.Unloaded += OnTargetPanel_Unloaded;
            m_targetPanel.CompositionScaleChanged += OnTargetPanel_CompositionScaleChanged;

            UpdateRenderLoopViewSize();

            // Define unloading behavior
            if (VisualTreeHelper.GetParent(m_targetPanel.Panel) != null)
            {
                m_renderLoop.RegisterRenderLoop();
            }
        }

        /// <summary>
        /// Gets the current target pixel size for the render panel.
        /// </summary>
        private Size2 GetTargetRenderPixelSize()
        {
            if (m_targetPanel == null) { return new Size2((int)MIN_PIXEL_SIZE_WIDTH, (int)MIN_PIXEL_SIZE_HEIGHT); }

            double currentWidth = m_targetPanel.ActualWidth * m_targetPanel.CompositionScaleX;
            double currentHeight = m_targetPanel.ActualHeight * m_targetPanel.CompositionScaleY;

            return new Size2(
                (int)(currentWidth > MIN_PIXEL_SIZE_WIDTH ? currentWidth : MIN_PIXEL_SIZE_WIDTH),
                (int)(currentHeight > MIN_PIXEL_SIZE_HEIGHT ? currentHeight : MIN_PIXEL_SIZE_HEIGHT));
        }

        /// <summary>
        /// Update current configured view size on the render loop.
        /// </summary>
        private void UpdateRenderLoopViewSize()
        {
            Size2 viewSize = GetTargetRenderPixelSize();
            m_renderLoop.Camera.SetScreenSize(viewSize.Width, viewSize.Height);
            m_renderLoop.SetCurrentViewSize(
                (int)viewSize.Width,
                (int)viewSize.Height);
        }

        /// <summary>
        /// Called when the target panel gets unloaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnTargetPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            m_renderLoop.DeregisterRenderLoop();

            // Trigger detach if requested
            if(m_detachOnUnload)
            {
                Detach();
            }
        }

        /// <summary>
        /// Called when the target panel gets loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnTargetPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (!m_renderLoop.IsRegisteredOnMainLoop)
            {
                m_renderLoop.RegisterRenderLoop();
            }
        }

        /// <summary>
        /// Called when the size of the target panel has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Windows.UI.Xaml.SizeChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnTargetPanelThrottled_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (!GraphicsCore.IsLoaded) { return; }

                // Ignore event, if nothing has changed..
                Size2 actSize = GetTargetRenderPixelSize();
                if (((int)m_lastRefreshTargetSize.Width == (int)actSize.Width) &&
                    ((int)m_lastRefreshTargetSize.Height == (int)actSize.Height)) 
                { 
                    return; 
                }

                UpdateRenderLoopViewSize();
            }
            catch (Exception ex)
            {
                throw new SeeingSharpException($"Error during resize of the view: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Called when the size of the target panel has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
        private void OnTargetPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!GraphicsCore.IsLoaded) { return; }

            //Resize render target only on greater size changes
            Size2 viewSize = GetTargetRenderPixelSize();
            double resizeFactorWidth = (double)viewSize.Width > m_lastRefreshTargetSize.Width ? (double)viewSize.Width / m_lastRefreshTargetSize.Width : m_lastRefreshTargetSize.Width / (double)viewSize.Width;
            double resizeFactorHeight = (double)viewSize.Height > m_lastRefreshTargetSize.Height ? (double)viewSize.Height / m_lastRefreshTargetSize.Height : m_lastRefreshTargetSize.Height / (double)viewSize.Height;
            if ((resizeFactorWidth > 1.3) || (resizeFactorHeight > 1.3))
            {
                UpdateRenderLoopViewSize();
            }

            m_lastSizeChange = DateTime.UtcNow;
        }

        /// <summary>
        /// Some configuration like 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTargetPanel_CompositionScaleChanged(object sender, EventArgs e)
        {
            this.UpdateRenderLoopViewSize();

            m_compositionScaleChanged = true;
        }

        /// <summary>
        /// Disposes all loaded view resources.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_DisposeViewResources(EngineDevice engineDevice)
        {
            m_renderTargetDepth = SeeingSharpTools.DisposeObject(m_renderTargetDepth);
            m_depthBuffer = SeeingSharpTools.DisposeObject(m_depthBuffer);
            m_renderTargetView = SeeingSharpTools.DisposeObject(m_renderTargetView);
            m_backBuffer = SeeingSharpTools.DisposeObject(m_backBuffer);
            m_backBufferMultisampled = SeeingSharpTools.DisposeObject(m_backBufferMultisampled);
            m_swapChain = SeeingSharpTools.DisposeObject(m_swapChain);
        }

        /// <summary>
        /// Create all view resources.
        /// </summary>
        Tuple<D3D11.Texture2D, D3D11.RenderTargetView, D3D11.Texture2D, D3D11.DepthStencilView, SharpDX.Mathematics.Interop.RawViewportF, Size2, DpiScaling> IRenderLoopHost.OnRenderLoop_CreateViewResources(EngineDevice engineDevice)
        {
            m_backBufferMultisampled = null;

            Size2 viewSize = GetTargetRenderPixelSize();

            // Create the SwapChain and associate it with the SwapChainBackgroundPanel 
            m_swapChain = GraphicsHelperUwp.CreateSwapChainForComposition(engineDevice, viewSize.Width, viewSize.Height, m_renderLoop.ViewConfiguration);
            m_targetPanel.SwapChain = m_swapChain;
            m_compositionScaleChanged = true;

            // Get the backbuffer from the SwapChain
            m_backBuffer = D3D11.Resource.FromSwapChain<D3D11.Texture2D>(m_swapChain, 0);

            // Define the render target (in case of multisample an own render target)
            D3D11.Texture2D backBufferForRenderloop = null;
            if (m_renderLoop.ViewConfiguration.AntialiasingEnabled)
            {
                m_backBufferMultisampled = GraphicsHelper.CreateRenderTargetTexture(engineDevice, viewSize.Width, viewSize.Height, m_renderLoop.ViewConfiguration);
                m_renderTargetView = new D3D11.RenderTargetView(engineDevice.DeviceD3D11_1, m_backBufferMultisampled);
                backBufferForRenderloop = m_backBufferMultisampled;
            }
            else
            {
                m_renderTargetView = new D3D11.RenderTargetView(engineDevice.DeviceD3D11_1, m_backBuffer);
                backBufferForRenderloop = m_backBuffer;
            }

            //Create the depth buffer
            m_depthBuffer = GraphicsHelper.CreateDepthBufferTexture(engineDevice, viewSize.Width, viewSize.Height, m_renderLoop.ViewConfiguration);
            m_renderTargetDepth = new D3D11.DepthStencilView(engineDevice.DeviceD3D11_1, m_depthBuffer);

            //Define the viewport for rendering
            SharpDX.Mathematics.Interop.RawViewportF viewPort = GraphicsHelper.CreateDefaultViewport(viewSize.Width, viewSize.Height);
            m_lastRefreshTargetSize = new Size(viewSize.Width, viewSize.Height);

            DpiScaling dpiScaling = new DpiScaling();
            dpiScaling.DpiX = (float)(96.0 * m_targetPanel.CompositionScaleX);
            dpiScaling.DpiY = (float)(96.0 * m_targetPanel.CompositionScaleY);

            return Tuple.Create(backBufferForRenderloop, m_renderTargetView, m_depthBuffer, m_renderTargetDepth, viewPort, viewSize, dpiScaling);
        }

        /// <summary>
        /// Called when RenderLoop object checks wheter it is possible to render.
        /// </summary>
        bool IRenderLoopHost.OnRenderLoop_CheckCanRender(EngineDevice engineDevice)
        {
            if (m_targetPanel == null) { return false; }
            if (m_targetPanel.ActualWidth <= 0) { return false; }
            if (m_targetPanel.ActualHeight <= 0) { return false; }

            return true;
        }

        void IRenderLoopHost.OnRenderLoop_PrepareRendering(EngineDevice engineDevice)
        {
            if (m_targetPanel == null){ return; }
            if (m_renderLoop == null){ return; }

            // Update swap chain scaling (only relevant for SwapChainPanel targets)
            //  see https://www.packtpub.com/books/content/integrating-direct3d-xaml-and-windows-81
            if ((m_renderLoop.Camera != null) &&
                (m_swapChain != null))
            {
                if (m_compositionScaleChanged &&
                    m_targetPanel.CompositionRescalingNeeded)
                {
                    m_compositionScaleChanged = false;
                    SharpDX.DXGI.SwapChain2 swapChain2 = m_swapChain.QueryInterfaceOrNull<SharpDX.DXGI.SwapChain2>();
                    if (swapChain2 != null)
                    {
                        try
                        {
                            SharpDX.Mathematics.Interop.RawMatrix3x2 inverseScale = new SharpDX.Mathematics.Interop.RawMatrix3x2();
                            inverseScale.M11 = 1.0f / (float)m_targetPanel.CompositionScaleX;
                            inverseScale.M22 = 1.0f / (float)m_targetPanel.CompositionScaleY;
                            swapChain2.MatrixTransform = inverseScale;
                        }
                        finally
                        {
                            swapChain2.Dispose();
                        }
                    }
                }
            }

            // Handle thottled resizing of view resources
            if ((m_lastSizeChange != DateTime.MinValue) &&
                (DateTime.UtcNow - m_lastSizeChange > SeeingSharpConstantsUwp.THROTTLED_VIEW_RECREATION_TIME_ON_RESIZE))
            {
                m_lastSizeChange = DateTime.MinValue;
                UpdateRenderLoopViewSize();
            }
        }

        /// <summary>
        /// Called when RenderLoop wants to present its results.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_Present(EngineDevice engineDevice)
        {
            // Copy contents of the backbuffer if in multisampling mode
            if (m_backBufferMultisampled != null)
            {
                engineDevice.DeviceImmediateContextD3D11.ResolveSubresource(m_backBufferMultisampled, 0, m_backBuffer, 0, GraphicsHelper.DEFAULT_TEXTURE_FORMAT);
            }

            // Present all rendered stuff on screen
            // First parameter indicates synchronization with vertical blank
            //  see http://msdn.microsoft.com/en-us/library/windows/desktop/bb174576(v=vs.85).aspx
            //  see example http://msdn.microsoft.com/en-us/library/windows/apps/hh825871.aspx
            m_swapChain.Present(1, SharpDX.DXGI.PresentFlags.None);
        }

        /// <summary>
        /// Called when RenderLoop has finished rendering.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_AfterRendering(EngineDevice engineDevice)
        {

        }

        /// <summary>
        /// Discard rendering?
        /// </summary>
        public bool DiscardRendering
        {
            get => m_renderLoop.DiscardRendering;
            set => m_renderLoop.DiscardRendering = value;
        }

        /// <summary>
        /// Gets the current 3D scene.
        /// </summary>
        public Scene Scene
        {
            get => m_renderLoop.Scene;
            set => m_renderLoop.SetScene(value);
        }

        /// <summary>
        /// Gets or sets the current 3D camera.
        /// </summary>
        public Camera3DBase Camera
        {
            get => m_renderLoop.Camera;
            set => m_renderLoop.Camera = value;
        }

        /// <summary>
        /// Gets current renderloop object.
        /// </summary>
        public RenderLoop RenderLoop => m_renderLoop;

        /// <summary>
        /// Should we detach automatically if the TargetPanel gets unloaded?
        /// </summary>
        public bool DetachOnUnload
        {
            get => m_detachOnUnload;
            set => m_detachOnUnload = value;
        }

        /// <summary>
        /// Is this painter attached to any panel?
        /// </summary>
        public bool IsAttachedToGui => m_targetPanel != null;

        /// <summary>
        /// Gets the current pixel size of the target panel.
        /// </summary>
        public Size2 PixelSize => GetTargetRenderPixelSize();

        public Size2 ActualSize => new Size2((int)m_targetPanel.ActualWidth, (int)m_targetPanel.ActualHeight);

        /// <summary>
        /// Does the target control have focus?
        /// (Return true here if rendering runs, because in winrt we are everytime at fullscreen)
        /// </summary>
        bool IInputEnabledView.Focused => m_renderLoop.IsRegisteredOnMainLoop;

        /// <summary>
        /// Gets or sets the clear color for the 3D view.
        /// </summary>
        public Windows.UI.Color ClearColor
        {
            get
            {
                var clearColor = m_renderLoop.ClearColor;
                return SeeingSharpUwpTools.UIColorFromColor4(ref clearColor);
            }
            set => m_renderLoop.ClearColor = SeeingSharpUwpTools.Color4FromUIColor(ref value);
        }

        public Panel TargetPanel => m_targetPanel?.Panel;

        /// <summary>
        /// True if the control is connected with the main rendering loop.
        /// False if something went wrong.
        /// </summary>
        public bool IsOperational => m_renderLoop.IsOperational;

        public CoreDispatcher Disptacher => m_targetPanel?.Dispatcher;
    }
}