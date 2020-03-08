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
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Multimedia.Core
{
    public class EngineFactory : IDisposable, ICheckDisposed
    {
        private FactoryHandlerDXGI m_handlerDXGI;
        private FactoryHandlerD2D m_handlerDirect2D;
        private FactoryHandlerDWrite m_handlerDirectWrite;
        private FactoryHandlerWIC m_handlerWIC;

        internal EngineFactory(GraphicsCoreConfiguration coreConfiguration)
        {
            this.DXGI = new FactoryHandlerDXGI(coreConfiguration);
            this.WIC = new FactoryHandlerWIC(coreConfiguration);
            this.Direct2D = new FactoryHandlerD2D(coreConfiguration);
            this.DirectWrite = new FactoryHandlerDWrite(coreConfiguration);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.DXGI = SeeingSharpUtil.DisposeObject(this.DXGI);
            this.Direct2D = SeeingSharpUtil.DisposeObject(this.Direct2D);
            this.DirectWrite = SeeingSharpUtil.DisposeObject(this.DirectWrite);
            this.WIC = SeeingSharpUtil.DisposeObject(this.WIC);
            this.IsDisposed = true;
        }

        public bool IsDisposed { get; private set; }

        public FactoryHandlerDXGI DXGI
        {
            get
            {
                if(this.IsDisposed){ throw new ObjectDisposedException(nameof(EngineFactory)); }
                return m_handlerDXGI;
            }
            private set => m_handlerDXGI = value;
        }

        public FactoryHandlerD2D Direct2D
        {
            get
            {
                if(this.IsDisposed){ throw new ObjectDisposedException(nameof(EngineFactory)); }
                return m_handlerDirect2D;
            }
            private set => m_handlerDirect2D = value;
        }

        public FactoryHandlerDWrite DirectWrite
        {
            get
            {
                if(this.IsDisposed){ throw new ObjectDisposedException(nameof(EngineFactory)); }
                return m_handlerDirectWrite;
            }
            private set => m_handlerDirectWrite = value;
        }

        public FactoryHandlerWIC WIC
        {
            get
            {
                if(this.IsDisposed){ throw new ObjectDisposedException(nameof(EngineFactory)); }
                return m_handlerWIC;
            }
            private set => m_handlerWIC = value;
        }

        internal D2D.Factory2 FactoryD2D_2
        {
            get
            {
                if(this.IsDisposed){ throw new ObjectDisposedException(nameof(EngineFactory)); }
                return this.Direct2D.Factory;
            }
        }
    }
}