﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SeeingSharp.Core;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Core.Devices;
using SeeingSharp.Drawing3D;
using SeeingSharp.Input;
using SeeingSharp.Util;
using SharpGen.Runtime;
using Vortice.DXGI;
using Vortice.Mathematics;
using D3D11 = Vortice.Direct3D11;
using GDI = System.Drawing;

namespace SeeingSharp.Views
{
    public class SeeingSharpRendererControl : Panel, ISeeingSharpPainter, IInputEnabledView, IRenderLoopHost, IDpiScalingProvider
    {
        private const string TEXT_GRAPHICS_NOT_INITIALIZED = "Graphics not initialized!";

        // Main reference to 3D-Engine
        private RenderLoop _renderLoop;

        // Resources for Direct3D 11
        private IDXGISwapChain1? _swapChain;
        private D3D11.ID3D11Device? _renderDevice;
        private D3D11.ID3D11RenderTargetView? _renderTarget;
        private D3D11.ID3D11DepthStencilView? _renderTargetDepth;
        private D3D11.ID3D11Texture2D? _backBuffer;
        private D3D11.ID3D11Texture2D? _depthBuffer;

        // Generic members
        private GDI.Brush? _backBrush;
        private GDI.Brush? _foreBrushText;
        private GDI.Brush? _backBrushText;
        private GDI.Pen? _borderPen;

