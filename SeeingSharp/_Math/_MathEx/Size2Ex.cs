using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeeingSharp
{
    public static class Size2Ex
    {
        public static Vector2 ToVector2(this ref Size2 size)
        {
            return new Vector2((float)size.Width, (float)size.Height);
        }
    }
}
