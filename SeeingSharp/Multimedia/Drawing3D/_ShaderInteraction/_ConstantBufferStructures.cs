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
using System.Numerics;
using System.Runtime.InteropServices;

namespace SeeingSharp.Multimedia.Drawing3D
{
    // Structures representing the content of ConstantBuffers on graphics hardware
    // See hlsl file "_constants.hlsl"

    /// <summary>
    /// This buffer contains all settings which are set once per frame.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct CBPerFrame
    {
        public int Time;
        public Vector3 Dummy;
    }

    /// <summary>
    /// This buffer contains all settings which are set once per view.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct CBPerView
    {
        public Matrix4x4 ViewProj;
        public Vector3 CameraPosition;
        public float BorderFactor;
        public float LightPower;
        public float StrongLightFactor;
        public float Ambient;
        public float Accentuation;
        public Vector2 ScreenPixelSize;
        public Vector2 Dummy;
    }

    /// <summary>
    /// This buffer contains all settings which are set once per object.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct CBPerObject
    {
        public Matrix4x4 World;
        public Vector4 Color;
        public float BorderPart;
        public float BorderMultiplier;
        public float Opacity;
        public float AccentuationFactor;

        /// <summary>
        /// Needed to rescale texture coordinate depending on the object's scaling factors.
        /// </summary>
        public Vector3 ObjectScaling;

        /// <summary>
        /// Only relevant for sprite rendering. Scales coordinates in VertexShader by
        /// this factor.
        /// </summary>
        public float SpriteScaling;
    }

    /// <summary>
    /// This buffer contains all settings which are set once per material.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct CBPerMaterial
    {
        /// <summary>
        /// A material color that can be processed by the shader.
        /// (e. g. ForcedColor shader)
        /// </summary>
        public Vector4 MaterialDiffuseColor;

        /// <summary>
        /// The reciprocal as described in FXAA white paper (see NVidia SDK 11, FXAA sample).
        /// Defined by { 1.0f/inputTextureWidth, 1.0f/inputTextureHeight, 0.0f, 0.0f }
        /// </summary>
        public Vector4 TextureReciprocal;

        /// <summary>
        /// This flag is needed to tell the shader whether a texture has been set or not.
        ///  see http://stackoverflow.com/questions/11297123/how-can-i-check-existence-of-texture-in-shader-programming
        /// </summary>
        public float Texture0Factor;

        /// <summary>
        /// Max distance form pixel to camera on which to apply the ClipFactor property.
        /// </summary>
        public float MaxClipDistance;

        /// <summary>
        /// Up to an alpha value defined by this Clipfactor pixels are clipped within the pixel shader.
        /// </summary>
        public float ClipFactor;

        public float AdjustTextureCoordinates;

        /// <summary>
        /// Needed for video rendering (Frames from the MF SourceReader have alpha always to zero)
        /// </summary>
        public float AddToAlpha;

        public float FadeIntensity;

        public float DiffuseColorFactor;

        public float Dummy;
    }
}
