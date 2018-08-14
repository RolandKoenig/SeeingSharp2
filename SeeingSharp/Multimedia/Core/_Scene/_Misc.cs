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

namespace SeeingSharp.Multimedia.Core
{
    public delegate void Rendering3DHandler(object sender, Rendering3DArgs e);
    public delegate void Updating3DHandler(object sender, Updating3DArgs e);

    /// <summary>
    /// The transformation type used within a scene object node.
    /// </summary>
    public enum SpacialTransformationType
    {
        /// <summary>
        /// Scaling, translation and Rotation using euler angles (pitch, yaw and roll).
        /// </summary>
        ScalingTranslationEulerAngles,

        /// <summary>
        /// Scaling, translation and Rotation using quaternion.
        /// </summary>
        ScalingTranslationQuaternion,

        /// <summary>
        /// Scaling, translation and rotation based on direction vectors (forward, up)
        /// </summary>
        ScalingTranslationDirection,

        /// <summary>
        /// Scaling and translation components.
        /// </summary>
        ScalingTranslation,

        /// <summary>
        /// Translation and Rotation using euler angles (pitch, yaw and roll).
        /// </summary>
        TranslationEulerAngles,

        /// <summary>
        /// Tranlation and Rotation using quaternion.
        /// </summary>
        TranslationQuaternion,

        /// <summary>
        /// Translation and rotation using direction vectors (forward, up)
        /// </summary>
        TranslationDirection,

        /// <summary>
        /// Just translation transformation
        /// </summary>
        Translation,
        
        /// <summary>
        /// A custom transformation matrix.
        /// </summary>
        CustomTransform,

        /// <summary>
        /// The object should take its transform from another object.
        /// </summary>
        TakeFromOtherObject,

        /// <summary>
        /// No transformation at all
        /// </summary>
        None
    }

    /// <summary>
    /// The instancing mode implemented by a material.
    /// </summary>
    internal enum MaterialApplyInstancingMode
    {
        /// <summary>
        /// Shaders for rendering a single object.
        /// </summary>
        SingleObject,

        /// <summary>
        /// Shaders for rendering multi objects using standard instancing technique.
        /// </summary>
        StandardInstancing
    }

    /// <summary>
    /// Defines the host-mode remote object hosting.
    /// </summary>
    public enum ObjectHostMode
    {
        /// <summary>
        /// Default mode - overtake all possible values.
        /// </summary>
        Default,

        /// <summary>
        /// Take all values, but don't take rotation value.
        /// </summary>
        IgnoreRotation,

        /// <summary>
        /// Take all values, but don't take size value.
        /// </summary>
        IgnoreScaling,

        /// <summary>
        /// Take all values, but don't take size and rotation value.
        /// </summary>
        IgnoreRotationScaling
    }

    /// <summary>
    /// EventArgs class for Rendering3DHandler.
    /// </summary>
    public class Rendering3DArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rendering3DArgs"/> class.
        /// </summary>
        /// <param name="renderState">Current render state.</param>
        public Rendering3DArgs(RenderState renderState)
        {
            this.RenderState = renderState;
        }

        /// <summary>
        /// Gets the render state.
        /// </summary>
        /// <value>Gets the render state.</value>
        public RenderState RenderState
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// EventArgs class for Updating3DHandler.
    /// </summary>
    public class Updating3DArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Updating3DArgs"/> class.
        /// </summary>
        public Updating3DArgs(UpdateState updateState)
        {
            this.UpdateState = updateState;
        }

        /// <summary>
        /// Gets or sets the update state.
        /// </summary>
        public UpdateState UpdateState
        {
            get;
            private set;
        }
    }
}