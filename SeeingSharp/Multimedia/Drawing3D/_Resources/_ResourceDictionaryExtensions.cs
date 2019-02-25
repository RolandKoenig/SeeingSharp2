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

namespace SeeingSharp.Multimedia.Drawing3D
{
    #region using

    using Core;
    using Objects;
    using SeeingSharp.Util;

    #endregion

    internal static class ResourceDictionaryExtensions
    {
        /// <summary>
        /// Gets or creates the material resource for the given VertexStructure object.
        /// </summary>
        internal static MaterialResource GetOrCreateMaterialResourceAndEnsureLoaded(this ResourceDictionary resourceDict, VertexStructureSurface targetSurface)
        {
            MaterialResource materialResource = GetOrCreateMaterialResource(resourceDict, targetSurface);
            if (!materialResource.IsLoaded)
            {
                materialResource.LoadResource();
            }

            return materialResource;
        }

        /// <summary>
        /// Gets or creates the material resource for the given VertexStructure object.
        /// </summary>
        internal static MaterialResource GetOrCreateMaterialResource(this ResourceDictionary resourceDict, VertexStructureSurface targetSurface)
        {
            NamedOrGenericKey materialKey = targetSurface.Material;
            NamedOrGenericKey textureKey = targetSurface.TextureKey;

            // Get the material if it is already created
            if ((!materialKey.IsEmpty) && (resourceDict.ContainsResource(materialKey))) { return resourceDict.GetResource<MaterialResource>(materialKey); }

            // Generate a dynamic material key
            if (materialKey.IsEmpty)
            {
                materialKey = new NamedOrGenericKey(targetSurface.MaterialProperties.GetDynamicResourceKey());
            }

            // Get the material if it is already created
            if (resourceDict.ContainsResource(materialKey)) { return resourceDict.GetResource<MaterialResource>(materialKey); }

            if(textureKey.IsEmpty)
            {
                // Create a default material without any texture
                SimpleColoredMaterialResource result = resourceDict.AddResource<SimpleColoredMaterialResource>(materialKey, new SimpleColoredMaterialResource());
                result.MaterialDiffuseColor = targetSurface.MaterialProperties.DiffuseColor;
                return result;
            }
            else
            {
                // Create texture resource if needed
                try
                {
                    if ((!resourceDict.ContainsResource(textureKey)) &&
                       (!string.IsNullOrEmpty(textureKey.NameKey)))
                    {
                        // Try to find and create the texture resource by its name
                        if (targetSurface.ResourceLink != null)
                        {
                            var textureResourceLink = targetSurface.ResourceLink.GetForAnotherFile(textureKey.NameKey);
                           
                            resourceDict.AddResource<StandardTextureResource>(
                                textureKey,
                                new StandardTextureResource(
                                    targetSurface.ResourceLink.GetForAnotherFile(textureKey.NameKey)));
                        }
                        else if (targetSurface.ResourceSourceAssembly != null)
                        {
                            var textureResourceLink = new AssemblyResourceLink(
                                targetSurface.ResourceSourceAssembly,
                                targetSurface.ResourceSourceAssembly.GetName().Name + ".Resources.Textures",
                                textureKey.NameKey);
                            if (textureResourceLink.IsValid())
                            {
                                resourceDict.AddResource<StandardTextureResource>(
                                    textureKey,
                                    new StandardTextureResource(textureResourceLink));
                            }
                            else
                            {
                                // Unable to resolve texture
                                textureKey = NamedOrGenericKey.Empty;
                            }
                        }
                        else
                        {
                            // Unable to resolve texture
                            textureKey = NamedOrGenericKey.Empty;
                        }
                    }
                }
                catch { }

                // Create a default textured material 
                if (!textureKey.IsEmpty)
                {
                    SimpleColoredMaterialResource result = resourceDict.AddResource<SimpleColoredMaterialResource>(
                        materialKey, 
                        new SimpleColoredMaterialResource(textureKey));
                    result.MaterialDiffuseColor = targetSurface.MaterialProperties.DiffuseColor;
                    return result;
                }
                else
                {
                    SimpleColoredMaterialResource result = resourceDict.AddResource<SimpleColoredMaterialResource>(
                        materialKey,
                        new SimpleColoredMaterialResource());
                    result.MaterialDiffuseColor = targetSurface.MaterialProperties.DiffuseColor;
                    return result;
                }
            }
        }

