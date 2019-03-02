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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.DrawingVideo;
using SeeingSharp.Multimedia.Input;
using SeeingSharp.Util;
using SharpDX;
using SharpDX.Mathematics.Interop;
using D3D11 = SharpDX.Direct3D11;
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Multimedia.Core
{
    /// <summary>
    /// This class controls rendering logic behind each view object.
    /// </summary>
    public class RenderLoop : IDisposable
    {
        // Configuration values
        private GraphicsViewConfiguration m_viewConfiguration;
        private Camera3DBase m_camera;
        
        // Async actions
        private ThreadSaveQueue<Action> m_afterPresentActions;
        
        // Target parameters for rendering
        private DateTime m_lastTargetParametersChanged;
        private EngineDevice m_targetDevice;
        private Size2 m_targetSize;
        private DpiScaling m_currentDpiScaling;
        private Scene m_targetScene;
        private bool m_viewRefreshForced;
        
        // Callback methods for current host object
        private IRenderLoopHost m_renderLoopHost;
        
        // Values needed for runtime
        private bool m_lastRenderSuccessfully;
        private bool m_nextRenderAllowed;
        private int m_totalRenderCount;
        private RenderState m_renderState;
        private List<SeeingSharpVideoWriter> m_videoWriters;
        private bool m_callPresentInUiThread;
        
        // Direct3D resources and other values gathered during graphics loading
        private List<Custom2DDrawingLayer> m_2dDrawingLayers;
        private DebugDrawingLayer m_debugDrawingLayer;
        private EngineDevice m_currentDevice;
        private Size2 m_currentViewSize;
        private Size2F m_currentViewSizeDpiScaled;
        private Scene m_currentScene;
        private Direct2DOverlayRenderer m_d2dOverlay;
        private D3D11.Texture2D m_renderTarget;
        private D3D11.Texture2D m_renderTargetDepth;
        private D3D11.RenderTargetView m_renderTargetView;
        private D3D11.DepthStencilView m_renderTargetDepthView;
        private RawViewportF m_viewport;

        // Direct3D resources for rendertarget capturing
        // ----
        // A staging texture for reading contents by Cpu
        // A standard texture for copying data from multisample texture to standard one
        // see http://www.rolandk.de/wp/2013/06/inhalt-der-rendertarget-textur-in-ein-bitmap-kopieren/
        private D3D11.Texture2D m_copyHelperTextureStaging;
        private D3D11.Texture2D m_copyHelperTextureStandard;

        /// <summary>
        /// Raised when the corresponding device has changed.
        /// </summary>
        public event EventHandler DeviceChanged;

        public event EventHandler CurrentViewSizeChanged;

        /// <summary>
        /// Raised when the current camera has changed.
        /// </summary>
        public event EventHandler CameraChanged;

        /// <summary>
        /// Raised directly after rendering a frame.
        /// Be careful, this event comes from a background rendering thread!
        /// </summary>
        public event EventHandler Rendered;

        /// <summary>
        /// Raised when it is possible for the UI thread to manipulate current filter list.
        /// </summary>
        public event EventHandler<ManipulateFilterListArgs> ManipulateFilterList;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderLoop" /> class.
        /// </summary>
        public RenderLoop(
            SynchronizationContext guiSyncContext,
            IRenderLoopHost renderLoopHost,
            bool isDesignMode = false)
        {
            Internals = new RenderLoopInternals(this);

            m_afterPresentActions = new ThreadSaveQueue<Action>();

            UISynchronizationContext = guiSyncContext;

            SceneComponents = new ObservableCollection<SceneComponentBase>();
            SceneComponents.CollectionChanged += OnSceneComponents_Changed;

            Filters = new List<SceneObjectFilter>();
            m_2dDrawingLayers = new List<Custom2DDrawingLayer>();

            // Load DebugDrawingLayer if debug mode is enabled
            if (!isDesignMode)
            {
                if (GraphicsCore.IsLoaded &&
                    GraphicsCore.Current.IsDebugEnabled)
                {
                    m_debugDrawingLayer = new DebugDrawingLayer();
                }
            }

            ViewInformation = new ViewInformation(this);
            m_viewConfiguration = new GraphicsViewConfiguration();

            m_afterPresentActions = new ThreadSaveQueue<Action>();

            m_videoWriters = new List<SeeingSharpVideoWriter>();

            // Assign all given actions
            m_renderLoopHost = renderLoopHost;

            // Create default objects
            ClearColor = Color4.White;

            // Set initial target parameters
            m_lastTargetParametersChanged = DateTime.MinValue;
            m_targetDevice = null;
            m_targetSize = new Size2(0, 0);

            // Create default scene and camera
            Camera = new PerspectiveCamera3D();
            SetScene(new Scene());

            if (!GraphicsCore.IsLoaded) { return; }
            if (isDesignMode) { return; }

            // Apply default rendering device for this RenderLoop
            SetRenderingDevice(GraphicsCore.Current.DefaultDevice);
        }

        /// <summary>
        /// Sets the current with and height of the view.
        /// The RenderLoop will apply the values later.
        /// </summary>
        /// <param name="width">Current width in pixels.</param>
        /// <param name="height">Current height in pixels.</param>
        public void SetCurrentViewSize(int width, int height)
        {
            width.EnsurePositive(nameof(width));
            height.EnsurePositive(nameof(height));

            if (width < SeeingSharpConstants.MIN_VIEW_WIDTH) { width = SeeingSharpConstants.MIN_VIEW_WIDTH; }
            if (height < SeeingSharpConstants.MIN_VIEW_HEIGHT) { height = SeeingSharpConstants.MIN_VIEW_HEIGHT; }

            m_targetSize = new Size2(width, height);
        }

        /// <summary>
        /// Forces a refresh of the current view.
        /// </summary>
        internal void ForceViewRefresh()
        {
            m_viewRefreshForced = true;
        }

        /// <summary>
        /// Gets the current index of this view within the current scene.
        /// </summary>
        public int GetViewIndex()
        {
            var viewInformation = ViewInformation;

            if (viewInformation != null)
            {
                return viewInformation.ViewIndex + 1;
            }
            return -1;
        }

        /// <summary>
        /// Sets the given rendering device for this renderloop.
        /// </summary>
        /// <param name="device">The device to be set.</param>
        public void SetRenderingDevice(EngineDevice device)
        {
            device.EnsureNotNull(nameof(device));

            // Only set state values here. All logic is triggered during render process
            m_targetDevice = device;
        }

        /// <summary>
        /// Sets the scene for rendering
        /// </summary>
        /// <param name="scene">The scene to be set.</param>
        public void SetScene(Scene scene)
        {
            scene.EnsureNotNull(nameof(scene));

            m_targetScene = scene;
        }

        /// <summary>
        /// Forces the view to reload itself.
        /// </summary>
        public void ForceViewReload()
        {
            m_viewConfiguration.ViewNeedsRefresh = true;
        }

        /// <summary>
        /// Enqueue the given action to be called after presenting the next frame.
        /// </summary>
        /// <param name="action">The action to be called.</param>
        public void EnqueueAfterPresentAction(Action action)
        {
            m_afterPresentActions.Enqueue(action);
        }

        /// <summary>
        /// Waits for the next finished render process.
        /// </summary>
        public Task WaitForNextFinishedRenderAsync()
        {
            var result = new TaskCompletionSource<object>();

            m_afterPresentActions.Enqueue(() =>
            {
                result.TrySetResult(null);
            });

            return result.Task;
        }

        /// <summary>
        /// Registers the given drawing layer that is used for 2d rendering.
        /// </summary>
        /// <param name="drawAction">The action that performs 2D drawing</param>
        public Task<Custom2DDrawingLayer> Register2DDrawingLayerAsync(Action<Graphics2D> drawAction)
        {
            drawAction.EnsureNotNull(nameof(drawAction));

            var result = new Custom2DDrawingLayer(drawAction);
            return Register2DDrawingLayerAsync(result)
                .ContinueWith(givenTask => result);
        }

        /// <summary>
        /// Moves the camera to a default location which may see most of the warehouse.
        /// </summary>
        /// <param name="horizontalRotation">The horizontal rotation seen from the middle point of the scene.</param>
        /// <param name="verticalRotation">The vertical rotation seen from the middle point of the scene.</param>
        public async Task MoveCameraToDefaultLocationAsync(float horizontalRotation, float verticalRotation)
        {
            if (m_currentScene == null) { return; }
            if (m_currentDevice == null) { return; }

            var sceneBox = await CalculateSceneBoxAsync();

            if (sceneBox.IsEmpty()) { return; }
            if (m_currentScene == null) { return; }
            if (m_currentDevice == null) { return; }

            if (m_camera != null)
            {
                await m_currentScene.PerformBeforeUpdateAsync(() =>
                {
                    var bottomCenter = sceneBox.GetBottomCenter();
                    var anyEdge = sceneBox.GetTopFrontMiddleCoordinate();
                    var centerToEdgeVector = anyEdge - bottomCenter;
                    var centerToEdgeDistance = centerToEdgeVector.Length();

                    var direction = Vector3.TransformNormal(
                        new Vector3(0f, 0f, -1f),
                        Matrix.RotationYawPitchRoll(horizontalRotation, verticalRotation, 0f));

                    m_camera.Position = bottomCenter + direction * (centerToEdgeDistance * 2f);
                    m_camera.Target = bottomCenter;
                    m_camera.UpdateCamera();
                });
            }
        }

        /// <summary>
        /// Calculates a BoundingBox which contains all objects of the scene.
        /// </summary>
        public async Task<BoundingBox> CalculateSceneBoxAsync()
        {
            var result = default(BoundingBox);

            if (m_currentScene == null) { return result; }
            if (m_currentDevice == null) { return result; }

            await m_currentScene.PerformBesideRenderingAsync(() =>
            {
                foreach (var actLayer in m_currentScene.Layers)
                {
                    foreach (var actSpacialObject in actLayer.SpacialObjects)
                    {
                        var actBoundingBox = actSpacialObject.TryGetBoundingBox(ViewInformation);

                        if (actBoundingBox.IsEmpty())
                        {
                            continue;
                        }

                        if (result.IsEmpty())
                        {
                            result = actBoundingBox;
                        }
                        else
                        {
                            result.MergeWith(actBoundingBox);
                        }
                    }
                }
            });

            return result;
        }

        /// <summary>
        /// Finishes the given VideoWriter object and deregisters it from this RenderLoop.
        /// </summary>
        /// <param name="videoWriter">The VideoWriter to be finished.</param>
        public Task FinishVideoWriterAsync(SeeingSharpVideoWriter videoWriter)
        {
            videoWriter.EnsureNotNull(nameof(videoWriter));

            if (videoWriter.AssociatedRenderLoop != this)
            {
                throw new SeeingSharpGraphicsException("The given VideoWriter is not associated with this RenderLoop!");
            }

            var result = new TaskCompletionSource<object>();
            m_afterPresentActions.Enqueue(() =>
            {
                try
                {
                    if (videoWriter.AssociatedRenderLoop != this) { throw new SeeingSharpGraphicsException("The given VideoWriter is not associated with this RenderLoop!"); }

                    // Try to finish rendering first
                    if (videoWriter.HasStarted)
                    {
                        videoWriter.FinishRendering();
                    }

                    // Remote the VideoWriter from this RenderLoop
                    m_videoWriters.Remove(videoWriter);
                    videoWriter.AssociatedRenderLoop = null;

                    result.SetResult(null);
                }
                catch (Exception ex)
                {
                    result.SetException(ex);
                }
            });

            return result.Task;
        }

        /// <summary>
        /// Applies the given VideoWriter to this RenderLoop.
        /// </summary>
        /// <param name="videoWriter">The VideoWriter to be applied.</param>
        public Task RegisterVideoWriterAsync(SeeingSharpVideoWriter videoWriter)
        {
            videoWriter.EnsureNotNull(nameof(videoWriter));

            if (m_currentScene == null) { throw new SeeingSharpGraphicsException("No scene set to RenderLoop!"); }
            if (m_currentDevice == null) { throw new SeeingSharpGraphicsException("No device associated to RenderLoop!"); }
            if (videoWriter.AssociatedRenderLoop != null) { throw new SeeingSharpGraphicsException("Given VideoWriter is associated to another RenderLoop!"); }

            var result = new TaskCompletionSource<object>();
            m_afterPresentActions.Enqueue(() =>
            {
                try
                {
                    if (videoWriter.AssociatedRenderLoop == this) { return; }
                    if (videoWriter.AssociatedRenderLoop != null) { throw new SeeingSharpGraphicsException("Given VideoWriter is associated to another RenderLoop!"); }

                    // Apply the VideoWriter to this RenderLoop
                    m_videoWriters.Add(videoWriter);
                    videoWriter.AssociatedRenderLoop = this;

                    result.SetResult(null);
                }
                catch (Exception ex)
                {
                    result.SetException(ex);
                }
            });

            return result.Task;
        }

        /// <summary>
        /// Registers the given drawing layer that is used for 2d rendering.
        /// </summary>
        /// <param name="drawingLayer">The layer to be registered.</param>
        public Task Register2DDrawingLayerAsync(Custom2DDrawingLayer drawingLayer)
        {
            drawingLayer.EnsureNotNull(nameof(drawingLayer));

            var result = new TaskCompletionSource<object>();

            m_afterPresentActions.Enqueue(() =>
            {
                try
                {
                    if (!m_2dDrawingLayers.Contains(drawingLayer))
                    {
                        m_2dDrawingLayers.Add(drawingLayer);

                        result.SetResult(null);
                    }
                }
                catch (Exception ex)
                {
                    result.SetException(ex);
                }
            });

            return result.Task;
        }

        /// <summary>
        /// Removes all currently registered drawing layers from this render loop.
        /// </summary>
        public Task Clear2DDrawingLayersAsync()
        {
            var result = new TaskCompletionSource<object>();

            m_afterPresentActions.Enqueue(() =>
            {
                try
                {
                    m_2dDrawingLayers.Clear();
                    result.SetResult(null);
                }
                catch (Exception ex)
                {
                    result.SetException(ex);
                }
            });

            return result.Task;
        }

        /// <summary>
        /// Deregisters the given drawing layer from this RenderLoop.
        /// </summary>
        /// <param name="drawingLayer">The drawing layer to be deregistered.</param>
        public Task Deregister2DDrawingLayerAsync(Custom2DDrawingLayer drawingLayer)
        {
            drawingLayer.EnsureNotNull(nameof(drawingLayer));

            var result = new TaskCompletionSource<object>();

            m_afterPresentActions.Enqueue(() =>
            {
                try
                {
                    m_2dDrawingLayers.Remove(drawingLayer);
                }
                catch (Exception ex)
                {
                    result.SetException(ex);
                }
            });

            return result.Task;
        }

        /// <summary>
        /// Gets the object on the given pixel location (location local to this control).
        /// </summary>
        /// <param name="pickingOptions">Options for picking logic.</param>
        /// <param name="pixelLocation">X, Y location of the cursor in pixels.</param>
        public async Task<List<SceneObject>> PickObjectAsync(Point pixelLocation, PickingOptions pickingOptions)
        {
            List<SceneObject> result = null;

            if (m_currentScene != null &&
                m_camera != null)
            {
                var projectionMatrix = m_camera.Projection;

                Vector3 pickingVector;
                pickingVector.X = (2.0f * pixelLocation.X / m_currentViewSize.Width - 1) / projectionMatrix.M11;
                pickingVector.Y = -(2.0f * pixelLocation.Y / m_currentViewSize.Height - 1) / projectionMatrix.M22;
                pickingVector.Z = 1f;

                var worldMatrix = Matrix.Identity;
                var viewWorld = m_camera.View * worldMatrix;
                Matrix inversionViewWorld;
                Matrix.Invert(ref viewWorld, out inversionViewWorld);

                var rayDirection = Vector3.Normalize(new Vector3(
                    pickingVector.X * inversionViewWorld.M11 + pickingVector.Y * inversionViewWorld.M21 + pickingVector.Z * inversionViewWorld.M31,
                    pickingVector.X * inversionViewWorld.M12 + pickingVector.Y * inversionViewWorld.M22 + pickingVector.Z * inversionViewWorld.M32,
                    pickingVector.X * inversionViewWorld.M13 + pickingVector.Y * inversionViewWorld.M23 + pickingVector.Z * inversionViewWorld.M33));
                var rayStart = new Vector3(
                    inversionViewWorld.M41,
                    inversionViewWorld.M42,
                    inversionViewWorld.M43);

                // Pick objects async in update thread
                await m_currentScene.PerformBesideRenderingAsync(() =>
                {
                    if (m_currentScene != null &&
                        ViewInformation != null &&
                        m_currentScene.IsViewRegistered(ViewInformation))
                    {
                        result = m_currentScene.Pick(rayStart, rayDirection, ViewInformation, pickingOptions);
                    }
                    else
                    {
                        result = new List<SceneObject>();
                    }
                });
            }

            return result;
        }

        /// <summary>
        /// Changes the ViewConfiguration object.
        /// </summary>
        /// <param name="newViewConfiguration">The new configuration object to be applied.</param>
        public Task ExchangeViewConfigurationAsync(GraphicsViewConfiguration newViewConfiguration)
        {
            newViewConfiguration.EnsureNotNull(nameof(newViewConfiguration));

            var result = new TaskCompletionSource<object>();

            m_afterPresentActions.Enqueue(() =>
            {
                var deviceConfig = m_viewConfiguration.DeviceConfiguration;

                m_viewConfiguration = newViewConfiguration;
                m_viewConfiguration.DeviceConfiguration = deviceConfig;
                m_viewConfiguration.ViewNeedsRefresh = true;

                result.TrySetResult(null);
            });

            return result.Task;
        }

#if DESKTOP

        /// <summary>
        /// Takes a screenshot and returns it as a gdi bitmap.
        /// </summary>
        public Task<GDI.Bitmap> GetScreenshotGdiAsync()
        {
            TaskCompletionSource<GDI.Bitmap> result = new TaskCompletionSource<GDI.Bitmap>();

            m_afterPresentActions.Enqueue(() =>
            {
                try
                {
                    GDI.Bitmap resultBitmap = GetScreenshotGdiInternal();

                    result.SetResult(resultBitmap);
                }
                catch (Exception ex)
                {
                    result.SetException(ex);
                }
            });

            return result.Task;
        }

        /// <summary>
        /// Gets a screenshot in form of a gdi bitmap.
        /// (be careful. This call is executed synchronous.
        /// </summary>
        internal GDI.Bitmap GetScreenshotGdiInternal()
        {
            // Concept behind this see http://www.rolandk.de/wp/2013/06/inhalt-der-rendertarget-textur-in-ein-bitmap-kopieren/
            int width = m_currentViewSize.Width;
            int height = m_currentViewSize.Height;
            if (width <= 0) { throw new InvalidOperationException("View not initialized correctly!"); }
            if (height <= 0) { throw new InvalidOperationException("View not initialized correctly!"); }

            //Get and read data from the gpu (create copy helper texture on demand)
            if (m_copyHelperTextureStaging == null)
            {
                m_copyHelperTextureStaging = GraphicsHelper.CreateStagingTexture(m_currentDevice, width, height);
                m_copyHelperTextureStandard = GraphicsHelper.CreateTexture(m_currentDevice, width, height);
            }
            m_currentDevice.DeviceImmediateContextD3D11.ResolveSubresource(m_renderTarget, 0, m_copyHelperTextureStandard, 0, GraphicsHelper.DEFAULT_TEXTURE_FORMAT);
            m_currentDevice.DeviceImmediateContextD3D11.CopyResource(m_copyHelperTextureStandard, m_copyHelperTextureStaging);

            // Load the bitmap
            GDI.Bitmap resultBitmap = GraphicsHelper.LoadBitmapFromStagingTexture(m_currentDevice, m_copyHelperTextureStaging, width, height);
            return resultBitmap;
        }

#endif

        /// <summary>
        /// Resets the render counter to zero.
        /// </summary>
        public void ResetRenderCounter()
        {
            m_totalRenderCount = 0;
        }

        /// <summary>
        /// Refreshes the view resources.
        /// </summary>
        private void RefreshViewResources()
        {
            if (m_currentDevice == null) { return; }

            // Unload current view resources first
            UnloadViewResources();

            // Recreate view resources
            var generatedViewResources = m_renderLoopHost.OnRenderLoop_CreateViewResources(m_currentDevice);
            if (generatedViewResources == null) { return; }
            m_renderTarget = generatedViewResources.Item1;
            m_renderTargetView = generatedViewResources.Item2;
            m_renderTargetDepth = generatedViewResources.Item3;
            m_renderTargetDepthView = generatedViewResources.Item4;
            m_viewport = generatedViewResources.Item5;
            m_currentViewSize = generatedViewResources.Item6;
            m_targetSize = m_currentViewSize;
            m_currentDpiScaling = generatedViewResources.Item7;
            m_currentViewSizeDpiScaled = new Size2F(
                m_currentViewSize.Width / m_currentDpiScaling.ScaleFactorX,
                m_currentViewSize.Height / m_currentDpiScaling.ScaleFactorY);

            // Update view size on camera
            if (m_camera != null) { m_camera.SetScreenSize(m_currentViewSize.Width, m_currentViewSize.Height); }

            m_viewConfiguration.ViewNeedsRefresh = false;

            // Try to create a Direct2D overlay
            m_d2dOverlay = new Direct2DOverlayRenderer(
                m_currentDevice,
                m_renderTarget,
                m_currentViewSize.Width, m_currentViewSize.Height,
                m_currentDpiScaling);

            // Create or update current renderstate
            if (m_renderState == null ||
                m_renderState.Device != m_currentDevice)
            {
                m_renderState = new RenderState(
                    m_currentDevice,
                    GraphicsCore.Current.PerformanceCalculator,
                    new RenderTargets(m_renderTargetView, m_renderTargetDepthView),
                    m_viewport, m_camera, ViewInformation);
            }
        }

        /// <summary>
        /// Unload all loaded ViewResources.
        /// This call ensures that it will be executed in the correct UI thread.
        /// </summary>
        internal async Task UnloadViewResourcesAsync()
        {
            await UISynchronizationContext.PostAsync(() =>
            {
                UnloadViewResources();
            });
        }

        /// <summary>
        /// Unloads all view resources (is also called from EngineMainLoop in a synchronized execution block).
        /// </summary>
        internal void UnloadViewResources()
        {
            // TODO: Do this action synchronized..

            m_lastRenderSuccessfully = false;

            if (m_currentDevice == null) { return; }

            // Dispose resources of parent object first
            m_renderLoopHost.OnRenderLoop_DisposeViewResources(m_currentDevice);

            // Free direct2d render target (if created)
            SeeingSharpTools.SafeDispose(ref m_d2dOverlay);

            // Set values to initial
            m_renderTarget = null;
            m_renderTargetView = null;
            m_renderTargetDepth = null;
            m_renderTargetDepthView = null;
            m_viewport = new RawViewportF();
            m_currentViewSize = new Size2(SeeingSharpConstants.MIN_VIEW_WIDTH, SeeingSharpConstants.MIN_VIEW_HEIGHT);

            // Dispose local resources
            SeeingSharpTools.SafeDispose(ref m_copyHelperTextureStaging);
            SeeingSharpTools.SafeDispose(ref m_copyHelperTextureStandard);
        }

        /// <summary>
        /// Internal method that resets some flags directly before rendering.
        /// </summary>
        internal void ResetFlagsBeforeRendering()
        {
            // Handle VisibleObjectCount field
            VisibleObjectCount = VisibleObjectCountInternal;
            VisibleObjectCountInternal = 0;
        }

        ///// <summary>
        ///// Queries for all view related input states (e. g. Mouse, Touch, Keyboard).
        ///// </summary>
        //internal async Task<List<InputStateBase>> QueryViewRelatedInputStateAsync()
        //{
        //    // Query all states
        //    List<InputStateBase> result = new List<InputStateBase>(10);
        //    await m_guiSyncContext.PostAsync(() =>
        //    {
        //        foreach(InputStateBase actInputState in m_renderLoopHost.OnRenderLoop_QueryInputStates())
        //        {
        //            if(actInputState == null) { continue; }

        //            InputStateBase actQueriedObject = actInputState.CopyAndResetForUpdatePass();
        //            actQueriedObject.RelatedView = m_viewInformation;
        //            result.Add(actQueriedObject);
        //        }
        //    });

        //    return result;
        //}

        /// <summary>
        /// Prepares rendering (refreshes view resources, post last rendered image to the view, ...)
        /// </summary>
        internal async Task<List<Action>> PrepareRenderAsync()
        {
            var continuationActions = new List<Action>();
            if (DiscardRendering) { return continuationActions; }

            var writeVideoFrames = m_lastRenderSuccessfully;

            // Call present from Threadpool (if configured)
            if (!m_callPresentInUiThread)
            {
                if (!DiscardPresent) { PresentFrameInternal(); }
                m_lastRenderSuccessfully = false;
            }

            await UISynchronizationContext.PostAsync(() =>
            {
                // Call present from UI thread (if configured)
                if (m_callPresentInUiThread)
                {
                    if (!DiscardPresent) { PresentFrameInternal(); }
                    m_lastRenderSuccessfully = false;
                }

                if (DiscardRendering) { return; }

                // Update view frustum
                ViewInformation.UpdateFrustum(m_camera.ViewProjection);

                // Allow next rendering by default
                m_nextRenderAllowed = true;

                // Need to update view parameters?              (=> Later: Render Thread)
                var viewSizeChanged = m_targetSize != m_currentViewSize;
                if (m_renderTargetView == null ||
                    viewSizeChanged ||
                    m_targetDevice != null && m_targetDevice != m_currentDevice ||
                    m_viewConfiguration.ViewNeedsRefresh ||
                    m_viewRefreshForced)
                {
                    m_viewRefreshForced = false;

                    try
                    {
                        // Trigger deregister on scene if needed
                        var reregisterOnScene = m_targetDevice != null && m_targetDevice != m_currentDevice && m_currentScene != null;

                        if (reregisterOnScene)
                        {
                            var localScene = m_currentScene;
                            continuationActions.Add(() => localScene.DeregisterView(ViewInformation));
                        }

                        // Unload view resources first
                        UnloadViewResources();

                        // Update device ob object (size is updated in RefreshViewResources)
                        if (m_targetDevice != null)
                        {
                            m_currentDevice = m_targetDevice;
                            m_targetDevice = null;

                            DeviceChanged.Raise(this, EventArgs.Empty);
                        }

                        // Load view resources again
                        RefreshViewResources();

                        // Trigger reregister on scene if needed
                        if (reregisterOnScene)
                        {
                            var localScene = m_currentScene;
                            continuationActions.Add(() => localScene.RegisterView(ViewInformation));
                        }

                        if (viewSizeChanged)
                        {
                            CurrentViewSizeChanged?.Invoke(this, EventArgs.Empty);
                        }
                    }
                    catch (Exception ex)
                    {
                        m_nextRenderAllowed = false;

                        throw new SeeingSharpGraphicsException("Unable to refresh view on device " + m_currentDevice + "!", ex);
                    }
                }

                // Check needed resources
                if (m_currentDevice == null ||
                    m_renderTargetView == null ||
                    m_renderTargetDepthView == null)
                {
                    m_nextRenderAllowed = false;
                    return;
                }

                // Handle changed scene
                if (m_targetScene != null &&
                    m_currentScene != m_targetScene)
                {
                    try
                    {
                        // Trigger deregister on the current scene
                        if (m_currentScene != null)
                        {
                            var localScene = m_currentScene;
                            continuationActions.Add(() => localScene.DeregisterView(ViewInformation));

                            foreach (var actComponent in SceneComponents)
                            {
                                localScene.DetachComponent(actComponent, ViewInformation);
                            }
                        }

                        // Change scene property
                        m_currentScene = m_targetScene;
                        m_targetScene = null;

                        // Trigger reregister on the new scene
                        if (m_currentScene != null)
                        {
                            var localScene = m_currentScene;
                            continuationActions.Add(() => localScene.RegisterView(ViewInformation));

                            foreach (var actComponent in SceneComponents)
                            {
                                localScene.AttachComponent(actComponent, ViewInformation);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        m_nextRenderAllowed = false;

                        throw new SeeingSharpGraphicsException("Unable to change the scene!", ex);
                    }
                }
                // Ensure that this loop is registered on the current view
                else if (m_currentScene != null &&
                        !m_currentScene.IsViewRegistered(ViewInformation))
                {
                    var localScene = m_currentScene;
                    continuationActions.Add(() => localScene.RegisterView(ViewInformation));
                }

                // Check needed resources
                if (m_currentScene == null)
                {
                    m_nextRenderAllowed = false;
                    return;
                }

                try
                {
                    // Check here wether we can render or not
                    if (!m_renderLoopHost.OnRenderLoop_CheckCanRender(m_currentDevice))
                    {
                        m_nextRenderAllowed = false;
                        return;
                    }

                    // Perform some preparation for rendering
                    m_renderLoopHost.OnRenderLoop_PrepareRendering(m_currentDevice);
                }
                catch (Exception ex)
                {
                    m_nextRenderAllowed = false;
                    throw new SeeingSharpGraphicsException("Unable to prepare rendering", ex);
                }

                // Let UI manipulate current filter list
                ManipulateFilterList.Raise(this, new ManipulateFilterListArgs(Filters));
            });

            // Draw all frames to registered VideoWriters
            if (writeVideoFrames)
            {
                DrawVideoFrames();
            }

            return continuationActions;
        }

        /// <summary>
        /// This method call draws the current frame into all registered video writers.
        /// </summary>
        internal void DrawVideoFrames()
        {
            // Write current frame if we have an associated video writer
            if (m_videoWriters.Count > 0)
            {
                using (var texUploader = new TextureUploader(m_currentDevice, m_renderTarget))
                using (var mappedTexture = new MemoryMappedTexture32bpp(m_currentViewSize))
                {
                    // Upload texture
                    texUploader.UploadToIntBuffer(mappedTexture);

                    // Render the texture to all video writers
                    Parallel.For(0, m_videoWriters.Count, actIndex =>
                    {
                        var actVideoWriter = m_videoWriters[actIndex];

                        // Start video rendering if not done so before
                        if (!actVideoWriter.HasStarted)
                        {
                            actVideoWriter.StartRendering(m_currentViewSize);
                        }

                        // Render current frame
                        if (actVideoWriter.HasStarted)
                        {
                            actVideoWriter.DrawFrame(m_currentDevice, mappedTexture);
                        }
                    });
                }

                // Check for occurred errors (Cancel writing if there was any)
                for (var loop = 0; loop < m_videoWriters.Count; loop++)
                {
                    var actWriter = m_videoWriters[loop];

                    if (actWriter.LastDrawException != null ||
                        actWriter.LastStartException != null ||
                        actWriter.LastFinishException != null)
                    {
                        // Try to finish rendering first
                        actWriter.FinishRendering();

                        // Remote the VideoWriter from this RenderLoop
                        m_videoWriters.RemoveAt(loop);
                        actWriter.AssociatedRenderLoop = null;
                        loop--;
                    }
                }
            }
        }

        /// <summary>
        /// Presents the current frame.
        /// </summary>
        private void PresentFrameInternal()
        {
            // Post last frame to the screen if rendering was successful.
            //  For now: Do this in render thread. This should be possible
            //  see: http://msdn.microsoft.com/en-us/library/windows/desktop/bb205075(v=vs.85).aspx#Multithread_Considerations
            if (m_lastRenderSuccessfully)
            {
                try
                {
                    // Presents all contents on the screen
                    GraphicsCore.Current.ExecuteAndMeasureActivityDuration(
                        string.Format(SeeingSharpConstants.PERF_RENDERLOOP_PRESENT, m_currentDevice.DeviceIndex, ViewInformation.ViewIndex + 1),
                        () => m_renderLoopHost.OnRenderLoop_Present(m_currentDevice));

                    // Execute all deferred actions to be called after present
                    m_afterPresentActions.DequeueAll().ForEachInEnumeration(actAction => actAction());

                    // Finish rendering now
                    m_renderLoopHost.OnRenderLoop_AfterRendering(m_currentDevice);
                }
                catch (Exception ex)
                {
                    throw new SeeingSharpGraphicsException("Unable to present a view with device " + m_currentDevice + "!", ex);
                }
            }
        }

        /// <summary>
        /// Renders this instance.
        /// </summary>
        internal void Render()
        {
            if (DiscardRendering) { return; }
            if (!m_nextRenderAllowed) { return; }
            m_nextRenderAllowed = false;

            var renderTimeMeasurenment = GraphicsCore.Current.BeginMeasureActivityDuration(
                string.Format(SeeingSharpConstants.PERF_RENDERLOOP_RENDER, m_currentDevice.DeviceIndex, ViewInformation.ViewIndex + 1));
            try
            {
                // Handle all resources within the scene
                if (m_currentScene != null &&
                    m_camera != null &&
                    m_currentScene.IsViewRegistered(ViewInformation))
                {
                    // Renders current scene on this view
                    m_currentScene.HandleRenderResources(m_renderState);
                }

                // Update render state
                m_renderState.Reset(
                    new RenderTargets(m_renderTargetView, m_renderTargetDepthView),
                    m_viewport, m_camera, ViewInformation);
                m_renderState.ApplyMaterial(null);

                // Paint using Direct3D
                m_currentDevice.DeviceImmediateContextD3D11.ClearRenderTargetView(m_renderTargetView, ClearColor);
                m_currentDevice.DeviceImmediateContextD3D11.ClearDepthStencilView(m_renderTargetDepthView, D3D11.DepthStencilClearFlags.Depth | D3D11.DepthStencilClearFlags.Stencil, 1f, 0);

                // Render currently configured scene
                if (m_currentScene != null &&
                    m_camera != null &&
                    m_currentScene.IsViewRegistered(ViewInformation))
                {
                    // Renders current scene on this view
                    m_currentScene.Render(m_renderState);
                }

                // Clear current state after rendering
                m_renderState.ClearState();

                if (m_totalRenderCount < int.MaxValue) { m_totalRenderCount++; }

                // Render 2D overlay if possible (may be not available on some older OS or older graphics cards)
                if (m_d2dOverlay != null &&
                    m_d2dOverlay.IsLoaded)
                {
                    var d2dOverlayTime = GraphicsCore.Current.PerformanceCalculator.BeginMeasureActivityDuration(
                        string.Format(SeeingSharpConstants.PERF_RENDERLOOP_RENDER_2D, m_currentDevice.DeviceIndex, ViewInformation.ViewIndex + 1));
                    m_d2dOverlay.BeginDraw();
                    try
                    {
                        m_renderState.RenderTarget2D = m_d2dOverlay.RenderTarget2D;
                        m_renderState.Graphics2D = m_d2dOverlay.Graphics;

                        // Render scene contents
                        m_currentScene.Render2DOverlay(m_renderState);

                        // Perform rendering of custom 2D drawing layers
                        foreach (var act2DLayer in m_2dDrawingLayers)
                        {
                            act2DLayer.Draw2DInternal(m_d2dOverlay.Graphics);
                        }

                        // Draw debug layer if created
                        if (m_debugDrawingLayer != null)
                        {
                            m_debugDrawingLayer.Draw2DInternal(m_d2dOverlay.Graphics);
                        }
                    }
                    finally
                    {
                        m_renderState.RenderTarget2D = null;
                        m_renderState.Graphics2D = null;

                        m_d2dOverlay.EndDraw();
                        d2dOverlayTime.Dispose();
                    }
                }

                // Send all draw calls to the device and wait until finished
                m_currentDevice.DeviceImmediateContextD3D11.Flush();

                // Update flag indicating that last render was successful
                m_lastRenderSuccessfully = true;

                try
                {
                    Rendered.Raise(this, EventArgs.Empty);
                }
                catch
                {
                    // TODO: Notify this exception somehow to the host application
                }
            }
            finally
            {
                renderTimeMeasurenment.Dispose();
            }
        }

        /// <summary>
        /// Deregisters this view from the render loop.
        /// </summary>
        public void DeregisterRenderLoop()
        {
            if (!GraphicsCore.IsLoaded) { return; }

            // Deregister this Renderloop object
            if (IsRegisteredOnMainLoop)
            {
                GraphicsCore.Current.MainLoop.DeregisterRenderLoop(this);

                var inputInterface = m_renderLoopHost as IInputEnabledView;

                if (inputInterface != null)
                {
                    GraphicsCore.Current.InputGatherer.DeregisterView(inputInterface);
                }
            }
        }

        /// <summary>
        /// Registers this view on the render loop.
        /// </summary>
        public void RegisterRenderLoop()
        {
            if (!GraphicsCore.IsLoaded) { return; }

            // Deregister this Renderloop object
            if (!IsRegisteredOnMainLoop)
            {
                GraphicsCore.Current.MainLoop.RegisterRenderLoop(this);

                var inputInterface = m_renderLoopHost as IInputEnabledView;

                if (inputInterface != null)
                {
                    GraphicsCore.Current.InputGatherer.RegisterView(inputInterface);
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!GraphicsCore.IsLoaded) { return; }

            // Deregister this Renderloop object
            GraphicsCore.Current.MainLoop.DeregisterRenderLoop(this);

            SeeingSharpTools.SafeDispose(ref m_debugDrawingLayer);

            // Assign new dummy camera
            //  (this call detaches the previous camera from this renderloop)
            Camera = new PerspectiveCamera3D();
        }

        /// <summary>
        /// Cleaing for currently registered components with equal group names.
        /// </summary>
        private void CleanSceneComponentList()
        {
            if (m_currentScene != null)
            {
                throw new SeeingSharpException("Internal error: CleanSceneComponentList must only be called when current scene = null!");
            }

            var componentGroups = new List<string>(SceneComponents.Count);
            var sceneComponentCount = SceneComponents.Count;
            for (var loop = sceneComponentCount - 1; loop >= 0; loop--)
            {
                var actComponent = SceneComponents[loop];
                if (!string.IsNullOrEmpty(actComponent.ComponentGroup))
                {
                    if (componentGroups.Contains(actComponent.ComponentGroup))
                    {
                        SceneComponents.RemoveAt(loop);
                    }
                    else
                    {
                        componentGroups.Add(actComponent.ComponentGroup);
                    }
                }
            }
        }

        /// <summary>
        /// Called when the collection of SceneComponents has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void OnSceneComponents_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            var actScene = m_currentScene;

            var componentsToRemove = new List<SceneComponentBase>();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    if (e.NewItems != null)
                    {
                        foreach (SceneComponentBase actComponent in e.NewItems)
                        {
                            // Get all components with the same group-name
                            //  .. we've to remove them later
                            if (!string.IsNullOrEmpty(actComponent.ComponentGroup))
                            {
                                componentsToRemove.AddRange(
                                    SceneComponents.Where(
                                        actInnerComponent =>
                                            actInnerComponent.ComponentGroup == actComponent.ComponentGroup &&
                                            actInnerComponent != actComponent));
                            }

                            // Attach this new component
                            if (actScene != null)
                            {
                                actScene.AttachComponent(actComponent, ViewInformation);
                            }
                        }
                    }
                    if (e.OldItems != null)
                    {
                        if (actScene != null)
                        {
                            foreach (SceneComponentBase actComponent in e.OldItems)
                            {
                                actScene.DetachComponent(actComponent, ViewInformation);
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    break;
            }

            // Remove components which where replaced by new ones
            if (componentsToRemove.Count > 0)
            {
                foreach (var actComponentToRemove in componentsToRemove)
                {
                    SceneComponents.Remove(actComponentToRemove);
                }
            }
        }

        /// <summary>
        /// Gets an identifyer related to this render looop.
        /// </summary>
        public ViewInformation ViewInformation { get; }

        /// <summary>
        /// Gets the current view configuration.
        /// </summary>
        public GraphicsViewConfiguration ViewConfiguration => m_viewConfiguration;

        /// <summary>
        /// Gets the current scene object.
        /// </summary>
        public Scene Scene
        {
            get
            {
                if (m_targetScene != null) { return m_targetScene; }
                return m_currentScene;
            }
        }

        public ObservableCollection<SceneComponentBase> SceneComponents { get; }

        /// <summary>
        /// Gets the current target scene.
        /// </summary>
        internal Scene TargetScene => m_targetScene;

        public int TotalRenderCount => m_totalRenderCount;

        /// <summary>
        /// Are view resources loaded?
        /// </summary>
        public bool ViewResourcesLoaded => m_renderTarget != null;

        /// <summary>
        /// Discard rendering?
        /// </summary>
        public bool DiscardRendering { get; set; }

        public bool DiscardPresent { get; set; }

        /// <summary>
        /// Gets the current SynchronizationContext.
        /// </summary>
        public SynchronizationContext UISynchronizationContext { get; }

        /// <summary>
        /// Gets or sets the current clear color.
        /// </summary>
        public Color4 ClearColor { get; set; }

        /// <summary>
        /// Gets the collection containing all filters.
        /// </summary>
        internal List<SceneObjectFilter> Filters { get; }

        /// <summary>
        /// Counts the currently applied video writers.
        /// </summary>
        public int CountVideoWriters => m_videoWriters.Count;

        /// <summary>
        /// Gets or sets current camera object.
        /// </summary>
        public Camera3DBase Camera
        {
            get => m_camera;
            set
            {
                if (m_camera != value)
                {
                    // Reset AssociatedRenderLoop flag on previous one
                    if (m_camera != null)
                    {
                        m_camera.AssociatedRenderLoop = null;
                    }

                    // Change the current camera reference
                    Camera3DBase newCamera = null;
                    newCamera = value ?? new PerspectiveCamera3D();
                    if (newCamera.AssociatedRenderLoop != null)
                    {
                        throw new SeeingSharpGraphicsException("Unable to change camera: The given one is already associated to another RenderLoop!");
                    }
                    m_camera = newCamera;

                    // Set AssociatedRenderLoop flag on new one
                    if (m_camera != null)
                    {
                        m_camera.SetScreenSize(CurrentViewSize.Width, CurrentViewSize.Height);
                        m_camera.AssociatedRenderLoop = this;
                    }

                    CameraChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the current view size in pixels.
        /// </summary>
        public Size2 CurrentViewSize => m_currentViewSize;

        /// <summary>
        /// Gets the device this renderloop is using.
        /// </summary>
        public EngineDevice Device => m_currentDevice;

        /// <summary>
        /// Gets the total count of visible objects.
        /// </summary>
        public int VisibleObjectCount
        {
            get;
            private set;
        }

        /// <summary>
        /// True if the <see cref="RenderLoop"/> is connected with the main rendering loop.
        /// False if something went wrong during initialization.
        /// </summary>
        public bool IsOperational
        {
            get
            {
                if (!GraphicsCore.IsLoaded) { return false; }
                if (GraphicsCore.Current.DeviceCount <= 0) { return false; }
                return IsRegisteredOnMainLoop;
            }
        }

        /// <summary>
        /// Is this RenderLoop registered on the main loop?
        /// </summary>
        public bool IsRegisteredOnMainLoop
        {
            get;
            internal set;
        }

        /// <summary>
        /// Internal properties and methods that sould be used with care.
        /// </summary>
        public RenderLoopInternals Internals { get; }

        /// <summary>
        /// Internal field that is used to count visible objects.
        /// </summary>
        internal int VisibleObjectCountInternal;

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Internal members of the <see cref="RenderLoop"/> class.
        /// Be carefull when working with them.
        /// </summary>
        public class RenderLoopInternals
        {
            private RenderLoop m_target;

            public void UnloadViewResources()
            {
                m_target.UnloadViewResources();
            }

            public RenderLoopInternals(RenderLoop target)
            {
                m_target = target;
            }

            public D3D11.Texture2D RenderTarget => m_target.m_renderTarget;

            public D3D11.Texture2D RenderTargetDepth => m_target.m_renderTargetDepth;

            public D3D11.Texture2D CopyHelperTextureStaging
            {
                get => m_target.m_copyHelperTextureStaging;
                set => m_target.m_copyHelperTextureStaging = value;
            }

            public D3D11.Texture2D CopyHelperTextureStandard
            {
                get => m_target.m_copyHelperTextureStandard;
                set => m_target.m_copyHelperTextureStandard = value;
            }

            public bool CallPresentInUIThread
            {
                get => m_target.m_callPresentInUiThread;
                set => m_target.m_callPresentInUiThread = value;
            }
        }
    }
}