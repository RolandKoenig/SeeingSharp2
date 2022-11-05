using System;
using SeeingSharp.Core;

namespace SeeingSharp.Drawing3D.Resources
{
    public enum ShaderResourceKind
    {
        /// <summary>
        /// The resource is a text based hlsl file.
        /// </summary>
        HlsFile,

        /// <summary>
        /// The resource is pure bytecode.
        /// </summary>
        Bytecode
    }

    public enum GradientDirection
    {
        LeftToRight,

        TopToBottom,

        Directional
    }

    /// <summary>
    /// Identify a technique for resolving texture coordinates that are outside of the boundaries of a texture.
    /// </summary>
    public enum SeeingSharpTextureAddressMode
    {
        /// <summary>
        /// Tile the texture at every (u,v) integer junction. For example, for u values between 0 and 3, the texture is repeated three times.
        /// </summary>
        Wrap = 1,

        /// <summary>
        /// Flip the texture at every (u,v) integer junction. For u values between 0 and 1, for example, the texture is addressed normally;
        /// between 1 and 2, the texture is flipped (mirrored); between 2 and 3, the texture is normal again; and so on.
        /// </summary>
        Mirror = 2,

        /// <summary>
        /// Texture coordinates outside the range [0.0, 1.0] are set to the texture color at 0.0 or 1.0, respectively.
        /// </summary>
        Clamp = 3,

        /// <summary>
        /// Texture coordinates outside the range [0.0, 1.0] are set to the border color specified in <see cref="TextureResource"/>.
        /// </summary>
        Border = 4,

        /// <summary>
        /// Similar to Mirror and Clamp.
        /// Takes the absolute value of the texture coordinate (thus, mirroring around 0), and then clamps to the maximum value.
        /// </summary>
        MirrorOnce = 5
    }

    /// <summary>
    /// Filtering options during texture sampling.
    /// </summary>
    public enum SeeingSharpFilter
    {
        /// <summary>
        /// Use point sampling for minification, magnification, and mip-level sampling.
        /// </summary>
        MinMagMipPoint = 0,

        /// <summary>
        /// Use point sampling for minification and magnification; use linear interpolation for mip-level sampling.
        /// </summary>
        MinMagPointMipLinear = 1,

        /// <summary>
        /// Use point sampling for minification; use linear interpolation for magnification; use point sampling for mip-level sampling.
        /// </summary>
        MinPointMagLinearMipPoint = 4,
        
        /// <summary>
        /// Use point sampling for minification; use linear interpolation for magnification and mip-level sampling.
        /// </summary>
        MinPointMagMipLinear = 5,

        /// <summary>
        /// Use linear interpolation for minification; use point sampling for magnification and mip-level sampling.
        /// </summary>
        MinLinearMagMipPoint = 16, // 0x00000010
        
        /// <summary>
        /// Use linear interpolation for minification; use point sampling for magnification; use linear interpolation for mip-level sampling.
        /// </summary>
        MinLinearMagPointMipLinear = 17, // 0x00000011
        
        /// <summary>
        /// Use linear interpolation for minification and magnification; use point sampling for mip-level sampling.
        /// </summary>
        MinMagLinearMipPoint = 20, // 0x00000014
        
        /// <summary>
        /// Use linear interpolation for minification, magnification, and mip-level sampling.
        /// </summary>
        MinMagMipLinear = 21, // 0x00000015
        
        /// <summary>
        /// Use anisotropic interpolation for minification, magnification, and mip-level sampling.
        /// </summary>
        Anisotropic = 85, // 0x00000055
        
        /// <summary>
        /// Use point sampling for minification, magnification, and mip-level sampling.
        /// Compare the result to the comparison value.
        /// </summary>
        ComparisonMinMagMipPoint = 128, // 0x00000080
        
        /// <summary>
        /// Use point sampling for minification and magnification;
        /// use linear interpolation for mip-level sampling. Compare the result to the comparison value.
        /// </summary>
        ComparisonMinMagPointMipLinear = 129, // 0x00000081
        
        /// <summary>
        /// Use point sampling for minification; use linear interpolation for magnification;
        /// use point sampling for mip-level sampling. Compare the result to the comparison value.
        /// </summary>
        ComparisonMinPointMagLinearMipPoint = 132, // 0x00000084

        /// <summary>
        /// Use point sampling for minification; use linear interpolation for magnification and mip-level sampling.
        /// Compare the result to the comparison value.
        /// </summary>
        ComparisonMinPointMagMipLinear = 133, // 0x00000085

        /// <summary>
        /// Use linear interpolation for minification; use point sampling for magnification and mip-level sampling.
        /// Compare the result to the comparison value.
        /// </summary>
        ComparisonMinLinearMagMipPoint = 144, // 0x00000090

        /// <summary>
        /// Use linear interpolation for minification; use point sampling for magnification; use linear interpolation for mip-level sampling.
        /// Compare the result to the comparison value.
        /// </summary>
        ComparisonMinLinearMagPointMipLinear = 145, // 0x00000091

