using System;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Util;
using DWrite = SharpDX.DirectWrite;

namespace SeeingSharp.Core
{
    public class FactoryHandlerDWrite : IDisposable, ICheckDisposed
    {
        private DWrite.Factory _factory;

        /// <summary>
        /// Is DirectWrite initialized successfully?
        /// </summary>
        public bool IsDisposed => _factory == null;

        /// <summary>
        /// Gets the Factory object.
        /// </summary>
        internal DWrite.Factory Factory
        {
            get
            {
                if(_factory == null){ throw new ObjectDisposedException(nameof(FactoryHandlerDWrite)); }
                return _factory;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryHandlerDWrite"/> class.
        /// </summary>
        internal FactoryHandlerDWrite(GraphicsCoreConfiguration coreConfiguration)
        {
            // Create DirectWrite Factory object
            _factory = new DWrite.Factory(DWrite.FactoryType.Shared);
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        public void Dispose()
        {
            _factory = SeeingSharpUtil.DisposeObject(_factory);
        }
    }
}