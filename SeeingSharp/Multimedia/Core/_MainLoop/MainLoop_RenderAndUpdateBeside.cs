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
        private GraphicsCore _core;
        private EngineMainLoop _mainLoop;

        // Dependencies for one pass
        private IReadOnlyList<RenderLoop> _registeredRenderLoops;
        private IReadOnlyList<Scene> _scenesToRender;
        private IReadOnlyList<EngineDevice> _devicesInUse;
        private UpdateState _updateState;

        // Cached values and collections
        private Action<int> _actionTriggerRenderOrUpdateBesideTask;
        private ConcurrentQueue<RenderLoop> _invalidRenderLoops;
        private List<string> _perfSceneUpdateBesideActivityNames;
        private List<string> _perfDeviceRenderActivityNames;

        public MainLoop_RenderAndUpdateBeside(GraphicsCore core, EngineMainLoop mainLoop)
        {
            _core = core;
            _mainLoop = mainLoop;

            _invalidRenderLoops = new ConcurrentQueue<RenderLoop>();
            _perfSceneUpdateBesideActivityNames = new List<string>(16);
            _perfDeviceRenderActivityNames = new List<string>(16);

            _actionTriggerRenderOrUpdateBesideTask = this.TriggerRenderOrUpdateBesideTask;
        }

        public void SetPassParameters(
            IReadOnlyList<RenderLoop> registeredRenderLoops, IReadOnlyList<Scene> scenesToRender,
            IReadOnlyList<EngineDevice> devicesInUse, UpdateState updateState)
        {
            _registeredRenderLoops = registeredRenderLoops;
            _scenesToRender = scenesToRender;
            _devicesInUse = devicesInUse;
            _updateState = updateState;

            // Clear render loop queue
            while (_invalidRenderLoops.TryDequeue(out _)) { }

            // Prepare cached activity names for measuring scene updates
            while (_scenesToRender.Count > _perfSceneUpdateBesideActivityNames.Count)
            {
                _perfSceneUpdateBesideActivityNames.Add(
                    string.Format(SeeingSharpConstants.PERF_GLOBAL_UPDATE_BESIDE, _perfSceneUpdateBesideActivityNames.Count));
            }
            for (var loop = 0; loop < _devicesInUse.Count; loop++)
            {
                var actDevice = _devicesInUse[loop];
                while (_perfDeviceRenderActivityNames.Count <= actDevice.DeviceIndex)
                {
                    _perfDeviceRenderActivityNames.Add(string.Empty);
                }

                if (_perfDeviceRenderActivityNames[actDevice.DeviceIndex] == string.Empty)
                {
                    _perfDeviceRenderActivityNames[actDevice.DeviceIndex] =
                        string.Format(SeeingSharpConstants.PERF_GLOBAL_RENDER_DEVICE, actDevice.AdapterDescription);
                }
            }
        }

        public void ExecutePass()
        {
            using (_core.BeginMeasureActivityDuration(SeeingSharpConstants.PERF_GLOBAL_RENDER_AND_UPDATE_BESIDE))
            {
                // Trigger all tasks for 'Update' pass
                Parallel.For(0, _devicesInUse.Count + _scenesToRender.Count, _actionTriggerRenderOrUpdateBesideTask);

                // Handle all invalid render loops
                while (_invalidRenderLoops.TryDequeue(out var actRenderLoop))
                {
                    _mainLoop.DeregisterRenderLoop(actRenderLoop);
                }

                // Reset camera changed flags
                for (var loop = 0; loop < _registeredRenderLoops.Count; loop++)
                {
                    _registeredRenderLoops[loop].Camera.StateChanged = false;
                }
            }
        }

        private void TriggerRenderOrUpdateBesideTask(int actTaskIndex)
        {   
            if (actTaskIndex < _devicesInUse.Count)
            {
                // Render all targets for the current device
                var actDevice = _devicesInUse[actTaskIndex];
                using (_core.BeginMeasureActivityDuration(_perfDeviceRenderActivityNames[actDevice.DeviceIndex]))
                {
                    for (var loop = 0; loop < _registeredRenderLoops.Count; loop++)
                    {
                        var actRenderLoop = _registeredRenderLoops[loop];

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
                            _invalidRenderLoops.Enqueue(actRenderLoop);

                            // Publish exception info
                            GraphicsCore.PublishInternalExceptionInfo(ex, InternalExceptionLocation.EngineMainLoop_Render);
                        }
                    }
                }
            }
            else
            {
                // Perform updates beside rendering for the current scene
                var sceneIndex = actTaskIndex - _devicesInUse.Count;
                using (_core.BeginMeasureActivityDuration(_perfSceneUpdateBesideActivityNames[sceneIndex]))
                {
                    var actScene = _scenesToRender[sceneIndex];
                    var actUpdateState = actScene.CachedUpdateState;

                    actUpdateState.OnStartSceneUpdate(actScene, _updateState);
                    actScene.UpdateBesideRender(actUpdateState);
                }
            }
        }
    }
}
