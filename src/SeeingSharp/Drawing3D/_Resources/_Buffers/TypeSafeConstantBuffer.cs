using SeeingSharp.Core.Devices;
using SeeingSharp.Util.Sdx;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D
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
            : base(SdxUtilities.SizeOf<T>())
        {
            _initialData = new T();
            _structureSize = SdxUtilities.SizeOf<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeSafeConstantBufferResource{T}" /> class.
        /// </summary>
        public TypeSafeConstantBufferResource(T initialData)
            : base(SdxUtilities.SizeOf<T>())
        {
            _initialData = initialData;
            _structureSize = SdxUtilities.SizeOf<T>();
        }

        /// <summary>
        /// Creates the constant buffer object.
        /// </summary>
        protected internal override D3D11.ID3D11Buffer CreateConstantBuffer(EngineDevice device)
        {
            using (var dataStream = new DataStream(SdxUtilities.SizeOf<T>(), true, true))
            {
                dataStream.Write(_initialData);
                dataStream.Position = 0;

                return device.DeviceD3D11_1.CreateBuffer(
                    new D3D11.BufferDescription(
                        _structureSize,
                        D3D11.ResourceUsage.Dynamic,
                        D3D11.BindFlags.ConstantBuffer,
                        D3D11.CpuAccessFlags.Write,
                        D3D11.ResourceOptionFlags.None,
                        0),
                    dataStream.DataPointer);
            }
        }

        /// <summary>
        /// Sets given content to the constant buffer.
        /// </summary>
        /// <param name="deviceContext">The context used for updating the constant buffer.</param>
        /// <param name="dataToSet">The data to set.</param>
        internal void SetData(D3D11.ID3D11DeviceContext deviceContext, T dataToSet)
        {
            var dataBox = deviceContext.Map(this.ConstantBuffer, 0, D3D11.MapMode.WriteDiscard, D3D11.MapFlags.None);
            SdxUtilities.Write(dataBox.DataPointer, ref dataToSet);
            deviceContext.Unmap(this.ConstantBuffer, 0);
        }
    }
}
