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
using System.Collections.Generic;
using SeeingSharp.Checking;
using SeeingSharp.Util;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using D2D = SharpDX.Direct2D1;
using DWrite = SharpDX.DirectWrite;

namespace SeeingSharp.Multimedia.Core
{
    public class EngineDevice
    {
        // Members for antialiasing
        private SampleDescription m_sampleDescWithAntialiasing;

        // Main members
        private SeeingSharpLoader m_initializer;
        private EngineFactory m_engineFactory;
        private DeviceLoadSettings m_deviceLoadSettings;
        private EngineHardwareInfo m_hardwareInfo;
        private EngineAdapterInfo m_adapterInfo;

        // Some configuration
        private bool m_isDetailLevelForced;
        private DetailLevel m_forcedDetailLevel;

        // Handlers for different DirectX Apis
        private DeviceHandlerDXGI m_handlerDXGI;
        private DeviceHandlerD3D11 m_handlerD3D11;
        private DeviceHandlerD2D m_handlerD2D;
        private List<IDisposable> m_additionalDeviceHandlers;

        // Device resources
        private DeviceResourceManager m_deviceResources;

        // Possible antialiasing modes
        private SampleDescription m_antialiasingConfigLow;
        private SampleDescription m_antialiasingConfigMedium;
        private SampleDescription m_antialiasingConfigHigh;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineDevice"/> class.
        /// </summary>
        internal EngineDevice(
            DeviceLoadSettings loadSettings, SeeingSharpLoader initializer,
            EngineFactory engineFactory, GraphicsCoreConfiguration coreConfiguration,
            EngineHardwareInfo hardwareInfo, EngineAdapterInfo adapterInfo)
        {
            loadSettings.EnsureNotNull(nameof(loadSettings));
            engineFactory.EnsureNotNull(nameof(engineFactory));
            coreConfiguration.EnsureNotNull(nameof(coreConfiguration));
            hardwareInfo.EnsureNotNull(nameof(hardwareInfo));
            adapterInfo.EnsureNotNull(nameof(adapterInfo));

            this.Internals = new EngineDeviceInternals(this);

            m_deviceResources = new DeviceResourceManager(this);

            m_additionalDeviceHandlers = new List<IDisposable>();

            m_hardwareInfo = hardwareInfo;
            m_adapterInfo = adapterInfo;

            m_deviceLoadSettings = loadSettings;
            m_initializer = initializer;
            m_engineFactory = engineFactory;
            this.IsSoftware = adapterInfo.IsSoftwareAdapter;

            // Set default configuration
            this.Configuration = new GraphicsDeviceConfiguration(coreConfiguration);
            this.Configuration.TextureQuality = !this.IsSoftware ? TextureQuality.High : TextureQuality.Low;
            this.Configuration.GeometryQuality = !this.IsSoftware ? GeometryQuality.High : GeometryQuality.Low;

            // Set default antialiasing configurations
            m_sampleDescWithAntialiasing = new SampleDescription(1, 0);

            // Load all resources
            this.LoadResources();
        }

        public T TryGetAdditionalDeviceHandler<T>()
            where T : class
        {
            foreach (var actAdditionalDeviceHandler in m_additionalDeviceHandlers)
            {
                if (actAdditionalDeviceHandler is T result) { return result; }
            }

            return null;
        }

        /// <summary>
        /// Forces the given detail level.
        /// </summary>
        public void ForceDetailLevel(DetailLevel detailLevel)
        {
            m_isDetailLevelForced = true;
            m_forcedDetailLevel = detailLevel;
        }

        /// <summary>
        /// Get the sample description for the given quality level.
        /// </summary>
        /// <param name="qualityLevel">The quality level for which a sample description is needed.</param>
        internal SampleDescription GetSampleDescription(AntialiasingQualityLevel qualityLevel)
        {
            switch (qualityLevel)
            {
                case AntialiasingQualityLevel.Low:
                    return m_antialiasingConfigLow;

                case AntialiasingQualityLevel.Medium:
                    return m_antialiasingConfigMedium;

                case AntialiasingQualityLevel.High:
                    return m_antialiasingConfigHigh;
            }

            return new SampleDescription(1, 0);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.AdapterDescription;
        }

