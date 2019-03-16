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

using System;
using SharpDX;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public enum Graphics2DTransformMode
    {
        Custom,

        AutoScaleToVirtualScreen
    }

    /// <summary>
    /// Some bitmap format for Direct2D rendering.
    /// Maps directly to a subset of DXGI formats.
    /// </summary>
    public enum BitmapFormat
    {
        Bgra = SharpDX.DXGI.Format.B8G8R8A8_UNorm
    }

    ///// <summary>
    ///// Represents enum from Direct2D.
    ///// Contains the position and color of a gradient stop.
    ///// 
    ///// Gradient stops can be specified in any order if they are at different positions.
    ///// Two stops may share a position. In this case, the first stop specified is treated
    ///// as the "low" stop (nearer 0.0f) and subsequent stops are treated as "higher"
    ///// (nearer 1.0f). This behavior is useful if a caller wants an instant transition
    ///// in the middle of a stop.Typically, there are at least two points in a collection,
    ///// although creation with only one stop is permitted. For example, one point is
    ///// at position 0.0f, another point is at position 1.0f, and additional points are
    ///// distributed in the [0, 1] range. Where the gradient progression is beyond the
    ///// range of [0, 1], the stops are stored, but may affect the gradient. When drawn,
    ///// the [0, 1] range of positions is mapped to the brush, in a brush-dependent way.
    ///// For details, see SharpDX.Direct2D1.LinearGradientBrush and SharpDX.Direct2D1.RadialGradientBrush.
    ///// Gradient stops with a position outside the [0, 1] range cannot be seen explicitly,
    ///// but they can still affect the colors produced in the [0, 1] range. For example,
    ///// a two-stop gradient 0.0f, Black}, {2.0f, White is indistinguishable visually
    ///// from 0.0f, Black}, {1.0f, Mid-level gray. Also, the colors are clamped before
    ///// interpolation.
    ///// </summary>
    //public struct GradientStop
    //{
    //    public GradientStop(Color4 color, float position)
    //    {
    //        Color = color;
    //        Position = position;
    //    }

    //    /// <summary>
    //    /// The color of the gradient stop.
    //    /// </summary>
    //    public Color4 Color;

    //    /// <summary>
    //    /// A value that indicates the relative position of the gradient stop in the brush.
    //    /// This value must be in the [0.0f, 1.0f] range if the gradient stop is to be seen
    //    ///  explicitly.
    //    /// </summary>
    //    public float Position;
    //}
}