using System.Numerics;
using SharpDX.Mathematics.Interop;

namespace SeeingSharp
{
    internal static class SdxMathHelper
    {
        internal static unsafe RawColor4 RawFromColor4(Color4 value)
        {
            return *(RawColor4*)(&value);
        }

        internal static unsafe RawMatrix3x2 RawFromMatrix3x2(Matrix3x2 value)
        {
            return *(RawMatrix3x2*)(&value);
        }

        internal static unsafe Matrix3x2 Matrix3x2FromRaw(RawMatrix3x2 value)
        {
            return *(Matrix3x2*)(&value);
        }

        internal static unsafe RawVector2 RawFromVector2(Vector2 value)
        {
            return *(RawVector2*)(&value);
        }

        internal static unsafe Vector2 Vector2FromRaw(RawVector2 value)
        {
            return *(Vector2*)(&value);
        }

        internal static unsafe RawRectangle RawFromRectangle(Rectangle value)
        {
            return *(RawRectangle*)(&value);
        }

        internal static unsafe RawRectangleF RawFromRectangleF(RectangleF value)
        {
            return *(RawRectangleF*)(&value);
        }

        internal static unsafe SharpDX.Size2 SdxFromSize2(Size2 value)
        {
            return *(SharpDX.Size2*)(&value);
        }
    }
}
