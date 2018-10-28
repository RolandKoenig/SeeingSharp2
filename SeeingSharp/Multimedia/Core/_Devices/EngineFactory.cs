#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
#endregion
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

        public FactoryHandlerD2D Direct2D => m_handlerD2D;

        public FactoryHandlerDWrite DirectWrite => m_handlerDWrite;

        public FactoryHandlerWIC WindowsImagingComponent => m_handlerWic;

        internal D2D.Factory2 FactoryD2D_2 => m_handlerD2D.Factory2;
    }
}
