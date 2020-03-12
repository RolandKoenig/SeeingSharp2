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
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Numerics;
using D2D = SharpDX.Direct2D1;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Core
{
    public class RenderState : IDisposable
    {
        // Generic fields
        private bool _disposed;
        private Stack<Tuple<Scene, ResourceDictionary>> _sceneStack;
        private Scene _currentScene;
        private ResourceDictionary _currentResourceDictionary;
        private Matrix4Stack _world;

        // Render stack
        private RenderStackEntry _currentRenderSettings;
        private Stack<RenderStackEntry> _renderSettingsStack;
        private Stack<RenderStackEntry> _cachedRenderStackEntries;

        // Current state
        private MaterialResource _forcedMaterial;
        private MaterialResource _lastAppliedMaterial;
        private RenderPassDump _currentTargetDump;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderState"/> class.
        /// </summary>
        /// <param name="device">The device object.</param>
        private RenderState(EngineDevice device)
        {
            // Set device members
            this.Device = device;
            DeviceIndex = device.DeviceIndex;

            // Initialize world matrix
            _world = new Matrix4Stack(Matrix4x4.Identity);

            // Create settings stack
            _cachedRenderStackEntries = new Stack<RenderStackEntry>(8);
            _renderSettingsStack = new Stack<RenderStackEntry>();
            _sceneStack = new Stack<Tuple<Scene, ResourceDictionary>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderState"/> class.
        /// </summary>
        internal RenderState(
            EngineDevice device,
            RenderTargets renderTargets,
            RawViewportF viewport,
            Camera3DBase camera, ViewInformation viewInformation)
            : this(device)
        {
            this.Reset(renderTargets, viewport, camera, viewInformation);
        }

        /// <summary>
        /// Renders all given chunks.
        /// </summary>
        /// <param name="chunks">The chunks to be rendered.</param>
        internal void RenderChunks(in RenderingChunk[] chunks)
        {
            var device = this.Device;
            var deviceContext = device.DeviceImmediateContextD3D11;

            var lastVertexBufferId = -1;
            var lastIndexBufferId = -1;
            for (var loop = 0; loop < chunks.Length; loop++)
            {
                var actChunk = chunks[loop];

                // Apply VertexBuffer
                if (lastVertexBufferId != actChunk.Template.VertexBufferID)
                {
                    lastVertexBufferId = actChunk.Template.VertexBufferID;
                    deviceContext.InputAssembler.InputLayout = actChunk.InputLayout;
                    deviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(actChunk.Template.VertexBuffer, actChunk.Template.SizePerVertex, 0));
                }

                // Apply IndexBuffer
                if (lastIndexBufferId != actChunk.Template.IndexBufferID)
                {
                    lastIndexBufferId = actChunk.Template.IndexBufferID;
                    deviceContext.InputAssembler.SetIndexBuffer(actChunk.Template.IndexBuffer, Format.R32_UInt, 0);
                }

                // Apply material
                this.ApplyMaterial(actChunk.Material);
                D3D11.InputLayout newInputLayout = null;
                if (this.ForcedMaterial != null)
                {
                    newInputLayout = this.ForcedMaterial.GetInputLayout(
                        device,
                        StandardVertex.InputElements);
                    deviceContext.InputAssembler.InputLayout = newInputLayout;
                }
                try
                {
                    // Draw current render block
                    deviceContext.DrawIndexed(
                        actChunk.Template.IndexCount,
                        actChunk.Template.StartIndex,
                        0);
                }
                finally
                {
                    if (newInputLayout != null)
                    {
                        deviceContext.InputAssembler.InputLayout = null;
                        SeeingSharpUtil.SafeDispose(ref newInputLayout);
                    }
                }
            }
        }

        /// <summary>
        /// Applies current render target settings.
        /// </summary>
        public void ClearState()
        {
            if (_disposed) { throw new ObjectDisposedException("RenderState"); }

            // Clear material properties
            _lastAppliedMaterial?.Discard(this);
            _lastAppliedMaterial = null;
            _forcedMaterial = null;

            this.Device.DeviceImmediateContextD3D11.ClearState();
            _currentRenderSettings?.Apply(this.Device.DeviceImmediateContextD3D11);
        }

        /// <summary>
        /// Clears current depth buffer.
        /// </summary>
        public void ClearCurrentDepthBuffer()
        {
            this.ClearCurrentDepthBuffer(1f, 0);
        }

        /// <summary>
        /// Clears current depth buffer.
        /// </summary>
        /// <param name="depth">The depth value to write over the whole buffer.</param>
        /// <param name="stencil">The stencil value to write over the whole buffer.</param>
        public void ClearCurrentDepthBuffer(float depth, byte stencil)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("RenderState");
            }

            var currentTargets = this.CurrentRenderTargets;

            if (currentTargets.DepthStencilBuffer != null)
            {
                this.Device.DeviceImmediateContextD3D11.ClearDepthStencilView(
                    currentTargets.DepthStencilBuffer,
                    D3D11.DepthStencilClearFlags.Depth | D3D11.DepthStencilClearFlags.Stencil,
                    depth, stencil);
            }
        }

        /// <summary>
        /// Clears current color buffer.
        /// </summary>
        /// <param name="color">The color used for clearing.</param>
        public void ClearCurrentColorBuffer(Color4 color)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("RenderState");
            }

            var currentTargets = this.CurrentRenderTargets;

            if (currentTargets.ColorBuffer != null)
            {
                this.Device.DeviceImmediateContextD3D11.ClearRenderTargetView(
                    currentTargets.ColorBuffer,
                    SdxMathHelper.RawFromColor4(color));
            }
        }

        /// <summary>
        /// Clears current normal-depth buffer.
        /// </summary>
        public void ClearCurrentNormalDepth()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("RenderState");
            }

            var currentTargets = this.CurrentRenderTargets;

            if (currentTargets.NormalDepthBuffer != null)
            {
                this.Device.DeviceImmediateContextD3D11.ClearRenderTargetView(
                    currentTargets.NormalDepthBuffer,
                    SdxMathHelper.RawFromColor4(Color4.Transparent));
            }
        }

        /// <summary>
        /// Pushes a scene onto the stack.
        /// </summary>
        /// <param name="scene">Scene to be pushed onto the stack.</param>
        /// <param name="resourceDictionary">The <see cref="ResourceDictionary"/> to be pushed onto the stack.</param>
        internal void PushScene(Scene scene, ResourceDictionary resourceDictionary)
        {
            if (_disposed) { throw new ObjectDisposedException("RenderState"); }

            _sceneStack.Push(Tuple.Create(_currentScene, _currentResourceDictionary));
            _currentScene = scene;
            _currentResourceDictionary = resourceDictionary;
        }

        /// <summary>
        /// Pops a scene from the stack.
        /// </summary>
        internal void PopScene()
        {
            if (_disposed) { throw new ObjectDisposedException("RenderState"); }
            if (_sceneStack.Count < 0) { throw new SeeingSharpGraphicsException("There is only one element on the render stack!"); }

            var lowerContent = _sceneStack.Pop();
            _currentScene = lowerContent.Item1;
            _currentResourceDictionary = lowerContent.Item2;
        }

        /// <summary>
        /// Pops a render target from the render target stack.
        /// </summary>
        internal void PopRenderTarget()
        {
            if (_disposed) { throw new ObjectDisposedException("RenderState"); }
            if (_renderSettingsStack.Count < 1) { throw new SeeingSharpGraphicsException("There is only one element on the render stack!"); }

            // Register current stack entry for reusing
            _cachedRenderStackEntries.Push(_currentRenderSettings);

            // Apply old configuration
            _currentRenderSettings = _renderSettingsStack.Pop();
            _currentRenderSettings.Apply(this.Device.DeviceImmediateContextD3D11);
        }

        /// <summary>
        /// Applies current target settings.
        /// </summary>
        internal void ApplyCurrentTarget()
        {
            if (_disposed) { throw new ObjectDisposedException("RenderState"); }

            _currentRenderSettings?.Apply(this.Device.DeviceImmediateContextD3D11);
        }

        /// <summary>
        /// Generates a WorldViewProjection matrix.
        /// </summary>
        /// <param name="world">The world matrix.</param>
        public Matrix4x4 GenerateWorldViewProj(Matrix4x4 world)
        {
            if (_disposed) { throw new ObjectDisposedException("RenderState"); }

            return world * _currentRenderSettings.ViewProj;
        }

        /// <summary>
        /// Disposes all resources of this object.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) { return; }

            _disposed = true;
        }

        /// <summary>
        /// Forces the use of the given material.
        /// </summary>
        /// <param name="material">The material to be forced for further rendering. Null means to disable material forcing.</param>
        internal void ForceMaterial(MaterialResource material)
        {
            _lastAppliedMaterial?.Discard(this);
            _lastAppliedMaterial = null;

            _forcedMaterial = material;
        }

        /// <summary>
        /// Applies the given material to the renderer.
        /// </summary>
        /// <param name="resourceToApply">The material to apply.</param>
        internal void ApplyMaterial(MaterialResource resourceToApply)
        {
            // Use forced material if any set
            if (_forcedMaterial != null &&
                resourceToApply != _forcedMaterial)
            {
                resourceToApply = _forcedMaterial;
            }

            // Disable logic if given material is null
            if (resourceToApply == null)
            {
                _lastAppliedMaterial?.Discard(this);
                _lastAppliedMaterial = null;
                return;
            }

            if (_lastAppliedMaterial != resourceToApply)
            {
                _lastAppliedMaterial?.Discard(this);

                // Apply material (material or instancing mode has changed)
                resourceToApply.Apply(this, _lastAppliedMaterial);

                _lastAppliedMaterial = resourceToApply;
            }
        }

        /// <summary>
        /// An internal helper method which tells the RenderState to clear
        /// the cached material resource, which was applied lastly.
        /// This method must be called if other parts (e. g. postprocessing) work
        /// with shaders or such like outside of the renderstate.
        /// </summary>
        internal void ClearCachedAppliedMaterial()
        {
            _lastAppliedMaterial?.Discard(this);
            _lastAppliedMaterial = null;
        }

        internal void DumpCurrentRenderTargets(string dumpKey)
        {
            if (_currentTargetDump == null) { return; }
            if (_currentRenderSettings == null) { return; }

            _currentTargetDump.Dump(dumpKey, _currentRenderSettings.RenderTargets);
        }

        /// <summary>
        /// Start writing to given <see cref="RenderPassDump"/> object.
        /// </summary>
        internal void StartDump(RenderPassDump targetDump)
        {
            _currentTargetDump = targetDump;
        }

        /// <summary>
        /// Stop writing to dump object.
        /// </summary>
        internal void StopDump()
        {
            _currentTargetDump = null;
        }

        /// <summary>
        /// Resets the render state.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="camera">The camera for the new render target.</param>
        /// <param name="viewInformation">The view information.</param>
        /// <param name="renderTargets">The render targets used for rendering.</param>
        internal void Reset(
            in RenderTargets renderTargets,
            in RawViewportF viewport,
            in Camera3DBase camera, in ViewInformation viewInformation)
        {
            _currentTargetDump = null;
            _renderSettingsStack.Clear();
            _sceneStack.Clear();
            _currentScene = null;
            _world.ResetStackToIdentity();

            // Initialize current render properties
            if (_currentRenderSettings == null)
            {
                if (_cachedRenderStackEntries.Count > 0)
                {
                    _currentRenderSettings = _cachedRenderStackEntries.Pop();
                    _currentRenderSettings.Reset(camera, renderTargets, viewport, viewInformation);
                }
                else
                {
                    _currentRenderSettings = new RenderStackEntry(camera, renderTargets, viewport, viewInformation);
                }
            }
            else
            {
                _currentRenderSettings.Reset(camera, renderTargets, viewport, viewInformation);
            }

            // Apply initial render properties
            _currentRenderSettings.Apply(this.Device.DeviceImmediateContextD3D11);
        }

        /// <summary>
        /// Pushes a new render target onto the render target stack.
        /// </summary>
        /// <param name="viewport">The viewport object.</param>
        /// <param name="renderTargets">A structure containing all relevant render targets.</param>
        /// <param name="camera">The camera for the new render target.</param>
        /// <param name="viewInformation">The view information.</param>
        /// <exception cref="System.ObjectDisposedException">RenderState</exception>
        internal void PushRenderTarget(
            in RenderTargets renderTargets,
            in RawViewportF viewport,
            in Camera3DBase camera, in ViewInformation viewInformation)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("RenderState");
            }

            // Build new render stack entry
            RenderStackEntry newEntry;
            if (_cachedRenderStackEntries.Count > 0)
            {
                newEntry = _cachedRenderStackEntries.Pop();
                newEntry.Reset(camera, renderTargets, viewport, viewInformation);
            }
            else
            {
                newEntry = new RenderStackEntry(camera, renderTargets, viewport, viewInformation);
            }

            // Overtake device settings
            newEntry.Apply(this.Device.DeviceImmediateContextD3D11);

            // Push new entry onto the stack
            _renderSettingsStack.Push(_currentRenderSettings);
            _currentRenderSettings = newEntry;
        }

        /// <summary>
        /// Gets current Device object.
        /// </summary>
        public EngineDevice Device { get; }

        /// <summary>
        /// Gets the ViewProj matrix.
        /// </summary>
        public Matrix4x4 ViewProj => _currentRenderSettings.ViewProj;

        /// <summary>
        /// Gets current world matrix.
        /// </summary>
        /// <value>The world.</value>
        public Matrix4Stack World => _world;

        /// <summary>
        /// Gets current scene object.
        /// </summary>
        public Scene CurrentScene => _currentScene;

        /// <summary>
        /// Gets the current ResourceDictionary object.
        /// </summary>
        public ResourceDictionary CurrentResources => _currentResourceDictionary;

        /// <summary>
        /// Gets current camera.
        /// </summary>
        public Camera3DBase Camera => _currentRenderSettings?.Camera;

        /// <summary>
        /// Gets current common information about the view.
        /// </summary>
        public ViewInformation ViewInformation => _currentRenderSettings?.ViewInformation;

        /// <summary>
        /// Is this object disposed?
        /// </summary>
        public bool Disposed => _disposed;

        /// <summary>
        /// Gets the currently forced material.
        /// </summary>
        public MaterialResource ForcedMaterial => _forcedMaterial;

        public bool IsWritingRenderPassDump => _currentTargetDump != null;

        /// <summary>
        /// Gets or sets the current object for 2D rendering.
        /// </summary>
        public Graphics2D Graphics2D
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the current main viewport.
        /// </summary>
        internal RawViewportF Viewport
        {
            get
            {
                if (_currentRenderSettings == null) { return new RawViewportF(); }
                return _currentRenderSettings.SingleViewport;
            }
        }

        /// <summary>
        /// Gets the currently bound render targets (on top of the render stack).
        /// </summary>
        internal RenderTargets CurrentRenderTargets
        {
            get
            {
                if (_currentRenderSettings == null) { return new RenderTargets(); }
                return _currentRenderSettings.RenderTargets;
            }
        }

        /// <summary>
        /// Gets or sets the current device index.
        /// </summary>
        internal int DeviceIndex;

        /// <summary>
        /// Gets or sets the current render target for 2D rendering.
        /// </summary>
        internal D2D.RenderTarget RenderTarget2D;

        /// <summary>
        /// Gets or sets the current view index.
        /// </summary>
        internal int ViewIndex;

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// HelperClass for RenderState class
        /// </summary>
        private class RenderStackEntry
        {
            // Local array which store all RenderTargets for usage
            private D3D11.RenderTargetView[] _targetArray;
            private RawViewportF[] _viewports;

            public RenderStackEntry(
                in Camera3DBase camera, in RenderTargets renderTargets, 
                in RawViewportF viewPort, in ViewInformation viewInfo)
            {
                this.Matrix4Stack = new Matrix4Stack();
                this.Reset(camera, renderTargets, viewPort, viewInfo);
            }

            public void Reset(
                in Camera3DBase camera, in RenderTargets renderTargets,
                in RawViewportF viewPort, in ViewInformation viewInfo)
            {
                this.Camera = camera;
                this.Matrix4Stack.ResetStackToIdentity();
                this.RenderTargets = renderTargets;
                this.SingleViewport = viewPort;
                this.ViewInformation = viewInfo;
            }

            /// <summary>
            /// Applies all properties.
            /// </summary>
            /// <param name="deviceContext">Target DeviceContext object.</param>
            public void Apply(D3D11.DeviceContext deviceContext)
            {
                // Create render target array (if not done before)
                if (_targetArray == null) { _targetArray = new D3D11.RenderTargetView[3]; }
                _targetArray[0] = this.RenderTargets.ColorBuffer;
                _targetArray[1] = this.RenderTargets.ObjectIDBuffer;
                _targetArray[2] = this.RenderTargets.NormalDepthBuffer;

                if (_viewports == null){ _viewports = new RawViewportF[3]; }
                _viewports[0] = this.SingleViewport;
                _viewports[1] = this.SingleViewport;
                _viewports[2] = this.SingleViewport;

                // Set render targets to output merger
                deviceContext.Rasterizer.SetViewports(_viewports);
                deviceContext.OutputMerger.SetTargets(this.RenderTargets.DepthStencilBuffer, _targetArray);
            }

            /// <summary>
            /// Gets the current view projection matrix.
            /// </summary>
            public Matrix4x4 ViewProj => this.Camera.ViewProjection;

            public Camera3DBase Camera { get; private set; }

            public Matrix4Stack Matrix4Stack { get; private set; }

            public RenderTargets RenderTargets { get; private set; }

            public RawViewportF SingleViewport { get; private set; }

            public ViewInformation ViewInformation { get; private set; }
        }
    }
}
