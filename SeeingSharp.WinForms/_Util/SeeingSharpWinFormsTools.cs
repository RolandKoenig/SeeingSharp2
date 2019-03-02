#region License information
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
#endregion
#region using

// Namespace mappings
using GDI = System.Drawing;
using SDX = SharpDX;

#endregion

namespace SeeingSharp.Util
{
    public static class SeeingSharpWinFormsTools
    {
        public static SDX.Color4 Color4FromGdiColor(this ref GDI.Color drawingColor)
        {
            return new SDX.Color4(
                (float)drawingColor.R / 255f,
                (float)drawingColor.G / 255f,
                (float)drawingColor.B / 255f,
                (float)drawingColor.A / 255f);
        }

        public static SDX.Point PointFromGdiPoint(GDI.Point point)
        {
            return new SDX.Point(point.X, point.Y);
        }
    }
}