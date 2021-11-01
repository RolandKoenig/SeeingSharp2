using System;
using SeeingSharp.Util;
using SharpDX.WIC;

namespace SeeingSharp.Multimedia.Core
{
    public class FactoryHandlerWIC : IDisposable, ICheckDisposed
    {
        private ImagingFactory _wicFactory;

        public bool IsDisposed => this.Factory == null;

        /// <summary>
        /// Gets the WIC factory object.
        /// </summary>
        internal ImagingFactory Factory
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
            this.Factory = new ImagingFactory();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Factory = SeeingSharpUtil.DisposeObject(this.Factory);
        }
    }
}