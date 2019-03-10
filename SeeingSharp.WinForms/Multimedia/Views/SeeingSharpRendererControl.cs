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

//Some namespace mappings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Input;
using SeeingSharp.Util;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using D3D11 = SharpDX.Direct3D11;
using GDI = System.Drawing;

namespace SeeingSharp.Multimedia.Views
{
    public class SeeingSharpRendererControl : Panel, ISeeingSharpPainter, IInputEnabledView, IRenderLoopHost
    {
        private const string TEXT_GRAPHICS_NOT_INITIALIZED = "Graphics not initialized!";

        // Main reference to 3D-Engine
        private RenderLoop m_renderLoop;

        // Resources for Direct3D 11
        private Factory m_factory;
        private SwapChain1 m_swapChain;
        private D3D11.Device m_renderDevice;
        private D3D11.DeviceContext m_renderDeviceContext;
        private D3D11.RenderTargetView m_renderTarget;
        private D3D11.DepthStencilView m_renderTargetDepth;
        private D3D11.Texture2D m_backBuffer;
        private D3D11.Texture2D m_depthBuffer;

        // Generic members
        private GDI.Brush m_backBrush;
        private GDI.Brush m_foreBrushText;
        private GDI.Brush m_backBrushText;
        private GDI.Pen m_borderPen;

        // Misc
        private bool m_isMouseInside;
        private DateTime m_lastSizeChange;
        private Dictionary<MouseButtons, DateTime> m_mouseButtonDownTime;

        /// <summary>
        /// Raised when it is possible for the UI thread to manipulate current filter list.
        /// </summary>
        public event EventHandler<ManipulateFilterListArgs> ManipulateFilterList;

        /// <summary>
        /// Raises when mouse was down a short amount of time.
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseClickEx;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SeeingSharp.Multimedia.Views.SeeingSharpRendererControl" /> class.
        /// </summary>
        public SeeingSharpRendererControl()
        {
            // Set style parameters for this control
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
            SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.Selectable, true);
            base.DoubleBuffered = false;

            m_lastSizeChange = DateTime.MinValue;
            m_mouseButtonDownTime = new Dictionary<MouseButtons, DateTime>();

            // Create the render loop
            var backColor = BackColor;
            m_renderLoop = new RenderLoop(SynchronizationContext.Current, this, DesignMode);
            m_renderLoop.ManipulateFilterList += OnRenderLoopManipulateFilterList;
            m_renderLoop.ClearColor = backColor.Color4FromGdiColor();
            m_renderLoop.DiscardRendering = true;
            Disposed += (sender, eArgs) =>
            {
                m_renderLoop.Dispose();
            };

            // Perform default initialization logic (if not called before)
            if (GraphicsCore.IsLoaded)
            {
                m_renderLoop.SetScene(new Scene());
                m_renderLoop.Camera = new PerspectiveCamera3D();

                //Initialize background brush
                UpdateDrawingResourcesForFailoverRendering();
            }

            Disposed += OnDisposed;

            UpdateDrawingResourcesForFailoverRendering();
        }

        /// <summary>
        /// Gets the scene object below the cursor.
        /// </summary>
        public async Task<SceneObject> GetObjectBelowCursorAsync()
        {
            if (!m_isMouseInside) { return null; }

            var objects = await m_renderLoop.PickObjectAsync(
                SeeingSharpWinFormsUtil.PointFromGdiPoint(PointToClient(Cursor.Position)),
                new PickingOptions { OnlyCheckBoundingBoxes = false });
            return objects?.FirstOrDefault();
        }

        /// <summary>
        /// Gets all objects that are below the cursor.
        /// </summary>
        public async Task<List<SceneObject>> GetObjectsBelowCursorAsync()
        {
            if (!m_isMouseInside)
            {
                return new List<SceneObject>();
            }

            return await m_renderLoop.PickObjectAsync(
                SeeingSharpWinFormsUtil.PointFromGdiPoint(PointToClient(Cursor.Position)),
                new PickingOptions { OnlyCheckBoundingBoxes = false });
        }

