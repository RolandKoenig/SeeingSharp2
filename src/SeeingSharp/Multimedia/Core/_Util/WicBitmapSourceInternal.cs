using System;
using SeeingSharp.Util;
using SharpDX.WIC;

namespace SeeingSharp.Multimedia.Core
{
    public class WicBitmapSourceInternal : IDisposable, ICheckDisposed
    {
        public bool IsDisposed => this.Converter == null;

        internal BitmapDecoder Decoder
        {
            get;
            private set;
        }

        internal FormatConverter Converter
        {
            get;
            private set;
        }

        public WicBitmapSourceInternalInternals Internals { get; }

        internal WicBitmapSourceInternal(BitmapDecoder decoder, FormatConverter converter)
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

            public BitmapDecoder Decoder => _owner.Decoder;

            public FormatConverter Converter => _owner.Converter;

            public WicBitmapSourceInternalInternals(WicBitmapSourceInternal owner)
            {
                _owner = owner;
            }
        }
    }
}
