#region License information
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
#endregion
#region using

// Namespace mappings
using D2D = SharpDX.Direct2D1;

#endregion

namespace SeeingSharp.Multimedia.Drawing2D
{
    #region using

    using System;
    using System.IO;
    using Checking;
    using Core;
    using SeeingSharp.Util;

    #endregion

    /// <summary>
    /// This object represents a inmemory chached bitmap which is
    /// loaded from a ResourceLink (e. g. a file).
    /// </summary>
    public class StandardBitmapResource : BitmapResource
    {
        #region Resources
        private D2D.Bitmap[] m_loadedBitmaps;
        #endregion

        #region Configuration
        private ResourceLink m_resourceLink;
        private int m_framesX;
        private int m_framesY;
        private int m_totalFrameCount;
        #endregion

        #region RuntimeValues
        private bool m_firstLoadDone;
        private int m_framePixelWidth;
        private int m_framePixelHeight;
        private int m_pixelWidth;
        private int m_pixelHeight;
        private double m_dpiX;
        private double m_dpyY;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardBitmapResource"/> class.
        /// </summary>
        /// <param name="resource">The resource from w.</param>
        /// <param name="frameCountX">Total count of frames in x direction.</param>
        /// <param name="frameCountY">Total count of frames in y direction.</param>
        public StandardBitmapResource(ResourceLink resource, int frameCountX = 1, int frameCountY = 1)
        {
            frameCountX.EnsurePositiveAndNotZero(nameof(frameCountX));
            frameCountY.EnsurePositiveAndNotZero(nameof(frameCountY));

            m_loadedBitmaps = new D2D.Bitmap[GraphicsCore.Current.DeviceCount];
            m_resourceLink = resource;

            // Set default values (modified after first load)
            m_firstLoadDone = false;
            m_pixelWidth = 0;
            m_pixelHeight = 0;
            m_dpiX = 96.0;
            m_dpyY = 96.0;
            m_framesX = frameCountX;
            m_framesY = frameCountY;
            m_totalFrameCount = m_framesX * m_framesY;
        }

        public override string ToString()
        {
            return string.Format("Bitmap ({0}x{1} pixels)", m_pixelWidth, m_pixelHeight);
        }

        /// <summary>
        /// Gets the bitmap.
        /// </summary>
        /// <param name="engineDevice">The engine device.</param>
        internal override D2D.Bitmap GetBitmap(EngineDevice engineDevice)
        {
            if (base.IsDisposed) { throw new ObjectDisposedException(this.GetType().Name); }

            var result = m_loadedBitmaps[engineDevice.DeviceIndex];

            if (result == null)
            {
                using (var inputStream = m_resourceLink.OpenInputStream())
                using (var bitmapSourceWrapper = GraphicsHelper.LoadBitmapSource_D2D(inputStream))
                {
                    SharpDX.WIC.BitmapSource bitmapSource = bitmapSourceWrapper.Converter;

                    // Store common properties about the bitmap
                    if (!m_firstLoadDone)
                    {
                        m_firstLoadDone = true;
                        m_pixelWidth = bitmapSource.Size.Width;
                        m_pixelHeight = bitmapSource.Size.Height;
                        if(m_totalFrameCount > 1)
                        {
                            m_framePixelWidth = m_pixelWidth / m_framesX;
                            m_framePixelHeight = m_pixelHeight / m_framesY;
                        }
                        else
                        {
                            m_framePixelWidth = m_pixelWidth;
                            m_framePixelHeight = m_pixelHeight;
                        }
                        bitmapSource.GetResolution(out m_dpiX, out m_dpyY);
                    }

                    // Load the bitmap into Direct2D
                    result = D2D.Bitmap.FromWicBitmap(
                        engineDevice.FakeRenderTarget2D, bitmapSource,
                        new D2D.BitmapProperties(new D2D.PixelFormat(
                            SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                            D2D.AlphaMode.Premultiplied)));

                    // Register loaded bitmap
                    m_loadedBitmaps[engineDevice.DeviceIndex] = result;
                }
            }

            return result;
        }

        /// <summary>
        /// Disposes this object.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
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
                SeeingSharpTools.DisposeObject(brush);
                m_loadedBitmaps[engineDevice.DeviceIndex] = null;
            }
        }

        /// <summary>
        /// Gets the width of the bitmap in pixel´.
        /// </summary>
        public override int PixelWidth
        {
            get { return m_pixelWidth; }
        }

        /// <summary>
        /// Gets the height of the bitmap in pixel.
        /// </summary>
        public override int PixelHeight
        {
            get { return m_pixelHeight; }
        }

        public override double DpiX
        {
            get
            {
                return m_dpiX;
            }
        }

        public override double DpiY
        {
            get
            {
                return m_dpyY;
            }
        }

        public override int FrameCountX
        {
            get{ return m_framesX; }
        }

        public override int FrameCountY
        {
            get
            {
                return m_framesY;
            }
        }

        public override int TotalFrameCount
        {
            get
            {
                return m_totalFrameCount;
            }
        }

        public override int SingleFramePixelWidth
        {
            get { return m_framePixelWidth; }
        }

        public override int SingleFramePixelHeight
        {
            get { return m_framePixelHeight; }
        }
    }
}
