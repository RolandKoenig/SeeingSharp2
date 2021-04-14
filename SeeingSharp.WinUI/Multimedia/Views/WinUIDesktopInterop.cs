using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;
using SharpDX;
using SharpDX.DXGI;

namespace SeeingSharp.Multimedia.Views
{
    static class WinUIDesktopInterop
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
