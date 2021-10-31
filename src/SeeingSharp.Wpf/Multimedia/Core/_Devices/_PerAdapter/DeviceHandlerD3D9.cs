/*
    SeeingSharp and all applications distributed together with it. 
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
using SeeingSharp.Util;
using SharpDX.DXGI;
using D3D9 = SharpDX.Direct3D9;

namespace SeeingSharp.Multimedia.Core
{
    public class DeviceHandlerD3D9 : IDisposable
    {
        private D3D9.DeviceEx _deviceEx;
        private D3D9.Direct3DEx _direct3DEx;

        /// <summary>
        /// Is the device successfully initialized?
        /// </summary>
        public bool IsInitialized => _deviceEx != null;

        /// <summary>
        /// Gets the initialized device.
        /// </summary>
        internal D3D9.DeviceEx Device => _deviceEx;

        /// <summary>
        /// Gets current DirectX context.
        /// </summary>
        internal D3D9.Direct3DEx Context => _direct3DEx;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceHandlerD3D9"/> class.
        /// </summary>
        /// <param name="dxgiAdapter">The target adapter.</param>
        /// <param name="isSoftwareAdapter">Are we in software mode?</param>
        internal DeviceHandlerD3D9(Adapter1 dxgiAdapter, bool isSoftwareAdapter)
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
                    _direct3DEx = new D3D9.Direct3DEx();

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
                    _deviceEx = new D3D9.DeviceEx(_direct3DEx, adapterIndex, D3D9.DeviceType.Hardware, IntPtr.Zero, CREATE_FLAGS, presentparams);
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