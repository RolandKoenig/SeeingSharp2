using System;

namespace SeeingSharp.Util
{
    /// <summary>
    /// Dummy class that implements IDisposable.
    /// </summary>
    public class DummyDisposable : IDisposable
    {
        private Action _onDisposeAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyDisposable"/> class.
        /// </summary>
        /// <param name="onDisposeAction">The action to call on Dispose.</param>
        public DummyDisposable(Action onDisposeAction)
        {
            _onDisposeAction = onDisposeAction;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _onDisposeAction.Invoke();
        }
    }
}
