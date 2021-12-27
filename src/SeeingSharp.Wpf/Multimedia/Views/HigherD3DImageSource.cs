using System;
using System.Windows;
using System.Windows.Interop;
using SeeingSharp.Checking;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using Vortice.DXGI;
using D3D11 = Vortice.Direct3D11;
using D3D9 = Vortice.Direct3D9;
using DXGI = Vortice.DXGI;

namespace SeeingSharp.Views
{
    public class HigherD3DImageSource : D3DImage, IDisposable
    {
        private D3D9.IDirect3DDevice9Ex _d3dDevice;
        private D3D9.IDirect3DTexture9 _d3dRenderTarget;

        public bool HasRenderTarget => _d3dRenderTarget != null;

        /// <summary>
        /// Initializes a new instance of the <see cref="HigherD3DImageSource"/> class.
        /// </summary>
        public HigherD3DImageSource(EngineDevice device, DeviceHandlerD3D9 deviceHandlerD3D9)
        {
            _d3dDevice = deviceHandlerD3D9.Device;

            if (_d3dDevice == null)
            {
                throw new SeeingSharpException("Unable to create Wpf image source: No Direct3D 9 device available on " + device);
            }
        }

        /// <summary>
        /// Invalidates the direct3D image.
        /// </summary>
        public void InvalidateD3DImage()
        {
            if (_d3dRenderTarget != null)
            {
                this.AddDirtyRect(new Int32Rect(0, 0, this.PixelWidth, this.PixelHeight));
            }
        }

        /// <summary>
        /// Sets the render target of this D3DImage object.
        /// </summary>
        /// <param name="renderTarget">The render target to set.</param>
        public void SetRenderTarget(D3D11.ID3D11Texture2D renderTarget)
        {
            if (_d3dRenderTarget != null)
            {
                _d3dRenderTarget = null;

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
            _d3dRenderTarget = _d3dDevice.CreateTexture(
                renderTarget.Description.Width,
                renderTarget.Description.Height,
                1, D3D9.Usage.RenderTarget, format, D3D9.Pool.Default, ref handle);

            using (var surface = _d3dRenderTarget.GetSurfaceLevel(0))
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

            _d3dRenderTarget = SeeingSharpUtil.DisposeObject(_d3dRenderTarget);
        }

        /// <summary>
        /// Gets the handle that can be used for resource sharing.
        /// </summary>
        /// <param name="texture">The texture to be shared.</param>
        private IntPtr GetSharedHandle(D3D11.ID3D11Texture2D texture)
        {
            texture.EnsureNotNull(nameof(texture));

            using (var resource = texture.QueryInterface<DXGI.IDXGIResource>())
            {
                return resource.SharedHandle;
            }
        }

        /// <summary>
        /// Gets the format for sharing.
        /// </summary>
        /// <param name="texture">The texture to get the format for.</param>
        private static D3D9.Format TranslateFormat(D3D11.ID3D11Texture2D texture)
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
        private static bool IsShareable(D3D11.ID3D11Texture2D textureToCheck)
        {
            return (textureToCheck.Description.OptionFlags & D3D11.ResourceOptionFlags.Shared) != 0;
        }
    }
}