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
        private static readonly TimeSpan SINGLE_FRAME_DURATION = TimeSpan.FromMilliseconds(1000.0 / SeeingSharpConstants.INPUT_FRAMES_PER_SECOND);

        // Synchronization
        private ConcurrentQueue<Action> m_commandQueue;
        private ConcurrentQueue<InputFrame> m_recoveredInputFrames;
        private ConcurrentQueue<InputFrame> m_gatheredInputFrames;
        private InputFrame m_lastInputFrame;
        private List<InputStateBase> m_cachedStates;

        // Thread local state
        private List<IInputHandler> m_globalInputHandlers;
        private Dictionary<IInputEnabledView, List<IInputHandler>> m_viewInputHandlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputGathererThread"/> class.
        /// </summary>
        internal InputGathererThread()
            : base("Input Gatherer", 1000 / SeeingSharpConstants.INPUT_FRAMES_PER_SECOND)
        {
            m_commandQueue = new ConcurrentQueue<Action>();
            m_gatheredInputFrames = new ConcurrentQueue<InputFrame>();
            m_recoveredInputFrames = new ConcurrentQueue<InputFrame>();
            m_viewInputHandlers = new Dictionary<IInputEnabledView, List<IInputHandler>>();
            m_cachedStates = new List<InputStateBase>(8);
        }

        protected override void OnTick(EventArgs eArgs)
        {
            base.OnTick(eArgs);

            if (!GraphicsCore.IsLoaded)
            {
                return;
            }

            // Query for all input handlers on first tick
            if (m_globalInputHandlers == null)
            {
                m_globalInputHandlers = InputHandlerFactory.CreateInputHandlersForGlobal();

                foreach (var actInputHandler in m_globalInputHandlers)
                {
                    actInputHandler.Start(null);
                }
            }

            // Execute all commands within the command queue
            if (m_commandQueue.Count > 0)
            {
                while (m_commandQueue.TryDequeue(out var actCommand))
                {
                    actCommand();
                }
            }

            // Gather all input data
            var expectedStateCount = m_lastInputFrame?.CountStates ?? 6;

            // Create new InputFrame object or reuse an old one
            if (m_recoveredInputFrames.TryDequeue(out var newInputFrame))
            {
                newInputFrame.Reset(expectedStateCount, SINGLE_FRAME_DURATION);
            }
            else
            {
                newInputFrame = new InputFrame(expectedStateCount, SINGLE_FRAME_DURATION);
            }

            // Gather all input states (without dependency to a view)
            foreach (var actInputHandler in m_globalInputHandlers)
            {
                actInputHandler.GetInputStates(m_cachedStates);
                for (var loop = 0; loop < m_cachedStates.Count; loop++)
                {
                    var actInputState = m_cachedStates[loop];
                    if(actInputState == null){ continue; }

                    newInputFrame.AddCopyOfState(actInputState, null);
                }
                m_cachedStates.Clear();
            }

            // Gather all input states (with view dependency)
            foreach (var actViewSpecificHandlers in m_viewInputHandlers)
            {
                var renderLoop = actViewSpecificHandlers.Key.RenderLoop;
                if (renderLoop == null)
                {
                    continue;
                }

                foreach (var actInputHandler in actViewSpecificHandlers.Value)
                {
                    actInputHandler.GetInputStates(m_cachedStates);
                    for (var loop = 0; loop < m_cachedStates.Count; loop++)
                    {
                        var actInputState = m_cachedStates[loop];
                        if(actInputState == null){ continue; }

                        newInputFrame.AddCopyOfState(actInputState, renderLoop.ViewInformation);
                    }
                    m_cachedStates.Clear();
                }
            }

            // Store the generated InputFrame
            m_lastInputFrame = newInputFrame;
            m_gatheredInputFrames.Enqueue(newInputFrame);

            // Ensure that we hold input frames for a maximum time range of a second
            //  (older input is obsolete)
            while (m_gatheredInputFrames.Count > SeeingSharpConstants.INPUT_FRAMES_PER_SECOND)
            {
                m_gatheredInputFrames.TryDequeue(out _);
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
                m_recoveredInputFrames.Enqueue(targetList[loop]);
            }
            targetList.Clear();

            // Enqueue new frames
            while (m_gatheredInputFrames.TryDequeue(out var actInputFame))
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

            m_commandQueue.Enqueue(() =>
            {
                var inputHandlers = InputHandlerFactory.CreateInputHandlersForView(view);
                if (inputHandlers == null) { return; }
                if (inputHandlers.Count == 0) { return; }

                // Deregister old input handlers if necessary
                if (m_viewInputHandlers.ContainsKey(view))
                {
                    var oldList = m_viewInputHandlers[view];

                    foreach (var actOldInputHandler in oldList)
                    {
                        actOldInputHandler.Stop();
                    }
                    m_viewInputHandlers.Remove(view);
                }

                // Register new ones
                m_viewInputHandlers[view] = inputHandlers;

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

            m_commandQueue.Enqueue(() =>
            {
                if (m_viewInputHandlers.ContainsKey(view))
                {
                    var oldList = m_viewInputHandlers[view];
                    foreach (var actOldInputHandler in oldList)
                    {
                        actOldInputHandler.Stop();
                    }

                    m_viewInputHandlers.Remove(view);
                }
            });
        }
    }
}