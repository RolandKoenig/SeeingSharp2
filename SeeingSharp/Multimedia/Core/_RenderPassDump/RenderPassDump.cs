﻿/*
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
using System.Text;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Core
{
    public class RenderPassDump : IDisposable, ICheckDisposed
    {
        private bool m_isDisposed;
        private TextureUploader m_uploaderColor;

        private List<RenderPassDumpEntry> m_dumpResults;

        internal RenderPassDump(EngineDevice device, Size2 size2, bool isMultisampled)
        {
            m_dumpResults = new List<RenderPassDumpEntry>(8);

            m_uploaderColor = new TextureUploader(
                device, size2.Width, size2.Height, GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT, isMultisampled);
        }

        internal void Dump(string dumpKey, RenderTargets renderTargets)
        {
            if(m_isDisposed){ throw new ObjectDisposedException(nameof(RenderPassDump)); }

            
        }

        /// <inheritdoc />
        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref m_uploaderColor);

            foreach (var actDumpResult in m_dumpResults)
            {
                SeeingSharpUtil.DisposeObject(actDumpResult);
            }
            m_dumpResults.Clear();

            m_isDisposed = true;
        }

        public IReadOnlyList<RenderPassDumpEntry> DumpResults => m_dumpResults;

        /// <inheritdoc />
        public bool IsDisposed => m_isDisposed;
    }
}
