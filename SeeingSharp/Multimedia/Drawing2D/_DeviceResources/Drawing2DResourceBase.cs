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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public abstract class Drawing2DResourceBase : IDisposable, ICheckDisposed
    {
        #region Helper flags
        private bool m_isDisposed;
        #endregion

        /// <summary>
        /// Disposes this object.
        /// </summary>
        public virtual void Dispose()
        {
            if (!m_isDisposed)
            {
                m_isDisposed = true;

                GraphicsCore.Current.MainLoop.RegisterForUnload(this);
            }
        }

        /// <summary>
        /// Unloads all resources loaded on the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to unload the resource.</param>
        internal abstract void UnloadResources(EngineDevice engineDevice);

        /// <summary>
        /// Is this object disposed?
        /// </summary>
        public bool IsDisposed
        {
            get { return m_isDisposed; }
        }
    }
}
