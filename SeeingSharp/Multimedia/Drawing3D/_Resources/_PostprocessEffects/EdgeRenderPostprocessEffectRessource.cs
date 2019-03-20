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

using System.Runtime.InteropServices;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using SharpDX;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class EdgeDetectPostprocessEffectResource : PostprocessEffectResource
    {
        // Static resource keys
        private static readonly NamedOrGenericKey RES_KEY_PIXEL_SHADER_BLUR = GraphicsCore.GetNextGenericResourceKey();

        // Instance resource keys
        private readonly NamedOrGenericKey KEY_RENDER_TARGET = GraphicsCore.GetNextGenericResourceKey();
        private readonly NamedOrGenericKey KEY_CONSTANT_BUFFER = GraphicsCore.GetNextGenericResourceKey();

        // Configuration
        private Color4 m_borderColor;

        // Resources
        private RenderTargetTextureResource m_renderTarget;
        private DefaultResources m_defaultResources;
        private PixelShaderResource m_pixelShaderBlur;
        private CBPerObject m_constantBufferData;
        private TypeSafeConstantBufferResource<CBPerObject> m_constantBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="FocusPostprocessEffectResource"/> class.
        /// </summary>
        public EdgeDetectPostprocessEffectResource()
        {
            this.Thickness = 2f;
            m_borderColor = Color4Ex.BlueColor;
            this.DrawOriginalObject = true;
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            base.LoadResourceInternal(device, resources);

            // Load graphics resources
            m_pixelShaderBlur = resources.GetResourceAndEnsureLoaded(
                RES_KEY_PIXEL_SHADER_BLUR,
                () => GraphicsHelper.GetPixelShaderResource(device, "Postprocessing", "PostprocessEdgeDetect"));
            m_renderTarget = resources.GetResourceAndEnsureLoaded(
                KEY_RENDER_TARGET,
                () => new RenderTargetTextureResource(RenderTargetCreationMode.Color));
            m_defaultResources = resources.DefaultResources;

            // Load constant buffer
            m_constantBufferData = new CBPerObject();
            m_constantBuffer = resources.GetResourceAndEnsureLoaded(
                KEY_CONSTANT_BUFFER,
                () => new TypeSafeConstantBufferResource<CBPerObject>(m_constantBufferData));
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            base.UnloadResourceInternal(device, resources);

            m_pixelShaderBlur = null;
            m_renderTarget = null;
            m_defaultResources = null;
            m_constantBuffer = null;
        }

        /// <summary>
        /// Notifies that rendering begins.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        /// <param name="passID">The ID of the current pass (starting with 0)</param>
        internal override void NotifyBeforeRender(RenderState renderState, int passID)
        {
            switch (passID)
            {
                //******************************
                // 1. Pass: Draw all pixels that ly behind other already rendered elements
                case 0:
                    // Apply current render target size an push render target texture on current rendering stack
                    m_renderTarget.ApplySize(renderState);
                    m_renderTarget.PushOnRenderState(renderState, PushRenderTargetMode.Default_OwnColor_PrevDepthObjectIDNormalDepth);

                    // Clear current render target
                    renderState.ClearCurrentColorBuffer(new Color(0f, 0f, 0f, 0f));
                    break;
            }
        }

        /// <summary>
        /// Notifies that rendering of the plain part has finished.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        /// <param name="passID">The ID of the current pass (starting with 0)</param>
        internal override void NotifyAfterRenderPlain(RenderState renderState, int passID)
        {
            // Nothing to be done here
        }

        /// <summary>
        /// Notifies that rendering has finished.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        /// <param name="passID">The ID of the current pass (starting with 0)</param>
        /// <returns>
        /// True, if rendering should continue with next pass. False if postprocess effect is finished.
        /// </returns>
        internal override bool NotifyAfterRender(RenderState renderState, int passID)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;

            // Reset settings made on render state (needed for all passes)
            deviceContext.Rasterizer.State = m_defaultResources.RasterStateDefault;

            // Reset render target (needed for all passes)
            m_renderTarget.PopFromRenderState(renderState);

            // Update constant buffer data
            var currentViewSize = renderState.ViewInformation.CurrentViewSize;
            m_constantBufferData.ScreenPixelSize = currentViewSize.ToVector2();
            m_constantBufferData.Opacity = 0.9f;
            m_constantBufferData.Threshold = 0.2f;
            m_constantBufferData.Thickness = this.Thickness;
            m_constantBufferData.BorderColor = m_borderColor.ToVector3();
            m_constantBufferData.OriginalColorAlpha = this.DrawOriginalObject ? 1f : 0f;
            m_constantBuffer.SetData(deviceContext, m_constantBufferData);

            // Render result of current pass to the main render target
            switch (passID)
            {
                case 0:
                    this.ApplyAlphaBasedSpriteRendering(deviceContext);
                    try
                    {
                        deviceContext.PixelShader.SetShaderResource(0, m_renderTarget.TextureView);
                        deviceContext.PixelShader.SetSampler(0, m_defaultResources.GetSamplerState(TextureSamplerQualityLevel.Low));
                        deviceContext.PixelShader.SetConstantBuffer(2, m_constantBuffer.ConstantBuffer);
                        deviceContext.PixelShader.Set(m_pixelShaderBlur.PixelShader);
                        deviceContext.Draw(3, 0);
                    }
                    finally
                    {
                        this.DiscardAlphaBasedSpriteRendering(deviceContext);
                    }
                    return false;
            }

            return false;
        }

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded =>
            m_renderTarget != null &&
            m_renderTarget.IsLoaded;

        public float Thickness { get; set; }

        public Color4 BorderColor
        {
            get => m_borderColor;
            set => m_borderColor = value;
        }

        public bool DrawOriginalObject { get; set; }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        [StructLayout(LayoutKind.Sequential)]
        private struct CBPerObject
        {
            public float Opacity;
            public Vector2 ScreenPixelSize;
            public float Thickness;
            public float Threshold;
            public Vector3 BorderColor;
            public float OriginalColorAlpha;
            public Vector3 Dummy;
        }
    }
}
