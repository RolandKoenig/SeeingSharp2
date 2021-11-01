using SeeingSharp.Multimedia.Core;
using SharpDX;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class TypeSafeConstantBufferResource<T> : ConstantBufferResource
        where T : struct
    {
        // Configuration
        private T _initialData;
        private int _structureSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeSafeConstantBufferResource{T}" /> class.
        /// </summary>
        public TypeSafeConstantBufferResource()
            : base(Utilities.SizeOf<T>())
        {
            _initialData = new T();
            _structureSize = Utilities.SizeOf<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeSafeConstantBufferResource{T}" /> class.
        /// </summary>
        public TypeSafeConstantBufferResource(T initialData)
            : base(Utilities.SizeOf<T>())
        {
            _initialData = initialData;
            _structureSize = Utilities.SizeOf<T>();
        }

        /// <summary>
        /// Creates the constant buffer object.
        /// </summary>
        protected internal override D3D11.Buffer CreateConstantBuffer(EngineDevice device)
        {
            using (var dataStream = new DataStream(Utilities.SizeOf<T>(), true, true))
            {
                dataStream.Write(_initialData);
                dataStream.Position = 0;

                return new D3D11.Buffer(
                    device.DeviceD3D11_1,
                    dataStream,
                    new D3D11.BufferDescription(
                        _structureSize,
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
