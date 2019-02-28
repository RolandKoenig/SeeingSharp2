#region License information
/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwhise.
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

//Some namespace mappings
using D3D11 = SharpDX.Direct3D11;

#endregion

namespace SeeingSharp.Multimedia.Drawing3D
{
    #region using

    using System;
    using Core;
    using SeeingSharp.Util;

    #endregion

    public class DefaultResources : Resource
    {
        public static readonly NamedOrGenericKey RESOURCE_KEY = new NamedOrGenericKey(typeof(DefaultResources));

        #region Blend states
        private Lazy<D3D11.BlendState> m_defaultBlendState;
        private Lazy<D3D11.BlendState> m_alphaBlendingBlendState;
        #endregion

        #region Depth stencil states
        private Lazy<D3D11.DepthStencilState> m_depthStencilStateDefault;
        private Lazy<D3D11.DepthStencilState> m_depthStencilStateDisableZWrites;
        private Lazy<D3D11.DepthStencilState> m_depthStencilStateInvertedZTest;
        private Lazy<D3D11.DepthStencilState> m_depthStencilStateAllwaysPass;
        #endregion

        #region Rastarizer states
        private Lazy<D3D11.RasterizerState> m_rasterStateLines;
        private Lazy<D3D11.RasterizerState> m_rasterStateDefault;
        private Lazy<D3D11.RasterizerState> m_rasterStateBiased;
        private Lazy<D3D11.RasterizerState> m_rasterStateWireframe;
        #endregion

        #region Sample states
        private Lazy<D3D11.SamplerState> m_samplerStateLow;
        private Lazy<D3D11.SamplerState> m_samplerStateMedium;
        private Lazy<D3D11.SamplerState> m_samplerStateHigh;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultResources" /> class.
        /// </summary>
        public DefaultResources()
        {
        }

