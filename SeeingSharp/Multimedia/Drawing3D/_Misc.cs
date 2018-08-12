#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.Multimedia.Drawing3D
{
    [Flags]
    internal enum RenderTargetCreationMode : int
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
        ObjectID = 4,

        /// <summary>
        /// Do create the normal-depth buffer.
        /// </summary>
        NormalDepth = 8,

        /// <summary>
        /// Create all textures.
        /// </summary>
        All = Color | Depth | ObjectID | NormalDepth
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
    internal enum PushRenderTargetMode : int
    {
        /// <summary>
        /// Use all buffers from this RenderTargetTexture.
        /// </summary>
        Default_OwnAll = UseOwnColorBuffer | UseOwnDepthBuffer | UseOwnObjectIDBuffer | UseOwnNormalDepthBuffer,

        Default_OwnColorDepth_PrevObjectIDNormalDepth = UseOwnColorBuffer | UseOwnDepthBuffer | OvertakeObjectIDBuffer | OvertakeNormalDepthBuffer,

        Default_OwnColor_PrevDepthObjectIDNormalDepth = UseOwnColorBuffer | OvertakeDepthBuffer | OvertakeObjectIDBuffer | OvertakeNormalDepthBuffer,

        Default_OwnColorNormalDepth_PrevDepthObjectID = UseOwnColorBuffer | OvertakeDepthBuffer | OvertakeObjectIDBuffer | UseOwnNormalDepthBuffer,

        /// <summary>
        /// Use the color buffer defined by this render-target texture.
        /// </summary>
        UseOwnColorBuffer = 1,

        UseOwnDepthBuffer = 2,

        UseOwnObjectIDBuffer = 4,

        UseOwnNormalDepthBuffer = 8,

        OvertakeColorBuffer = 16,

        OvertakeDepthBuffer = 32,

        OvertakeObjectIDBuffer = 64,

        OvertakeNormalDepthBuffer = 128
    }
}
