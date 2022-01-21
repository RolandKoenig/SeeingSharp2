using System;
using System.Reflection;
using System.Collections.Generic;
using SeeingSharp.Checking;
using SeeingSharp.Core;

namespace SeeingSharp.Input
{
    public class InputHandlerFactory
    {
        private List<IInputHandler> _inputHandlers;

        /// <summary>
        /// Gets the total count of loaded input handlers
        /// </summary>
        public int Count => _inputHandlers.Count;

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
        public List<IInputHandler> GetInputHandler(Type? givenViewType)
        {
            var result = new List<IInputHandler>();

            foreach (var actInputHandler in _inputHandlers)
            {
                // Query for the input handler's information
                var actSupportedViewTypes = actInputHandler.GetSupportedViewTypes();
                var viewTypeSupported = false;

                // Check for view-type support
                if ((actSupportedViewTypes == null) ||
                    (actSupportedViewTypes.Length == 0))
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
                result.Add((IInputHandler)Activator.CreateInstance(actInputHandler.GetType())!);
            }
            return result;
        }

        /// <summary>
        /// Creates all input handlers which are not associated to one view.
        /// </summary>
        internal static List<IInputHandler>? CreateInputHandlersForGlobal()
        {
            return GraphicsCore.Current.InputHandlers?.GetInputHandler(null);
        }

        /// <summary>
        /// Creates all input handlers which are associated to one view.
        /// </summary>
        internal static List<IInputHandler>? CreateInputHandlersForView(IInputEnabledView viewObject)
        {
            viewObject.EnsureNotNull(nameof(viewObject));

            return GraphicsCore.Current.InputHandlers?.GetInputHandler(viewObject.GetType());
        }
    }
}