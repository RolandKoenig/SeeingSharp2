#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SeeingSharp;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Input;
using SeeingSharp.Multimedia.Views;
using SeeingSharp.Util;
using SharpDX;

//Some namespace mappings
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using GDI = System.Drawing;

namespace SeeingSharp.Desktop
{
    public partial class SeeingSharpRendererControl : Panel, ISeeingSharpPainter, IInputEnabledView, IRenderLoopHost
    {
        private const string TEXT_GRAPHICS_NOT_INITIALIZED = "Graphics not initialized!";

        #region Main reference to 3D-Engine
        private RenderLoop m_renderLoop;
        #endregion Main reference to 3D-Engine

        #region Resources for Direct3D 11
        private DXGI.Factory m_factory;
        private DXGI.SwapChain1 m_swapChain;
        private D3D11.Device m_renderDevice;
        private D3D11.DeviceContext m_renderDeviceContext;
        private D3D11.RenderTargetView m_renderTarget;
        private D3D11.DepthStencilView m_renderTargetDepth;
        private D3D11.Texture2D m_backBuffer;
        private D3D11.Texture2D m_depthBuffer;
        #endregion Resources for Direct3D 11

        #region Members for input handling
        private bool m_isMouseInside;
        #endregion Members for input handling

        #region Generic members
        private Brush m_backBrush;
        private Brush m_foreBrushText;
        private Brush m_backBrushText;
        private Pen m_borderPen;
        #endregion Generic members

        /// <summary>
        /// Raised when it is possible for the UI thread to manipulate current filter list.
        /// </summary>
        public event EventHandler<ManipulateFilterListArgs> ManipulateFilterList;

        /// <summary>
        /// Raises when mouse was down a short amount of time.
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseClickEx;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeeingSharpRendererControl"/> class.
        /// </summary>
        public SeeingSharpRendererControl()
        {
            //Set style parameters for this control
            base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            base.SetStyle(ControlStyles.ResizeRedraw, true);
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
            base.SetStyle(ControlStyles.Opaque, true);
            base.SetStyle(ControlStyles.Selectable, true);
            base.DoubleBuffered = false;

            //Create the render loop
            GDI.Color backColor = this.BackColor;
            m_renderLoop = new RenderLoop(SynchronizationContext.Current, this, this.DesignMode);
            m_renderLoop.ManipulateFilterList += OnRenderLoopManipulateFilterList;
            m_renderLoop.ClearColor = backColor.Color4FromGdiColor();
            m_renderLoop.DiscardRendering = true;
            this.Disposed += (sender, eArgs) =>
            {
                m_renderLoop.Dispose();
            };

            // Perform default initialization logic (if not called before)
            if (GraphicsCore.IsInitialized)
            {
                m_renderLoop.SetScene(new Scene());
                m_renderLoop.Camera = new PerspectiveCamera3D();

                ////Observe resize event and throttle them
                //this.HandleCreateDisposeOnControl(
                //    () => Observable.FromEventPattern(this, "Resize")
                //        .Merge(Observable.FromEventPattern(m_renderLoop.ViewConfiguration, "ConfigurationChanged"))
                //        .Throttle(TimeSpan.FromSeconds(0.5))
                //        .ObserveOn(SynchronizationContext.Current)
                //        .Subscribe((eArgs) => OnThrottledViewRecreation()));

                //Initialize background brush
                UpdateDrawingResourcesForFailoverRendering();

                //// Observe mouse click event
                //this.HandleCreateDisposeOnControl(() =>
                //{
                //    var mouseDownEvent = Observable.FromEventPattern<MouseEventArgs>(this, "MouseDown");
                //    var mouseUpEvent = Observable.FromEventPattern<MouseEventArgs>(this, "MouseUp");
                //    var mouseClick = from down in mouseDownEvent
                //                     let timeStampDown = DateTime.UtcNow
                //                     from up in mouseUpEvent
                //                     where up.EventArgs.Button == down.EventArgs.Button
                //                     let timeStampUp = DateTime.UtcNow
                //                     where timeStampUp - timeStampDown < TimeSpan.FromMilliseconds(200.0)
                //                     select new { Down = down, Up = up };
                //    return mouseClick.Subscribe((givenItem) =>
                //    {
                //        MouseClickEx.Raise(this, givenItem.Up.EventArgs);
                //    });
                //});
            }

            this.Disposed += OnDisposed;

            UpdateDrawingResourcesForFailoverRendering();
        }

