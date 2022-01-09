using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SeeingSharp.Core.Devices;
using SeeingSharp.Drawing2D.Resources;
using SeeingSharp.Drawing3D;
using SeeingSharp.Input;
using SeeingSharp.Util;

namespace SeeingSharp.Core
{
    /// <summary>
    /// The core main loop for coordinating render and update processes over all
    /// registered RenderLoops.
    /// </summary>
    public class EngineMainLoop
    {
        // Common
        private GraphicsCore _host;

        // Main thread synchronization
        private Task? _suspendWaiter;
        private TaskCompletionSource<object?>? _suspendWaiterSource;
        private TaskCompletionSource<object?>? _suspendCallWaiterSource;
        private Task? _runningTask;
        private ConcurrentQueue<Action> _globalLoopAwaitors;

        // RenderLoop collections
        private List<RenderLoop> _registeredRenderLoops;
        private List<RenderLoop> _unregisteredRenderLoops;
        private object _registeredRenderLoopsLock;

        // Scene collections
        private List<Scene> _scenesForUnload;
        private object _scenesForUnloadLock;

        // Members regarding 2D resources
        private ConcurrentQueue<Drawing2DResourceBase> _drawing2DResourcesToUnload;

        // Cached objects
        private GenericInputEventArgs _cachedInputEventArgs;

        // Logic blocks
        private MainLoop_UpdateAndPrepareRendering _logicUpdateAndPrepareRendering;
        private MainLoop_RenderAndUpdateBeside _logicRenderAndUpdateBeside;

        /// <summary>
        /// Is the MainLoop running?
        /// </summary>
        public bool IsRunning => _runningTask != null;

        /// <summary>
        /// Gets the total count of registered RenderLoop objects.
        /// </summary>
        public int RegisteredRenderLoopCount => _registeredRenderLoops.Count;

        /// <summary>
        /// Occurs each pass within the MainLoop and holds information about generic
        /// input states (like Gamepad).
        /// Be careful with subscribing/unsubscribing because this event is raised
        /// by ThreadPoolThreads.
        /// See http://www.codeproject.com/Articles/37474/Threadsafe-Events
        /// </summary>
        public event EventHandler<GenericInputEventArgs>? GenericInput;

        /// <summary>
        /// Prevents a default instance of the <see cref="EngineMainLoop"/> class from being created.
        /// </summary>
        internal EngineMainLoop(GraphicsCore graphicsCore)
        {
            _host = graphicsCore;

            _globalLoopAwaitors = new ConcurrentQueue<Action>();
            _registeredRenderLoops = new List<RenderLoop>();
            _unregisteredRenderLoops = new List<RenderLoop>();
            _registeredRenderLoopsLock = new object();

            _drawing2DResourcesToUnload = new ConcurrentQueue<Drawing2DResourceBase>();

            _scenesForUnload = new List<Scene>();
            _scenesForUnloadLock = new object();

            _cachedInputEventArgs = new GenericInputEventArgs();

            _logicUpdateAndPrepareRendering = new MainLoop_UpdateAndPrepareRendering(graphicsCore, this);
            _logicRenderAndUpdateBeside = new MainLoop_RenderAndUpdateBeside(_host, this);
        }

        /// <summary>
        /// Suspends rendering completely.
        /// </summary>
        public Task SuspendAsync()
        {
            if (_suspendWaiter == null)
            {
                _suspendCallWaiterSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
                _suspendWaiterSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
                _suspendWaiter = _suspendWaiterSource.Task;
            }

            return _suspendCallWaiterSource!.Task;
        }

        /// <summary>
        /// Resumes rendering when in suspend state.
        /// </summary>
        public void Resume()
        {
            if (_suspendWaiter != null)
            {
                if (!_suspendCallWaiterSource!.Task.IsCompleted) { _suspendCallWaiterSource.TrySetResult(null); }

                _suspendWaiterSource!.TrySetResult(null);
                _suspendWaiterSource = null;
                _suspendWaiter = null;
            }
        }

        /// <summary>
        /// Waits for next passed loop cycle.
        /// </summary>
        public Task WaitForNextPassedLoopAsync()
        {
            var result = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            _globalLoopAwaitors.Enqueue(() =>
            {
                result.SetResult(null);
            });
            return result.Task;
        }

