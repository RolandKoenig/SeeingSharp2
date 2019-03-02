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

//Some namespace mappings
using D2D = SharpDX.Direct2D1;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using DWrite = SharpDX.DirectWrite;

#endregion

namespace SeeingSharp.Multimedia.Core
{
    #region using

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Checking;
    using Input;
    using Objects;
    using SeeingSharp.Util;

    #endregion

    public class GraphicsCore
    {
        #region Members for Unittesting
        private static bool s_throwDeviceInitError;
        #endregion

        #region Members for exception handling
        private static List<EventHandler<InternalCatchedExceptionEventArgs>> s_internalExListeners;
        private static object s_internalExListenersLock;
        #endregion

        #region Singleton instance
        private static GraphicsCore s_current;
        #endregion

        #region Hardware 
        private EngineFactory m_engineFactory;
        private List<EngineDevice> m_devices;
        private EngineDevice m_defaultDevice;
        #endregion

        #region Some helpers

        #endregion

        #region Members for input

        #endregion

        #region Global device handlers
        private Exception m_initException;
        private FactoryHandlerWIC m_factoryHandlerWIC;
        private FactoryHandlerD2D m_factoryHandlerD2D;
        private FactoryHandlerDWrite m_factoryHandlerDWrite;
        #endregion

        #region Members for threading
        private static Task m_defaultInitTask;
        private Task m_mainLoopTask;
        private CancellationTokenSource m_mainLoopCancelTokenSource;
        #endregion

        #region Configurations

        #endregion

