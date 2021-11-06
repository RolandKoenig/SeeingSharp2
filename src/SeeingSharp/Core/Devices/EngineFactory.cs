using System;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Util;
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Core.Devices
{
    public class EngineFactory : IDisposable, ICheckDisposed
    {
        private FactoryHandlerDXGI _handlerDXGI;
        private FactoryHandlerD2D _handlerDirect2D;
        private FactoryHandlerDWrite _handlerDirectWrite;
        private FactoryHandlerWIC _handlerWIC;

        public bool IsDisposed { get; private set; }

        public FactoryHandlerDXGI DXGI
        {
            get
            {
                if(this.IsDisposed){ throw new ObjectDisposedException(nameof(EngineFactory)); }
                return _handlerDXGI;
            }
            private set => _handlerDXGI = value;
        }

        public FactoryHandlerD2D Direct2D
        {
            get
            {
                if(this.IsDisposed){ throw new ObjectDisposedException(nameof(EngineFactory)); }
                return _handlerDirect2D;
            }
            private set => _handlerDirect2D = value;
        }

        public FactoryHandlerDWrite DirectWrite
        {
            get
            {
                if(this.IsDisposed){ throw new ObjectDisposedException(nameof(EngineFactory)); }
                return _handlerDirectWrite;
            }
            private set => _handlerDirectWrite = value;
        }

        public FactoryHandlerWIC WIC
        {
            get
            {
                if(this.IsDisposed){ throw new ObjectDisposedException(nameof(EngineFactory)); }
                return _handlerWIC;
            }
            private set => _handlerWIC = value;
        }

        internal D2D.Factory2 FactoryD2D_2
        {
            get
            {
                if(this.IsDisposed){ throw new ObjectDisposedException(nameof(EngineFactory)); }
                return this.Direct2D.Factory;
            }
        }

        internal EngineFactory(GraphicsCoreConfiguration coreConfiguration)
        {
            this.DXGI = new FactoryHandlerDXGI(coreConfiguration);
            this.WIC = new FactoryHandlerWIC(coreConfiguration);
            this.Direct2D = new FactoryHandlerD2D(coreConfiguration);
            this.DirectWrite = new FactoryHandlerDWrite(coreConfiguration);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.DXGI = SeeingSharpUtil.DisposeObject(this.DXGI);
            this.Direct2D = SeeingSharpUtil.DisposeObject(this.Direct2D);
            this.DirectWrite = SeeingSharpUtil.DisposeObject(this.DirectWrite);
            this.WIC = SeeingSharpUtil.DisposeObject(this.WIC);
            this.IsDisposed = true;
        }
    }
}