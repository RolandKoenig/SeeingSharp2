﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Core.Devices;
using SeeingSharp.Core.HardwareInfo;
using SeeingSharp.Drawing3D.ImportExport;
using SeeingSharp.Input;
using SeeingSharp.Util;
using Vortice.WIC;
using D2D = Vortice.Direct2D1;
using DWrite = Vortice.DirectWrite;

namespace SeeingSharp.Core
{
    public class GraphicsCore
    {
        // Members for exception handling
        private static List<EventHandler<InternalCatchedExceptionEventArgs>> s_internalExListeners;
        private static object s_internalExListenersLock;

        // Members for Unit testing
        private static bool s_throwDeviceInitError;

        // Singleton instance
        private static GraphicsCore? s_current;

        // Hardware 
        private List<EngineDevice> _devices;
        private EngineDevice? _defaultDevice;

        // Global device handlers
        private Exception? _initException;

        // Misc dependencies
        private SeeingSharpLoader _loader;

        /// <summary>
        /// Gets current singleton instance.
        /// </summary>
        public static GraphicsCore Current
        {
            get
            {
                if (s_current == null)
                {
                    throw new SeeingSharpException($"Unable to access {nameof(GraphicsCore)}.{nameof(Current)} before Load was called or a rendering control created!");
                }

                return s_current;
            }
        }

        public static SeeingSharpLoader Loader
        {
            get
            {
                if (s_current != null) { throw new SeeingSharpException($"Unable to access {nameof(GraphicsCore)}.{nameof(Loader)} after successfully loading!"); }
                return new SeeingSharpLoader();
            }
        }

        /// <summary>
        /// Gets the dxgi factory object.
        /// </summary>
        public EngineHardwareInfo? HardwareInfo { get; }

        /// <summary>
        /// Gets the first output monitor.
        /// </summary>
        public EngineOutputInfo? DefaultOutput =>
            this.HardwareInfo?.Adapters
                .FirstOrDefault()?
                .Outputs.FirstOrDefault();

        /// <summary>
        /// Is GraphicsCore loaded successfully?
        /// </summary>
        public static bool IsLoaded => s_current != null;

        /// <summary>
        /// Is GraphicsCore loaded successfully with all graphics support?
        /// </summary>
        public static bool IsGraphicsLoaded =>
            s_current != null &&
            s_current.MainLoop != null;

        public Exception? InitException => s_current?._initException;

        /// <summary>
        /// Is debug enabled?
        /// </summary>
        public bool IsDebugEnabled => this.Configuration.DebugEnabled;

        /// <summary>
        /// Gets the current resource key generator.
        /// </summary>
        public static UniqueGenericKeyGenerator ResourceKeyGenerator { get; }

        /// <summary>
        /// Gets current graphics configuration.
        /// </summary>
        public GraphicsCoreConfiguration Configuration { get; }

        /// <summary>
        /// Gets the default device.
        /// </summary>
        public EngineDevice? DefaultDevice
        {
            get => _defaultDevice;
            set
            {
                if (value == null) { throw new ArgumentNullException(nameof(value)); }
                if (!_devices.Contains(value)) { throw new ArgumentException("Device is not available on this GraphicsCore!"); }

                _defaultDevice = value;
            }
        }

        /// <summary>
        /// Gets a collection containing all loaded devices.
        /// </summary>
        public IReadOnlyList<EngineDevice> Devices => _devices;

        /// <summary>
        /// Gets the total count of loaded devices.
        /// </summary>
        public int DeviceCount => _devices.Count;

        /// <summary>
        /// Gets the total count of registered RenderLoop objects.
        /// </summary>
        public int RegisteredRenderLoopCount
        {
            get
            {
                if (this.MainLoop == null) { return 0; }
                return this.MainLoop.RegisteredRenderLoopCount;
            }
        }

        /// <summary>
        /// Gets the default software device.
        /// </summary>
        public EngineDevice? DefaultSoftwareDevice
        {
            get
            {
                return _devices.FirstOrDefault(actDevice => actDevice.IsSoftware);
            }
        }

        /// <summary>
        /// Gets a collection containing all devices.
        /// </summary>
        public IEnumerable<EngineDevice> LoadedDevices => _devices;

        /// <summary>
        /// Gets a list containing all input handlers.
        /// </summary>
        public InputHandlerFactory? InputHandlers { get; }

