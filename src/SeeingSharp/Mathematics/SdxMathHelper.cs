using System.Numerics;

namespace SeeingSharp.Mathematics
{
    internal static class SdxMathHelper
    {
        internal static unsafe Matrix3x2 Matrix3x2FromRaw(Matrix3x2 value)
        {
            return *(Matrix3x2*)(&value);
        }

        internal static unsafe System.Drawing.PointF RawFromVector2(Vector2 value)
        {
            return *(System.Drawing.PointF*)(&value);
        }

        internal static unsafe Vector2 Vector2FromRaw(System.Drawing.PointF value)
        {
            return *(Vector2*)(&value);
        }

        internal static unsafe System.Drawing.Rectangle RawFromRectangle(Rectangle value)
        {
            return *(System.Drawing.Rectangle*)(&value);
        }

        internal static unsafe System.Drawing.RectangleF RawFromRectangleF(RectangleF value)
        {
            return *(System.Drawing.RectangleF*)(&value);
        }

        internal static System.Drawing.Color RawFromColor4(Color4 color)
        {
            return System.Drawing.Color.FromArgb(color.ToArgb());
        }
    }
}
