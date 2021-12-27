using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PInvoke;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;
using WinRT.Interop;

namespace SeeingSharp.WinUIDesktopSamples
{
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        private SampleBase _actSample;
        private SampleSettings _actSampleSettings;
        private SampleMetadata _actSampleInfo;
        private bool _isChangingSample;

        private List<ChildRenderWindow> _childPages;

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

        public MainWindowViewModel ViewModel => this.CtrlMainContent.DataContext as MainWindowViewModel;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            this.InitializeComponent();

            _childPages = new List<ChildRenderWindow>();

            // Set window title
            var package = Package.Current;
            var appName = package.DisplayName;
            this.Title = $@"{appName} - Main window - {Assembly.GetExecutingAssembly().GetName().Version}";

            this.LoadIcon("Icon.ico");

            this.CtrlMainContent.Loaded += this.OnLoaded;
        }

        private void LoadIcon(string iconName)
        {
            //Get the Window's HWND
            var hwnd = WindowNative.GetWindowHandle(this);
            var hIcon = User32.LoadImage(IntPtr.Zero, iconName,
                User32.ImageType.IMAGE_ICON, 32, 32, User32.LoadImageFlags.LR_LOADFROMFILE);

            User32.SendMessage(hwnd, User32.WindowMessage.WM_SETICON, (IntPtr)0, hIcon);
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

            this.UpdateMainMenuBar();
        }

        private void UpdateMainMenuBar()
        {
            // Clear previous sample commands from UI
            var sampleCommandMenuItems = this.CtrlMainMenuBar.Items
                .Where(actItem => actItem.Tag as string == "SampleCommand")
                .ToList();
            foreach(var actSampleCommandMenuItem in sampleCommandMenuItems)
            {
                this.CtrlMainMenuBar.Items.Remove(actSampleCommandMenuItem);
            }

            // Add new sample commands to UI
            foreach(var actSampleCommand in this.ViewModel.SampleCommands)
            {
                var actSampleCommandInner = actSampleCommand;

                var newBarItem = new MenuBarItem()
                {
                    Title = actSampleCommandInner.CommandText,
                    IsEnabled = actSampleCommandInner.CanExecuteAsProperty,
                };
                newBarItem.Tapped += (sender, eArgs) =>
                {
                    if (!actSampleCommandInner.CanExecuteAsProperty) { return; }
                    actSampleCommandInner.Execute(null);
                };
                newBarItem.Tag = "SampleCommand";
                newBarItem.Style = (Style)App.Current.Resources["MainMenuBarItem"];

                this.CtrlMainMenuBar.Items.Add(newBarItem);
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

            var viewModel = (MainWindowViewModel)this.CtrlMainContent.DataContext;
            //viewModel.NewChildWindowRequest += this.OnViewModel_NewChildWindowRequest;
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
            if (!(this.CtrlMainContent.DataContext is MainWindowViewModel viewModel)) { return; }

            var selectedSample = viewModel.SelectedSample;
            if (selectedSample == null) { return; }

            this.ApplySample(selectedSample.SampleMetadata, viewModel.SampleSettings);
        }

        private void OnSampleCommand_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!(sender is NavigationViewItem navViewItem)) { return; }
            if (!(navViewItem.Tag is SampleCommand sampleCommand)) { return; }

            sampleCommand.Execute(null);
        }
    }
}
