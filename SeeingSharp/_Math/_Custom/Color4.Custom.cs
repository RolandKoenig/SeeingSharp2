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
using System.Numerics;
using SeeingSharp.Checking;

namespace SeeingSharp
{
    public partial struct Color4
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> class.
        /// </summary>
        /// <param name="red">The red.</param>
        /// <param name="green">The green.</param>
        /// <param name="blue">The blue.</param>
        /// <param name="alpha">The alpha.</param>
        public Color4(int red, int green, int blue, int alpha)
        {
            this.Alpha = alpha / 255f;
            this.Red = red / 255f;
            this.Green = green / 255f;
            this.Blue = blue / 255f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> class.
        /// </summary>
        /// <param name="red">The red.</param>
        /// <param name="green">The green.</param>
        /// <param name="blue">The blue.</param>
        public Color4(int red, int green, int blue)
            : this(red, green, blue, 255)
        {

        }

        public Color4 ChangeAlphaTo(float newAlpha)
        {
            this.Alpha = newAlpha;
            return this;
        }

        public Color4 ChangeColorByLight(float changeFactor)
        {
            changeFactor.EnsureInRange(0.000001f, 0.4999999f, nameof(changeFactor));

            this.Red = this.Red < 0.5f ? this.Red + changeFactor : this.Red - changeFactor;
            this.Green = this.Green < 0.5f ? this.Green + changeFactor : this.Green - changeFactor;
            this.Blue = this.Blue < 0.5f ? this.Blue + changeFactor : this.Blue - changeFactor;

            return this;
        }

#if DESKTOP
        /// <summary>
        /// Initializes a new instance of the <see cref="Color4" /> struct.
        /// </summary>
        /// <param name="wpfColor">Color of the WPF.</param>
        public Color4(System.Windows.Media.Color wpfColor)
            : this((int)wpfColor.R, (int)wpfColor.G, (int)wpfColor.B, 0)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct.
        /// </summary>
        /// <param name="gdiColor">Color of the GDI.</param>
        public Color4(System.Drawing.Color gdiColor)
            : this(gdiColor.R, gdiColor.G, gdiColor.B, gdiColor.A)
        {
        }

        /// <summary>
        /// Generates a gdi color object
        /// </summary>
        public System.Drawing.Color ToGdiColor()
        {
            return System.Drawing.Color.FromArgb(
                (int)(255f * this.Alpha),
                (int)(255f * this.Red),
                (int)(255f * this.Green),
                (int)(255f * this.Blue));
        }

        /// <summary>
        /// Converts this value to a argb value
        /// </summary>
        public int ToArgb()
        {
            return this.ToGdiColor().ToArgb();
        }
#endif

#if UNIVERSAL
        /// <summary>
        /// Initializes a new instance of the <see cref="Color4"/> struct.
        /// </summary>
        /// <param name="winColor">The Windows.UI.Color structure from which to create a Color4.</param>
        public Color4(Windows.UI.Color winColor)
            : this(winColor.R, winColor.G, winColor.B, winColor.A)
        {

        }

        /// <summary>
        /// Converts this value to a argb value
        /// </summary>
        public int ToArgb()
        {
            uint a = (uint)(Alpha * 255.0f) & 255;
            uint r = (uint)(Red * 255.0f) & 255;
            uint g = (uint)(Green * 255.0f) & 255;
            uint b = (uint)(Blue * 255.0f) & 255;

            uint value = a;
            value |= r << 8;
            value |= g << 16;
            value |= b << 24;

            return (int)value;
        }
#endif
    }
}
