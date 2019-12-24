using System;
using System.Collections.Generic;
using System.Text;
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
            InitializeComponent();

            this.Loaded += OnLoaded;
        }

        public void SetRenderingData(Scene scene, Camera3DViewPoint viewPoint)
        {
            this.CtrlRenderer.Scene = scene;
            this.CtrlRenderer.Camera.ApplyViewPoint(viewPoint);
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!GraphicsCore.IsLoaded)
            {
                return;
            }

            await CtrlRenderer.RenderLoop.Register2DDrawingLayerAsync(
                new PerformanceMeasureDrawingLayer(GraphicsCore.Current.PerformanceAnalyzer, 10f));
        }
    }
}
