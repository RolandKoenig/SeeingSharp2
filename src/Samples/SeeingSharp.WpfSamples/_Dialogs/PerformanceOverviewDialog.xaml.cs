using System;
using System.Windows;
using System.Windows.Threading;
using SeeingSharp.Core;

namespace SeeingSharp.WpfSamples
{
    /// <summary>
    /// Interaction logic for PerformanceOverviewDialog.xaml
    /// </summary>
    public partial class PerformanceOverviewDialog : Window
    {
        private DispatcherTimer _refreshTimer;

        public PerformanceOverviewDialog()
        {
            this.InitializeComponent();

            if (GraphicsCore.IsLoaded)
            {
                this.Loaded += this.OnLoaded;
                this.Unloaded += this.OnUnloaded;
            }
        }

        private void OnRefreshRefreshTimer_Tick(object sender, EventArgs e)
        {
            if (this.DataContext is PerformanceOverviewViewModel viewModel)
            {
                viewModel.TriggerRefresh();
            }

            CtrlDataGrid.Items.Refresh();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_refreshTimer != null) { return; }

            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromMilliseconds(500.0);
            _refreshTimer.Tick += this.OnRefreshRefreshTimer_Tick;
            _refreshTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if(_refreshTimer == null){ return; }

            _refreshTimer.Stop();
            _refreshTimer = null;
        }
    }
}
