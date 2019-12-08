using System.Reflection;
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
        private SampleSettings m_actSampleSettings;
        private SampleMetadata m_actSampleInfo;
        private bool m_isChangingSample;

        public MainPage()
        {
            this.InitializeComponent();

            // Manipulate titlebar
            //  see https://social.msdn.microsoft.com/Forums/windows/en-US/08462adc-a7ba-459f-9d2b-32a14c7a7de1/uwp-how-to-change-the-text-of-the-application-title-bar?forum=wpdevelop
            var package = Package.Current;
            var appName = package.DisplayName;
            TextAppTitle.Text = $@"{appName} ({Assembly.GetExecutingAssembly().GetName().Version})";
            Window.Current.SetTitleBar(AppTitleBar);

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
                if (m_actSampleSettings != null)
                {
                    m_actSampleSettings.RecreateRequest -= this.OnSampleSettings_RecreateRequest;
                }

                // Reset members
                m_actSample = null;
                m_actSampleSettings = null;
                m_actSampleInfo = null;

                // Apply new sample
                if (sampleInfo != null)
                {
                    var sampleObject = sampleInfo.CreateSampleObject();
                    await sampleObject.OnStartupAsync(CtrlSwapChain.RenderLoop, sampleSettings);
                    await sampleObject.OnReloadAsync(CtrlSwapChain.RenderLoop, sampleSettings);

                    m_actSample = sampleObject;
                    m_actSampleSettings = sampleSettings;
                    m_actSampleInfo = sampleInfo;

                    if (m_actSampleSettings != null)
                    {
                        m_actSampleSettings.RecreateRequest += this.OnSampleSettings_RecreateRequest;
                    }

                    await CtrlSwapChain.RenderLoop.Register2DDrawingLayerAsync(
                        new PerformanceMeasureDrawingLayer(GraphicsCore.Current.PerformanceAnalyzer, 130f));
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

        private async void OnSampleSettings_RecreateRequest(object sender, System.EventArgs e)
        {
            var sample = m_actSample;
            var sampleSettings = m_actSampleSettings;
            if (sample == null)
            {
                return;
            }
            if (sampleSettings == null)
            {
                return;
            }

            await sample.OnReloadAsync(CtrlSwapChain.RenderLoop, sampleSettings);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled) { return; }

            //this.na = $@"{this.Title} ({Assembly.GetExecutingAssembly().GetName().Version})";

            var sampleRepo = new SampleRepository();
            sampleRepo.LoadSampleData();

            var viewModel = this.DataContext as MainWindowViewModel;
            viewModel?.LoadSampleData(sampleRepo, CtrlSwapChain.RenderLoop);

            // Start update loop
            CtrlSwapChain.RenderLoop.ViewConfiguration.AlphaEnabledSwapChain = true;
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