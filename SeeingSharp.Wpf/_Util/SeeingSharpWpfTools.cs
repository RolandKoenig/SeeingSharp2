using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

// Namespace mappings
using Wpf = System.Windows;
using WpfMedia = System.Windows.Media;


namespace SeeingSharp.Util
{
    public static class SeeingSharpWpfTools
    {
        public static void Color4FromWpfColor(ref WpfMedia.Color source, ref SharpDX.Color4 target)
        {
            target.Red = (float) source.R / 255f;
            target.Green = (float) source.G / 255f;
            target.Blue = (float) source.B / 255f;
            target.Alpha = (float) source.A / 255f;
        }

        public static void WpfColorFromColor4(ref SharpDX.Color4 source, ref WpfMedia.Color target)
        {
            target.A = (byte) EngineMath.Clamp(0f, 255f, source.Alpha * 255f);
            target.R= (byte) EngineMath.Clamp(0f, 255f, source.Alpha * 255f);
            target.G = (byte) EngineMath.Clamp(0f, 255f, source.Alpha * 255f);
            target.B = (byte) EngineMath.Clamp(0f, 255f, source.Alpha * 255f);
        }

        public static Wpf.Size GetPixelSize(Wpf.UIElement uiElement, Wpf.Size minSize)
        {
            Wpf.PresentationSource source = Wpf.PresentationSource.FromVisual(uiElement);
            double dpiScaleFactorX = 1.0;
            double dpiScaleFactorY = 1.0;
            if (source?.CompositionTarget != null)
            {
                dpiScaleFactorX = source.CompositionTarget.TransformToDevice.M11;
                dpiScaleFactorY = source.CompositionTarget.TransformToDevice.M22;
            }

            return new Wpf.Size(
                Math.Max(uiElement.RenderSize.Width * dpiScaleFactorX, 100),
                Math.Max(uiElement.RenderSize.Height * dpiScaleFactorY, 100));
        }

        public static T ReadPrivateMember<T, U>(U sourceObject, string memberName)
        {
            FieldInfo fInfo = typeof(U).GetTypeInfo().GetField(memberName, BindingFlags.NonPublic | BindingFlags.Instance);
            if(fInfo == null) { throw new SeeingSharpException($"Unable to read member {memberName} from object of type {typeof(U).FullName}!"); }
            return (T)fInfo.GetValue(sourceObject);
        }
    }
}
