#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Util;
using SeeingSharp.Checking;

// Some namespace mappings
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using D2D = SharpDX.Direct2D1;
using DWrite = SharpDX.DirectWrite;

namespace SeeingSharp.Multimedia.Core
{
    public class EngineDevice
    {
        #region Constants
        private const string CATEGORY_ADAPTER = "Adapter";
        #endregion

        #region Main members
        private EngineDeviceInternals m_internals;
        private DXGI.Adapter1 m_adapter1;
        private DXGI.AdapterDescription1 m_adapterDesc1;
        private GraphicsDeviceConfiguration m_configuration;
        private DeviceLoadSettings m_deviceLoadSettings;
        private EngineFactory m_engineFactory;
        private bool m_isSoftwareAdapter;
        private Exception m_initializationException;
        #endregion

        #region Some configuration
        private bool m_isDetailLevelForced;
        private DetailLevel m_forcedDetailLevel;
        #endregion

        #region Handlers for different DirectX Apis
        private DeviceHandlerDXGI m_handlerDXGI;
        private DeviceHandlerD3D11 m_handlerD3D11;
        private DeviceHandlerD2D m_handlerD2D;
        private List<IDisposable> m_additionalDeviceHandlers;
        #endregion

        #region Possible antialiasing modes
        private DXGI.SampleDescription m_antialiasingConfigLow;
        private DXGI.SampleDescription m_antialiasingConfigMedium;
        private DXGI.SampleDescription m_antialiasingConfigHigh;
        #endregion

