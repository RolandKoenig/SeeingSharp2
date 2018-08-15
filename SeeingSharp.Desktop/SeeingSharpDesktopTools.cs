using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Namespace mappings
using GDI = System.Drawing;
using SDX = SharpDX;

namespace SeeingSharp.Desktop
{
    public static class SeeingSharpDesktopTools
    {
        public static Color4 Color4FromGdiColor(this ref GDI.Color drawingColor)
        {
            return new Color4(
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
