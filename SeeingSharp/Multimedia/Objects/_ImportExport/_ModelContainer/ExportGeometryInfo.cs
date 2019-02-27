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

namespace SeeingSharp.Multimedia.Objects
{
    #region using

    using SeeingSharp.Util;

    #endregion

    /// <summary>
    /// All needed information for exported geometry.
    /// </summary>
    public class ExportGeometryInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportGeometryInfo"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="objType">Type of the object.</param>
        public ExportGeometryInfo(NamedOrGenericKey key, ObjectType objType)
        {
            Key = key;
            ObjectType = objType;
        }

        public NamedOrGenericKey Key { get; }

        public ObjectType ObjectType { get; }
    }
}