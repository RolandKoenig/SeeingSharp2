using System;
using SeeingSharp.Core;

namespace SeeingSharp.Drawing3D
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