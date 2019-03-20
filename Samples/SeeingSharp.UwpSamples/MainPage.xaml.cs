using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;

namespace SeeingSharp.UwpSamples
{
    public sealed partial class MainPage : Page
    {
        private SampleBase m_actSample;
        private SampleMetadata m_actSampleInfo;
        private bool m_isChangingSample;

        public MainPage()
        {
            this.InitializeComponent();

            this.Loaded += this.OnLoaded;
        }

        /// <summary>
        /// Applies the given sample.
        /// </summary>
        private async void ApplySample(SampleMetadata sampleInfo, SampleSettings sampleSettings)
        {
            if (DesignMode.DesignModeEnabled) { return; }
            if (m_isChangingSample) { return; }

            m_isChangingSample = true;
            try
            {
                if (m_actSampleInfo == sampleInfo) { return; }

                // Clear previous sample
                if (m_actSampleInfo != null)
                {
                    await CtrlSwapChain.RenderLoop.Scene.ManipulateSceneAsync(manipulator =>
                    {
                        manipulator.Clear(true);
                    });
                    await CtrlSwapChain.RenderLoop.Clear2DDrawingLayersAsync();
                    m_actSample.NotifyClosed();
                }

                // Reset members
                m_actSample = null;
                m_actSampleInfo = null;

                // Apply new sample
                if (sampleInfo != null)
                {
                    var sampleObject = sampleInfo.CreateSampleObject();
                    await sampleObject.OnStartupAsync(CtrlSwapChain.RenderLoop, sampleSettings);

                    m_actSample = sampleObject;
                    m_actSampleInfo = sampleInfo;

                    await CtrlSwapChain.RenderLoop.Register2DDrawingLayerAsync(
                        new PerformanceMeasureDrawingLayer(GraphicsCore.Current.PerformanceCalculator, 130f));
                }

                // Wait for next finished rendering
                await CtrlSwapChain.RenderLoop.WaitForNextFinishedRenderAsync();

                await CtrlSwapChain.RenderLoop.WaitForNextFinishedRenderAsync();
            }
            finally
            {
                m_isChangingSample = false;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled) { return; }

            var sampleRepo = new SampleRepository();
            sampleRepo.LoadSampleData();

            var viewModel = this.DataContext as MainWindowViewModel;
            viewModel?.LoadSampleData(sampleRepo, CtrlSwapChain.RenderLoop);

            // Start update loop
            CtrlSwapChain.RenderLoop.PrepareRender += (innerSender, eArgs) =>
            {
                var actSample = m_actSample;
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
    }
}