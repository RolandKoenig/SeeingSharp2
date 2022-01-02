using System;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SeeingSharp.Core;
using SeeingSharp.Views;

namespace SeeingSharp.WinUIDesktopSamples.Controls
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows10.0.19041.0")]
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
