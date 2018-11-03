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
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Namespace mappings
using SDX = SharpDX;

namespace SeeingSharp.Util
{
    public static class SeeingSharpUwpTools
    {
        public static Color4 Color4FromUIColor(ref Windows.UI.Color uiColor)
        {
            return new Color4(
                (float)uiColor.R / 255f,
                (float)uiColor.G / 255f,
                (float)uiColor.B / 255f,
                (float)uiColor.A / 255f);
        }

        public static Windows.UI.Color UIColorFromColor4(ref Color4 color)
        {
            var uiColor = new Windows.UI.Color();
            uiColor.A = (byte)EngineMath.Clamp(0f, 255f, color.Alpha * 255f);
            uiColor.R = (byte)EngineMath.Clamp(0f, 255f, color.Red * 255f);
            uiColor.G = (byte)EngineMath.Clamp(0f, 255f, color.Green * 255f);
            uiColor.B = (byte)EngineMath.Clamp(0f, 255f, color.Blue * 255f);
            return uiColor;
        }
    }
}
