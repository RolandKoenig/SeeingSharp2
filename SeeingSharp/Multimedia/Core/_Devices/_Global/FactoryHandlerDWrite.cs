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
using DWrite = SharpDX.DirectWrite;

namespace SeeingSharp.Multimedia.Core
{
    public class FactoryHandlerDWrite : IDisposable, ICheckDisposed
    {
        private DWrite.Factory _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryHandlerDWrite"/> class.
        /// </summary>
        internal FactoryHandlerDWrite(GraphicsCoreConfiguration coreConfiguration)
        {
            // Create DirectWrite Factory object
            _factory = new DWrite.Factory(DWrite.FactoryType.Shared);
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        public void Dispose()
        {
            _factory = SeeingSharpUtil.DisposeObject(_factory);
        }

        /// <summary>
        /// Is DirectWrite initialized successfully?
        /// </summary>
        public bool IsDisposed => _factory == null;

        /// <summary>
        /// Gets the Factory object.
        /// </summary>
        internal DWrite.Factory Factory
        {
            get
            {
                if(_factory == null){ throw new ObjectDisposedException(nameof(FactoryHandlerDWrite)); }
                return _factory;
            }
        }
    }
}