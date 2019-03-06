﻿#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion

namespace SeeingSharp.Multimedia.Views
{
    #region using

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Windows.Foundation;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Core;
    using Drawing2D;
    using Drawing3D;
    using Input;

    #endregion

    // [ContentProperty(Name = "SceneComponents")]
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
            if (!GraphicsCore.IsLoaded) { return; }
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) { return; }
            if (!(sender is SeeingSharpRenderPanel renderPanel)) { return; }

            if (e.Property == SceneProperty) { renderPanel.RenderLoop.SetScene(e.NewValue as Scene); }
            else if (e.Property == CameraProperty) { renderPanel.Camera = e.NewValue as Camera3DBase; }
            else if (e.Property == DrawingLayer2DProperty)
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
            get => m_painter.DiscardRendering;
            set => m_painter.DiscardRendering = value;
        }

        /// <summary>
        /// Gets or sets the currently applied scene.
        /// </summary>
        public Scene Scene
        {
            get => (Scene)GetValue(SceneProperty);
            set => SetValue(SceneProperty, value);
        }

        /// <summary>
        /// Gets or sets the custom layer for 2D rendering.
        /// </summary>
        public Custom2DDrawingLayer DrawingLayer2D
        {
            get => (Custom2DDrawingLayer)GetValue(DrawingLayer2DProperty);
            set => SetValue(DrawingLayer2DProperty, value);
        }

        /// <summary>
        /// Gets or sets the camera.
        /// </summary>
        public Camera3DBase Camera
        {
            get => (Camera3DBase)GetValue(CameraProperty);
            set => SetValue(CameraProperty, value);
        }

        /// <summary>
        /// Gets the RenderLoop that is currently in use.
        /// </summary>
        public RenderLoop RenderLoop => m_painter.RenderLoop;

        /// <summary>
        /// Does the target control have focus?
        /// </summary>
        public bool Focused => ((IInputEnabledView)m_painter).Focused;

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
    }
}