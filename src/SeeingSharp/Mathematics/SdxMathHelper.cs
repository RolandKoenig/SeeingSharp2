using System.Drawing;
using System.Numerics;

namespace SeeingSharp.Mathematics
{
    internal static class SdxMathHelper
    {
        internal static unsafe Matrix3x2 Matrix3x2FromRaw(Matrix3x2 value)
        {
            return *(Matrix3x2*)(&value);
        }

        internal static unsafe PointF RawFromVector2(Vector2 value)
        {
            return *(PointF*)(&value);
        }

        internal static unsafe Vector2 Vector2FromRaw(PointF value)
        {
            return *(Vector2*)(&value);
        }

        internal static unsafe Vortice.Mathematics.Color4 RawFromColor4(Color4 value)
        {
            return *(Vortice.Mathematics.Color4*)(&value);
        }

        internal static System.Drawing.Color ColorFromColor4(Color4 value)
        {
            return System.Drawing.Color.FromArgb(value.ToArgb());
        }
    }
}