        /// <summary>
        /// Gets a collection containing all registered render loops.
        /// </summary>
        public IEnumerable<RenderLoop> GetRegisteredRenderLoops()
        {
            lock (_registeredRenderLoopsLock)
            {
                return new List<RenderLoop>(_registeredRenderLoops);
            }
        }

        /// <summary>
        /// Registers the given resource for unloading.
        /// </summary>
        /// <param name="drawing2DResourceBase">The resource to be unloaded.</param>
        internal void RegisterForUnload(Drawing2DResourceBase drawing2DResourceBase)
        {
            _drawing2DResourcesToUnload.Enqueue(drawing2DResourceBase);
        }

        /// <summary>
        /// Registers the given RenderLoop object.
        /// </summary>
        /// <param name="renderLoop">The RenderLoop to be registered.</param>
        internal void RegisterRenderLoop(RenderLoop renderLoop)
        {
            lock (_registeredRenderLoopsLock)
            {
                if (!_registeredRenderLoops.Contains(renderLoop))
                {
                    _registeredRenderLoops.Add(renderLoop);
                    renderLoop.IsRegisteredOnMainLoop = true;
                }
            }
        }

        /// <summary>
        /// Deregisters the given RenderLoop object.
        /// </summary>
        /// <param name="renderLoop">The render loop to be deregistered.</param>
        internal void DeregisterRenderLoop(RenderLoop renderLoop)
        {
            lock (_registeredRenderLoopsLock)
            {
                _registeredRenderLoops.Remove(renderLoop);
                renderLoop.IsRegisteredOnMainLoop = false;

                if (!_unregisteredRenderLoops.Contains(renderLoop))
                {
                    _unregisteredRenderLoops.Add(renderLoop);
                }
            }
        }

