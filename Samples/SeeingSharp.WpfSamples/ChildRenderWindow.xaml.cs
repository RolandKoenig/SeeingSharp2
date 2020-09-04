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

using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;

namespace SeeingSharp.WpfSamples
{
    /// <summary>
    /// Interaction logic for ChildRenderWindow.xaml
    /// </summary>
    public partial class ChildRenderWindow : Window
    {
        public bool DiscardPresent
        {
            get => this.CtrlRenderer.DiscardPresent;
            set => this.CtrlRenderer.DiscardPresent = value;
        }

        public ChildRenderWindow()
        {
            this.InitializeComponent();

            this.Loaded += this.OnLoaded;
        }

        public void InitializeChildWindow(Scene scene, Camera3DViewPoint viewPoint)
        {
            CtrlRenderer.Scene = scene;
            CtrlRenderer.Camera.ApplyViewPoint(viewPoint);
        }

        public async Task SetRenderingDataAsync(SampleBase actSample)
        {
            await actSample.OnInitRenderingWindowAsync(CtrlRenderer.RenderLoop);

            await CtrlRenderer.RenderLoop.Register2DDrawingLayerAsync(
                new PerformanceMeasureDrawingLayer(10f, CtrlRenderer.ViewInformation));
        }

        public async Task ClearAsync()
        {
            await CtrlRenderer.RenderLoop.Clear2DDrawingLayersAsync();
            CtrlRenderer.RenderLoop.ObjectFilters.Clear();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.Title = $@"{this.Title} ({Assembly.GetExecutingAssembly().GetName().Version})";
        }
    }
}
