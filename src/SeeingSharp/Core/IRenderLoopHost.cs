using System;
using System.Drawing;
using SeeingSharp.Core.Devices;
using Vortice.Mathematics;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Core
{
    public interface IRenderLoopHost
    {
        Tuple<D3D11.ID3D11Texture2D, D3D11.ID3D11RenderTargetView, D3D11.ID3D11Texture2D, D3D11.ID3D11DepthStencilView, Viewport, Size, DpiScaling> OnRenderLoop_CreateViewResources(EngineDevice device);

        void OnRenderLoop_DisposeViewResources(EngineDevice device);

        bool OnRenderLoop_CheckCanRender(EngineDevice device);

        void OnRenderLoop_PrepareRendering(EngineDevice device);

        void OnRenderLoop_AfterRendering(EngineDevice device);

        void OnRenderLoop_Present(EngineDevice device);
    }
}
