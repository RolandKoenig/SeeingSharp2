using System.Drawing.Imaging;
using SeeingSharp.Checking;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using DXGI = Vortice.DXGI;
using D3D11 = Vortice.Direct3D11;
using GDI = System.Drawing;
using WinForms = System.Windows.Forms;

namespace SeeingSharp.Core
{
    public static class GraphicsHelperWinForms
    {
        /// <summary>
        /// Loads a bitmap from the given texture. Be careful: The texture must have CPU read access and this only matches for staging textures.
        /// </summary>
        /// <param name="texture">The texture to be loaded into the bitmap.</param>
        public static GDI.Bitmap LoadBitmapFromMemoryMappedTexture(MemoryMappedTexture<int> texture)
        {
            texture.EnsureNotNullOrDisposed(nameof(texture));

            var width = texture.Width;
            var height = texture.Height;

            // Create and lock bitmap so it can be accessed for texture loading
            var resultBitmap = new GDI.Bitmap(width, height);
            var bitmapData = resultBitmap.LockBits(
                new GDI.Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                SeeingSharpUtil.CopyMemory(
                    texture.Pointer, bitmapData.Scan0, texture.SizeInBytes);
            }
            finally
            {
                resultBitmap.UnlockBits(bitmapData);
            }

            return resultBitmap;
        }

        /// <summary>
        /// Creates a default SwapChain for the given target control.
        /// </summary>
        /// <param name="targetControl">Target control of the swap chain.</param>
        /// <param name="device">Graphics device.</param>
        /// <param name="gfxConfig">The current graphics configuration.</param>
        internal static DXGI.IDXGISwapChain1 CreateSwapChainForWinForms(WinForms.Control targetControl, EngineDevice device, GraphicsViewConfiguration gfxConfig)
        {
            targetControl.EnsureNotNull(nameof(targetControl));
            device.EnsureNotNull(nameof(device));
            gfxConfig.EnsureNotNull(nameof(gfxConfig));

            // Create the swap chain description
            var swapChainDesc = new DXGI.SwapChainDescription1();

            if (gfxConfig.AntialiasingEnabled && device.IsStandardAntialiasingPossible)
            {
                swapChainDesc.BufferCount = 2;
                swapChainDesc.SampleDescription = device.Internals.GetSampleDescription(gfxConfig.AntialiasingQuality);
            }
            else
            {
                swapChainDesc.BufferCount = 2;
                swapChainDesc.SampleDescription = new DXGI.SampleDescription(1, 0);
            }

            // Set common parameters
            swapChainDesc.Width = targetControl.Width;
            swapChainDesc.Height = targetControl.Height;
            swapChainDesc.Format = GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT;
            swapChainDesc.Scaling = DXGI.Scaling.Stretch;
            swapChainDesc.SwapEffect = DXGI.SwapEffect.Discard;
            swapChainDesc.Usage = DXGI.Usage.RenderTargetOutput;

            // Create and return the swap chain and the render target
            return device.Internals.FactoryDxgi.CreateSwapChainForHwnd(
                device.Internals.DeviceD3D11_1, targetControl.Handle,
                swapChainDesc);
        }

        /// <summary>
        /// Converts a System.Drawing.Bitmap to a DirectX 11 texture object.
        /// </summary>
        /// <param name="device">Device on which the resource should be created.</param>
        /// <param name="bitmap">The source bitmap.</param>
        internal static D3D11.ID3D11Texture2D LoadTextureFromBitmap(EngineDevice device, GDI.Bitmap bitmap)
        {
            device.EnsureNotNull(nameof(device));
            bitmap.EnsureNotNull(nameof(bitmap));

            return LoadTextureFromBitmap(device, bitmap, 1);
        }