        /// <summary>
        /// Starts the engine's main loop.
        /// </summary>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        internal Task Start(CancellationToken cancelToken)
        {
            if (_runningTask != null) { throw new SeeingSharpGraphicsException("Unable to start engine's main loop: Loop has already started!"); }

            _runningTask = Task.Factory.StartNew(async () =>
            {
                var renderStopWatch = new Stopwatch();
                renderStopWatch.Start();

                var renderingRenderLoops = new List<RenderLoop>(16);
                var scenesToRender = new List<Scene>(16);
                var camerasToUpdate = new List<Camera3DBase>(16);
                var devicesInUse = new List<EngineDevice>(16);
                var inputFrames = new List<InputFrame>(16);
                var updateState = new UpdateState(TimeSpan.Zero, null);

                while (!cancelToken.IsCancellationRequested)
                {
                    var exceptionOccurred = false;

                    try
                    {
                        using (_host.BeginMeasureActivityDuration(SeeingSharpConstants.PERF_GLOBAL_PER_FRAME))
                        {
                            // Wait some time before doing anything..
                            var lastRenderMilliseconds = renderStopWatch.GetTrueElapsedMilliseconds();
                            var delayTime = SeeingSharpConstants.MINIMUM_FRAME_TIME_MS - lastRenderMilliseconds;
                            if (delayTime < SeeingSharpConstants.MINIMUM_DELAY_TIME_MS)
                            {
                                delayTime = SeeingSharpConstants.MINIMUM_DELAY_TIME_MS;
                            }

                            using (_host.BeginMeasureActivityDuration(SeeingSharpConstants.PERF_GLOBAL_WAIT_TIME))
                            {
                                SeeingSharpUtil.MaximumDelay(delayTime);
                            }

                            // Get all render loops
                            renderingRenderLoops.Clear();
                            lock (_registeredRenderLoopsLock)
                            {
                                renderingRenderLoops.AddRange(_registeredRenderLoops);
                            }

                            // Queries for devices / scenes in use
                            QueryForScenesAndCameras(renderingRenderLoops, scenesToRender, camerasToUpdate);
                            QueryForDevicesInUse(renderingRenderLoops, devicesInUse);
                            var deviceInUseCount = devicesInUse.Count;

                            // Handle device lost events
                            for(var loop=0; loop<deviceInUseCount; loop++)
                            {
                                var actDevice = devicesInUse[loop];
                                if(!actDevice.IsLost){ continue; }

                                actDevice.RecreateAfterDeviceLost();
                            }

                            // Cleanup device resources
                            for(var loop=0; loop<deviceInUseCount; loop++)
                            {
                                var actDevice = devicesInUse[loop];
                                actDevice.CleanupDeviceResourceCollection();
                            }

                            // Get all input frames
                            _host.InputGatherer.QueryForCurrentFrames(inputFrames);

                            // Build new UpdateState object
                            var updateTime = renderStopWatch.Elapsed;
                            if (updateTime.TotalMilliseconds > 100.0)
                            {
                                updateTime = TimeSpan.FromMilliseconds(100.0);
                            }

                            updateState.Reset(updateTime, inputFrames);

                            // Restart the stopwatch
                            renderStopWatch.Restart();

                            // First global pass: Update scene and prepare rendering
                            _logicUpdateAndPrepareRendering.SetPassParameters(renderingRenderLoops, scenesToRender, devicesInUse, updateState);
                            await _logicUpdateAndPrepareRendering.ExecutePassAsync()
                                .ConfigureAwait(false);

                            // Update all cameras
                            foreach (var actCamera in camerasToUpdate)
                            {
                                actCamera.AnimationHandler.Update(updateState);
                            }

                            // Queries for devices / scenes in use (may have changed during prepare)
                            QueryForScenesAndCameras(renderingRenderLoops, scenesToRender, camerasToUpdate);
                            QueryForDevicesInUse(renderingRenderLoops, devicesInUse);

                            // Second global pass: Render scene(s) and update beside
                            _logicRenderAndUpdateBeside.SetPassParameters(renderingRenderLoops, scenesToRender, devicesInUse, updateState);
                            _logicRenderAndUpdateBeside.ExecutePass();

                            // Raise generic input event (if registered)
                            _cachedInputEventArgs.NotifyNewPass(inputFrames);
                            try
                            {
                                this.GenericInput?.Raise(this, _cachedInputEventArgs);
                            }
                            catch (Exception ex)
                            {
                                GraphicsCore.PublishInternalExceptionInfo(ex, InternalExceptionLocation.EngineMainLoop_GenericInputEvent);
                            }

                            // Clear unreferenced Scenes finally
                            lock (_scenesForUnloadLock)
                            {
                                foreach (var actScene in _scenesForUnload)
                                {
                                    actScene.UnloadResources();
                                    actScene.Clear(true);
                                }

                                _scenesForUnload.Clear();
                            }

                            // Unload all Direct2D resources which are not needed anymore
                            while (_drawing2DResourcesToUnload.TryDequeue(out var act2DResourceToUnload))
                            {
                                var deviceCount = _host.DeviceCount;
                                for (var loop = 0; loop < deviceCount; loop++)
                                {
                                    act2DResourceToUnload.UnloadResources(_host.Devices[loop]);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptionOccurred = true;

                        // Publish exception info
                        GraphicsCore.PublishInternalExceptionInfo(ex, InternalExceptionLocation.EngineMainLoop_Loop);
                    }

                    // Execute global awaitors
                    var prevCount = _globalLoopAwaitors.Count;
                    var currentIndex = 0;
                    while (currentIndex < prevCount)
                    {
                        currentIndex++;
                        if (_globalLoopAwaitors.TryDequeue(out var currentAction))
                        {
                            currentAction();
                        }
                    }

                    if (exceptionOccurred)
                    {
                        // Wait some time and try rendering again
                        await Task.Delay(1000);
                    }

                    // Handle suspend / resume
                    var suspendWaiter = _suspendWaiter;
                    if (suspendWaiter != null)
                    {
                        _suspendCallWaiterSource!.TrySetResult(null);

                        // Wait for resuming
                        await suspendWaiter;
                    }

                    // Trigger calculation of performance values
                    _host.PerformanceAnalyzer.CalculateResults();
                }
            });

            return _runningTask;
        }

        /// <summary>
        /// Registers the given scene for unload.
        /// </summary>
        /// <param name="scene">The scene to be registered.</param>
        internal void RegisterSceneForUnload(Scene scene)
        {
            lock (_scenesForUnloadLock)
            {
                if (!_scenesForUnload.Contains(scene))
                {
                    _scenesForUnload.Add(scene);
                }
            }
        }

        internal void DeregisterSceneForUnload(Scene scene)
        {
            lock (_scenesForUnloadLock)
            {
                while (_scenesForUnload.Remove(scene)) { }
            }
        }

        /// <summary>
        /// Updates current RenderLoop registrations.
        /// </summary>
        /// <param name="renderingRenderLoops">The list of currently working RenderLoops.</param>
        internal async Task UpdateRenderLoopRegistrationsAsync(ICollection<RenderLoop> renderingRenderLoops)
        {
            // Unload all deregistered RenderLoops
            if (_unregisteredRenderLoops.Count > 0)
            {
                // Handle global and local RenderLoop collections
                List<RenderLoop>? copiedUnregisteredRenderLoops = null;

                lock (_registeredRenderLoopsLock)
                {
                    copiedUnregisteredRenderLoops = new List<RenderLoop>(_unregisteredRenderLoops);
                    _unregisteredRenderLoops.Clear();

                    foreach (var actRenderLoop in copiedUnregisteredRenderLoops)
                    {
                        // RemoveObject this RenderLoop for further rendering inside this pass
                        while (renderingRenderLoops.Contains(actRenderLoop)) { renderingRenderLoops.Remove(actRenderLoop); }
                    }
                }

                // Now perform cleaning operations
                foreach (var actRenderLoop in copiedUnregisteredRenderLoops)
                {
                    try
                    {
                        // Perform unloading of UI resources
                        await actRenderLoop.UnloadViewResourcesAsync();

                        // Deregister this view from the scene
                        if (actRenderLoop.Scene != null &&
                            actRenderLoop.ViewInformation != null &&
                            actRenderLoop.Scene.IsViewRegistered(actRenderLoop.ViewInformation))
                        {
                            actRenderLoop.Scene.DeregisterView(actRenderLoop.ViewInformation);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Publish exception info
                        GraphicsCore.PublishInternalExceptionInfo(ex, InternalExceptionLocation.RenderLoop_Unload);
                    }
                }
            }
        }

        /// <summary>
        /// Queries for all scenes to be rendered for all given RenderLoop objects.
        /// </summary>
        /// <param name="registeredRenderLoops">The render loops from which to get the scenes.</param>
        /// <param name="scenesToRender">The collection to be modified.</param>
        /// <param name="camerasToUpdate">A list containing all cameras which are defined in currently bound scenes.</param>
        private static void QueryForScenesAndCameras(IReadOnlyList<RenderLoop> registeredRenderLoops, ICollection<Scene> scenesToRender, ICollection<Camera3DBase> camerasToUpdate)
        {
            scenesToRender.Clear();
            camerasToUpdate.Clear();

            for (var loop = 0; loop < registeredRenderLoops.Count; loop++)
            {
                var actScene = registeredRenderLoops[loop].Scene;
                if (actScene != null && !scenesToRender.Contains(actScene))
                {
                    scenesToRender.Add(actScene);
                }

                var actCamera = registeredRenderLoops[loop].Camera;
                if (actCamera != null && !camerasToUpdate.Contains(actCamera))
                {
                    camerasToUpdate.Add(actCamera);
                }
            }
        }

        /// <summary>
        /// Queries for all devices in use by given RenderLoop objects.
        /// </summary>
        /// <param name="registeredRenderLoops">The render loops from which to get the devices.</param>
        /// <param name="devicesInUse">The collection to be modified.</param>
        private static void QueryForDevicesInUse(IReadOnlyList<RenderLoop> registeredRenderLoops, ICollection<EngineDevice> devicesInUse)
        {
            devicesInUse.Clear();

            for (var loop = 0; loop < registeredRenderLoops.Count; loop++)
            {
                var actDevice = registeredRenderLoops[loop].Device;

                if (actDevice != null && !devicesInUse.Contains(actDevice))
                {
                    devicesInUse.Add(actDevice);
                }
            }
        }
    }
}
