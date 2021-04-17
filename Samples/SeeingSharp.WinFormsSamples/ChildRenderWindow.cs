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
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;

namespace SeeingSharp.WinFormsSamples
{
    public partial class ChildRenderWindow : Form
    {
        public bool DiscardPresent
        {
            get => _ctrlRenderer.DiscardPresent;
            set => _ctrlRenderer.DiscardPresent = value;
        }

        public ChildRenderWindow()
        {
            this.InitializeComponent();
        }

        public void InitializeChildWindow(Scene scene, Camera3DViewPoint viewPoint)
        {
            _ctrlRenderer.Scene = scene;
            _ctrlRenderer.Camera.ApplyViewPoint(viewPoint);
        }

        public async Task SetRenderingDataAsync(SampleBase actSample)
        {
            await actSample.OnInitRenderingWindowAsync(_ctrlRenderer.RenderLoop);

            await _ctrlRenderer.RenderLoop.Register2DDrawingLayerAsync(
                new PerformanceMeasureDrawingLayer(10f, _ctrlRenderer.ViewInformation));
        }

        public async Task ClearAsync()
        {
            await _ctrlRenderer.RenderLoop.Clear2DDrawingLayersAsync();
            _ctrlRenderer.RenderLoop.ObjectFilters.Clear();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.DesignMode)
            {
                return;
            }

            this.Text = $@"{this.Text} ({Assembly.GetExecutingAssembly().GetName().Version})";
        }

        private void OnRefreshTimer_Tick(object sender, EventArgs e)
        {
            _renderWindowControlsComponent.UpdateTargetControlStates();
        }
    }
}
