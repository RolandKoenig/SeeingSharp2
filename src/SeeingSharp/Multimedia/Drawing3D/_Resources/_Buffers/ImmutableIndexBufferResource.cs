﻿using System;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class ImmutableIndexBufferResource : Resource
    {
        // Direct3D resources
        private D3D11.Buffer _buffer;

        // Configuration
        private Func<int[]> _bufferDataFactory;

        internal D3D11.Buffer Buffer => _buffer;

        public override bool IsLoaded => _buffer != null;

        public ImmutableIndexBufferResource(Func<int[]> bufferDataFactory)
        {
            _bufferDataFactory = bufferDataFactory;
        }

        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _buffer = GraphicsHelper.Internals.CreateImmutableIndexBuffer(
                device, _bufferDataFactory());
        }

        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            SeeingSharpUtil.SafeDispose(ref _buffer);
        }
    }
}
