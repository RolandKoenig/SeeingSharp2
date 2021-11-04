using System.Numerics;
using System.Runtime.InteropServices;

namespace SeeingSharp.Drawing3D
{
    // Structures representing the content of ConstantBuffers on graphics hardware
    // See hlsl file "_constants.hlsl"

    // See following link for packaging rules on hlsl side
    // https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-packing-rules

    /// <summary>
    /// This buffer contains all settings which are set once per frame.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct CBPerFrame
    {
        public int Time;
        public Vector3 Dummy1;
    }

    /// <summary>
    /// This buffer contains all settings which are set once per view.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct CBPerView
    {
        public Matrix4x4 ViewProj;
        public Matrix4x4 View;
        public Vector3 CameraPosition;
        public float BorderFactor;
        public float LightPower;
        public float StrongLightFactor;
        public float Ambient;
        public float Accentuation;
        public Vector2 ScreenPixelSize;
        public Vector2 Dummy2;
    }

    /// <summary>
    /// This buffer contains all settings which are set once per object.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct CBPerObject
    {
        public Matrix4x4 World;
        public Vector4 Color;
        public float Opacity;
        public float AccentuationFactor;

        /// <summary>
        /// Only relevant for sprite rendering. Scales coordinates in VertexShader by
        /// this factor.
        /// </summary>
        public float SpriteScaling;

        public float Dummy3;

        /// <summary>
        /// Needed to rescale texture coordinate depending on the object's scaling factors.
        /// </summary>
        public Vector3 ObjectScaling;

        public float Dummy4;
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
        /// Up to an alpha value defined by this clip factor pixels are clipped within the pixel shader.
        /// </summary>
        public float ClipFactor;

        public float AdjustTextureCoordinates;

        /// <summary>
        /// Needed for video rendering (Frames from the MF SourceReader have alpha always to zero)
        /// </summary>
        public float AddToAlpha;

        public float FadeIntensity;

        public float DiffuseColorFactor;

        public float BorderPart;

        public float BorderMultiplier;

        public Vector3 Dummy5;
    }
}
