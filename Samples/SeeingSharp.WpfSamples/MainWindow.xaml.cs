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
using SeeingSharp.SampleContainer.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

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

            this.Title = $@"{this.Title} ({Assembly.GetExecutingAssembly().GetName().Version})";

            var sampleRepo = new SampleRepository();
            sampleRepo.LoadSampleData();

            var viewModel = new MainWindowViewModel(sampleRepo, CtrlRenderer.RenderLoop);
            viewModel.ReloadRequest += this.OnViewModel_ReloadRequest;
            this.DataContext = viewModel;

            // Register viewbox filter
            this.CtrlRenderer.RenderLoop.Filters.Add(new SceneViewboxObjectFilter());

            CtrlRenderer.RenderLoop.PrepareRender += this.OnRenderLoop_PrepareRender;
        }

        private void OnRenderLoop_ManipulateFilterList(object sender, ManipulateFilterListArgs e)
        {
            if (e.FilterList.Count == 0)
            {
                e.FilterList.Add(new SceneViewboxObjectFilter());
            }
        }

        private async void OnViewModel_ReloadRequest(object sender, EventArgs e)
        {
            if (!(this.DataContext is MainWindowViewModel viewModel))
            {
                return;
            }

            if (_actSample != null)
            {
                await _actSample.OnReloadAsync(CtrlRenderer.RenderLoop, viewModel.SampleSettings);
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

                // Clear previous sample
                if (_actSampleInfo != null)
                {
                    await CtrlRenderer.RenderLoop.Scene.ManipulateSceneAsync(manipulator =>
                    {
                        manipulator.Clear(true);
                    });
                    await CtrlRenderer.RenderLoop.Clear2DDrawingLayersAsync();

                    foreach (var actChildWindow in _childWindows)
                    {
                        await actChildWindow.ClearAsync();
                    }

                    _actSample.OnClosed();
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
                        new PerformanceMeasureDrawingLayer(GraphicsCore.Current.PerformanceAnalyzer, 120f));
                }

                // Wait for next finished rendering
                await CtrlRenderer.RenderLoop.WaitForNextFinishedRenderAsync();

                await CtrlRenderer.RenderLoop.WaitForNextFinishedRenderAsync();
            }
            finally
            {
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

            var diffToFullWidth = this.Width - this.CtrlRenderer.RenderLoop.CurrentViewSize.Width;
            var diffToFullHeight = this.Height - this.CtrlRenderer.RenderLoop.CurrentViewSize.Height;
            this.Width = targetWith + diffToFullWidth;
            this.Height = targetHeight + diffToFullHeight;
        }
    }
}