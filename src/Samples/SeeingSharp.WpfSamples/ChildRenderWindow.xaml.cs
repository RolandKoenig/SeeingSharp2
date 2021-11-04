using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using SeeingSharp.Core;
using SeeingSharp.Drawing3D;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;

namespace SeeingSharp.WpfSamples
{
    /// <summary>
    /// Interaction logic for ChildRenderWindow.xaml
    /// </summary>
    public partial class ChildRenderWindow : Window
    {
        public bool DiscardPresent
        {
            get => this.CtrlRenderer.DiscardPresent;
            set => this.CtrlRenderer.DiscardPresent = value;
        }

        public ChildRenderWindow()
        {
            this.InitializeComponent();

            this.Loaded += this.OnLoaded;
        }

        public void InitializeChildWindow(Scene scene, Camera3DViewPoint viewPoint)
        {
            CtrlRenderer.Scene = scene;
            CtrlRenderer.Camera.ApplyViewPoint(viewPoint);
        }

        public async Task SetRenderingDataAsync(SampleBase actSample)
        {
            await actSample.OnInitRenderingWindowAsync(CtrlRenderer.RenderLoop);

            await CtrlRenderer.RenderLoop.Register2DDrawingLayerAsync(
                new PerformanceMeasureDrawingLayer(10f, CtrlRenderer.ViewInformation));
        }

        public async Task ClearAsync()
        {
            await CtrlRenderer.RenderLoop.Clear2DDrawingLayersAsync();
            CtrlRenderer.RenderLoop.ObjectFilters.Clear();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.Title = $@"{this.Title} ({Assembly.GetExecutingAssembly().GetName().Version})";
        }
    }
}
