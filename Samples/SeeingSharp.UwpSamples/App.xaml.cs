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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.SampleContainer;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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

            // Hide default title bar.
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            if (!(Window.Current.Content is Frame rootFrame))
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += this.OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: load previous state
                }

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
