using System;
using SharpDX;

namespace SeeingSharp.Util
{
    internal class DummyComObject : IUnknown, ICallbackable, IDisposable
    {
        private int _referenceCounter;

        /// <inheritdoc />
        public IDisposable Shadow { get; set; }

        /// <inheritdoc />
        public virtual void Dispose()
        {

        }

        /// <inheritdoc />
        public Result QueryInterface(ref Guid guid, out IntPtr comObject)
        {
            comObject = IntPtr.Zero;
            return Result.NoInterface;
        }

        /// <inheritdoc />
        public int AddReference()
        {
            _referenceCounter++;
            return _referenceCounter;
        }

        /// <inheritdoc />
        public int Release()
        {
            _referenceCounter--;
            return _referenceCounter;
        }
    }
}
