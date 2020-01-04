using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Views;

namespace SeeingSharp.WinFormsSamples
{
    [SuppressMessage("ReSharper", "LocalizableElement")]
    public class RenderWindowControlsComponent : Component
    {
        private SeeingSharpRendererControl m_renderControl;
        private ToolStripDropDownButton m_mnuChooseDevice;
        private ToolStripDropDownButton m_mnuChangeResolution;

        public void UpdateTargetControlStates()
        {
            if (!GraphicsCore.IsLoaded) { return; }

            // Update all status labels
            if (this.LblCurrentResolution != null)
            {
                var viewSize = m_renderControl.RenderLoop.ViewInformation.CurrentViewSize;
                this.LblCurrentResolution.Text = $"{viewSize.Width}x{viewSize.Height}";
            }
            if (this.LblCurrentObjectCount != null)
            {
                this.LblCurrentObjectCount.Text = m_renderControl.RenderLoop.Scene.CountObjects.ToString();
            }
            if (this.LblCurrentDevice != null)
            {
                this.LblCurrentDevice.Text = m_renderControl.RenderLoop.Device?.AdapterDescription ?? "-";
            }
        }

        public void ChangeRenderResolution(int width, int height)
        {
            var renderControl = m_renderControl;
            if (renderControl == null) { return; }

            var targetWindow = this.TargetWindow;
            if(targetWindow == null){ return; }

            var currentViewSize = renderControl.RenderLoop.CurrentViewSize;
            var currentWindowSize = new Size2(targetWindow.Width, targetWindow.Height);
            var difference = new Size2(
                currentWindowSize.Width - currentViewSize.Width,
                currentWindowSize.Height - currentViewSize.Height);
            var newWindowSize = new Size2(width + difference.Width, height + difference.Height);

            targetWindow.WindowState = FormWindowState.Normal;
            targetWindow.Width = newWindowSize.Width;
            targetWindow.Height = newWindowSize.Height;
        }

        private void OnCmdChangeDevice_Click(object sender, EventArgs eArgs)
        {
            if (m_renderControl == null) { return; }
            if (!(sender is ToolStripButton changeButton)) { return; }
            if (!(changeButton.Tag is EngineDevice device)) { return; }

            m_renderControl.RenderLoop.SetRenderingDevice(device);
        }

        private void OnCmdChangeResolution_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;

            if (menuItem?.Tag == null) { return; }

            var splittedResolution = menuItem.Tag.ToString().Split('x');

            if (splittedResolution.Length != 2) { return; }
            if (!int.TryParse(splittedResolution[0], out var width)) { return; }
            if (!int.TryParse(splittedResolution[1], out var height)) { return; }

            this.ChangeRenderResolution(width, height);
        }

        public SeeingSharpRendererControl RenderControl
        {
            get => m_renderControl;
            set
            {
                if (m_renderControl != value)
                {
                    m_renderControl = value;
                    if (m_renderControl != null)
                    {
                        this.UpdateTargetControlStates();
                    }
                }
            }
        }

        public ToolStripStatusLabel LblCurrentResolution
        {
            get;
            set;
        }

        public ToolStripStatusLabel LblCurrentObjectCount
        {
            get;
            set;
        }

        public ToolStripStatusLabel LblCurrentDevice
        {
            get;
            set;
        }

        public ToolStripDropDownButton MnuChooseDevice
        {
            get => m_mnuChooseDevice;
            set
            {
                if (m_mnuChooseDevice != value)
                {
                    // Clear previously generated menu buttons
                    m_mnuChooseDevice?.DropDownItems.Clear();
         
                    // Update member
                    m_mnuChooseDevice = value;

                    // Generate one menu button per device
                    if (GraphicsCore.IsLoaded)
                    {
                        foreach (var actDevice in GraphicsCore.Current.Devices)
                        {
                            var newButton = new ToolStripButton($"to {actDevice.AdapterDescription}")
                            {
                                Tag = actDevice
                            };

                            newButton.Click += this.OnCmdChangeDevice_Click;
                            m_mnuChooseDevice.DropDownItems.Add(newButton);
                        }
                    }
                }
            }
        }

        public ToolStripDropDownButton MnuChangeResolution
        {
            get => m_mnuChangeResolution;
            set
            {
                if (m_mnuChangeResolution != value)
                {
                    // Deregister from old menu
                    foreach (var actItem in m_mnuChangeResolution.DropDownItems)
                    {
                        if (!(actItem is ToolStripMenuItem actDropDownButton)) { continue; }
                        if (!(actDropDownButton.Tag is string actTag)){ continue; }
                        if (!actTag.Contains("x")){ continue; }

                        actDropDownButton.Click -= this.OnCmdChangeResolution_Click;
                    }

                    // Update member
                    m_mnuChangeResolution = value;

                    // Register on new menu
                    if(GraphicsCore.IsLoaded)
                    {
                        foreach (var actItem in m_mnuChangeResolution.DropDownItems)
                        {
                            if (!(actItem is ToolStripMenuItem actDropDownButton)) { continue; }
                            if (!(actDropDownButton.Tag is string actTag)){ continue; }
                            if (!actTag.Contains("x")){ continue; }

                            actDropDownButton.Click += this.OnCmdChangeResolution_Click;
                        }
                    }
                }
            }
        }

        public Form TargetWindow
        {
            get;
            set;
        }
    }
}
