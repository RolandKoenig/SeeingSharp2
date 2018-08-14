#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
#endregion
using SeeingSharp.Util;
using SeeingSharp.Checking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.Multimedia.Input
{
    /// <summary>
    /// This class is responsible for input gathering.
    /// </summary>
    public class InputGathererThread : ObjectThread
    {
        #region Constants
        private static readonly TimeSpan SINGLE_FRAME_DURATION = TimeSpan.FromMilliseconds(1000.0 / Constants.INPUT_FRAMES_PER_SECOND);
        #endregion

        #region Synchronization
        private ThreadSaveQueue<Action> m_commandQueue;
        private ThreadSaveQueue<InputFrame> m_recoveredInputFrames;
        private ThreadSaveQueue<InputFrame> m_gatheredInputFrames;
        private InputFrame m_lastInputFrame;
        #endregion

        #region Thread local state
        private List<IInputHandler> m_globalInputHandlers;
        private Dictionary<IInputEnabledView, List<IInputHandler>> m_viewInputHandlers;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="InputGathererThread"/> class.
        /// </summary>
        internal InputGathererThread()
            : base("Input Gatherer", 1000 / Constants.INPUT_FRAMES_PER_SECOND)
        {
            m_commandQueue = new ThreadSaveQueue<Action>();
            m_gatheredInputFrames = new ThreadSaveQueue<InputFrame>();
            m_recoveredInputFrames = new ThreadSaveQueue<InputFrame>();

            //m_globalInputHandlers = new List<IInputHandler>();
            m_viewInputHandlers = new Dictionary<IInputEnabledView, List<IInputHandler>>();
        }

        /// <summary>
        /// Gets all gathered InputFrames.
        /// </summary>
        internal void QueryForCurrentFrames(List<InputFrame> targetList)
        {
            // Do first recover all old frames
            m_recoveredInputFrames.Enqueue(targetList);
            targetList.Clear();

            // Enqueue new frames
            m_gatheredInputFrames.DequeueAll(targetList);
        }

        /// <summary>
        /// Registers the given view on this thread.
        /// </summary>
        internal void RegisterView(IInputEnabledView view)
        {
            view.EnsureNotNull(nameof(view));

            m_commandQueue.Enqueue(() =>
            {
                List<IInputHandler> inputHandlers = InputHandlerFactory.CreateInputHandlersForView(view);
                if(inputHandlers == null) { return; }
                if(inputHandlers.Count == 0) { return; }

                // Deregister old input handlers if necessary
                if(m_viewInputHandlers.ContainsKey(view))
                {
                    List<IInputHandler> oldList = m_viewInputHandlers[view];
                    foreach(IInputHandler actOldInputHanlder in oldList)
                    {
                        actOldInputHanlder.Stop();
                    }
                    m_viewInputHandlers.Remove(view);
                }

                // Register new ones
                m_viewInputHandlers[view] = inputHandlers;
                foreach(IInputHandler actInputHandler in inputHandlers)
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
                    List<IInputHandler> oldList = m_viewInputHandlers[view];
                    foreach (IInputHandler actOldInputHanlder in oldList)
                    {
                        actOldInputHanlder.Stop();
                    }
                    m_viewInputHandlers.Remove(view);
                }
            });
        }

        protected override void OnTick(EventArgs eArgs)
        {
            base.OnTick(eArgs);

            if (!GraphicsCore.IsInitialized) { return; }

            // Query for all input handlers on first tick
            if(m_globalInputHandlers == null)
            {
                m_globalInputHandlers = InputHandlerFactory.CreateInputHandlersForGlobal();
                foreach (IInputHandler actInputHandler in m_globalInputHandlers)
                {
                    actInputHandler.Start(null);
                }
            }

            // Execute all commands within the command queue
            if(m_commandQueue.Count > 0)
            {
                Action actCommand = null;
                while(m_commandQueue.Dequeue(out actCommand))
                {
                    actCommand();
                }
            }

            // Gather all input data
            int expectedStateCount = m_lastInputFrame != null ? m_lastInputFrame.CountStates : 6;

            // Create new InputFrame object or reuse an old one
            InputFrame newInputFrame = null;
            if(m_recoveredInputFrames.Dequeue(out newInputFrame))
            {
                newInputFrame.Reset(expectedStateCount, SINGLE_FRAME_DURATION);
            }
            else
            {
                newInputFrame = new InputFrame(expectedStateCount, SINGLE_FRAME_DURATION);
            }

            // Gather all input states
            foreach(IInputHandler actInputHandler in m_globalInputHandlers)
            {
                foreach(InputStateBase actInputState in actInputHandler.GetInputStates())
                {
                    actInputState.EnsureNotNull(nameof(actInputState));

                    newInputFrame.AddCopyOfState(actInputState, null);
                }
            }
            foreach(KeyValuePair<IInputEnabledView, List<IInputHandler>> actViewSpecificHandlers in m_viewInputHandlers)
            {
                RenderLoop renderLoop = actViewSpecificHandlers.Key.RenderLoop;
                if (renderLoop == null) { continue; }

                foreach (IInputHandler actInputHandler in actViewSpecificHandlers.Value)
                {
                    foreach (InputStateBase actInputState in actInputHandler.GetInputStates())
                    {
                        actInputState.EnsureNotNull(nameof(actInputState));

                        newInputFrame.AddCopyOfState(actInputState, renderLoop.ViewInformation);
                    }
                }
            }

            // Store the generated InputFrame 
            m_lastInputFrame = newInputFrame;
            m_gatheredInputFrames.Enqueue(newInputFrame);

            // Ensure that we hold input frames for a maximum time range of a second
            //  (older input is obsolete)
            while (m_gatheredInputFrames.Count > Constants.INPUT_FRAMES_PER_SECOND)
            {
                InputFrame dummyFrame = null;
                m_gatheredInputFrames.Dequeue(out dummyFrame);
            }
        }
    }
}
