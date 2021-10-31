/*
    SeeingSharp and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Foundation;
using Microsoft.UI.Xaml.Controls;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Input;

namespace SeeingSharp.Multimedia.Views
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