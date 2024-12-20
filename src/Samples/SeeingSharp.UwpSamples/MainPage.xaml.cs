﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using SeeingSharp.Core;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;
using SeeingSharp.UwpSamples.Util;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;

namespace SeeingSharp.UwpSamples
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private SampleBase _actSample;
        private SampleSettings _actSampleSettings;
        private SampleMetadata _actSampleInfo;
        private bool _isChangingSample;

        private List<ChildRenderPage> _childPages;

        public bool IsChangingSample
        {
            get => _isChangingSample;
            set
            {
                if (_isChangingSample != value)
                {
                    _isChangingSample = value;
                    this.RaisePropertyChanged(nameof(this.IsChangingSample));
                    this.RaisePropertyChanged(nameof(this.ProgressRingVisibility));
                }
            }
        }

        public Visibility ProgressRingVisibility => _isChangingSample ? Visibility.Visible : Visibility.Collapsed;

        public MainWindowViewModel ViewModel => this.DataContext as MainWindowViewModel;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            this.InitializeComponent();

            _childPages = new List<ChildRenderPage>();

            CommonUtil.UpdateApplicationTitleBar("Main window");

            this.Loaded += this.OnLoaded;
        }

        /// <summary>
        /// Applies the given sample.
        /// </summary>
        private async void ApplySample(SampleMetadata sampleInfo, SampleSettings sampleSettings)
        {
            if (DesignMode.DesignModeEnabled) { return; }
            if (this.IsChangingSample) { return; }

            this.IsChangingSample = true;
            try
            {
                if (_actSampleInfo == sampleInfo) { return; }

                // Discard presenting before updating current sample
                this.CtrlSwapChain.DiscardPresent = true;
                foreach (var actChildWindow in _childPages)
                {
                    actChildWindow.DiscardPresent = true;
                }

                // Clear previous sample
                if (_actSampleInfo != null)
                {
                    await CtrlSwapChain.RenderLoop.Scene.ManipulateSceneAsync(manipulator =>
                    {
                        manipulator.Clear(true);
                    });
                    await CtrlSwapChain.RenderLoop.Clear2DDrawingLayersAsync();
                    CtrlSwapChain.RenderLoop.ObjectFilters.Clear();

                    foreach (var actChildWindow in _childPages)
                    {
                        await actChildWindow.ClearAsync();
                    }

                    _actSample.OnSampleClosed();
                }
                if (_actSampleSettings != null)
                {
                    _actSampleSettings.RecreateRequest -= this.OnSampleSettings_RecreateRequest;
                }

                // Reset members
                _actSample = null;
                _actSampleSettings = null;
                _actSampleInfo = null;

                // Apply new sample
                if (sampleInfo != null)
                {
                    var sampleObject = sampleInfo.CreateSampleObject();
                    await sampleObject.OnStartupAsync(CtrlSwapChain.RenderLoop, sampleSettings);
                    await sampleObject.OnInitRenderingWindowAsync(CtrlSwapChain.RenderLoop);
                    await sampleObject.OnReloadAsync(CtrlSwapChain.RenderLoop, sampleSettings);
                    
                    foreach (var actChildWindow in _childPages)
                    {
                        await actChildWindow.SetRenderingDataAsync(sampleObject);
                    }

                    _actSample = sampleObject;
                    _actSampleSettings = sampleSettings;
                    _actSampleInfo = sampleInfo;

                    if (_actSampleSettings != null)
                    {
                        _actSampleSettings.RecreateRequest += this.OnSampleSettings_RecreateRequest;
                    }

                    await CtrlSwapChain.RenderLoop.Register2DDrawingLayerAsync(
                        new PerformanceMeasureDrawingLayer(130f, CtrlSwapChain.ViewInformation));
                }

                // Wait for next finished rendering
                await CtrlSwapChain.RenderLoop.WaitForNextFinishedRenderAsync();
                await CtrlSwapChain.RenderLoop.WaitForNextFinishedRenderAsync();
            }
            finally
            {
                // Continue presenting
                this.CtrlSwapChain.DiscardPresent = false;
                foreach (var actChildWindow in _childPages)
                {
                    actChildWindow.DiscardPresent = false;
                }

                // Do a simple fade in for the rendering control
                SwapChainFadeInStoryboard.Begin();
                foreach (var actChildWindow in _childPages)
                {
                    actChildWindow.TriggerRenderControlFadeInAnimation();
                }

                // Reset lock for 'ApplySample' method
                this.IsChangingSample = false;
            }
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void OnSampleSettings_RecreateRequest(object sender, EventArgs e)
        {
            var sample = _actSample;
            var sampleSettings = _actSampleSettings;
            if (sample == null)
            {
                return;
            }
            if (sampleSettings == null)
            {
                return;
            }

            if (this.IsChangingSample) { return; }
            this.IsChangingSample = true;
            try
            {
                // Discard presenting before updating current sample
                this.CtrlSwapChain.DiscardPresent = true;
                foreach (var actChildWindow in _childPages)
                {
                    actChildWindow.DiscardPresent = true;
                }

                // Reload sample
                await sample.OnReloadAsync(CtrlSwapChain.RenderLoop, sampleSettings);

                // Wait for next finished rendering
                await CtrlSwapChain.RenderLoop.WaitForNextFinishedRenderAsync();
                await CtrlSwapChain.RenderLoop.WaitForNextFinishedRenderAsync();
            }
            finally
            {
                // Continue presenting
                this.CtrlSwapChain.DiscardPresent = false;
                foreach (var actChildWindow in _childPages)
                {
                    actChildWindow.DiscardPresent = false;
                }

                // Reset lock for 'ApplySample' method
                this.IsChangingSample = false;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled) { return; }

            var sampleRepo = new SampleRepository();
            sampleRepo.LoadSampleData();

            var viewModel = (MainWindowViewModel)this.DataContext;
            viewModel.NewChildWindowRequest += this.OnViewModel_NewChildWindowRequest;
            viewModel.LoadSampleData(sampleRepo, CtrlSwapChain.RenderLoop);

            // Start update loop
            CtrlSwapChain.RenderLoop.Configuration.AlphaEnabledSwapChain = true;
            CtrlSwapChain.RenderLoop.PrepareRender += (innerSender, eArgs) =>
            {
                var actSample = _actSample;
                actSample?.Update();
            };
        }

        private void OnSelectedSampleChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled) { return; }
            if (!(this.DataContext is MainWindowViewModel viewModel)) { return; }

            var selectedSample = viewModel.SelectedSample;
            if (selectedSample == null) { return; }

            this.ApplySample(selectedSample.SampleMetadata, viewModel.SampleSettings);
        }

        private async void OnViewModel_NewChildWindowRequest(object sender, EventArgs eArgs)
        {
            // Get current scene and camera viewpoint
            var currentScene = CtrlSwapChain.Scene;
            var currentViewPoint = CtrlSwapChain.Camera.GetViewPoint();

            // Create the child window and the page
            // Be careful: the child view has it's own UI thread. See https://docs.microsoft.com/en-us/windows/uwp/design/layout/application-view
            var newView = CoreApplication.CreateNewView();
            var newViewId = 0;
            ChildRenderPage childPage = null;
            var hostDispatcher = this.Dispatcher;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                childPage = new ChildRenderPage();
                childPage.InitializeChildWindow(currentScene, currentViewPoint);

                Window.Current.Content = childPage;
                Window.Current.Activate();
                Window.Current.VisibilityChanged += async (o, args) =>
                {
                    // 'Visible == false' means that the user closed the window
                    if (args.Visible == false)
                    {
                        // Set content to null --> This ensures that SeeingSharp gets the unloaded event from the view internally
                        Window.Current.Content = null;

                        // Notify the host that the ChildRenderPage was removed
                        await hostDispatcher.RunAsync(
                            CoreDispatcherPriority.Normal,
                            () => _childPages.Remove(childPage));

                        // Wait some loop passes to ensure that all references to this additional view are cleared
                        await GraphicsCore.Current.MainLoop.WaitForNextPassedLoopAsync();
                        await GraphicsCore.Current.MainLoop.WaitForNextPassedLoopAsync();
                        await GraphicsCore.Current.MainLoop.WaitForNextPassedLoopAsync();
                        
                        // Finally close the window
                        Window.Current.Close();
                    }
                };

                newViewId = ApplicationView.GetForCurrentView().Id;
            });

            // Show the child window
            var viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
            if (viewShown)
            {
                // Register the child page
                _childPages.Add(childPage);

                // We can call this one in the current thread
                // Should be thread save
                await childPage.SetRenderingDataAsync(_actSample);
            }
        }

        private void OnSampleCommand_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!(sender is NavigationViewItem navViewItem)) { return; }
            if (!(navViewItem.Tag is SampleCommand sampleCommand)) { return; }

            sampleCommand.Execute(null);
        }
    }
}