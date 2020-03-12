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
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SeeingSharp.Multimedia.Input
{
    public class InputHandlerFactory
    {
        private List<IInputHandler> _inputHandlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputHandlerFactory"/> class.
        /// </summary>
        internal InputHandlerFactory(SeeingSharpLoader loader)
        {
            _inputHandlers = new List<IInputHandler>(4);
            foreach (var actExtension in loader.Extensions)
            {
                var actInputHandlers = actExtension.CreateInputHandlers();
                if(actInputHandlers == null){ continue; }

                foreach (var actInputHandler in actInputHandlers)
                {
                    _inputHandlers.Add(actInputHandler);
                }
            }
        }

        /// <summary>
        /// Gets all possible GraphicsInputHandlers for the given view and camera types.
        /// </summary>
        /// <typeparam name="TViewType">Gets the type of the view.</typeparam>
        /// <param name="viewObject">The view for which to the input handlers.</param>
        public List<IInputHandler> GetInputHandler<TViewType>(IInputEnabledView viewObject)
            where TViewType : class
        {
            var givenViewType = typeof(TViewType);

            return this.GetInputHandler(givenViewType);
        }

        /// <summary>
        /// Gets all possible GraphicsInputHandlers for the given view and camera types.
        /// Pass null to all parameters to return all generic input handlers.
        /// </summary>
        /// <param name="givenViewType">The type of the camera.</param>
        public List<IInputHandler> GetInputHandler(Type givenViewType)
        {
            var result = new List<IInputHandler>();

            foreach (var actInputHandler in _inputHandlers)
            {
                // Query for the input handler's information
                var actSupportedViewTypes = actInputHandler.GetSupportedViewTypes();
                var viewTypeSupported = false;

                // Check for view-type support
                if (actSupportedViewTypes == null)
                {
                    viewTypeSupported = givenViewType == null;
                }
                else if (givenViewType != null)
                {
                    foreach (var actViewType in actSupportedViewTypes)
                    {
                        if (actViewType.GetTypeInfo().IsAssignableFrom(givenViewType.GetTypeInfo()))
                        {
                            viewTypeSupported = true;
                            break;
                        }
                    }
                }

                if (!viewTypeSupported)
                {
                    continue;
                }

                // Create a new input handler
                result.Add(Activator.CreateInstance(actInputHandler.GetType()) as IInputHandler);
            }
            return result;
        }

        /// <summary>
        /// Creates all input handlers which are not associated to one view.
        /// </summary>
        internal static List<IInputHandler> CreateInputHandlersForGlobal()
        {
            return GraphicsCore.Current.InputHandlers.GetInputHandler(null);
        }

        /// <summary>
        /// Creates all input handlers which are associated to one view.
        /// </summary>
        internal static List<IInputHandler> CreateInputHandlersForView(IInputEnabledView viewObject)
        {
            viewObject.EnsureNotNull(nameof(viewObject));

            return GraphicsCore.Current.InputHandlers.GetInputHandler(viewObject.GetType());
        }

        /// <summary>
        /// Updates all currently active input handlers for the given view.
        /// </summary>
        /// <param name="viewObject">The object of the view control.</param>
        /// <param name="inputHandlers">The collection of input handlers managed by the view object.</param>
        /// <param name="renderLoop">The renderloop used by the view object.</param>
        /// <param name="currentlyDisposing">Is the view currently disposing?</param>
        internal static void UpdateInputHandlerList(
            IInputEnabledView viewObject,
            List<IInputHandler> inputHandlers,
            RenderLoop renderLoop,
            bool currentlyDisposing)
        {
            viewObject.EnsureNotNull(nameof(viewObject));
            inputHandlers.EnsureNotNull(nameof(inputHandlers));
            renderLoop.EnsureNotNull(nameof(renderLoop));

            // Clear previous input handlers
            if (inputHandlers.Count > 0)
            {
                foreach (var actHandler in inputHandlers)
                {
                    actHandler.Stop();
                }
                inputHandlers.Clear();
            }

            // Check whether this object is disposed
            if (currentlyDisposing) { return; }

            // Check for other dependencies
            if (renderLoop?.Camera == null)
            {
                return;
            }

            // Get all possible input handlers
            inputHandlers.AddRange(GraphicsCore.Current.InputHandlers.GetInputHandler(
                viewObject.GetType()));

            // Start them all
            foreach (var actInputHandler in inputHandlers)
            {
                actInputHandler.Start(viewObject);
            }
        }

        /// <summary>
        /// Gets the total count of loaded input handlers
        /// </summary>
        public int Count => _inputHandlers.Count;
    }
}