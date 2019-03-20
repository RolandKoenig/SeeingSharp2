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
using System.Windows;
using System.Windows.Interop;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using D3D9 = SharpDX.Direct3D9;
using Resource = SharpDX.DXGI.Resource;

namespace SeeingSharp.Multimedia.Views
{
    public class HigherD3DImageSource : D3DImage, IDisposable
    {
        private static volatile int s_activeClients;
        private D3D9.Direct3DEx m_d3dContext;
        private D3D9.DeviceEx m_d3dDevice;
        private D3D9.Texture m_d3dRenderTarget;

        private EngineDevice m_device;

        /// <summary>
        /// Initializes a new instance of the <see cref="HigherD3DImageSource"/> class.
        /// </summary>
        public HigherD3DImageSource(EngineDevice device, DeviceHandlerD3D9 deviceHandlerD3D9)
        {
            m_device = device;

            m_d3dContext = deviceHandlerD3D9.Context;
            m_d3dDevice = deviceHandlerD3D9.Device;

            if(m_d3dDevice == null)
            {
                throw new SeeingSharpException("Unable to create Wpf image source: No Direct3D 9 device available on " + device);
            }

            s_activeClients++;
        }

        /// <summary>
        /// Invalidates the direct3D image.
        /// </summary>
        public void InvalidateD3DImage()
        {
            if (m_d3dRenderTarget != null)
            {
                this.AddDirtyRect(new Int32Rect(0, 0, this.PixelWidth, this.PixelHeight));
            }
        }

        /// <summary>
        /// Sets the render target of this D3DImage object.
        /// </summary>
        /// <param name="renderTarget">The render target to set.</param>
        public void SetRenderTarget(D3D11.Texture2D renderTarget)
        {
            if (m_d3dRenderTarget != null)
            {
                m_d3dRenderTarget = null;

                this.Lock();
                this.SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
                this.Unlock();
            }

            if (renderTarget == null) { return; }
            if (!IsShareable(renderTarget))
            {
                throw new ArgumentException("texture must be created with ResourceOptionFlags.Shared");
            }

            var format = TranslateFormat(renderTarget);

            if (format == D3D9.Format.Unknown)
            {
                throw new ArgumentException("texture format is not compatible with OpenSharedResource");
            }

            var handle = this.GetSharedHandle(renderTarget);

            if (handle == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(handle));
            }

            //Map the texture to the D3DImage base class
            var tDisposed = renderTarget.IsDisposed;
            float tWidth = renderTarget.Description.Width;
            float tHeight = renderTarget.Description.Height;
            m_d3dRenderTarget = new D3D9.Texture(
                m_d3dDevice,
                renderTarget.Description.Width,
                renderTarget.Description.Height,
                1, D3D9.Usage.RenderTarget, format, D3D9.Pool.Default, ref handle);

            using (var surface = m_d3dRenderTarget.GetSurfaceLevel(0))
            {
                this.Lock();
                this.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
                this.Unlock();
            }
        }

        /// <summary>
        /// Disposes this object.
        /// </summary>
        public void Dispose()
        {
            this.SetRenderTarget(null);

            m_d3dRenderTarget = SeeingSharpUtil.DisposeObject(m_d3dRenderTarget);
            s_activeClients--;
        }

        /// <summary>
        /// Gets the handle that can be used for resource sharing.
        /// </summary>
        /// <param name="texture">The texture to be shared.</param>
        private IntPtr GetSharedHandle(D3D11.Texture2D texture)
        {
            texture.EnsureNotNull(nameof(texture));

            using (var resource = texture.QueryInterface<Resource>())
            {
                return resource.SharedHandle;
            }
        }

        /// <summary>
        /// Gets the format for sharing.
        /// </summary>
        /// <param name="texture">The texture to get the format for.</param>
        private static D3D9.Format TranslateFormat(D3D11.Texture2D texture)
        {
            switch (texture.Description.Format)
            {
                case Format.R10G10B10A2_UNorm:
                    return D3D9.Format.A2B10G10R10;

                case Format.R16G16B16A16_Float:
                    return D3D9.Format.A16B16G16R16F;

                case Format.B8G8R8A8_UNorm:
                    return D3D9.Format.A8R8G8B8;

                default:
                    return D3D9.Format.Unknown;
            }
        }

        /// <summary>
        /// Is the given texture sharable?
        /// </summary>
        /// <param name="textureToCheck">The checker to check.</param>
        private static bool IsShareable(D3D11.Texture2D textureToCheck)
        {
            return (textureToCheck.Description.OptionFlags & D3D11.ResourceOptionFlags.Shared) != 0;
        }

        public bool HasRenderTarget => m_d3dRenderTarget != null;
    }
}