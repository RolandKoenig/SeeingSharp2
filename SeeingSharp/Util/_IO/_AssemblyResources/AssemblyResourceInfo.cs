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
using System.IO;
using System.Reflection;

namespace SeeingSharp.Util
{
    public class AssemblyResourceInfo
    {
        /// <summary>
        /// Opens a reading stream
        /// </summary>
        public Stream OpenRead()
        {
            return TargetAssembly.GetManifestResourceStream(ResourcePath);
        }

        /// <summary>
        /// Creates a new AssemblyResourceInfo object
        /// </summary>
        internal AssemblyResourceInfo(Assembly targetAssembly, string resourcePath, string key)
        {
            TargetAssembly = targetAssembly;
            ResourcePath = resourcePath;
            Key = key;
        }

        /// <summary>
        /// Gets the path to the resource
        /// </summary>
        public string ResourcePath { get; }

        /// <summary>
        /// Gets the target assembly
        /// </summary>
        public Assembly TargetAssembly { get; }

        /// <summary>
        /// Gets the key of this object
        /// </summary>
        public string Key { get; }
    }
}