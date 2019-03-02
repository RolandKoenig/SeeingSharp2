#region License information
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
#endregion
#region using

//Same namespace mappings.
using D2D = SharpDX.Direct2D1;
using D3D11 = SharpDX.Direct3D11;

#endregion

namespace SeeingSharp.Multimedia.Core
{
    #region using

    using System;
    using System.Collections.Generic;
    using Drawing2D;
    using Drawing3D;
    using SeeingSharp.Util;
    using SharpDX;

    #endregion

    public class RenderState : IDisposable
    {
        #region Resources for Direct3D 11 rendering

        #endregion

        #region  Generic fields
        private bool m_disposed;
        private Stack<RenderStackEntry> m_renderSettingsStack;
        private Stack<Tuple<Scene, ResourceDictionary>> m_sceneStack;
        private RenderStackEntry m_currentRenderSettings;
        private Scene m_currentScene;
        private ResourceDictionary m_currentResourceDictionary;
        private Matrix4Stack m_world;
        private PerformanceAnalyzer m_perfomanceCalculator;
        #endregion

        #region Current state
        private MaterialResource m_forcedMaterial;
        private MaterialResource m_lastAppliedMaterial;
        private MaterialApplyInstancingMode m_lastMaterialInstancingMode;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderState"/> class.
        /// </summary>
        /// <param name="device">The device object.</param>
        /// <param name="performanceCalculator">The object used to calculate performance values</param>
        private RenderState(EngineDevice device, PerformanceAnalyzer performanceCalculator)
        {
            //Set device members
            Device = device;
            this.DeviceIndex = device.DeviceIndex;

            //Initialize world matrix
            m_world = new Matrix4Stack(Matrix.Identity);

            //Create settings stack
            m_renderSettingsStack = new Stack<RenderStackEntry>();
            m_sceneStack = new Stack<Tuple<Core.Scene, ResourceDictionary>>();

            m_perfomanceCalculator = performanceCalculator;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderState"/> class.
        /// </summary>
        internal RenderState(
            EngineDevice device,
            PerformanceAnalyzer performanceCalculator,
            RenderTargets renderTargets,
            SharpDX.Mathematics.Interop.RawViewportF viewport,
            Camera3DBase camera, ViewInformation viewInformation)
            : this(device, performanceCalculator)
        {
            Reset(renderTargets, viewport, camera, viewInformation);
        }

        /// <summary>
        /// Forces the use of the given material.
        /// </summary>
        /// <param name="material">The material to be forced for further rendering. Null means to disable material forcing.</param>
        internal void ForceMaterial(MaterialResource material)
        {
            m_lastAppliedMaterial = null;
            m_forcedMaterial = material;
        }

        /// <summary>
        /// Applies the given material to the renderer.
        /// </summary>
        /// <param name="resourceToApply">The material to apply.</param>
        internal void ApplyMaterial(MaterialResource resourceToApply)
        {
            ApplyMaterial(resourceToApply, MaterialApplyInstancingMode.SingleObject);
        }

        /// <summary>
        /// Applies the given material to the renderer.
        /// </summary>
        /// <param name="resourceToApply">The material to apply.</param>
        /// <param name="instancingMode">The instancing mode for which to apply the material.</param>
        internal void ApplyMaterial(MaterialResource resourceToApply, MaterialApplyInstancingMode instancingMode)
        {
            // Use forced material if any set
            if ((m_forcedMaterial != null) &&
                (resourceToApply != m_forcedMaterial))
            {
                resourceToApply = m_forcedMaterial;
            }

            // Disable logic if given material is null
            if (resourceToApply == null)
            {
                m_lastAppliedMaterial = null;
                return;
            }

            if ((m_lastAppliedMaterial != resourceToApply) || (m_lastMaterialInstancingMode != instancingMode))
            {
                // Apply material (material or instancing mode has changed)
                resourceToApply.Apply(this, instancingMode, m_lastAppliedMaterial);

                m_lastAppliedMaterial = resourceToApply;
                m_lastMaterialInstancingMode = instancingMode;
            }
        }

        /// <summary>
        /// An internal helper method which tells the RenderState to clear
        /// the cached material resource, which was applied lastly.
        /// This method musst be called if other parts (e. g. postprocessing) work
        /// with shaders or such like outside of the renderstate.
        /// </summary>
        internal void ClearChachedAppliedMaterial()
        {
            m_lastAppliedMaterial = null;
            m_lastMaterialInstancingMode = MaterialApplyInstancingMode.SingleObject;
        }

        /// <summary>
        /// Disposes all resources of this object.
        /// </summary>
        public void Dispose()
        {
            if (m_disposed) { return; }

            m_disposed = true;
        }

        /// <summary>
        /// Resets the render state.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="camera">The camera for the new render target.</param>
        /// <param name="viewInformation">The view information.</param>
        /// <param name="renderTargets">The render targets used for rendering.</param>
        internal void Reset(
            RenderTargets renderTargets,
            SharpDX.Mathematics.Interop.RawViewportF viewport,
            Camera3DBase camera, ViewInformation viewInformation)
        {
            m_renderSettingsStack.Clear();
            m_sceneStack.Clear();
            m_currentScene = null;
            m_world = new Matrix4Stack(Matrix.Identity);

            //Inititialize current render properties
            m_currentRenderSettings = new RenderStackEntry
            {
                Matrix4Stack = new Matrix4Stack(),
                RenderTargets = renderTargets,
                SingleViewport = viewport,
                Camera = camera,
                ViewInformation = viewInformation
            };

            //Apply initial render properties
            m_currentRenderSettings.Apply(Device.DeviceImmediateContextD3D11);
        }

        /// <summary>
        /// Applies current render target settings.
        /// </summary>
        public void ClearState()
        {
            if (m_disposed) { throw new ObjectDisposedException("RenderState"); }

            // Clear material properties
            m_lastAppliedMaterial = null;
            m_forcedMaterial = null;
            m_lastMaterialInstancingMode = MaterialApplyInstancingMode.SingleObject;

            Device.DeviceImmediateContextD3D11.ClearState();
            if (m_currentRenderSettings != null) { m_currentRenderSettings.Apply(Device.DeviceImmediateContextD3D11); }
        }

        /// <summary>
        /// Clears current depth buffer.
        /// </summary>
        public void ClearCurrentDepthBuffer()
        {
            ClearCurrentDepthBuffer(1f, 0);
        }

        /// <summary>
        /// Clears current depth buffer.
        /// </summary>
        /// <param name="depth">The depth value to write over the whole buffer.</param>
        /// <param name="stencil">The stencil value to write over the whole buffer.</param>
        public void ClearCurrentDepthBuffer(float depth, byte stencil)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException("RenderState");
            }

