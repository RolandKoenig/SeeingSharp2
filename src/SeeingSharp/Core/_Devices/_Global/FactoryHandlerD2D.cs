using System;
using SeeingSharp.Util;
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Core
{
    public class FactoryHandlerD2D : IDisposable, ICheckDisposed
    {
        private D2D.Factory2 _factory;

        /// <summary>
        /// Is Direct2D initialized?
        /// </summary>
        public bool IsDisposed => _factory == null;

        internal D2D.Factory2 Factory
        {
            get
            {
                if(_factory == null){ throw new ObjectDisposedException(nameof(FactoryHandlerD2D)); }
                return _factory;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryHandlerD2D"/> class.
        /// </summary>
        internal FactoryHandlerD2D(GraphicsCoreConfiguration coreConfiguration)
        {
            _factory = new D2D.Factory2(
                D2D.FactoryType.SingleThreaded,
                coreConfiguration.DebugEnabled ? D2D.DebugLevel.Information : D2D.DebugLevel.None);
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref _factory);
        }
    }
}