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

// Some namespace mappings
using D3D11 = SharpDX.Direct3D11;

#endregion

namespace SeeingSharp.Multimedia.Drawing3D
{
    #region using

    using Core;
    using SeeingSharp.Util;
    using SharpDX;

    #endregion

    internal class RenderTargetTextureResource : TextureResource
    {
        #region Configuration
        private RenderTargetCreationMode m_creationMode;
        private int m_width;
        private int m_heigth;
        private bool m_antialiasingEnabled;
        private AntialiasingQualityLevel m_antialiasingQuality;
        private SharpDX.Mathematics.Interop.RawViewportF m_viewportF;
        #endregion

        #region Resources for depth buffer
        private D3D11.Texture2D m_depthBuffer;
        private D3D11.DepthStencilView m_depthBufferView;
        #endregion

        #region Resources for color buffer
        private D3D11.Texture2D m_colorBuffer;
        private D3D11.RenderTargetView m_colorBufferRenderTargetView;
        private D3D11.Texture2D m_colorBufferShaderResource;
        private D3D11.ShaderResourceView m_colorBufferShaderResourceView;
        #endregion

        #region Resources for ObjectID buffer
        private D3D11.Texture2D m_objectIDBuffer;
        private D3D11.RenderTargetView m_objectIDBufferRenderTargetView;
        #endregion

        #region Resources for normal/depth buffer
        private D3D11.Texture2D m_normalDepthBuffer;
        private D3D11.RenderTargetView m_normalDepthBufferRenderTargetView;
        private D3D11.Texture2D m_normalDepthBufferShaderResource;
        private D3D11.ShaderResourceView m_normalDepthBufferShaderResourceView;
        #endregion

        #region Runtime variables
        private bool m_shaderResourceCreated;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetTextureResource" /> class.
        /// </summary>
        /// <param name="creationMode">Tells this object which texture to create.</param>
        public RenderTargetTextureResource(RenderTargetCreationMode creationMode)
            : base()
        {
            m_creationMode = creationMode;
            m_width = -1;
            m_heigth = -1;
            m_viewportF = new SharpDX.Mathematics.Interop.RawViewportF();
            m_shaderResourceCreated = false;
        }