        internal void RegisterDeviceResource(IEngineDeviceResource resource)
        {
            m_deviceResources.RegisterDeviceResource(resource);
        }

        internal void DeregisterDeviceResource(IEngineDeviceResource resource)
        {
            m_deviceResources.DeregisterDeviceResource(resource);
        }

        internal void CleanupDeviceResourceCollection()
        {
            if (m_deviceResources.CleanupNeeded)
            {
                m_deviceResources.Cleanup();
            }
        }

        /// <summary>
        /// Recreates this device after a device lost event.
        /// </summary>
        internal void RecreateAfterDeviceLost()
        {
            this.IsLost.EnsureTrue(nameof(this.IsLost));

            // Unload all resources first
            m_deviceResources.UnloadResources();
            this.UnloadResources();

            // Try to restore the device
            var successfullyLoaded = false;
            try
            {
                successfullyLoaded = this.LoadResources();
            }
            catch (Exception)
            {
                return;
            }

            // Reset is lost flag
            if (successfullyLoaded)
            {
                this.LoadDeviceIndex++;
                this.IsLost = false;
            }
        }

        /// <summary>
        /// Get the sample description for the given quality level.
        /// </summary>
        internal SampleDescription GetSampleDescription(bool antialiasingEnabled)
        {
            if (antialiasingEnabled) { return m_sampleDescWithAntialiasing; }
            return new SampleDescription(1, 0);
        }

        /// <summary>
        /// Loads all device resources.
        /// </summary>
        private bool LoadResources()
        {
            // Initialize all direct3D APIs
            this.LoadDeviceIndex++;
            try
            {
                m_handlerDXGI = new DeviceHandlerDXGI(m_hardwareInfo, m_adapterInfo);
                m_handlerD3D11 = new DeviceHandlerD3D11(m_deviceLoadSettings, m_handlerDXGI.Adapter);
            }
            catch (Exception ex)
            {
                this.InitializationException = ex;
                this.UnloadResources();
                return false;
            }

            // Initialize handlers for feature support information
            if (this.InitializationException == null)
            {
                this.IsStandardAntialiasingPossible = this.CheckIsStandardAntialiasingPossible();
            }

            // Initialize direct2D handler finally
            if (m_handlerD3D11 != null)
            {
                m_handlerD2D = new DeviceHandlerD2D(m_deviceLoadSettings, m_engineFactory, this);
                this.FakeRenderTarget2D = m_handlerD2D.RenderTarget;
            }

            // Create additional device handlers
            foreach (var actExtension in m_initializer.Extensions)
            {
                foreach (var actAdditionalDeviceHandler in actExtension.CreateAdditionalDeviceHandlers(this))
                {
                    if (actAdditionalDeviceHandler == null)
                    {
                        continue;
                    }
                    m_additionalDeviceHandlers.Add(actAdditionalDeviceHandler);
                }
            }

            return true;
        }

        /// <summary>
        /// Unloads all device resources.
        /// </summary>
        private void UnloadResources()
        {
            foreach (var actAdditionalDeviceHandler in m_additionalDeviceHandlers)
            {
                SeeingSharpUtil.DisposeObject(actAdditionalDeviceHandler);
            }
            m_additionalDeviceHandlers.Clear();

            m_handlerD2D?.UnloadResources();
            m_handlerD2D = null;
            this.FakeRenderTarget2D = null;

            m_handlerD3D11?.UnloadResources();
            m_handlerD3D11 = null;

            m_handlerDXGI?.UnloadResources();
            m_handlerDXGI = null;
        }

