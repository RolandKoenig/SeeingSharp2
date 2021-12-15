using System;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Util;
using D2D = Vortice.Direct2D1;
using static Vortice.Direct2D1.D2D1;

namespace SeeingSharp.Core.Devices
{
    public class FactoryHandlerD2D : IDisposable, ICheckDisposed
    {
        private D2D.ID2D1Factory2 _factory;

        /// <summary>
        /// Is Direct2D initialized?
        /// </summary>
        public bool IsDisposed => _factory == null;

        internal D2D.ID2D1Factory2 Factory
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
            var factoryOptions = new D2D.FactoryOptions();
            factoryOptions.DebugLevel = coreConfiguration.DebugEnabled ? D2D.DebugLevel.Information : D2D.DebugLevel.None;

            _factory = D2D1CreateFactory<D2D.ID2D1Factory2>(
                D2D.FactoryType.SingleThreaded,
                factoryOptions);
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