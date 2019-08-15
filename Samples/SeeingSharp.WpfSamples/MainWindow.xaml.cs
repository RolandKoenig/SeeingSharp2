using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;

namespace SeeingSharp.WpfSamples
{
    public partial class MainWindow : Window
    {
        private SampleBase m_actSample;
        private SampleMetadata m_actSampleInfo;
        private bool m_isChangingSample;

        public MainWindow()
        {
            this.InitializeComponent();

            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var sampleRepo = new SampleRepository();
                sampleRepo.LoadSampleData();
                this.DataContext = new MainWindowViewModel(sampleRepo, CtrlRenderer.RenderLoop);

                CtrlRenderer.RenderLoop.PrepareRender += this.OnRenderLoop_PrepareRender;
            }
        }

        private void OnSelectedSampleChanged(object sender, SelectionChangedEventArgs e)
        {
            if(!(this.DataContext is MainWindowViewModel viewModel)) { return; }

            var selectedSample = viewModel.SelectedSample;
            if(selectedSample == null) { return; }

            this.ApplySample(selectedSample.SampleMetadata, viewModel.SampleSettings);
        }

        /// <summary>
        /// Applies the given sample.
        /// </summary>
        private async void ApplySample(SampleMetadata sampleInfo, SampleSettings sampleSettings)
        {
            if (m_isChangingSample) { return; }

            m_isChangingSample = true;
            try
            {
                if (m_actSampleInfo == sampleInfo) { return; }

                // Clear previous sample
                if (m_actSampleInfo != null)
                {
                    await CtrlRenderer.RenderLoop.Scene.ManipulateSceneAsync(manipulator =>
                    {
                        manipulator.Clear(true);
                    });
                    await CtrlRenderer.RenderLoop.Clear2DDrawingLayersAsync();
                    m_actSample.NotifyClosed();
                }

                // Reset members
                m_actSample = null;
                m_actSampleInfo = null;

                // Apply new sample
                if (sampleInfo != null)
                {
                    var sampleObject = sampleInfo.CreateSampleObject();
                    await sampleObject.OnStartupAsync(CtrlRenderer.RenderLoop, sampleSettings);

                    m_actSample = sampleObject;
                    m_actSampleInfo = sampleInfo;

                    await CtrlRenderer.RenderLoop.Register2DDrawingLayerAsync(
                        new PerformanceMeasureDrawingLayer(GraphicsCore.Current.PerformanceAnalyzer, 120f));
                }

                // Wait for next finished rendering
                await CtrlRenderer.RenderLoop.WaitForNextFinishedRenderAsync();

                await CtrlRenderer.RenderLoop.WaitForNextFinishedRenderAsync();
            }
            finally
            {
                m_isChangingSample = false;
            }
        }

        private void OnRenderLoop_PrepareRender(object sender, EventArgs e)
        {
            var actSample = m_actSample;
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
    }
}