        /// <summary>
        /// Gets the scene object below the cursor.
        /// </summary>
        public async Task<SceneObject> GetObjectBelowCursorAsync()
        {
            if (!m_isMouseInside) { return null; }

            List<SceneObject> objects = await m_renderLoop.PickObjectAsync(
                SeeingSharpDesktopTools.PointFromGdiPoint(this.PointToClient(Cursor.Position)),
                new PickingOptions() { OnlyCheckBoundingBoxes = false });

            if (objects == null) { return null; }
            return objects.FirstOrDefault();
        }

        /// <summary>
        /// Gets all objects that are below the cursor.
        /// </summary>
        public async Task<List<SceneObject>> GetObjectsBelowCursorAsync()
        {
            if (!m_isMouseInside) { return new List<SceneObject>(); }

            return await m_renderLoop.PickObjectAsync(
                SeeingSharpDesktopTools.PointFromGdiPoint(this.PointToClient(Cursor.Position)),
                new PickingOptions() { OnlyCheckBoundingBoxes = false });
        }

        /// <summary>
        /// Saves a screenshot to harddisc.
        /// </summary>
        /// <param name="targetFile">Target file path.</param>
        public async Task SaveScreenshotAsync(string targetFile)
        {
            if (m_backBuffer != null)
            {
                Bitmap screenshot = await m_renderLoop.GetScreenshotGdiAsync();
                screenshot.Save(targetFile);
            }
        }

        /// <summary>
        /// Saves a screenshot to harddisc.
        /// </summary>
        /// <param name="targetFile">Target file path.</param>
        /// <param name="imageFormat">Target file format.</param>
        public async Task SaveScreenshotAsync(string targetFile, System.Drawing.Imaging.ImageFormat imageFormat)
        {
            if (m_backBuffer != null)
            {
                Bitmap screenshot = await m_renderLoop.GetScreenshotGdiAsync();
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

            if ((!m_renderLoop.ViewResourcesLoaded) ||
                (!m_renderLoop.IsRegisteredOnMainLoop))
            {
                // Paint using System.Drawing
                e.Graphics.FillRectangle(m_backBrush, e.ClipRectangle);

                // Paint a simple grid on the background to have something for the Designer
                if (!GraphicsCore.IsInitialized)
                {
                    GDI.SizeF targetSize = e.Graphics.MeasureString(TEXT_GRAPHICS_NOT_INITIALIZED, this.Font);
                    GDI.RectangleF targetRect = new GDI.RectangleF(
                        10f, 10f, targetSize.Width, targetSize.Height);
                    if ((targetRect.Width > 10) &&
                       (targetRect.Height > 10))
                    {
                        e.Graphics.FillRectangle(m_backBrushText, targetRect);
                        e.Graphics.DrawString(
                            TEXT_GRAPHICS_NOT_INITIALIZED, this.Font,
                            m_foreBrushText, targetRect.X, targetRect.Y);
                    }
                }

                // Paint a border rectangle
                e.Graphics.DrawRectangle(
                    m_borderPen,
                    new GDI.Rectangle(0, 0, this.Width - 1, this.Height - 1));
            }
        }

        /// <summary>
        /// Stops rendering.
        /// </summary>
        private void StartRendering()
        {
            if (this.DesignMode) { return; }
            if (!GraphicsCore.IsInitialized) { return; }

            if (!m_renderLoop.IsRegisteredOnMainLoop)
            {
                m_renderLoop.SetCurrentViewSize(this.Width, this.Height);
                m_renderLoop.DiscardRendering = false;
                m_renderLoop.RegisterRenderLoop();
            }
        }

        /// <summary>
        /// Stops rendering.
        /// </summary>
        private void StopRendering()
        {
            if (this.DesignMode) { return; }
            if (!GraphicsCore.IsInitialized) { return; }

            if (m_renderLoop.IsRegisteredOnMainLoop)
            {
                m_renderLoop.DiscardRendering = true;
                m_renderLoop.DeregisterRenderLoop();
            }
        }

        /// <summary>
        /// Updates the background brush used for failover rendering in System.Drawing.
        /// </summary>
        private void UpdateDrawingResourcesForFailoverRendering()
        {
            SeeingSharpTools.SafeDispose(ref m_backBrush);
            SeeingSharpTools.SafeDispose(ref m_foreBrushText);
            SeeingSharpTools.SafeDispose(ref m_backBrushText);
            SeeingSharpTools.SafeDispose(ref m_borderPen);

            m_backBrush = new System.Drawing.Drawing2D.HatchBrush(
                GDI.Drawing2D.HatchStyle.DottedGrid,
                GDI.Color.Gray, this.BackColor);
            m_backBrushText = new SolidBrush(GDI.Color.White);
            m_foreBrushText = new SolidBrush(GDI.Color.Black);
            m_borderPen = new Pen(GDI.Color.DarkGray);
        }

        /// <summary>
        /// Called when BackColor property has changed.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);

            //Update background brush
            UpdateDrawingResourcesForFailoverRendering();
        }

