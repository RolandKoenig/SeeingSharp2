using System;

namespace SeeingSharp.Util
{
    public class MultiDisposable : IDisposable
    {
        private IDisposable[] _collection;

        public MultiDisposable(params IDisposable[] collection)
        {
            _collection = collection;
        }

        public void Dispose()
        {
            if (_collection == null) { return; }

            foreach (var actDisposable in _collection)
            {
                actDisposable.Dispose();
            }
            _collection = null;
        }
    }
}
