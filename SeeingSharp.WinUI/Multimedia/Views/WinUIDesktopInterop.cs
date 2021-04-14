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
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("88fd8248-10da-4810-bb4c-010dd27faea9")]
        public interface ISwapChainPanelNative
        {
            //[PreserveSig] Result SetSwapChain([In]SwapChain swapChain);
            [PreserveSig] Result SetSwapChain([In]IntPtr swapChain);
        }

        //[Guid("63aad0b8-7c24-40ff-85a8-640d944cc325")]
        //public class ISwapChainPanelNative : ComObject
        //{
        //    public ISwapChainPanelNative(IntPtr nativePtr)
        //        : base(nativePtr)
        //    {

        //    }

        //    public static explicit operator ISwapChainPanelNative(
        //        IntPtr nativePtr)
        //    {
        //        return !(nativePtr == IntPtr.Zero) ? new ISwapChainPanelNative(nativePtr) : null;
        //    }

        //    ///// <summary>
        //    ///// <p>Sets the DirectX swap chain for <strong>SwapChainBackgroundPanel</strong>.</p>
        //    ///// </summary>
        //    ///// <doc-id>hh848327</doc-id>
        //    ///// <unmanaged>SetSwapChain</unmanaged>
        //    ///// <unmanaged-short>SetSwapChain</unmanaged-short>
        //    //public SwapChain SwapChain
        //    //{
        //    //    set => this.SetSwapChain(value);
        //    //}

        //    public SwapChain SwapChain
        //    {
        //        get;
        //        set;
        //    }

        //    ///// <summary>
        //    ///// <p>Sets the DirectX swap chain for <strong>SwapChainBackgroundPanel</strong>.</p>
        //    ///// </summary>
        //    ///// <param name="swapChain">No documentation.</param>
        //    ///// <returns><p>If this method succeeds, it returns <strong><see cref="F:SharpDX.Result.Ok" /></strong>. Otherwise, it returns an <strong><see cref="T:SharpDX.Result" /></strong> error code.</p></returns>
        //    ///// <doc-id>hh848327</doc-id>
        //    ///// <unmanaged>HRESULT ISwapChainBackgroundPanelNative::SetSwapChain([In] IDXGISwapChain* swapChain)</unmanaged>
        //    ///// <unmanaged-short>ISwapChainBackgroundPanelNative::SetSwapChain</unmanaged-short>
        //    //internal unsafe void SetSwapChain(SwapChain swapChain)
        //    //{
        //    //    IntPtr zero = IntPtr.Zero;
        //    //    // ISSUE: cast to a function pointer type
        //    //    // ISSUE: function pointer call
        //    //    ((Result) __calli((__FnPtr<int (void*, void*)>) *(IntPtr*) (*(IntPtr*) this._nativePointer + new IntPtr(3) * sizeof (void*)))(this._nativePointer, (void*) CppObject.ToCallbackPtr<SwapChain>((ICallbackable) swapChain))).CheckError();
        //    //}
        //}

        //[Guid("63aad0b8-7c24-40ff-85a8-640d944cc325")]
        //public class ISwapChainBackgroundPanelNative : ComObject
        //{
        //    public ISwapChainBackgroundPanelNative(IntPtr nativePtr)
        //        : base(nativePtr)
        //    {

        //    }

        //    public static explicit operator ISwapChainBackgroundPanelNative(
        //        IntPtr nativePtr)
        //    {
        //        return !(nativePtr == IntPtr.Zero) ? new ISwapChainBackgroundPanelNative(nativePtr) : null;
        //    }

        //    ///// <summary>
        //    ///// <p>Sets the DirectX swap chain for <strong>SwapChainBackgroundPanel</strong>.</p>
        //    ///// </summary>
        //    ///// <doc-id>hh848327</doc-id>
        //    ///// <unmanaged>SetSwapChain</unmanaged>
        //    ///// <unmanaged-short>SetSwapChain</unmanaged-short>
        //    //public SwapChain SwapChain
        //    //{
        //    //    set => this.SetSwapChain(value);
        //    //}

        //    public SwapChain SwapChain
        //    {
        //        get;
        //        set;
        //    }

        //    ///// <summary>
        //    ///// <p>Sets the DirectX swap chain for <strong>SwapChainBackgroundPanel</strong>.</p>
        //    ///// </summary>
        //    ///// <param name="swapChain">No documentation.</param>
        //    ///// <returns><p>If this method succeeds, it returns <strong><see cref="F:SharpDX.Result.Ok" /></strong>. Otherwise, it returns an <strong><see cref="T:SharpDX.Result" /></strong> error code.</p></returns>
        //    ///// <doc-id>hh848327</doc-id>
        //    ///// <unmanaged>HRESULT ISwapChainBackgroundPanelNative::SetSwapChain([In] IDXGISwapChain* swapChain)</unmanaged>
        //    ///// <unmanaged-short>ISwapChainBackgroundPanelNative::SetSwapChain</unmanaged-short>
        //    //internal unsafe void SetSwapChain(SwapChain swapChain)
        //    //{
        //    //    IntPtr zero = IntPtr.Zero;
        //    //    // ISSUE: cast to a function pointer type
        //    //    // ISSUE: function pointer call
        //    //    ((Result) __calli((__FnPtr<int (void*, void*)>) *(IntPtr*) (*(IntPtr*) this._nativePointer + new IntPtr(3) * sizeof (void*)))(this._nativePointer, (void*) CppObject.ToCallbackPtr<SwapChain>((ICallbackable) swapChain))).CheckError();
        //    //}
        //}
    }
}