        // Misc
        private DateTime _lastSizeChange;
        private Dictionary<MouseButtons, DateTime> _mouseButtonDownTime;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Scene Scene
        {
            get => _renderLoop.Scene;
            set => _renderLoop.SetScene(value);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Camera3DBase Camera
        {
            get => _renderLoop.Camera;
            set => _renderLoop.Camera = value;
        }

        /// <summary>
        /// Gets the render loop object.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RenderLoop RenderLoop => _renderLoop;

        /// <summary>
        /// Discard rendering?
        /// </summary>
        [Browsable(false)]
        [DefaultValue(false)]
        public bool DiscardRendering
        {
            get => _renderLoop.DiscardRendering;
            set => _renderLoop.DiscardRendering = value;
        }

        /// <summary>
        /// Discard present of rendering results on the screen?
        /// </summary>
        [Category(SeeingSharpConstantsWinForms.DESIGNER_CATEGORY_RENDERER)]
        [DefaultValue(false)]
        public bool DiscardPresent { get; set; }

        /// <summary>
        /// Gets the view configuration.
        /// </summary>
        [Browsable(true)]
        [Category(SeeingSharpConstantsWinForms.DESIGNER_CATEGORY_RENDERER)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public GraphicsViewConfiguration Configuration => _renderLoop.Configuration;

        public sealed override GDI.Color BackColor
        {
            get => base.BackColor;
            set
            {
                base.BackColor = value;
                _renderLoop.ClearColor = value.Color4FromGdiColor();
            }
        }

        [Browsable(false)]
        public EngineDevice? Device => _renderLoop.Device;

        /// <summary>
        /// True if the control is connected with the main rendering loop.
        /// False if something went wrong.
        /// </summary>
        [Browsable(false)]
        public bool IsOperational => _renderLoop.IsOperational;

        [Browsable(false)]
        public ViewInformation ViewInformation => this.RenderLoop.ViewInformation;

        /// <summary>
        /// Raises when mouse was down a short amount of time.
        /// </summary>
        public event EventHandler<MouseEventArgs>? MouseClickEx;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SeeingSharp.Views.SeeingSharpRendererControl" /> class.
        /// </summary>
        public SeeingSharpRendererControl()
        {
            // Set style parameters for this control
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
            this.SetStyle(ControlStyles.Opaque, true);
            this.SetStyle(ControlStyles.Selectable, true);
            base.DoubleBuffered = false;

            _lastSizeChange = DateTime.MinValue;
            _mouseButtonDownTime = new Dictionary<MouseButtons, DateTime>();

            // 
            var syncContext = SynchronizationContext.Current;
            if (syncContext == null)
            {
                throw new SeeingSharpException("Unable to determine the current SynchronizationContext!");
            }

            // Create the render loop
            var backColor = this.BackColor;
            _renderLoop = new RenderLoop(syncContext, this, this.DesignMode);
            _renderLoop.ClearColor = backColor.Color4FromGdiColor();
            _renderLoop.DiscardRendering = true;
            _renderLoop.Internals.CallPresentInUIThread = false;
            this.Disposed += (_, _) =>
            {
                _renderLoop.Dispose();
            };

            // Perform default initialization logic (if not called before)
            if (GraphicsCore.IsLoaded)
            {
                _renderLoop.SetScene(new Scene());
                _renderLoop.Camera = new PerspectiveCamera3D();

                //Initialize background brush
                this.UpdateDrawingResourcesForFailoverRendering();
            }

            this.Disposed += this.OnDisposed;

            this.UpdateDrawingResourcesForFailoverRendering();
        }

        /// <inheritdoc />
        public DpiScaling GetCurrentDpiScaling()
        {
            // Win.Forms always uses pixel for rendering
            return DpiScaling.Default;
        }

        /// <summary>
        /// Gets the object on the given location (location local to this control).
        /// </summary>
        /// <param name="pickingOptions">Options for picking logic.</param>
        /// <param name="location">X, Y location of the cursor in DIP.</param>
        public Task<List<SceneObject>?> PickObjectAsync(GDI.Point location, PickingOptions pickingOptions)
        {
            return this.RenderLoop.PickObjectAsync(location, pickingOptions);
        }

        /// <summary>
        /// Saves a screenshot to hard disc.
        /// </summary>
        /// <param name="targetFile">Target file path.</param>
        public async Task SaveScreenshotAsync(string targetFile)
        {
            if (_backBuffer != null)
            {
                var screenshot = await _renderLoop.GetScreenshotGdiAsync();
                screenshot.Save(targetFile);
            }
        }

        /// <summary>
        /// Saves a screenshot to hard disc.
        /// </summary>
        /// <param name="targetFile">Target file path.</param>
        /// <param name="imageFormat">Target file format.</param>
        public async Task SaveScreenshotAsync(string targetFile, ImageFormat imageFormat)
        {
            if (_backBuffer != null)
            {
                var screenshot = await _renderLoop.GetScreenshotGdiAsync();
                screenshot.Save(targetFile, imageFormat);
            }
        }

        /// <summary>
        /// Called when system wants to paint this panel.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!_renderLoop.ViewResourcesLoaded ||
                !_renderLoop.IsRegisteredOnMainLoop ||
                _renderLoop.IsDeviceLost)
            {
                // Paint using System.Drawing
                e.Graphics.FillRectangle(_backBrush!, e.ClipRectangle);

                // Paint a simple grid on the background to have something for the Designer
                if (!GraphicsCore.IsLoaded)
                {
                    var targetSize = e.Graphics.MeasureString(TEXT_GRAPHICS_NOT_INITIALIZED, this.Font);
                    var targetRect = new GDI.RectangleF(
                        10f, 10f, targetSize.Width, targetSize.Height);
                    if (targetRect.Width > 10 &&
                       targetRect.Height > 10)
                    {
                        e.Graphics.FillRectangle(_backBrushText!, targetRect);
                        e.Graphics.DrawString(
                            TEXT_GRAPHICS_NOT_INITIALIZED, this.Font,
                            _foreBrushText!, targetRect.X, targetRect.Y);
                    }
                }

                // Paint a border rectangle
                e.Graphics.DrawRectangle(
                    _borderPen!,
                    new GDI.Rectangle(0, 0, this.Width - 1, this.Height - 1));
            }
        }

        /// <summary>
        /// Called when BackColor property has changed.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);

            //Update background brush
            this.UpdateDrawingResourcesForFailoverRendering();
        }

        /// <summary>
        /// Called when the size of the viewport has changed.
        /// </summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (this.DesignMode) { return; }
            if (!GraphicsCore.IsLoaded) { return; }

            if (this.Width > 0 && this.Height > 0)
            {
                _renderLoop.Camera.SetScreenSize(this.Width, this.Height);
                _lastSizeChange = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Called when the window handle is created.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            this.StartRendering();
        }

        /// <summary>
        /// Called when the window handle is destroyed.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);

