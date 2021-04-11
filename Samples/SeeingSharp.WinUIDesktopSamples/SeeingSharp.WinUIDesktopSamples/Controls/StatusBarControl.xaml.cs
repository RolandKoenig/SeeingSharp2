/*
    Seeing# and all applications distributed together with it. 
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
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Views;

namespace SeeingSharp.WinUIDesktopSamples.Controls
{
    public sealed partial class StatusBarControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty CtrlRendererProperty =
            DependencyProperty.Register(nameof(CtrlRenderer), typeof(SeeingSharpRenderPanel), typeof(StatusBarControl), new PropertyMetadata(null));

        private DispatcherTimer _refreshTimer;

        public SeeingSharpRenderPanel CtrlRenderer
        {
            get => (SeeingSharpRenderPanel)this.GetValue(CtrlRendererProperty);
            set => this.SetValue(CtrlRendererProperty, value);
        }

        public int CountResources => this.CtrlRenderer?.RenderLoop.CountGraphicsResources ?? 0;

        public int CountObjects => this.CtrlRenderer?.RenderLoop.CountVisibleObjects ?? 0;

        public int CountDrawCalls => this.CtrlRenderer?.RenderLoop.CountDrawCalls ?? 0;

        public event PropertyChangedEventHandler PropertyChanged;

        public StatusBarControl()
        {
            this.InitializeComponent();

            if (!GraphicsCore.IsLoaded) { return; }

            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if(_refreshTimer != null){ return; }

            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromMilliseconds(500.0);
            _refreshTimer.Tick += this.OnRefreshTimer_Tick;
            _refreshTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if(_refreshTimer == null){ return; }

            _refreshTimer.Stop();
            _refreshTimer = null;
        }

        private void OnRefreshTimer_Tick(object sender, object e)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CountResources)));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CountObjects)));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CountDrawCalls)));
        }
    }
}
