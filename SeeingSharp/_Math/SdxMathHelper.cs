using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Numerics;

namespace SeeingSharp
{  
    internal static class SdxMathHelper
    {
        internal static unsafe SharpDX.Mathematics.Interop.RawColor4 RawFromColor4(Color4 value)
        {
            return *(SharpDX.Mathematics.Interop.RawColor4*)(&value);
        }

        internal static unsafe SharpDX.Mathematics.Interop.RawMatrix3x2 RawFromMatrix3x2(Matrix3x2 value)
        {
            return *(SharpDX.Mathematics.Interop.RawMatrix3x2*)(&value);
        }

        internal static unsafe Matrix3x2 Matrix3x2FromRaw(SharpDX.Mathematics.Interop.RawMatrix3x2 value)
        {
            return *(Matrix3x2*)(&value);
        }

        internal static unsafe SharpDX.Mathematics.Interop.RawVector2 RawFromVector2(Vector2 value)
        {
            return *(SharpDX.Mathematics.Interop.RawVector2*) (&value);
        }

        internal static unsafe Vector2 Vector2FromRaw(SharpDX.Mathematics.Interop.RawVector2 value)
        {
            return *(Vector2*)(&value);
        }

        internal static unsafe SharpDX.Mathematics.Interop.RawRectangle RawFromRectangle(Rectangle value)
        {
            return *(SharpDX.Mathematics.Interop.RawRectangle*)(&value);
        }

        internal static unsafe SharpDX.Mathematics.Interop.RawRectangleF RawFromRectangleF(RectangleF value)
        {
            return *(SharpDX.Mathematics.Interop.RawRectangleF*)(&value);
        }

        internal static unsafe SharpDX.Size2 SdxFromSize2(Size2 value)
        {
            return *(SharpDX.Size2*)(&value);
        }
    }
}