        /// <summary>
        /// Gets the sampler state with the given requested quality level.
        /// </summary>
        /// <param name="qualityLevel">The quality level to get the sampler state for.</param>
        internal D3D11.SamplerState GetSamplerState(TextureSamplerQualityLevel qualityLevel)
        {
            switch (qualityLevel)
            {
                case TextureSamplerQualityLevel.High:
                    return m_samplerStateHigh.Value;

                case TextureSamplerQualityLevel.Medium:
                    return m_samplerStateMedium.Value;

                case TextureSamplerQualityLevel.Low:
                    return m_samplerStateLow.Value;
            }

            return m_samplerStateLow.Value;
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            // Create default blend state
            m_defaultBlendState = new Lazy<D3D11.BlendState>(() =>
            {
                var blendDesc = D3D11.BlendStateDescription.Default();
                return new D3D11.BlendState(device.DeviceD3D11_1, blendDesc);
            });

            // Create alpha blending blend state
            m_alphaBlendingBlendState = new Lazy<D3D11.BlendState>(() =>
            {
                //Define the blend state (based on http://www.rastertek.com/dx11tut26.html)
                var blendDesc = D3D11.BlendStateDescription.Default();
                blendDesc.RenderTarget[0].IsBlendEnabled = true;
                blendDesc.RenderTarget[0].SourceBlend = D3D11.BlendOption.SourceAlpha;
                blendDesc.RenderTarget[0].DestinationBlend = D3D11.BlendOption.InverseSourceAlpha;
                blendDesc.RenderTarget[0].BlendOperation = D3D11.BlendOperation.Add;
                blendDesc.RenderTarget[0].DestinationAlphaBlend = D3D11.BlendOption.One;
                blendDesc.RenderTarget[0].SourceAlphaBlend = D3D11.BlendOption.One;
                blendDesc.RenderTarget[0].AlphaBlendOperation = D3D11.BlendOperation.Maximum;
                blendDesc.RenderTarget[0].RenderTargetWriteMask = D3D11.ColorWriteMaskFlags.All;

                //Create the blendstate object
                return new D3D11.BlendState(device.DeviceD3D11_1, blendDesc);
            });

            // Create default depth stencil state
            m_depthStencilStateDefault = new Lazy<D3D11.DepthStencilState>(() =>
            {
                var stateDesc = D3D11.DepthStencilStateDescription.Default();
                stateDesc.DepthComparison = D3D11.Comparison.LessEqual;
                return new D3D11.DepthStencilState(device.DeviceD3D11_1, stateDesc);
            });

            // Create the depth stencil state for diabling z writes
            m_depthStencilStateDisableZWrites = new Lazy<D3D11.DepthStencilState>(() =>
            {
                var stateDesc = D3D11.DepthStencilStateDescription.Default();
                stateDesc.DepthWriteMask = D3D11.DepthWriteMask.Zero;
                stateDesc.DepthComparison = D3D11.Comparison.LessEqual;
                return new D3D11.DepthStencilState(device.DeviceD3D11_1, stateDesc);
            });

            m_depthStencilStateAllwaysPass = new Lazy<D3D11.DepthStencilState>(() =>
            {
                var stateDesc = D3D11.DepthStencilStateDescription.Default();
                stateDesc.DepthWriteMask = D3D11.DepthWriteMask.Zero;
                stateDesc.DepthComparison = D3D11.Comparison.Always;
                stateDesc.IsDepthEnabled = false;
                return new D3D11.DepthStencilState(device.DeviceD3D11_1, stateDesc);
            });

            // Create the depth stencil state for inverting z logic
            m_depthStencilStateInvertedZTest = new Lazy<D3D11.DepthStencilState>(() =>
            {
                var stateDesc = D3D11.DepthStencilStateDescription.Default();
                stateDesc.DepthComparison = D3D11.Comparison.Greater;
                stateDesc.DepthWriteMask = D3D11.DepthWriteMask.Zero;
                return new D3D11.DepthStencilState(device.DeviceD3D11_1, stateDesc);
            });

            // Create default rasterizer state
            m_rasterStateDefault = new Lazy<D3D11.RasterizerState>(() =>
            {
                return new D3D11.RasterizerState(device.DeviceD3D11_1, D3D11.RasterizerStateDescription.Default());
            });

            // Create a raster state with depth bias
            m_rasterStateBiased = new Lazy<D3D11.RasterizerState>(() =>
            {
                var rasterDesc = D3D11.RasterizerStateDescription.Default();
                rasterDesc.DepthBias = GraphicsHelper.GetDepthBiasValue(device, -0.00003f);
                return new D3D11.RasterizerState(device.DeviceD3D11_1, rasterDesc);
            });

            // Create a raster state for wireframe rendering
            m_rasterStateWireframe = new Lazy<SharpDX.Direct3D11.RasterizerState>(() =>
            {
                var rasterDesc = D3D11.RasterizerStateDescription.Default();
                rasterDesc.FillMode = D3D11.FillMode.Wireframe;
                return new D3D11.RasterizerState(device.DeviceD3D11_1, rasterDesc);
            });

            // Create the rasterizer state for line rendering
            m_rasterStateLines = new Lazy<D3D11.RasterizerState>(() =>
            {
                var stateDesc = D3D11.RasterizerStateDescription.Default();
                stateDesc.CullMode = D3D11.CullMode.None;
                stateDesc.IsAntialiasedLineEnabled = true;
                stateDesc.FillMode = D3D11.FillMode.Solid;
                return new D3D11.RasterizerState(device.DeviceD3D11_1, stateDesc);
            });

            // Create sampler states
            m_samplerStateLow = new Lazy<D3D11.SamplerState>(() =>
            {
                return GraphicsHelper.CreateDefaultTextureSampler(device, TextureSamplerQualityLevel.Low);
            });
            m_samplerStateMedium = new Lazy<D3D11.SamplerState>(() =>
            {
                return GraphicsHelper.CreateDefaultTextureSampler(device, TextureSamplerQualityLevel.Medium);
            });
            m_samplerStateHigh = new Lazy<D3D11.SamplerState>(() =>
            {
                return GraphicsHelper.CreateDefaultTextureSampler(device, TextureSamplerQualityLevel.High);
            });
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            m_defaultBlendState = SeeingSharpTools.DisposeObjectLazy(m_defaultBlendState);
            m_depthStencilStateDefault = SeeingSharpTools.DisposeObjectLazy(m_depthStencilStateDefault);
            m_depthStencilStateDisableZWrites = SeeingSharpTools.DisposeObjectLazy(m_depthStencilStateDisableZWrites);
            m_rasterStateLines = SeeingSharpTools.DisposeObjectLazy(m_rasterStateLines);
            m_rasterStateDefault = SeeingSharpTools.DisposeObjectLazy(m_rasterStateDefault);
            m_samplerStateLow = SeeingSharpTools.DisposeObjectLazy(m_samplerStateLow);
            m_samplerStateMedium = SeeingSharpTools.DisposeObjectLazy(m_samplerStateMedium);
            m_samplerStateHigh = SeeingSharpTools.DisposeObjectLazy(m_samplerStateHigh);
        }

