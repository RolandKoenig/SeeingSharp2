#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
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

// Namespace mappings
using GDI = System.Drawing;
using D3D11 = SharpDX.Direct3D11;

#endregion

namespace SeeingSharp.Multimedia.Core
{
    #region using

    using Checking;
    using SharpDX;

    #endregion

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
        internal static SharpDX.DXGI.SwapChain1 CreateSwapChainForCoreWindow(EngineDevice device, ComObject coreWindow, int width, int height, GraphicsViewConfiguration gfxConfig)
        {
            device.EnsureNotNull(nameof(device));
            width.EnsurePositive(nameof(width));
            height.EnsurePositive(nameof(height));
            gfxConfig.EnsureNotNull(nameof(gfxConfig));

            var desc = new SharpDX.DXGI.SwapChainDescription1()
            {
                Width = width,
                Height = height,
                Format = GraphicsHelper.DEFAULT_TEXTURE_FORMAT,
                Stereo = false,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Usage = SharpDX.DXGI.Usage.BackBuffer | SharpDX.DXGI.Usage.RenderTargetOutput,
                BufferCount = 2,
                Scaling = SharpDX.DXGI.Scaling.None,
                SwapEffect = SharpDX.DXGI.SwapEffect.FlipSequential,
                AlphaMode = SharpDX.DXGI.AlphaMode.Ignore
            };

            //Creates the swap chain for the given CoreWindow object
            return new SharpDX.DXGI.SwapChain1(device.FactoryDxgi, device.DeviceD3D11_1, coreWindow, ref desc);
        }

        /// <summary>
        /// Creates the SwapChain object that is used on WinRT platforms.
        /// </summary>
        /// <param name="device">The device on which to create the SwapChain.</param>
        /// <param name="width">Width of the screen in pixels.</param>
        /// <param name="height">Height of the screen in pixels.</param>
        /// <param name="gfxConfig">Current graphics configuration.</param>
        internal static SharpDX.DXGI.SwapChain1 CreateSwapChainForComposition(EngineDevice device, int width, int height, GraphicsViewConfiguration gfxConfig)
        {
            device.EnsureNotNull(nameof(device));
            width.EnsurePositive(nameof(width));
            height.EnsurePositive(nameof(height));
            gfxConfig.EnsureNotNull(nameof(gfxConfig));

            var desc = new SharpDX.DXGI.SwapChainDescription1()
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
                AlphaMode = gfxConfig.AlphaEnabledSwapChain ? SharpDX.DXGI.AlphaMode.Premultiplied : SharpDX.DXGI.AlphaMode.Ignore
            };

            //Creates the swap chain for XAML composition
            return new SharpDX.DXGI.SwapChain1(device.FactoryDxgi, device.DeviceD3D11_1, ref desc);
        }
    }
}