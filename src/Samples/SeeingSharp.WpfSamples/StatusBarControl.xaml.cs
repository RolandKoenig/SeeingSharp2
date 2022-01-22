using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SeeingSharp.Core;
using SeeingSharp.Views;

namespace SeeingSharp.WpfSamples
{
    /// <summary>
    /// Interaction logic for StatusBarControl.xaml
    /// </summary>
    public partial class StatusBarControl : UserControl
    {
        public static readonly DependencyProperty CtrlRendererProperty =
            DependencyProperty.Register(nameof(CtrlRenderer), typeof(SeeingSharpRendererElement), typeof(StatusBarControl), new PropertyMetadata(null));

        private DispatcherTimer? _refreshTimer;

        public SeeingSharpRendererElement? CtrlRenderer
        {
            get => (SeeingSharpRendererElement)this.GetValue(CtrlRendererProperty);
            set => this.SetValue(CtrlRendererProperty, value);
        }

        public StatusBarControl()
        {
            this.InitializeComponent();

            if (GraphicsCore.IsLoaded)
            {
                this.Loaded += this.OnLoaded;
                this.Unloaded += this.OnUnloaded;
            }
        }

        private void OnRefreshTimer_Tick(object? sender, EventArgs e)
        {
            TxtResourceCount.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
            TxtVisibleObjectCount.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
            TxtCountDrawCalls.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Tick += this.OnRefreshTimer_Tick;
            _refreshTimer.Interval = TimeSpan.FromMilliseconds(500.0);
            _refreshTimer.Start();
        }

        private void OnUnloaded(object? sender, RoutedEventArgs e)
        {
            _refreshTimer?.Stop();
            _refreshTimer = null;
        }
    }
}
