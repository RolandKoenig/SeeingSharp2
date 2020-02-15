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
using System.Threading.Tasks;
using System.Collections.Generic;
using SeeingSharp.Multimedia.Input;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Core
{
    internal class MainLoop_UpdateAndPrepareRendering
    {
        // Common Dependencies
        private GraphicsCore m_core;
        private EngineMainLoop m_mainLoop;

        // Common caching
        private List<string> m_perfSceneUpdateActivityNames;

        // Dependencies for each render loop
        private List<RenderLoop> m_renderingRenderLoops;
        private IReadOnlyList<Scene> m_scenesToRender;
        private IReadOnlyList<EngineDevice> m_devicesInUse;
        private IEnumerable<InputFrame> m_inputFrames;
        private UpdateState m_updateState;

        // Caches values and collections
        private ThreadSaveQueue<Exception> m_exceptionsDuringUpdate;
        private List<Action> m_additionalContinuationActions;
        private object m_additionalContinuationActionsLock;
        private List<Task<List<Action>>> m_prepareRenderTasks;
        private Action<int> m_actionUpdateSingleScene;

        public MainLoop_UpdateAndPrepareRendering(GraphicsCore core, EngineMainLoop mainLoop)
        {
            m_core = core;
            m_mainLoop = mainLoop;
            m_perfSceneUpdateActivityNames = new List<string>(16);
            m_exceptionsDuringUpdate = new ThreadSaveQueue<Exception>();

            m_additionalContinuationActions = new List<Action>(6);
            m_additionalContinuationActionsLock = new object();

            m_prepareRenderTasks = new List<Task<List<Action>>>(16);

            m_actionUpdateSingleScene = new Action<int>(this.UpdateSingleScene);
        }

        public void SetPassParameters(
            List<RenderLoop> renderingRenderLoops, IReadOnlyList<Scene> scenesToRender,
            IReadOnlyList<EngineDevice> devicesInUse, IEnumerable<InputFrame> inputFrames, UpdateState updateState)
        {
            // Update all local members
            m_renderingRenderLoops = renderingRenderLoops;
            m_scenesToRender = scenesToRender;
            m_devicesInUse = devicesInUse;
            m_inputFrames = inputFrames;
            m_updateState = updateState; 

            // Clear temporary needed arrays
            m_exceptionsDuringUpdate.Clear();
            m_additionalContinuationActions.Clear();
            m_prepareRenderTasks.Clear();

            // Prepare cached activity names for measuring scene updates
            while (m_scenesToRender.Count > m_perfSceneUpdateActivityNames.Count)
            {
                m_perfSceneUpdateActivityNames.Add(
                    string.Format(SeeingSharpConstants.PERF_GLOBAL_UPDATE_SCENE, m_perfSceneUpdateActivityNames.Count));
            }
        }

        public async Task ExecutePassAsync()
        {
            using (m_core.BeginMeasureActivityDuration(SeeingSharpConstants.PERF_GLOBAL_UPDATE_AND_PREPARE))
            {
                // Trigger all tasks for preparing views
                for (var actDeviceIndex = 0; actDeviceIndex < m_devicesInUse.Count; actDeviceIndex++)
                {
                    m_prepareRenderTasks.Add(this.PrepareRenderForDeviceAsync(
                        m_renderingRenderLoops, m_devicesInUse[actDeviceIndex],
                        m_additionalContinuationActions, m_additionalContinuationActionsLock));
                }

                // Update all scenes
                if(m_scenesToRender.Count == 1){ this.UpdateSingleScene(0); }
                else
                {
                    Parallel.For(0, m_scenesToRender.Count, m_actionUpdateSingleScene);
                }
  
                // Await synchronizations with the view(s)
                if (m_prepareRenderTasks.Count > 0)
                {
                    await Task.WhenAll(m_prepareRenderTasks.ToArray());
                }

                // Handle initial configuration of render loops (=> No current device or changing device)
                var prepareRenderingOnChangedDeviceTask = this.PrepareRenderForDeviceAsync(
                    m_renderingRenderLoops, null,
                    m_additionalContinuationActions, m_additionalContinuationActionsLock);
                await prepareRenderingOnChangedDeviceTask;
                m_prepareRenderTasks.Add(prepareRenderingOnChangedDeviceTask);

                // Throw exceptions if any occurred during scene update
                //  => This would be a fatal exception, so throw up to main loop
                if (m_exceptionsDuringUpdate.HasAny())
                {
                    throw new AggregateException("Error(s) during Scene update!", m_exceptionsDuringUpdate.DequeueAll().ToArray());
                }

                // Trigger all continuation actions returned by the previously executed prepare tasks
                foreach (var actPrepareTasks in m_prepareRenderTasks)
                {
                    if (actPrepareTasks.Result != null)
                    {
                        foreach (var actContinuationAction in actPrepareTasks.Result)
                        {
                            actContinuationAction();
                        }
                    }
                }

                foreach (var actAction in m_additionalContinuationActions)
                {
                    actAction();
                }

                // SetPassParameters all dummy flags before rendering
                foreach (var actRenderLoop in m_renderingRenderLoops)
                {
                    actRenderLoop.ResetFlagsBeforeRendering();
                }

                // Unload all deregistered RenderLoops
                await m_mainLoop.UpdateRenderLoopRegistrationsAsync(m_renderingRenderLoops);
            }
        }

        private async Task<List<Action>> PrepareRenderForDeviceAsync(
            List<RenderLoop> renderingRenderLoops,
            EngineDevice device,
            List<Action> additionalContinuationActions,
            object additionalContinuationActionsLock)
        {
            List<Action> result = null;
            for (var loop = 0; loop < renderingRenderLoops.Count; loop++)
            {
                var actRenderLoop = renderingRenderLoops[loop];
                if ((actRenderLoop.Device != device) || (actRenderLoop.Internals.TargetDevice != null))
                {
                    if(device != null){ continue; }
                }

                try
                {
                    var currentResult = await actRenderLoop.PrepareRenderAsync();

                    if((currentResult == null) || (currentResult.Count == 0))
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
                catch (Exception)
                {
                    // Deregister this RenderLoop
                    var actRenderLoopInner = actRenderLoop;
                    var renderingRenderLoopsInner = renderingRenderLoops;
                    void DeregisterRenderLoopAction()
                    {
                        m_mainLoop.DeregisterRenderLoop(actRenderLoopInner);
                        renderingRenderLoopsInner.Remove(actRenderLoopInner);
                    }

                    // Append this as a continuation action
                    lock (additionalContinuationActionsLock)
                    {
                        additionalContinuationActions.Add(DeregisterRenderLoopAction);
                    }
                }
            }
            return result;
        }

        private void UpdateSingleScene(int sceneIndex)
        {
            try
            {
                using (m_core.BeginMeasureActivityDuration(m_perfSceneUpdateActivityNames[sceneIndex]))
                {
                    var actScene = m_scenesToRender[sceneIndex];
                    var actUpdateState = actScene.CachedUpdateState;

                    actUpdateState.OnStartSceneUpdate(actScene, m_updateState, m_inputFrames);

                    actScene.Update(actUpdateState);
                }
            }
            catch (Exception ex)
            {
                m_exceptionsDuringUpdate.Enqueue(ex);
            }
        }
    }
}