        /// <summary>
        /// Gets an object which manages all importers and exporters.
        /// </summary>
        public ImporterExporterRepository? ImportersAndExporters { get; }

        /// <summary>
        /// Gets the current main loop object of the graphics engine.
        /// </summary>
        public EngineMainLoop? MainLoop { get; }

        /// <summary>
        /// Gets the task that represents the main loop (the task should never stop)
        /// </summary>
        public Task? MainLoopTask { get; }

        /// <summary>
        /// Gets global factories.
        /// </summary>
        public EngineFactory? Factory { get; }

        public InputGathererThread? InputGatherer { get; }

        /// <summary>
        /// Gets the current performance calculator.
        /// </summary>
        public PerformanceAnalyzer? PerformanceAnalyzer { get; }

        /// <summary>
        /// Gets the DirectWrite factory object.
        /// </summary>
        internal DWrite.IDWriteFactory? FactoryDWrite { get; }

        /// <summary>
        /// Gets the Direct2D factory object.
        /// </summary>
        internal D2D.ID2D1Factory2? FactoryD2D;

        /// <summary>
        /// Gets the WIC factory object.
        /// </summary>
        internal IWICImagingFactory? FactoryWIC;

        public GraphicsCoreInternals Internals { get; }

        public static event EventHandler<InternalCatchedExceptionEventArgs>? InternalCatchedException
        {
            add
            {
                if (value == null) { return; }
                lock (s_internalExListenersLock)
                {
                    s_internalExListeners.Add(value);
                }
            }
            remove
            {
                if (value == null) { return; }
                lock (s_internalExListenersLock)
                {
                    s_internalExListeners.Remove(value);
                }
            }
        }

        static GraphicsCore()
        {
            s_internalExListeners = new List<EventHandler<InternalCatchedExceptionEventArgs>>();
            s_internalExListenersLock = new object();

            // Create the key generator for resource keys
            ResourceKeyGenerator = new UniqueGenericKeyGenerator();
        }

        private GraphicsCore(SeeingSharpLoadSettings loadSettings, SeeingSharpLoader loader)
        {
            this.Internals = new GraphicsCoreInternals(this);

            _loader = loader;
            _devices = new List<EngineDevice>();

            // Create CoreConfiguration object
            this.Configuration = new GraphicsCoreConfiguration
            {
                DebugEnabled = loadSettings.DebugEnabled,
                ThrowD2DInitDeviceError = loadSettings.ThrowD2DInitDeviceError
            };

            try
            {
                // Modify configuration by loaders
                foreach (var actExtension in loader.Extensions)
                {
                    actExtension.EditCoreConfiguration(this.Configuration);
                }

                // Try to load global api factories (mostly for 2D rendering / operations)
                try
                {
                    if (s_throwDeviceInitError)
                    {
                        throw new SeeingSharpException("Simulated device load exception");
                    }

                    this.Factory = new EngineFactory(this.Configuration);
                }
                catch (Exception ex)
                {
                    _initException = ex;

                    _devices.Clear();
                    return;
                }
                this.FactoryD2D = this.Factory.Direct2D.Factory;
                this.FactoryDWrite = this.Factory.DirectWrite.Factory;
                this.FactoryWIC = this.Factory.WIC.Factory;

                // Create the object containing all hardware information
                this.HardwareInfo = new EngineHardwareInfo(this.Factory);
                var actIndex = 0;
                foreach (var actAdapterInfo in this.HardwareInfo.Adapters)
                {
                    var actEngineDevice = new EngineDevice(loader, this.Factory, this.Configuration, actAdapterInfo);
                    if (actEngineDevice.IsLoadedSuccessfully)
                    {
                        actEngineDevice.DeviceIndex = actIndex;
                        actIndex++;

                        _devices.Add(actEngineDevice);
                    }
                }
                _defaultDevice = _devices.FirstOrDefault();

                // Start performance value measuring
                this.PerformanceAnalyzer =
                    new PerformanceAnalyzer(TimeSpan.FromSeconds(1.0));

                // Start input gathering
                this.InputGatherer = new InputGathererThread();
                this.InputGatherer.Start();

                // Create container object for all input handlers
                this.InputHandlers = new InputHandlerFactory(loader);
                this.ImportersAndExporters = new ImporterExporterRepository(loader);

                // Start main loop
                this.MainLoop = new EngineMainLoop(this);
                if (_devices.Count > 0)
                {
                    this.MainLoopTask = this.MainLoop.Start(CancellationToken.None);
                }
            }
            catch (Exception ex2)
            {
                _initException = ex2;

                this.HardwareInfo = null;
                _devices.Clear();
            }
        }

