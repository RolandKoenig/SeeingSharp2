using System;
using SeeingSharp.Util;
using Vortice.WIC;

namespace SeeingSharp.Core
{
    public class WicBitmapSourceInternal : IDisposable, ICheckDisposed
    {
        public bool IsDisposed => this.Converter == null;

        internal IWICBitmapDecoder Decoder
        {
            get;
            private set;
        }

        internal IWICFormatConverter Converter
        {
            get;
            private set;
        }

        public WicBitmapSourceInternalInternals Internals { get; }

        internal WicBitmapSourceInternal(IWICBitmapDecoder decoder, IWICFormatConverter converter)
        {
            this.Decoder = decoder;
            this.Converter = converter;

            this.Internals = new WicBitmapSourceInternalInternals(this);
        }

        public void Dispose()
        {
            this.Converter = SeeingSharpUtil.DisposeObject(this.Converter);
            this.Decoder = SeeingSharpUtil.DisposeObject(this.Decoder);
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