            var currentTargets = this.CurrentRenderTargets;

            if (currentTargets.DepthStencilBuffer != null)
            {
                Device.DeviceImmediateContextD3D11.ClearDepthStencilView(
                    currentTargets.DepthStencilBuffer,
                    D3D11.DepthStencilClearFlags.Depth | D3D11.DepthStencilClearFlags.Stencil,
                    depth, stencil);
            }
        }

        /// <summary>
        /// Clears current color buffer.
        /// </summary>
        /// <param name="color">The culor used for clering.</param>
        public void ClearCurrentColorBuffer(Color4 color)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException("RenderState");
            }

            var currentTargets = this.CurrentRenderTargets;

            if (currentTargets.ColorBuffer != null)
            {
                Device.DeviceImmediateContextD3D11.ClearRenderTargetView(
                    currentTargets.ColorBuffer,
                    color);
            }
        }

        /// <summary>
        /// Clears current normal-depth buffer.
        /// </summary>
        public void ClearCurrentNormalDepth()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException("RenderState");
            }

            var currentTargets = this.CurrentRenderTargets;

            if (currentTargets.NormalDepthBuffer != null)
            {
                Device.DeviceImmediateContextD3D11.ClearRenderTargetView(
                    currentTargets.NormalDepthBuffer,
                    Color4Ex.Transparent);
            }
        }

        /// <summary>
        /// Pushes a scene onto the stack.
        /// </summary>
        /// <param name="scene">Scene to be pushed onto the stack.</param>
        /// <param name="resourceDictionary">The <see cref="ResourceDictionary"/> to be pushed onto the stack.</param>
        public IDisposable PushScene(Scene scene, ResourceDictionary resourceDictionary)
        {
            if (m_disposed) { throw new ObjectDisposedException("RenderState"); }
            if (m_currentScene == scene) { return new DummyDisposable(() => { }); };

            m_sceneStack.Push(Tuple.Create(m_currentScene, m_currentResourceDictionary));
            m_currentScene = scene;
            m_currentResourceDictionary = resourceDictionary;

            return new DummyDisposable(() => this.PopScene());
        }

        /// <summary>
        /// Pops a scene from the stack.
        /// </summary>
        public void PopScene()
        {
            if (m_disposed) { throw new ObjectDisposedException("RenderState"); }
            if (m_sceneStack.Count < 0) { throw new SeeingSharpGraphicsException("There is only one element on the render stack!"); }

            var lowerContent = m_sceneStack.Pop();
            m_currentScene = lowerContent.Item1;
            m_currentResourceDictionary = lowerContent.Item2;
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
            RenderTargets renderTargets,
            SharpDX.Mathematics.Interop.RawViewportF viewport,
            Camera3DBase camera, ViewInformation viewInformation)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException("RenderState");
            }

            //Build new render stack entry
            var newEntry = new RenderStackEntry
            {
                Matrix4Stack = new Matrix4Stack(),
                Camera = camera,
                RenderTargets = renderTargets,
                SingleViewport = viewport,
                ViewInformation = viewInformation
            };

            //Overtake device settings
            newEntry.Apply(Device.DeviceImmediateContextD3D11);

            //Push new entry onto the stack
            m_renderSettingsStack.Push(m_currentRenderSettings);
            m_currentRenderSettings = newEntry;
        }

        /// <summary>
        /// Pops a render target from the render target stack.
        /// </summary>
        public void PopRenderTarget()
        {
            if (m_disposed) { throw new ObjectDisposedException("RenderState"); }
            if (m_renderSettingsStack.Count < 1) { throw new SeeingSharpGraphicsException("There is only one element on the render stack!"); }

            //Pop last entry
            m_currentRenderSettings = m_renderSettingsStack.Pop();

            //Apply old configuration
            m_currentRenderSettings.Apply(Device.DeviceImmediateContextD3D11);
        }

        /// <summary>
        /// Applies current target settings.
        /// </summary>
        public void ApplyCurrentTarget()
        {
            if (m_disposed) { throw new ObjectDisposedException("RenderState"); }

            if (m_currentRenderSettings != null)
            {
                m_currentRenderSettings.Apply(Device.DeviceImmediateContextD3D11);
            }
        }

        /// <summary>
        /// Generates a WorldViewProjection matrix.
        /// </summary>
        /// <param name="world">The world matrix.</param>
        public Matrix GenerateWorldViewProj(Matrix world)
        {
            if (m_disposed) { throw new ObjectDisposedException("RenderState"); }

            return world * m_currentRenderSettings.ViewProj;
        }

        /// <summary>
        /// Gets current Device object.
        /// </summary>
        public EngineDevice Device { get; }

        /// <summary>
        /// Gets the ViewProj matrix.
        /// </summary>
        public Matrix ViewProj
        {
            get { return m_currentRenderSettings.ViewProj; }
        }

        /// <summary>
        /// Gets current world matrix.
        /// </summary>
        /// <value>The world.</value>
        public Matrix4Stack World
        {
            get { return m_world; }
        }

        /// <summary>
        /// Gets current scene object.
        /// </summary>
        public Scene CurrentScene
        {
            get { return m_currentScene; }
        }

        /// <summary>
        /// Gets the current ResourceDictionary object.
        /// </summary>
        public ResourceDictionary CurrentResources
        {
            get { return m_currentResourceDictionary; }
        }

        /// <summary>
        /// Gets current camera.
        /// </summary>
        public Camera3DBase Camera
        {
            get
            {
                if (m_currentRenderSettings != null) { return m_currentRenderSettings.Camera; }
                else { return null; }
            }
        }

        /// <summary>
        /// Gets current common information about the view.
        /// </summary>
        public ViewInformation ViewInformation
        {
            get
            {
                if (m_currentRenderSettings != null) { return m_currentRenderSettings.ViewInformation; }
                else { return null; }
            }
        }

        /// <summary>
        /// Gets the current main viewport.
        /// </summary>
        internal SharpDX.Mathematics.Interop.RawViewportF Viewport
        {
            get
            {
                if (m_currentRenderSettings == null) { return new SharpDX.Mathematics.Interop.RawViewportF(); }
                return m_currentRenderSettings.SingleViewport;
            }
        }

        /// <summary>
        /// Is this object disposed?
        /// </summary>
        public bool Disposed
        {
            get { return m_disposed; }
        }

        /// <summary>
        /// Gets or sets the current view index.
        /// </summary>
        internal int ViewIndex;

        /// <summary>
        /// Gets or sets the current device index.
        /// </summary>
        internal int DeviceIndex;

        //internal int LastRenderBlockID;

        /// <summary>
        /// Gets or sets the current render target for 2D rendering.
        /// </summary>
        internal D2D.RenderTarget RenderTarget2D;

        /// <summary>
        /// Gets the currently forced material.
        /// </summary>
        public MaterialResource ForcedMaterial
        {
            get { return m_forcedMaterial; }
        }

        /// <summary>
        /// Gets or sets the current object for 2D rendering.
        /// </summary>
        public Graphics2D Graphics2D
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the currently bound render targets (on top of the render stack).
        /// </summary>
        internal RenderTargets CurrentRenderTargets
        {
            get
            {
                if (m_currentRenderSettings == null) { return new RenderTargets(); }
                return m_currentRenderSettings.RenderTargets;
            }
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// HelperClass for RenderState class
        /// </summary>
        private class RenderStackEntry
        {
            public Matrix4Stack Matrix4Stack;
            public Camera3DBase Camera;
            public RenderTargets RenderTargets;
            public SharpDX.Mathematics.Interop.RawViewportF SingleViewport;
            public ViewInformation ViewInformation;

            // Local array which store alls rendertargets for usage
            private D3D11.RenderTargetView[] m_targetArray;

            /// <summary>
            /// Initializes a new instance of the <see cref="RenderStackEntry"/> class.
            /// </summary>
            public RenderStackEntry()
            {

            }

            /// <summary>
            /// Gets the current view projection matrix.
            /// </summary>
            public Matrix ViewProj
            {
                get { return Camera.ViewProjection; }
            }

            /// <summary>
            /// Applies all properties.
            /// </summary>
            /// <param name="deviceContext">Target DeviceContext object.</param>
            public void Apply(D3D11.DeviceContext deviceContext)
            {
                // Create render target array (if not done before)
                if (m_targetArray == null)
                {
                    m_targetArray = new D3D11.RenderTargetView[3];
                    m_targetArray[0] = RenderTargets.ColorBuffer;
                    m_targetArray[1] = RenderTargets.ObjectIDBuffer;
                    m_targetArray[2] = RenderTargets.NormalDepthBuffer;
                }

                // Set render targets to output merger
                //deviceContext.Rasterizer.SetViewport(SingleViewport);
                deviceContext.Rasterizer.SetViewports(new SharpDX.Mathematics.Interop.RawViewportF[] { SingleViewport, SingleViewport, SingleViewport });
                deviceContext.OutputMerger.SetTargets(RenderTargets.DepthStencilBuffer, m_targetArray);
            }
        }
    }
}
