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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class ImportedResourceInfo
    {
        public NamedOrGenericKey ResourceKey
        {
            get;
        }

        public Func<EngineDevice, Resource> ResourceFactory
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportedResourceInfo"/> class.
        /// </summary>
        /// <param name="resourceKey">The key which is used by added the resource to a scene.</param>
        /// <param name="resourceFactory">The resource factory.</param>
        public ImportedResourceInfo(NamedOrGenericKey resourceKey, Func<EngineDevice, Resource> resourceFactory)
        {
            this.ResourceKey = resourceKey;
            this.ResourceFactory = resourceFactory;
        }

        public override string ToString()
        {
            return this.ResourceKey.ToString();
        }
    }
}
