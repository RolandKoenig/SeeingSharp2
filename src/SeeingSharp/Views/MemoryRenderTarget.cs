﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Drawing3D;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;
using D3D11 = Vortice.Direct3D11;
using Viewport = Vortice.Mathematics.Viewport;

namespace SeeingSharp.Views
{
    //For handling of staging resource see
    // http://msdn.microsoft.com/en-us/library/windows/desktop/ff476259(v=vs.85).aspx
    public class MemoryRenderTarget : IDisposable, ISeeingSharpPainter, IRenderLoopHost, IDpiScalingProvider
    {
        // Configuration
        private int _pixelWidth;
        private int _pixelHeight;

        // All needed direct3d resources
        private D3D11.ID3D11Device? _device;
        private D3D11.ID3D11DeviceContext? _deviceContext;
        private D3D11.ID3D11Texture2D? _renderTarget;
        private D3D11.ID3D11Texture2D? _renderTargetDepth;
        private D3D11.ID3D11RenderTargetView? _renderTargetView;
        private D3D11.ID3D11DepthStencilView? _renderTargetDepthView;

        /// <summary>
        /// Gets or sets the scene.
        /// </summary>
        public Scene Scene
        {
            get => this.RenderLoop.Scene;
            set => this.RenderLoop.SetScene(value);
        }

        public EngineDevice? Device => this.RenderLoop.Device;

        public Camera3DBase Camera
        {
            get => this.RenderLoop.Camera;
            set => this.RenderLoop.Camera = value;
        }

        /// <summary>
        /// Gets the renderloop object.
        /// </summary>
        public RenderLoop RenderLoop { get; }

        public Color4 ClearColor
        {
            get => this.RenderLoop.ClearColor;
            set => this.RenderLoop.ClearColor = value;
        }

        public SynchronizationContext UiSynchronizationContext => this.RenderLoop.UiSynchronizationContext;

        public bool IsOperational => this.RenderLoop.IsOperational;

        /// <summary>
        /// Raises before the render target starts rendering.
        /// </summary>
        public event CancelEventHandler? BeforeRender;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryRenderTarget" /> class.
        /// </summary>
        /// <param name="pixelHeight">Height of the offline render target in pixels.</param>
        /// <param name="pixelWidth">Width of the offline render target in pixels.</param>
        /// <param name="syncContext">Sets the SynchronizationContext which should be used by default.</param>
        public MemoryRenderTarget(int pixelWidth, int pixelHeight, SynchronizationContext? syncContext = null)
        {
            // Set configuration
            _pixelWidth = pixelWidth;
            _pixelHeight = pixelHeight;

            if (syncContext == null) { syncContext = new SynchronizationContext(); }

            // Create the RenderLoop object
            this.RenderLoop = new RenderLoop(syncContext, this);
            this.RenderLoop.Camera.SetScreenSize(pixelWidth, pixelHeight);
            this.RenderLoop.RegisterRenderLoop();
        }

        /// <inheritdoc />
        public DpiScaling GetCurrentDpiScaling()
        {
            return DpiScaling.Default;
        }

        /// <summary>
        /// Gets the object on the given location (location local to this control).
        /// </summary>
        /// <param name="pickingOptions">Options for picking logic.</param>
        /// <param name="location">X, Y location of the cursor in DIP.</param>
        public Task<List<SceneObject>?> PickObjectAsync(Point location, PickingOptions pickingOptions)
        {
            return this.RenderLoop.PickObjectAsync(location, pickingOptions);
        }

        /// <summary>
        /// Awaits next render.
        /// </summary>
        public Task AwaitRenderAsync()
        {
            if (!this.IsOperational) { return Task.Delay(100); }

            var result = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            this.RenderLoop.EnqueueAfterPresentAction(() =>
            {
                result.TrySetResult(null);
            });

            return result.Task;
        }

        public void Dispose()
        {
            this.RenderLoop.Dispose();
        }

        /// <summary>
        /// Disposes all loaded view resources.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_DisposeViewResources(EngineDevice device)
        {
            _renderTargetDepthView = SeeingSharpUtil.DisposeObject(_renderTargetDepthView);
            _renderTargetDepth = SeeingSharpUtil.DisposeObject(_renderTargetDepth);
            _renderTargetView = SeeingSharpUtil.DisposeObject(_renderTargetView);
            _renderTarget = SeeingSharpUtil.DisposeObject(_renderTarget);

            _device = null;
            _deviceContext = null;
        }

        /// <summary>
        /// Create all view resources.
        /// </summary>
        Tuple<D3D11.ID3D11Texture2D, D3D11.ID3D11RenderTargetView, D3D11.ID3D11Texture2D, D3D11.ID3D11DepthStencilView, Viewport, Size, DpiScaling> IRenderLoopHost.OnRenderLoop_CreateViewResources(EngineDevice device)
        {
            var width = _pixelWidth;
            var height = _pixelHeight;

            //Get references to current render device
            _device = device.DeviceD3D11_1;
            _deviceContext = _device.ImmediateContext;

            //Create the swap chain and the render target
            _renderTarget = GraphicsHelper.Internals.CreateRenderTargetTexture(device, width, height, this.RenderLoop.Configuration);
            _renderTargetView = _device.CreateRenderTargetView(_renderTarget);

            //Create the depth buffer
            _renderTargetDepth = GraphicsHelper.Internals.CreateDepthBufferTexture(device, width, height, this.RenderLoop.Configuration);
            _renderTargetDepthView = _device.CreateDepthStencilView(_renderTargetDepth);

            //Define the viewport for rendering
            var viewPort = GraphicsHelper.Internals.CreateDefaultViewport(width, height);

            //Return all generated objects
            return Tuple.Create(_renderTarget, _renderTargetView, _renderTargetDepth, _renderTargetDepthView, viewPort, new Size(width, height), DpiScaling.Default);
        }

        /// <summary>
        /// Called when RenderLoop object checks whether it is possible to render.
        /// </summary>
        bool IRenderLoopHost.OnRenderLoop_CheckCanRender(EngineDevice device)
        {
            var eventArgs = new CancelEventArgs(false);

            this.BeforeRender?.Invoke(this, eventArgs);

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
            _deviceContext!.Flush();
            _deviceContext!.ClearState();
        }

        /// <summary>
        /// Called when RenderLoop has finished rendering.
        /// </summary>
        void IRenderLoopHost.OnRenderLoop_AfterRendering(EngineDevice device)
        {

        }
    }
}
