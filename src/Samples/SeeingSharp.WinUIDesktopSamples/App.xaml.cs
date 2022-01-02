using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using SeeingSharp.Core;

namespace SeeingSharp.WinUIDesktopSamples
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows10.0.19041.0")]
    public partial class App : Application
    {
        private Window _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Initialize graphics
            GraphicsCore.Loader
                .SupportWinUI()
                .Load();

            _window = new MainWindow();
            _window.Activate();
        }
    }
}
