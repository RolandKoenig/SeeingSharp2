﻿#region License information
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

// Some namespace mappings
using D3D11 = SharpDX.Direct3D11;
using D2D = SharpDX.Direct2D1;

#endregion

namespace SeeingSharp.Multimedia.Core
{
    #region using

    using System;
    using Drawing2D;
    using SeeingSharp.Util;
    using SharpDX;

    #endregion

    /// <summary>
    /// This class makes a Direct3D 11 texture available for 2D rendering with Direct2D.
    /// </summary>
    class Direct2DOverlayRenderer : IDisposable
    {
        private static readonly NamedOrGenericKey RES_KEY_FALLBACK_TEXTURE = GraphicsCore.GetNextGenericResourceKey();

        #region Graphics object
        private Graphics2D m_graphics2D;
        #endregion

        #region Given resources
        private EngineDevice m_device;
        private D3D11.Texture2D m_renderTarget3D;
        #endregion

        #region Own 2D render target resource
        private D2D.RenderTarget m_renderTarget2D;
        private D2D.Bitmap1 m_renderTargetBitmap;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Direct2DOverlayRenderer"/> class.
        /// </summary>
        public Direct2DOverlayRenderer(EngineDevice device, D3D11.Texture2D renderTarget3D, int viewWidth, int viewHeight, DpiScaling dpiScaling)
            : this(device, renderTarget3D, viewWidth, viewHeight, dpiScaling, false)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Direct2DOverlayRenderer"/> class.
        /// </summary>
        internal Direct2DOverlayRenderer(EngineDevice device, D3D11.Texture2D renderTarget3D, int viewWidth, int viewHeight, DpiScaling dpiScaling, bool forceInit)
        {
            m_device = device;
            m_renderTarget3D = renderTarget3D;

            CreateResources(viewWidth, viewHeight, dpiScaling, forceInit);
        }

        /// <summary>
        /// Disposes all resources of this object completely.
        /// </summary>
        public void Dispose()
        {
            // Dispose all created objects
            if (m_renderTarget2D != null)
            {
                SeeingSharpTools.SafeDispose(ref m_renderTargetBitmap);
                m_renderTarget2D = null;
            }
            else
            {
                SeeingSharpTools.SafeDispose(ref m_renderTarget2D);
            }
        }

        /// <summary>
        /// Creates all resources
        /// </summary>
        private void CreateResources(int viewWidth, int viewHeight, DpiScaling dpiScaling, bool forceInit)
        {
            // Calculate the screen size in device independent units
            var scaledScreenSize = new Size2F(
                (float)viewWidth / dpiScaling.ScaleFactorX,
                (float)viewHeight / dpiScaling.ScaleFactorY);

            // Cancel here if the device does not support 2D rendering
            if ((!forceInit) &&
               (!m_device.Supports2D))
            {
                return;
            }

            // Create the render target
            using (var dxgiSurface = m_renderTarget3D.QueryInterface<SharpDX.DXGI.Surface>())
            {
                var bitmapProperties = new D2D.BitmapProperties1
                {
                    DpiX = dpiScaling.DpiX,
                    DpiY = dpiScaling.DpiY,
                    BitmapOptions = D2D.BitmapOptions.Target | D2D.BitmapOptions.CannotDraw,
                    PixelFormat = new D2D.PixelFormat(GraphicsHelper.DEFAULT_TEXTURE_FORMAT,
                        D2D.AlphaMode.Premultiplied)
                };

                m_renderTargetBitmap = new SharpDX.Direct2D1.Bitmap1(m_device.DeviceContextD2D, dxgiSurface, bitmapProperties);
                m_renderTarget2D = m_device.DeviceContextD2D;
                m_graphics2D = new Graphics2D(m_device, m_device.DeviceContextD2D, scaledScreenSize);
            }
        }

        /// <summary>
        /// Begins the draw.
        /// </summary>
        public void BeginDraw()
        {
            if (m_renderTarget2D == null) { return; }

            m_device.DeviceContextD2D.Target = m_renderTargetBitmap;
            m_device.DeviceContextD2D.DotsPerInch = m_renderTargetBitmap.DotsPerInch;

            // Start Direct2D rendering
            m_renderTarget2D.BeginDraw();
        }

        /// <summary>
        /// Finishes Direct2D drawing.
        /// </summary>
        public void EndDraw()
        {
            if (m_renderTarget2D == null) { return; }

            m_renderTarget2D.EndDraw();

            // Finish Direct2D drawing
            m_device.DeviceContextD2D.Target = null;
        }

        /// <summary>
        /// Is this resource loaded correctly?
        /// </summary>
        public bool IsLoaded
        {
            get { return m_renderTarget2D != null; }
        }

        /// <summary>
        /// Gets the Direct2D render target.
        /// </summary>
        internal D2D.RenderTarget RenderTarget2D
        {
            get { return m_renderTarget2D; }
        }

        /// <summary>
        /// Gets the 2D graphics object.
        /// </summary>
        internal Graphics2D Graphics
        {
            get { return m_graphics2D; }
        }
    }
}