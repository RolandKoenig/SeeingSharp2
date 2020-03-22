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
        private SeeingSharpRendererControl _renderControl;
        private ToolStripDropDownButton _mnuChooseDevice;
        private ToolStripDropDownButton _mnuChangeResolution;

        public void UpdateTargetControlStates()
        {
            if (!GraphicsCore.IsLoaded) { return; }

            // Update all status labels
            if (this.LblCurrentResolution != null)
            {
                var viewSize = _renderControl.RenderLoop.ViewInformation.CurrentViewSize;
                this.LblCurrentResolution.Text = $"{viewSize.Width}x{viewSize.Height}";
            }
            if(this.LblCurrentResourceCount != null)
            {
                this.LblCurrentResourceCount.Text = _renderControl.RenderLoop.CountGraphicsResources.ToString();
            }
            if (this.LblCurrentObjectCount != null)
            {
                this.LblCurrentObjectCount.Text = _renderControl.RenderLoop.CountVisibleObjects.ToString();
            }
            if (this.LblCurrentDevice != null)
            {
                this.LblCurrentDevice.Text = _renderControl.RenderLoop.Device?.AdapterDescription ?? "-";
            }
        }

        public void ChangeRenderResolution(int width, int height)
        {
            var renderControl = _renderControl;
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
            if (_renderControl == null) { return; }
            if (!(sender is ToolStripButton changeButton)) { return; }
            if (!(changeButton.Tag is EngineDevice device)) { return; }

            _renderControl.RenderLoop.SetRenderingDevice(device);
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
            get => _renderControl;
            set
            {
                if (_renderControl != value)
                {
                    _renderControl = value;
                    if (_renderControl != null)
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

        public ToolStripStatusLabel LblCurrentResourceCount
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
            get => _mnuChooseDevice;
            set
            {
                if (_mnuChooseDevice != value)
                {
                    // Clear previously generated menu buttons
                    _mnuChooseDevice?.DropDownItems.Clear();
         
                    // Update member
                    _mnuChooseDevice = value;

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
                            _mnuChooseDevice.DropDownItems.Add(newButton);
                        }
                    }
                }
            }
        }

        public ToolStripDropDownButton MnuChangeResolution
        {
            get => _mnuChangeResolution;
            set
            {
                if (_mnuChangeResolution != value)
                {
                    // Deregister from old menu
                    if (_mnuChangeResolution != null)
                    {
                        foreach (var actItem in _mnuChangeResolution.DropDownItems)
                        {
                            if (!(actItem is ToolStripMenuItem actDropDownButton)) { continue; }
                            if (!(actDropDownButton.Tag is string actTag)){ continue; }
                            if (!actTag.Contains("x")){ continue; }

                            actDropDownButton.Click -= this.OnCmdChangeResolution_Click;
                        }
                    }

                    // Update member
                    _mnuChangeResolution = value;

                    // Register on new menu
                    if(GraphicsCore.IsLoaded)
                    {
                        foreach (var actItem in _mnuChangeResolution.DropDownItems)
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
