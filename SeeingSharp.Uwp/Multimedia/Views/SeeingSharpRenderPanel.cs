#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
#endregion
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace SeeingSharp.Multimedia.Views
{
    //[ContentProperty(Name = "SceneComponents")]
    public class SeeingSharpRenderPanel : SwapChainPanel, IInputEnabledView, INotifyPropertyChanged
    {
        #region Dependency properties
        public static readonly DependencyProperty SceneProperty =
            DependencyProperty.Register("Scene", typeof(Scene), typeof(SeeingSharpRenderPanel), new PropertyMetadata(new Scene(), OnPropertyChanged));
        public static readonly DependencyProperty CameraProperty =
            DependencyProperty.Register("Camera", typeof(Camera3DBase), typeof(SeeingSharpRenderPanel), new PropertyMetadata(new PerspectiveCamera3D(), OnPropertyChanged));
        public static readonly DependencyProperty DrawingLayer2DProperty =
            DependencyProperty.Register("DrawingLayer2D", typeof(Custom2DDrawingLayer), typeof(SeeingSharpRenderPanel), new PropertyMetadata(null, OnPropertyChanged));
        #endregion

        private SeeingSharpPanelPainter m_painter;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeeingSharpRenderPanel"/> class.
        /// </summary>
        public SeeingSharpRenderPanel()
        {
            m_painter = new SeeingSharpPanelPainter(this);
            m_painter.RenderLoop.CurrentViewSizeChanged += OnRenderLoop_CurrentViewSizeChanged;
            m_painter.RenderLoop.DeviceChanged += OnRenderLoop_DeviceChanged;
        }

        private void OnRenderLoop_DeviceChanged(object sender, EventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedDevice)));
        }

        private void OnRenderLoop_CurrentViewSizeChanged(object sender, EventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentViewSize)));
        }

        /// <summary>
        /// Called when one of the dependency properties has changed.
        /// </summary>
        private static async void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!GraphicsCore.IsInitialized) { return; }
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) { return; }

            SeeingSharpRenderPanel renderPanel = sender as SeeingSharpRenderPanel;
            if(renderPanel == null) { return; }

            if (e.Property == SeeingSharpRenderPanel.SceneProperty) { renderPanel.RenderLoop.SetScene(e.NewValue as Scene); }
            else if (e.Property == SeeingSharpRenderPanel.CameraProperty) { renderPanel.Camera = e.NewValue as Camera3DBase; }
            else if (e.Property == SeeingSharpRenderPanel.DrawingLayer2DProperty)
            {
                if (e.OldValue != null) { await renderPanel.RenderLoop.Deregister2DDrawingLayerAsync(e.OldValue as Custom2DDrawingLayer); }
                if (e.NewValue != null) { await renderPanel.RenderLoop.Register2DDrawingLayerAsync(e.NewValue as Custom2DDrawingLayer); }
            }
        }

        /// <summary>
        /// Discard rendering?
        /// </summary>
        public bool DiscardRendering
        {
            get { return m_painter.DiscardRendering; }
            set { m_painter.DiscardRendering = value; }
        }

        /// <summary>
        /// Gets or sets the currently applied scene.
        /// </summary>
        public Scene Scene
        {
            get { return (Scene)GetValue(SceneProperty); }
            set { SetValue(SceneProperty, value); }
        }

        /// <summary>
        /// Gets or sets the custom layer for 2D rendering.
        /// </summary>
        public Custom2DDrawingLayer DrawingLayer2D
        {
            get { return (Custom2DDrawingLayer)GetValue(DrawingLayer2DProperty); }
            set { SetValue(DrawingLayer2DProperty, value); }
        }

        /// <summary>
        /// Gets or sets the camera.
        /// </summary>
        public Camera3DBase Camera
        {
            get { return (Camera3DBase)GetValue(CameraProperty); }
            set { SetValue(CameraProperty, value); }
        }

        /// <summary>
        /// Gets the RenderLoop that is currently in use.
        /// </summary>
        public RenderLoop RenderLoop
        {
            get { return m_painter.RenderLoop; }
        }

        /// <summary>
        /// Does the target control have focus?
        /// </summary>
        public bool Focused
        {
            get { return ((IInputEnabledView)m_painter).Focused; }
        }

        public EngineDevice SelectedDevice
        {
            get => m_painter.RenderLoop.Device;
            set => m_painter.RenderLoop.SetRenderingDevice(value);
        }

        public Size CurrentViewSize
        {
            get
            {
                var currentViewSize = m_painter.RenderLoop.CurrentViewSize;
                Size result = new Size();
                result.Width = currentViewSize.Width;
                result.Height = currentViewSize.Height;
                return result;
            }
        }

        public IEnumerable<EngineDevice> PossibleDevices
        {
            get => GraphicsCore.Current.Devices;
        }
    }
}
