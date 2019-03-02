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
        // Constants
        private const string CATEGORY_ADAPTER = "Adapter";

        // Members for antialiasing
        private SampleDescription m_sampleDescWithAntialiasing;

        // Main members
        private Adapter1 m_adapter1;
        private AdapterDescription1 m_adapterDesc1;
        private DeviceLoadSettings m_deviceLoadSettings;
        private EngineFactory m_engineFactory;

        // Some configuration
        private bool m_isDetailLevelForced;
        private DetailLevel m_forcedDetailLevel;

        // Handlers for different DirectX Apis
        private DeviceHandlerDXGI m_handlerDXGI;
        private DeviceHandlerD3D11 m_handlerD3D11;
        private DeviceHandlerD2D m_handlerD2D;
        private List<IDisposable> m_additionalDeviceHandlers;

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
            Adapter1 adapter, bool isSoftwareAdapter)
        {
            loadSettings.EnsureNotNull(nameof(loadSettings));
            engineFactory.EnsureNotNull(nameof(engineFactory));
            coreConfiguration.EnsureNotNull(nameof(coreConfiguration));
            adapter.EnsureNotNull(nameof(adapter));

            Internals = new EngineDeviceInternals(this);

            m_additionalDeviceHandlers = new List<IDisposable>();

            m_engineFactory = engineFactory;
            m_deviceLoadSettings = loadSettings;
            m_adapter1 = adapter;
            m_adapterDesc1 = m_adapter1.Description1;
            IsSoftware = isSoftwareAdapter;
            Configuration = new GraphicsDeviceConfiguration(coreConfiguration);

            // Set default antialiasing configurations
            m_sampleDescWithAntialiasing = new SampleDescription(1, 0);

            // Initialize all direct3D APIs
            try
            {
                m_handlerD3D11 = new DeviceHandlerD3D11(m_deviceLoadSettings, adapter);
                m_handlerDXGI = new DeviceHandlerDXGI(adapter, m_handlerD3D11.Device1);
            }
            catch (Exception ex)
            {
                InitializationException = ex;
                m_handlerD3D11 = null;
                m_handlerDXGI = null;
            }

            // Set default configuration
            Configuration.TextureQuality = !isSoftwareAdapter && m_handlerD3D11.IsDirect3D10OrUpperHardware ? TextureQuality.Hight : TextureQuality.Low;
            Configuration.GeometryQuality = !isSoftwareAdapter && m_handlerD3D11.IsDirect3D10OrUpperHardware ? GeometryQuality.Hight : GeometryQuality.Low;

            // Initialize handlers for feature support information
            if (InitializationException == null)
            {
                IsStandardAntialiasingPossible = CheckIsStandardAntialiasingPossible();
            }

            // Initialize direct2D handler finally
            if (m_handlerD3D11 != null)
            {
                m_handlerD2D = new DeviceHandlerD2D(m_deviceLoadSettings, m_engineFactory, this);
                FakeRenderTarget2D = m_handlerD2D.RenderTarget;
            }

            // Create additional device handlers
            foreach(var actExtension in initializer.Extensions)
            {
                foreach(var actAdditionalDeviceHandler in actExtension.CreateAdditionalDeviceHandlers(this))
                {
                    if(actAdditionalDeviceHandler == null) { continue; }
                    m_additionalDeviceHandlers.Add(actAdditionalDeviceHandler);
                }
            }
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
        public SampleDescription GetSampleDescription(AntialiasingQualityLevel qualityLevel)
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
            return AdapterDescription;
            //if (m_initializationException != null) { return m_adapter1.Description1.Description; }
            //else { return m_handlerD3D11.DeviceModeDescription; }
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
        /// Checks for standard antialiasing support.
        /// </summary>
        private bool CheckIsStandardAntialiasingPossible()
        {
            // Very important to check possible antialiasing
            // More on the used technique
            //  see http://msdn.microsoft.com/en-us/library/windows/apps/dn458384.aspx

            var formatSupport = m_handlerD3D11.Device1.CheckFormatSupport(GraphicsHelper.DEFAULT_TEXTURE_FORMAT);

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
                    Format = GraphicsHelper.DEFAULT_TEXTURE_FORMAT,
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
            var lowQualityLevels = m_handlerD3D11.Device1.CheckMultisampleQualityLevels(GraphicsHelper.DEFAULT_TEXTURE_FORMAT, 2);
            var mediumQualityLevels = m_handlerD3D11.Device1.CheckMultisampleQualityLevels(GraphicsHelper.DEFAULT_TEXTURE_FORMAT, 4);
            var hightQualityLevels = m_handlerD3D11.Device1.CheckMultisampleQualityLevels(GraphicsHelper.DEFAULT_TEXTURE_FORMAT, 8);

            // Generate sample descriptions for each possible quality level
            if (lowQualityLevels > 0)
            {
                m_antialiasingConfigLow = new SampleDescription(2, lowQualityLevels - 1);
            }
            if (mediumQualityLevels > 0)
            {
                m_antialiasingConfigMedium = new SampleDescription(4, mediumQualityLevels - 1);
            }
            if (hightQualityLevels > 0)
            {
                m_antialiasingConfigHigh = new SampleDescription(8, hightQualityLevels - 1);
            }

            return lowQualityLevels > 0;
        }

        /// <summary>
        /// Checks for standard antialiasing support.
        /// </summary>
        public bool IsStandardAntialiasingPossible { get; }

        /// <summary>
        /// Gets the exception occurred during initialization of the driver (if any).
        /// </summary>
        public Exception InitializationException { get; }

        /// <summary>
        /// Gets the description of this adapter.
        /// </summary>
        public string AdapterDescription => m_adapter1?.Description1.Description.Replace("\0", "") ?? nameof(EngineDevice);

        /// <summary>
        /// Is this device loaded successfully.
        /// </summary>
        public bool IsLoadedSuccessfully => InitializationException == null;

        public bool IsSoftware { get; }

        /// <summary>
        /// Gets the supported detail level of this device.
        /// </summary>
        public DetailLevel SupportedDetailLevel
        {
            get
            {
                if (m_isDetailLevelForced) { return m_forcedDetailLevel; }

                if (IsSoftware) { return DetailLevel.Low; }
                return DetailLevel.High;
            }
        }

        /// <summary>
        /// Is high detail supported with this card?
        /// </summary>
        public bool IsHighDetailSupported => (SupportedDetailLevel | DetailLevel.High) == DetailLevel.High;

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
                switch(DriverLevel)
                {
                    case HardwareDriverLevel.Direct3D9_1:
                    case HardwareDriverLevel.Direct3D9_2:
                        return "ps_4_0_level_9_1";

                    case HardwareDriverLevel.Direct3D9_3:
                        return "ps_4_0_level_9_3";

                    case HardwareDriverLevel.Direct3D11:
                        return "ps_5_0";

                    default:
                        return "ps_4_0";
                }
            }
        }

        /// <summary>
        /// Some older hardware only support 16-bit index buffers.
        /// </summary>
        public bool SupportsOnly16BitIndexBuffer => DriverLevel == HardwareDriverLevel.Direct3D9_1;

        /// <summary>
        /// Gets the name of the default shader model.
        /// </summary>
        public string DefaultVertexShaderModel
        {
            get
            {
                switch (DriverLevel)
                {
                    case HardwareDriverLevel.Direct3D9_1:
                    case HardwareDriverLevel.Direct3D9_2:
                        return "vs_4_0_level_9_1";

                    case HardwareDriverLevel.Direct3D9_3:
                        return "vs_4_0_level_9_3";

                    case HardwareDriverLevel.Direct3D11:
                        return "vs_5_0";

                    default:
                        return "vs_4_0";
                }
            }
        }

        /// <summary>
        /// Gets the Direct3D 11 device object.
        /// </summary>
        public D3D11.Device1 DeviceD3D11_1 => m_handlerD3D11.Device1;

        public D3D11.Device3 Device3D11_3 => m_handlerD3D11.Device3;

        public D2D.Device DeviceD2D => m_handlerD2D.Device;

        public D2D.DeviceContext DeviceContextD2D => m_handlerD2D.DeviceContext;

        public Device3 DeviceDxgi => m_handlerDXGI.Device;

        /// <summary>
        /// A unique value that identifies the adapter.
        /// </summary>
        public long Luid => m_adapterDesc1.Luid;

        public bool Supports2D =>
            m_handlerD2D != null &&
            m_handlerD2D.IsLoaded;

        /// <summary>
        /// Gets the main Direct3D 11 context object.
        /// </summary>
        public D3D11.DeviceContext DeviceImmediateContextD3D11 => m_handlerD3D11.ImmediateContext;

        /// <summary>
        /// Gets the current device configuration.
        /// </summary>
        public GraphicsDeviceConfiguration Configuration { get; }

        /// <summary>
        /// Gets the DXGI factory object.
        /// </summary>
        public Factory2 FactoryDxgi => m_handlerDXGI.Factory;

        /// <summary>
        /// Gets the 2D render target which can be used to load 2D resources on this device.
        /// </summary>
        public D2D.RenderTarget FakeRenderTarget2D
        {
            get;
            internal set;
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

            public Adapter1 Adapter => m_host.m_adapter1;

            public AdapterDescription1 AdapterDescription => m_host.m_adapterDesc1;
        }
    }
}
