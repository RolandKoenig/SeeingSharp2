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
using System.Windows.Media.Imaging;
using SeeingSharp.Checking;
using SeeingSharp.Util;
using GDI = System.Drawing;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Core
{
    public static class GraphicsHelperWpf
    {
        public static void UniformRescale(ref int width, ref int height, float maxDimension)
        {
            var biggerValue = width;
            if(height > biggerValue) { biggerValue = height; }

            float biggerValueF = biggerValue;
            if(biggerValueF <= maxDimension) { return; }

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

            // Prepare target bitmap
            var dataBox = device.DeviceImmediateContextD3D11.MapSubresource(stagingTexture, 0, D3D11.MapMode.Read, D3D11.MapFlags.None);

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
                    var rowPitch = (ulong)(width * 4);

                    for (var loopRow = 0; loopRow < height; loopRow++)
                    {
                        var rowPitchSource = dataBox.RowPitch;
                        var rowPitchDestination = width * 4;
                        SeeingSharpUtil.CopyMemory(
                            dataBox.DataPointer + loopRow * rowPitchSource,
                            targetBitmap.BackBuffer + loopRow * rowPitchDestination,
                            rowPitch);
                    }
                }
                finally
                {
                    targetBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
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
