using System;
using SeeingSharp.Util;
using SharpDX.DXGI;

namespace SeeingSharp.Core
{
    public class FactoryHandlerDXGI : IDisposable, ICheckDisposed
    {
        private Factory1 _dxgiFactory;

        public bool IsDisposed => _dxgiFactory == null;

        internal Factory1 Factory
        {
            get
            {
                if(_dxgiFactory == null){ throw new ObjectDisposedException(nameof(FactoryHandlerDXGI)); }
                return _dxgiFactory;
            }
        }

        internal FactoryHandlerDXGI(GraphicsCoreConfiguration coreConfiguration)
        {
            _dxgiFactory = SeeingSharpUtil.TryExecute(() => new Factory4());
            if (_dxgiFactory == null) { _dxgiFactory = SeeingSharpUtil.TryExecute(() => new Factory2()); }
            if (_dxgiFactory == null) { _dxgiFactory = SeeingSharpUtil.TryExecute(() => new Factory1()); }
            if (_dxgiFactory == null) { throw new SeeingSharpGraphicsException("Unable to create the DXGI Factory object!"); }
        }

        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref _dxgiFactory);
        }
    }
}
