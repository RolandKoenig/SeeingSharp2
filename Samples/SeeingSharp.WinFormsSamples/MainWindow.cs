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
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace SeeingSharp.WinFormsSamples
{
    public partial class MainWindow : Form
    {
        private SampleBase _actSample;
        private SampleSettings _actSampleSettings;
        private SampleMetadata _actSampleInfo;
        private bool _isChangingSample;
        private List<ToolStripItem> _sampleCommandToolbarItems;

        private List<ChildRenderWindow> _childWindows;

        public MainWindow()
        {
            this.InitializeComponent();

            _sampleCommandToolbarItems = new List<ToolStripItem>();
            _childWindows = new List<ChildRenderWindow>();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.DesignMode) { return; }
            if (!GraphicsCore.IsLoaded) { return; }

            this.Text = $@"{this.Text} ({Assembly.GetExecutingAssembly().GetName().Version})";

            // Register viewbox filter
            _ctrlRenderPanel.RenderLoop.Filters.Add(new SceneViewboxObjectFilter());

            _ctrlRenderPanel.RenderLoop.PrepareRender += this.OnRenderLoop_PrepareRender;

            // AddObject all sample pages
            var sampleRepo = new SampleRepository();
            sampleRepo.LoadSampleData();
            TabPage firstTabPage = null;
            ListView firstListView = null;
            ListViewItem firstListViewItem = null;
            foreach (var actSampleGroup in sampleRepo.SampleGroups)
            {
                var actTabPage = new TabPage(actSampleGroup.GroupName);
                _tabControlSamples.TabPages.Add(actTabPage);

                if (firstTabPage == null)
                {
                    firstTabPage = actTabPage;
                }

                var actListView = new ListView
                {
                    Dock = DockStyle.Fill,
                    Activation = ItemActivation.OneClick
                };

                actListView.ItemSelectionChanged += this.OnListView_ItemSelectionChanged;
                actListView.MultiSelect = false;
                actListView.LargeImageList = _images;
                actListView.SmallImageList = _images;
                actTabPage.Controls.Add(actListView);

                if (firstListView == null)
                {
                    firstListView = actListView;
                }

                foreach (var actSample in actSampleGroup.Samples)
                {
                    // Generate the new list entry for the current sample
                    var newListItem = new ListViewItem
                    {
                        Text = actSample.Name,
                        Tag = actSample
                    };

                    actListView.Items.Add(newListItem);

                    if (firstListViewItem == null)
                    {
                        firstListViewItem = newListItem;
                    }

                    // Load the item's image
                    var sampleImageLink = actSample.TryGetSampleImageLink();

                    if (sampleImageLink != null)
                    {
                        using (var inStream = sampleImageLink.OpenRead())
                        {
                            _images.Images.Add(Image.FromStream(inStream));
                            newListItem.ImageIndex = _images.Images.Count - 1;
                        }
                    }
                }
            }

            if (firstTabPage != null)
            {
                _tabControlSamples.SelectedTab = firstTabPage;
            }

            if (firstListViewItem != null)
            {
                firstListView.SelectedIndices.Add(0);
            }
        }

        /// <summary>
        /// Applies the given sample.
        /// </summary>
        private async void ApplySample(SampleMetadata sampleInfo, SampleSettings sampleSettings)
        {
            if (_isChangingSample) { return; }

            _isChangingSample = true;
            try
            {
                if (_actSampleInfo == sampleInfo) { return; }

                // Clear previous sample
                if (_actSampleInfo != null)
                {
                    await _ctrlRenderPanel.RenderLoop.Scene.ManipulateSceneAsync(manipulator =>
                    {
                        manipulator.Clear(true);
                    });
                    await _ctrlRenderPanel.RenderLoop.Clear2DDrawingLayersAsync();

                    foreach (var actChildWindow in _childWindows)
                    {
                        await actChildWindow.ClearAsync();
                    }

                    _actSample.OnClosed();
                }
                if (_actSampleSettings != null)
                {
                    _actSampleSettings.RecreateRequest -= this.OnSampleSettings_RecreateRequest;
                }
                if (this.IsDisposed || !this.IsHandleCreated) { return; }

                // Reset members
                _actSample = null;
                _actSampleInfo = null;

                // Apply new sample
                if (sampleInfo != null)
                {
                    var sampleObject = sampleInfo.CreateSampleObject();
                    await sampleObject.OnStartupAsync(_ctrlRenderPanel.RenderLoop, sampleSettings);
                    await sampleObject.OnInitRenderingWindowAsync(_ctrlRenderPanel.RenderLoop);
                    await sampleObject.OnReloadAsync(_ctrlRenderPanel.RenderLoop, sampleSettings);

                    foreach (var actChildWindow in _childWindows)
                    {
                        await actChildWindow.SetRenderingDataAsync(sampleObject);
                    }

                    _actSample = sampleObject;
                    _actSampleSettings = sampleSettings;
                    _actSampleInfo = sampleInfo;

                    if (_actSampleSettings != null)
                    {
                        _actSampleSettings.RecreateRequest += this.OnSampleSettings_RecreateRequest;
                    }

                    _propertyGrid.SelectedObject = sampleSettings;
                    this.UpdateSampleCommands(sampleSettings);

                    await _ctrlRenderPanel.RenderLoop.Register2DDrawingLayerAsync(
                        new PerformanceMeasureDrawingLayer(0f, _ctrlRenderPanel.ViewInformation));
                }
                else
                {
                    _propertyGrid.SelectedObject = null;
                    this.UpdateSampleCommands(null);
                }
                if (this.IsDisposed || !this.IsHandleCreated) { return; }

                // Wait for next finished rendering
                await _ctrlRenderPanel.RenderLoop.WaitForNextFinishedRenderAsync();
                if (this.IsDisposed || !this.IsHandleCreated) { return; }

                await _ctrlRenderPanel.RenderLoop.WaitForNextFinishedRenderAsync();
                if (this.IsDisposed || !this.IsHandleCreated) { return; }
            }
            finally
            {
                _isChangingSample = false;
            }
        }

        private void UpdateSampleCommands(SampleSettings settings)
        {
            foreach (var actLastItem in _sampleCommandToolbarItems)
            {
                _barTools.Items.Remove(actLastItem);
                actLastItem.Dispose();
            }
            _sampleCommandToolbarItems.Clear();

            if (settings == null) { return; }
            var isFirst = true;
            foreach (var actCommand in settings.GetCommands())
            {
                var actCommandInner = actCommand;

                if (isFirst)
                {
                    var separator = new ToolStripSeparator();
                    _barTools.Items.Add(separator);
                    _sampleCommandToolbarItems.Add(separator);
                    isFirst = false;
                }

                var actNewToolbarItem = _barTools.Items.Add(actCommandInner.CommandText);
                actNewToolbarItem.Tag = actCommand;
                actNewToolbarItem.Click += (sender, eArgs) =>
                {
                    if (actCommandInner.CanExecute(null)) { actCommandInner.Execute(null); }
                };
                _sampleCommandToolbarItems.Add(actNewToolbarItem);
            }
        }

        private void OnListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
            {
                e.Item.BackColor = System.Drawing.Color.Transparent;
                return;
            }
            e.Item.BackColor = System.Drawing.Color.LightBlue;

            var actListView = sender as ListView;
            actListView.EnsureNotNull(nameof(actListView));
            e.Item.EnsureNotNull($"{nameof(e)}.{nameof(e.Item)}");

            var sampleInfo = e.Item.Tag as SampleMetadata;
            sampleInfo.EnsureNotNull(nameof(sampleInfo));

            var sampleSettings = sampleInfo.CreateSampleSettingsObject();
            sampleSettings.ThrottleRecreateRequest = false;
            sampleSettings.SetEnvironment(_ctrlRenderPanel.RenderLoop, sampleInfo);

            // Now apply the sample
            this.ApplySample(sampleInfo, sampleSettings);
        }

        private void OnRefreshTimer_Tick(object sender, EventArgs e)
        {
            _renderWindowControlsComponent.UpdateTargetControlStates();
        }

        private async void OnCmdCopyScreenshot_Click(object sender, EventArgs e)
        {
            var bitmap = await _ctrlRenderPanel.RenderLoop.GetScreenshotGdiAsync();
            Clipboard.SetImage(bitmap);
        }

        private void OnRenderLoop_PrepareRender(object sender, EventArgs e)
        {
            var actSample = _actSample;
            actSample?.Update();
        }

        private async void OnSampleSettings_RecreateRequest(object sender, EventArgs e)
        {
            var sample = _actSample;
            var sampleSettings = _actSampleSettings;
            if (sample == null)
            {
                return;
            }
            if (sampleSettings == null)
            {
                return;
            }

            await sample.OnReloadAsync(_ctrlRenderPanel.RenderLoop, sampleSettings);
        }

        private async void OnCmdNewChildWindow_Click(object sender, EventArgs e)
        {
            var childWindow = new ChildRenderWindow();
            childWindow.Icon = this.Icon;
            childWindow.InitializeChildWindow(_ctrlRenderPanel.Scene, _ctrlRenderPanel.Camera.GetViewPoint());

            _childWindows.Add(childWindow);
            childWindow.Closed += (_1, _2) => { _childWindows.Remove(childWindow); };

            childWindow.Show(this);

            await childWindow.SetRenderingDataAsync(_actSample);
        }
    }
}
