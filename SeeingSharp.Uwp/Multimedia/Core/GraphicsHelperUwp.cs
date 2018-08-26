using SeeingSharp.Multimedia.Core;
using SeeingSharp.Checking;
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

// Namespace mappings
using GDI = System.Drawing;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Core
{
    public static class GraphicsHelperUwp
    {
        /// <summary>
        /// Creates the SwapChain object that is used on WinRT platforms.
        /// </summary>
        /// <param name="device">The device on which to create the SwapChain.</param>
        /// <param name="coreWindow">The target CoreWindow object.</param>
        /// <param name="width">Width of the screen in pixels.</param>
        /// <param name="height">Height of the screen in pixels.</param>
        /// <param name="gfxConfig">Current graphics configuration.</param>
        internal static DXGI.SwapChain1 CreateSwapChainForCoreWindow(EngineDevice device, ComObject coreWindow, int width, int height, GraphicsViewConfiguration gfxConfig)
        {
            device.EnsureNotNull(nameof(device));
            width.EnsurePositive(nameof(width));
            height.EnsurePositive(nameof(height));
            gfxConfig.EnsureNotNull(nameof(gfxConfig));

            DXGI.SwapChainDescription1 desc = new SharpDX.DXGI.SwapChainDescription1()
            {
                Width = width,
                Height = height,
                Format = GraphicsHelper.DEFAULT_TEXTURE_FORMAT,
                Stereo = false,
                SampleDescription = new DXGI.SampleDescription(1, 0),
                Usage = SharpDX.DXGI.Usage.BackBuffer | SharpDX.DXGI.Usage.RenderTargetOutput,
                BufferCount = 2,
                Scaling = DXGI.Scaling.None,
                SwapEffect = SharpDX.DXGI.SwapEffect.FlipSequential,
                AlphaMode = SharpDX.DXGI.AlphaMode.Ignore
            };

            //Creates the swap chain for the given CoreWindow object
            return new DXGI.SwapChain1(device.FactoryDxgi, device.DeviceD3D11_1, coreWindow, ref desc);
        }

        /// <summary>
        /// Creates the SwapChain object that is used on WinRT platforms.
        /// </summary>
        /// <param name="device">The device on which to create the SwapChain.</param>
        /// <param name="width">Width of the screen in pixels.</param>
        /// <param name="height">Height of the screen in pixels.</param>
        /// <param name="gfxConfig">Current graphics configuration.</param>
        internal static DXGI.SwapChain1 CreateSwapChainForComposition(EngineDevice device, int width, int height, GraphicsViewConfiguration gfxConfig)
        {
            device.EnsureNotNull(nameof(device));
            width.EnsurePositive(nameof(width));
            height.EnsurePositive(nameof(height));
            gfxConfig.EnsureNotNull(nameof(gfxConfig));

            DXGI.SwapChainDescription1 desc = new SharpDX.DXGI.SwapChainDescription1()
            {
                Width = width,
                Height = height,
                Format = GraphicsHelper.DEFAULT_TEXTURE_FORMAT,
                Stereo = false,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Usage = SharpDX.DXGI.Usage.BackBuffer | SharpDX.DXGI.Usage.RenderTargetOutput,
                BufferCount = 2,
                Scaling = SharpDX.DXGI.Scaling.Stretch,
                SwapEffect = SharpDX.DXGI.SwapEffect.FlipSequential,
                AlphaMode = gfxConfig.AlphaEnabledSwapChain ? DXGI.AlphaMode.Premultiplied : SharpDX.DXGI.AlphaMode.Ignore
            };

            //Creates the swap chain for XAML composition
            return new DXGI.SwapChain1(device.FactoryDxgi, device.DeviceD3D11_1, ref desc);
        }
    }
}
