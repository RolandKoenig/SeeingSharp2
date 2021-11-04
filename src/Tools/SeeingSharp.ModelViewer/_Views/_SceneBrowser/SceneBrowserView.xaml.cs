using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SeeingSharp.Core;

namespace SeeingSharp.ModelViewer
{
    /// <summary>
    /// Interaction logic for SceneBrowserView.xaml
    /// </summary>
    public partial class SceneBrowserView : UserControl
    {
        private DispatcherTimer? _refreshTimer;

        public SceneBrowserView()
        {
            this.InitializeComponent();

            if (GraphicsCore.IsLoaded)
            {
                this.Loaded += OnLoaded;
                this.Unloaded += OnUnloaded;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_refreshTimer != null)
            {
                _refreshTimer.Stop();
                _refreshTimer = null;
            }

            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromMilliseconds(500.0);
            _refreshTimer.Tick += OnRefreshTimer_Tick;
            _refreshTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_refreshTimer != null)
            {
                _refreshTimer.Stop();
                _refreshTimer = null;
            }
        }

        private void OnRefreshTimer_Tick(object? sender, EventArgs e)
        {
            if(_refreshTimer == null){ return; }

            if (this.DataContext is SceneBrowserViewModel viewModel)
            {
                viewModel.RefreshData();
            }
        }

        private void OnTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!(this.DataContext is SceneBrowserViewModel viewModel)) { return; }

            viewModel.SelectedObject = e.NewValue as SceneObjectInfo;
        }
    }
}
