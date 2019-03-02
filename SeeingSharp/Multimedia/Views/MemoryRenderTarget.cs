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

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;
using SharpDX;
using SharpDX.Mathematics.Interop;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Views
{
    //For handling of staging resource see
    // http://msdn.microsoft.com/en-us/library/windows/desktop/ff476259(v=vs.85).aspx

    public class MemoryRenderTarget : IDisposable, ISeeingSharpPainter, IRenderLoopHost
    {
        // Configuration
        private int m_pixelWidth;
        private int m_pixelHeight;

        // All needed direct3d resources
        private D3D11.Device m_device;
        private D3D11.DeviceContext m_deviceContext;
        private D3D11.Texture2D m_copyHelperTextureStaging;
        private D3D11.Texture2D m_renderTarget;
        private D3D11.Texture2D m_renderTargetDepth;
        private D3D11.RenderTargetView m_renderTargetView;
        private D3D11.DepthStencilView m_renderTargetDepthView;

        /// <summary>
        /// Raises before the render target starts rendering.
        /// </summary>
        public event CancelEventHandler BeforeRender;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryRenderTarget" /> class.
        /// </summary>
        /// <param name="pixelHeight">Height of the offline render target in pixels.</param>
        /// <param name="pixelWidth">Width of the offline render target in pixels.</param>
        /// <param name="syncContext">Sets the SynchronizationContext which should be used by default.</param>
        public MemoryRenderTarget(int pixelWidth, int pixelHeight, SynchronizationContext syncContext = null)
        {
            //Set confiugration
            m_pixelWidth = pixelWidth;
            m_pixelHeight = pixelHeight;

            if (syncContext == null) { syncContext = new SynchronizationContext(); }

            //Create the RenderLoop object
            RenderLoop = new RenderLoop(syncContext, this);
            RenderLoop.Camera.SetScreenSize(pixelWidth, pixelHeight);
            RenderLoop.RegisterRenderLoop();
        }

        /// <summary>
        /// Awaits next render.
        /// </summary>
        public Task AwaitRenderAsync()
        {
            if (!IsOperational) { return Task.Delay(100); }

            var result = new TaskCompletionSource<object>();
            RenderLoop.EnqueueAfterPresentAction(() =>
            {
                result.TrySetResult(null);
            });

            return result.Task;
        }

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben aus, die mit dem Freigeben, Zurückgeben oder Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        public void Dispose()
        {
            RenderLoop.Dispose();
        }

        /// <summary>
        /// Disposes all loaded view resources.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_DisposeViewResources(EngineDevice device)
        {
            m_renderTargetDepthView = SeeingSharpTools.DisposeObject(m_renderTargetDepthView);
            m_renderTargetDepth = SeeingSharpTools.DisposeObject(m_renderTargetDepth);
            m_renderTargetView = SeeingSharpTools.DisposeObject(m_renderTargetView);
            m_renderTarget = SeeingSharpTools.DisposeObject(m_renderTarget);
            m_copyHelperTextureStaging = SeeingSharpTools.DisposeObject(m_copyHelperTextureStaging);

            m_device = null;
            m_deviceContext = null;
        }

        /// <summary>
        /// Create all view resources.
        /// </summary>
        Tuple<D3D11.Texture2D, D3D11.RenderTargetView, D3D11.Texture2D, D3D11.DepthStencilView, RawViewportF, Size2, DpiScaling> IRenderLoopHost.OnRenderLoop_CreateViewResources(EngineDevice device)
        {
            var width = m_pixelWidth;
            var height = m_pixelHeight;

            //Get references to current render device
            m_device = device.DeviceD3D11_1;
            m_deviceContext = m_device.ImmediateContext;

            //Create the swap chain and the render target
            m_renderTarget = GraphicsHelper.CreateRenderTargetTexture(device, width, height, RenderLoop.ViewConfiguration);
            m_renderTargetView = new D3D11.RenderTargetView(m_device, m_renderTarget);

            //Create the depth buffer
            m_renderTargetDepth = GraphicsHelper.CreateDepthBufferTexture(device, width, height, RenderLoop.ViewConfiguration);
            m_renderTargetDepthView = new D3D11.DepthStencilView(m_device, m_renderTargetDepth);

            //Define the viewport for rendering
            var viewPort = GraphicsHelper.CreateDefaultViewport(width, height);

            //Return all generated objects
            return Tuple.Create(m_renderTarget, m_renderTargetView, m_renderTargetDepth, m_renderTargetDepthView, viewPort, new Size2(width, height), DpiScaling.Default);
        }

        /// <summary>
        /// Called when RenderLoop object checks wheter it is possible to render.
        /// </summary>
        bool IRenderLoopHost.OnRenderLoop_CheckCanRender(EngineDevice device)
        {
            var eventArgs = new CancelEventArgs(false);

            if (BeforeRender != null)
            {
                BeforeRender(this, eventArgs);
            }

            return !eventArgs.Cancel;
        }

        void IRenderLoopHost.OnRenderLoop_PrepareRendering(EngineDevice device)
        {
        }

        /// <summary>
        /// Called when RenderLoop wants to present its results.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_Present(EngineDevice device)
        {
            // Finish rendering of all render tasks
            m_deviceContext.Flush();
            m_deviceContext.ClearState();
        }

        /// <summary>
        /// Called when RenderLoop has finished rendering.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_AfterRendering(EngineDevice device)
        {

        }

        /// <summary>
        /// Gets or sets the scene.
        /// </summary>
        public Scene Scene
        {
            get => RenderLoop.Scene;
            set => RenderLoop.SetScene(value);
        }

        public Camera3DBase Camera
        {
            get => RenderLoop.Camera;
            set => RenderLoop.Camera = value;
        }

        /// <summary>
        /// Gets the renderloop object.
        /// </summary>
        public RenderLoop RenderLoop { get; }

        public Color4 ClearColor
        {
            get => RenderLoop.ClearColor;
            set => RenderLoop.ClearColor = value;
        }

        public SynchronizationContext UISynchronizationContext => RenderLoop.UISynchronizationContext;

        public bool IsOperational => RenderLoop.IsOperational;
    }
}
