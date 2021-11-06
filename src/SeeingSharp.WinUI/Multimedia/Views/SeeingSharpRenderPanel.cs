using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Foundation;
using Microsoft.UI.Xaml.Controls;
using SeeingSharp.Core;
using SeeingSharp.Core.Configuration;
using SeeingSharp.Drawing3D;
using SeeingSharp.Input;

namespace SeeingSharp.Views
{
    public class SeeingSharpRenderPanel : SwapChainPanel, IInputEnabledView, INotifyPropertyChanged
    {
        private SeeingSharpPanelPainter _painter;

        /// <summary>
        /// Gets or sets the currently applied scene.
        /// </summary>
        public Scene Scene
        {
            get => this.RenderLoop.Scene;
            set => this.RenderLoop.SetScene(value);
        }

        /// <summary>
        /// Gets the RenderLoop that is currently in use.
        /// </summary>
        public RenderLoop RenderLoop => _painter.RenderLoop;

        /// <summary>
        /// Does the target control have focus?
        /// </summary>
        public bool Focused => ((IInputEnabledView)_painter).Focused;

        /// <summary>
        /// Discard rendering?
        /// </summary>
        public bool DiscardRendering
        {
            get => _painter.DiscardRendering;
            set => _painter.DiscardRendering = value;
        }

        /// <summary>
        /// Discard presenting frames?
        /// </summary>
        public bool DiscardPresent
        {
            get => _painter.DiscardPresent;
            set => _painter.DiscardPresent = value;
        }

        public EngineDevice SelectedDevice
        {
            get => _painter.RenderLoop.Device;
            set => _painter.RenderLoop.SetRenderingDevice(value);
        }

        public GraphicsViewConfiguration Configuration => _painter.RenderLoop.Configuration;

        public Size CurrentViewSize
        {
            get
            {
                var currentViewSize = _painter.RenderLoop.CurrentViewSize;

                var result = new Size
                {
                    Width = currentViewSize.Width,
                    Height = currentViewSize.Height
                };

                return result;
            }
        }

        public IEnumerable<EngineDevice> PossibleDevices
        {
            get
            {
                if (!GraphicsCore.IsLoaded)
                {
                    return new EngineDevice[0];
                }

                return GraphicsCore.Current.Devices;
            }
        }

        /// <summary>
        /// Gets or sets the camera.
        /// </summary>
        public Camera3DBase Camera
        {
            get => this.RenderLoop.Camera;
            set => this.RenderLoop.Camera = value;
        }

        public ViewInformation ViewInformation => this.RenderLoop.ViewInformation;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeeingSharpRenderPanel"/> class.
        /// </summary>
        public SeeingSharpRenderPanel()
        {
            _painter = new SeeingSharpPanelPainter(this);
            _painter.RenderLoop.CurrentViewSizeChanged += this.OnRenderLoop_CurrentViewSizeChanged;
            _painter.RenderLoop.DeviceChanged += this.OnRenderLoop_DeviceChanged;
        }

        private void OnRenderLoop_DeviceChanged(object sender, EventArgs e)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.SelectedDevice)));
        }

        private void OnRenderLoop_CurrentViewSizeChanged(object sender, EventArgs e)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CurrentViewSize)));
        }
    }
}