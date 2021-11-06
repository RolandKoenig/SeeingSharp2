using System;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.Core
{
    public class RenderPassDumpEntry : IDisposable, ICheckDisposed
    {
        private MemoryMappedTexture<int> _bufferColor;

        public string Key { get; }

        public MemoryMappedTexture<int> BufferColor => _bufferColor;

        /// <inheritdoc />
        public bool IsDisposed => _bufferColor == null;

        internal RenderPassDumpEntry(string dumpKey, Size2 size)
        {
            this.Key = dumpKey;
            _bufferColor = new MemoryMappedTexture<int>(size);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Key;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref _bufferColor);
        }
    }
}
