﻿#region License information (SeeingSharp and all based games/applications)
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
            Alpha = alpha / 255f;
            Red = red / 255f;
            Green = green / 255f;
            Blue = blue / 255f;
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
            Alpha = newAlpha;
            return this;
        }

        public Color4 ChangeColorByLight(float changeFactor)
        {
            changeFactor.EnsureInRange(0.000001f, 0.4999999f, nameof(changeFactor));

            Red = Red < 0.5f ? Red + changeFactor : Red - changeFactor;
            Green = Green < 0.5f ? Green + changeFactor : Green - changeFactor;
            Blue = Blue < 0.5f ? Blue + changeFactor : Blue - changeFactor;

            return this;
        }
    }
}