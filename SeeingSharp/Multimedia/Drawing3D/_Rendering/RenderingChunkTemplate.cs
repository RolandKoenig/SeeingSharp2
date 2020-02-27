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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using System;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class RenderingChunkTemplate : IDisposable, ICheckDisposed
    {
        internal D3D11.Buffer IndexBuffer;
        internal int IndexBufferID;
        internal int IndexCount;
        internal int SizePerVertex;
        internal int StartIndex;
        internal D3D11.Buffer VertexBuffer;
        internal int VertexBufferID;
        internal Geometry Geometry;
        internal D3D11.InputElement[] InputElements;

        public RenderingChunk CreateChunk(EngineDevice device, MaterialResource material)
        {
            var result = new RenderingChunk();
            result.Template = this;
            result.Material = material;
            result.InputLayout = material.GetInputLayout(device, InputElements);

            return result;
        }

        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref IndexBuffer);
            SeeingSharpUtil.SafeDispose(ref VertexBuffer);
        }

        public bool IsDisposed => IndexBuffer != null;
    }
}