        /// <summary>
        /// Called when the size of the viewport has changed.
        /// </summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (this.DesignMode) { return; }
            if (!GraphicsCore.IsInitialized) { return; }

            if ((this.Width > 0) && (this.Height > 0))
            {
                m_renderLoop.Camera.SetScreenSize(this.Width, this.Height);
            }
        }

        /// <summary>
        /// Called when the window handle is created.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            StartRendering();
        }

        /// <summary>
        /// Called when the window handle is destroyed.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);

            StopRendering();
        }

        /// <summary>
        /// Handle changed control visibility.
        /// </summary>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (this.Visible) { StartRendering(); }
            else if (!this.Visible) { StopRendering(); }
        }

        /// <summary>
        /// Called when this view gets disposed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnDisposed(object sender, EventArgs e)
        {
            if (!this.DesignMode)
            {
                m_renderLoop.Dispose();
                m_renderLoop = null;
            }
        }

        /// <summary>
        /// Create all view resources.
        /// </summary>
        Tuple<D3D11.Texture2D, D3D11.RenderTargetView, D3D11.Texture2D, D3D11.DepthStencilView, SharpDX.Mathematics.Interop.RawViewportF, Size2, DpiScaling> IRenderLoopHost.OnRenderLoop_CreateViewResources(EngineDevice device)
        {
            int width = this.Width;
            int height = this.Height;
            if (width <= SeeingSharpConstants.MIN_VIEW_WIDTH) { width = SeeingSharpConstants.MIN_VIEW_WIDTH; }
            if (height <= SeeingSharpConstants.MIN_VIEW_HEIGHT) { height = SeeingSharpConstants.MIN_VIEW_HEIGHT; }

            //Get all factories
            m_factory = device.FactoryDxgi;

            //Get all devices
            m_renderDevice = device.DeviceD3D11_1;
            m_renderDeviceContext = m_renderDevice.ImmediateContext;

            //Create the swap chain and the render target
            m_swapChain = GraphicsHelperDesktop.CreateSwapChainForWinForms(this, device, m_renderLoop.ViewConfiguration);
            m_backBuffer = D3D11.Texture2D.FromSwapChain<D3D11.Texture2D>(m_swapChain, 0);
            m_renderTarget = new D3D11.RenderTargetView(m_renderDevice, m_backBuffer);

            //Create the depth buffer
            m_depthBuffer = GraphicsHelper.CreateDepthBufferTexture(device, width, height, m_renderLoop.ViewConfiguration);
            m_renderTargetDepth = new D3D11.DepthStencilView(m_renderDevice, m_depthBuffer);

            //Define the viewport for rendering
            SharpDX.Mathematics.Interop.RawViewportF viewPort = GraphicsHelper.CreateDefaultViewport(width, height);

            // Query for current dpi value
            DpiScaling dpiScaling = DpiScaling.Default;
            using (Graphics graphics = this.CreateGraphics())
            {
                dpiScaling.DpiX = graphics.DpiX;
                dpiScaling.DpiY = graphics.DpiY;
            }

            //Return all generated objects
            return Tuple.Create(m_backBuffer, m_renderTarget, m_depthBuffer, m_renderTargetDepth, viewPort, new Size2(width, height), dpiScaling);
        }

        /// <summary>
        /// Disposes all loaded view resources.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_DisposeViewResources(EngineDevice device)
        {
            m_factory = null;
            m_renderDevice = null;
            m_renderDeviceContext = null;

            m_renderTargetDepth = SeeingSharpTools.DisposeObject(m_renderTargetDepth);
            m_depthBuffer = SeeingSharpTools.DisposeObject(m_depthBuffer);
            m_renderTarget = SeeingSharpTools.DisposeObject(m_renderTarget);
            m_backBuffer = SeeingSharpTools.DisposeObject(m_backBuffer);
            m_swapChain = SeeingSharpTools.DisposeObject(m_swapChain);
        }

        /// <summary>
        /// Called when RenderLoop object checks wheter it is possible to render.
        /// </summary>
        bool IRenderLoopHost.OnRenderLoop_CheckCanRender(EngineDevice device)
        {
            // Check properties on self
            if (this.IsDisposed) { return false; }
            if (this.Parent == null) { return false; }
            if (!this.Visible) { return false; }
            if (this.Width <= 0) { return false; }
            if (this.Height <= 0) { return false; }

            // Check parent form
            Form parentForm = this.GetParentForm();
            if(parentForm == null)
            {
                // Control's parent chain is broken -> it is not visible.
                return false;
            }
            else
            {
                if(parentForm.WindowState == FormWindowState.Minimized) { return false; }
            }

            return true;
        }

        /// <summary>
        /// Called when the render loop prepares rendering.
        /// </summary>
        /// <param name="device">The current rendering device.</param>
        void IRenderLoopHost.OnRenderLoop_PrepareRendering(EngineDevice device)
        {

        }

        /// <summary>
        /// Called when RenderLoop wants to present its results.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_Present(EngineDevice device)
        {
            //Present all rendered stuff on screen
            m_swapChain.Present(0, DXGI.PresentFlags.DoNotWait, new DXGI.PresentParameters());
        }

        /// <summary>
        /// Called when RenderLoop has finished rendering.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_AfterRendering(EngineDevice device)
        {
            //m_isOnRendering = false;
            if (!this.Visible)
            {
                StopRendering();
            }
        }

        ///// <summary>
        ///// Called when the control has changed its size.
        ///// </summary>
        //private void OnThrottledViewRecreation()
        //{
        //    if (!this.DesignMode)
        //    {
        //        m_renderLoop.SetCurrentViewSize(this.Width, this.Height);

        //        //if ((this.Width > 0) && (this.Height > 0))
        //        //{
        //        //    m_renderLoop.RefreshViewResources();
        //        //}
        //    }
        //}

        /// <summary>
        /// Called when the mouse is inside the area of this control.
        /// </summary>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            m_isMouseInside = true;
        }

        /// <summary>
        /// Called when the mouse is outside the area of this control.
        /// </summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            m_isMouseInside = false;
        }

        /// <summary>
        /// Called when RenderLoop allows it to manipulate current filter list.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The decimal.</param>
        private void OnRenderLoopManipulateFilterList(object sender, ManipulateFilterListArgs e)
        {
            ManipulateFilterList.Raise(this, e);
        }

        /// <summary>
        /// Discard rendering?
        /// </summary>
        [Browsable(false)]
        [DefaultValue(false)]
        public bool DiscardRendering
        {
            get { return m_renderLoop.DiscardRendering; }
            set { m_renderLoop.DiscardRendering = value; }
        }

        /// <summary>
        /// Discard present of rendering results on the screen?
        /// </summary>
        [Category(SeeingSharpConstants.DESIGNER_CATEGORY_RENDERER)]
        [DefaultValue(false)]
        public bool DiscardPresent
        {
            get { return m_renderLoop.DiscardPresent; }
            set { m_renderLoop.DiscardPresent = value; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Scene Scene
        {
            get { return m_renderLoop.Scene; }
            set { m_renderLoop.SetScene(value); }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Camera3DBase Camera
        {
            get { return m_renderLoop.Camera; }
            set { m_renderLoop.Camera = value; }
        }

        /// <summary>
        /// Gets the view configuration.
        /// </summary>
        [Browsable(true)]
        [Category(SeeingSharpConstants.DESIGNER_CATEGORY_RENDERER)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public GraphicsViewConfiguration ViewConfiguration
        {
            get { return m_renderLoop.ViewConfiguration; }
        }

        /// <summary>
        /// Gets the render loop object.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RenderLoop RenderLoop
        {
            get { return m_renderLoop; }
        }

        /// <summary>
        /// Ruft die Hintergrundfarbe für das Steuerelement ab oder legt diese fest.
        /// </summary>
        /// <returns>Eine <see cref="T:System.Drawing.Color" />, die die Hintergrundfarbe des Steuerelements darstellt. Der Standardwert ist der Wert der <see cref="P:System.Windows.Forms.Control.DefaultBackColor" />-Eigenschaft.</returns>
        public override GDI.Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                base.BackColor = value;
                m_renderLoop.ClearColor = new Color4(this.BackColor);
            }
        }

        [Browsable(false)]
        public EngineDevice Device
        {
            get
            {
                if (m_renderLoop != null) { return m_renderLoop.Device; }
                else { return null; }
            }
        }

        /// <summary>
        /// True if the control is connected with the main rendering loop.
        /// False if something went wrong.
        /// </summary>
        [Browsable(false)]
        public bool IsOperational
        {
            get
            {
                return m_renderLoop.IsOperational;
            }
        }
    }
}