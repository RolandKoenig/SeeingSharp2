﻿using SeeingSharp.Core;
using SeeingSharp.Drawing3D.Geometries;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.Drawing3D.Resources
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
            return sceneManipulator.AddResource(_ => new StandardTextureResource(textureSource));
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
            return sceneManipulator.AddResource(_ => new StandardTextureResource(textureSourceHighQuality, textureSourceLowQuality));
        }

        /// <summary>
        /// Adds a new simple colored material resource to the scene.
        /// </summary>
        /// <param name="sceneManipulator">The manipulator of the scene.</param>
        public static NamedOrGenericKey AddStandardMaterialResource(this SceneManipulator sceneManipulator)
        {
            return sceneManipulator.AddResource(_ => new StandardMaterialResource());
        }

        /// <summary>
        /// Adds a new simple colored material resource to the scene.
        /// </summary>
        /// <param name="sceneManipulator">The manipulator of the scene.</param>
        /// <param name="textureKey">The resource key of the texture to be used.</param>
        /// <param name="clipFactor">Pixel are clipped up to an alpha value defined by this clipping factor within the pixel shader.</param>
        /// <param name="maxClipDistance">The maximum distance on which to apply pixel clipping (defined by ClipFactor property).</param>
        /// <param name="adjustTextureCoordinates">Interpolate texture coordinate based on xy-scaling.</param>
        /// <param name="addToAlpha">Needed for video rendering (Frames from the MF SourceReader have alpha always to zero).</param>
        /// <param name="materialDiffuseColor">The fixed diffuse color for this material.</param>
        /// <param name="useVertexColors">Set this to false to use the material's diffuse color.</param>
        /// <param name="enableShaderGeneratedBorder">Enable drawing of borders which are generated by the pixel shader?</param>
        public static NamedOrGenericKey AddStandardMaterialResource(
            this SceneManipulator sceneManipulator,
            NamedOrGenericKey textureKey = default,
            float clipFactor = 0f,
            float maxClipDistance = 1000f,
            bool adjustTextureCoordinates = false,
            float addToAlpha = 0f,
            Color4 materialDiffuseColor = default,
            bool useVertexColors = true,
            bool enableShaderGeneratedBorder = false)
        {
            return sceneManipulator.AddResource(
                _ => new StandardMaterialResource(textureKey, enableShaderGeneratedBorder)
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
            return sceneManipulator.AddResource(_ => new StandardMaterialResource(resTexture));
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
            return sceneManipulator.AddResource(_ => new StandardMaterialResource(resTexture));
        }

        /// <summary>
        /// Adds a new geometry resource to the scene.
        /// </summary>
        /// <param name="sceneManipulator">The manipulator of the scene.</param>
        /// <param name="geometry">The geometry.</param>
        public static NamedOrGenericKey AddGeometryResource(this SceneManipulator sceneManipulator, Geometry geometry)
        {
            return sceneManipulator.AddResource(_ => new GeometryResource(geometry));
        }

        /// <summary>
        /// Adds a new geometry resource to the scene.
        /// </summary>
        /// <param name="sceneManipulator">The manipulator of the scene.</param>
        /// <param name="objectType">The geometry to be loaded.</param>
        public static NamedOrGenericKey AddGeometryResource(this SceneManipulator sceneManipulator, GeometryFactory objectType)
        {
            return sceneManipulator.AddResource(_ => new GeometryResource(objectType));
        }
    }
}