        /// <summary>
        /// Are resources loaded?
        /// </summary>
        public bool ResourcesLoaded
        {
            get { return m_defaultBlendState != null; }
        }

        internal D3D11.BlendState DefaultBlendState
        {
            get
            {
                if (m_defaultBlendState == null) { return null; }
                return m_defaultBlendState.Value;
            }
        }

        internal D3D11.BlendState AlphaBlendingBlendState
        {
            get
            {
                if (m_alphaBlendingBlendState == null) { return null; }
                return m_alphaBlendingBlendState.Value;
            }
        }

        internal D3D11.DepthStencilState DepthStencilStateDefault
        {
            get
            {
                if (m_depthStencilStateDefault == null) { return null; }
                return m_depthStencilStateDefault.Value;
            }
        }

        internal D3D11.DepthStencilState DepthStencilStateDisableZWrites
        {
            get
            {
                if (m_depthStencilStateDisableZWrites == null) { return null; }
                return m_depthStencilStateDisableZWrites.Value;
            }
        }

        internal D3D11.DepthStencilState DepthStencilStateAllwaysPassDepth
        {
            get
            {
                if (m_depthStencilStateAllwaysPass == null) { return null; }
                return m_depthStencilStateAllwaysPass.Value;
            }
        }

        internal D3D11.DepthStencilState DepthStencilStateInvertedZTest
        {
            get
            {
                if (m_depthStencilStateInvertedZTest == null) { return null; }
                return m_depthStencilStateInvertedZTest.Value;
            }
        }

        internal D3D11.RasterizerState RasterStateDefault
        {
            get
            {
                if (m_rasterStateDefault == null) { return null; }
                return m_rasterStateDefault.Value;
            }
        }

        internal D3D11.RasterizerState RasterStateBiased
        {
            get
            {
                if (m_rasterStateBiased == null) { return null; }
                return m_rasterStateBiased.Value;
            }
        }

        internal D3D11.RasterizerState RasterStateWireframe
        {
            get
            {
                if(m_rasterStateWireframe == null) { return null; }
                return m_rasterStateWireframe.Value;
            }
        }

        internal D3D11.RasterizerState RasterStateLines
        {
            get
            {
                if (m_rasterStateLines == null) { return null; }
                return m_rasterStateLines.Value;
            }
        }

        internal D3D11.SamplerState SamplerStateDefault
        {
            get
            {
                if (m_samplerStateMedium == null) { return null; }
                return m_samplerStateMedium.Value;
            }
        }

        public override bool IsLoaded
        {
            get { return m_defaultBlendState != null; }
        }
    }
}
