using System;
using System.Collections.Generic;

namespace SeeingSharp.Multimedia.Input
{
    public interface IInputHandler
    {
        /// <summary>
        /// Gets a list containing all supported view types.
        /// Null means that this handler is not bound to a view.
        /// </summary>
        Type[] GetSupportedViewTypes();

        /// <summary>
        /// Starts input handling.
        /// </summary>
        /// <param name="viewObject">The view object (e. g. Direct3D11Canvas).</param>
        void Start(IInputEnabledView viewObject);

        /// <summary>
        /// Stops input handling.
        /// </summary>
        void Stop();

        /// <summary>
        /// Queries all current input states.
        /// </summary>
        void GetInputStates(List<InputStateBase> target);
    }
}