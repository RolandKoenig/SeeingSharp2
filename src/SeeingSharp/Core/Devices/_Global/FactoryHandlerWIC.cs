using System;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Util;
using WIC = Vortice.WIC;
using static Vortice.WIC.WIC;


namespace SeeingSharp.Core.Devices
{
    public class FactoryHandlerWIC : IDisposable, ICheckDisposed
    {
        private WIC.IWICImagingFactory _wicFactory;

        public bool IsDisposed => this.Factory == null;

        /// <summary>
        /// Gets the WIC factory object.
        /// </summary>
        internal WIC.IWICImagingFactory Factory
        {
            get
            {
                if(_wicFactory == null){ throw new ObjectDisposedException(nameof(FactoryHandlerWIC)); }
                return _wicFactory;
            }
            private set => _wicFactory = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryHandlerWIC" /> class.
        /// </summary>
        internal FactoryHandlerWIC(GraphicsCoreConfiguration coreConfiguration)
        {
            this.Factory = new WIC.IWICImagingFactory();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Factory = SeeingSharpUtil.DisposeObject(this.Factory);
        }
    }
}