        public static event EventHandler<InternalCatchedExceptionEventArgs> InternalCachedException
        {
            add
            {
                if(value == null) { return; }
                lock(s_internalExListenersLock)
                {
                    s_internalExListeners.Add(value);
                }
            }
            remove
            {
                if(value == null) { return; }
                lock(s_internalExListenersLock)
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

        private GraphicsCore(DeviceLoadSettings loadSettings, SeeingSharpLoader loader)
        {
            try
            {
                // Upate RK.Common members
                m_devices = new List<EngineDevice>();

                PerformanceCalculator = new PerformanceAnalyzer(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(2.0))
                {
                    SyncContext = SynchronizationContext.Current
                };

                // <-- TODO
                PerformanceCalculator.RunAsync(CancellationToken.None)
                    .FireAndForget();

                Configuration = new GraphicsCoreConfiguration
                {
                    DebugEnabled = loadSettings.DebugEnabled
                };

                // Create container object for all input handlers
                InputHandlers = new InputHandlerFactory(loader);
                ImportersAndExporters = new ImporterExporterRepository(loader);

                // Try to load global api factories (mostly for 2D rendering / operations)
                try
                {
                    if(s_throwDeviceInitError)
                    {
                        throw new SeeingSharpException("Simulated device load exception");
                    }

                    m_engineFactory = new EngineFactory(loadSettings);
                    m_factoryHandlerWIC = m_engineFactory.WindowsImagingComponent;
                    m_factoryHandlerD2D = m_engineFactory.Direct2D;
                    m_factoryHandlerDWrite = m_engineFactory.DirectWrite;
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
                this.FactoryD2D = m_factoryHandlerD2D.Factory2;
                this.FactoryD2D_2 = m_factoryHandlerD2D.Factory2;
                this.FactoryDWrite = m_factoryHandlerDWrite.Factory;
                this.FactoryWIC = m_factoryHandlerWIC.Factory;

                // Create the object containing all hardware information
                HardwareInfo = new EngineHardwareInfo();
                int actIndex = 0;

                foreach(var actAdapterInfo in HardwareInfo.Adapters)
                {
                    var actEngineDevice = new EngineDevice(
                        loadSettings,
                        loader,
                        m_engineFactory,
                        Configuration,
                        actAdapterInfo.Adapter,
                        actAdapterInfo.IsSoftwareAdapter);

                    if(actEngineDevice.IsLoadedSuccessfully)
                    {
                        actEngineDevice.DeviceIndex = actIndex;
                        actIndex++;

                        m_devices.Add(actEngineDevice);
                    }
                }

                m_defaultDevice = m_devices.FirstOrDefault();

                // Start input gathering
                InputGatherer = new InputGathererThread();
                InputGatherer.Start();

                // Start main loop
                MainLoop = new EngineMainLoop(this);
                if (m_devices.Count > 0)
                {
                    m_mainLoopCancelTokenSource = new CancellationTokenSource();
                    m_mainLoopTask = MainLoop.Start(m_mainLoopCancelTokenSource.Token);
                }
            }
            catch (Exception ex2)
            {
                m_initException = ex2;

                HardwareInfo = null;
                m_devices.Clear();
                Configuration = null;
            }
        }

        internal static void PublishInternalExceptionInfo(
            Exception ex,
            InternalExceptionLocation location)
        {
            List<EventHandler<InternalCatchedExceptionEventArgs>> handlers = null;
            lock(s_internalExListenersLock)
            {
                if(s_internalExListeners.Count > 0)
                {
                    handlers = new List<EventHandler<InternalCatchedExceptionEventArgs>>(
                        s_internalExListeners);
                }
            }

            foreach(var actEventHandler in handlers)
            {
                try
                {
                    actEventHandler(null, new InternalCatchedExceptionEventArgs(
                        ex, location));
                }
                catch { }
            }
        }

        /// <summary>
        /// This method is implemented for automated tests only!
        /// Is sets <see cref="GraphicsCore.Current"/> to null to enable a separate instance inside a using block.
        /// </summary>
        public static IDisposable AutomatedTest_NewTestEnviornment()
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
        public EngineDevice TryGetDeviceByLuid(long luid)
        {
            return m_devices.FirstOrDefault(
                (actDevice) => actDevice.Luid == luid);
        }

        /// <summary>
        /// Gets a collection containing all available font familily names.
        /// </summary>
        public IEnumerable<string> GetFontFamilyNames(string localeName = "en-us")
        {
            localeName.EnsureNotNullOrEmpty(nameof(localeName));
            localeName = localeName.ToLower();

            // Query for all available FontFamilies installed on the system
            List<string> result = null;

            using (var fontCollection = this.FactoryDWrite.GetSystemFontCollection(false))
            {
                int fontFamilyCount = fontCollection.FontFamilyCount;
                result = new List<string>(fontFamilyCount);

                for(var loop =0; loop< fontFamilyCount; loop++)
                {
                    using (var actFamily = fontCollection.GetFontFamily(loop))
                    using (var actLocalizedStrings = actFamily.FamilyNames)
                    {
                        int localeIndex = -1;

                        if ((bool)actLocalizedStrings.FindLocaleName(localeName, out localeIndex))
                        {
                            string actName = actLocalizedStrings.GetString(0);

                            if(!string.IsNullOrWhiteSpace(actName))
                            {
                                result.Add(actName);
                            }
                        }
                        else if((bool)actLocalizedStrings.FindLocaleName("en-us", out localeIndex))
                        {
                            string actName = actLocalizedStrings.GetString(0);

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
        /// Resumes rendering when in supendet state.
        /// </summary>
        public void Resume()
        {
            if (MainLoop == null) { throw new SeeingSharpGraphicsException("GraphicsCore not loaded!"); }

            MainLoop.Resume();
        }

        /// <summary>
        /// Suspends rendering completely.
        /// </summary>
        public Task SuspendAsync()
        {
            if (MainLoop == null) { throw new SeeingSharpGraphicsException("GraphicsCore not loaded!"); }

            return MainLoop.SuspendAsync();
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
        /// Checks if there is any hardware device loaded.
        /// </summary>
        public bool IsAnyHardwareDeviceLoaded()
        {
            return m_devices.Any((actDevice) => !actDevice.IsSoftware);
        }

        /// <summary>
        /// Sets the default device to a software device.
        /// </summary>
        public void SetDefaultDeviceToSoftware()
        {
            m_defaultDevice = m_devices.FirstOrDefault((actDevice) => actDevice.IsSoftware);
        }

        /// <summary>
        /// Sets the default device to a hardware device.
        /// </summary>
        public void SetDefaultDeviceToHardware()
        {
            m_defaultDevice = m_devices.FirstOrDefault((actDevice) => actDevice.IsSoftware);
        }

        /// <summary>
        /// Starts measuring the duration of the given activity.
        /// </summary>
        /// <param name="activityName">The name of the activity.</param>
        internal IDisposable BeginMeasureActivityDuration(string activityName)
        {
            return this.PerformanceCalculator.BeginMeasureActivityDuration(activityName);
        }

        /// <summary>
        /// Executes the given action and measures the time it takes.
        /// </summary>
        /// <param name="activityName">Name of the activity.</param>
        /// <param name="activityAction">The activity action.</param>
        internal void ExecuteAndMeasureActivityDuration(string activityName, Action activityAction)
        {
            this.PerformanceCalculator.ExecuteAndMeasureActivityDuration(activityName, activityAction);
        }

        /// <summary>
        /// Notifies the duration of the given activity.
        /// </summary>
        /// <param name="activityName"></param>
        /// <param name="durationTicks"></param>
        internal void NotifyActivityDuration(string activityName, long durationTicks)
        {
            this.PerformanceCalculator.NotifyActivityDuration(activityName, durationTicks);
        }

        /// <summary>
        /// Gets the next generic resource key.
        /// </summary>
        public static NamedOrGenericKey GetNextGenericResourceKey()
        {
            return ResourceKeyGenerator.GetNextGeneric();
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
                    throw new SeeingSharpException($"Unable to access {nameof(GraphicsCore)}.{nameof(GraphicsCore.Current)} before Load was called or a rendering control created!");
                }

                return s_current;
            }
        }

        public static SeeingSharpLoader Loader
        {
            get
            {
                if(s_current != null) { throw new SeeingSharpException($"Unable to access {nameof(GraphicsCore)}.{nameof(GraphicsCore.Loader)} after successfully loading!"); }
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
        public EngineOutputInfo DefaultOutput
        {
            get
            {
                return HardwareInfo.Adapters
                    .FirstOrDefault()?
                    .Outputs.FirstOrDefault();
            }
        }

        /// <summary>
        /// Is GraphicsCore loaded successfully?
        /// </summary>
        public static bool IsLoaded
        {
            get
            {
                return
                    (s_current != null) &&
                    (s_current.m_devices.Count > 0) &&
                    (s_current.m_initException == null);
            }
        }

        public Exception InitException
        {
            get
            {
                return s_current?.m_initException;
            }
        }

        /// <summary>
        /// Is debug enabled?
        /// </summary>
        public bool IsDebugEnabled { get; }

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
            get{ return m_defaultDevice; }
            set
            {
                if (value == null) { throw new ArgumentNullException("DefaultDevice"); }
                if (!m_devices.Contains(value)) { throw new ArgumentException("Device is not available on this GraphicsCore!"); }

                m_defaultDevice = value;
            }
        }

        /// <summary>
        /// Gets a collection containing all loaded devices.
        /// </summary>
        public IEnumerable<EngineDevice> Devices
        {
            get { return m_devices; }
        }

        /// <summary>
        /// Gets the total count of loaded devices.
        /// </summary>
        public int DeviceCount
        {
            get { return m_devices.Count; }
        }

        /// <summary>
        /// Gets the total count of registered RenderLoop objects.
        /// </summary>
        public int RegisteredRenderLoopCount
        {
            get
            {
                if (MainLoop == null) { return 0; }
                return MainLoop.RegisteredRenderLoopCount;
            }
        }

        /// <summary>
        /// Gets the default software device.
        /// </summary>
        public EngineDevice DefaultSoftwareDevice
        {
            get
            {
                if (m_devices == null) { return null; }
                return m_devices.FirstOrDefault((actDevice) => actDevice.IsSoftware);
            }
        }

        /// <summary>
        /// Gets a collection containing all devices.
        /// </summary>
        public IEnumerable<EngineDevice> LoadedDevices
        {
            get { return m_devices; }
        }

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
        public PerformanceAnalyzer PerformanceCalculator { get; }

        /// <summary>
        /// Gets the WIC factory object.
        /// </summary>
        internal SharpDX.WIC.ImagingFactory FactoryWIC;

        /// <summary>
        /// Gets the Direct2D factory object.
        /// </summary>
        internal D2D.Factory FactoryD2D;

        /// <summary>
        /// Gets the Direct2D factory object.
        /// </summary>
        internal D2D.Factory2 FactoryD2D_2;

        /// <summary>
        /// Gets the DirectWrite factory object.
        /// </summary>
        internal DWrite.Factory FactoryDWrite;

        internal bool Force2DFallbackMethod { get; }
    }
}