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
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Input;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using D2D = SharpDX.Direct2D1;
using DWrite = SharpDX.DirectWrite;

namespace SeeingSharp.Multimedia.Core
{
    public class GraphicsCore
    {
        // Members for exception handling
        private static List<EventHandler<InternalCatchedExceptionEventArgs>> s_internalExListeners;
        private static object s_internalExListenersLock;

        // Members for Unit testing
        private static bool s_throwDeviceInitError;

        // Singleton instance
        private static GraphicsCore s_current;

        // Hardware 
        private List<EngineDevice> m_devices;
        private EngineDevice m_defaultDevice;

        // Global device handlers
        private Exception m_initException;
        private FactoryHandlerWIC m_factoryHandlerWIC;
        private FactoryHandlerD2D m_factoryHandlerD2D;
        private FactoryHandlerDWrite m_factoryHandlerDWrite;
        private Task m_mainLoopTask;
        private CancellationTokenSource m_mainLoopCancelTokenSource;

        // Misc dependencies
        private SeeingSharpLoader m_loader;

        public static event EventHandler<InternalCatchedExceptionEventArgs> InternalCatchedException
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

            try
            {
                m_loader = loader;
                m_devices = new List<EngineDevice>();

                // Start performance value measuring
                this.PerformanceAnalyzer =
                    new PerformanceAnalyzer(TimeSpan.FromSeconds(1.0));

                // Create CoreConfiguration object
                this.Configuration = new GraphicsCoreConfiguration
                {
                    DebugEnabled = loadSettings.DebugEnabled,
                    ThrowD2DInitDeviceError = loadSettings.ThrowD2DInitDeviceError
                };
                foreach (var actExtension in loader.Extensions)
                {
                    actExtension.EditCoreConfiguration(this.Configuration);
                }

                // Create container object for all input handlers
                this.InputHandlers = new InputHandlerFactory(loader);
                this.ImportersAndExporters = new ImporterExporterRepository(loader);

                // Try to load global api factories (mostly for 2D rendering / operations)
                EngineFactory engineFactory = null;
                try
                {
                    if (s_throwDeviceInitError)
                    {
                        throw new SeeingSharpException("Simulated device load exception");
                    }

                    engineFactory = new EngineFactory(this.Configuration);
                    m_factoryHandlerWIC = engineFactory.WindowsImagingComponent;
                    m_factoryHandlerD2D = engineFactory.Direct2D;
                    m_factoryHandlerDWrite = engineFactory.DirectWrite;
                }
                catch (Exception ex)
                {
                    m_initException = ex;

                    m_devices.Clear();
                    m_factoryHandlerWIC = null;
                    m_factoryHandlerD2D = null;
                    m_factoryHandlerDWrite = null;
                    return;
                }
                FactoryD2D = m_factoryHandlerD2D.Factory2;
                this.FactoryDWrite = m_factoryHandlerDWrite.Factory;
                FactoryWIC = m_factoryHandlerWIC.Factory;

                // Create the object containing all hardware information
                this.HardwareInfo = new EngineHardwareInfo();
                var actIndex = 0;

                foreach (var actAdapterInfo in this.HardwareInfo.Adapters)
                {
                    var actEngineDevice = new EngineDevice(
                        loader,
                        engineFactory, this.Configuration,
                        this.HardwareInfo, actAdapterInfo);

                    if (actEngineDevice.IsLoadedSuccessfully)
                    {
                        actEngineDevice.DeviceIndex = actIndex;
                        actIndex++;

                        m_devices.Add(actEngineDevice);
                    }
                }

                m_defaultDevice = m_devices.FirstOrDefault();

                // Start input gathering
                this.InputGatherer = new InputGathererThread();
                this.InputGatherer.Start();

                // Start main loop
                this.MainLoop = new EngineMainLoop(this);
                if (m_devices.Count > 0)
                {
                    m_mainLoopCancelTokenSource = new CancellationTokenSource();
                    m_mainLoopTask = this.MainLoop.Start(m_mainLoopCancelTokenSource.Token);
                }
            }
            catch (Exception ex2)
            {
                m_initException = ex2;

                this.HardwareInfo = null;
                m_devices.Clear();
                this.Configuration = null;
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

        internal void InitializeViewConfiguration(RenderLoop renderLoop, GraphicsViewConfiguration viewConfig)
        {
            foreach (var actExtension in m_loader.Extensions)
            {
                actExtension.EditViewConfiguration(renderLoop, viewConfig);
            }
        }

        /// <summary>
        /// Gets the device with the given luid.
        /// Null is returned if no device found.
        /// </summary>
        public EngineDevice TryGetDeviceByLuid(long luid)
        {
            return m_devices.FirstOrDefault(
                actDevice => actDevice.Luid == luid);
        }

        /// <summary>
        /// Gets a collection containing all available font family names.
        /// </summary>
        public IEnumerable<string> GetFontFamilyNames(string localeName = "en-us")
        {
            localeName.EnsureNotNullOrEmpty(nameof(localeName));
            localeName = localeName.ToLower();

            // Query for all available FontFamilies installed on the system
            List<string> result = null;

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
            return m_devices.Any(actDevice => !actDevice.IsSoftware);
        }

        /// <summary>
        /// Sets the default device to a software device.
        /// </summary>
        public void SetDefaultDeviceToSoftware()
        {
            m_defaultDevice = m_devices.FirstOrDefault(actDevice => actDevice.IsSoftware);
        }

        /// <summary>
        /// Sets the default device to a hardware device.
        /// </summary>
        public void SetDefaultDeviceToHardware()
        {
            m_defaultDevice = m_devices.FirstOrDefault(actDevice => actDevice.IsSoftware);
        }

        /// <summary>
        /// Gets the next generic resource key.
        /// </summary>
        public static NamedOrGenericKey GetNextGenericResourceKey()
        {
            return ResourceKeyGenerator.GetNextGeneric();
        }

        internal static void PublishInternalExceptionInfo(
            Exception ex,
            InternalExceptionLocation location)
        {
            List<EventHandler<InternalCatchedExceptionEventArgs>> handlers = null;
            lock (s_internalExListenersLock)
            {
                if (s_internalExListeners.Count > 0)
                {
                    handlers = new List<EventHandler<InternalCatchedExceptionEventArgs>>(
                        s_internalExListeners);
                }
            }
            if (handlers == null) { return; }

            foreach (var actEventHandler in handlers)
            {
                try
                {
                    actEventHandler(null, new InternalCatchedExceptionEventArgs(
                        ex, location));
                }
                catch
                {
                    // ignored
                }
            }
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
            return this.PerformanceAnalyzer.BeginMeasureActivityDuration(activityName);
        }

        /// <summary>
        /// Executes the given action and measures the time it takes.
        /// </summary>
        /// <param name="activityName">Name of the activity.</param>
        /// <param name="activityAction">The activity action.</param>
        internal void ExecuteAndMeasureActivityDuration(string activityName, Action activityAction)
        {
            this.PerformanceAnalyzer.ExecuteAndMeasureActivityDuration(activityName, activityAction);
        }

        /// <summary>
        /// Notifies the duration of the given activity.
        /// </summary>
        /// <param name="activityName"></param>
        /// <param name="durationTicks"></param>
        internal void NotifyActivityDuration(string activityName, long durationTicks)
        {
            this.PerformanceAnalyzer.NotifyActivityDuration(activityName, durationTicks);
        }

        /// <summary>
        /// Gets current singleton instance.
        /// </summary>
        public static GraphicsCore Current
        {
            get
            {

                // Do nothing if we are within the Wpf designer
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    return null;
                }

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
        public EngineHardwareInfo HardwareInfo { get; }

        /// <summary>
        /// Gets the first output monitor.
        /// </summary>
        public EngineOutputInfo DefaultOutput =>
            this.HardwareInfo.Adapters
                .FirstOrDefault()?
                .Outputs.FirstOrDefault();

        /// <summary>
        /// Is GraphicsCore loaded successfully?
        /// </summary>
        public static bool IsLoaded =>
            s_current != null &&
            s_current.m_devices.Count > 0 &&
            s_current.m_initException == null;

        public Exception InitException => s_current?.m_initException;

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
        public EngineDevice DefaultDevice
        {
            get => m_defaultDevice;
            set
            {
                if (value == null) { throw new ArgumentNullException(nameof(this.DefaultDevice)); }
                if (!m_devices.Contains(value)) { throw new ArgumentException("Device is not available on this GraphicsCore!"); }

                m_defaultDevice = value;
            }
        }

        /// <summary>
        /// Gets a collection containing all loaded devices.
        /// </summary>
        public IEnumerable<EngineDevice> Devices => m_devices;

        /// <summary>
        /// Gets the total count of loaded devices.
        /// </summary>
        public int DeviceCount => m_devices.Count;

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
        public EngineDevice DefaultSoftwareDevice
        {
            get
            {
                return m_devices?.FirstOrDefault(actDevice => actDevice.IsSoftware);
            }
        }

        /// <summary>
        /// Gets a collection containing all devices.
        /// </summary>
        public IEnumerable<EngineDevice> LoadedDevices => m_devices;

        /// <summary>
        /// Gets a list containing all input handlers.
        /// </summary>
        public InputHandlerFactory InputHandlers { get; }

        /// <summary>
        /// Gets an object which manages all importers and exporters.
        /// </summary>
        public ImporterExporterRepository ImportersAndExporters { get; }

        /// <summary>
        /// Gets the current main loop object of the graphics engine.
        /// </summary>
        public EngineMainLoop MainLoop { get; }

        public InputGathererThread InputGatherer { get; }

        /// <summary>
        /// Gets the current performance calculator.
        /// </summary>
        public PerformanceAnalyzer PerformanceAnalyzer { get; }

        /// <summary>
        /// Gets the DirectWrite factory object.
        /// </summary>
        internal DWrite.Factory FactoryDWrite { get; }

        /// <summary>
        /// Gets the Direct2D factory object.
        /// </summary>
        internal D2D.Factory2 FactoryD2D;

        /// <summary>
        /// Gets the WIC factory object.
        /// </summary>
        internal ImagingFactory FactoryWIC;

        public GraphicsCoreInternals Internals { get; }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        public class GraphicsCoreInternals
        {
            private GraphicsCore m_parent;

            internal GraphicsCoreInternals(GraphicsCore parent)
            {
                m_parent = parent;
            }

            public D2D.Factory2 FactoryD2D => m_parent.FactoryD2D;

            public DWrite.Factory FactoryDWrite => m_parent.FactoryDWrite;

            public ImagingFactory FactoryWIC => m_parent.FactoryWIC;
        }
    }
}