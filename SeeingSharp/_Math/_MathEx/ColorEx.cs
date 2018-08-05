using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeeingSharp
{
    public static class ColorEx
    {
        public static Color FromArgb(byte a, byte r, byte g, byte b)
        {
            SharpDX.Color4 d;
      
            return new Color(r, g, b, a);
        }

        public static Color FromArgb(int a, int r, int g, int b)
        {
            return new Color((byte)r, (byte)g, (byte)b, (byte)a);
        }
    }
}
