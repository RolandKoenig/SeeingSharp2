using System;
using SeeingSharp.Core.Devices;
using SeeingSharp.Mathematics;
using SharpDX.Mathematics.Interop;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Core
{
    public interface IRenderLoopHost
    {
        Tuple<D3D11.Texture2D, D3D11.RenderTargetView, D3D11.Texture2D, D3D11.DepthStencilView, RawViewportF, Size2, DpiScaling> OnRenderLoop_CreateViewResources(EngineDevice device);

        void OnRenderLoop_DisposeViewResources(EngineDevice device);

        bool OnRenderLoop_CheckCanRender(EngineDevice device);

        void OnRenderLoop_PrepareRendering(EngineDevice device);

        void OnRenderLoop_AfterRendering(EngineDevice device);

        void OnRenderLoop_Present(EngineDevice device);
    }
}
