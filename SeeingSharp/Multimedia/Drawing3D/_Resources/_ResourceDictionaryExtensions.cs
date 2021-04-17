/*
    SeeingSharp and all applications distributed together with it. 
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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Drawing3D
{
    internal static class ResourceDictionaryExtensions
    {
        public static MaterialResource GetOrCreateDefaultMaterialResource(this ResourceDictionary resourceDict)
        {
            return resourceDict.GetResourceAndEnsureLoaded(
                ResourceKeys.RES_MATERIAL_COLORED,
                () => new StandardMaterialResource());
        }

        /// <summary>
        /// Adds a new geometry resource.
        /// </summary>
        internal static GeometryResource AddGeometry(this ResourceDictionary resourceDictionary, Geometry geometry)
        {
            return resourceDictionary.AddResource(new GeometryResource(geometry));
        }

        /// <summary>
        /// Adds a new geometry resource.
        /// </summary>
        internal static GeometryResource AddGeometry(this ResourceDictionary resourceDictionary, NamedOrGenericKey resourceKey, Geometry geometry)
        {
            return resourceDictionary.AddResource(resourceKey, new GeometryResource(geometry));
        }

        /// <summary>
        /// Adds a new geometry resource.
        /// </summary>
        internal static GeometryResource AddGeometry(this ResourceDictionary resourceDictionary, GeometryFactory objectType)
        {
            return resourceDictionary.AddResource(new GeometryResource(objectType));
        }

        /// <summary>
        /// Adds a new geometry resource.
        /// </summary>
        internal static GeometryResource AddGeometry(this ResourceDictionary resourceDictionary, NamedOrGenericKey resourceKey, GeometryFactory objectType)
        {
            return resourceDictionary.AddResource(resourceKey, new GeometryResource(objectType));
        }

        /// <summary>
        /// Adds a new text geometry with the given text.
        /// </summary>
        internal static GeometryResource AddTextGeometry(this ResourceDictionary resourceDictionary, string textToAdd)
        {
            return resourceDictionary.AddTextGeometry(textToAdd, TextGeometryOptions.Default);
        }

        /// <summary>
        /// Adds a new text geometry with the given text.
        /// </summary>
        internal static GeometryResource AddTextGeometry(this ResourceDictionary resourceDictionary, NamedOrGenericKey resourceKey, string textToAdd)
        {
            return resourceDictionary.AddTextGeometry(resourceKey, textToAdd, TextGeometryOptions.Default);
        }

        /// <summary>
        /// Adds a new text geometry with the given text.
        /// </summary>
        internal static GeometryResource AddTextGeometry(this ResourceDictionary resourceDictionary, string textToAdd, TextGeometryOptions textGeometryOptions)
        {
            var newGeometry = new Geometry();
            newGeometry.FirstSurface.BuildTextGeometry(textToAdd, textGeometryOptions);
            return resourceDictionary.AddGeometry(newGeometry);
        }

        /// <summary>
        /// Adds a new text geometry with the given text.
        /// </summary>
        internal static GeometryResource AddTextGeometry(this ResourceDictionary resourceDictionary, NamedOrGenericKey resourceKey, string textToAdd, TextGeometryOptions textGeometryOptions)
        {
            var newGeometry = new Geometry();
            newGeometry.FirstSurface.BuildTextGeometry(textToAdd, textGeometryOptions);
            return resourceDictionary.AddGeometry(resourceKey, newGeometry);
        }

        /// <summary>
        /// Adds a new texture resource pointing to the given texture file name.
        /// </summary>
        internal static StandardTextureResource AddTexture(this ResourceDictionary resourceDictionary, string textureFileName)
        {
            return resourceDictionary.AddResource(new StandardTextureResource(textureFileName));
        }

        /// <summary>
        /// Adds a new texture resource pointing to the given texture file name.
        /// </summary>
        internal static StandardTextureResource AddTexture(this ResourceDictionary resourceDictionary, NamedOrGenericKey resourceKey, string textureFileName)
        {
            return resourceDictionary.AddResource(resourceKey, new StandardTextureResource(textureFileName));
        }
    }
}
