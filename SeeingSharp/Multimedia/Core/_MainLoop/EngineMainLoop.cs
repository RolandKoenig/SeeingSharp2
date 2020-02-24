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
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Input;
using SeeingSharp.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SeeingSharp.Multimedia.Core
{
    /// <summary>
    /// The core main loop for coordinating render and update processes over all
    /// registered RenderLoops.
    /// </summary>
    public class EngineMainLoop
    {
        // Common
        private GraphicsCore m_host;

        // Main thread synchronization
        private Task m_suspendWaiter;
        private TaskCompletionSource<object> m_suspendWaiterSource;
        private TaskCompletionSource<object> m_suspendCallWaiterSource;
        private Task m_runningTask;
        private ConcurrentQueue<Action> m_globalLoopAwaitors;

        // RenderLoop collections
        private List<RenderLoop> m_registeredRenderLoops;
        private List<RenderLoop> m_unregisteredRenderLoops;
        private object m_registeredRenderLoopsLock;

        // Scene collections
        private List<Scene> m_scenesForUnload;
        private object m_scenesForUnloadLock;

        // Members regarding 2D resources
        private ConcurrentQueue<Drawing2DResourceBase> m_drawing2DResourcesToUnload;

        // Cached objects
        private GenericInputEventArgs m_cachedInputEventArgs;

        // Logic blocks
        private MainLoop_UpdateAndPrepareRendering m_logicUpdateAndPrepareRendering;
        private MainLoop_RenderAndUpdateBeside m_logicRenderAndUpdateBeside;

        /// <summary>
        /// Occurs each pass within the MainLoop and holds information about generic
        /// input states (like Gamepad).
        /// Be careful with subscribing/unsubscribing because this event is raised
        /// by ThreadPoolThreads.
        /// See http://www.codeproject.com/Articles/37474/Threadsafe-Events
        /// </summary>
        public event EventHandler<GenericInputEventArgs> GenericInput;

        /// <summary>
        /// Prevents a default instance of the <see cref="EngineMainLoop"/> class from being created.
        /// </summary>
        internal EngineMainLoop(GraphicsCore graphicsCore)
        {
            m_host = graphicsCore;

            m_globalLoopAwaitors = new ConcurrentQueue<Action>();
            m_registeredRenderLoops = new List<RenderLoop>();
            m_unregisteredRenderLoops = new List<RenderLoop>();
            m_registeredRenderLoopsLock = new object();

            m_drawing2DResourcesToUnload = new ConcurrentQueue<Drawing2DResourceBase>();

            m_scenesForUnload = new List<Scene>();
            m_scenesForUnloadLock = new object();

            m_cachedInputEventArgs = new GenericInputEventArgs();

            m_logicUpdateAndPrepareRendering = new MainLoop_UpdateAndPrepareRendering(graphicsCore, this);
            m_logicRenderAndUpdateBeside = new MainLoop_RenderAndUpdateBeside(m_host, this);
        }

        /// <summary>
        /// Suspends rendering completely.
        /// </summary>
        public Task SuspendAsync()
        {
            if (m_suspendWaiter == null)
            {
                m_suspendCallWaiterSource = new TaskCompletionSource<object>();
                m_suspendWaiterSource = new TaskCompletionSource<object>();
                m_suspendWaiter = m_suspendWaiterSource.Task;
            }

            return m_suspendCallWaiterSource.Task;
        }

        /// <summary>
        /// Resumes rendering when in suspend state.
        /// </summary>
        public void Resume()
        {
            if (m_suspendWaiter != null)
            {
                if (!m_suspendCallWaiterSource.Task.IsCompleted) { m_suspendCallWaiterSource.TrySetResult(null); }

                m_suspendWaiterSource.TrySetResult(null);
                m_suspendWaiterSource = null;
                m_suspendWaiter = null;
            }
        }

        /// <summary>
        /// Waits for next passed loop cycle.
        /// </summary>
        public Task WaitForNextPassedLoopAsync()
        {
            var result = new TaskCompletionSource<object>();
            m_globalLoopAwaitors.Enqueue(() =>
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
            lock (m_registeredRenderLoopsLock)
            {
                return new List<RenderLoop>(m_registeredRenderLoops);
            }
        }

        /// <summary>
        /// Registers the given resource for unloading.
        /// </summary>
        /// <param name="drawing2DResourceBase">The resource to be unloaded.</param>
        internal void RegisterForUnload(Drawing2DResourceBase drawing2DResourceBase)
        {
            m_drawing2DResourcesToUnload.Enqueue(drawing2DResourceBase);
        }

        /// <summary>
        /// Registers the given RenderLoop object.
        /// </summary>
        /// <param name="renderLoop">The RenderLoop to be registered.</param>
        internal void RegisterRenderLoop(RenderLoop renderLoop)
        {
            lock (m_registeredRenderLoopsLock)
            {
                if (!m_registeredRenderLoops.Contains(renderLoop))
                {
                    m_registeredRenderLoops.Add(renderLoop);
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
            lock (m_registeredRenderLoopsLock)
            {
                m_registeredRenderLoops.Remove(renderLoop);
                renderLoop.IsRegisteredOnMainLoop = false;

                if (!m_unregisteredRenderLoops.Contains(renderLoop))
                {
                    m_unregisteredRenderLoops.Add(renderLoop);
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
            if (m_runningTask != null) { throw new SeeingSharpGraphicsException("Unable to start engine's main loop: Loop has already started!"); }

            m_runningTask = Task.Factory.StartNew(async () =>
            {
                var renderStopWatch = new Stopwatch();
                renderStopWatch.Start();

                var renderingRenderLoops = new List<RenderLoop>(16);
                var scenesToRender = new List<Scene>(16);
                var camerasToUpdate = new List<Camera3DBase>(16);
                var devicesInUse = new List<EngineDevice>(16);
                var inputFrames = new List<InputFrame>(16);
                var updateState = new UpdateState(TimeSpan.Zero);

                while (!cancelToken.IsCancellationRequested)
                {
                    var exceptionOccurred = false;

                    try
                    {
                        using (m_host.BeginMeasureActivityDuration(SeeingSharpConstants.PERF_GLOBAL_PER_FRAME))
                        {
                            // Wait some time before doing anything..
                            var lastRenderMilliseconds = renderStopWatch.GetTrueElapsedMilliseconds();
                            var delayTime = SeeingSharpConstants.MINIMUM_FRAME_TIME_MS - lastRenderMilliseconds;
                            if (delayTime < SeeingSharpConstants.MINIMUM_DELAY_TIME_MS)
                            {
                                delayTime = SeeingSharpConstants.MINIMUM_DELAY_TIME_MS;
                            }

                            using (m_host.BeginMeasureActivityDuration(SeeingSharpConstants.PERF_GLOBAL_WAIT_TIME))
                            {
                                SeeingSharpUtil.MaximumDelay(delayTime);
                            }

                            // Get all render loops
                            renderingRenderLoops.Clear();
                            lock (m_registeredRenderLoopsLock)
                            {
                                renderingRenderLoops.AddRange(m_registeredRenderLoops);
                            }

                            // Queries for devices / scenes in use
                            QueryForScenesAndCameras(renderingRenderLoops, scenesToRender, camerasToUpdate);
                            QueryForDevicesInUse(renderingRenderLoops, devicesInUse);

                            // Handle device lost events
                            foreach (var actDevice in devicesInUse)
                            {
                                if (actDevice.IsLost)
                                {
                                    actDevice.RecreateAfterDeviceLost();
                                }
                            }

                            // Cleanup device resources
                            foreach (var actDevice in devicesInUse)
                            {
                                actDevice.CleanupDeviceResourceCollection();
                            }

                            // Build new UpdateState object
                            var updateTime = renderStopWatch.Elapsed;
                            if (updateTime.TotalMilliseconds > 100.0)
                            {
                                updateTime = TimeSpan.FromMilliseconds(100.0);
                            }

                            updateState.Reset(updateTime);

                            // Restart the stopwatch
                            renderStopWatch.Restart();

                            // Get all input frames
                            m_host.InputGatherer.QueryForCurrentFrames(inputFrames);

                            // First global pass: Update scene and prepare rendering
                            m_logicUpdateAndPrepareRendering.SetPassParameters(renderingRenderLoops, scenesToRender, devicesInUse, inputFrames, updateState);
                            await m_logicUpdateAndPrepareRendering.ExecutePassAsync()
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
                            m_logicRenderAndUpdateBeside.SetPassParameters(renderingRenderLoops, scenesToRender, devicesInUse, updateState);
                            m_logicRenderAndUpdateBeside.ExecutePass();

                            // Raise generic input event (if registered)
                            m_cachedInputEventArgs.NotifyNewPass(inputFrames);
                            this.GenericInput?.Raise(this, m_cachedInputEventArgs);

                            // Clear unreferenced Scenes finally
                            lock (m_scenesForUnloadLock)
                            {
                                foreach (var actScene in m_scenesForUnload)
                                {
                                    actScene.UnloadResources();
                                    actScene.Clear(true);
                                }

                                m_scenesForUnload.Clear();
                            }

                            // Unload all Direct2D resources which are not needed anymore
                            while (m_drawing2DResourcesToUnload.TryDequeue(out var act2DResourceToUnload))
                            {
                                var deviceCount = m_host.DeviceCount;
                                for (var loop = 0; loop < deviceCount; loop++)
                                {
                                    act2DResourceToUnload.UnloadResources(m_host.Devices[loop]);
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
                    while (m_globalLoopAwaitors.Count > 0)
                    {
                        if (m_globalLoopAwaitors.TryDequeue(out var currentAction))
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
                    var suspendWaiter = m_suspendWaiter;

                    if (suspendWaiter != null)
                    {
                        m_suspendCallWaiterSource.TrySetResult(null);

                        // Wait for resuming
                        await suspendWaiter;
                    }

                    // Trigger calculation of performance values
                    m_host.PerformanceAnalyzer.CalculateResults();
                }
            });

            return m_runningTask;
        }

        /// <summary>
        /// Registers the given scene for unload.
        /// </summary>
        /// <param name="scene">The scene to be registered.</param>
        internal void RegisterSceneForUnload(Scene scene)
        {
            lock (m_scenesForUnloadLock)
            {
                if (!m_scenesForUnload.Contains(scene))
                {
                    m_scenesForUnload.Add(scene);
                }
            }
        }

        internal void DeregisterSceneForUnload(Scene scene)
        {
            lock (m_scenesForUnloadLock)
            {
                while (m_scenesForUnload.Remove(scene)) { }
            }
        }

        /// <summary>
        /// Updates current RenderLoop registrations.
        /// </summary>
        /// <param name="renderingRenderLoops">The list of currently working RenderLoops.</param>
        internal async Task UpdateRenderLoopRegistrationsAsync(ICollection<RenderLoop> renderingRenderLoops)
        {
            // Unload all deregistered RenderLoops
            if (m_unregisteredRenderLoops.Count > 0)
            {
                // Handle global and local RenderLoop collections
                List<RenderLoop> copiedUnregisteredRenderLoops = null;

                lock (m_registeredRenderLoopsLock)
                {
                    copiedUnregisteredRenderLoops = new List<RenderLoop>(m_unregisteredRenderLoops);
                    m_unregisteredRenderLoops.Clear();

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

        /// <summary>
        /// Is the MainLoop running?
        /// </summary>
        public bool IsRunning => m_runningTask != null;

        /// <summary>
        /// Gets the total count of registered RenderLoop objects.
        /// </summary>
        public int RegisteredRenderLoopCount => m_registeredRenderLoops.Count;
    }
}
