using System;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using SeeingSharp.Checking;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D.Resources
{
    public class DefaultResources : Resource
    {
        public static readonly NamedOrGenericKey RESOURCE_KEY = new NamedOrGenericKey(typeof(DefaultResources));

        // Blend states
        private Lazy<D3D11.ID3D11BlendState>? _defaultBlendState;
        private Lazy<D3D11.ID3D11BlendState>? _alphaBlendingBlendState;

        // Depth stencil states
        private Lazy<D3D11.ID3D11DepthStencilState>? _depthStencilStateDefault;
        private Lazy<D3D11.ID3D11DepthStencilState>? _depthStencilStateDisableZWrites;
        private Lazy<D3D11.ID3D11DepthStencilState>? _depthStencilStateInvertedZTest;
        private Lazy<D3D11.ID3D11DepthStencilState>? _depthStencilStateAlwaysPass;

        // Rasterizer states
        private Lazy<D3D11.ID3D11RasterizerState>? _rasterStateLines;
        private Lazy<D3D11.ID3D11RasterizerState>? _rasterStateDefault;
        private Lazy<D3D11.ID3D11RasterizerState>? _rasterStateBiased;
        private Lazy<D3D11.ID3D11RasterizerState>? _rasterStateWireframe;

        /// <summary>
        /// Are resources loaded?
        /// </summary>
        public bool ResourcesLoaded => _defaultBlendState != null;

        public override bool IsLoaded => _defaultBlendState != null;

        internal D3D11.ID3D11BlendState DefaultBlendState
        {
            get
            {
                _defaultBlendState.EnsureResourceLoaded(typeof(DefaultResources));
                return _defaultBlendState!.Value;
            }
        }

        internal D3D11.ID3D11BlendState AlphaBlendingBlendState
        {
            get
            {
                _alphaBlendingBlendState.EnsureResourceLoaded(typeof(DefaultResources));
                return _alphaBlendingBlendState!.Value;
            }
        }

        internal D3D11.ID3D11DepthStencilState DepthStencilStateDefault
        {
            get
            {
                _depthStencilStateDefault.EnsureResourceLoaded(typeof(DefaultResources));
                return _depthStencilStateDefault!.Value;
            }
        }

        internal D3D11.ID3D11DepthStencilState DepthStencilStateDisableZWrites
        {
            get
            {
                _depthStencilStateDisableZWrites.EnsureResourceLoaded(typeof(DefaultResources));
                return _depthStencilStateDisableZWrites!.Value;
            }
        }

        internal D3D11.ID3D11DepthStencilState DepthStencilStateAlwaysPassDepth
        {
            get
            {
                _depthStencilStateAlwaysPass.EnsureResourceLoaded(typeof(DefaultResources));
                return _depthStencilStateAlwaysPass!.Value;
            }
        }

        internal D3D11.ID3D11DepthStencilState DepthStencilStateInvertedZTest
        {
            get
            {
                _depthStencilStateInvertedZTest.EnsureResourceLoaded(typeof(DefaultResources));
                return _depthStencilStateInvertedZTest!.Value;
            }
        }

        internal D3D11.ID3D11RasterizerState RasterStateDefault
        {
            get
            {
                _rasterStateDefault.EnsureResourceLoaded(typeof(DefaultResources));
                return _rasterStateDefault!.Value;
            }
        }

        internal D3D11.ID3D11RasterizerState RasterStateBiased
        {
            get
            {
                _rasterStateBiased.EnsureResourceLoaded(typeof(DefaultResources));
                return _rasterStateBiased!.Value;
            }
        }

        internal D3D11.ID3D11RasterizerState RasterStateWireframe
        {
            get
            {
                _rasterStateWireframe.EnsureResourceLoaded(typeof(DefaultResources));
                return _rasterStateWireframe!.Value;
            }
        }

        internal D3D11.ID3D11RasterizerState RasterStateLines
        {
            get
            {
                _rasterStateLines.EnsureResourceLoaded(typeof(DefaultResources));
                return _rasterStateLines!.Value;
            }
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            // Create default blend state
            _defaultBlendState = new Lazy<D3D11.ID3D11BlendState>(() =>
            {
                var blendDesc = CreateDefaultBlendDescription();
                return device.DeviceD3D11_1.CreateBlendState(blendDesc);
            });

            // Create alpha blending blend state
            _alphaBlendingBlendState = new Lazy<D3D11.ID3D11BlendState>(() =>
            {
                //Define the blend state (based on http://www.rastertek.com/dx11tut26.html)
                var blendDesc = CreateDefaultBlendDescription();
                blendDesc.RenderTarget[0].IsBlendEnabled = true;
                blendDesc.RenderTarget[0].SourceBlend = D3D11.Blend.SourceAlpha;
                blendDesc.RenderTarget[0].DestinationBlend = D3D11.Blend.InverseSourceAlpha;
                blendDesc.RenderTarget[0].BlendOperation = D3D11.BlendOperation.Add;
                blendDesc.RenderTarget[0].DestinationBlendAlpha = D3D11.Blend.One;
                blendDesc.RenderTarget[0].SourceBlendAlpha = D3D11.Blend.One;
                blendDesc.RenderTarget[0].BlendOperationAlpha = D3D11.BlendOperation.Max;
                blendDesc.RenderTarget[0].RenderTargetWriteMask = D3D11.ColorWriteEnable.All;

                //Create the BlendState object
                return device.DeviceD3D11_1.CreateBlendState(blendDesc);
            });

            // Create default depth stencil state
            _depthStencilStateDefault = new Lazy<D3D11.ID3D11DepthStencilState>(() =>
            {
                var stateDesc = CreateDefaultDepthStencilDescription();
                stateDesc.DepthFunc = D3D11.ComparisonFunction.LessEqual;
                return device.DeviceD3D11_1.CreateDepthStencilState(stateDesc);
            });

            // Create the depth stencil state for disabling z writes
            _depthStencilStateDisableZWrites = new Lazy<D3D11.ID3D11DepthStencilState>(() =>
            {
                var stateDesc = CreateDefaultDepthStencilDescription();
                stateDesc.DepthWriteMask = D3D11.DepthWriteMask.Zero;
                stateDesc.DepthFunc = D3D11.ComparisonFunction.LessEqual;
                return device.DeviceD3D11_1.CreateDepthStencilState(stateDesc);
            });

            _depthStencilStateAlwaysPass = new Lazy<D3D11.ID3D11DepthStencilState>(() =>
            {
                var stateDesc = CreateDefaultDepthStencilDescription();
                stateDesc.DepthWriteMask = D3D11.DepthWriteMask.Zero;
                stateDesc.DepthFunc = D3D11.ComparisonFunction.Always;
                stateDesc.DepthEnable = false;
                return device.DeviceD3D11_1.CreateDepthStencilState(stateDesc);
            });

            // Create the depth stencil state for inverting z logic
            _depthStencilStateInvertedZTest = new Lazy<D3D11.ID3D11DepthStencilState>(() =>
            {
                var stateDesc = CreateDefaultDepthStencilDescription();
                stateDesc.DepthFunc = D3D11.ComparisonFunction.Greater;
                stateDesc.DepthWriteMask = D3D11.DepthWriteMask.Zero;
                return device.DeviceD3D11_1.CreateDepthStencilState(stateDesc);
            });

            // Create default rasterizer state
            _rasterStateDefault = new Lazy<D3D11.ID3D11RasterizerState>(
                () =>
                {
                    var stateDesc = CreateDefaultRasterizerDescription();
                    stateDesc.CullMode = D3D11.CullMode.Back;
                    stateDesc.AntialiasedLineEnable = true;
                    stateDesc.MultisampleEnable = true;
                    stateDesc.FillMode = D3D11.FillMode.Solid;
                    return device.DeviceD3D11_1.CreateRasterizerState(stateDesc);
                });

            // Create a raster state with depth bias
            _rasterStateBiased = new Lazy<D3D11.ID3D11RasterizerState>(() =>
            {
                var rasterDesc = CreateDefaultRasterizerDescription();
                rasterDesc.DepthBias = GraphicsHelper.Internals.GetDepthBiasValue(device, -0.00003f);
                return device.DeviceD3D11_1.CreateRasterizerState(rasterDesc);
            });

            // Create a raster state for wireframe rendering
            _rasterStateWireframe = new Lazy<D3D11.ID3D11RasterizerState>(() =>
            {
                var rasterDesc = CreateDefaultRasterizerDescription();
                rasterDesc.FillMode = D3D11.FillMode.Wireframe;
                rasterDesc.AntialiasedLineEnable = true;
                rasterDesc.MultisampleEnable = true;
                return device.DeviceD3D11_1.CreateRasterizerState(rasterDesc);
            });

            // Create the rasterizer state for line rendering
            _rasterStateLines = new Lazy<D3D11.ID3D11RasterizerState>(() =>
            {
                var rasterDesc = CreateDefaultRasterizerDescription();
                rasterDesc.CullMode = D3D11.CullMode.None;
                rasterDesc.AntialiasedLineEnable = true;
                rasterDesc.MultisampleEnable = true;
                rasterDesc.FillMode = D3D11.FillMode.Solid;
                return device.DeviceD3D11_1.CreateRasterizerState(rasterDesc);
            });
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _defaultBlendState = SeeingSharpUtil.DisposeObjectLazy(_defaultBlendState);
            _depthStencilStateDefault = SeeingSharpUtil.DisposeObjectLazy(_depthStencilStateDefault);
            _depthStencilStateDisableZWrites = SeeingSharpUtil.DisposeObjectLazy(_depthStencilStateDisableZWrites);
            _rasterStateLines = SeeingSharpUtil.DisposeObjectLazy(_rasterStateLines);
            _rasterStateDefault = SeeingSharpUtil.DisposeObjectLazy(_rasterStateDefault);
        }

        /// <summary>
        /// Returns default values for <see cref="D3D11.RasterizerDescription"/>.
        /// Original code from https://github.com/sharpdx/SharpDX/blob/master/Source/SharpDX.Direct3D11/RasterizerStateDescription.cs
        /// </summary>
        private static D3D11.RasterizerDescription CreateDefaultRasterizerDescription()
        {
            return new D3D11.RasterizerDescription()
            {
                FillMode = D3D11.FillMode.Solid,
                CullMode = D3D11.CullMode.Back,
                FrontCounterClockwise = false,
                DepthBias = 0,
                SlopeScaledDepthBias = 0.0f,
                DepthBiasClamp = 0.0f,
                DepthClipEnable = true,
                ScissorEnable = false,
                MultisampleEnable = false,
                AntialiasedLineEnable = false
            };
        }

        /// <summary>
        /// Returns default values for <see cref="D3D11.BlendDescription"/>.
        /// Original code from https://github.com/sharpdx/SharpDX/blob/master/Source/SharpDX.Direct3D11/BlendStateDescription.cs
        /// </summary>
        private static D3D11.BlendDescription CreateDefaultBlendDescription()
        {
            var description = new D3D11.BlendDescription()
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,
            };
            var renderTargets = description.RenderTarget;
            for (var i = 0; i < renderTargets.Length; i++)
            {
                renderTargets[i].IsBlendEnabled = false;
                renderTargets[i].SourceBlend = D3D11.Blend.One;
                renderTargets[i].DestinationBlend = D3D11.Blend.Zero;
                renderTargets[i].BlendOperation = D3D11.BlendOperation.Add;

                renderTargets[i].SourceBlendAlpha = D3D11.Blend.One;
                renderTargets[i].DestinationBlendAlpha = D3D11.Blend.Zero;
                renderTargets[i].BlendOperationAlpha = D3D11.BlendOperation.Add;

                renderTargets[i].RenderTargetWriteMask = D3D11.ColorWriteEnable.All;
            }

            return description;
        }

        /// <summary>
        /// Returns default values for <see cref="D3D11.DepthStencilDescription"/>.
        /// Original code from https://github.com/sharpdx/SharpDX/blob/master/Source/SharpDX.Direct3D11/DepthStencilStateDescription.cs
        /// </summary>
        private static D3D11.DepthStencilDescription CreateDefaultDepthStencilDescription()
        {
            return new D3D11.DepthStencilDescription()
            {
                DepthEnable = true,
                DepthWriteMask = D3D11.DepthWriteMask.All, 
                DepthFunc = D3D11.ComparisonFunction.Less,
                StencilEnable = false,
                StencilReadMask = 0xFF,
                StencilWriteMask = 0xFF,
                FrontFace = new D3D11.DepthStencilOperationDescription()
                {
                    StencilFunc = D3D11.ComparisonFunction.Always,
                    StencilDepthFailOp = D3D11.StencilOperation.Keep,
                    StencilFailOp = D3D11.StencilOperation.Keep,
                    StencilPassOp = D3D11.StencilOperation.Keep
                },
                BackFace = new D3D11.DepthStencilOperationDescription()
                {
                    StencilFunc = D3D11.ComparisonFunction.Always,
                    StencilDepthFailOp = D3D11.StencilOperation.Keep,
                    StencilFailOp = D3D11.StencilOperation.Keep,
                    StencilPassOp = D3D11.StencilOperation.Keep
                }
            };
        }
    }
}
