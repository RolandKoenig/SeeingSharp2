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
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Core
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
