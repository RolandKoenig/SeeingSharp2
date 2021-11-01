using GDI = System.Drawing;

namespace SeeingSharp.Util
{
    public static class SeeingSharpWinFormsUtil
    {
        public static Color4 Color4FromGdiColor(this ref GDI.Color drawingColor)
        {
            return new Color4(
                drawingColor.R / 255f,
                drawingColor.G / 255f,
                drawingColor.B / 255f,
                drawingColor.A / 255f);
        }

        public static Point PointFromGdiPoint(GDI.Point point)
        {
            return new Point(point.X, point.Y);
        }
    }
}