using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeeingSharp.Core
{
    internal class MainLoop_UpdateAndPrepareRendering
    {
        // Common Dependencies
        private GraphicsCore _core;
        private EngineMainLoop _mainLoop;

        // Common caching
        private List<string> _perfSceneUpdateActivityNames;

        // Dependencies for each render loop
        private List<RenderLoop> _renderingRenderLoops;
        private IReadOnlyList<Scene> _scenesToRender;
        private IReadOnlyList<EngineDevice> _devicesInUse;
        private UpdateState _updateState;

        // Caches values and collections
        private ConcurrentQueue<Exception> _exceptionsDuringUpdate;
        private List<Action> _additionalContinuationActions;
        private object _additionalContinuationActionsLock;
        private List<Task<List<Action>>> _prepareRenderTasks;
        private Action<int> _actionUpdateSingleScene;

        public MainLoop_UpdateAndPrepareRendering(GraphicsCore core, EngineMainLoop mainLoop)
        {
            _core = core;
            _mainLoop = mainLoop;
            _perfSceneUpdateActivityNames = new List<string>(16);
            _exceptionsDuringUpdate = new ConcurrentQueue<Exception>();

            _additionalContinuationActions = new List<Action>(6);
            _additionalContinuationActionsLock = new object();

            _prepareRenderTasks = new List<Task<List<Action>>>(16);

            _actionUpdateSingleScene = this.UpdateSingleScene;
        }

        public void SetPassParameters(
            List<RenderLoop> renderingRenderLoops, IReadOnlyList<Scene> scenesToRender,
            IReadOnlyList<EngineDevice> devicesInUse, UpdateState updateState)
        {
            // Update all local members
            _renderingRenderLoops = renderingRenderLoops;
            _scenesToRender = scenesToRender;
            _devicesInUse = devicesInUse;
            _updateState = updateState; 

            // Clear temporary needed arrays
            while (_exceptionsDuringUpdate.TryDequeue(out _)) { }
            _additionalContinuationActions.Clear();
            _prepareRenderTasks.Clear();

            // Prepare cached activity names for measuring scene updates
            while (_scenesToRender.Count > _perfSceneUpdateActivityNames.Count)
            {
                _perfSceneUpdateActivityNames.Add(
                    string.Format(SeeingSharpConstants.PERF_GLOBAL_UPDATE_SCENE, _perfSceneUpdateActivityNames.Count));
            }
        }

        public async Task ExecutePassAsync()
        {
            using (_core.BeginMeasureActivityDuration(SeeingSharpConstants.PERF_GLOBAL_UPDATE_AND_PREPARE))
            {
                // Trigger all tasks for preparing views
                for (var actDeviceIndex = 0; actDeviceIndex < _devicesInUse.Count; actDeviceIndex++)
                {
                    _prepareRenderTasks.Add(this.PrepareRenderForDeviceAsync(
                        _renderingRenderLoops, _devicesInUse[actDeviceIndex],
                        _additionalContinuationActions, _additionalContinuationActionsLock));
                }

                // Update all scenes
                if(_scenesToRender.Count == 1){ this.UpdateSingleScene(0); }
                else
                {
                    Parallel.For(0, _scenesToRender.Count, _actionUpdateSingleScene);
                }
  
                // Await synchronizations with the view(s)
                if (_prepareRenderTasks.Count > 0)
                {
                    await Task.WhenAll(_prepareRenderTasks.ToArray());
                }

                // Handle initial configuration of render loops (=> No current device or changing device)
                var prepareRenderingOnChangedDeviceTask = this.PrepareRenderForDeviceAsync(
                    _renderingRenderLoops, null,
                    _additionalContinuationActions, _additionalContinuationActionsLock);
                await prepareRenderingOnChangedDeviceTask;
                _prepareRenderTasks.Add(prepareRenderingOnChangedDeviceTask);

                // Throw exceptions if any occurred during scene update
                //  => This would be a fatal exception, so throw up to main loop
                if (_exceptionsDuringUpdate.Count > 0)
                {
                    throw new AggregateException("Error(s) during Scene update!", _exceptionsDuringUpdate.ToArray());
                }

                // Trigger all continuation actions returned by the previously executed prepare tasks
                foreach (var actPrepareTasks in _prepareRenderTasks)
                {
                    if (actPrepareTasks.Result != null)
                    {
                        foreach (var actContinuationAction in actPrepareTasks.Result)
                        {
                            actContinuationAction();
                        }
                    }
                }

                foreach (var actAction in _additionalContinuationActions)
                {
                    actAction();
                }

                // Unload all deregistered RenderLoops
                await _mainLoop.UpdateRenderLoopRegistrationsAsync(_renderingRenderLoops);
            }
        }

        private async Task<List<Action>> PrepareRenderForDeviceAsync(
            List<RenderLoop> renderingRenderLoops,
            EngineDevice deviceToUpdate,
            List<Action> additionalContinuationActions,
            object additionalContinuationActionsLock)
        {
            List<Action> result = null;
            for (var loop = 0; loop < renderingRenderLoops.Count; loop++)
            {
                var actRenderLoop = renderingRenderLoops[loop];
                var actDevice = actRenderLoop.Device;
                var actTargetDevice = actRenderLoop.Internals.TargetDevice;

                var isChangingDevice = actTargetDevice != null &&
                                       actDevice != actTargetDevice;
                var isSameDevice = actDevice != null &&
                                   deviceToUpdate == actDevice;

                // ReSharper disable once ReplaceWithSingleAssignment.False
                var doCallPrepareRender = false;
                
                // ReSharper disable once ConvertIfToOrExpression
                if (deviceToUpdate == null && isChangingDevice)
                {
                    doCallPrepareRender = true;
                }
                if (deviceToUpdate != null && isSameDevice)
                {
                    doCallPrepareRender = true;
                }

                if(!doCallPrepareRender){ continue; }
                try
                {
                    var currentResult = await actRenderLoop.PrepareRenderAsync(_updateState);
                    if(currentResult == null || currentResult.Count == 0)
                    {
                        // Nothing to do in this case - no continuation actions
                    }
                    else if (result == null)
                    {
                        result = new List<Action>(currentResult);
                    }
                    else
                    {
                        result.AddRange(currentResult);
                    }
                }
                catch (Exception ex)
                {
                    // Deregister this RenderLoop
                    var actRenderLoopInner = actRenderLoop;
                    var renderingRenderLoopsInner = renderingRenderLoops;
                    void DeregisterRenderLoopAction()
                    {
                        _mainLoop.DeregisterRenderLoop(actRenderLoopInner);
                        renderingRenderLoopsInner.Remove(actRenderLoopInner);
                    }

                    // Append this as a continuation action
                    lock (additionalContinuationActionsLock)
                    {
                        additionalContinuationActions.Add(DeregisterRenderLoopAction);
                    }

                    // Publish exception info
                    GraphicsCore.PublishInternalExceptionInfo(ex, InternalExceptionLocation.EngineMainLoop_PrepareRendering);
                }
            }
            return result;
        }

        private void UpdateSingleScene(int sceneIndex)
        {
            try
            {
                using (_core.BeginMeasureActivityDuration(_perfSceneUpdateActivityNames[sceneIndex]))
                {
                    var actScene = _scenesToRender[sceneIndex];
                    var actUpdateState = actScene.CachedUpdateState;

                    actUpdateState.OnStartSceneUpdate(actScene, _updateState);

                    actScene.Update(actUpdateState);
                }
            }
            catch (Exception ex)
            {
                _exceptionsDuringUpdate.Enqueue(ex);
            }
        }
    }
}