        /// <summary>
        /// Use linear interpolation for minification and magnification; use point sampling for mip-level sampling.
        /// Compare the result to the comparison value.
        /// </summary>
        ComparisonMinMagLinearMipPoint = 148, // 0x00000094
        
        /// <summary>
        /// Use linear interpolation for minification, magnification, and mip-level sampling.
        /// Compare the result to the comparison value.
        /// </summary>
        ComparisonMinMagMipLinear = 149, // 0x00000095
        
        /// <summary>
        /// Use anisotropic interpolation for minification, magnification, and mip-level sampling.
        /// Compare the result to the comparison value.
        /// </summary>
        ComparisonAnisotropic = 213, // 0x000000D5

        // The following are not guaranteed to work. 
        // One has to do feature checking before using them, but SeeingSharp doesn't support feature checks currently.

        // /// <summary>No documentation.</summary>
        // /// <unmanaged>D3D11_FILTER_MINIMUM_MIN_MAG_MIP_POINT</unmanaged>
        // /// <unmanaged-short>D3D11_FILTER_MINIMUM_MIN_MAG_MIP_POINT</unmanaged-short>
        // MinimumMinMagMipPoint = 256, // 0x00000100
        // /// <summary>No documentation.</summary>
        // /// <unmanaged>D3D11_FILTER_MINIMUM_MIN_MAG_POINT_MIP_LINEAR</unmanaged>
        // /// <unmanaged-short>D3D11_FILTER_MINIMUM_MIN_MAG_POINT_MIP_LINEAR</unmanaged-short>
        // MinimumMinMagPointMipLinear = 257, // 0x00000101
        // /// <summary>No documentation.</summary>
        // /// <unmanaged>D3D11_FILTER_MINIMUM_MIN_POINT_MAG_LINEAR_MIP_POINT</unmanaged>
        // /// <unmanaged-short>D3D11_FILTER_MINIMUM_MIN_POINT_MAG_LINEAR_MIP_POINT</unmanaged-short>
        // MinimumMinPointMagLinearMipPoint = 260, // 0x00000104
        // /// <summary>No documentation.</summary>
        // /// <unmanaged>D3D11_FILTER_MINIMUM_MIN_POINT_MAG_MIP_LINEAR</unmanaged>
        // /// <unmanaged-short>D3D11_FILTER_MINIMUM_MIN_POINT_MAG_MIP_LINEAR</unmanaged-short>
        // MinimumMinPointMagMipLinear = 261, // 0x00000105
        // /// <summary>No documentation.</summary>
        // /// <unmanaged>D3D11_FILTER_MINIMUM_MIN_LINEAR_MAG_MIP_POINT</unmanaged>
        // /// <unmanaged-short>D3D11_FILTER_MINIMUM_MIN_LINEAR_MAG_MIP_POINT</unmanaged-short>
        // MinimumMinLinearMagMipPoint = 272, // 0x00000110
        // /// <summary>No documentation.</summary>
        // /// <unmanaged>D3D11_FILTER_MINIMUM_MIN_LINEAR_MAG_POINT_MIP_LINEAR</unmanaged>
        // /// <unmanaged-short>D3D11_FILTER_MINIMUM_MIN_LINEAR_MAG_POINT_MIP_LINEAR</unmanaged-short>
        // MinimumMinLinearMagPointMipLinear = 273, // 0x00000111
        // /// <summary>No documentation.</summary>
        // /// <unmanaged>D3D11_FILTER_MINIMUM_MIN_MAG_LINEAR_MIP_POINT</unmanaged>
        // /// <unmanaged-short>D3D11_FILTER_MINIMUM_MIN_MAG_LINEAR_MIP_POINT</unmanaged-short>
        // MinimumMinMagLinearMipPoint = 276, // 0x00000114
        // /// <summary>No documentation.</summary>
        // /// <unmanaged>D3D11_FILTER_MINIMUM_MIN_MAG_MIP_LINEAR</unmanaged>
        // /// <unmanaged-short>D3D11_FILTER_MINIMUM_MIN_MAG_MIP_LINEAR</unmanaged-short>
        // MinimumMinMagMipLinear = 277, // 0x00000115
        // /// <summary>No documentation.</summary>
        // /// <unmanaged>D3D11_FILTER_MINIMUM_ANISOTROPIC</unmanaged>
        // /// <unmanaged-short>D3D11_FILTER_MINIMUM_ANISOTROPIC</unmanaged-short>
        // MinimumAnisotropic = 341, // 0x00000155
        // /// <summary>No documentation.</summary>
        // /// <unmanaged>D3D11_FILTER_MAXIMUM_MIN_MAG_MIP_POINT</unmanaged>
        // /// <unmanaged-short>D3D11_FILTER_MAXIMUM_MIN_MAG_MIP_POINT</unmanaged-short>
        // MaximumMinMagMipPoint = 384, // 0x00000180
        // /// <summary>No documentation.</summary>
        // /// <unmanaged>D3D11_FILTER_MAXIMUM_MIN_MAG_POINT_MIP_LINEAR</unmanaged>
        // /// <unmanaged-short>D3D11_FILTER_MAXIMUM_MIN_MAG_POINT_MIP_LINEAR</unmanaged-short>
        // MaximumMinMagPointMipLinear = 385, // 0x00000181
        // /// <summary>No documentation.</summary>
        // /// <unmanaged>D3D11_FILTER_MAXIMUM_MIN_POINT_MAG_LINEAR_MIP_POINT</unmanaged>
        // /// <unmanaged-short>D3D11_FILTER_MAXIMUM_MIN_POINT_MAG_LINEAR_MIP_POINT</unmanaged-short>
        // MaximumMinPointMagLinearMipPoint = 388, // 0x00000184
        // /// <summary>No documentation.</summary>
        // /// <unmanaged>D3D11_FILTER_MAXIMUM_MIN_POINT_MAG_MIP_LINEAR</unmanaged>
        // /// <unmanaged-short>D3D11_FILTER_MAXIMUM_MIN_POINT_MAG_MIP_LINEAR</unmanaged-short>
        // MaximumMinPointMagMipLinear = 389, // 0x00000185
        // /// <summary>No documentation.</summary>
        // /// <unmanaged>D3D11_FILTER_MAXIMUM_MIN_LINEAR_MAG_MIP_POINT</unmanaged>
        // /// <unmanaged-short>D3D11_FILTER_MAXIMUM_MIN_LINEAR_MAG_MIP_POINT</unmanaged-short>
        // MaximumMinLinearMagMipPoint = 400, // 0x00000190
        // /// <summary>No documentation.</summary>
        // /// <unmanaged>D3D11_FILTER_MAXIMUM_MIN_LINEAR_MAG_POINT_MIP_LINEAR</unmanaged>
        // /// <unmanaged-short>D3D11_FILTER_MAXIMUM_MIN_LINEAR_MAG_POINT_MIP_LINEAR</unmanaged-short>
        // MaximumMinLinearMagPointMipLinear = 401, // 0x00000191
        // /// <summary>No documentation.</summary>
        // /// <unmanaged>D3D11_FILTER_MAXIMUM_MIN_MAG_LINEAR_MIP_POINT</unmanaged>
        // /// <unmanaged-short>D3D11_FILTER_MAXIMUM_MIN_MAG_LINEAR_MIP_POINT</unmanaged-short>
        // MaximumMinMagLinearMipPoint = 404, // 0x00000194
        // /// <summary>No documentation.</summary>
        // /// <unmanaged>D3D11_FILTER_MAXIMUM_MIN_MAG_MIP_LINEAR</unmanaged>
        // /// <unmanaged-short>D3D11_FILTER_MAXIMUM_MIN_MAG_MIP_LINEAR</unmanaged-short>
        // MaximumMinMagMipLinear = 405, // 0x00000195
        // /// <summary>No documentation.</summary>
        // /// <unmanaged>D3D11_FILTER_MAXIMUM_ANISOTROPIC</unmanaged>
        // /// <unmanaged-short>D3D11_FILTER_MAXIMUM_ANISOTROPIC</unmanaged-short>
        // MaximumAnisotropic = 469, // 0x000001D5
    }

