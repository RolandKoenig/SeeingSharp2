using System.Drawing;
using System.Numerics;

namespace SeeingSharp.Mathematics
{
    internal static class MathConverter
    {
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
    }
}
