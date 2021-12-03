using System;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Util;
using DWrite = Vortice.DirectWrite;
using static Vortice.DirectWrite.DWrite;

namespace SeeingSharp.Core.Devices
{
    public class FactoryHandlerDWrite : IDisposable, ICheckDisposed
    {
        private DWrite.IDWriteFactory _factory;

        /// <summary>
        /// Is DirectWrite initialized successfully?
        /// </summary>
        public bool IsDisposed => _factory == null;

        /// <summary>
        /// Gets the Factory object.
        /// </summary>
        internal DWrite.IDWriteFactory Factory
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
            _factory = DWriteCreateFactory<DWrite.IDWriteFactory>(DWrite.FactoryType.Shared);
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