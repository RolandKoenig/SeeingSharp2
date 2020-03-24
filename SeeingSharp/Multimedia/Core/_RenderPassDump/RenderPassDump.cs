/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Collections.Generic;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Core
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
            using (var colorBufferTexture = renderTargets.ColorBuffer.ResourceAs<D3D11.Texture2D>())
            {
                _uploaderColor.UploadToMemoryMappedTexture(colorBufferTexture, actDumpEntry.BufferColor);
            }

            _dumpResults.Add(actDumpEntry);
        }
    }
}
