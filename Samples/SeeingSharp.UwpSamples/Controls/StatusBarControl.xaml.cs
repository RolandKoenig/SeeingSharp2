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

        private DispatcherTimer m_refreshTimer;

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
            if(m_refreshTimer != null){ return; }

            m_refreshTimer = new DispatcherTimer();
            m_refreshTimer.Interval = TimeSpan.FromMilliseconds(500.0);
            m_refreshTimer.Tick += this.OnRefreshTimer_Tick;
            m_refreshTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if(m_refreshTimer == null){ return; }

            m_refreshTimer.Stop();
            m_refreshTimer = null;
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
