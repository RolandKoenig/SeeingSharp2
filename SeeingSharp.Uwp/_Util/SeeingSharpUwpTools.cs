using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Namespace mappings
using SDX = SharpDX;

namespace SeeingSharp.Util
{
    public static class SeeingSharpUwpTools
    {
        public static Color4 Color4FromUIColor(ref Windows.UI.Color uiColor)
        {
            return new Color4(
                (float)uiColor.R / 255f,
                (float)uiColor.G / 255f,
                (float)uiColor.B / 255f,
                (float)uiColor.A / 255f);
        }

        public static Windows.UI.Color UIColorFromColor4(ref Color4 color)
        {
            var uiColor = new Windows.UI.Color();
            uiColor.A = (byte)EngineMath.Clamp(0f, 255f, color.Alpha * 255f);
            uiColor.R = (byte)EngineMath.Clamp(0f, 255f, color.Red * 255f);
            uiColor.G = (byte)EngineMath.Clamp(0f, 255f, color.Green * 255f);
            uiColor.B = (byte)EngineMath.Clamp(0f, 255f, color.Blue * 255f);
            return uiColor;
        }
    }
}
