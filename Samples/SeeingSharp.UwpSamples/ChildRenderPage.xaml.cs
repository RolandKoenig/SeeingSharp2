using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;
using SeeingSharp.UwpSamples.Util;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SeeingSharp.UwpSamples
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChildRenderPage : Page
    {
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
