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

using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Util;
using SharpDX;
using D3D11 = SharpDX.Direct3D11;
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class Direct2DTextureResource : TextureResource, IRenderableResource
    {
        private Custom2DDrawingLayer m_drawingLayer;
        private int m_width;
        private int m_height;

        // Resources for Direct3D
        private D3D11.Texture2D m_renderTargetTexture;
        private D3D11.ShaderResourceView m_renderTargetTextureView;

        // Resourcs for Direct2D
        private Graphics2D m_graphics2D;
        private Direct2DOverlayRenderer m_overlayRenderer;

        /// <summary>
        /// Loads all resource.
        /// </summary>
        /// <param name="device">The device on which to load all resources.</param>
        /// <param name="resources">The current ResourceDictionary.</param>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            m_renderTargetTexture = GraphicsHelper.CreateRenderTargetTexture(
                device, m_width, m_height, new GraphicsViewConfiguration { AntialiasingEnabled = false });
            m_renderTargetTextureView = new D3D11.ShaderResourceView(device.DeviceD3D11_1, m_renderTargetTexture);

            m_overlayRenderer = new Direct2DOverlayRenderer(
                device,
                m_renderTargetTexture,
                m_width, m_height,
                DpiScaling.Default);
            m_graphics2D = new Graphics2D(device, m_overlayRenderer.RenderTarget2D, new Size2F(m_width, m_height));
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        /// <param name="device">The device on which the resources where loaded.</param>
        /// <param name="resources">The current ResourceDictionary.</param>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            m_graphics2D = null;
            SeeingSharpTools.SafeDispose(ref m_overlayRenderer);
            SeeingSharpTools.SafeDispose(ref m_renderTargetTextureView);
            SeeingSharpTools.SafeDispose(ref m_renderTargetTexture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Direct2DTextureResource"/> class.
        /// </summary>
        /// <param name="drawingLayer">The drawing layer.</param>
        /// <param name="height">The width of the generated texture.</param>
        /// <param name="width">The height of the generated texture.</param>
        public Direct2DTextureResource(Custom2DDrawingLayer drawingLayer, int width, int height)
        {
            drawingLayer.EnsureNotNull(nameof(drawingLayer));
            width.EnsurePositive(nameof(width));
            height.EnsurePositive(nameof(height));

            m_drawingLayer = drawingLayer;
            m_width = width;
            m_height = height;
        }

        /// <summary>
        /// Triggers internal update within the resource (e. g. Render to Texture).
        /// </summary>
        /// <param name="updateState">Current state of update process.</param>
        public void Update(SceneRelatedUpdateState updateState)
        {

        }

        /// <summary>
        /// Triggers internal rendering within the resource (e. g. Render to Texture).
        /// </summary>
        /// <param name="renderState">Current render state.</param>
        public void Render(RenderState renderState)
        {
            m_overlayRenderer.BeginDraw();
            try
            {
                if (m_graphics2D != null)
                {
                    m_drawingLayer.Draw2DInternal(m_graphics2D);
                }
            }
            finally
            {
                m_overlayRenderer.EndDraw();
            }
        }

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => m_graphics2D != null;

        /// <summary>
        /// Gets the texture object.
        /// </summary>
        public override D3D11.Texture2D Texture => m_renderTargetTexture;

        /// <summary>
        /// Gets a ShaderResourceView targeting the texture.
        /// </summary>
        public override D3D11.ShaderResourceView TextureView => m_renderTargetTextureView;

        /// <summary>
        /// Gets the size of the texture array.
        /// 1 for normal textures.
        /// 6 for cubemap textures.
        /// </summary>
        public override int ArraySize => 1;
    }
}