        /// <summary>
        /// Converts a System.Drawing.Bitmap to a DirectX 11 texture object.
        /// </summary>
        /// <param name="device">Device on which the resource should be created.</param>
        /// <param name="bitmap">The source bitmap.</param>
        /// <param name="mipLevels">Total count of levels for mipmapping.</param>
        internal static D3D11.ID3D11Texture2D LoadTextureFromBitmap(EngineDevice device, GDI.Bitmap bitmap, int mipLevels)
        {
            device.EnsureNotNull(nameof(device));
            bitmap.EnsureNotNull(nameof(bitmap));
            mipLevels.EnsurePositiveOrZero(nameof(mipLevels));

            D3D11.ID3D11Texture2D result = null;

            // Lock bitmap so it can be accessed for texture loading
            var bitmapData = bitmap.LockBits(
                new GDI.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                // Open a reading stream for bitmap memory
                var dataRectangle = new D3D11.SubresourceData(bitmapData.Scan0, bitmap.Width * 4);

                // Load the texture
                result = device.Internals.DeviceD3D11_1.CreateTexture2D(
                    new D3D11.Texture2DDescription
                    {
                        BindFlags = D3D11.BindFlags.ShaderResource | D3D11.BindFlags.RenderTarget,
                        CpuAccessFlags = D3D11.CpuAccessFlags.None,
                        Format = GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT,
                        OptionFlags = D3D11.ResourceOptionFlags.None | D3D11.ResourceOptionFlags.GenerateMips,
                        MipLevels = 0,
                        Usage = D3D11.ResourceUsage.Default,
                        Width = bitmap.Width,
                        Height = bitmap.Height,
                        ArraySize = 1,
                        SampleDescription = new DXGI.SampleDescription(1, 0)
                    },
                    new D3D11.SubresourceData[]
                    {
                        dataRectangle, dataRectangle, dataRectangle, dataRectangle, 
                        dataRectangle, dataRectangle, dataRectangle, dataRectangle, 
                        dataRectangle, dataRectangle, dataRectangle, dataRectangle 
                    });

                // Workaround for now... auto generate mip-levels
                using var shaderResourceView = device.Internals.DeviceD3D11_1.CreateShaderResourceView(result);
                device.Internals.DeviceImmediateContextD3D11.GenerateMips(shaderResourceView);
            }
            finally
            {
                // Free bitmap-access resources
                bitmap.UnlockBits(bitmapData);
            }

            return result;
        }

        /// <summary>
        /// Loads a bitmap from the given texture. Be careful: The texture must have CPU read access and this only matches for staging textures.
        /// </summary>
        /// <param name="device">The device on which the texture is created.</param>
        /// <param name="stagingTexture">The texture to be loaded into the bitmap.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        internal static GDI.Bitmap LoadBitmapFromStagingTexture(EngineDevice device, D3D11.ID3D11Texture2D stagingTexture, int width, int height)
        {
            device.EnsureNotNull(nameof(device));
            stagingTexture.EnsureNotNull(nameof(stagingTexture));
            width.EnsurePositiveOrZero(nameof(width));
            height.EnsurePositiveOrZero(nameof(height));

            //Prepare target bitmap
            var resultBitmap = new GDI.Bitmap(width, height);
            var dataBox = device.Internals.DeviceImmediateContextD3D11.Map(stagingTexture, 0, D3D11.MapMode.Read, D3D11.MapFlags.None);

            try
            {
                //Lock bitmap so it can be accessed for texture loading
                var bitmapData = resultBitmap.LockBits(
                    new GDI.Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                try
                {
                    //Copy data row by row
                    // => Rows form data source may have more pixels because driver changes the size of textures
                    var rowPitch = (uint)(width * 4);

                    for (var loopRow = 0; loopRow < height; loopRow++)
                    {
                        // Copy bitmap data
                        var rowPitchSource = dataBox.RowPitch;
                        var rowPitchDestination = width * 4;
                        SeeingSharpUtil.CopyMemory(
                            dataBox.DataPointer + loopRow * rowPitchSource,
                            bitmapData.Scan0 + loopRow * rowPitchDestination,
                            rowPitch);
                    }
                }
                finally
                {
                    resultBitmap.UnlockBits(bitmapData);
                }
            }
            finally
            {
                device.Internals.DeviceImmediateContextD3D11.Unmap(stagingTexture, 0);
            }
            return resultBitmap;
        }
    }
}