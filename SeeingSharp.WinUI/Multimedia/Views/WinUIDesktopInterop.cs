/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
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
using System;
using System.Runtime.InteropServices;
using SharpDX;

namespace SeeingSharp.Multimedia.Views
{
    internal static class WinUIDesktopInterop
    {
        /// <summary>
        /// Interface from microsoft.ui.xaml.media.dxinterop.h
        /// </summary>
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("24d43d84-4246-4aa7-9774-8604cb73d90d")]
        public interface ISwapChainBackgroundPanelNative
        {
            [PreserveSig] Result SetSwapChain([In]IntPtr swapChain);
        }

        /// <summary>
        /// Interface from microsoft.ui.xaml.media.dxinterop.h
        /// </summary>
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("63aad0b8-7c24-40ff-85a8-640d944cc325")]
        public interface ISwapChainPanelNative
        {
            [PreserveSig] Result SetSwapChain([In]IntPtr swapChain);
        }
    }
}