        /// <summary>
        /// Checks for standard antialiasing support.
        /// </summary>
        private bool CheckIsStandardAntialiasingPossible()
        {
            // Very important to check possible antialiasing
            // More on the used technique
            //  see http://msdn.microsoft.com/en-us/library/windows/apps/dn458384.aspx

            var formatSupport = m_handlerD3D11.Device1.CheckFormatSupport(GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT);

            if ((formatSupport & D3D11.FormatSupport.MultisampleRenderTarget) != D3D11.FormatSupport.MultisampleRenderTarget) { return false; }
            if ((formatSupport & D3D11.FormatSupport.MultisampleResolve) != D3D11.FormatSupport.MultisampleResolve) { return false; }
            if (m_handlerD3D11.FeatureLevel == FeatureLevel.Level_9_1) { return false; }

            try
            {
                var textureDescription = new D3D11.Texture2DDescription
                {
                    Width = 100,
                    Height = 100,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT,
                    Usage = D3D11.ResourceUsage.Default,
                    SampleDescription = new SampleDescription(2, 0),
                    BindFlags = D3D11.BindFlags.ShaderResource | D3D11.BindFlags.RenderTarget,
                    CpuAccessFlags = D3D11.CpuAccessFlags.None,
                    OptionFlags = D3D11.ResourceOptionFlags.None
                };

                var testTexture = new D3D11.Texture2D(m_handlerD3D11.Device1, textureDescription);
                SeeingSharpUtil.SafeDispose(ref testTexture);
            }
            catch(Exception)
            {
                return false;
            }

            // Check for quality levels
            var lowQualityLevels = m_handlerD3D11.Device1.CheckMultisampleQualityLevels(GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT, 2);
            var mediumQualityLevels = m_handlerD3D11.Device1.CheckMultisampleQualityLevels(GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT, 4);
            var highQualityLevels = m_handlerD3D11.Device1.CheckMultisampleQualityLevels(GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT, 8);

            // Generate sample descriptions for each possible quality level
            if (lowQualityLevels > 0)
            {
                m_antialiasingConfigLow = new SampleDescription(2, lowQualityLevels - 1);
            }
            if (mediumQualityLevels > 0)
            {
                m_antialiasingConfigMedium = new SampleDescription(4, mediumQualityLevels - 1);
            }
            if (highQualityLevels > 0)
            {
                m_antialiasingConfigHigh = new SampleDescription(8, highQualityLevels - 1);
            }

            return lowQualityLevels > 0;
        }

        /// <summary>
        /// Checks for standard antialiasing support.
        /// </summary>
        public bool IsStandardAntialiasingPossible { get; private set; }

        /// <summary>
        /// Gets the exception occurred during initialization of the driver (if any).
        /// </summary>
        public Exception InitializationException { get; private set; }

        /// <summary>
        /// Gets the description of this adapter.
        /// </summary>
        public string AdapterDescription => m_adapterInfo.AdapterDescription;

        /// <summary>
        /// Is this device loaded successfully.
        /// </summary>
        public bool IsLoadedSuccessfully => this.InitializationException == null;

        public bool IsSoftware { get; }

        /// <summary>
        /// Gets the supported detail level of this device.
        /// </summary>
        public DetailLevel SupportedDetailLevel
        {
            get
            {
                if (m_isDetailLevelForced) { return m_forcedDetailLevel; }

                if (this.IsSoftware) { return DetailLevel.Low; }
                return DetailLevel.High;
            }
        }

        /// <summary>
        /// Is high detail supported with this card?
        /// </summary>
        public bool IsHighDetailSupported => (this.SupportedDetailLevel | DetailLevel.High) == DetailLevel.High;

        /// <summary>
        /// Gets the level of the graphics driver.
        /// </summary>
        public HardwareDriverLevel DriverLevel
        {
            get
            {
                if (m_handlerD3D11 != null) { return m_handlerD3D11.DriverLevel; }
                return HardwareDriverLevel.Direct3D11;
            }
        }

        /// <summary>
        /// Gets the name of the default shader model.
        /// </summary>
        public string DefaultPixelShaderModel
        {
            get
            {
                switch(this.DriverLevel)
                {
                    case HardwareDriverLevel.Direct3D12:
                    case HardwareDriverLevel.Direct3D11:
                        return "ps_5_0";

                    case HardwareDriverLevel.Direct3D10:
                        return "ps_4_0";

                    default:
                        throw new SeeingSharpGraphicsException($"Unable to get shader model for DriverLevel {this.DriverLevel}!");
                }
            }
        }

