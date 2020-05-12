using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;

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

            // Manipulate titlebar
            //  see https://social.msdn.microsoft.com/Forums/windows/en-US/08462adc-a7ba-459f-9d2b-32a14c7a7de1/uwp-how-to-change-the-text-of-the-application-title-bar?forum=wpdevelop
            var package = Package.Current;
            var appName = package.DisplayName;
            TextAppTitle.Text = $@"{appName} - Child window - {Assembly.GetExecutingAssembly().GetName().Version}";
            Window.Current.SetTitleBar(TextAppTitleRow);

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
            CtrlSwapChain.RenderLoop.Filters.Clear();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled) { return; }

            CtrlSwapChain.RenderLoop.Configuration.AlphaEnabledSwapChain = true;
        }
    }
}