        /// <summary>
        /// Adds a new geometry resource.
        /// </summary>
        internal static GeometryResource AddGeometry(this ResourceDictionary resourceDiciontary, VertexStructure structure)
        {
            return resourceDiciontary.AddResource(new GeometryResource(structure));
        }

        /// <summary>
        /// Adds a new geometry resource.
        /// </summary>
        internal static GeometryResource AddGeometry(this ResourceDictionary resourceDiciontary, NamedOrGenericKey resourceKey, VertexStructure structure)
        {
            return resourceDiciontary.AddResource(resourceKey, new GeometryResource(structure));
        }

        /// <summary>
        /// Adds a new geometry resource.
        /// </summary>
        internal static GeometryResource AddGeometry(this ResourceDictionary resourceDiciontary, ObjectType objectType)
        {
            return resourceDiciontary.AddResource(new GeometryResource(objectType));
        }

        /// <summary>
        /// Adds a new geometry resource.
        /// </summary>
        internal static GeometryResource AddGeometry(this ResourceDictionary resourceDiciontary, NamedOrGenericKey resourceKey, ObjectType objectType)
        {
            return resourceDiciontary.AddResource(resourceKey, new GeometryResource(objectType));
        }

        /// <summary>
        /// Adds a new text geometry with the given text.
        /// </summary>
        internal static GeometryResource AddTextGeometry(this ResourceDictionary resourceDiciontary, string textToAdd)
        {
            return resourceDiciontary.AddTextGeometry(textToAdd, TextGeometryOptions.Default);
        }

        /// <summary>
        /// Adds a new text geometry with the given text.
        /// </summary>
        internal static GeometryResource AddTextGeometry(this ResourceDictionary resourceDiciontary, NamedOrGenericKey resourceKey, string textToAdd)
        {
            return resourceDiciontary.AddTextGeometry(resourceKey, textToAdd, TextGeometryOptions.Default);
        }

        /// <summary>
        /// Adds a new text geometry with the given text.
        /// </summary>
        internal static GeometryResource AddTextGeometry(this ResourceDictionary resourceDiciontary, string textToAdd, TextGeometryOptions textGeometryOptions)
        {
            VertexStructure newStructure = new VertexStructure();
            newStructure.FirstSurface.BuildTextGeometry(textToAdd, textGeometryOptions);
            newStructure.FirstSurface.Material = textGeometryOptions.SurfaceMaterial;
            return resourceDiciontary.AddGeometry(newStructure);
        }

        /// <summary>
        /// Adds a new text geometry with the given text.
        /// </summary>
        internal static GeometryResource AddTextGeometry(this ResourceDictionary resourceDiciontary, NamedOrGenericKey resourceKey, string textToAdd, TextGeometryOptions textGeometryOptions)
        {
            VertexStructure newStructure = new VertexStructure();
            newStructure.FirstSurface.BuildTextGeometry(textToAdd, textGeometryOptions);
            newStructure.FirstSurface.Material = textGeometryOptions.SurfaceMaterial;
            return resourceDiciontary.AddGeometry(resourceKey, newStructure);
        }

        /// <summary>
        /// Adds a new texture resource pointing to the given texture file name.
        /// </summary>
        internal static StandardTextureResource AddTexture(this ResourceDictionary resourceDiciontary, string textureFileName)
        {
            return resourceDiciontary.AddResource(new StandardTextureResource(textureFileName));
        }

        /// <summary>
        /// Adds a new texture resource pointing to the given texture file name.
        /// </summary>
        internal static StandardTextureResource AddTexture(this ResourceDictionary resourceDiciontary, NamedOrGenericKey resourceKey, string textureFileName)
        {
            return resourceDiciontary.AddResource(resourceKey, new StandardTextureResource(textureFileName));
        }
    }
}
