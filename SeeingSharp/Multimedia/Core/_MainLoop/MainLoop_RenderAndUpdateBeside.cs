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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeeingSharp.Multimedia.Core
{
    internal class MainLoop_RenderAndUpdateBeside
    {
        // Common Dependencies
        private GraphicsCore m_core;
        private EngineMainLoop m_mainLoop;

        // Dependencies for one pass
        private IReadOnlyList<RenderLoop> m_registeredRenderLoops;
        private IReadOnlyList<Scene> m_scenesToRender;
        private IReadOnlyList<EngineDevice> m_devicesInUse;
        private UpdateState m_updateState;

        // Cached values and collections
        private Action<int> m_actionTriggerRenderOrUpdateBesideTask;
        private ConcurrentQueue<RenderLoop> m_invalidRenderLoops;
        private List<string> m_perfSceneUpdateBesideActivityNames;
        private List<string> m_perfDeviceRenderActivityNames;

        public MainLoop_RenderAndUpdateBeside(GraphicsCore core, EngineMainLoop mainLoop)
        {
            m_core = core;
            m_mainLoop = mainLoop;

            m_invalidRenderLoops = new ConcurrentQueue<RenderLoop>();
            m_perfSceneUpdateBesideActivityNames = new List<string>(16);
            m_perfDeviceRenderActivityNames = new List<string>(16);

            m_actionTriggerRenderOrUpdateBesideTask = this.TriggerRenderOrUpdateBesideTask;
        }

        public void SetPassParameters(
            IReadOnlyList<RenderLoop> registeredRenderLoops, IReadOnlyList<Scene> scenesToRender,
            IReadOnlyList<EngineDevice> devicesInUse, UpdateState updateState)
        {
            m_registeredRenderLoops = registeredRenderLoops;
            m_scenesToRender = scenesToRender;
            m_devicesInUse = devicesInUse;
            m_updateState = updateState;

            // Clear render loop queue
            while (m_invalidRenderLoops.TryDequeue(out _)) { }

            // Prepare cached activity names for measuring scene updates
            while (m_scenesToRender.Count > m_perfSceneUpdateBesideActivityNames.Count)
            {
                m_perfSceneUpdateBesideActivityNames.Add(
                    string.Format(SeeingSharpConstants.PERF_GLOBAL_UPDATE_BESIDE, m_perfSceneUpdateBesideActivityNames.Count));
            }
            for (var loop = 0; loop < m_devicesInUse.Count; loop++)
            {
                var actDevice = m_devicesInUse[loop];
                while (m_perfDeviceRenderActivityNames.Count <= actDevice.DeviceIndex)
                {
                    m_perfDeviceRenderActivityNames.Add(string.Empty);
                }

                if (m_perfDeviceRenderActivityNames[actDevice.DeviceIndex] == string.Empty)
                {
                    m_perfDeviceRenderActivityNames[actDevice.DeviceIndex] =
                        string.Format(SeeingSharpConstants.PERF_GLOBAL_RENDER_DEVICE, actDevice.AdapterDescription);
                }
            }
        }

        public void ExecutePass()
        {
            using (m_core.BeginMeasureActivityDuration(SeeingSharpConstants.PERF_GLOBAL_RENDER_AND_UPDATE_BESIDE))
            {
                // Trigger all tasks for 'Update' pass
                Parallel.For(0, m_devicesInUse.Count + m_scenesToRender.Count, m_actionTriggerRenderOrUpdateBesideTask);

                // Handle all invalid render loops
                while (m_invalidRenderLoops.TryDequeue(out var actRenderLoop))
                {
                    m_mainLoop.DeregisterRenderLoop(actRenderLoop);
                }

                // Reset camera changed flags
                for (var loop = 0; loop < m_registeredRenderLoops.Count; loop++)
                {
                    m_registeredRenderLoops[loop].Camera.StateChanged = false;
                }
            }
        }

        private void TriggerRenderOrUpdateBesideTask(int actTaskIndex)
        {   
            if (actTaskIndex < m_devicesInUse.Count)
            {
                // Render all targets for the current device
                var actDevice = m_devicesInUse[actTaskIndex];
                using (m_core.BeginMeasureActivityDuration(m_perfDeviceRenderActivityNames[actDevice.DeviceIndex]))
                {
                    for (var loop = 0; loop < m_registeredRenderLoops.Count; loop++)
                    {
                        var actRenderLoop = m_registeredRenderLoops[loop];

                        try
                        {
                            if (actRenderLoop.Device == actDevice)
                            {
                                actRenderLoop.Render();
                            }
                        }
                        catch (Exception ex)
                        {
                            // Mark this renderloop as invalid
                            m_invalidRenderLoops.Enqueue(actRenderLoop);

                            // Publish exception info
                            GraphicsCore.PublishInternalExceptionInfo(ex, InternalExceptionLocation.EngineMainLoop_Render);
                        }
                    }
                }
            }
            else
            {
                // Perform updates beside rendering for the current scene
                var sceneIndex = actTaskIndex - m_devicesInUse.Count;
                using (m_core.BeginMeasureActivityDuration(m_perfSceneUpdateBesideActivityNames[sceneIndex]))
                {
                    var actScene = m_scenesToRender[sceneIndex];
                    var actUpdateState = actScene.CachedUpdateState;

                    actUpdateState.OnStartSceneUpdate(actScene, m_updateState, null);
                    actScene.UpdateBesideRender(actUpdateState);
                }
            }
        }
    }
}
