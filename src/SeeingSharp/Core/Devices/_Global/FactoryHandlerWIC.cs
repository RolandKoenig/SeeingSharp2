using System;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Util;
using Vortice.WIC;

namespace SeeingSharp.Core.Devices
{
    public class FactoryHandlerWIC : IDisposable, ICheckDisposed
    {
        private IWICImagingFactory? _wicFactory;

        public bool IsDisposed => _wicFactory == null;

        /// <summary>
        /// Gets the WIC factory object.
        /// </summary>
        internal IWICImagingFactory Factory
        {
            get
            {
                if(_wicFactory == null){ throw new ObjectDisposedException(nameof(FactoryHandlerWIC)); }
                return _wicFactory;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryHandlerWIC" /> class.
        /// </summary>
        internal FactoryHandlerWIC(GraphicsCoreConfiguration coreConfiguration)
        {
            _wicFactory = new IWICImagingFactory();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _wicFactory = SeeingSharpUtil.DisposeObject(this.Factory);
        }
    }
}