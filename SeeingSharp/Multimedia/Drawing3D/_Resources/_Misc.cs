#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
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

    using System;
    using Core;

    #endregion

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

        Directional,
    }

    public delegate void TextureChangedHandler(object sender, TextureChangedEventArgs e);

    /// <summary>
    /// EventArgs class for TextureChangedHandler delegate
    /// </summary>
    public class TextureChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextureChangedEventArgs"/> class.
        /// </summary>
        /// <param name="renderState">Current render state.</param>
        internal TextureChangedEventArgs(RenderState renderState)
        {
            RenderState = renderState;
        }

        /// <summary>
        /// Gets current renderstate object.
        /// </summary>
        public RenderState RenderState { get; }
    }
}