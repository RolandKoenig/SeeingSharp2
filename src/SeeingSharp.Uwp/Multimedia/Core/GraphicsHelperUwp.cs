﻿using SeeingSharp.Checking;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Core.Devices;
using Vortice.DXGI;

namespace SeeingSharp.Core
{
    public static class GraphicsHelperUwp
    {
        /// <summary>
        /// Creates the SwapChain object that is used on WinRT platforms.
        /// </summary>
        /// <param name="device">The device on which to create the SwapChain.</param>
        /// <param name="width">Width of the screen in pixels.</param>
        /// <param name="height">Height of the screen in pixels.</param>
        /// <param name="gfxConfig">Current graphics configuration.</param>
        internal static IDXGISwapChain1 CreateSwapChainForComposition(EngineDevice device, int width, int height, GraphicsViewConfiguration gfxConfig)
        {
            device.EnsureNotNull(nameof(device));
            width.EnsurePositiveOrZero(nameof(width));
            height.EnsurePositiveOrZero(nameof(height));
            gfxConfig.EnsureNotNull(nameof(gfxConfig));

            var desc = new SwapChainDescription1
            {
                Width = width,
                Height = height,
                Format = GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT,
                Stereo = false,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.Backbuffer | Usage.RenderTargetOutput,
                BufferCount = 2,
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.FlipSequential,
                AlphaMode = gfxConfig.AlphaEnabledSwapChain ? AlphaMode.Premultiplied : AlphaMode.Ignore
            };

            //Creates the swap chain for XAML composition
            return device.Internals.FactoryDxgi.CreateSwapChainForComposition(device.Internals.DeviceD3D11_1, desc);
        }
    }
}