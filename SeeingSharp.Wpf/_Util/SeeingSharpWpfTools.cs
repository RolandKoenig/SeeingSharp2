#region License information
/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
#endregion
#region using

// Namespace mappings
using Wpf = System.Windows;
using WpfMedia = System.Windows.Media;

#endregion

namespace SeeingSharp.Util
{
    #region using

    using System;
    using System.Reflection;

    #endregion

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
            GetDpiScalingFactor(uiElement, out double dpiScaleFactorX, out double dpiScaleFactorY);

            return new Wpf.Size(
                Math.Max(uiElement.RenderSize.Width * dpiScaleFactorX, 100),
                Math.Max(uiElement.RenderSize.Height * dpiScaleFactorY, 100));
        }

        public static T ReadPrivateMember<T, U>(U sourceObject, string memberName)
        {
            var fInfo = typeof(U).GetTypeInfo().GetField(memberName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (fInfo == null)
            {
                throw new SeeingSharpException($"Unable to read member {memberName} from object of type {typeof(U).FullName}!");
            }

            return (T)fInfo.GetValue(sourceObject);
        }
    }
}