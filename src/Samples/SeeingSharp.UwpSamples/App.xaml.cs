using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.SampleContainer;

namespace SeeingSharp.UwpSamples
{
    sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();

            PlatformDependentMethods.SetOpenUrlInBrowser(async url =>
            {
                var targetUrl = new Uri(url);
                await Launcher.LaunchUriAsync(targetUrl);
            });

            this.Suspending += this.OnSuspending;
            this.Resuming += this.OnResuming;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Initialize graphics
            GraphicsCore.Loader
                .SupportUwp()
                .Load();

            ApplicationView.TerminateAppOnFinalViewClose = true;

            if (!(Window.Current.Content is Frame rootFrame))
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += this.OnNavigationFailed;

                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                Window.Current.Activate();
            }
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            if (GraphicsCore.IsLoaded)
            {
                await GraphicsCore.Current.SuspendAsync();
            }

            deferral.Complete();
        }

        private void OnResuming(object sender, object e)
        {
            GraphicsCore.Current.Resume();
        }
    }
}