        /// <summary>
        /// Applies the given size.
        /// </summary>
        /// <param name="renderState">The render state used for creating all resources.</param>
        public void ApplySize(RenderState renderState)
        {
            var viewInfo = renderState.ViewInformation;
            var viewConfig = viewInfo.ViewConfiguration;

            // Get current view size and antialiasing settings
            var currentViewSize = viewInfo.CurrentViewSize;
            bool currentAntialiasingEnabled = viewConfig.AntialiasingEnabled;
            var currentAntialiasingQuality = viewConfig.AntialiasingQuality;

            if ((m_width != currentViewSize.Width) ||
                (m_heigth != currentViewSize.Height) ||
                (m_antialiasingEnabled != currentAntialiasingEnabled) ||
                (m_antialiasingQuality != currentAntialiasingQuality))
            {
                // Dispose color-buffer resources
                SeeingSharpTools.SafeDispose(ref m_colorBuffer);
                SeeingSharpTools.SafeDispose(ref m_colorBufferRenderTargetView);
                SeeingSharpTools.SafeDispose(ref m_colorBufferShaderResourceView);
                if (m_shaderResourceCreated) { SeeingSharpTools.SafeDispose(ref m_colorBufferShaderResource); }

                // Dispose depth-buffer resources
                SeeingSharpTools.SafeDispose(ref m_depthBufferView);
                SeeingSharpTools.SafeDispose(ref m_depthBuffer);

                // Dispose object-id buffer
                SeeingSharpTools.SafeDispose(ref m_objectIDBufferRenderTargetView);
                SeeingSharpTools.SafeDispose(ref m_objectIDBuffer);

                // Dispose normal-depth resources
                SeeingSharpTools.SafeDispose(ref m_normalDepthBuffer);
                SeeingSharpTools.SafeDispose(ref m_normalDepthBufferRenderTargetView);
                SeeingSharpTools.SafeDispose(ref m_normalDepthBufferShaderResourceView);
                if (m_shaderResourceCreated) { SeeingSharpTools.SafeDispose(ref m_normalDepthBufferShaderResource); }

                // Create color-buffer resources
                if (m_creationMode.HasFlag(RenderTargetCreationMode.Color))
                {
                    m_colorBuffer = GraphicsHelper.CreateRenderTargetTexture(
                        renderState.Device, currentViewSize.Width, currentViewSize.Height, renderState.ViewInformation.ViewConfiguration);
                    m_colorBufferShaderResource = m_colorBuffer;
                    if (renderState.ViewInformation.ViewConfiguration.AntialiasingEnabled)
                    {
                        m_colorBufferShaderResource = GraphicsHelper.CreateTexture(renderState.Device, currentViewSize.Width, currentViewSize.Height);
                        m_shaderResourceCreated = true;
                    }
                    else
                    {
                        m_shaderResourceCreated = false;
                    }
                    m_colorBufferRenderTargetView = new D3D11.RenderTargetView(renderState.Device.DeviceD3D11_1, m_colorBuffer);
                    m_colorBufferShaderResourceView = new D3D11.ShaderResourceView(renderState.Device.DeviceD3D11_1, m_colorBufferShaderResource);
                }

                // Create depth-buffer resources
                if (m_creationMode.HasFlag(RenderTargetCreationMode.Depth))
                {
                    m_depthBuffer = GraphicsHelper.CreateDepthBufferTexture(
                        renderState.Device, currentViewSize.Width, currentViewSize.Height, renderState.ViewInformation.ViewConfiguration);
                    m_depthBufferView = GraphicsHelper.CreateDepthBufferView(renderState.Device, m_depthBuffer);
                }

                // Create object-id resources
                if (m_creationMode.HasFlag(RenderTargetCreationMode.ObjectID))
                {
                    m_objectIDBuffer = GraphicsHelper.CreateRenderTargetTextureObjectIDs(
                        renderState.Device, currentViewSize.Width, currentViewSize.Height, renderState.ViewInformation.ViewConfiguration);
                    m_objectIDBufferRenderTargetView = new D3D11.RenderTargetView(renderState.Device.DeviceD3D11_1, m_objectIDBuffer);
                }

                // Create normal-depth buffer resources
                if (m_creationMode.HasFlag(RenderTargetCreationMode.NormalDepth))
                {
                    m_normalDepthBuffer = GraphicsHelper.CreateRenderTargetTextureNormalDepth(
                        renderState.Device, currentViewSize.Width, currentViewSize.Height, renderState.ViewInformation.ViewConfiguration);
                    m_normalDepthBufferShaderResource = m_normalDepthBuffer;
                    if (m_shaderResourceCreated)
                    {
                        m_normalDepthBufferShaderResource = GraphicsHelper.CreateTexture(
                            renderState.Device, currentViewSize.Width, currentViewSize.Height, GraphicsHelper.DEFAULT_TEXTURE_FORMAT_NORMAL_DEPTH);
                    }
                    m_normalDepthBufferRenderTargetView = new D3D11.RenderTargetView(renderState.Device.DeviceD3D11_1, m_normalDepthBuffer);
                    m_normalDepthBufferShaderResourceView = new D3D11.ShaderResourceView(renderState.Device.DeviceD3D11_1, m_normalDepthBufferShaderResource);
                }

                // Remember values
                m_width = currentViewSize.Width;
                m_heigth = currentViewSize.Height;
                m_antialiasingEnabled = currentAntialiasingEnabled;
                m_antialiasingQuality = currentAntialiasingQuality;
                m_viewportF = renderState.Viewport;
            }
        }

