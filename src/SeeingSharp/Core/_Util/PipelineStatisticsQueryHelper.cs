using System;
using System.Diagnostics;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Core
{
    internal class PipelineStatisticsQueryHelper : IDisposable, ICheckDisposed
    {
        private D3D11.DeviceContext _deviceContext;
        private D3D11.Query _query;
        private D3D11.QueryDataPipelineStatistics _result;
        private bool _resultGot;

        /// <inheritdoc />
        public bool IsDisposed => _query == null;

        public PipelineStatisticsQueryHelper(EngineDevice device)
        {
            var deviceD3D11 = device.DeviceD3D11_1;
            _deviceContext = device.DeviceImmediateContextD3D11;

            var queryDesc = new D3D11.QueryDescription();
            queryDesc.Type = D3D11.QueryType.PipelineStatistics;
            _query = new D3D11.Query(deviceD3D11, queryDesc);
            _deviceContext.Begin(_query);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref _query);
        }

        public void PrintPipelineStatisticsToDebug(string header)
        {
            var result = this.GetPipelineStatistics();

            Debug.WriteLine("");
            Debug.WriteLine($"===== Print Pipeline Statistics ({header})");
            Debug.WriteLine($"IA VertexCount: {result.IAVerticeCount}");
            Debug.WriteLine($"IA PrimitiveCount: {result.IAPrimitiveCount}");
            Debug.WriteLine($"VS InvocationCount: {result.VSInvocationCount}");
            Debug.WriteLine($"GS InvocationCount: {result.GSInvocationCount}");
            Debug.WriteLine($"GS PrimitiveCount: {result.GSPrimitiveCount}");
            Debug.WriteLine($"C  InvocationCount: {result.CInvocationCount}");
            Debug.WriteLine($"C  PrimitiveCount: {result.CPrimitiveCount}");
            Debug.WriteLine($"PS InvocationCount: {result.PSInvocationCount}");
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