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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeeingSharp.Util
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public class AssemblyResourceFileAttribute : Attribute
    {
        private string m_resourcePath;
        private string m_key;

        /// <summary>
        /// Links the given resource
        /// </summary>
        public AssemblyResourceFileAttribute(string resourcePath)
        {
            m_resourcePath = resourcePath;
            m_key = null;
        }

        /// <summary>
        /// Links the given resource with the given key
        /// </summary>
        public AssemblyResourceFileAttribute(string resourcePath, string key)
        {
            m_resourcePath = resourcePath;
            m_key = key;
        }

        /// <summary>
        /// Gets the path to the resource
        /// </summary>
        public string ResourcePath
        {
            get { return m_resourcePath; }
        }

        /// <summary>
        /// Gets the key (may be null)
        /// </summary>
        public string Key
        {
            get { return m_key; }
        }
    }
}
