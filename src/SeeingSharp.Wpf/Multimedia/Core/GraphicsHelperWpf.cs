using System;
using System.Windows;
using System.Windows.Media.Imaging;
using SeeingSharp.Checking;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Core
{
    public static class GraphicsHelperWpf
    {
        public static void UniformRescale(ref int width, ref int height, float maxDimension)
        {
            var biggerValue = width;
            if (height > biggerValue) { biggerValue = height; }

            float biggerValueF = biggerValue;
            if (biggerValueF <= maxDimension) { return; }

            var scaleFactor = maxDimension / biggerValueF;
            width = (int)(width * scaleFactor);
            height = (int)(height * scaleFactor);
        }

        /// <summary>
        /// Loads a bitmap from the given texture. Be careful: The texture must have CPU read access and this only matches for staging textures.
        /// </summary>
        /// <param name="device">The device on which the texture is created.</param>
        /// <param name="stagingTexture">The texture to be loaded into the bitmap.</param>
        /// <param name="targetBitmap">The target bitmap to write all contents to.</param>
        /// <param name="pixelWidth">With of the bitmap in pixels.</param>
        /// <param name="pixelHeight">Height of the bitmap in pixels.</param>
        /// <param name="lockTimeout">Timeout for locking the target bitmap.</param>
        internal static void LoadBitmapFromStagingTexture(EngineDevice device, D3D11.Texture2D stagingTexture, WriteableBitmap targetBitmap, int pixelWidth, int pixelHeight, TimeSpan lockTimeout)
        {
            device.EnsureNotNull(nameof(device));
            stagingTexture.EnsureNotNull(nameof(stagingTexture));
            targetBitmap.EnsureNotNull(nameof(targetBitmap));

            var textureDesc = stagingTexture.Description;
            pixelWidth.EnsureEqualComparable(textureDesc.Width, $"{nameof(textureDesc)}.{nameof(textureDesc.Width)}");
            pixelHeight.EnsureEqualComparable(textureDesc.Height, $"{nameof(textureDesc)}.{nameof(textureDesc.Height)}");

            // Prepare target bitmap
            var dataBox = device.Internals.DeviceImmediateContextD3D11.MapSubresource(stagingTexture, 0, D3D11.MapMode.Read, D3D11.MapFlags.None);
            try
            {
                if (!targetBitmap.TryLock(new Duration(lockTimeout)))
                {
                    return;
                }

                try
                {
                    // Copy data row by row
                    //  => Rows from data source may have more pixels because driver changes the size of textures
                    var rowPitch = (uint) (pixelWidth * 4);
                    for (var loopRow = 0; loopRow < pixelHeight; loopRow++)
                    {
                        var rowPitchSource = dataBox.RowPitch;
                        var rowPitchDestination = pixelWidth * 4;
                        SeeingSharpUtil.CopyMemory(
                            dataBox.DataPointer + loopRow * rowPitchSource,
                            targetBitmap.BackBuffer + loopRow * rowPitchDestination,
                            rowPitch);
                    }
                }
                finally
                {
                    targetBitmap.AddDirtyRect(new Int32Rect(0, 0, pixelWidth, pixelHeight));
                    targetBitmap.Unlock();
                }
            }
            finally
            {
                device.Internals.DeviceImmediateContextD3D11.UnmapSubresource(stagingTexture, 0);
            }
        }
    }
}
