#region License information
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
using SharpDX;

// Some namespace mappings
using DXGI = SharpDX.DXGI;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public enum Graphics2DTransformMode
    {
        Custom,

        AutoScaleToVirtualScreen
    }

    /// <summary>
    /// Represents enum from Direct2D.
    /// </summary>
    [Flags]
    public enum DrawTextOptions : int
    {
        /// <summary>
        /// Text is vertically snapped to pixel boundaries and is not clipped to the layout rectangle.
        /// </summary>
        None = 0,

        /// <summary>
        /// Text is not vertically snapped to pixel boundaries. This setting is recommended
        /// for text that is being animated.
        /// </summary>
        NoSnap = 1,

        /// <summary>
        /// Text is clipped to the layout rectangle.
        /// </summary>
        Clip = 2,
    }

    /// <summary>
    /// Represents enum from Direct2D.
    /// </summary>
    public enum MeasuringMode : int
    {
        /// <summary>
        /// Specifies that text is measured using glyph ideal metrics whose values are
        /// independent to the current display resolution.
        /// </summary>
        Natural = 0,

        /// <summary>
        /// Specifies that text is measured using glyph display-compatible metrics whose
        /// values tuned for the current display resolution.
        /// </summary>
        GdiClassic = 1,

        /// <summary>
        /// Specifies that text is measured using the same glyph display metrics as
        /// text measured by GDI using a font created with CLEARTYPE_NATURAL_QUALITY.
        /// </summary>
        GdiNatural = 2,
    }

    /// <summary>
    /// Represents enum from DirectWrite.
    /// </summary>
    public enum FontStyle : int
    {
        Normal = 0,
        Oblique = 1,
        Italic = 2,
    }

    /// <summary>
    /// Represents enum from DirectWrite.
    /// </summary>
    public enum FontWeight : int
    {
        Thin = 100,
        ExtraLight = 200,
        UltraLight = 200,
        Light = 300,
        Normal = 400,
        Regular = 400,
        Medium = 500,
        DemiBold = 600,
        SemiBold = 600,
        Bold = 700,
        UltraBold = 800,
        ExtraBold = 800,
        Heavy = 900,
        Black = 900,
        ExtraBlack = 950,
        UltraBlack = 950,
    }

    /// <summary>
    /// Represents enum from DirectWrite.
    /// </summary>
    public enum FontStretch : int
    {
        Undefined = 0,
        UltraCondensed = 1,
        ExtraCondensed = 2,
        Condensed = 3,
        SemiCondensed = 4,
        Normal = 5,
        Medium = 5,
        SemiExpanded = 6,
        Expanded = 7,
        ExtraExpanded = 8,
        UltraExpanded = 9,
    }

    /// <summary>
    /// Represents enum from DirectWrite.
    /// </summary>
    public enum ParagraphAlignment : int
    {
        Near = 0,
        Far = 1,
        Center = 2,
    }

    /// <summary>
    /// Represents enum from DirectWrite.
    /// </summary>
    public enum TextAlignment : int
    {
        Leading = 0,
        Trailing = 1,
        Center = 2,
    }

    /// <summary>
    /// Represents enum from DirectWrite.
    /// </summary>
    public enum WordWrapping : int
    {
        Wrap = 0,
        NoWrap = 1,
    }

    /// <summary>
    /// Represents enum from DirectWrite.
    /// </summary>
    public enum ReadingDirection : int
    {
        LeftToRight = 0,
        RightToLeft = 1,
    }

    /// <summary>
    /// Represents enum from Direct2D.
    /// </summary>
    public enum AlphaMode
    {
        /// <summary>
        /// The alpha value might not be meaningful.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The alpha value has been premultiplied. Each color is first scaled by the
        /// alpha value. The alpha value itself is the same in both straight and premultiplied
        /// alpha. Typically, no color channel value is greater than the alpha channel
        /// value. If a color channel value in a premultiplied format is greater than
        /// the alpha channel, the standard source-over blending math results in an additive
        /// blend.
        /// </summary>
        Premultiplied = 1,

        /// <summary>
        /// The alpha value has not been premultiplied. The alpha channel indicates
        /// the transparency of the color.
        /// </summary>
        Straight = 2,

        /// <summary>
        /// The alpha value is ignored.
        /// </summary>
        Ignore = 3,
    }

    /// <summary>
    /// Represents enum from Direct2D.
    /// </summary>
    public enum BitmapInterpolationMode : int
    {
        NearestNeighbor = 0,
        Linear = 1,
    }

    /// <summary>
    /// Some bitmap format for Direct2D rendering.
    /// Maps directly to a subset of DXGI formats.
    /// </summary>
    public enum BitmapFormat : int
    {
        Bgra = DXGI.Format.B8G8R8A8_UNorm
    }

    /// <summary>
    /// Represents enum from Direct2D.
    /// Specifies how a brush paints areas outside of its normal content area.
    /// </summary>
    public enum ExtendMode : int
    {
        /// <summary>
        /// Repeat the edge pixels of the brush's content for all regions outside the normal
        /// content area.
        /// </summary>  
        Clamp = 0,

        /// <summary>
        /// Repeat the brush's content.
        /// </summary>
        Wrap = 1,

        /// <summary>
        /// The same as ExtendMode.Wrap, except that alternate tiles of
        /// the brush's content are flipped. (The brush's normal content is drawn untransformed.)
        /// </summary>
        Mirror = 2
    }

    /// <summary>
    /// Represents enum from Direct2D.
    /// Specifies which gamma is used for interpolation.
    /// 
    /// Interpolating in a linear gamma space (SharpDX.Direct2D1.Gamma.Linear) can avoid
    /// changes in perceived brightness caused by the effect of gamma correction in spaces
    /// where the gamma is not 1.0, such as the default sRGB color space, where the gamma
    /// is 2.2. For an example of the differences between these two blending modes, consider
    /// the following illustration, which shows two gradients, each of which blends from
    /// red to blue to green:The first gradient is interpolated linearly in the space
    /// of the render target (sRGB in this case), and one can see the dark bands between
    /// each color. The second gradient uses a gamma-correct linear interpolation, and
    /// thus does not exhibit the same variations in brightness.
    /// </summary>
    public enum Gamma
    {
        /// <summary>
        /// Interpolation is performed in the standard RGB (sRGB) gamma.
        /// </summary>
        StandardRgb = 0,

        /// <summary>
        /// Interpolation is performed in the linear-gamma color space.
        /// </summary>  
        Linear = 1
    }

    /// <summary>
    /// Represents enum from Direct2D.
    /// Contains the position and color of a gradient stop.
    /// 
    /// Gradient stops can be specified in any order if they are at different positions.
    /// Two stops may share a position. In this case, the first stop specified is treated
    /// as the "low" stop (nearer 0.0f) and subsequent stops are treated as "higher"
    /// (nearer 1.0f). This behavior is useful if a caller wants an instant transition
    /// in the middle of a stop.Typically, there are at least two points in a collection,
    /// although creation with only one stop is permitted. For example, one point is
    /// at position 0.0f, another point is at position 1.0f, and additional points are
    /// distributed in the [0, 1] range. Where the gradient progression is beyond the
    /// range of [0, 1], the stops are stored, but may affect the gradient. When drawn,
    /// the [0, 1] range of positions is mapped to the brush, in a brush-dependent way.
    /// For details, see SharpDX.Direct2D1.LinearGradientBrush and SharpDX.Direct2D1.RadialGradientBrush.
    /// Gradient stops with a position outside the [0, 1] range cannot be seen explicitly,
    /// but they can still affect the colors produced in the [0, 1] range. For example,
    /// a two-stop gradient 0.0f, Black}, {2.0f, White is indistinguishable visually
    /// from 0.0f, Black}, {1.0f, Mid-level gray. Also, the colors are clamped before
    /// interpolation.
    /// </summary> 
    public struct GradientStop
    {
        public GradientStop(Color4 color, float position)
        {
            this.Color = color;
            this.Position = position;
        }

        /// <summary>
        /// The color of the gradient stop.
        /// </summary>
        public Color4 Color;

        /// <summary>
        /// A value that indicates the relative position of the gradient stop in the brush.
        /// This value must be in the [0.0f, 1.0f] range if the gradient stop is to be seen
        ///  explicitly.
        /// </summary> 
        public float Position;
    }
}
