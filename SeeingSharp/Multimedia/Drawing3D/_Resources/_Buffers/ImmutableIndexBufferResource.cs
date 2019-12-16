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
using System.Text;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class ImmutableIndexBufferResource : Resource
    {
        // Direct3D resources
        private D3D11.Buffer m_buffer;

        // Configuration
        private Func<int[]> m_bufferDataFactory;

        public ImmutableIndexBufferResource(Func<int[]> bufferDataFactory)
        {
            m_bufferDataFactory = bufferDataFactory;
        }

        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            m_buffer = GraphicsHelper.Internals.CreateImmutableIndexBuffer(
                device, m_bufferDataFactory());
        }

        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            SeeingSharpUtil.SafeDispose(ref m_buffer);
        }

        internal D3D11.Buffer Buffer => m_buffer;

        public override bool IsLoaded => m_buffer != null;
    }
}
