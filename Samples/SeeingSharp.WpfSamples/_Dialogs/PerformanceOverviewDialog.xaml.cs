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
using System.Windows;
using System.Windows.Threading;
using SeeingSharp.Multimedia.Core;

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
