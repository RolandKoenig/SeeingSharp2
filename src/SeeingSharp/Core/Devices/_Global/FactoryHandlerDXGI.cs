using System;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Util;
using Vortice.DXGI;
using static Vortice.DXGI.DXGI;

namespace SeeingSharp.Core.Devices
{
    public class FactoryHandlerDXGI : IDisposable, ICheckDisposed
    {
        private IDXGIFactory1 _dxgiFactory;

        public bool IsDisposed => _dxgiFactory == null;

        internal IDXGIFactory1 Factory
        {
            get
            {
                if(_dxgiFactory == null){ throw new ObjectDisposedException(nameof(FactoryHandlerDXGI)); }
                return _dxgiFactory;
            }
        }

        internal FactoryHandlerDXGI(GraphicsCoreConfiguration coreConfiguration)
        {
            _dxgiFactory = SeeingSharpUtil.TryExecute(() => CreateDXGIFactory2<IDXGIFactory4>(coreConfiguration.DebugEnabled));
            if (_dxgiFactory == null) { _dxgiFactory = SeeingSharpUtil.TryExecute(() => CreateDXGIFactory2<IDXGIFactory2>(coreConfiguration.DebugEnabled)); }
            if (_dxgiFactory == null) { _dxgiFactory = SeeingSharpUtil.TryExecute(() => CreateDXGIFactory1<IDXGIFactory1>()); }
            if (_dxgiFactory == null) { throw new SeeingSharpGraphicsException("Unable to create the DXGI Factory object!"); }
        }

        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref _dxgiFactory);
        }
    }
}
