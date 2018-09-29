using SeeingSharp.Multimedia.Core;
using SeeingSharp.Checking;
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SharpDX;

// Namespace mappings
using GDI = System.Drawing;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Core
{
    public static class GraphicsHelperWpf
    {
        public static void UniformRescale(ref int width, ref int height, float maxDimension)
        {
            int biggerValue = width;
            if(height > biggerValue) { biggerValue = height; }

            float biggerValueF = biggerValue;
            if(biggerValueF <= maxDimension) { return; }

            float scaleFactor = maxDimension / biggerValueF;
            width = (int)((float)width * scaleFactor);
            height = (int)((float)height * scaleFactor);
        }

        /// <summary>
        /// Loads a bitmap from the given texture. Be careful: The texture musst have CPU read access and this only matches for staging textures.
        /// </summary>
        /// <param name="device">The device on which the texture is created.</param>
        /// <param name="stagingTexture">The texture to be loaded into the bitmap.</param>
        /// <param name="targetBitmap">The target bitmap to write all contents to.</param>
        /// <param name="lockTimeout">Timeout for locking the target bitmap.</param>
        internal static void LoadBitmapFromStagingTexture(EngineDevice device, D3D11.Texture2D stagingTexture, WriteableBitmap targetBitmap, TimeSpan lockTimeout)
        {
            device.EnsureNotNull(nameof(device));
            stagingTexture.EnsureNotNull(nameof(stagingTexture));
            targetBitmap.EnsureNotNull(nameof(targetBitmap));

            var width = targetBitmap.PixelWidth;
            var height = targetBitmap.PixelHeight;
            var textureDesc = stagingTexture.Description;
            width.EnsureEqualComparable(textureDesc.Width, $"{nameof(textureDesc)}.{nameof(textureDesc.Width)}");
            height.EnsureEqualComparable(textureDesc.Height, $"{nameof(textureDesc)}.{nameof(textureDesc.Height)}");

            //Prepare target bitmap 
            SharpDX.DataBox dataBox = device.DeviceImmediateContextD3D11.MapSubresource(stagingTexture, 0, D3D11.MapMode.Read, D3D11.MapFlags.None);
            try
            {
                targetBitmap.TryLock(new System.Windows.Duration(lockTimeout));
                try
                {
                    //Copy data row by row
                    // => Rows form datasource may have more pixels because driver changes the size of textures
                    ulong rowPitch = (ulong)(width * 4);
                    for (int loopRow = 0; loopRow < height; loopRow++)
                    {
                        // Copy bitmap data
                        targetBitmap.WritePixels(
                            new System.Windows.Int32Rect(0, 0, width, height),
                            dataBox.DataPointer,
                            dataBox.SlicePitch,
                            dataBox.RowPitch,
                            0, 0);
                    }
                }
                finally
                {
                    targetBitmap.Unlock();
                }
            }
            finally
            {
                device.DeviceImmediateContextD3D11.UnmapSubresource(stagingTexture, 0);
            }
        }
    }
}
