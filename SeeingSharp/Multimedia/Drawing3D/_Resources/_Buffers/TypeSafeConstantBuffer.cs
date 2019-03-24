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
using SharpDX;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class TypeSafeConstantBufferResource<T> : ConstantBufferResource
        where T : struct
    {
        // Configuration
        private T m_initialData;
        private int m_structureSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeSafeConstantBufferResource{T}" /> class.
        /// </summary>
        public TypeSafeConstantBufferResource()
            : base(Utilities.SizeOf<T>())
        {
            m_initialData = new T();
            m_structureSize = Utilities.SizeOf<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeSafeConstantBufferResource{T}" /> class.
        /// </summary>
        public TypeSafeConstantBufferResource(T initialData)
            : base(Utilities.SizeOf<T>())
        {
            m_initialData = initialData;
            m_structureSize = Utilities.SizeOf<T>();
        }

        /// <summary>
        /// Creates the constant buffer object.
        /// </summary>
        protected internal override D3D11.Buffer CreateConstantBuffer(EngineDevice device)
        {
            using (var dataStream = new DataStream(Utilities.SizeOf<T>(), true, true))
            {
                dataStream.Write(m_initialData);
                dataStream.Position = 0;

                return new D3D11.Buffer(
                    device.DeviceD3D11_1,
                    dataStream,
                    new D3D11.BufferDescription(
                        m_structureSize,
                        D3D11.ResourceUsage.Dynamic,
                        D3D11.BindFlags.ConstantBuffer,
                        D3D11.CpuAccessFlags.Write,
                        D3D11.ResourceOptionFlags.None,
                        0));
            }
        }

        /// <summary>
        /// Sets given content to the constant buffer.
        /// </summary>
        /// <param name="deviceContext">The context used for updating the constant buffer.</param>
        /// <param name="dataToSet">The data to set.</param>
        internal void SetData(D3D11.DeviceContext deviceContext, T dataToSet)
        {
            var dataBox = deviceContext.MapSubresource(this.ConstantBuffer, 0, D3D11.MapMode.WriteDiscard, D3D11.MapFlags.None);
            Utilities.Write(dataBox.DataPointer, ref dataToSet);
            deviceContext.UnmapSubresource(this.ConstantBuffer, 0);
        }
    }
}