            this.StopRendering();
        }

        /// <summary>
        /// Handle changed control visibility.
        /// </summary>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (this.Visible)
            {
                this.StartRendering();
            }
            else if (!this.Visible)
            {
                this.StopRendering();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            foreach (MouseButtons actButton in Enum.GetValues(typeof(MouseButtons)))
            {
                if (((int)e.Button | (int)actButton) != (int)actButton) { continue; }
                if (_mouseButtonDownTime.ContainsKey(actButton)) { continue; }

                _mouseButtonDownTime[actButton] = DateTime.UtcNow;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            foreach (MouseButtons actButton in Enum.GetValues(typeof(MouseButtons)))
            {
                if (((int)e.Button | (int)actButton) != (int)actButton) { continue; }
                if (!_mouseButtonDownTime.ContainsKey(actButton)) { continue; }

                var downTimeStamp = _mouseButtonDownTime[actButton];
                _mouseButtonDownTime.Remove(actButton);

                if (DateTime.UtcNow - downTimeStamp < SeeingSharpConstantsWinForms.MOUSE_CLICK_MAX_TIME)
                {
                    this.MouseClickEx?.Invoke(this, e);
                }
            }
        }

        /// <summary>
        /// Stops rendering.
        /// </summary>
        private void StartRendering()
        {
            if (this.DesignMode) { return; }
            if (!GraphicsCore.IsLoaded) { return; }

            if (!_renderLoop.IsRegisteredOnMainLoop)
            {
                _renderLoop.SetCurrentViewSize(this.Width, this.Height);
                _renderLoop.DiscardRendering = false;
                _renderLoop.RegisterRenderLoop();
            }
        }

        /// <summary>
        /// Stops rendering.
        /// </summary>
        private void StopRendering()
        {
            if (this.DesignMode) { return; }
            if (!GraphicsCore.IsLoaded) { return; }

            if (_renderLoop.IsRegisteredOnMainLoop)
            {
                _renderLoop.DiscardRendering = true;
                _renderLoop.DeregisterRenderLoop();
            }
        }

        /// <summary>
        /// Updates the background brush used for failover rendering in System.Drawing.
        /// </summary>
        private void UpdateDrawingResourcesForFailoverRendering()
        {
            SeeingSharpUtil.SafeDispose(ref _backBrush);
            SeeingSharpUtil.SafeDispose(ref _foreBrushText);
            SeeingSharpUtil.SafeDispose(ref _backBrushText);
            SeeingSharpUtil.SafeDispose(ref _borderPen);

            _backBrush = new HatchBrush(
                HatchStyle.DottedGrid,
                GDI.Color.Gray, this.BackColor);
            _backBrushText = new GDI.SolidBrush(GDI.Color.White);
            _foreBrushText = new GDI.SolidBrush(GDI.Color.Black);
            _borderPen = new GDI.Pen(GDI.Color.DarkGray);
        }

        /// <summary>
        /// Called when this view gets disposed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnDisposed(object? sender, EventArgs e)
        {
            if (!this.DesignMode)
            {
                _renderLoop.Dispose();
            }
        }

        /// <summary>
        /// Create all view resources.
        /// </summary>
        Tuple<D3D11.ID3D11Texture2D, D3D11.ID3D11RenderTargetView, D3D11.ID3D11Texture2D, D3D11.ID3D11DepthStencilView, Viewport, GDI.Size, DpiScaling> IRenderLoopHost.OnRenderLoop_CreateViewResources(EngineDevice device)
        {
            var width = this.Width;
            var height = this.Height;
            if (width <= SeeingSharpConstants.MIN_VIEW_WIDTH) { width = SeeingSharpConstants.MIN_VIEW_WIDTH; }
            if (height <= SeeingSharpConstants.MIN_VIEW_HEIGHT) { height = SeeingSharpConstants.MIN_VIEW_HEIGHT; }

            // Get all devices
            _renderDevice = device.Internals.DeviceD3D11_1;

            // Create the swap chain and the render target
            _swapChain = GraphicsHelperWinForms.CreateSwapChainForWinForms(this, device, _renderLoop.Configuration);
            _backBuffer = _swapChain.GetBuffer<D3D11.ID3D11Texture2D>(0);
            _renderTarget = _renderDevice.CreateRenderTargetView(_backBuffer);

            // Create the depth buffer
            _depthBuffer = GraphicsHelper.Internals.CreateDepthBufferTexture(device, width, height, _renderLoop.Configuration);
            _renderTargetDepth = _renderDevice.CreateDepthStencilView(_depthBuffer);

            // Define the viewport for rendering
            var viewPort = GraphicsHelper.Internals.CreateDefaultViewport(width, height);

            // Query for current dpi value
            var dpiScaling = DpiScaling.Default;

            using (var graphics = this.CreateGraphics())
            {
                dpiScaling.DpiX = graphics.DpiX;
                dpiScaling.DpiY = graphics.DpiY;
            }

            // Return all generated objects
            return Tuple.Create(_backBuffer, _renderTarget, _depthBuffer, _renderTargetDepth, viewPort, new GDI.Size(width, height), dpiScaling);
        }

        /// <summary>
        /// Disposes all loaded view resources.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_DisposeViewResources(EngineDevice device)
        {
            _renderDevice = null;

            _renderTargetDepth = SeeingSharpUtil.DisposeObject(_renderTargetDepth);
            _depthBuffer = SeeingSharpUtil.DisposeObject(_depthBuffer);
            _renderTarget = SeeingSharpUtil.DisposeObject(_renderTarget);
            _backBuffer = SeeingSharpUtil.DisposeObject(_backBuffer);
            _swapChain = SeeingSharpUtil.DisposeObject(_swapChain);
        }

        /// <summary>
        /// Called when RenderLoop object checks whether it is possible to render.
        /// </summary>
        bool IRenderLoopHost.OnRenderLoop_CheckCanRender(EngineDevice device)
        {
            // Check properties on self
            if (this.IsDisposed) { return false; }
            if (this.Parent == null) { return false; }
            if (!this.Visible) { return false; }
            if (this.Width <= 0) { return false; }
            if (this.Height <= 0) { return false; }

            // Find parent form
            Form? parentForm = null;
            var actParent = this.Parent;
            while (parentForm == null && actParent != null)
            {
                parentForm = actParent as Form;
                actParent = actParent.Parent;
            }

            // Find top level parent
            Control topLevelParent = this;
            while (topLevelParent.Parent != null)
            {
                topLevelParent = topLevelParent.Parent;
            }

            // Check parent form
            if (parentForm == null)
            {
                // In this case we may be hosted inside a wpf environment
                if (topLevelParent.IsDisposed) { return false; }
                if (!topLevelParent.Visible) { return false;}

                return true;
            }
            if (parentForm.WindowState == FormWindowState.Minimized) { return false; }

            return true;
        }

        /// <summary>
        /// Called when the render loop prepares rendering.
        /// </summary>
        /// <param name="device">The current rendering device.</param>
        void IRenderLoopHost.OnRenderLoop_PrepareRendering(EngineDevice device)
        {
            if (_lastSizeChange != DateTime.MinValue &&
               DateTime.UtcNow - _lastSizeChange > SeeingSharpConstantsWinForms.THROTTLED_VIEW_RECREATION_TIME_ON_RESIZE)
            {
                _lastSizeChange = DateTime.MinValue;

                var width = this.Width;
                var height = this.Height;
                if (width > 0 && height > 0)
                {
                    _renderLoop.SetCurrentViewSize(this.Width, this.Height);
                }
            }
        }

        /// <summary>
        /// Called when RenderLoop wants to present its results.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_Present(EngineDevice device)
        {
            if (this.DiscardPresent) { return; }

            //Present all rendered stuff on screen
            try
            {
                _swapChain!.Present(0, PresentFlags.DoNotWait, new PresentParameters());
            }
            catch (SharpGenException ex)
            {
                // Skip present on error DXGI_ERROR_WAS_STILL_DRAWING
                // This error occurs some times on slower hardware
                if (ex.ResultCode == ResultCode.WasStillDrawing) { return; }

                throw;
            }
        }

        /// <summary>
        /// Called when RenderLoop has finished rendering.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_AfterRendering(EngineDevice device)
        {
            //_isOnRendering = false;
            if (!this.Visible)
            {
                this.StopRendering();
            }
        }
    }
}