        /// <summary>
        /// Gets the name of the default shader model.
        /// </summary>
        public string DefaultVertexShaderModel
        {
            get
            {
                switch (this.DriverLevel)
                {
                    case HardwareDriverLevel.Direct3D12:
                    case HardwareDriverLevel.Direct3D11:
                        return "vs_5_0";

                    case HardwareDriverLevel.Direct3D10:
                        return "vs_4_0";

                    default:
                        throw new SeeingSharpGraphicsException($"Unable to get shader model for DriverLevel {this.DriverLevel}!");
                }
            }
        }

        /// <summary>
        /// Gets the Direct3D 11 device object.
        /// </summary>
        internal D3D11.Device1 DeviceD3D11_1 => m_handlerD3D11.Device1;

        internal D2D.Device DeviceD2D => m_handlerD2D.Device;

        internal D2D.DeviceContext DeviceContextD2D => m_handlerD2D.DeviceContext;

        /// <summary>
        /// A unique value that identifies the adapter.
        /// </summary>
        public long Luid => m_adapterInfo.Luid;

        public bool Supports2D =>
            m_handlerD2D != null &&
            m_handlerD2D.IsLoaded;

        /// <summary>
        /// Gets the main Direct3D 11 context object.
        /// </summary>
        internal D3D11.DeviceContext DeviceImmediateContextD3D11 => m_handlerD3D11.ImmediateContext;

        /// <summary>
        /// Gets the current device configuration.
        /// </summary>
        public GraphicsDeviceConfiguration Configuration { get; }

        /// <summary>
        /// Gets the DXGI factory object.
        /// </summary>
        internal Factory2 FactoryDxgi => m_handlerDXGI.Factory;

        /// <summary>
        /// Gets the 2D render target which can be used to load 2D resources on this device.
        /// </summary>
        internal D2D.RenderTarget FakeRenderTarget2D
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the index of this device.
        /// </summary>
        public int DeviceIndex
        {
            get;
            internal set;
        }

        /// <summary>
        /// Internal property for device lost handling.
        /// This value gets incremented each time when a device is recreated after a device lost event.
        /// </summary>
        internal int LoadDeviceIndex { get; private set; }

        /// <summary>
        /// This property is set to true if the device got lost.
        /// </summary>
        public bool IsLost
        {
            get;
            internal set;
        }

        /// <summary>
        /// Is debug mode enabled?
        /// </summary>
        public bool DebugEnabled => m_deviceLoadSettings.DebugEnabled;

        /// <summary>
        /// Internal members, use with care.
        /// </summary>
        public EngineDeviceInternals Internals { get; }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        public class EngineDeviceInternals
        {
            private EngineDevice m_host;

            internal EngineDeviceInternals(EngineDevice host)
            {
                m_host = host;
            }

            public void RegisterDeviceResource(IEngineDeviceResource resource)
            {
                m_host.RegisterDeviceResource(resource);
            }

            public void DeregisterDeviceResource(IEngineDeviceResource resource)
            {
                m_host.DeregisterDeviceResource(resource);
            }

            public SampleDescription GetSampleDescription(AntialiasingQualityLevel qualityLevel)
            {
                return m_host.GetSampleDescription(qualityLevel);
            }

            public Adapter1 Adapter => m_host.m_handlerDXGI?.Adapter;

            public D2D.RenderTarget FakeRenderTarget2D => m_host.FakeRenderTarget2D;

            public Factory2 FactoryDxgi => m_host.FactoryDxgi;

            public D3D11.DeviceContext DeviceImmediateContextD3D11 => m_host.DeviceImmediateContextD3D11;

            public D2D.DeviceContext DeviceContextD2D => m_host.DeviceContextD2D;

            public D2D.Device DeviceD2D => m_host.DeviceD2D;

            public D3D11.Device1 DeviceD3D11_1 => m_host.DeviceD3D11_1;
        }
    }
}
