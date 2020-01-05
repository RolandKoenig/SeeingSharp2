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
using SeeingSharp.Util;
using DWrite = SharpDX.DirectWrite;

namespace SeeingSharp.Multimedia.Core
{
    public class FactoryHandlerDWrite
    {
        // Resources for DirectWrite
        private DWrite.Factory m_factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryHandlerDWrite"/> class.
        /// </summary>
        internal FactoryHandlerDWrite(GraphicsCoreConfiguration coreConfiguration)
        {
            // Create DirectWrite Factory object
            m_factory = new DWrite.Factory(DWrite.FactoryType.Shared);
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        internal void UnloadResources()
        {
            m_factory = SeeingSharpUtil.DisposeObject(m_factory);
        }

        /// <summary>
        /// Is DirectWrite initialized successfully?
        /// </summary>
        public bool IsInitialized => m_factory != null;

        /// <summary>
        /// Gets the Factory object.
        /// </summary>
        internal DWrite.Factory Factory => m_factory;
    }
}