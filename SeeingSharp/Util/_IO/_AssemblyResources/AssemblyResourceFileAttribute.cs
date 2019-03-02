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

namespace SeeingSharp.Util
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public class AssemblyResourceFileAttribute : Attribute
    {
        /// <summary>
        /// Links the given resource
        /// </summary>
        public AssemblyResourceFileAttribute(string resourcePath)
        {
            ResourcePath = resourcePath;
            Key = null;
        }

        /// <summary>
        /// Links the given resource with the given key
        /// </summary>
        public AssemblyResourceFileAttribute(string resourcePath, string key)
        {
            ResourcePath = resourcePath;
            Key = key;
        }

        /// <summary>
        /// Gets the path to the resource
        /// </summary>
        public string ResourcePath { get; }

        /// <summary>
        /// Gets the key (may be null)
        /// </summary>
        public string Key { get; }
    }
}