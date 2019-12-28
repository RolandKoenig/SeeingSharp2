using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;
using Window = Windows.UI.Xaml.Window;

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


            this.Loaded += OnLoaded;
        }

        public void InitializeChildWindow(Scene scene, Camera3DViewPoint viewPoint)
        {
            this.CtrlSwapChain.Scene = scene;
            this.CtrlSwapChain.Camera.ApplyViewPoint(viewPoint);
        }

        public async Task SetRenderingDataAsync(SampleBase actSample)
        {
            await actSample.OnInitRenderingWindowAsync(this.CtrlSwapChain.RenderLoop);

            await CtrlSwapChain.RenderLoop.Register2DDrawingLayerAsync(
                new PerformanceMeasureDrawingLayer(GraphicsCore.Current.PerformanceAnalyzer, 10f));
        }

        public async Task ClearAsync()
        {
            await CtrlSwapChain.RenderLoop.Clear2DDrawingLayersAsync();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled) { return; }

            CtrlSwapChain.RenderLoop.ViewConfiguration.AlphaEnabledSwapChain = true;
        }
    }
}
