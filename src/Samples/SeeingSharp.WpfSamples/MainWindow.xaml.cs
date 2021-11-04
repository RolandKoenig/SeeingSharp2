using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using SeeingSharp.Core;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;

namespace SeeingSharp.WpfSamples
{
    public partial class MainWindow : Window
    {
        private SampleBase _actSample;
        private SampleMetadata _actSampleInfo;
        private bool _isChangingSample;

        private List<ChildRenderWindow> _childWindows;

        public MainWindow()
        {
            this.InitializeComponent();

            _childWindows = new List<ChildRenderWindow>();

            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.Title = $@"{this.Title} ({Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "Unknown version"})";

            var sampleRepo = new SampleRepository();
            sampleRepo.LoadSampleData();

            var viewModel = new MainWindowViewModel(sampleRepo, CtrlRenderer.RenderLoop);
            viewModel.ReloadRequest += this.OnViewModel_ReloadRequest;
            this.DataContext = viewModel;

            CtrlRenderer.RenderLoop.PrepareRender += this.OnRenderLoop_PrepareRender;
        }

        private async void OnViewModel_ReloadRequest(object sender, EventArgs e)
        {
            if (!(this.DataContext is MainWindowViewModel viewModel))
            {
                return;
            }
            if (_actSample == null) { return; }

            if (_isChangingSample) { return; }
            _isChangingSample = true;

            try
            {
                // Discard presenting before updating current sample
                this.CtrlRenderer.DiscardPresent = true;
                foreach (var actChildWindow in _childWindows)
                {
                    actChildWindow.DiscardPresent = true;
                }

                // Update the sample
                await _actSample.OnReloadAsync(CtrlRenderer.RenderLoop, viewModel.SampleSettings);

                // Wait for next finished rendering
                await CtrlRenderer.RenderLoop.WaitForNextFinishedRenderAsync();
                await CtrlRenderer.RenderLoop.WaitForNextFinishedRenderAsync();
            }
            finally
            {
                // Continue presenting
                this.CtrlRenderer.DiscardPresent = false;
                foreach (var actChildWindow in _childWindows)
                {
                    actChildWindow.DiscardPresent = false;
                }

                // Reset lock for 'ApplySample' method
                _isChangingSample = false;
            }
        }

        private void OnSelectedSampleChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(this.DataContext is MainWindowViewModel viewModel)) { return; }

            var selectedSample = viewModel.SelectedSample;
            if (selectedSample == null) { return; }

            this.ApplySample(selectedSample.SampleMetadata, viewModel.SampleSettings);
        }

        /// <summary>
        /// Applies the given sample.
        /// </summary>
        private async void ApplySample(SampleMetadata sampleInfo, SampleSettings sampleSettings)
        {
            if (_isChangingSample) { return; }

            _isChangingSample = true;
            try
            {
                if (_actSampleInfo == sampleInfo) { return; }

                // Discard presenting before updating current sample
                this.CtrlRenderer.DiscardPresent = true;
                foreach (var actChildWindow in _childWindows)
                {
                    actChildWindow.DiscardPresent = true;
                }

                // Clear previous sample
                if (_actSampleInfo != null)
                {
                    await CtrlRenderer.RenderLoop.Scene.ManipulateSceneAsync(manipulator =>
                    {
                        manipulator.Clear(true);
                    });
                    await CtrlRenderer.RenderLoop.Clear2DDrawingLayersAsync();
                    CtrlRenderer.RenderLoop.ObjectFilters.Clear();

                    foreach (var actChildWindow in _childWindows)
                    {
                        await actChildWindow.ClearAsync();
                    }

                    _actSample.OnSampleClosed();
                }

                // Reset members
                _actSample = null;
                _actSampleInfo = null;

                // Apply new sample
                if (sampleInfo != null)
                {
                    var sampleObject = sampleInfo.CreateSampleObject();
                    await sampleObject.OnStartupAsync(CtrlRenderer.RenderLoop, sampleSettings);
                    await sampleObject.OnInitRenderingWindowAsync(CtrlRenderer.RenderLoop);
                    await sampleObject.OnReloadAsync(CtrlRenderer.RenderLoop, sampleSettings);

                    foreach (var actChildWindow in _childWindows)
                    {
                        await actChildWindow.SetRenderingDataAsync(sampleObject);
                    }

                    _actSample = sampleObject;
                    _actSampleInfo = sampleInfo;

                    await CtrlRenderer.RenderLoop.Register2DDrawingLayerAsync(
                        new PerformanceMeasureDrawingLayer(120f, CtrlRenderer.ViewInformation));
                }

                // Wait for next finished rendering
                await CtrlRenderer.RenderLoop.WaitForNextFinishedRenderAsync();
                await CtrlRenderer.RenderLoop.WaitForNextFinishedRenderAsync();
            }
            finally
            {
                // Continue presenting
                this.CtrlRenderer.DiscardPresent = false;
                foreach (var actChildWindow in _childWindows)
                {
                    actChildWindow.DiscardPresent = false;
                }

                _isChangingSample = false;
            }
        }

        private void OnRenderLoop_PrepareRender(object sender, EventArgs e)
        {
            var actSample = _actSample;
            actSample?.Update();
        }

        private void OnMnuCmdPerformance_ItemClick(object sender, RoutedEventArgs e)
        {
            var perfVM = new PerformanceOverviewViewModel(
                GraphicsCore.Current.PerformanceAnalyzer);
            var dlgPerformance = new PerformanceOverviewDialog();
            dlgPerformance.DataContext = perfVM;
            dlgPerformance.Owner = this;
            dlgPerformance.Show();
        }

        private async void OnMnuCmdNewChildWindow_Click(object sender, RoutedEventArgs e)
        {
            var childWindow = new ChildRenderWindow();
            childWindow.InitializeChildWindow(CtrlRenderer.Scene, CtrlRenderer.Camera.GetViewPoint());

            _childWindows.Add(childWindow);
            childWindow.Closed += (_1, _2) => { _childWindows.Remove(childWindow); };

            childWindow.Owner = this;
            childWindow.Show();

            await childWindow.SetRenderingDataAsync(_actSample);
        }

        private void OnMnuCmdChangeResolution_Click(object sender, RoutedEventArgs e)
        {
            if(!(sender is MenuItem menuItem)){ return; }

            var itemTag = (string)menuItem.Tag;
            var parts = itemTag.Split('x');
            var targetWith = int.Parse(parts[0]);
            var targetHeight = int.Parse(parts[1]);

            var diffToFullWidth = this.Width - CtrlRenderer.RenderLoop.CurrentViewSize.Width;
            var diffToFullHeight = this.Height - CtrlRenderer.RenderLoop.CurrentViewSize.Height;
            this.Width = targetWith + diffToFullWidth;
            this.Height = targetHeight + diffToFullHeight;
        }
    }
}