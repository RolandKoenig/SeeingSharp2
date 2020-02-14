using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Util;

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
        private ThreadSaveQueue<RenderLoop> m_invalidRenderLoops;
        private List<string> m_perfSceneUpdateBesideActivityNames;
        private List<string> m_perfDeviceRenderActivityNames;

        public MainLoop_RenderAndUpdateBeside(GraphicsCore core, EngineMainLoop mainLoop)
        {
            m_core = core;
            m_mainLoop = mainLoop;

            m_invalidRenderLoops = new ThreadSaveQueue<RenderLoop>();
            m_perfSceneUpdateBesideActivityNames = new List<string>(16);
            m_perfDeviceRenderActivityNames = new List<string>(16);

            m_actionTriggerRenderOrUpdateBesideTask = new Action<int>(this.TriggerRenderOrUpdateBesideTask);
        }

        public void SetPassParameters(
            IReadOnlyList<RenderLoop> registeredRenderLoops, IReadOnlyList<Scene> scenesToRender,
            IReadOnlyList<EngineDevice> devicesInUse, UpdateState updateState)
        {
            m_registeredRenderLoops = registeredRenderLoops;
            m_scenesToRender = scenesToRender;
            m_devicesInUse = devicesInUse;
            m_updateState = updateState;

            m_invalidRenderLoops.Clear();

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

                if (m_perfSceneUpdateBesideActivityNames[actDevice.DeviceIndex] == string.Empty)
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
                if (m_invalidRenderLoops.HasAny())
                {
                    while (m_invalidRenderLoops.Dequeue(out var actRenderLoop))
                    {
                        m_mainLoop.DeregisterRenderLoop(actRenderLoop);
                    }
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
                        catch (Exception)
                        {
                            // Mark this renderloop as invalid
                            m_invalidRenderLoops.Enqueue(actRenderLoop);
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
