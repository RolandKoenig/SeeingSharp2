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
        [Guid("63aad0b8-7c24-40ff-85a8-640d944cc325")]
        public interface ISwapChainPanelNative
        {
            [PreserveSig] Result SetSwapChain([In]IntPtr swapChain);
        }
    }
}