        public static void EnsureGraphicsSupportLoaded()
        {
            if (!IsGraphicsLoaded)
            {
                throw new SeeingSharpGraphicsException($"Graphics support on {nameof(GraphicsCore)} not loaded!");
            }
        }

        /// <summary>
        /// This method is implemented for automated tests only!
        /// Is sets <see cref="GraphicsCore.Current"/> to null to enable a separate instance inside a using block.
        /// </summary>
        public static IDisposable AutomatedTest_NewTestEnvironment()
        {
            var lastCurrent = s_current;

            if (lastCurrent?.MainLoop?.RegisteredRenderLoopCount > 0)
            {
                throw new SeeingSharpException("Current environment still active!");
            }

            s_current = null;

            return new DummyDisposable(() => s_current = lastCurrent);
        }

        /// <summary>
        /// This method is implemented for automated tests only!
        /// It simulates a device initialization exception all next times GraphicsCore.Initialize is called.
        /// </summary>
        public static IDisposable AutomatedTest_ForceDeviceInitError()
        {
            if (s_current != null)
            {
                throw new SeeingSharpException("This call is only valid before Initialize was called!");
            }

            s_throwDeviceInitError = true;

            return new DummyDisposable(() => s_throwDeviceInitError = false);
        }

        /// <summary>
        /// Gets the device with the given luid.
        /// Null is returned if no device found.
        /// </summary>
        public EngineDevice? TryGetDeviceByLuid(long luid)
        {
            return _devices.FirstOrDefault(
                actDevice => actDevice.Luid == luid);
        }

        /// <summary>
        /// Gets a collection containing all available font family names.
        /// </summary>
        public IEnumerable<string> GetFontFamilyNames(string localeName = "en-us")
        {
            localeName.EnsureNotNullOrEmpty(nameof(localeName));
            localeName = localeName.ToLower();

            if (this.FactoryDWrite == null) { return Array.Empty<string>(); }

            // Query for all available FontFamilies installed on the system
            List<string> result;
            using (var fontCollection = this.FactoryDWrite.GetSystemFontCollection(false))
            {
                var fontFamilyCount = fontCollection.FontFamilyCount;
                result = new List<string>(fontFamilyCount);

                for (var loop = 0; loop < fontFamilyCount; loop++)
                {
                    using (var actFamily = fontCollection.GetFontFamily(loop))
                    using (var actLocalizedStrings = actFamily.FamilyNames)
                    {
                        if (actLocalizedStrings.FindLocaleName(localeName, out _))
                        {
                            var actName = actLocalizedStrings.GetString(0);

                            if (!string.IsNullOrWhiteSpace(actName))
                            {
                                result.Add(actName);
                            }
                        }
                        else if (actLocalizedStrings.FindLocaleName("en-us", out _))
                        {
                            var actName = actLocalizedStrings.GetString(0);

                            if (!string.IsNullOrWhiteSpace(actName))
                            {
                                result.Add(actName);
                            }
                        }
                    }
                }
            }

            // Sort the list finally
            result.Sort();

            return result;
        }

        /// <summary>
        /// Resumes rendering when in suspend state.
        /// </summary>
        public void Resume()
        {
            if (this.MainLoop == null) { throw new SeeingSharpGraphicsException("GraphicsCore not loaded!"); }

            this.MainLoop.Resume();
        }

        /// <summary>
        /// Suspends rendering completely.
        /// </summary>
        public Task SuspendAsync()
        {
            if (this.MainLoop == null) { throw new SeeingSharpGraphicsException("GraphicsCore not loaded!"); }

            return this.MainLoop.SuspendAsync();
        }

        /// <summary>
        /// Checks if there is any hardware device loaded.
        /// </summary>
        public bool IsAnyHardwareDeviceLoaded()
        {
            return _devices.Any(actDevice => !actDevice.IsSoftware);
        }

        /// <summary>
        /// Sets the default device to a software device.
        /// </summary>
        public void SetDefaultDeviceToSoftware()
        {
            _defaultDevice = _devices.FirstOrDefault(actDevice => actDevice.IsSoftware);
        }

