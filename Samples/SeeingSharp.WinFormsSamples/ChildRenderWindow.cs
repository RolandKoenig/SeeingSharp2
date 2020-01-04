using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
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
            InitializeComponent();
        }

        public void InitializeChildWindow(Scene scene, Camera3DViewPoint viewPoint)
        {
            m_ctrlRenderer.Scene = scene;
            m_ctrlRenderer.Camera.ApplyViewPoint(viewPoint);
        }

        public async Task SetRenderingDataAsync(SampleBase actSample)
        {
            await actSample.OnInitRenderingWindowAsync(m_ctrlRenderer.RenderLoop);

            await m_ctrlRenderer.RenderLoop.Register2DDrawingLayerAsync(
                new PerformanceMeasureDrawingLayer(GraphicsCore.Current.PerformanceAnalyzer, 10f));
        }

        public async Task ClearAsync()
        {
            await m_ctrlRenderer.RenderLoop.Clear2DDrawingLayersAsync();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.DesignMode)
            {
                return;
            }

            this.Text = $@"{this.Text} ({Assembly.GetExecutingAssembly().GetName().Version})";
        }

        private void OnRefreshTimer_Tick(object sender, EventArgs e)
        {
            this.m_renderWindowControlsComponent.UpdateTargetControlStates();
        }
    }
}
