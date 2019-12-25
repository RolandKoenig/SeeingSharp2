using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;

namespace SeeingSharp.WpfSamples
{
    /// <summary>
    /// Interaction logic for ChildRenderWindow.xaml
    /// </summary>
    public partial class ChildRenderWindow : Window
    {
        public ChildRenderWindow()
        {
            this.InitializeComponent();

            this.Loaded += this.OnLoaded;
        }

        public void InitializeChildWindow(Scene scene, Camera3DViewPoint viewPoint)
        {
            this.CtrlRenderer.Scene = scene;
            this.CtrlRenderer.Camera.ApplyViewPoint(viewPoint);
        }

        public async Task SetRenderingDataAsync(SampleBase actSample)
        {
            await actSample.OnInitRenderingWindowAsync(this.CtrlRenderer.RenderLoop);

            await CtrlRenderer.RenderLoop.Register2DDrawingLayerAsync(
                new PerformanceMeasureDrawingLayer(GraphicsCore.Current.PerformanceAnalyzer, 10f));
        }

        public async Task ClearAsync()
        {
            await CtrlRenderer.RenderLoop.Clear2DDrawingLayersAsync();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.Title = $@"{this.Title} ({Assembly.GetExecutingAssembly().GetName().Version})";
        }

        private void OnCmdClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
