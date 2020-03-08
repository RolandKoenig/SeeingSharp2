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
using SeeingSharp.Util;
using DXGI = SharpDX.DXGI;

namespace SeeingSharp.Multimedia.Core
{
    public class FactoryHandlerDXGI : IDisposable, ICheckDisposed
    {
        private DXGI.Factory1 m_dxgiFactory;

        internal FactoryHandlerDXGI(GraphicsCoreConfiguration coreConfiguration)
        {
            m_dxgiFactory = SeeingSharpUtil.TryExecute(() => new DXGI.Factory4());
            if (m_dxgiFactory == null) { m_dxgiFactory = SeeingSharpUtil.TryExecute(() => new DXGI.Factory2()); }
            if (m_dxgiFactory == null) { m_dxgiFactory = SeeingSharpUtil.TryExecute(() => new DXGI.Factory1()); }
            if (m_dxgiFactory == null) { throw new SeeingSharpGraphicsException("Unable to create the DXGI Factory object!"); }
        }

        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref m_dxgiFactory);
        }

        public bool IsDisposed => m_dxgiFactory == null;

        internal DXGI.Factory1 Factory
        {
            get
            {
                if(m_dxgiFactory == null){ throw new ObjectDisposedException(nameof(FactoryHandlerDXGI)); }
                return m_dxgiFactory;
            }
        }
    }
}
