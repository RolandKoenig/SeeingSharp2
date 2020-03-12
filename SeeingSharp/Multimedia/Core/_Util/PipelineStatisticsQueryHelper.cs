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
using System.Diagnostics;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Core
{
    internal class PipelineStatisticsQueryHelper : IDisposable, ICheckDisposed
    {
        private D3D11.DeviceContext m_deviceContext;
        private D3D11.Query m_query;
        private D3D11.QueryDataPipelineStatistics m_result;
        private bool m_resultGot;

        public PipelineStatisticsQueryHelper(EngineDevice device)
        {
            var deviceD3D11 = device.DeviceD3D11_1;
            m_deviceContext = device.DeviceImmediateContextD3D11;

            var queryDesc = new D3D11.QueryDescription();
            queryDesc.Type = D3D11.QueryType.PipelineStatistics;
            m_query = new D3D11.Query(deviceD3D11, queryDesc);
            m_deviceContext.Begin(m_query);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref m_query);
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
            if (m_query == null) { throw new ObjectDisposedException(nameof(PipelineStatisticsQueryHelper)); }
            if (m_resultGot) { return m_result; }

            m_deviceContext.End(m_query);
            while (!m_deviceContext.GetData(m_query, out m_result)) { }
            m_resultGot = true;

            return m_result;
        }

        /// <inheritdoc />
        public bool IsDisposed => m_query == null;
    }
}