        #region Members for antialiasing
        private bool m_isStandardAntialiasingSupported;
        private DXGI.SampleDescription m_sampleDescWithAntialiasing;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineDevice"/> class.
        /// </summary>
        internal EngineDevice(
            DeviceLoadSettings loadSettings, SeeingSharpInitializer initializer,
            EngineFactory engineFactory, GraphicsCoreConfiguration coreConfiguration, 
            DXGI.Adapter1 adapter, bool isSoftwareAdapter)
        {
            loadSettings.EnsureNotNull(nameof(loadSettings));
            engineFactory.EnsureNotNull(nameof(engineFactory));
            coreConfiguration.EnsureNotNull(nameof(coreConfiguration));
            adapter.EnsureNotNull(nameof(adapter));

            m_internals = new EngineDeviceInternals(this);

            m_additionalDeviceHandlers = new List<IDisposable>();

            m_engineFactory = engineFactory;
            m_deviceLoadSettings = loadSettings;
            m_adapter1 = adapter;
            m_adapterDesc1 = m_adapter1.Description1;
            m_isSoftwareAdapter = isSoftwareAdapter;
            m_configuration = new GraphicsDeviceConfiguration(coreConfiguration);

            // Set default antialiasing configurations
            m_sampleDescWithAntialiasing = new DXGI.SampleDescription(1, 0);

            // Initialize all direct3D APIs
            try
            {
                m_handlerD3D11 = new DeviceHandlerD3D11(m_deviceLoadSettings, adapter);
                m_handlerDXGI = new DeviceHandlerDXGI(adapter, m_handlerD3D11.Device1);
            }
            catch (Exception ex)
            {
                m_initializationException = ex;
                m_handlerD3D11 = null;
                m_handlerDXGI = null;
            }

            // Set default configuration
            m_configuration.TextureQuality = !isSoftwareAdapter && m_handlerD3D11.IsDirect3D10OrUpperHardware ? TextureQuality.Hight : TextureQuality.Low;
            m_configuration.GeometryQuality = !isSoftwareAdapter && m_handlerD3D11.IsDirect3D10OrUpperHardware ? GeometryQuality.Hight : GeometryQuality.Low;

            // Initialize handlers for feature support information
            if (m_initializationException == null)
            {
                m_isStandardAntialiasingSupported = CheckIsStandardAntialiasingPossible();
            }

            // Initialize direct2D handler finally
            if (m_handlerD3D11 != null)
            {
                m_handlerD2D = new DeviceHandlerD2D(m_deviceLoadSettings, m_engineFactory, this);
                this.FakeRenderTarget2D = m_handlerD2D.RenderTarget;
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
        public DXGI.SampleDescription GetSampleDescription(AntialiasingQualityLevel qualityLevel)
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

            return new DXGI.SampleDescription(1, 0);
        }

        /// <summary>
        /// Get the sample description for the given quality level.
        /// </summary>
        internal DXGI.SampleDescription GetSampleDescription(bool antialiasingEnabled)
        {
            if (antialiasingEnabled) { return m_sampleDescWithAntialiasing; }
            else { return new DXGI.SampleDescription(1, 0); }
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
            //if (m_initializationException != null) { return m_adapter1.Description1.Description; }
            //else { return m_handlerD3D11.DeviceModeDescription; }
        }

        /// <summary>
        /// Checks for standard antialiasing support.
        /// </summary>
        private bool CheckIsStandardAntialiasingPossible()
        {
            // Very important to check possible antialiasing
            // More on the used technique
            //  see http://msdn.microsoft.com/en-us/library/windows/apps/dn458384.aspx

            D3D11.FormatSupport formatSupport = m_handlerD3D11.Device1.CheckFormatSupport(GraphicsHelper.DEFAULT_TEXTURE_FORMAT);
            if ((formatSupport & D3D11.FormatSupport.MultisampleRenderTarget) != D3D11.FormatSupport.MultisampleRenderTarget) { return false; }
            if ((formatSupport & D3D11.FormatSupport.MultisampleResolve) != D3D11.FormatSupport.MultisampleResolve) { return false; }
            if (m_handlerD3D11.FeatureLevel == SharpDX.Direct3D.FeatureLevel.Level_9_1) { return false; }

            try
            {
                D3D11.Texture2DDescription textureDescription = new D3D11.Texture2DDescription();
                textureDescription.Width = 100;
                textureDescription.Height = 100;
                textureDescription.MipLevels = 1;
                textureDescription.ArraySize = 1;
                textureDescription.Format = GraphicsHelper.DEFAULT_TEXTURE_FORMAT;
                textureDescription.Usage = D3D11.ResourceUsage.Default;
                textureDescription.SampleDescription = new DXGI.SampleDescription(2, 0);
                textureDescription.BindFlags = D3D11.BindFlags.ShaderResource | D3D11.BindFlags.RenderTarget;
                textureDescription.CpuAccessFlags = D3D11.CpuAccessFlags.None;
                textureDescription.OptionFlags = D3D11.ResourceOptionFlags.None;
                D3D11.Texture2D testTexture = new D3D11.Texture2D(m_handlerD3D11.Device1, textureDescription);
                SeeingSharpUtil.SafeDispose(ref testTexture);
            }
            catch(Exception)
            {
                return false;
            }

            // Check for quality levels
            int lowQualityLevels = m_handlerD3D11.Device1.CheckMultisampleQualityLevels(GraphicsHelper.DEFAULT_TEXTURE_FORMAT, 2);
            int mediumQualityLevels = m_handlerD3D11.Device1.CheckMultisampleQualityLevels(GraphicsHelper.DEFAULT_TEXTURE_FORMAT, 4);
            int hightQualityLevels = m_handlerD3D11.Device1.CheckMultisampleQualityLevels(GraphicsHelper.DEFAULT_TEXTURE_FORMAT, 8);

            // Generate sample descriptions for each possible quality level
            if (lowQualityLevels > 0)
            {
                m_antialiasingConfigLow = new DXGI.SampleDescription(2, lowQualityLevels - 1);
            }
            if (mediumQualityLevels > 0)
            {
                m_antialiasingConfigMedium = new DXGI.SampleDescription(4, mediumQualityLevels - 1);
            }
            if (hightQualityLevels > 0)
            {
                m_antialiasingConfigHigh = new DXGI.SampleDescription(8, hightQualityLevels - 1);
            }

            return lowQualityLevels > 0;
        }

        /// <summary>
        /// Checks for standard antialiasing support.
        /// </summary>
        public bool IsStandardAntialiasingPossible
        {
            get 
            {
                return m_isStandardAntialiasingSupported; 
            }
        }

        /// <summary>
        /// Gets the exception occurred during initialization of the driver (if any).
        /// </summary>
        public Exception InitializationException
        {
            get { return m_initializationException; }
        }

        /// <summary>
        /// Gets the description of this adapter.
        /// </summary>
        public string AdapterDescription
        {
            get { return m_adapter1?.Description1.Description.Replace("\0", "") ?? nameof(EngineDevice); }
        }

        /// <summary>
        /// Is this device loaded successfully.
        /// </summary>
        public bool IsLoadedSuccessfully
        {
            get { return m_initializationException == null; }
        }

        public bool IsSoftware
        {
            get { return m_isSoftwareAdapter; }
        }

        /// <summary>
        /// Gets the supported detail level of this device.
        /// </summary>
        public DetailLevel SupportedDetailLevel
        {
            get
            {
                if (m_isDetailLevelForced) { return m_forcedDetailLevel; }

                if (m_isSoftwareAdapter) { return DetailLevel.Low; }
                else { return DetailLevel.High; }
            }
        }

        /// <summary>
        /// Is high detail supported with this card?
        /// </summary>
        public bool IsHighDetailSupported
        {
            get
            {
                return (this.SupportedDetailLevel | DetailLevel.High) == DetailLevel.High;
            }
        }

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
        public bool SupportsOnly16BitIndexBuffer
        {
            get
            {
                // see https://msdn.microsoft.com/en-us/library/windows/desktop/ff471324(v=vs.85).aspx
                return DriverLevel == HardwareDriverLevel.Direct3D9_1;
            }
        }

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
        public D3D11.Device1 DeviceD3D11_1
        {
            get { return m_handlerD3D11.Device1; }
        }

        public D3D11.Device3 Device3D11_3
        {
            get { return m_handlerD3D11.Device3; }
        }

        public D2D.Device DeviceD2D
        {
            get { return m_handlerD2D.Device; }
        }

        public D2D.DeviceContext DeviceContextD2D
        {
            get { return m_handlerD2D.DeviceContext; }
        }

        public DXGI.Device3 DeviceDxgi
        {
            get { return m_handlerDXGI.Device; }
        }

        /// <summary>
        /// A unique value that identifies the adapter.
        /// </summary>
        public long Luid
        {
            get { return m_adapterDesc1.Luid; }
        }

        public bool Supports2D
        {
            get
            {
                return
                    (m_handlerD2D != null) &&
                    (m_handlerD2D.IsLoaded);
            }
        }

        /// <summary>
        /// Gets the main Direct3D 11 context object.
        /// </summary>
        public D3D11.DeviceContext DeviceImmediateContextD3D11
        {
            get { return m_handlerD3D11.ImmediateContext; }
        }

        /// <summary>
        /// Gets the current device configuration.
        /// </summary>
        public GraphicsDeviceConfiguration Configuration
        {
            get { return m_configuration; }
        }

        /// <summary>
        /// Gets the DXGI factory object.
        /// </summary>
        public DXGI.Factory2 FactoryDxgi
        {
            get
            {
                return m_handlerDXGI.Factory;
            }
        }

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
        public EngineDeviceInternals Internals => m_internals;

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

            public DXGI.Adapter1 Adapter => m_host.m_adapter1;

            public DXGI.AdapterDescription1 AdapterDescription => m_host.m_adapterDesc1;
        }
    }
}
