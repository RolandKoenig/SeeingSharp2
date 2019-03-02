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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class ConstantBufferResource : Resource
    {
        // Direct3D resources
        private D3D11.Buffer m_constantBuffer;

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            m_constantBuffer = CreateConstantBuffer(device);
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            m_constantBuffer = SeeingSharpTools.DisposeObject(m_constantBuffer);
        }

        /// <summary>
        /// Creates the constant buffer object.
        /// </summary>
        protected internal virtual D3D11.Buffer CreateConstantBuffer(EngineDevice device)
        {
            return new D3D11.Buffer(
                device.DeviceD3D11_1,
                BufferSize,
                D3D11.ResourceUsage.Dynamic,
                D3D11.BindFlags.ConstantBuffer,
                D3D11.CpuAccessFlags.Write,
                D3D11.ResourceOptionFlags.None,
                0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantBufferResource" /> class.
        /// </summary>
        public ConstantBufferResource(int bufferSize)
        {
            if (bufferSize < 1) { throw new ArgumentException("Invalid value for buffer size!", "bufferSize"); }
            BufferSize = bufferSize;
        }

        /// <summary>
        /// Is the buffer loaded correctly?
        /// </summary>
        public override bool IsLoaded => m_constantBuffer != null;

        /// <summary>
        /// Gets the buffer object.
        /// </summary>
        internal D3D11.Buffer ConstantBuffer => m_constantBuffer;

        /// <summary>
        /// Gets the total size of the constant buffer.
        /// </summary>
        public int BufferSize { get; }
    }
}