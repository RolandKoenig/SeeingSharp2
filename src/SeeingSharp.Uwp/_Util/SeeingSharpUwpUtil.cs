﻿using SeeingSharp.Mathematics;

namespace SeeingSharp.Util
{
    public static class SeeingSharpUwpUtil
    {
        public static Color4 Color4FromUIColor(ref Color uiColor)
        {
            return new Color4(
                uiColor.R / 255f,
                uiColor.G / 255f,
                uiColor.B / 255f,
                uiColor.A / 255f);
        }

        public static Color UIColorFromColor4(ref Color4 color)
        {
            var uiColor = new Color
            {
                A = (byte)EngineMath.Clamp(0f, 255f, color.Alpha * 255f),
                R = (byte)EngineMath.Clamp(0f, 255f, color.Red * 255f),
                G = (byte)EngineMath.Clamp(0f, 255f, color.Green * 255f),
                B = (byte)EngineMath.Clamp(0f, 255f, color.Blue * 255f)
            };

            return uiColor;
        }
    }
}