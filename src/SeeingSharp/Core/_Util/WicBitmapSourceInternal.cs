using System;
using SeeingSharp.Util;
using Vortice.WIC;

namespace SeeingSharp.Core
{
    public class WicBitmapSourceInternal : IDisposable, ICheckDisposed
    {
        private IWICBitmapDecoder? _decoder;
        private IWICFormatConverter? _converter;

        public bool IsDisposed => _decoder == null;

        internal IWICBitmapDecoder Decoder
        {
            get
            {
                if (_decoder == null) { throw new ObjectDisposedException(nameof(WicBitmapSourceInternal)); }
                return _decoder;
            }
        }

        internal IWICFormatConverter Converter
        {
            get
            {
                if (_converter == null) { throw new ObjectDisposedException(nameof(WicBitmapSourceInternal)); }
                return _converter;
            }
        }

        public WicBitmapSourceInternalInternals Internals { get; }

        internal WicBitmapSourceInternal(IWICBitmapDecoder decoder, IWICFormatConverter converter)
        {
            _decoder = decoder;
            _converter = converter;

            this.Internals = new WicBitmapSourceInternalInternals(this);
        }

        public void Dispose()
        {
            _decoder = SeeingSharpUtil.DisposeObject(_decoder);
            _converter = SeeingSharpUtil.DisposeObject(_converter);
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        public class WicBitmapSourceInternalInternals
        {
            private WicBitmapSourceInternal _owner;

            public IWICBitmapDecoder Decoder => _owner.Decoder;

            public IWICFormatConverter Converter => _owner.Converter;

            public WicBitmapSourceInternalInternals(WicBitmapSourceInternal owner)
            {
                _owner = owner;
            }
        }
    }
}
