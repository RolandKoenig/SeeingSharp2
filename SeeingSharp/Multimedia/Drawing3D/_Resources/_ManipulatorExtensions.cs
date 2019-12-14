﻿/*
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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public static class SceneManipulatorExtensions
    {
        /// <summary>
        /// Adds a new texture resource to the scene.
        /// </summary>
        /// <param name="sceneManipulator">The manipulator of the scene.</param>
        /// <param name="textureSource">The source of the texture.</param>
        public static NamedOrGenericKey AddTextureResource(this SceneManipulator sceneManipulator, ResourceLink textureSource)
        {
            return sceneManipulator.AddResource(device => new StandardTextureResource(textureSource));
        }

        /// <summary>
        /// Adds a new texture resource to the scene.
        /// </summary>
        /// <param name="sceneManipulator">The manipulator of the scene.</param>
        /// <param name="textureSourceHighQuality">The source of the texture in high quality.</param>
        /// <param name="textureSourceLowQuality">The texture in low quality.</param>
        public static NamedOrGenericKey AddTextureResource(
            this SceneManipulator sceneManipulator,
            ResourceLink textureSourceHighQuality,
            ResourceLink textureSourceLowQuality)
        {
            return sceneManipulator.AddResource(device => new StandardTextureResource(textureSourceHighQuality, textureSourceLowQuality));
        }

        /// <summary>
        /// Adds a new simple colored material resource to the scene.
        /// </summary>
        /// <param name="sceneManipulator">The manipulator of the scene.</param>
        public static NamedOrGenericKey AddStandardMaterialResource(this SceneManipulator sceneManipulator)
        {
            return sceneManipulator.AddResource(device => new StandardMaterialResource());
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
        /// <param name="materialDiffuseColor">The fixed diffuse color for this material.</param>
        /// <param name="useVertexColors">Set this to false to use the material's diffuse color.</param>
        public static NamedOrGenericKey AddStandardMaterialResource(
            this SceneManipulator sceneManipulator, 
            NamedOrGenericKey textureKey = default(NamedOrGenericKey),
            float clipFactor = 0f,
            float maxClipDistance = 1000f,
            bool adjustTextureCoordinates = false,
            float addToAlpha = 0f,
            Color4 materialDiffuseColor = default(Color4),  
            bool useVertexColors = true)
        {
            return sceneManipulator.AddResource(
                device => new StandardMaterialResource(textureKey)
                {
                    AdjustTextureCoordinates = adjustTextureCoordinates,
                    MaxClipDistance = maxClipDistance,
                    ClipFactor = clipFactor,
                    AddToAlpha = addToAlpha,
                    MaterialDiffuseColor = materialDiffuseColor,
                    UseVertexColors = useVertexColors
                });
        }

        /// <summary>
        /// Adds a new simple colored material resource to the scene.
        /// </summary>
        /// <param name="sceneManipulator">The manipulator of the scene.</param>
        /// <param name="textureSource">The source of the texture which should be loaded.</param>
        public static NamedOrGenericKey AddStandardMaterialResource(this SceneManipulator sceneManipulator, ResourceLink textureSource)
        {
            var resTexture = sceneManipulator.AddTextureResource(textureSource);
            return sceneManipulator.AddResource(device => new StandardMaterialResource(resTexture));
        }

        /// <summary>
        /// Adds a new simple colored material resource to the scene.
        /// </summary>
        /// <param name="sceneManipulator">The manipulator of the scene.</param>
        /// <param name="textureSourceHighQuality">The source of the texture which should be loaded.</param>
        /// <param name="textureSourceLowQuality">The source of the texture with low quality.</param>
        public static NamedOrGenericKey AddStandardMaterialResource(
            this SceneManipulator sceneManipulator,
            ResourceLink textureSourceHighQuality, ResourceLink textureSourceLowQuality)
        {
            var resTexture = sceneManipulator.AddTextureResource(textureSourceHighQuality, textureSourceLowQuality);
            return sceneManipulator.AddResource(device => new StandardMaterialResource(resTexture));
        }

        /// <summary>
        /// Adds a new geometry resource to the scene.
        /// </summary>
        /// <param name="sceneManipulator">The manipulator of the scene.</param>
        /// <param name="geometry">The geometry.</param>
        public static NamedOrGenericKey AddGeometryResource(this SceneManipulator sceneManipulator, Geometry geometry)
        {
            return sceneManipulator.AddResource(device => new GeometryResource(geometry));
        }

        /// <summary>
        /// Adds a new geometry resource to the scene.
        /// </summary>
        /// <param name="sceneManipulator">The manipulator of the scene.</param>
        /// <param name="objectType">The geometry to be loaded.</param>
        public static NamedOrGenericKey AddGeometryResource(this SceneManipulator sceneManipulator, GeometryFactory objectType)
        {
            return sceneManipulator.AddResource(device => new GeometryResource(objectType));
        }
    }
}
