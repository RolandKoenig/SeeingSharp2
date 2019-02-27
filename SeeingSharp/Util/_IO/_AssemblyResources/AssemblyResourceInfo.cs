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

namespace SeeingSharp.Util
{
    #region using

    using System.IO;
    using System.Reflection;

    #endregion

    public class AssemblyResourceInfo
    {
        private Assembly m_targetAssembly;
        private string m_resourcePath;

        /// <summary>
        /// Creates a new AssemblyResourceInfo object
        /// </summary>
        internal AssemblyResourceInfo(Assembly targetAssembly, string resourcePath, string key)
        {
            m_targetAssembly = targetAssembly;
            m_resourcePath = resourcePath;
            Key = key;
        }

        /// <summary>
        /// Opens a reading stream
        /// </summary>
        public Stream OpenRead()
        {
            return m_targetAssembly.GetManifestResourceStream(m_resourcePath);
        }

        /// <summary>
        /// Gets the path to the resource
        /// </summary>
        public string ResourcePath
        {
            get { return m_resourcePath; }
        }

        /// <summary>
        /// Gets the target assembly
        /// </summary>
        public Assembly TargetAssembly
        {
            get { return m_targetAssembly; }
        }

        /// <summary>
        /// Gets the key of this object
        /// </summary>
        public string Key { get; }
    }
}