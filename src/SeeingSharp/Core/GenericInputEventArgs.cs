using System;
using System.Collections.Generic;
using SeeingSharp.Input;

namespace SeeingSharp.Core
{
    public class GenericInputEventArgs : EventArgs
    {
        private List<InputFrame> _inputFrames;

        /// <summary>
        /// Gets a list containing all InputFrames.
        /// </summary>
        public IReadOnlyList<InputFrame> InputFrames => _inputFrames;

        internal GenericInputEventArgs()
        {
            _inputFrames = new List<InputFrame>(16);
        }

        internal void NotifyNewPass(List<InputFrame> inputFrames)
        {
            _inputFrames.Clear();
            _inputFrames.AddRange(inputFrames);
        }
    }
}