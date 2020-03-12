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
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SeeingSharp.Multimedia.Input
{
    /// <summary>
    /// This class is responsible for input gathering.
    /// </summary>
    public class InputGathererThread : ObjectThread
    {
        // Constants
        private static readonly TimeSpan s_singleFrameDuration = TimeSpan.FromMilliseconds(1000.0 / SeeingSharpConstants.INPUT_FRAMES_PER_SECOND);

        // Synchronization
        private ConcurrentQueue<Action> _commandQueue;
        private ConcurrentQueue<InputFrame> _recoveredInputFrames;
        private ConcurrentQueue<InputFrame> _gatheredInputFrames;
        private InputFrame _lastInputFrame;
        private List<InputStateBase> _cachedStates;

        // Thread local state
        private List<IInputHandler> _globalInputHandlers;
        private Dictionary<IInputEnabledView, List<IInputHandler>> _viewInputHandlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputGathererThread"/> class.
        /// </summary>
        internal InputGathererThread()
            : base("Input Gatherer", 1000 / SeeingSharpConstants.INPUT_FRAMES_PER_SECOND)
        {
            _commandQueue = new ConcurrentQueue<Action>();
            _gatheredInputFrames = new ConcurrentQueue<InputFrame>();
            _recoveredInputFrames = new ConcurrentQueue<InputFrame>();
            _viewInputHandlers = new Dictionary<IInputEnabledView, List<IInputHandler>>();
            _cachedStates = new List<InputStateBase>(8);
        }

        protected override void OnTick(EventArgs eArgs)
        {
            base.OnTick(eArgs);

            if (!GraphicsCore.IsLoaded)
            {
                return;
            }

            // Query for all input handlers on first tick
            if (_globalInputHandlers == null)
            {
                _globalInputHandlers = InputHandlerFactory.CreateInputHandlersForGlobal();

                foreach (var actInputHandler in _globalInputHandlers)
                {
                    actInputHandler.Start(null);
                }
            }

            // Execute all commands within the command queue
            if (_commandQueue.Count > 0)
            {
                var prevCount = _commandQueue.Count;
                var actIndex = 0;
                while ((actIndex < prevCount) &&
                       (_commandQueue.TryDequeue(out var actCommand)))
                {
                    actCommand();
                    actIndex++;
                }
            }

            // Gather all input data
            var expectedStateCount = _lastInputFrame?.CountStates ?? 6;

            // Create new InputFrame object or reuse an old one
            if (_recoveredInputFrames.TryDequeue(out var newInputFrame))
            {
                newInputFrame.Reset(expectedStateCount, s_singleFrameDuration);
            }
            else
            {
                newInputFrame = new InputFrame(expectedStateCount, s_singleFrameDuration);
            }

            // Gather all input states (without dependency to a view)
            foreach (var actInputHandler in _globalInputHandlers)
            {
                actInputHandler.GetInputStates(_cachedStates);
                for (var loop = 0; loop < _cachedStates.Count; loop++)
                {
                    var actInputState = _cachedStates[loop];
                    if(actInputState == null){ continue; }

                    newInputFrame.AddCopyOfState(actInputState, null);
                }
                _cachedStates.Clear();
            }

            // Gather all input states (with view dependency)
            foreach (var actViewSpecificHandlers in _viewInputHandlers)
            {
                var renderLoop = actViewSpecificHandlers.Key.RenderLoop;
                if (renderLoop == null)
                {
                    continue;
                }

                foreach (var actInputHandler in actViewSpecificHandlers.Value)
                {
                    actInputHandler.GetInputStates(_cachedStates);
                    for (var loop = 0; loop < _cachedStates.Count; loop++)
                    {
                        var actInputState = _cachedStates[loop];
                        if(actInputState == null){ continue; }

                        newInputFrame.AddCopyOfState(actInputState, renderLoop.ViewInformation);
                    }
                    _cachedStates.Clear();
                }
            }

            // Store the generated InputFrame
            _lastInputFrame = newInputFrame;
            _gatheredInputFrames.Enqueue(newInputFrame);

            // Ensure that we hold input frames for a maximum time range of a second
            //  (older input is obsolete)
            while (_gatheredInputFrames.Count > SeeingSharpConstants.INPUT_FRAMES_PER_SECOND)
            {
                _gatheredInputFrames.TryDequeue(out _);
            }
        }

        /// <summary>
        /// Gets all gathered InputFrames.
        /// </summary>
        internal void QueryForCurrentFrames(List<InputFrame> targetList)
        {
            // Do first recover all old frames
            for (var loop = 0; loop < targetList.Count; loop++)
            {
                _recoveredInputFrames.Enqueue(targetList[loop]);
            }
            targetList.Clear();

            // Enqueue new frames
            while (_gatheredInputFrames.TryDequeue(out var actInputFame))
            {
                targetList.Add(actInputFame);
            }
        }

        /// <summary>
        /// Registers the given view on this thread.
        /// </summary>
        internal void RegisterView(IInputEnabledView view)
        {
            view.EnsureNotNull(nameof(view));

            _commandQueue.Enqueue(() =>
            {
                var inputHandlers = InputHandlerFactory.CreateInputHandlersForView(view);
                if (inputHandlers == null) { return; }
                if (inputHandlers.Count == 0) { return; }

                // Deregister old input handlers if necessary
                if (_viewInputHandlers.ContainsKey(view))
                {
                    var oldList = _viewInputHandlers[view];

                    foreach (var actOldInputHandler in oldList)
                    {
                        actOldInputHandler.Stop();
                    }
                    _viewInputHandlers.Remove(view);
                }

                // Register new ones
                _viewInputHandlers[view] = inputHandlers;

                foreach (var actInputHandler in inputHandlers)
                {
                    actInputHandler.Start(view);
                }
            });
        }

        /// <summary>
        /// Deregisters the given view from this thread.
        /// </summary>
        internal void DeregisterView(IInputEnabledView view)
        {
            view.EnsureNotNull(nameof(view));

            _commandQueue.Enqueue(() =>
            {
                if (_viewInputHandlers.ContainsKey(view))
                {
                    var oldList = _viewInputHandlers[view];
                    foreach (var actOldInputHandler in oldList)
                    {
                        actOldInputHandler.Stop();
                    }

                    _viewInputHandlers.Remove(view);
                }
            });
        }
    }
}