using System;
using SeeingSharp.Mathematics;
using Wpf = System.Windows;
using WpfMedia = System.Windows.Media;

namespace SeeingSharp.Util
{

    public static class SeeingSharpWpfUtil
    {
        public static void Color4FromWpfColor(ref WpfMedia.Color source, ref Color4 target)
        {
            target.Red = source.R / 255f;
            target.Green = source.G / 255f;
            target.Blue = source.B / 255f;
            target.Alpha = source.A / 255f;
        }

        public static void WpfColorFromColor4(ref Color4 source, ref WpfMedia.Color target)
        {
            target.A = (byte)EngineMath.Clamp(0f, 255f, source.Alpha * 255f);
            target.R = (byte)EngineMath.Clamp(0f, 255f, source.Alpha * 255f);
            target.G = (byte)EngineMath.Clamp(0f, 255f, source.Alpha * 255f);
            target.B = (byte)EngineMath.Clamp(0f, 255f, source.Alpha * 255f);
        }

        public static void GetDpiScalingFactor(Wpf.UIElement uiElement, out double dpiScaleFactorX, out double dpiScaleFactorY)
        {
            var source = Wpf.PresentationSource.FromVisual(uiElement);
            dpiScaleFactorX = 1.0;
            dpiScaleFactorY = 1.0;

            if (source?.CompositionTarget != null)
            {
                dpiScaleFactorX = source.CompositionTarget.TransformToDevice.M11;
                dpiScaleFactorY = source.CompositionTarget.TransformToDevice.M22;
            }
        }

        public static Wpf.Size GetPixelSize(Wpf.UIElement uiElement, Wpf.Size minSize)
        {
            GetDpiScalingFactor(uiElement, out var dpiScaleFactorX, out var dpiScaleFactorY);

            return new Wpf.Size(
                Math.Max(uiElement.RenderSize.Width * dpiScaleFactorX, 100),
                Math.Max(uiElement.RenderSize.Height * dpiScaleFactorY, 100));
        }
    }
}