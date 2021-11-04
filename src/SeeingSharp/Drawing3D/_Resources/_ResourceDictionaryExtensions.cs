using SeeingSharp.Core;
using SeeingSharp.Util;

namespace SeeingSharp.Drawing3D
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
