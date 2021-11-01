using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;
using SeeingSharp.UwpSamples.Util;

namespace SeeingSharp.UwpSamples
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChildRenderPage : Page
    {
        public bool DiscardPresent
        {
            get => this.CtrlSwapChain.DiscardPresent;
            set => this.CtrlSwapChain.DiscardPresent = value;
        }

        public ChildRenderPage()
        {
            this.InitializeComponent();

            CommonUtil.UpdateApplicationTitleBar("Child window");

            this.Loaded += this.OnLoaded;
        }

        public void InitializeChildWindow(Scene scene, Camera3DViewPoint viewPoint)
        {
            CtrlSwapChain.Scene = scene;
            CtrlSwapChain.Camera.ApplyViewPoint(viewPoint);
        }

        public async void TriggerRenderControlFadeInAnimation()
        {
            try
            {
                await this.Dispatcher.RunAsync(
                    CoreDispatcherPriority.Normal,
                    () => { this.SwapChainFadeInStoryboard.Begin(); });
            }
            catch (Exception)
            {
                // We cane safely ignore exceptions here (only an optional animation)
            }
        }

        public async Task SetRenderingDataAsync(SampleBase actSample)
        {
            await actSample.OnInitRenderingWindowAsync(CtrlSwapChain.RenderLoop);

            await CtrlSwapChain.RenderLoop.Register2DDrawingLayerAsync(
                new PerformanceMeasureDrawingLayer(10f, CtrlSwapChain.ViewInformation));
        }

        public async Task ClearAsync()
        {
            await CtrlSwapChain.RenderLoop.Clear2DDrawingLayersAsync();
            CtrlSwapChain.RenderLoop.ObjectFilters.Clear();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled) { return; }

            CtrlSwapChain.RenderLoop.Configuration.AlphaEnabledSwapChain = true;
        }
    }
}
