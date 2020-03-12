﻿/*
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
using SeeingSharp.Util;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class FocusPostprocessEffectResource : PostprocessEffectResource
    {
        // Resource keys
        private static readonly NamedOrGenericKey s_resKeyPixelShaderBlur = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey s_keyMaterial = GraphicsCore.GetNextGenericResourceKey();
        private static readonly NamedOrGenericKey s_keyRenderTarget = GraphicsCore.GetNextGenericResourceKey();
        private readonly NamedOrGenericKey m_keyCbPass01 = GraphicsCore.GetNextGenericResourceKey();
        private readonly NamedOrGenericKey m_keyCbPass02 = GraphicsCore.GetNextGenericResourceKey();

        // Configuration
        private bool m_forceSimpleMethod;
        private float m_fadeIntensity;

        // Resources
        private SingleForcedColorMaterialResource m_singleForcedColor;
        private RenderTargetTextureResource m_renderTarget;
        private DefaultResources m_defaultResources;
        private PixelShaderResource m_pixelShaderBlur;
        private TypeSafeConstantBufferResource<CbPerObject> m_cbFirstPass;
        private TypeSafeConstantBufferResource<CbPerObject> m_cbSecondPass;

        /// <summary>
        /// Initializes a new instance of the <see cref="FocusPostprocessEffectResource"/> class.
        /// </summary>
        /// <param name="forceSimpleMethod">Force simple mode. Default to false.</param>
        /// <param name="fadeIntensity">Intensity of the fade effect.</param>
        public FocusPostprocessEffectResource(bool forceSimpleMethod = false, float fadeIntensity = 1f)
        {
            m_fadeIntensity.EnsureInRange(0f, 1f, nameof(fadeIntensity));

            m_forceSimpleMethod = forceSimpleMethod;
            m_fadeIntensity = fadeIntensity;
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
                s_resKeyPixelShaderBlur,
                () => GraphicsHelper.Internals.GetPixelShaderResource(device, "Postprocessing", "PostprocessBlur"));
            m_singleForcedColor = resources.GetResourceAndEnsureLoaded(
                s_keyMaterial,
                () => new SingleForcedColorMaterialResource { FadeIntensity = m_fadeIntensity });
            m_renderTarget = resources.GetResourceAndEnsureLoaded(
                s_keyRenderTarget,
                () => new RenderTargetTextureResource(RenderTargetCreationMode.Color));
            m_defaultResources = resources.DefaultResources;

            // Load constant buffers
            m_cbFirstPass = resources.GetResourceAndEnsureLoaded(
                m_keyCbPass01,
                () => new TypeSafeConstantBufferResource<CbPerObject>(new CbPerObject
                {
                    BlurIntensity = 0.0f,
                    BlurOpacity = 0.1f
                }));
            m_cbSecondPass = resources.GetResourceAndEnsureLoaded(
                m_keyCbPass02,
                () => new TypeSafeConstantBufferResource<CbPerObject>(new CbPerObject
                {
                    BlurIntensity = 0.8f,
                    BlurOpacity = 0.5f
                }));
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
            m_singleForcedColor = null;
            m_renderTarget = null;
            m_defaultResources = null;
            m_cbFirstPass = null;
            m_cbSecondPass = null;
        }

        /// <summary>
        /// Notifies that rendering begins.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        /// <param name="passId">The ID of the current pass (starting with 0)</param>
        internal override void NotifyBeforeRender(RenderState renderState, int passId)
        {
            if (renderState.Device.IsHighDetailSupported && !m_forceSimpleMethod)
            {
                switch (passId)
                {
                    //******************************
                    // 1. Pass: Draw all pixels that ly behind other already rendered elements
                    case 0:
                        // Force the single color material
                        renderState.ForceMaterial(m_singleForcedColor);

                        // Apply current render target size an push render target texture on current rendering stack
                        m_renderTarget.ApplySize(renderState);
                        m_renderTarget.PushOnRenderState(renderState, PushRenderTargetMode.Default_OwnColor_PrevDepthObjectIDNormalDepth);

                        // Clear current render target
                        renderState.ClearCurrentColorBuffer(new Color4(1f, 1f, 1f, 0f));

                        // ConfigureLoading stencil state(invert z logic, disable z writes)
                        renderState.Device.DeviceImmediateContextD3D11.OutputMerger.DepthStencilState = m_defaultResources.DepthStencilStateInvertedZTest;
                        break;

                    //******************************
                    // 2. Pass: Draw all visible pixels with some blur effect
                    case 1:
                        // Force the single color material
                        renderState.ForceMaterial(m_singleForcedColor);

                        // Push render target texture on current rendering stack again
                        m_renderTarget.PushOnRenderState(renderState, PushRenderTargetMode.Default_OwnColor_PrevDepthObjectIDNormalDepth);

                        // Clear current render target
                        renderState.ClearCurrentColorBuffer(new Color4(1f, 1f, 1f, 0f));

                        // Change raster state
                        renderState.Device.DeviceImmediateContextD3D11.Rasterizer.State = m_defaultResources.RasterStateBiased;
                        renderState.Device.DeviceImmediateContextD3D11.OutputMerger.DepthStencilState = m_defaultResources.DepthStencilStateDisableZWrites;
                        break;
                }
            }
            else
            {
                renderState.ForceMaterial(m_singleForcedColor);

                // Change raster state
                renderState.Device.DeviceImmediateContextD3D11.Rasterizer.State = m_defaultResources.RasterStateBiased;
            }
        }

        /// <summary>
        /// Notifies that rendering of the plain part has finished.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        /// <param name="passId">The ID of the current pass (starting with 0)</param>
        internal override void NotifyAfterRenderPlain(RenderState renderState, int passId)
        {
            // Nothing to be done here
        }

        /// <summary>
        /// Notifies that rendering has finished.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        /// <param name="passId">The ID of the current pass (starting with 0)</param>
        /// <returns>
        /// True, if rendering should continue with next pass. False if postprocess effect is finished.
        /// </returns>
        internal override bool NotifyAfterRender(RenderState renderState, int passId)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;

            if (renderState.Device.IsHighDetailSupported && !m_forceSimpleMethod)
            {
                // Reset settings made on render state (needed for all passes)
                deviceContext.Rasterizer.State = m_defaultResources.RasterStateDefault;
                deviceContext.OutputMerger.DepthStencilState = m_defaultResources.DepthStencilStateDefault;
                renderState.ForceMaterial(null);

                // Reset render target (needed for all passes)
                m_renderTarget.PopFromRenderState(renderState);

                // Clear cached material resource because we work with shaders directly here
                renderState.ClearCachedAppliedMaterial();

                // Render result of current pass to the main render target
                switch (passId)
                {
                    case 0:
                        this.ApplyAlphaBasedSpriteRendering(deviceContext);
                        try
                        {
                            deviceContext.PixelShader.SetShaderResource(0, m_renderTarget.TextureView);
                            deviceContext.PixelShader.SetSampler(0, m_defaultResources.GetSamplerState(TextureSamplerQualityLevel.Low));
                            deviceContext.PixelShader.SetConstantBuffer(2, m_cbFirstPass.ConstantBuffer);
                            deviceContext.PixelShader.Set(m_pixelShaderBlur.PixelShader);
                            deviceContext.Draw(3, 0);
                        }
                        finally
                        {
                            this.DiscardAlphaBasedSpriteRendering(deviceContext);
                        }
                        return true;

                    case 1:
                        this.ApplyAlphaBasedSpriteRendering(deviceContext);
                        try
                        {
                            deviceContext.PixelShader.SetShaderResource(0, m_renderTarget.TextureView);
                            deviceContext.PixelShader.SetSampler(0, m_defaultResources.GetSamplerState(TextureSamplerQualityLevel.Low));
                            deviceContext.PixelShader.SetConstantBuffer(2, m_cbSecondPass.ConstantBuffer);
                            deviceContext.PixelShader.Set(m_pixelShaderBlur.PixelShader);
                            deviceContext.Draw(3, 0);

                        }
                        finally
                        {
                            this.DiscardAlphaBasedSpriteRendering(deviceContext);
                        }
                        return false;
                }
            }
            else
            {
                // Reset changes from before
                deviceContext.Rasterizer.State = m_defaultResources.RasterStateDefault;
                renderState.ForceMaterial(null);

                // Now we ware finished
                return false;
            }

            return false;
        }

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded =>
            m_renderTarget != null &&
            m_singleForcedColor != null &&
            m_singleForcedColor.IsLoaded;

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        [StructLayout(LayoutKind.Sequential)]
        private struct CbPerObject
        {
            public float BlurIntensity;

            public float BlurOpacity;

            /// <summary>
            /// A dummy field to ensure a 4-based size of this structure.
            /// </summary>
            public Vector2 Dummy;
        }
    }
}
