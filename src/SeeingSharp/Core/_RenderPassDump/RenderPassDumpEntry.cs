using System;
using System.Drawing;
using SeeingSharp.Util;

namespace SeeingSharp.Core
{
    public class RenderPassDumpEntry : IDisposable, ICheckDisposed
    {
        private MemoryMappedTexture<int>? _bufferColor;

        public string Key { get; }

        public MemoryMappedTexture<int> BufferColor
        {
            get
            {
                if (_bufferColor == null) { throw new ObjectDisposedException(nameof(RenderPassDumpEntry)); }
                return _bufferColor;
            }
        }

        /// <inheritdoc />
        public bool IsDisposed => _bufferColor == null;

        internal RenderPassDumpEntry(string dumpKey, Size size)
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
