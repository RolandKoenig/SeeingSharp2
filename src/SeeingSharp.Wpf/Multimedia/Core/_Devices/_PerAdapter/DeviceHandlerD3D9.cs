using System;
using System.Runtime.InteropServices;
using SeeingSharp.Util;
using Vortice.DXGI;
using D3D9 = Vortice.Direct3D9;
using static Vortice.Direct3D9.D3D9;

namespace SeeingSharp.Core
{
    public class DeviceHandlerD3D9 : IDisposable
    {
        private D3D9.IDirect3DDevice9Ex? _deviceEx;
        private D3D9.IDirect3D9Ex? _direct3DEx;

        /// <summary>
        /// Is the device successfully initialized?
        /// </summary>
        public bool IsInitialized => _deviceEx != null;

        /// <summary>
        /// Gets the initialized device.
        /// </summary>
        internal D3D9.IDirect3DDevice9Ex Device
        {
            get
            {
                if (_deviceEx == null) { throw new ObjectDisposedException(nameof(DeviceHandlerD3D9)); }
                return _deviceEx;
            }
        }

        /// <summary>
        /// Gets current DirectX context.
        /// </summary>
        internal D3D9.IDirect3D9Ex Context
        {
            get
            {
                if (_direct3DEx == null) { throw new ObjectDisposedException(nameof(DeviceHandlerD3D9)); }
                return _direct3DEx;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceHandlerD3D9"/> class.
        /// </summary>
        /// <param name="dxgiAdapter">The target adapter.</param>
        /// <param name="isSoftwareAdapter">Are we in software mode?</param>
        internal DeviceHandlerD3D9(IDXGIAdapter1 dxgiAdapter, bool isSoftwareAdapter)
        {
            try
            {
                // Just needed when on true hardware
                if (!isSoftwareAdapter)
                {
                    //Prepare device creation
                    const D3D9.CreateFlags CREATE_FLAGS = D3D9.CreateFlags.HardwareVertexProcessing |
                                                          D3D9.CreateFlags.PureDevice |
                                                          D3D9.CreateFlags.FpuPreserve |
                                                          D3D9.CreateFlags.Multithreaded;

                    var presentparams = new D3D9.PresentParameters
                    {
                        Windowed = true,
                        SwapEffect = D3D9.SwapEffect.Discard,
                        DeviceWindowHandle = GetDesktopWindow(),
                        PresentationInterval = D3D9.PresentInterval.Default,
                        BackBufferCount = 1
                    };

                    //Create the device finally
                    var result = Create9Ex(out _direct3DEx);
                    result.CheckError();

                    // Try to find the Direct3D9 adapter that matches given DXGI adapter
                    var adapterIndex = -1;
                    for (var loop = 0; loop < _direct3DEx.AdapterCount; loop++)
                    {
                        var d3d9AdapterInfo = _direct3DEx.GetAdapterIdentifier(loop);
                        if (d3d9AdapterInfo.DeviceId == dxgiAdapter.Description1.DeviceId)
                        {
                            adapterIndex = loop;
                            break;
                        }
                    }

                    // Direct3D 9 is only relevant on the primary device
                    if (adapterIndex < 0) { return; }

                    // Try to create the device
                    _deviceEx = _direct3DEx.CreateDeviceEx(
                        adapterIndex, D3D9.DeviceType.Hardware, IntPtr.Zero,
                        CREATE_FLAGS, presentparams);
                }
            }
            catch (Exception)
            {
                // No direct3d 9 interface support
                SeeingSharpUtil.SafeDispose(ref _direct3DEx);
                SeeingSharpUtil.SafeDispose(ref _deviceEx);
            }
        }

        /// <summary>
        /// Unloads all loaded resources.
        /// </summary>
        public void Dispose()
        {
            _deviceEx = SeeingSharpUtil.DisposeObject(_deviceEx);
            _direct3DEx = SeeingSharpUtil.DisposeObject(_direct3DEx);
        }

        /// <summary>
        /// Gets current desktop window.
        /// </summary>
        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr GetDesktopWindow();
    }
}