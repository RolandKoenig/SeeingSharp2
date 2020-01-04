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
        private SampleBase m_actSample;
        private SampleSettings m_actSampleSettings;
        private SampleMetadata m_actSampleInfo;
        private bool m_isChangingSample;
        private List<ToolStripItem> m_sampleCommandToolbarItems;

        private List<ChildRenderWindow> m_childWindows;

        public MainWindow()
        {
            this.InitializeComponent();

            m_sampleCommandToolbarItems = new List<ToolStripItem>();
            m_childWindows = new List<ChildRenderWindow>();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.DesignMode) { return; }
            if (!GraphicsCore.IsLoaded) { return; }

            this.Text = $@"{this.Text} ({Assembly.GetExecutingAssembly().GetName().Version})";

            m_ctrlRenderPanel.RenderLoop.PrepareRender += this.OnRenderLoop_PrepareRender;

            // AddObject all sample pages
            var sampleRepo = new SampleRepository();
            sampleRepo.LoadSampleData();
            TabPage firstTabPage = null;
            ListView firstListView = null;
            ListViewItem firstListViewItem = null;
            var generatedTabs = new Dictionary<string, ListView>();

            foreach (var actSampleGroup in sampleRepo.SampleGroups)
            {
                var actTabPage = new TabPage(actSampleGroup.GroupName);
                m_tabControlSamples.TabPages.Add(actTabPage);

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
                actListView.LargeImageList = m_images;
                actListView.SmallImageList = m_images;
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
                            m_images.Images.Add(Image.FromStream(inStream));
                            newListItem.ImageIndex = m_images.Images.Count - 1;
                        }
                    }
                }
            }

            if (firstTabPage != null)
            {
                m_tabControlSamples.SelectedTab = firstTabPage;
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
            if (m_isChangingSample) { return; }

            m_isChangingSample = true;
            try
            {
                if (m_actSampleInfo == sampleInfo) { return; }

                // Clear previous sample
                if (m_actSampleInfo != null)
                {
                    await m_ctrlRenderPanel.RenderLoop.Scene.ManipulateSceneAsync(manipulator =>
                    {
                        manipulator.Clear(true);
                    });
                    await m_ctrlRenderPanel.RenderLoop.Clear2DDrawingLayersAsync();

                    foreach (var actChildWindow in m_childWindows)
                    {
                        await actChildWindow.ClearAsync();
                    }

                    m_actSample.OnClosed();
                }
                if (m_actSampleSettings != null)
                {
                    m_actSampleSettings.RecreateRequest -= this.OnSampleSettings_RecreateRequest;
                }
                if (this.IsDisposed || !this.IsHandleCreated) { return; }

                // Reset members
                m_actSample = null;
                m_actSampleInfo = null;

                // Apply new sample
                if (sampleInfo != null)
                {
                    var sampleObject = sampleInfo.CreateSampleObject();
                    await sampleObject.OnStartupAsync(m_ctrlRenderPanel.RenderLoop, sampleSettings);
                    await sampleObject.OnInitRenderingWindowAsync(m_ctrlRenderPanel.RenderLoop);
                    await sampleObject.OnReloadAsync(m_ctrlRenderPanel.RenderLoop, sampleSettings);

                    foreach (var actChildWindow in m_childWindows)
                    {
                        await actChildWindow.SetRenderingDataAsync(sampleObject);
                    }

                    m_actSample = sampleObject;
                    m_actSampleSettings = sampleSettings;
                    m_actSampleInfo = sampleInfo;

                    if (m_actSampleSettings != null)
                    {
                        m_actSampleSettings.RecreateRequest += this.OnSampleSettings_RecreateRequest;
                    }

                    m_propertyGrid.SelectedObject = sampleSettings;
                    this.UpdateSampleCommands(sampleSettings);

                    await m_ctrlRenderPanel.RenderLoop.Register2DDrawingLayerAsync(
                        new PerformanceMeasureDrawingLayer(GraphicsCore.Current.PerformanceAnalyzer, 0f));
                }
                else
                {
                    m_propertyGrid.SelectedObject = null;
                    this.UpdateSampleCommands(null);
                }
                if (this.IsDisposed || !this.IsHandleCreated) { return; }

                // Wait for next finished rendering
                await m_ctrlRenderPanel.RenderLoop.WaitForNextFinishedRenderAsync();
                if (this.IsDisposed || !this.IsHandleCreated) { return; }

                await m_ctrlRenderPanel.RenderLoop.WaitForNextFinishedRenderAsync();
                if (this.IsDisposed || !this.IsHandleCreated) { return; }
            }
            finally
            {
                m_isChangingSample = false;
            }
        }

        private void UpdateSampleCommands(SampleSettings settings)
        {
            foreach (var actLastItem in m_sampleCommandToolbarItems)
            {
                m_barTools.Items.Remove(actLastItem);
                actLastItem.Dispose();
            }
            m_sampleCommandToolbarItems.Clear();

            if (settings == null) { return; }
            var isFirst = true;
            foreach (var actCommand in settings.GetCommands())
            {
                var actCommandInner = actCommand;

                if (isFirst)
                {
                    var separator = new ToolStripSeparator();
                    m_barTools.Items.Add(separator);
                    m_sampleCommandToolbarItems.Add(separator);
                    isFirst = false;
                }

                var actNewToolbarItem = m_barTools.Items.Add(actCommandInner.CommandText);
                actNewToolbarItem.Tag = actCommand;
                actNewToolbarItem.Click += (sender, eArgs) =>
                {
                    if (actCommandInner.CanExecute(null)) { actCommandInner.Execute(null); }
                };
                m_sampleCommandToolbarItems.Add(actNewToolbarItem);
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
            sampleSettings.SetEnvironment(m_ctrlRenderPanel.RenderLoop, sampleInfo);

            // Now apply the sample
            this.ApplySample(sampleInfo, sampleSettings);
        }

        private void OnRefreshTimer_Tick(object sender, EventArgs e)
        {
            m_renderWindowControlsComponent.UpdateTargetControlStates();
        }

        private async void OnCmdCopyScreenshot_Click(object sender, EventArgs e)
        {
            var bitmap = await m_ctrlRenderPanel.RenderLoop.GetScreenshotGdiAsync();
            Clipboard.SetImage(bitmap);
        }

        private void OnRenderLoop_PrepareRender(object sender, EventArgs e)
        {
            var actSample = m_actSample;
            actSample?.Update();
        }

        private async void OnSampleSettings_RecreateRequest(object sender, EventArgs e)
        {
            var sample = m_actSample;
            var sampleSettings = m_actSampleSettings;
            if (sample == null)
            {
                return;
            }
            if (sampleSettings == null)
            {
                return;
            }

            await sample.OnReloadAsync(m_ctrlRenderPanel.RenderLoop, sampleSettings);
        }

        private async void OnCmdNewChildWindow_Click(object sender, EventArgs e)
        {
            var childWindow = new ChildRenderWindow();
            childWindow.Icon = this.Icon;
            childWindow.InitializeChildWindow(m_ctrlRenderPanel.Scene, m_ctrlRenderPanel.Camera.GetViewPoint());

            m_childWindows.Add(childWindow);
            childWindow.Closed += (_1, _2) => { m_childWindows.Remove(childWindow); };

            childWindow.Show(this);

            await childWindow.SetRenderingDataAsync(m_actSample);
        }
    }
}
