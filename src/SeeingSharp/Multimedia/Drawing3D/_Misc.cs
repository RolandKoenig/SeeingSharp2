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
using System;

namespace SeeingSharp.Multimedia.Drawing3D
{
    [Flags]
    internal enum RenderTargetCreationMode
    {
        /// <summary>
        /// Do create the color buffer.
        /// </summary>
        Color = 1,

        /// <summary>
        /// Do create the depth buffer.
        /// </summary>
        Depth = 2,

        /// <summary>
        /// Do create the object-id buffer.
        /// </summary>
        ObjectId = 4,

        /// <summary>
        /// Do create the normal-depth buffer.
        /// </summary>
        NormalDepth = 8,

        /// <summary>
        /// Create all textures.
        /// </summary>
        All = Color | Depth | ObjectId | NormalDepth
    }

    /// <summary>
    /// Describes the type of a 3D camera.
    /// </summary>
    public enum Camera3DType
    {
        Perspective,

        Orthographic
    }

    /// <summary>
    /// Describes the alpha blending mode of the texture painter.
    /// </summary>
    public enum TexturePainterAlphaBlendMode
    {
        /// <summary>
        /// Use alpha blending (draw pixels transparent).
        /// </summary>
        AlphaBlend,

        /// <summary>
        /// Ignore the alpha value.
        /// </summary>
        NoAlphaBlend
    }

    /// <summary>
    /// Describes the mode how a RenderTargetTexture pushes itself on the rendering stack.
    /// </summary>
    [Flags]
    internal enum PushRenderTargetMode
    {
        /// <summary>
        /// Use all buffers from this RenderTargetTexture.
        /// </summary>
        Default_OwnAll = UseOwnColorBuffer | UseOwnDepthBuffer | UseOwnObjectIdBuffer | UseOwnNormalDepthBuffer,

        Default_OwnColorDepth_PrevObjectIdNormalDepth = UseOwnColorBuffer | UseOwnDepthBuffer | OvertakeObjectIdBuffer | OvertakeNormalDepthBuffer,

        Default_OwnColor_PrevDepthObjectIdNormalDepth = UseOwnColorBuffer | OvertakeDepthBuffer | OvertakeObjectIdBuffer | OvertakeNormalDepthBuffer,

        Default_OwnColorNormalDepth_PrevDepthObjectId = UseOwnColorBuffer | OvertakeDepthBuffer | OvertakeObjectIdBuffer | UseOwnNormalDepthBuffer,

        /// <summary>
        /// Use the color buffer defined by this render-target texture.
        /// </summary>
        UseOwnColorBuffer = 1,

        UseOwnDepthBuffer = 2,

        UseOwnObjectIdBuffer = 4,

        UseOwnNormalDepthBuffer = 8,

        OvertakeColorBuffer = 16,

        OvertakeDepthBuffer = 32,

        OvertakeObjectIdBuffer = 64,

        OvertakeNormalDepthBuffer = 128
    }
}
