﻿using System;
using SeeingSharp.Checking;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D.Resources
{
    public class ImmutableVertexBufferResource<T> : Resource
        where T : unmanaged
    {
        // Direct3D resources
        private D3D11.ID3D11Buffer? _buffer;

        // Configuration
        private Func<T[]> _bufferDataFactory;

        internal D3D11.ID3D11Buffer Buffer
        {
            get
            {
                _buffer.EnsureResourceLoaded(typeof(ImmutableVertexBufferResource<T>));
                return _buffer!;
            }
        }

        public override bool IsLoaded => _buffer != null;

        public ImmutableVertexBufferResource(Func<T[]> bufferDataFactory)
        {
            _bufferDataFactory = bufferDataFactory;
        }

        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _buffer = GraphicsHelper.Internals.CreateImmutableVertexBuffer(
                device, _bufferDataFactory());
        }

        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            SeeingSharpUtil.SafeDispose(ref _buffer);
        }
    }
}
