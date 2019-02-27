#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
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

//Some namespace mappings
using D3D9 = SharpDX.Direct3D9;

#endregion

namespace SeeingSharp.Multimedia.Core
{
    #region using

    using System;
    using System.Runtime.InteropServices;
    using SeeingSharp.Util;

    #endregion

    public class DeviceHandlerD3D9 : IDisposable
    {
        private SharpDX.DXGI.Adapter1 m_dxgiAdapter;
        private D3D9.Direct3DEx m_direct3DEx;
        private D3D9.DeviceEx m_deviceEx;
        private int m_adapterIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceHandlerD3D9"/> class.
        /// </summary>
        /// <param name="dxgiAdapter">The target adapter.</param>
        /// <param name="isSoftwareAdapter">Are we in software mode?</param>
        /// <param name="debugEnabled">Is debug mode enabled?</param>
        internal DeviceHandlerD3D9(SharpDX.DXGI.Adapter1 dxgiAdapter, bool isSoftwareAdapter, bool debugEnabled)
        {
            // Update member variables
            m_dxgiAdapter = dxgiAdapter;

            try
            {
                // Just needed when on true hardware
                if (!isSoftwareAdapter)
                {
                    //Prepare device creation
                    var createFlags =
                        D3D9.CreateFlags.HardwareVertexProcessing |
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
                    m_direct3DEx = new D3D9.Direct3DEx();

                    // Try to find the Direct3D9 adapter that maches given DXGI adapter
                    m_adapterIndex = -1;
                    for (int loop = 0; loop < m_direct3DEx.AdapterCount; loop++)
                    {
                        var d3d9AdapterInfo = m_direct3DEx.GetAdapterIdentifier(loop);
                        if (d3d9AdapterInfo.DeviceId == m_dxgiAdapter.Description1.DeviceId)
                        {
                            m_adapterIndex = loop;
                            break;
                        }
                    }

                    // Direct3D 9 is only relevant on the primary device
                    if(m_adapterIndex < 0) { return; }

                    // Try to create the device
                    m_deviceEx = new D3D9.DeviceEx(m_direct3DEx, m_adapterIndex, D3D9.DeviceType.Hardware, IntPtr.Zero, createFlags, presentparams);
                }
                else
                {
                    //Not supported in software mode
                }
            }
            catch (Exception)
            {
                // No direct3d 9 interface support
                SeeingSharpTools.SafeDispose(ref m_direct3DEx);
                SeeingSharpTools.SafeDispose(ref m_deviceEx);
            }
        }

        /// <summary>
        /// Unloads all loaded resources.
        /// </summary>
        public void Dispose()
        {
            m_deviceEx = SeeingSharpTools.DisposeObject(m_deviceEx);
            m_direct3DEx = SeeingSharpTools.DisposeObject(m_direct3DEx);
        }

        /// <summary>
        /// Gets current desktop window.
        /// </summary>
        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr GetDesktopWindow();

        /// <summary>
        /// Is the device successfully initialized?
        /// </summary>
        public bool IsInitialized
        {
            get { return m_deviceEx != null; }
        }

        /// <summary>
        /// Gets the initialized device.
        /// </summary>
        internal D3D9.DeviceEx Device
        {
            get { return m_deviceEx; }
        }

        /// <summary>
        /// Gets current DirectX context.
        /// </summary>
        internal D3D9.Direct3DEx Context
        {
            get { return m_direct3DEx; }
        }
    }
}