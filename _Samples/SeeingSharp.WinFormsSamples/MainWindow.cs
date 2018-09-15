using SeeingSharp.SampleContainer;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SeeingSharp.Multimedia.Core;
using SharpDX;

namespace SeeingSharp.WinFormsSamples
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();

            UpdateWindowState();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            foreach (var actDevice in GraphicsCore.Current.Devices)
            {
                var newButton = new ToolStripButton($"to {actDevice.AdapterDescription}");
                newButton.Tag = actDevice;
                newButton.Click += OnCmdChangeDevice_Click;
                m_mnuChangeDevice.DropDownItems.Add(newButton);
            }

            SampleRepository sampleRepo = new SampleRepository();
            await sampleRepo.SampleGroups
                .First()
                .Samples
                .First()
                .CreateSampleObject().OnStartupAsync(m_ctrlRenderPanel.RenderLoop);

            UpdateWindowState();
        }

        /// <summary>
        /// Changes the render resolution to given width and height.
        /// </summary>
        /// <param name="width">The width in pixels.</param>
        /// <param name="height">The height in pixels.</param>
        private void ChangeRenderResolution(int width, int height)
        {
            var renderControl = m_ctrlRenderPanel;
            if (renderControl == null) { return; }

            Size2 currentViewSize = renderControl.RenderLoop.CurrentViewSize;
            Size2 currentWindowSize = new Size2(this.Width, this.Height);
            Size2 difference = new Size2(
                currentWindowSize.Width - currentViewSize.Width,
                currentWindowSize.Height - currentViewSize.Height);
            Size2 newWindowSize = new Size2(width + difference.Width, height + difference.Height);

            this.WindowState = FormWindowState.Normal;
            this.Width = newWindowSize.Width;
            this.Height = newWindowSize.Height;
        }

        private void UpdateWindowState()
        {
            var viewSize = m_ctrlRenderPanel.RenderLoop.ViewInformation.CurrentViewSize;
            m_lblResolution.Text = $"{viewSize.Width}x{viewSize.Height}";
            m_lblObjectCount.Text = m_ctrlRenderPanel.RenderLoop.Scene.CountObjects.ToString();
            m_lblDevice.Text = m_ctrlRenderPanel.RenderLoop.Device?.AdapterDescription ?? "-";
        }

        private void OnRefreshTimer_Tick(object sender, EventArgs e)
        {
            UpdateWindowState();
        }

        private void OnCmdChangeResolution_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem?.Tag == null) { return; }

            var splittedResolution = menuItem.Tag.ToString().Split('x');
            if (splittedResolution.Length != 2) { return; }
            if (!int.TryParse(splittedResolution[0], out int width)) { return; }
            if (!int.TryParse(splittedResolution[1], out int height)) { return; }

            ChangeRenderResolution(width, height);
        }

        private async void OnCmdCopyScreenshot_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = await m_ctrlRenderPanel.RenderLoop.GetScreenshotGdiAsync();
            Clipboard.SetImage(bitmap);
        }

        private void OnCmdChangeDevice_Click(object sender, EventArgs e)
        {
            if (!(sender is ToolStripButton changeButton)) { return; }
            if(!(changeButton.Tag is EngineDevice device)) { return; }

            m_ctrlRenderPanel.RenderLoop.SetRenderingDevice(device);
        }
    }
}
