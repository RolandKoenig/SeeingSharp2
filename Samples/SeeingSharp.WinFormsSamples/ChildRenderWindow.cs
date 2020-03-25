using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;

namespace SeeingSharp.WinFormsSamples
{
    public partial class ChildRenderWindow : Form
    {
        public ChildRenderWindow()
        {
            this.InitializeComponent();
        }

        public void InitializeChildWindow(Scene scene, Camera3DViewPoint viewPoint)
        {
            _ctrlRenderer.Scene = scene;
            _ctrlRenderer.Camera.ApplyViewPoint(viewPoint);
        }

        public async Task SetRenderingDataAsync(SampleBase actSample)
        {
            await actSample.OnInitRenderingWindowAsync(_ctrlRenderer.RenderLoop);

            await _ctrlRenderer.RenderLoop.Register2DDrawingLayerAsync(
                new PerformanceMeasureDrawingLayer(10f, _ctrlRenderer.ViewInformation));
        }

        public async Task ClearAsync()
        {
            await _ctrlRenderer.RenderLoop.Clear2DDrawingLayersAsync();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.DesignMode)
            {
                return;
            }

            this.Text = $@"{this.Text} ({Assembly.GetExecutingAssembly().GetName().Version})";

            // Register viewbox filter
            _ctrlRenderer.RenderLoop.Filters.Add(new SceneViewboxObjectFilter());
        }

        private void OnRefreshTimer_Tick(object sender, EventArgs e)
        {
            _renderWindowControlsComponent.UpdateTargetControlStates();
        }
    }
}
