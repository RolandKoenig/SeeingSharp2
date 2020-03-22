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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Views;

namespace SeeingSharp.WpfSamples
{
    /// <summary>
    /// Interaction logic for StatusBarControl.xaml
    /// </summary>
    public partial class StatusBarControl : UserControl
    {
        public static readonly DependencyProperty CtrlRendererProperty =
            DependencyProperty.Register(nameof(CtrlRenderer), typeof(SeeingSharpRendererElement), typeof(StatusBarControl), new PropertyMetadata(null));

        private DispatcherTimer _refreshTimer;

        public StatusBarControl()
        {
            this.InitializeComponent();

            if (GraphicsCore.IsLoaded)
            {
                this.Loaded += this.OnLoaded;
                this.Unloaded += this.OnUnloaded;
            }
        }

        private void OnRefreshTimer_Tick(object sender, EventArgs e)
        {
            this.TxtResourceCount.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
            this.TxtVisibleObjectCount.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
            this.TxtCountDrawCalls.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Tick += OnRefreshTimer_Tick;
            _refreshTimer.Interval = TimeSpan.FromMilliseconds(500.0);
            _refreshTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _refreshTimer.Stop();
        }

        public SeeingSharpRendererElement CtrlRenderer
        {
            get => (SeeingSharpRendererElement)this.GetValue(CtrlRendererProperty);
            set => this.SetValue(CtrlRendererProperty, value);
        }
    }
}