        /// <summary>
        /// Sets the default device to a hardware device.
        /// </summary>
        public void SetDefaultDeviceToHardware()
        {
            _defaultDevice = _devices.FirstOrDefault(actDevice => actDevice.IsSoftware);
        }

        /// <summary>
        /// Gets the next generic resource key.
        /// </summary>
        public static NamedOrGenericKey GetNextGenericResourceKey()
        {
            return ResourceKeyGenerator.GetNextGeneric();
        }

        internal void InitializeViewConfiguration(RenderLoop renderLoop, GraphicsViewConfiguration viewConfig)
        {
            foreach (var actExtension in _loader.Extensions)
            {
                actExtension.EditViewConfiguration(renderLoop, viewConfig);
            }
        }

        internal static void PublishInternalExceptionInfo(
            Exception ex,
            InternalExceptionLocation location)
        {
            // Get current list of event listeners
            List<EventHandler<InternalCatchedExceptionEventArgs>>? handlers = null;
            lock (s_internalExListenersLock)
            {
                if (s_internalExListeners.Count > 0)
                {
                    handlers = new List<EventHandler<InternalCatchedExceptionEventArgs>>(
                        s_internalExListeners);
                }
            }
            if (handlers == null) { return; }

            // Trigger event in separate ThreadPool task to ensure that SeeingSharp logic runs further without interruption
            ThreadPool.QueueUserWorkItem(
                state =>
                {
                    var exInfo = new InternalCatchedExceptionEventArgs(ex, location);
                    foreach (var actEventHandler in handlers)
                    {
                        try
                        {
                            actEventHandler(null, exInfo);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                },
                null);
        }

        /// <summary>
        /// Loads the GraphicsCore object.
        /// </summary>
        internal static void Load(SeeingSharpLoader loader)
        {
            if (s_current != null) { throw new SeeingSharpException("Graphics is already loaded!"); }

            var newCore = new GraphicsCore(loader.LoadSettings, loader);
            if (s_current != null) { throw new SeeingSharpException("Parallel loading of GraphicsCore detected!"); }
            s_current = newCore;
        }

        /// <summary>
        /// Starts measuring the duration of the given activity.
        /// </summary>
        /// <param name="activityName">The name of the activity.</param>
        internal IDisposable BeginMeasureActivityDuration(string activityName)
        {
            if (this.PerformanceAnalyzer == null)
            {
                throw new SeeingSharpException($"Performance measuring on {nameof(GraphicsCore)} is only available when all graphics support loaded successfully!");
            }

            return this.PerformanceAnalyzer.BeginMeasureActivityDuration(activityName);
        }

        /// <summary>
        /// Executes the given action and measures the time it takes.
        /// </summary>
        /// <param name="activityName">Name of the activity.</param>
        /// <param name="activityAction">The activity action.</param>
        internal void ExecuteAndMeasureActivityDuration(string activityName, Action activityAction)
        {
            if (this.PerformanceAnalyzer == null)
            {
                throw new SeeingSharpException($"Performance measuring on {nameof(GraphicsCore)} is only available when all graphics support loaded successfully!");
            }

            this.PerformanceAnalyzer.ExecuteAndMeasureActivityDuration(activityName, activityAction);
        }

        /// <summary>
        /// Notifies the duration of the given activity.
        /// </summary>
        /// <param name="activityName"></param>
        /// <param name="durationTicks"></param>
        internal void NotifyActivityDuration(string activityName, long durationTicks)
        {
            if (this.PerformanceAnalyzer == null)
            {
                throw new SeeingSharpException($"Performance measuring on {nameof(GraphicsCore)} is only available when all graphics support loaded successfully!");
            }

            this.PerformanceAnalyzer.NotifyActivityDuration(activityName, durationTicks);
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        public class GraphicsCoreInternals
        {
            private GraphicsCore _parent;

            public D2D.ID2D1Factory2? FactoryD2D => _parent.FactoryD2D;

            public DWrite.IDWriteFactory? FactoryDWrite => _parent.FactoryDWrite;

            public IWICImagingFactory? FactoryWIC => _parent.FactoryWIC;

            internal GraphicsCoreInternals(GraphicsCore parent)
            {
                _parent = parent;
            }
        }
    }
}