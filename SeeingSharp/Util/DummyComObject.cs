using System;
using SharpDX;

namespace SeeingSharp.Util
{
    public class DummyComObject : IUnknown, ICallbackable, IDisposable
    {
        private int m_referenceCounter;

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
            m_referenceCounter++;
            return m_referenceCounter;
        }

        /// <inheritdoc />
        public int Release()
        {
            m_referenceCounter--;
            return m_referenceCounter;
        }

        /// <inheritdoc />
        public IDisposable Shadow { get; set; }
    }
}
