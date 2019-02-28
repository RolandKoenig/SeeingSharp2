#region License information
/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwhise.
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
namespace SeeingSharp.Multimedia.Objects
{
    #region using

    using SeeingSharp.Util;

    #endregion

    public class FloorTileInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FloorTileInfo"/> class.
        /// </summary>
        /// <param name="material">The material to use for the tile (string.Empty or null to use default material).</param>
        public FloorTileInfo(NamedOrGenericKey material)
        {
            Material = material;
        }

        /// <summary>
        /// Gets the material used for this tile.
        /// </summary>
        public NamedOrGenericKey Material { get; }
    }
}