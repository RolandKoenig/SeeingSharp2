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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using SharpDX;
using SharpDX.DXGI;
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public class WriteableBitmapResource : BitmapResource
    {
        // Configuration
        private Size2 m_bitmapSize;
        private D2D.PixelFormat m_pixelFormat;
        private double m_dpiX;
        private double m_dpiY;

        // Resources
        private D2D.Bitmap[] m_loadedBitmaps;

        /// <summary>
        /// Initializes a new instance of the <see cref="WriteableBitmapResource"/> class.
        /// </summary>
        public WriteableBitmapResource(
            Size2 bitmapSize,
            BitmapFormat format = BitmapFormat.Bgra,
            AlphaMode alphaMode = AlphaMode.Straight,
            double dpiX = 96.0, double dpiY = 96.0)
        {
            m_loadedBitmaps = new D2D.Bitmap[GraphicsCore.Current.DeviceCount];
            m_bitmapSize = bitmapSize;
            m_pixelFormat = new D2D.PixelFormat(
                (Format)format,
                (D2D.AlphaMode)alphaMode);
            m_dpiX = dpiX;
            m_dpiY = dpiY;
        }

        /// <summary>
        /// Sets the bitmap's contents.
        /// </summary>
        /// <param name="graphics">The graphics object.</param>
        /// <param name="pointer">The pointer to the source data.</param>
        /// <param name="pitch"></param>
        public void SetBitmapContent(Graphics2D graphics, IntPtr pointer, int pitch)
        {
            var bitmap = GetBitmap(graphics.Device);
            bitmap.CopyFromMemory(pointer, pitch);
        }

        /// <summary>
        /// Gets the bitmap for the given device..
        /// </summary>
        /// <param name="engineDevice">The engine device.</param>
        internal override D2D.Bitmap GetBitmap(EngineDevice engineDevice)
        {
            // Check for disposed state
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            var result = m_loadedBitmaps[engineDevice.DeviceIndex];

            if (result == null)
            {
                // Load the bitmap initially
                result = new D2D.Bitmap(
                    engineDevice.FakeRenderTarget2D,
                    m_bitmapSize,
                    new D2D.BitmapProperties(m_pixelFormat, (float)m_dpiX, (float)m_dpiY));
                m_loadedBitmaps[engineDevice.DeviceIndex] = result;
            }

            return result;
        }

        /// <summary>
        /// Unloads all resources loaded on the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to unload the resource.</param>
        internal override void UnloadResources(EngineDevice engineDevice)
        {
            var brush = m_loadedBitmaps[engineDevice.DeviceIndex];

            if (brush != null)
            {
                SeeingSharpUtil.DisposeObject(brush);
                m_loadedBitmaps[engineDevice.DeviceIndex] = null;
            }
        }

        public override int PixelWidth => m_bitmapSize.Width;

        public override int PixelHeight => m_bitmapSize.Height;

        public override double DpiX => m_dpiX;

        public override double DpiY => m_dpiY;

        public override int FrameCountX => 1;

        public override int FrameCountY => 1;

        public override int TotalFrameCount => 1;

        public override int SingleFramePixelWidth => m_bitmapSize.Width;

        public override int SingleFramePixelHeight => m_bitmapSize.Height;
    }
}
