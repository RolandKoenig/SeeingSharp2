#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion

#region using

using DWrite = SharpDX.DirectWrite;

#endregion

namespace SeeingSharp.Multimedia.Core
{
    #region using

    using SeeingSharp.Util;

    #endregion

    public class FactoryHandlerDWrite
    {
        #region Resources for DirectWrite

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryHandlerDWrite"/> class.
        /// </summary>
        /// <param name="core">The core.</param>
        internal FactoryHandlerDWrite(DeviceLoadSettings deviceLoadSettings)
        {
            // Create DirectWrite Factory object
            Factory = new DWrite.Factory(DWrite.FactoryType.Shared);
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        internal void UnloadResources()
        {
            Factory = SeeingSharpUtil.DisposeObject(Factory);
        }

        /// <summary>
        /// Gets the Factory object.
        /// </summary>
        internal DWrite.Factory Factory { get; private set; }

        /// <summary>
        /// Is DirectWrite initialized successfully?
        /// </summary>
        public bool IsInitialized => Factory != null;
    }
}