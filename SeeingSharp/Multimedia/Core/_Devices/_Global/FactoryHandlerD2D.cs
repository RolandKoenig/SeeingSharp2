#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
using SeeingSharp.Util;
using System;

//Some namespace mappings
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Multimedia.Core
{
    public class FactoryHandlerD2D
    {
        #region Resources form Direct2D api
        private D2D.Factory2 m_factory2;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryHandlerD2D"/> class.
        /// </summary>
        internal FactoryHandlerD2D(DeviceLoadSettings deviceLoadSettings)
        {
            m_factory2 = new D2D.Factory2(
                D2D.FactoryType.SingleThreaded,
                deviceLoadSettings.DebugEnabled ? D2D.DebugLevel.Information : D2D.DebugLevel.None);
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        internal void UnloadResources()
        {
            SeeingSharpUtil.SafeDispose(ref m_factory2);
        }

        internal D2D.Factory2 Factory2
        {
            get { return m_factory2; }
        }

        /// <summary>
        /// Is Direct2D initialized?
        /// </summary>
        public bool IsInitialized
        {
            get { return m_factory2 != null; }
        }
    }
}