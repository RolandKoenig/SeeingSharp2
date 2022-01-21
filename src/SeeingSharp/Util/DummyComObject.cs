using System;
using SharpGen.Runtime;

namespace SeeingSharp.Util
{
    internal class DummyComObject : IUnknown, ICallbackable, IDisposable
    {
        private uint _referenceCounter;

        /// <inheritdoc />
        public ShadowContainer? Shadow { get; private set; }

        public DummyComObject()
        {
            this.Shadow = new ShadowContainer(this);
        }

        public void Dispose()
        {
            this.Shadow?.Dispose();
            this.Shadow = null;
        }

        public Result QueryInterface(ref Guid guid, out IntPtr comObject)
        {
            comObject = IntPtr.Zero;
            return Result.NoInterface;
        }

        /// <inheritdoc />
        public uint AddRef()
        {
            _referenceCounter++;
            return _referenceCounter;
        }

        /// <inheritdoc />
        public uint Release()
        {
            _referenceCounter--;
            return _referenceCounter;
        }
    }
}
