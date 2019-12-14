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
using SDX = SharpDX;

namespace SeeingSharp.Util
{
    public static class SeeingSharpUwpUtil
    {
        public static Color4 Color4FromUIColor(ref Color uiColor)
        {
            return new Color4(
                uiColor.R / 255f,
                uiColor.G / 255f,
                uiColor.B / 255f,
                uiColor.A / 255f);
        }

        public static Color UIColorFromColor4(ref Color4 color)
        {
            var uiColor = new Color
            {
                A = (byte) EngineMath.Clamp(0f, 255f, color.Alpha * 255f),
                R = (byte) EngineMath.Clamp(0f, 255f, color.Red * 255f),
                G = (byte) EngineMath.Clamp(0f, 255f, color.Green * 255f),
                B = (byte) EngineMath.Clamp(0f, 255f, color.Blue * 255f)
            };

            return uiColor;
        }
    }
}