        /// <summary>
        /// Pushes this render target on the given render state.
        /// </summary>
        /// <param name="renderState">The render state to push to.</param>
        /// <param name="mode"></param>
        internal void PushOnRenderState(RenderState renderState, PushRenderTargetMode mode)
        {
            // Store RenderTargets structures
            var prevRenderTargets = renderState.CurrentRenderTargets;
            var newRenderTargets = new RenderTargets();

            // Handle color buffer
            if (mode.HasFlag(PushRenderTargetMode.UseOwnColorBuffer))
            {
                newRenderTargets.ColorBuffer = m_colorBufferRenderTargetView;
            }
            else if (mode.HasFlag(PushRenderTargetMode.OvertakeColorBuffer))
            {
                newRenderTargets.ColorBuffer = prevRenderTargets.ColorBuffer;
            }

            // Handle depth-stencil buffer
            if (mode.HasFlag(PushRenderTargetMode.UseOwnDepthBuffer))
            {
                newRenderTargets.DepthStencilBuffer = m_depthBufferView;
            }
            else if (mode.HasFlag(PushRenderTargetMode.OvertakeDepthBuffer))
            {
                newRenderTargets.DepthStencilBuffer = prevRenderTargets.DepthStencilBuffer;
            }

            // Handle object-id buffer
            if (mode.HasFlag(PushRenderTargetMode.UseOwnObjectIDBuffer))
            {
                newRenderTargets.ObjectIDBuffer = m_objectIDBufferRenderTargetView;
            }
            else if (mode.HasFlag(PushRenderTargetMode.OvertakeObjectIDBuffer))
            {
                newRenderTargets.ObjectIDBuffer = prevRenderTargets.ObjectIDBuffer;
            }

            // Handle normal-depth buffer
            if (mode.HasFlag(PushRenderTargetMode.UseOwnNormalDepthBuffer))
            {
                newRenderTargets.NormalDepthBuffer = m_normalDepthBufferRenderTargetView;
            }
            else if (mode.HasFlag(PushRenderTargetMode.OvertakeNormalDepthBuffer))
            {
                newRenderTargets.NormalDepthBuffer = prevRenderTargets.NormalDepthBuffer;
            }

            // Push new RenderTargets structure onto the rendering stack
            renderState.PushRenderTarget(
                newRenderTargets,
                m_viewportF, renderState.Camera, renderState.ViewInformation);
        }

        /// <summary>
        /// Pops the render target from the given render state.
        /// </summary>
        /// <param name="renderState">The render state.</param>
        internal void PopFromRenderState(RenderState renderState)
        {
            renderState.PopRenderTarget();

            // Copy texture data when in antialiasing ode
            if (m_antialiasingEnabled)
            {
                // Resolve color buffer
                if (m_creationMode.HasFlag(RenderTargetCreationMode.Color))
                {
                    renderState.Device.DeviceImmediateContextD3D11.ResolveSubresource(
                        m_colorBuffer, 0, m_colorBufferShaderResource, 0, GraphicsHelper.DEFAULT_TEXTURE_FORMAT);
                }

                // Resolve normal-depth buffer
                if (m_creationMode.HasFlag(RenderTargetCreationMode.NormalDepth))
                {
                    renderState.Device.DeviceImmediateContextD3D11.ResolveSubresource(
                        m_normalDepthBuffer, 0, m_normalDepthBufferShaderResource, 0, GraphicsHelper.DEFAULT_TEXTURE_FORMAT_NORMAL_DEPTH);
                }
            }
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            SeeingSharpTools.SafeDispose(ref m_depthBufferView);
            SeeingSharpTools.SafeDispose(ref m_colorBufferRenderTargetView);
            SeeingSharpTools.SafeDispose(ref m_colorBufferShaderResourceView);
            SeeingSharpTools.SafeDispose(ref m_depthBuffer);
            SeeingSharpTools.SafeDispose(ref m_colorBuffer);
            SeeingSharpTools.SafeDispose(ref m_normalDepthBufferRenderTargetView);
            SeeingSharpTools.SafeDispose(ref m_normalDepthBufferShaderResourceView);
            SeeingSharpTools.SafeDispose(ref m_normalDepthBuffer);

            // Unload shader resource if it was created explecitely
            if (m_shaderResourceCreated)
            {
                SeeingSharpTools.SafeDispose(ref m_colorBufferShaderResource);
                SeeingSharpTools.SafeDispose(ref m_normalDepthBufferShaderResource);
                m_shaderResourceCreated = false;
            }
        }

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the texture itself.
        /// </summary>
        public override D3D11.Texture2D Texture
        {
            get { return m_colorBuffer; }
        }

        /// <summary>
        /// Gets the shader resource view to the texture.
        /// </summary>
        public override D3D11.ShaderResourceView TextureView
        {
            get { return m_colorBufferShaderResourceView; }
        }

        internal D3D11.Texture2D TextureColor
        {
            get { return m_colorBuffer; }
        }

        internal D3D11.ShaderResourceView TextureViewColor
        {
            get { return m_colorBufferShaderResourceView; }
        }

        /// <summary>
        /// Gets the shader resource view to the normal-depth texture.
        /// </summary>
        internal D3D11.ShaderResourceView TextureViewNormalDepth
        {
            get { return m_normalDepthBufferShaderResourceView; }
        }

        public override int ArraySize
        {
            get { return 1; }
        }
    }
}
