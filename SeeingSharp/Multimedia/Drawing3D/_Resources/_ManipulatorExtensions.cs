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
namespace SeeingSharp.Multimedia.Drawing3D
{
    #region using

    using Core;
    using Objects;
    using SeeingSharp.Util;

    #endregion

    public static class SceneManipulatorExtensions
    {
        /// <summary>
        /// Adds a new texture resource to the scene.
        /// </summary>
        /// <param name="sceneManipulator">The manipulator of the scene.</param>
        /// <param name="textureSource">The source of the texture.</param>
        public static NamedOrGenericKey AddTexture(this SceneManipulator sceneManipulator, ResourceLink textureSource)
        {
            return sceneManipulator.AddResource<StandardTextureResource>(() => new StandardTextureResource(textureSource));
        }

        /// <summary>
        /// Adds a new texture resource to the scene.
        /// </summary>
        /// <param name="sceneManipulator">The manipulator of the scene.</param>
        /// <param name="textureSourceHighQuality">The source of the texture in high quality.</param>
        /// <param name="textureSourceLowQuality">The texture in low quality.</param>
        public static NamedOrGenericKey AddTexture(
            this SceneManipulator sceneManipulator,
            ResourceLink textureSourceHighQuality,
            ResourceLink textureSourceLowQuality)
        {
            return sceneManipulator.AddResource<StandardTextureResource>(() => new StandardTextureResource(textureSourceHighQuality, textureSourceLowQuality));
        }

        /// <summary>
        /// Adds a new simple colored material resource to the scene.
        /// </summary>
        /// <param name="sceneManipulator">The manipulator of the scene.</param>
        public static NamedOrGenericKey AddSimpleColoredMaterial(this SceneManipulator sceneManipulator)
        {
            return sceneManipulator.AddResource<SimpleColoredMaterialResource>(() => new SimpleColoredMaterialResource());
        }

        /// <summary>
        /// Adds a new simple colored material resource to the scene.
        /// </summary>
        /// <param name="sceneManipulator">The manipulator of the scene.</param>
        /// <param name="textureKey">The resource key of the texture to be used.</param>
        /// <param name="clipFactor">Pixel are clipped up to an alpha value defined by this Clipfactor within the pixel shader.</param>
        /// <param name="maxClipDistance">The maximum distance on which to apply pixel clipping (defined by ClipFactor property).</param>
        /// <param name="adjustTextureCoordinates">Interpolate texture coordinate based on xy-scaling.</param>
        /// <param name="addToAlpha">Needed for video rendering (Frames from the MF SourceReader have alpha always to zero).</param>
        public static NamedOrGenericKey AddSimpleColoredMaterial(
            this SceneManipulator sceneManipulator, NamedOrGenericKey textureKey,
            float clipFactor = 0f,
            float maxClipDistance = 1000f,
            bool adjustTextureCoordinates = false,
            float addToAlpha = 0f)
        {
            return sceneManipulator.AddResource<SimpleColoredMaterialResource>(
                () => new SimpleColoredMaterialResource(textureKey)
                {
                    AdjustTextureCoordinates = adjustTextureCoordinates,
                    MaxClipDistance = maxClipDistance,
                    ClipFactor = clipFactor,
                    AddToAlpha = addToAlpha
                });
        }

        /// <summary>
        /// Adds a new simple colored material resource to the scene.
        /// </summary>
        /// <param name="sceneManipulator">The manipulator of the scene.</param>
        /// <param name="textureSource">The source of the texture which should be loaded.</param>
        public static NamedOrGenericKey AddSimpleColoredMaterial(this SceneManipulator sceneManipulator, ResourceLink textureSource)
        {
            var resTexture = sceneManipulator.AddTexture(textureSource);
            return sceneManipulator.AddResource<SimpleColoredMaterialResource>(() => new SimpleColoredMaterialResource(resTexture));
        }

        /// <summary>
        /// Adds a new simple colored material resource to the scene.
        /// </summary>
        /// <param name="sceneManipulator">The manipulator of the scene.</param>
        /// <param name="textureSourceHighQuality">The source of the texture which should be loaded.</param>
        /// <param name="textureSourceLowQuality">The source of the texture with low quality.</param>
        public static NamedOrGenericKey AddSimpleColoredMaterial(
            this SceneManipulator sceneManipulator,
            ResourceLink textureSourceHighQuality, ResourceLink textureSourceLowQuality)
        {
            var resTexture = sceneManipulator.AddTexture(textureSourceHighQuality, textureSourceLowQuality);
            return sceneManipulator.AddResource<SimpleColoredMaterialResource>(() => new SimpleColoredMaterialResource(resTexture));
        }

        /// <summary>
        /// Adds a new geometry resource to the scene.
        /// </summary>
        /// <param name="sceneManipulator">The manipulator of the scene.</param>
        /// <param name="vertexStructure">The structures which define the geometry.</param>
        public static NamedOrGenericKey AddGeometry(this SceneManipulator sceneManipulator, VertexStructure vertexStructure)
        {
            return sceneManipulator.AddResource<GeometryResource>(() => new GeometryResource(vertexStructure));
        }

        /// <summary>
        /// Adds a new geometry resource to the scene.
        /// </summary>
        /// <param name="sceneManipulator">The manipulator of the scene.</param>
        /// <param name="objectType">The geometry to be loaded.</param>
        public static NamedOrGenericKey AddGeometry(this SceneManipulator sceneManipulator, GeometryFactory objectType)
        {
            return sceneManipulator.AddResource<GeometryResource>(() => new GeometryResource(objectType));
        }
    }
}
