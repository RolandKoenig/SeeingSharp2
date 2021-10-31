/*
    SeeingSharp and all applications distributed together with it. 
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
using D2D = SharpDX.Direct2D1;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Core
{
    public class EngineDevice : IDisposable, ICheckDisposed
    {
        // Members for antialiasing
        private SampleDescription _sampleDescWithAntialiasing;

        // Main members
        private ISeeingSharpExtensionProvider _extensionProvider;
        private EngineFactory _engineFactory;
        private EngineAdapterInfo _adapterInfo;
        private bool _isDisposed;

        // Some configuration
        private bool _isDetailLevelForced;
        private DetailLevel _forcedDetailLevel;

        // Handlers for different DirectX Apis
        private DeviceHandlerDXGI _handlerDXGI;
        private DeviceHandlerD3D11 _handlerD3D11;
        private DeviceHandlerD2D _handlerD2D;
        private List<IDisposable> _additionalDeviceHandlers;

        // Device resources
        private DeviceResourceManager _deviceResources;

        // Possible antialiasing modes
        private SampleDescription _antialiasingConfigLow;
        private SampleDescription _antialiasingConfigMedium;
        private SampleDescription _antialiasingConfigHigh;

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
        public string AdapterDescription => _adapterInfo.AdapterDescription;

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
                if (_isDetailLevelForced) { return _forcedDetailLevel; }

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
                if (_handlerD3D11 != null) { return _handlerD3D11.DriverLevel; }
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
                return this.DriverLevel switch
                {
                    HardwareDriverLevel.Direct3D12 => "ps_5_0",
                    HardwareDriverLevel.Direct3D11 => "ps_5_0",
                    HardwareDriverLevel.Direct3D10 => "ps_4_0",
                    _ => throw new SeeingSharpGraphicsException(
                        $"Unable to get shader model for DriverLevel {this.DriverLevel}!")
                };
            }
        }

        /// <summary>
        /// Gets the name of the default shader model.
        /// </summary>
        public string DefaultVertexShaderModel
        {
            get
            {
                return this.DriverLevel switch
                {
                    HardwareDriverLevel.Direct3D12 => "vs_5_0",
                    HardwareDriverLevel.Direct3D11 => "vs_5_0",
                    HardwareDriverLevel.Direct3D10 => "vs_4_0",
                    _ => throw new SeeingSharpGraphicsException(
                        $"Unable to get shader model for DriverLevel {this.DriverLevel}!")
                };
            }
        }

        /// <summary>
        /// Gets the name of the default shader model.
        /// </summary>
        public string DefaultGeometryShaderModel
        {
            get
            {
                return this.DriverLevel switch
                {
                    HardwareDriverLevel.Direct3D12 => "gs_5_0",
                    HardwareDriverLevel.Direct3D11 => "gs_5_0",
                    HardwareDriverLevel.Direct3D10 => "gs_4_0",
                    _ => throw new SeeingSharpGraphicsException(
                        $"Unable to get shader model for DriverLevel {this.DriverLevel}!")
                };
            }
        }

        /// <summary>
        /// Gets the Direct3D 11 device object.
        /// </summary>
        internal D3D11.Device1 DeviceD3D11_1 => _handlerD3D11.Device1;

        internal D2D.Device DeviceD2D => _handlerD2D.Device;

        internal D2D.DeviceContext DeviceContextD2D => _handlerD2D.DeviceContext;

        /// <summary>
        /// A unique value that identifies the adapter.
        /// </summary>
        public long Luid => _adapterInfo.Luid;

        public bool Supports2D =>
            _handlerD2D != null &&
            _handlerD2D.IsLoaded;

        /// <summary>
        /// Gets the main Direct3D 11 context object.
        /// </summary>
        internal D3D11.DeviceContext DeviceImmediateContextD3D11 => _handlerD3D11.ImmediateContext;

        /// <summary>
        /// Gets the current device configuration.
        /// </summary>
        public GraphicsDeviceConfiguration Configuration { get; }

        /// <summary>
        /// Gets the DXGI factory object.
        /// </summary>
        internal Factory2 FactoryDxgi => _handlerDXGI.Factory;

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
        public bool DebugEnabled => this.Configuration.CoreConfiguration.DebugEnabled;

        /// <summary>
        /// Internal members, use with care.
        /// </summary>
        public EngineDeviceInternals Internals { get; }

        /// <inheritdoc />
        public bool IsDisposed => _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineDevice"/> class.
        /// </summary>
        internal EngineDevice(
            ISeeingSharpExtensionProvider extensionProvider,
            EngineFactory engineFactory, GraphicsCoreConfiguration coreConfiguration,
            EngineAdapterInfo adapterInfo)
        {
            engineFactory.EnsureNotNull(nameof(engineFactory));
            coreConfiguration.EnsureNotNull(nameof(coreConfiguration));
            adapterInfo.EnsureNotNull(nameof(adapterInfo));

            this.Internals = new EngineDeviceInternals(this);

            _deviceResources = new DeviceResourceManager(this);

            _additionalDeviceHandlers = new List<IDisposable>();

            _adapterInfo = adapterInfo;

            _extensionProvider = extensionProvider;
            _engineFactory = engineFactory;
            this.IsSoftware = adapterInfo.IsSoftwareAdapter;

            // Set default configuration
            this.Configuration = new GraphicsDeviceConfiguration(coreConfiguration);
            this.Configuration.TextureQuality = !this.IsSoftware ? TextureQuality.High : TextureQuality.Low;
            this.Configuration.GeometryQuality = !this.IsSoftware ? GeometryQuality.High : GeometryQuality.Low;

            // Set default antialiasing configurations
            _sampleDescWithAntialiasing = new SampleDescription(1, 0);

            // Let loaders edit the device configuration
            if (_extensionProvider != null)
            {
                foreach (var actExtension in _extensionProvider.Extensions)
                {
                    actExtension.EditDeviceConfiguration(adapterInfo, this.Configuration);
                }
            }

            // Load all resources
            this.LoadResources();
        }

        /// <summary>
        /// Creates a software based device which is not managed by SeeingSharp.
        /// This method is meant to be used during unit testing.
        /// </summary>
        public static EngineDevice CreateSoftwareDevice(EngineFactory factory)
        {
            var hardwareInfo = new EngineHardwareInfo(factory);
            var actSoftwareAdapter = hardwareInfo.SoftwareAdapter;
            if (actSoftwareAdapter == null)
            {
                throw new SeeingSharpGraphicsException("Unable to find a software device!");
            }

            return new EngineDevice(
                null, factory, new GraphicsCoreConfiguration(), actSoftwareAdapter);
        }

        /// <summary>
        /// Tries to get a device handler which was created by an extension.
        /// </summary>
        public T TryGetAdditionalDeviceHandler<T>()
            where T : class
        {
            if(_isDisposed) { throw new ObjectDisposedException(nameof(EngineDevice)); }

            foreach (var actAdditionalDeviceHandler in _additionalDeviceHandlers)
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
            if(_isDisposed) { throw new ObjectDisposedException(nameof(EngineDevice)); }

            _isDetailLevelForced = true;
            _forcedDetailLevel = detailLevel;
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

        /// <inheritdoc />
        public void Dispose()
        {
            this.UnloadResources();
            _isDisposed = true;
        }

        /// <summary>
        /// Get the sample description for the given quality level.
        /// </summary>
        /// <param name="qualityLevel">The quality level for which a sample description is needed.</param>
        internal SampleDescription GetSampleDescription(AntialiasingQualityLevel qualityLevel)
        {
            if(_isDisposed) { throw new ObjectDisposedException(nameof(EngineDevice)); }

            return qualityLevel switch
            {
                AntialiasingQualityLevel.Low => _antialiasingConfigLow,
                AntialiasingQualityLevel.Medium => _antialiasingConfigMedium,
                AntialiasingQualityLevel.High => _antialiasingConfigHigh,
                _ => new SampleDescription(1, 0)
            };
        }

        internal void RegisterDeviceResource(IEngineDeviceResource resource)
        {
            if(_isDisposed) { throw new ObjectDisposedException(nameof(EngineDevice)); }

            _deviceResources.RegisterDeviceResource(resource);
        }

        internal void DeregisterDeviceResource(IEngineDeviceResource resource)
        {
            if(_isDisposed) { throw new ObjectDisposedException(nameof(EngineDevice)); }

            _deviceResources.DeregisterDeviceResource(resource);
        }

        internal void CleanupDeviceResourceCollection()
        {
            if(_isDisposed) { throw new ObjectDisposedException(nameof(EngineDevice)); }

            if (_deviceResources.CleanupNeeded)
            {
                _deviceResources.Cleanup();
            }
        }

        /// <summary>
        /// Recreates this device after a device lost event.
        /// </summary>
        internal void RecreateAfterDeviceLost()
        {
            if(_isDisposed) { throw new ObjectDisposedException(nameof(EngineDevice)); }

            this.IsLost.EnsureTrue(nameof(this.IsLost));

            // Unload all resources first
            _deviceResources.UnloadResources();
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
            if(_isDisposed) { throw new ObjectDisposedException(nameof(EngineDevice)); }

            if (antialiasingEnabled) { return _sampleDescWithAntialiasing; }
            return new SampleDescription(1, 0);
        }

        /// <summary>
        /// Loads all device resources.
        /// </summary>
        private bool LoadResources()
        {
            if(_isDisposed) { throw new ObjectDisposedException(nameof(EngineDevice)); }

            // Initialize all direct3D APIs
            this.LoadDeviceIndex++;
            try
            {
                _handlerDXGI = new DeviceHandlerDXGI(_engineFactory, _adapterInfo);
                _handlerD3D11 = new DeviceHandlerD3D11(this.Configuration, _handlerDXGI.Adapter);
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
            if (_handlerD3D11 != null)
            {
                _handlerD2D = new DeviceHandlerD2D(this.Configuration, _engineFactory, this);
                this.FakeRenderTarget2D = _handlerD2D.RenderTarget;
            }

            // Create additional device handlers
            if (_extensionProvider != null)
            {
                foreach (var actExtension in _extensionProvider.Extensions)
                {
                    var additionalDeviceHandlers = actExtension.CreateAdditionalDeviceHandlers(this);
                    if (additionalDeviceHandlers == null) { continue; }

                    foreach (var actAdditionalDeviceHandler in additionalDeviceHandlers)
                    {
                        if (actAdditionalDeviceHandler == null) { continue; }
                        _additionalDeviceHandlers.Add(actAdditionalDeviceHandler);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Unloads all device resources.
        /// </summary>
        private void UnloadResources()
        {
            foreach (var actAdditionalDeviceHandler in _additionalDeviceHandlers)
            {
                SeeingSharpUtil.DisposeObject(actAdditionalDeviceHandler);
            }
            _additionalDeviceHandlers.Clear();

            _handlerD2D?.UnloadResources();
            _handlerD2D = null;
            this.FakeRenderTarget2D = null;

            _handlerD3D11?.UnloadResources();
            _handlerD3D11 = null;

            _handlerDXGI?.UnloadResources();
            _handlerDXGI = null;
        }

        /// <summary>
        /// Checks for standard antialiasing support.
        /// </summary>
        private bool CheckIsStandardAntialiasingPossible()
        {
            if(_isDisposed) { throw new ObjectDisposedException(nameof(EngineDevice)); }

            // Very important to check possible antialiasing
            // More on the used technique
            //  see http://msdn.microsoft.com/en-us/library/windows/apps/dn458384.aspx
            var formatSupport = _handlerD3D11.Device1.CheckFormatSupport(GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT);

            if ((formatSupport & D3D11.FormatSupport.MultisampleRenderTarget) != D3D11.FormatSupport.MultisampleRenderTarget) { return false; }
            if ((formatSupport & D3D11.FormatSupport.MultisampleResolve) != D3D11.FormatSupport.MultisampleResolve) { return false; }
            if (_handlerD3D11.FeatureLevel == FeatureLevel.Level_9_1) { return false; }

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

                var testTexture = new D3D11.Texture2D(_handlerD3D11.Device1, textureDescription);
                SeeingSharpUtil.SafeDispose(ref testTexture);
            }
            catch (Exception)
            {
                return false;
            }

            // Check for quality levels
            var lowQualityLevels = _handlerD3D11.Device1.CheckMultisampleQualityLevels(GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT, 2);
            var mediumQualityLevels = _handlerD3D11.Device1.CheckMultisampleQualityLevels(GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT, 4);
            var highQualityLevels = _handlerD3D11.Device1.CheckMultisampleQualityLevels(GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT, 8);

            // Generate sample descriptions for each possible quality level
            if (lowQualityLevels > 0)
            {
                _antialiasingConfigLow = new SampleDescription(2, lowQualityLevels - 1);
            }
            if (mediumQualityLevels > 0)
            {
                _antialiasingConfigMedium = new SampleDescription(4, mediumQualityLevels - 1);
            }
            if (highQualityLevels > 0)
            {
                _antialiasingConfigHigh = new SampleDescription(8, highQualityLevels - 1);
            }

            return lowQualityLevels > 0;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        public class EngineDeviceInternals
        {
            private EngineDevice _host;

            public Adapter1 Adapter => _host._handlerDXGI?.Adapter;

            public D2D.RenderTarget FakeRenderTarget2D => _host.FakeRenderTarget2D;

            public Factory2 FactoryDxgi => _host.FactoryDxgi;

            public D3D11.DeviceContext DeviceImmediateContextD3D11 => _host.DeviceImmediateContextD3D11;

            public D2D.DeviceContext DeviceContextD2D => _host.DeviceContextD2D;

            public D2D.Device DeviceD2D => _host.DeviceD2D;

            public D3D11.Device1 DeviceD3D11_1 => _host.DeviceD3D11_1;

            internal EngineDeviceInternals(EngineDevice host)
            {
                _host = host;
            }

            public void RegisterDeviceResource(IEngineDeviceResource resource)
            {
                _host.RegisterDeviceResource(resource);
            }

            public void DeregisterDeviceResource(IEngineDeviceResource resource)
            {
                _host.DeregisterDeviceResource(resource);
            }

            public SampleDescription GetSampleDescription(AntialiasingQualityLevel qualityLevel)
            {
                return _host.GetSampleDescription(qualityLevel);
            }
        }
    }
}
