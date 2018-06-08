using System;
using System.Collections.Generic;
using System.Text;

using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Multimedia.Core
{
    public class EngineFactory
    {
        private FactoryHandlerD2D m_handlerD2D;
        private FactoryHandlerDWrite m_handlerDWrite;
        private FactoryHandlerWIC m_handlerWic;

        public EngineFactory(DeviceLoadSettings loadSettings)
        {
            m_handlerWic = new FactoryHandlerWIC(loadSettings);
            m_handlerD2D = new FactoryHandlerD2D(loadSettings);
            m_handlerDWrite = new FactoryHandlerDWrite(loadSettings);
        }

        internal D2D.Factory2 FactoryD2D_2 => m_handlerD2D.Factory2;
    }
}