        /// <summary>
        /// Saves a screenshot to hard disc.
        /// </summary>
        /// <param name="targetFile">Target file path.</param>
        public async Task SaveScreenshotAsync(string targetFile)
        {
            if (m_backBuffer != null)
            {
                var screenshot = await m_renderLoop.GetScreenshotGdiAsync();
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
            if (m_backBuffer != null)
            {
                var screenshot = await m_renderLoop.GetScreenshotGdiAsync();
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

            if (!m_renderLoop.ViewResourcesLoaded ||
                !m_renderLoop.IsRegisteredOnMainLoop)
            {
                // Paint using System.Drawing
                e.Graphics.FillRectangle(m_backBrush, e.ClipRectangle);

                // Paint a simple grid on the background to have something for the Designer
                if (!GraphicsCore.IsLoaded)
                {
                    var targetSize = e.Graphics.MeasureString(TEXT_GRAPHICS_NOT_INITIALIZED, Font);
                    var targetRect = new GDI.RectangleF(
                        10f, 10f, targetSize.Width, targetSize.Height);
                    if (targetRect.Width > 10 &&
                       targetRect.Height > 10)
                    {
                        e.Graphics.FillRectangle(m_backBrushText, targetRect);
                        e.Graphics.DrawString(
                            TEXT_GRAPHICS_NOT_INITIALIZED, Font,
                            m_foreBrushText, targetRect.X, targetRect.Y);
                    }
                }

                // Paint a border rectangle
                e.Graphics.DrawRectangle(
                    m_borderPen,
                    new GDI.Rectangle(0, 0, Width - 1, Height - 1));
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
            UpdateDrawingResourcesForFailoverRendering();
        }

        /// <summary>
        /// Called when the size of the viewport has changed.
        /// </summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (DesignMode) { return; }
            if (!GraphicsCore.IsLoaded) { return; }

            if (Width > 0 && Height > 0)
            {
                m_renderLoop.Camera.SetScreenSize(Width, Height);
                m_lastSizeChange = DateTime.UtcNow;
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

            if (Visible) { StartRendering(); }
            else if (!Visible) { StopRendering(); }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            foreach (MouseButtons actButton in Enum.GetValues(typeof(MouseButtons)))
            {
                if (((int)e.Button | (int)actButton) != (int)actButton) { continue; }
                if (m_mouseButtonDownTime.ContainsKey(actButton)) { continue; }

                m_mouseButtonDownTime[actButton] = DateTime.UtcNow;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            foreach (MouseButtons actButton in Enum.GetValues(typeof(MouseButtons)))
            {
                if (((int)e.Button | (int)actButton) != (int)actButton) { continue; }
                if (!m_mouseButtonDownTime.ContainsKey(actButton)) { continue; }

                var downTimeStamp = m_mouseButtonDownTime[actButton];
                m_mouseButtonDownTime.Remove(actButton);

                if(DateTime.UtcNow - downTimeStamp < SeeingSharpConstantsWinForms.MOUSE_CLICK_MAX_TIME)
                {
                    MouseClickEx?.Invoke(this, e);
                }
            }
        }

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
            m_mouseButtonDownTime.Clear();
        }

        /// <summary>
        /// Stops rendering.
        /// </summary>
        private void StartRendering()
        {
            if (DesignMode) { return; }
            if (!GraphicsCore.IsLoaded) { return; }

            if (!m_renderLoop.IsRegisteredOnMainLoop)
            {
                m_renderLoop.SetCurrentViewSize(Width, Height);
                m_renderLoop.DiscardRendering = false;
                m_renderLoop.RegisterRenderLoop();
            }
        }

        /// <summary>
        /// Stops rendering.
        /// </summary>
        private void StopRendering()
        {
            if (DesignMode) { return; }
            if (!GraphicsCore.IsLoaded) { return; }

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
            SeeingSharpUtil.SafeDispose(ref m_backBrush);
            SeeingSharpUtil.SafeDispose(ref m_foreBrushText);
            SeeingSharpUtil.SafeDispose(ref m_backBrushText);
            SeeingSharpUtil.SafeDispose(ref m_borderPen);

            m_backBrush = new HatchBrush(
                HatchStyle.DottedGrid,
                GDI.Color.Gray, BackColor);
            m_backBrushText = new GDI.SolidBrush(GDI.Color.White);
            m_foreBrushText = new GDI.SolidBrush(GDI.Color.Black);
            m_borderPen = new GDI.Pen(GDI.Color.DarkGray);
        }

        /// <summary>
        /// Called when this view gets disposed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnDisposed(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                m_renderLoop.Dispose();
                m_renderLoop = null;
            }
        }

        /// <summary>
        /// Called when RenderLoop allows it to manipulate current filter list.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The decimal.</param>
        private void OnRenderLoopManipulateFilterList(object sender, ManipulateFilterListArgs e)
        {
            ManipulateFilterList?.Invoke(this, e);
        }

        /// <summary>
        /// Create all view resources.
        /// </summary>
        Tuple<D3D11.Texture2D, D3D11.RenderTargetView, D3D11.Texture2D, D3D11.DepthStencilView, RawViewportF, Size2, DpiScaling> IRenderLoopHost.OnRenderLoop_CreateViewResources(EngineDevice device)
        {
            var width = Width;
            var height = Height;
            if (width <= SeeingSharpConstants.MIN_VIEW_WIDTH) { width = SeeingSharpConstants.MIN_VIEW_WIDTH; }
            if (height <= SeeingSharpConstants.MIN_VIEW_HEIGHT) { height = SeeingSharpConstants.MIN_VIEW_HEIGHT; }

            //Get all factories
            m_factory = device.FactoryDxgi;

            //Get all devices
            m_renderDevice = device.DeviceD3D11_1;
            m_renderDeviceContext = m_renderDevice.ImmediateContext;

            //Create the swap chain and the render target
            m_swapChain = GraphicsHelperWinForms.CreateSwapChainForWinForms(this, device, m_renderLoop.ViewConfiguration);
            m_backBuffer = D3D11.Resource.FromSwapChain<D3D11.Texture2D>(m_swapChain, 0);
            m_renderTarget = new D3D11.RenderTargetView(m_renderDevice, m_backBuffer);

            //Create the depth buffer
            m_depthBuffer = GraphicsHelper.CreateDepthBufferTexture(device, width, height, m_renderLoop.ViewConfiguration);
            m_renderTargetDepth = new D3D11.DepthStencilView(m_renderDevice, m_depthBuffer);

            //Define the viewport for rendering
            var viewPort = GraphicsHelper.CreateDefaultViewport(width, height);

            // Query for current dpi value
            var dpiScaling = DpiScaling.Default;

            using (var graphics = CreateGraphics())
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

            m_renderTargetDepth = SeeingSharpUtil.DisposeObject(m_renderTargetDepth);
            m_depthBuffer = SeeingSharpUtil.DisposeObject(m_depthBuffer);
            m_renderTarget = SeeingSharpUtil.DisposeObject(m_renderTarget);
            m_backBuffer = SeeingSharpUtil.DisposeObject(m_backBuffer);
            m_swapChain = SeeingSharpUtil.DisposeObject(m_swapChain);
        }

        /// <summary>
        /// Called when RenderLoop object checks whether it is possible to render.
        /// </summary>
        bool IRenderLoopHost.OnRenderLoop_CheckCanRender(EngineDevice device)
        {
            // Check properties on self
            if (IsDisposed) { return false; }
            if (Parent == null) { return false; }
            if (!Visible) { return false; }
            if (Width <= 0) { return false; }
            if (Height <= 0) { return false; }

            Form parentForm = null;
            var actParent = Parent;

            while(parentForm == null && actParent != null)
            {
                parentForm = actParent as Form;
                actParent = actParent.Parent;
            }

            // Check parent form
            if(parentForm == null)
            {
                // TODO: Handle the case where we are hosted inside a wpf environment

                // Control's parent chain is broken -> it is not visible.
                return false;
            }
            if(parentForm.WindowState == FormWindowState.Minimized) { return false; }



            return true;
        }

        /// <summary>
        /// Called when the render loop prepares rendering.
        /// </summary>
        /// <param name="device">The current rendering device.</param>
        void IRenderLoopHost.OnRenderLoop_PrepareRendering(EngineDevice device)
        {
            if(m_lastSizeChange != DateTime.MinValue &&
               DateTime.UtcNow - m_lastSizeChange > SeeingSharpConstantsWinForms.THROTTLED_VIEW_RECREATION_TIME_ON_RESIZE)
            {
                m_lastSizeChange = DateTime.MinValue;

                var width = Width;
                var height = Height;
                if (width > 0 && height > 0)
                {
                    m_renderLoop.SetCurrentViewSize(Width, Height);
                }
            }
        }

        /// <summary>
        /// Called when RenderLoop wants to present its results.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_Present(EngineDevice device)
        {
            //Present all rendered stuff on screen
            try
            {
                m_swapChain.Present(0, PresentFlags.DoNotWait, new PresentParameters());
            }
            catch(SharpDXException ex)
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
            //m_isOnRendering = false;
            if (!Visible)
            {
                StopRendering();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Scene Scene
        {
            get => m_renderLoop.Scene;
            set => m_renderLoop.SetScene(value);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Camera3DBase Camera
        {
            get => m_renderLoop.Camera;
            set => m_renderLoop.Camera = value;
        }

        /// <summary>
        /// Gets the render loop object.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RenderLoop RenderLoop => m_renderLoop;

        /// <summary>
        /// Discard rendering?
        /// </summary>
        [Browsable(false)]
        [DefaultValue(false)]
        public bool DiscardRendering
        {
            get => m_renderLoop.DiscardRendering;
            set => m_renderLoop.DiscardRendering = value;
        }

        /// <summary>
        /// Discard present of rendering results on the screen?
        /// </summary>
        [Category(SeeingSharpConstantsWinForms.DESIGNER_CATEGORY_RENDERER)]
        [DefaultValue(false)]
        public bool DiscardPresent
        {
            get => m_renderLoop.DiscardPresent;
            set => m_renderLoop.DiscardPresent = value;
        }

        /// <summary>
        /// Gets the view configuration.
        /// </summary>
        [Browsable(true)]
        [Category(SeeingSharpConstantsWinForms.DESIGNER_CATEGORY_RENDERER)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public GraphicsViewConfiguration ViewConfiguration => m_renderLoop.ViewConfiguration;

        public sealed override GDI.Color BackColor
        {
            get => base.BackColor;
            set
            {
                base.BackColor = value;
                m_renderLoop.ClearColor = value.Color4FromGdiColor();
            }
        }

        [Browsable(false)]
        public EngineDevice Device => m_renderLoop?.Device;

        /// <summary>
        /// True if the control is connected with the main rendering loop.
        /// False if something went wrong.
        /// </summary>
        [Browsable(false)]
        public bool IsOperational => m_renderLoop.IsOperational;
    }
}