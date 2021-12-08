using System;
using System.Collections.Generic;
using SeeingSharp.Core.Devices;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Core
{
    public class RenderPassDump : IDisposable, ICheckDisposed
    {
        private bool _isDisposed;
        private TextureUploader _uploaderColor;
        private Size2 _size;

        private List<RenderPassDumpEntry> _dumpResults;

        public IReadOnlyList<RenderPassDumpEntry> DumpResults => _dumpResults;

        /// <inheritdoc />
        public bool IsDisposed => _isDisposed;

        internal RenderPassDump(EngineDevice device, Size2 size2, bool isMultisampled)
        {
            _dumpResults = new List<RenderPassDumpEntry>(8);

            _size = size2;
            _uploaderColor = new TextureUploader(
                device, size2.Width, size2.Height, GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT, isMultisampled);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref _uploaderColor);

            foreach (var actDumpResult in _dumpResults)
            {
                SeeingSharpUtil.DisposeObject(actDumpResult);
            }
            _dumpResults.Clear();

            _isDisposed = true;
        }

        internal void Dump(string dumpKey, RenderTargets renderTargets)
        {
            if(_isDisposed){ throw new ObjectDisposedException(nameof(RenderPassDump)); }

            
            var actDumpEntry = new RenderPassDumpEntry(dumpKey, _size);
            using (var colorBufferTexture = SharpGen.Runtime.ComObject.As<D3D11.ID3D11Texture2D>(renderTargets.ColorBuffer.Resource))
            {
                _uploaderColor.UploadToMemoryMappedTexture(colorBufferTexture, actDumpEntry.BufferColor);
            }

            _dumpResults.Add(actDumpEntry);
        }
    }
}