    /// <summary>
    /// Comparison options.
    /// </summary>
    public enum SeeingSharpComparisonFunction
    {
        /// <summary>
        /// Never pass the comparison.
        /// </summary>
        Never = 1,

        /// <summary>
        /// If the source data is less than the destination data, the comparison passes.
        /// </summary>
        Less = 2,

        /// <summary>
        /// If the source data is equal to the destination data, the comparison passes.
        /// </summary>
        Equal = 3,

        /// <summary>
        /// If the source data is less than or equal to the destination data, the comparison passes.
        /// </summary>
        LessEqual = 4,

        /// <summary>
        /// If the source data is greater than the destination data, the comparison passes.
        /// </summary>
        Greater = 5,
        
        /// <summary>
        /// If the source data is not equal to the destination data, the comparison passes.
        /// </summary>
        NotEqual = 6,
        
        /// <summary>
        /// If the source data is greater than or equal to the destination data, the comparison passes.
        /// </summary>
        GreaterEqual = 7,
        
        /// <summary>
        /// Always pass the comparison.
        /// </summary>
        Always = 8,
    }

    public delegate void TextureChangedHandler(object sender, TextureChangedEventArgs e);

    /// <summary>
    /// EventArgs class for TextureChangedHandler delegate
    /// </summary>
    public class TextureChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets current renderstate object.
        /// </summary>
        public RenderState RenderState { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureChangedEventArgs"/> class.
        /// </summary>
        /// <param name="renderState">Current render state.</param>
        internal TextureChangedEventArgs(RenderState renderState)
        {
            this.RenderState = renderState;
        }
    }
}