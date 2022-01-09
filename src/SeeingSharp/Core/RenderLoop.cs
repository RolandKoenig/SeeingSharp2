using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Core.Devices;
using SeeingSharp.Drawing2D;
using SeeingSharp.Drawing3D;
using SeeingSharp.DrawingVideo;
using SeeingSharp.Input;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;
using SharpGen.Runtime;
using Vortice.DXGI;
using BoundingBox = SeeingSharp.Mathematics.BoundingBox;
using Color4 = SeeingSharp.Mathematics.Color4;
using D2D = Vortice.Direct2D1;
using D3D11 = Vortice.Direct3D11;
using Viewport = Vortice.Mathematics.Viewport;

namespace SeeingSharp.Core
{
    /// <summary>
    /// This class controls rendering logic behind each view object.
    /// </summary>
    public class RenderLoop : IDisposable
    {
        // Configuration values
        private GraphicsViewConfiguration _configuration;
        private Camera3DBase _camera;

        // Async actions
        private ConcurrentQueue<Action> _afterPresentActions;
        private ConcurrentQueue<TaskCompletionSource<RenderPassDump>> _dumpRequests;

        // Target parameters for rendering
        private EngineDevice _targetDevice;
        private Size _targetSize;
        private DpiScaling _currentDpiScaling;
        private Scene? _targetScene;
        private bool _viewRefreshForced;
        private bool _reregisterViewOnSceneForced;

        // Callback methods for current host object
        private IRenderLoopHost _renderLoopHost;

        // Values needed for runtime
        private bool _loadCalled;
        private bool _lastRenderSuccessfully;
        private bool _nextRenderAllowed;
        private int _totalRenderCount;
        private RenderState _renderState;
        private List<SeeingSharpVideoWriter> _videoWriters;
        private bool _callPresentInUiThread;
        private ObjectFilterCollection _objectFilters;

        // Direct3D resources and other values gathered during graphics loading
        private UnsafeList<Custom2DDrawingLayer> _2dDrawingLayers;
        private DebugDrawingLayer _debugDrawingLayer;
        private EngineDevice _currentDevice;
        private Size _currentViewSize;

        private Scene _currentScene;
        private Direct2DOverlayRenderer _d2dOverlay;
        private D3D11.ID3D11Texture2D _renderTarget;
        private D3D11.ID3D11Texture2D _renderTargetDepth;
        private D3D11.ID3D11RenderTargetView _renderTargetView;
        private D3D11.ID3D11DepthStencilView _renderTargetDepthView;
        private Viewport _viewport;
        private int _loadDeviceIndex;

        // Cached values
        private Action _cachedPrepareRenderOnGui;
        private List<Action> _cachedPrepareRenderContinuationActions;
        private string _cachedPerfRenderLoopRender;
        private string _cachedPerfRenderLoopRender2D;
        private string _cachedPerfRenderLoopPresent;

        // Direct3D resources for rendertarget capturing
        // ----
        // A staging texture for reading contents by Cpu
        // A standard texture for copying data from multisample texture to standard one
        // see http://www.rolandk.de/wp/2013/06/inhalt-der-rendertarget-textur-in-ein-bitmap-kopieren/
        private D3D11.ID3D11Texture2D _copyHelperTextureStaging;
        private D3D11.ID3D11Texture2D _copyHelperTextureStandard;

        /// <summary>
        /// Gets an identifier related to this render loop.
        /// </summary>
        public ViewInformation ViewInformation { get; }

        /// <summary>
        /// Gets the current view configuration.
        /// </summary>
        public GraphicsViewConfiguration Configuration => _configuration;

        /// <summary>
        /// Gets the current scene object.
        /// </summary>
        public Scene Scene
        {
            get
            {
                if (_targetScene != null) { return _targetScene; }
                return _currentScene;
            }
        }

        public ObservableCollection<SceneComponentBase> SceneComponents { get; }

        public int TotalRenderCount => _totalRenderCount;

        /// <summary>
        /// Are view resources loaded?
        /// </summary>
        public bool ViewResourcesLoaded => _renderTarget != null;

        /// <summary>
        /// Discard rendering?
        /// </summary>
        public bool DiscardRendering { get; set; }

        /// <summary>
        /// Gets the current SynchronizationContext.
        /// </summary>
        public SynchronizationContext UiSynchronizationContext { get; }

        /// <summary>
        /// Gets or sets the current clear color.
        /// </summary>
        public Color4 ClearColor { get; set; }

        /// <summary>
        /// Counts the currently applied video writers.
        /// </summary>
        public int CountVideoWriters => _videoWriters.Count;

