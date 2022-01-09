using System;
using System.Diagnostics;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Core
{
    internal class PipelineStatisticsQueryHelper : IDisposable, ICheckDisposed
    {
        private D3D11.ID3D11DeviceContext _deviceContext;
        private D3D11.ID3D11Query _query;
        private D3D11.QueryDataPipelineStatistics _result;
        private bool _resultGot;

        /// <inheritdoc />
        public bool IsDisposed => _query == null;

        public PipelineStatisticsQueryHelper(EngineDevice device)
        {
            var deviceD3D11 = device.DeviceD3D11_1;
            _deviceContext = device.DeviceImmediateContextD3D11;

            var queryDesc = new D3D11.QueryDescription();
            queryDesc.QueryType = D3D11.QueryType.PipelineStatistics;

            _query = deviceD3D11.CreateQuery(queryDesc);
            
            _deviceContext.Begin(_query);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref _query!);
        }

        public void PrintPipelineStatisticsToDebug(string header)
        {
            var result = this.GetPipelineStatistics();

            Debug.WriteLine("");
            Debug.WriteLine($"===== Print Pipeline Statistics ({header})");
            Debug.WriteLine($"IA VertexCount: {result.IAVertices}");
            Debug.WriteLine($"IA PrimitiveCount: {result.IAPrimitives}");
            Debug.WriteLine($"VS InvocationCount: {result.VSInvocations}");
            Debug.WriteLine($"GS InvocationCount: {result.GSInvocations}");
            Debug.WriteLine($"GS PrimitiveCount: {result.GSPrimitives}");
            Debug.WriteLine($"C  InvocationCount: {result.CInvocations}");
            Debug.WriteLine($"C  PrimitiveCount: {result.CPrimitives}");
            Debug.WriteLine($"PS InvocationCount: {result.PSInvocations}");
        }

        public D3D11.QueryDataPipelineStatistics GetPipelineStatistics()
        {
            if (_query == null) { throw new ObjectDisposedException(nameof(PipelineStatisticsQueryHelper)); }
            if (_resultGot) { return _result; }

            _deviceContext.End(_query);
            while (!_deviceContext.GetData(_query, out _result)) { }
            _resultGot = true;

            return _result;
        }
    }
}