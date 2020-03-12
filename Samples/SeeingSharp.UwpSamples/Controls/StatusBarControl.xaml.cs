using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Views;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SeeingSharp.UwpSamples.Controls
{
    public sealed partial class StatusBarControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty CtrlRendererProperty =
            DependencyProperty.Register(nameof(CtrlRenderer), typeof(SeeingSharpRenderPanel), typeof(StatusBarControl), new PropertyMetadata(null));

        private DispatcherTimer _refreshTimer;

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
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CountObjects)));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CountResources)));
        }

        public SeeingSharpRenderPanel CtrlRenderer
        {
            get => (SeeingSharpRenderPanel)this.GetValue(CtrlRendererProperty);
            set => this.SetValue(CtrlRendererProperty, value);
        }

        public int CountObjects => this.CtrlRenderer?.RenderLoop.VisibleObjectCount ?? 0;

        public int CountResources => this.CtrlRenderer?.RenderLoop.ResourceCount ?? 0;
    }
}