        /// <summary>
        /// Gets or sets current camera object.
        /// </summary>
        public Camera3DBase Camera
        {
            get => _camera;
            set
            {
                if (_camera != value)
                {
                    // Reset AssociatedRenderLoop flag on previous one
                    if (_camera != null)
                    {
                        _camera.AssociatedRenderLoop = null;
                    }

                    // Change the current camera reference
                    Camera3DBase newCamera = null;
                    newCamera = value ?? new PerspectiveCamera3D();
                    if (newCamera.AssociatedRenderLoop != null)
                    {
                        throw new SeeingSharpGraphicsException("Unable to change camera: The given one is already associated to another RenderLoop!");
                    }
                    _camera = newCamera;

                    // Set AssociatedRenderLoop flag on new one
                    if (_camera != null)
                    {
                        _camera.SetScreenSize(this.CurrentViewSize.Width, this.CurrentViewSize.Height);
                        _camera.AssociatedRenderLoop = this;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current view size in pixels.
        /// </summary>
        public Size CurrentViewSize => _currentViewSize;

        /// <summary>
        /// Gets the device this renderloop is using.
        /// </summary>
        public EngineDevice Device => _currentDevice;

        /// <summary>
        /// Gets the total count of visible objects.
        /// </summary>
        public int CountVisibleObjects
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the total count of draw calls on last render.
        /// </summary>
        public int CountDrawCalls
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the total count of resources on the current device.
        /// </summary>
        public int CountGraphicsResources
        {
            get
            {
                var device = _currentDevice;
                var scene = _currentScene;
                if (device == null) { return 0; }
                if (scene == null) { return 0; }

                return scene.GetResourceCount(device);
            }
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
                return this.IsRegisteredOnMainLoop;
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
        /// The control which hosts this RenderLoop instance.
        /// </summary>
        public object RenderLoopHost => _renderLoopHost;

        /// <summary>
        /// Is the current device in DeviceLost state?
        /// </summary>
        public bool IsDeviceLost => _currentDevice?.IsLost == true;

        /// <summary>
        /// Gets a collection containing all object filters.
        /// </summary>
        public ObjectFilterCollection ObjectFilters => _objectFilters;

        /// <summary>
        /// Internal properties and methods that should be used with care.
        /// </summary>
        public RenderLoopInternals Internals { get; }

        /// <summary>
        /// Gets the current target scene.
        /// </summary>
        internal Scene TargetScene => _targetScene;

        /// <summary>
        /// Gets the collection containing all filters.
        /// </summary>
        internal List<SceneObjectFilter> FiltersInternal { get; }

        public event EventHandler CurrentViewSizeChanged;

        /// <summary>
        /// Raised when the corresponding device has changed.
        /// </summary>
        public event EventHandler DeviceChanged;

        /// <summary>
        /// Raised before start rendering a frame.
        /// This event is called within the UI thread.
        /// </summary>
        public event EventHandler PrepareRender;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderLoop" /> class.
        /// </summary>
        public RenderLoop(
            SynchronizationContext guiSyncContext,
            IRenderLoopHost renderLoopHost,
            bool isDesignMode = false)
        {
            this.Internals = new RenderLoopInternals(this);

            _afterPresentActions = new ConcurrentQueue<Action>();
            _dumpRequests = new ConcurrentQueue<TaskCompletionSource<RenderPassDump>>();
            _cachedPrepareRenderContinuationActions = new List<Action>();
            _cachedPrepareRenderOnGui = this.PrepareRenderOnGui;

            this.UiSynchronizationContext = guiSyncContext;

            this.SceneComponents = new ObservableCollection<SceneComponentBase>();
            this.SceneComponents.CollectionChanged += this.OnSceneComponents_Changed;

            this.FiltersInternal = new List<SceneObjectFilter>();
            _2dDrawingLayers = new UnsafeList<Custom2DDrawingLayer>();

            // Load DebugDrawingLayer if debug mode is enabled
            if (!isDesignMode)
            {
                if (GraphicsCore.IsLoaded &&
                    GraphicsCore.Current.IsDebugEnabled)
                {
                    _debugDrawingLayer = new DebugDrawingLayer();
                }
            }

            this.ViewInformation = new ViewInformation(this);
            _configuration = new GraphicsViewConfiguration();
            _objectFilters = new ObjectFilterCollection();

            _videoWriters = new List<SeeingSharpVideoWriter>();

            // Assign all given actions
            _renderLoopHost = renderLoopHost;

            // Create default objects
            this.ClearColor = Color4.White;

            // Set initial target parameters
            _targetDevice = null;
            _targetSize = new Size(0, 0);
            _loadDeviceIndex = -1;

            // Create default scene and camera
            this.Camera = new PerspectiveCamera3D();
            this.SetScene(new Scene());

            if (!GraphicsCore.IsLoaded) { return; }
            if (isDesignMode) { return; }

            // Apply default rendering device for this RenderLoop
            this.SetRenderingDevice(GraphicsCore.Current.DefaultDevice);
        }

        /// <summary>
        /// Dumps states of target buffers during rendering (like color- or depth buffer states).
        /// </summary>
        public Task<RenderPassDump> DumpRenderPassesAsync()
        {
            var taskSource = new TaskCompletionSource<RenderPassDump>(TaskCreationOptions.RunContinuationsAsynchronously);
            _dumpRequests.Enqueue(taskSource);
            return taskSource.Task;
        }

        /// <summary>
        /// Sets the current with and height of the view.
        /// The RenderLoop will apply the values later.
        /// </summary>
        /// <param name="width">Current width in pixels.</param>
        /// <param name="height">Current height in pixels.</param>
        public void SetCurrentViewSize(int width, int height)
        {
            width.EnsurePositiveOrZero(nameof(width));
            height.EnsurePositiveOrZero(nameof(height));

            if (width < SeeingSharpConstants.MIN_VIEW_WIDTH) { width = SeeingSharpConstants.MIN_VIEW_WIDTH; }
            if (height < SeeingSharpConstants.MIN_VIEW_HEIGHT) { height = SeeingSharpConstants.MIN_VIEW_HEIGHT; }

            _targetSize = new Size(width, height);
        }

        /// <summary>
        /// Gets the current index of this view within the current scene.
        /// </summary>
        public int GetViewIndex()
        {
            var viewInformation = this.ViewInformation;

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
            _targetDevice = device;
        }

        /// <summary>
        /// Sets the scene for rendering
        /// </summary>
        /// <param name="scene">The scene to be set.</param>
        public void SetScene(Scene scene)
        {
            scene.EnsureNotNull(nameof(scene));

            _targetScene = scene;
        }

        /// <summary>
        /// Forces the view to reload itself.
        /// </summary>
        public void ForceViewReload()
        {
            _configuration.ViewNeedsRefresh = true;
        }

        /// <summary>
        /// Enqueue the given action to be called after presenting the next frame.
        /// </summary>
        /// <param name="action">The action to be called.</param>
        public void EnqueueAfterPresentAction(Action action)
        {
            _afterPresentActions.Enqueue(action);
        }

        /// <summary>
        /// Waits for the next finished render process.
        /// </summary>
        public Task WaitForNextFinishedRenderAsync()
        {
            var result = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            _afterPresentActions.Enqueue(() =>
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
            return this.Register2DDrawingLayerAsync(result)
                .ContinueWith(givenTask => result);
        }

        /// <summary>
        /// Moves the camera to a default location which may see most of the warehouse.
        /// </summary>
        /// <param name="horizontalRotation">The horizontal rotation seen from the middle point of the scene.</param>
        /// <param name="verticalRotation">The vertical rotation seen from the middle point of the scene.</param>
        public async Task MoveCameraToDefaultLocationAsync(float horizontalRotation, float verticalRotation)
        {
            if (_currentScene == null) { return; }
            if (_currentDevice == null) { return; }

            var sceneBox = await this.CalculateSceneBoxAsync();

            if (sceneBox.IsEmpty()) { return; }
            if (_currentScene == null) { return; }
            if (_currentDevice == null) { return; }

            if (_camera != null)
            {
                await _currentScene.PerformBeforeUpdateAsync(() =>
                {
                    var bottomCenter = sceneBox.GetBottomCenter();
                    var anyEdge = sceneBox.GetTopFrontMiddleCoordinate();
                    var centerToEdgeVector = anyEdge - bottomCenter;
                    var centerToEdgeDistance = centerToEdgeVector.Length();

                    var direction = Vector3.TransformNormal(
                        new Vector3(0f, 0f, -1f),
                        Matrix4x4.CreateFromYawPitchRoll(horizontalRotation, verticalRotation, 0f));

                    _camera.Position = bottomCenter + direction * (centerToEdgeDistance * 2f);
                    _camera.Target = bottomCenter;
                    _camera.UpdateCamera();
                });
            }
        }

        /// <summary>
        /// Calculates a BoundingBox which contains all objects of the scene.
        /// </summary>
        public async Task<BoundingBox> CalculateSceneBoxAsync()
        {
            var result = default(BoundingBox);

            if (_currentScene == null) { return result; }
            if (_currentDevice == null) { return result; }

            await _currentScene.PerformBesideRenderingAsync(() =>
            {
                foreach (var actLayer in _currentScene.Layers)
                {
                    foreach (var actSpacialObject in actLayer.SpacialObjects)
                    {
                        var actBoundingBox = actSpacialObject.TryGetBoundingBox(this.ViewInformation);

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

            var result = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _afterPresentActions.Enqueue(() =>
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
                    _videoWriters.Remove(videoWriter);
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

            if (_currentScene == null) { throw new SeeingSharpGraphicsException("No scene set to RenderLoop!"); }
            if (_currentDevice == null) { throw new SeeingSharpGraphicsException("No device associated to RenderLoop!"); }
            if (videoWriter.AssociatedRenderLoop != null) { throw new SeeingSharpGraphicsException("Given VideoWriter is associated to another RenderLoop!"); }

            var result = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _afterPresentActions.Enqueue(() =>
            {
                try
                {
                    if (videoWriter.AssociatedRenderLoop == this) { return; }
                    if (videoWriter.AssociatedRenderLoop != null) { throw new SeeingSharpGraphicsException("Given VideoWriter is associated to another RenderLoop!"); }

                    // Apply the VideoWriter to this RenderLoop
                    _videoWriters.Add(videoWriter);
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

            var result = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            _afterPresentActions.Enqueue(() =>
            {
                try
                {
                    if (!_2dDrawingLayers.Contains(drawingLayer))
                    {
                        _2dDrawingLayers.Add(drawingLayer);

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
            var result = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            _afterPresentActions.Enqueue(() =>
            {
                try
                {
                    _2dDrawingLayers.Clear();
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

            var result = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            _afterPresentActions.Enqueue(() =>
            {
                try
                {
                    _2dDrawingLayers.Remove(drawingLayer);
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

            if (_currentScene != null &&
                _camera != null)
            {
                var projectionMatrix = _camera.Projection;

                Vector3 pickingVector;
                pickingVector.X = (2.0f * pixelLocation.X / _currentViewSize.Width - 1) / projectionMatrix.M11;
                pickingVector.Y = -(2.0f * pixelLocation.Y / _currentViewSize.Height - 1) / projectionMatrix.M22;
                pickingVector.Z = 1f;

                var worldMatrix = Matrix4x4.Identity;
                var viewWorld = _camera.View * worldMatrix;
                Matrix4x4.Invert(viewWorld, out var inversionViewWorld);

                var rayDirection = Vector3.Normalize(new Vector3(
                    pickingVector.X * inversionViewWorld.M11 + pickingVector.Y * inversionViewWorld.M21 + pickingVector.Z * inversionViewWorld.M31,
                    pickingVector.X * inversionViewWorld.M12 + pickingVector.Y * inversionViewWorld.M22 + pickingVector.Z * inversionViewWorld.M32,
                    pickingVector.X * inversionViewWorld.M13 + pickingVector.Y * inversionViewWorld.M23 + pickingVector.Z * inversionViewWorld.M33));
                var rayStart = new Vector3(
                    inversionViewWorld.M41,
                    inversionViewWorld.M42,
                    inversionViewWorld.M43);

                // Pick objects async in update thread
                await _currentScene.PerformBesideRenderingAsync(() =>
                {
                    if (_currentScene != null && this.ViewInformation != null &&
                        _currentScene.IsViewRegistered(this.ViewInformation))
                    {
                        result = _currentScene.Pick(rayStart, rayDirection, this.ViewInformation, pickingOptions);
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
        /// Changes the Configuration object.
        /// </summary>
        /// <param name="newViewConfiguration">The new configuration object to be applied.</param>
        public Task ExchangeViewConfigurationAsync(GraphicsViewConfiguration newViewConfiguration)
        {
            newViewConfiguration.EnsureNotNull(nameof(newViewConfiguration));

            var result = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            _afterPresentActions.Enqueue(() =>
            {
                var deviceConfig = _configuration.DeviceConfiguration;

                _configuration = newViewConfiguration;
                _configuration.DeviceConfiguration = deviceConfig;
                _configuration.ViewNeedsRefresh = true;

                result.TrySetResult(null);
            });

            return result.Task;
        }

        /// <summary>
        /// Resets the render counter to zero.
        /// </summary>
        public void ResetRenderCounter()
        {
            _totalRenderCount = 0;
        }

        /// <summary>
        /// Deregisters this view from the render loop.
        /// </summary>
        public void DeregisterRenderLoop()
        {
            if (!GraphicsCore.IsLoaded) { return; }

            // Deregister this Renderloop object
            if (this.IsRegisteredOnMainLoop)
            {
                GraphicsCore.Current.MainLoop.DeregisterRenderLoop(this);

                if (_renderLoopHost is IInputEnabledView inputInterface)
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
            if (!this.IsRegisteredOnMainLoop)
            {
                GraphicsCore.Current.MainLoop.RegisterRenderLoop(this);

                if (_renderLoopHost is IInputEnabledView inputInterface)
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

            SeeingSharpUtil.SafeDispose(ref _debugDrawingLayer);

            // Assign new dummy camera
            //  (this call detaches the previous camera from this renderloop)
            this.Camera = new PerspectiveCamera3D();
        }

        /// <summary>
        /// Forces a refresh of the current view.
        /// </summary>
        internal void ForceViewRefresh()
        {
            _viewRefreshForced = true;
        }

        /// <summary>
        /// Unload all loaded ViewResources.
        /// This call ensures that it will be executed in the correct UI thread.
        /// </summary>
        internal async Task UnloadViewResourcesAsync()
        {
            await this.UiSynchronizationContext.PostAsync(this.UnloadViewResources)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Unloads all view resources (is also called from EngineMainLoop in a synchronized execution block).
        /// </summary>
        internal void UnloadViewResources()
        {
            _lastRenderSuccessfully = false;

            if (_currentDevice == null) { return; }

            // Dispose resources of parent object first
            _renderLoopHost.OnRenderLoop_DisposeViewResources(_currentDevice);

            // Free direct2d render target (if created)
            SeeingSharpUtil.SafeDispose(ref _d2dOverlay);

            // Set values to initial
            _renderTarget = null;
            _renderTargetView = null;
            _renderTargetDepth = null;
            _renderTargetDepthView = null;
            _viewport = new Viewport();
            _currentViewSize = new Size(SeeingSharpConstants.MIN_VIEW_WIDTH, SeeingSharpConstants.MIN_VIEW_HEIGHT);

            // Dispose local resources
            SeeingSharpUtil.SafeDispose(ref _copyHelperTextureStaging);
            SeeingSharpUtil.SafeDispose(ref _copyHelperTextureStandard);
        }

        /// <summary>
        /// Prepares rendering (refreshes view resources, post last rendered image to the view, ...)
        /// </summary>
        internal async Task<IReadOnlyList<Action>> PrepareRenderAsync(UpdateState updateState)
        {
            _cachedPrepareRenderContinuationActions.Clear();
            if (this.DiscardRendering) { return _cachedPrepareRenderContinuationActions; }

            var writeVideoFrames = _lastRenderSuccessfully;

            // Call present from ThreadPool (if configured)
            if (!_callPresentInUiThread &&
                _currentDevice != null &&
                _loadDeviceIndex == _currentDevice.LoadDeviceIndex)
            {
                this.PresentFrameInternal();
                _lastRenderSuccessfully = false;
            }

            // Trigger prepare rendering on GUI thread
            var guiRefreshTask = this.UiSynchronizationContext.PostAsync(_cachedPrepareRenderOnGui);

            // Update overlays (use backing array because of possible multi threading issues)
            var drawingLayers2D = _2dDrawingLayers.BackingArray;
            var drawingLayers2DLength = _2dDrawingLayers.Count;
            for(var actDrawingLayer2DIndex = 0; (actDrawingLayer2DIndex < drawingLayers2DLength) && (actDrawingLayer2DIndex < drawingLayers2D.Length); actDrawingLayer2DIndex++)
            {
                var actOverlay = drawingLayers2D[actDrawingLayer2DIndex];
                if(actOverlay == null){ continue; }

                try
                {
                    actOverlay.UpdateInternal(updateState);
                }
                catch (Exception ex)
                {
                    GraphicsCore.PublishInternalExceptionInfo(ex, InternalExceptionLocation.RenderLoop_PrepareRendering_Update2DOverlay);
                }
            }

            // Update debug overlay (copy reference because of possible multi threading issues)
            var debugDrawingLayer = _debugDrawingLayer;
            if (debugDrawingLayer != null)
            {
                try
                {
                    debugDrawingLayer.UpdateInternal(updateState);
                }
                catch (Exception ex)
                {
                    GraphicsCore.PublishInternalExceptionInfo(ex, InternalExceptionLocation.RenderLoop_PrepareRendering_Update2DOverlay);
                }
            }

            // Wait for gui refresh
            await guiRefreshTask.ConfigureAwait(false);

            // Draw all frames to registered VideoWriters
            if (writeVideoFrames)
            {
                this.DrawVideoFrames();
            }

            return _cachedPrepareRenderContinuationActions;
        }

        /// <summary>
        /// This method call draws the current frame into all registered video writers.
        /// </summary>
        internal void DrawVideoFrames()
        {
            // Write current frame if we have an associated video writer
            if (_videoWriters.Count > 0)
            {
                using (var texUploader = TextureUploader.ConstructUsingPropertiesFromTexture(_currentDevice, _renderTarget))
                using (var mappedTexture = new MemoryMappedTexture<int>(_currentViewSize))
                {
                    // Upload texture
                    texUploader.UploadToMemoryMappedTexture(_renderTarget, mappedTexture);

                    // Render the texture to all video writers
                    Parallel.For(0, _videoWriters.Count, actIndex =>
                    {
                        var actVideoWriter = _videoWriters[actIndex];

                        // Start video rendering if not done so before
                        if (!actVideoWriter.HasStarted)
                        {
                            actVideoWriter.StartRendering(_currentViewSize);
                        }

                        // Render current frame
                        if (actVideoWriter.HasStarted)
                        {
                            actVideoWriter.DrawFrame(_currentDevice, mappedTexture);
                        }
                    });
                }

                // Check for occurred errors (Cancel writing if there was any)
                for (var loop = 0; loop < _videoWriters.Count; loop++)
                {
                    var actWriter = _videoWriters[loop];

                    if (actWriter.LastDrawException != null ||
                        actWriter.LastStartException != null ||
                        actWriter.LastFinishException != null)
                    {
                        // Try to finish rendering first
                        actWriter.FinishRendering();

                        // Remote the VideoWriter from this RenderLoop
                        _videoWriters.RemoveAt(loop);
                        actWriter.AssociatedRenderLoop = null;
                        loop--;
                    }
                }
            }
        }

        /// <summary>
        /// Renders this instance.
        /// </summary>
        internal void Render()
        {
            if (this.DiscardRendering) { return; }
            if (!_nextRenderAllowed) { return; }
            if (_currentDevice.IsLost) { return; }
            _nextRenderAllowed = false;

            var renderTimeMeasurement = GraphicsCore.Current.BeginMeasureActivityDuration(_cachedPerfRenderLoopRender);
            try
            {
                // Handle all resources within the scene
                if (_currentScene != null &&
                    _camera != null &&
                    _currentScene.IsViewRegistered(this.ViewInformation))
                {
                    // Renders current scene on this view
                    _currentScene.HandleRenderResources(_renderState);
                }

                // Cancel here if a device lost occurred
                if (_currentDevice.IsLost)
                {
                    return;
                }

                // Update render state
                _renderState.Reset(
                    new RenderTargets(_renderTargetView, _renderTargetDepthView),
                    _viewport, _camera, this.ViewInformation);
                _renderState.ApplyMaterial(null);

                // Handle render pass dumps
                _dumpRequests.TryDequeue(out var actRenderPassDumpTaskSource);
                RenderPassDump actRenderPassDump = null;
                if (actRenderPassDumpTaskSource != null)
                {
                    actRenderPassDump = new RenderPassDump(_currentDevice, _currentViewSize, true);
                    _renderState.StartDump(actRenderPassDump);
                }

                try
                {
                    // Paint using Direct3D
                    _currentDevice.DeviceImmediateContextD3D11.ClearRenderTargetView(_renderTargetView,
                        MathConverter.RawFromColor4(this.ClearColor));
                    _currentDevice.DeviceImmediateContextD3D11.ClearDepthStencilView(_renderTargetDepthView,
                        D3D11.DepthStencilClearFlags.Depth | D3D11.DepthStencilClearFlags.Stencil, 1f, 0);

                    // Render currently configured scene
                    if (_currentScene != null &&
                        _camera != null &&
                        _currentScene.IsViewRegistered(this.ViewInformation))
                    {
                        // Renders current scene on this view
                        _currentScene.Render(_renderState);
                    }

                    // Clear current state after rendering
                    _renderState.ClearState();

                    if (_totalRenderCount < int.MaxValue) { _totalRenderCount++; }

                    // Render 2D overlay if possible (may be not available on some older OS or older graphics cards)
                    if (_d2dOverlay != null &&
                        _d2dOverlay.IsLoaded)
                    {
                        var d2dOverlayTime =
                            GraphicsCore.Current.PerformanceAnalyzer.BeginMeasureActivityDuration(
                                _cachedPerfRenderLoopRender2D);
                        _d2dOverlay.BeginDraw();
                        try
                        {
                            _renderState.RenderTarget2D = _d2dOverlay.RenderTarget2D;
                            _renderState.Graphics2D = _d2dOverlay.Graphics;

                            // Render scene contents
                            _currentScene?.Render2DOverlay(_renderState);

                            // Perform rendering of custom 2D drawing layers
                            for (var loop = 0; loop < _2dDrawingLayers.Count; loop++)
                            {
                                try
                                {
                                    _2dDrawingLayers[loop].Draw2DInternal(_d2dOverlay.Graphics);
                                }
                                catch (Exception ex)
                                {
                                    // Publish exception info
                                    GraphicsCore.PublishInternalExceptionInfo(ex,
                                        InternalExceptionLocation.Rendering2DDrawingLayer);
                                }
                            }

                            // Draw debug layer if created
                            _debugDrawingLayer?.Draw2DInternal(_d2dOverlay.Graphics);
                        }
                        finally
                        {
                            _renderState.RenderTarget2D = null;
                            _renderState.Graphics2D = null;

                            d2dOverlayTime.Dispose();

                            try
                            {
                                _d2dOverlay.EndDraw();
                            }
                            catch (SharpGenException dxException)
                            {
                                if (dxException.ResultCode == D2D.ResultCode.RecreateTarget)
                                {
                                    // Mark the device as lost
                                    _currentDevice.IsLost = true;
                                }
                                else
                                {
                                    throw;
                                }
                            }
                        }
                    }

                    // Send all draw calls to the device and wait until finished
                    _currentDevice.DeviceImmediateContextD3D11.Flush();

                    // Update flag indicating that last render was successful
                    _lastRenderSuccessfully = true;
                }
                finally
                {
                    if (_renderState.IsWritingRenderPassDump)
                    {
                        _renderState.StopDump();
                        actRenderPassDumpTaskSource?.TrySetResult(actRenderPassDump);
                    }

                    this.CountDrawCalls = _renderState.CountDrawCallsInternal;
                    this.CountVisibleObjects = _renderState.CountVisibleObjectsInternal;
                }
            }
            finally
            {
                renderTimeMeasurement.Dispose();
            }
        }

        /// <summary>
        /// Prepares rendering (the part that runs on GUI thread)
        /// </summary>
        private void PrepareRenderOnGui()
        {
            if (!this.IsRegisteredOnMainLoop)
            {
                return;
            }

            // Call present from UI thread (if configured)
            if (_callPresentInUiThread)
            {
                if (_currentDevice != null &&
                    _loadDeviceIndex == _currentDevice.LoadDeviceIndex)
                {
                    this.PresentFrameInternal();
                }
                _lastRenderSuccessfully = false;
            }

            if (this.DiscardRendering)
            {
                return;
            }
            if (!this.IsRegisteredOnMainLoop)
            {
                return;
            }

            // Update view frustum
            this.ViewInformation.UpdateFrustum(_camera.ViewProjection);

            // Allow next rendering by default
            _nextRenderAllowed = true;

            // Need to update view parameters?              (=> Later: Render Thread)
            var viewSizeChanged = _targetSize != _currentViewSize;
            if (_renderTargetView == null ||
                viewSizeChanged ||
                _targetDevice != null && _targetDevice != _currentDevice ||
                _configuration.ViewNeedsRefresh ||
                _loadDeviceIndex != _currentDevice?.LoadDeviceIndex ||
                _viewRefreshForced || _reregisterViewOnSceneForced)
            {
                _viewRefreshForced = false;

                var refreshViewContinuationActions = new List<Action>(2);
                try
                {
                    // Trigger deregister on scene if needed
                    var reregisterOnScene = _targetDevice != null &&
                                            _targetDevice != _currentDevice &&
                                            _currentScene != null ||
                                            _reregisterViewOnSceneForced;
                    if (reregisterOnScene)
                    {
                        var localScene = _currentScene;
                        refreshViewContinuationActions.Add(() => localScene?.DeregisterView(this.ViewInformation));
                    }

                    // Unload view resources first
                    this.UnloadViewResources();

                    // Update device ob object (size is updated in RefreshViewResources)
                    if (_targetDevice != null)
                    {
                        _currentDevice = _targetDevice;
                        _targetDevice = null;

                        this.DeviceChanged.Raise(this, EventArgs.Empty);
                    }

                    // Load view resources again
                    try
                    {
                        this.RefreshViewResources();
                    }
                    catch (SharpGenException dxException)
                    {
                        if (dxException.ResultCode == ResultCode.DeviceRemoved ||
                            dxException.ResultCode == ResultCode.DeviceReset)
                        {
                            // Mark the device as lost
                            _currentDevice.IsLost = true;
                            _viewRefreshForced = true;
                            _nextRenderAllowed = false;
                            _reregisterViewOnSceneForced = true;
                            return;
                        }
                        throw;
                    }

                    // Trigger reregister on scene if needed
                    if (reregisterOnScene)
                    {
                        _reregisterViewOnSceneForced = false;

                        var localScene = _currentScene;
                        refreshViewContinuationActions.Add(() => localScene.RegisterView(this.ViewInformation));
                    }

                    if (viewSizeChanged)
                    {
                        this.CurrentViewSizeChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
                catch (Exception ex)
                {
                    _nextRenderAllowed = false;

                    throw new SeeingSharpGraphicsException("Unable to refresh view on device " + _currentDevice + "!",
                        ex);
                }

                if (_nextRenderAllowed)
                {
                    _cachedPerfRenderLoopRender2D = string.Format(
                        SeeingSharpConstants.PERF_RENDERLOOP_RENDER_2D,
                        _currentDevice.DeviceIndex.ToString(), (this.ViewInformation.ViewIndex + 1).ToString());
                    _cachedPerfRenderLoopRender = string.Format(
                        SeeingSharpConstants.PERF_RENDERLOOP_RENDER,
                        _currentDevice.DeviceIndex.ToString(), (this.ViewInformation.ViewIndex + 1).ToString());
                    _cachedPerfRenderLoopPresent = string.Format(
                        SeeingSharpConstants.PERF_RENDERLOOP_PRESENT,
                        _currentDevice.DeviceIndex.ToString(), (this.ViewInformation.ViewIndex + 1).ToString());
                }

                _cachedPrepareRenderContinuationActions.AddRange(refreshViewContinuationActions);
            }

            // Check needed resources
            if (_currentDevice == null ||
                _renderTargetView == null ||
                _renderTargetDepthView == null)
            {
                _nextRenderAllowed = false;
                return;
            }

            // Handle changed scene
            if (_targetScene != null &&
                _currentScene != _targetScene)
            {
                try
                {
                    // Trigger deregister on the current scene
                    if (_currentScene != null)
                    {
                        var localScene = _currentScene;
                        _cachedPrepareRenderContinuationActions.Add(() =>
                            localScene.DeregisterView(this.ViewInformation));

                        foreach (var actComponent in this.SceneComponents)
                        {
                            localScene.DetachComponent(actComponent, this.ViewInformation);
                        }
                    }

                    // Change scene property
                    _currentScene = _targetScene;
                    _targetScene = null;

                    // Trigger reregister on the new scene
                    if (_currentScene != null)
                    {
                        var localScene = _currentScene;
                        _cachedPrepareRenderContinuationActions.Add(
                            () => localScene.RegisterView(this.ViewInformation));

                        for (var loop = 0; loop < this.SceneComponents.Count; loop++)
                        {
                            localScene.AttachComponent(this.SceneComponents[loop], this.ViewInformation);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _nextRenderAllowed = false;

                    throw new SeeingSharpGraphicsException("Unable to change the scene!", ex);
                }
            }
            // Ensure that this loop is registered on the current view
            else if (_currentScene != null &&
                     !_currentScene.IsViewRegistered(this.ViewInformation))
            {
                var localScene = _currentScene;
                _cachedPrepareRenderContinuationActions.Add(() => localScene.RegisterView(this.ViewInformation));
            }

            // Check needed resources
            if (_currentScene == null)
            {
                _nextRenderAllowed = false;
                return;
            }

            try
            {
                // Check here whether we can render or not
                if (!_renderLoopHost.OnRenderLoop_CheckCanRender(_currentDevice))
                {
                    _nextRenderAllowed = false;
                    return;
                }

                // Perform some preparation for rendering
                _renderLoopHost.OnRenderLoop_PrepareRendering(_currentDevice);
            }
            catch (Exception ex)
            {
                _nextRenderAllowed = false;
                throw new SeeingSharpGraphicsException("Unable to prepare rendering", ex);
            }

            // Let UI manipulate current filter list
            if (_objectFilters.ObjectFilterCollectionChanged)
            {
                _objectFilters.ObjectFilterCollectionChanged = false;
                try
                {
                    this.FiltersInternal.Clear();
                    _objectFilters.CopyTo(this.FiltersInternal);
                }
                catch (Exception ex)
                {
                    // Publish exception info
                    GraphicsCore.PublishInternalExceptionInfo(ex,
                        InternalExceptionLocation.RenderLoop_ManipulateFilterList);
                }
            }

            // Raise prepare render event
            try
            {
                this.PrepareRender.Raise(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // Publish exception info
                GraphicsCore.PublishInternalExceptionInfo(ex, InternalExceptionLocation.RenderLoop_PrepareRendering);
            }
        }

        /// <summary>
        /// Refreshes the view resources.
        /// </summary>
        private void RefreshViewResources()
        {
            if (_currentDevice == null) { return; }

            // Unload current view resources first
            this.UnloadViewResources();

            // Return here if the current device is marked as lost
            if (_currentDevice.IsLost) { return; }

            // Initialize view configuration on the first load
            if (!_loadCalled)
            {
                GraphicsCore.Current.InitializeViewConfiguration(this, this.Configuration);
                _loadCalled = true;
            }

            // Recreate view resources
            var generatedViewResources = _renderLoopHost.OnRenderLoop_CreateViewResources(_currentDevice);
            if (generatedViewResources == null) { return; }
            _renderTarget = generatedViewResources.Item1;
            _renderTargetView = generatedViewResources.Item2;
            _renderTargetDepth = generatedViewResources.Item3;
            _renderTargetDepthView = generatedViewResources.Item4;
            _viewport = generatedViewResources.Item5;
            _currentViewSize = generatedViewResources.Item6;
            _targetSize = _currentViewSize;
            _currentDpiScaling = generatedViewResources.Item7;

            _loadDeviceIndex = _currentDevice.LoadDeviceIndex;

            // Update view size on camera
            _camera?.SetScreenSize(_currentViewSize.Width, _currentViewSize.Height);

            _configuration.ViewNeedsRefresh = false;

            // Try to create a Direct2D overlay
            _d2dOverlay = new Direct2DOverlayRenderer(
                _currentDevice,
                _renderTarget,
                _currentViewSize.Width, _currentViewSize.Height,
                _currentDpiScaling);

            // Create or update current renderstate
            if (_renderState == null ||
                _renderState.Device != _currentDevice)
            {
                _renderState = new RenderState(
                    _currentDevice,
                    new RenderTargets(_renderTargetView, _renderTargetDepthView),
                    _viewport, _camera, this.ViewInformation);
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
            if (_lastRenderSuccessfully)
            {
                try
                {
                    // Presents all contents on the screen
                    using (GraphicsCore.Current.BeginMeasureActivityDuration(_cachedPerfRenderLoopPresent))
                    {
                        _renderLoopHost.OnRenderLoop_Present(_currentDevice);
                    }

                    // Execute all deferred actions to be called after present
                    var prevCount = _afterPresentActions.Count;
                    var currentIndex = 0;
                    while (currentIndex < prevCount &&
                           _afterPresentActions.TryDequeue(out var actAction))
                    {
                        actAction();

                        currentIndex++;
                    }

                    // Finish rendering now
                    _renderLoopHost.OnRenderLoop_AfterRendering(_currentDevice);
                }
                catch (SharpGenException dxException)
                {
                    if (dxException.ResultCode == ResultCode.DeviceRemoved ||
                        dxException.ResultCode == ResultCode.DeviceReset)
                    {
                        // Mark the device as lost
                        _currentDevice.IsLost = true;
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    throw new SeeingSharpGraphicsException("Unable to present a view with device " + _currentDevice + "!", ex);
                }
            }
        }

        /// <summary>
        /// Cleaning for currently registered components with equal group names.
        /// </summary>
        private void CleanSceneComponentList()
        {
            if (_currentScene != null)
            {
                throw new SeeingSharpException("Internal error: CleanSceneComponentList must only be called when current scene = null!");
            }

            var componentGroups = new List<string>(this.SceneComponents.Count);
            var sceneComponentCount = this.SceneComponents.Count;
            for (var loop = sceneComponentCount - 1; loop >= 0; loop--)
            {
                var actComponent = this.SceneComponents[loop];
                if (!string.IsNullOrEmpty(actComponent.ComponentGroup))
                {
                    if (componentGroups.Contains(actComponent.ComponentGroup))
                    {
                        this.SceneComponents.RemoveAt(loop);
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
            var actScene = _currentScene;

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
                                componentsToRemove.AddRange(this.SceneComponents.Where(
                                        actInnerComponent =>
                                            actInnerComponent.ComponentGroup == actComponent.ComponentGroup &&
                                            actInnerComponent != actComponent));
                            }

                            // Attach this new component
                            actScene?.AttachComponent(actComponent, this.ViewInformation);
                        }
                    }
                    if (e.OldItems != null)
                    {
                        if (actScene != null)
                        {
                            foreach (SceneComponentBase actComponent in e.OldItems)
                            {
                                actScene.DetachComponent(actComponent, this.ViewInformation);
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    break;
            }

            // RemoveObject components which where replaced by new ones
            if (componentsToRemove.Count > 0)
            {
                foreach (var actComponentToRemove in componentsToRemove)
                {
                    this.SceneComponents.Remove(actComponentToRemove);
                }
            }
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Internal members of the <see cref="RenderLoop"/> class.
        /// Be careful when working with them.
        /// </summary>
        public class RenderLoopInternals
        {
            private RenderLoop _target;

            public D3D11.ID3D11Texture2D RenderTarget => _target._renderTarget;

            public D3D11.ID3D11Texture2D RenderTargetDepth => _target._renderTargetDepth;

            public D3D11.ID3D11Texture2D CopyHelperTextureStaging
            {
                get => _target._copyHelperTextureStaging;
                set => _target._copyHelperTextureStaging = value;
            }

            public D3D11.ID3D11Texture2D CopyHelperTextureStandard
            {
                get => _target._copyHelperTextureStandard;
                set => _target._copyHelperTextureStandard = value;
            }

            public bool CallPresentInUIThread
            {
                get => _target._callPresentInUiThread;
                set => _target._callPresentInUiThread = value;
            }

            public EngineDevice TargetDevice => _target._targetDevice;

            public RenderLoopInternals(RenderLoop target)
            {
                _target = target;
            }

            public void UnloadViewResources()
            {
                _target.UnloadViewResources();
            }
        }
    }
}