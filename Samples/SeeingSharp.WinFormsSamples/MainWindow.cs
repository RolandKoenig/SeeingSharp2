﻿#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion
using SeeingSharp.SampleContainer;
using SeeingSharp.Checking;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SeeingSharp.Multimedia.Core;
using SharpDX;

namespace SeeingSharp.WinFormsSamples
{
    public partial class MainWindow : Form
    {
        private bool m_isChangingSample;
        private SampleBase m_actSample;
        private SampleMetadata m_actSampleInfo;
        private List<ListView> m_generatedListViews;
        private List<ToolStripItem> m_sampleCommandToolbarItems;

        public MainWindow()
        {
            InitializeComponent();

            m_generatedListViews = new List<ListView>();
            m_sampleCommandToolbarItems = new List<ToolStripItem>();

            UpdateWindowState();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            foreach (var actDevice in GraphicsCore.Current.Devices)
            {
                var newButton = new ToolStripButton($"to {actDevice.AdapterDescription}");
                newButton.Tag = actDevice;
                newButton.Click += OnCmdChangeDevice_Click;
                m_mnuChangeDevice.DropDownItems.Add(newButton);
            }

            // Add all sample pages
            SampleRepository sampleRepo = new SampleRepository();
            sampleRepo.LoadSampleData();
            TabPage firstTabPage = null;
            ListView firstListView = null;
            ListViewItem firstListViewItem = null;
            Dictionary<string, ListView> generatedTabs = new Dictionary<string, ListView>();
            foreach (var actSampleGroup in sampleRepo.SampleGroups)
            {
                TabPage actTabPage = new TabPage(actSampleGroup.GroupName);
                m_tabControlSamples.TabPages.Add(actTabPage);
                if(firstTabPage == null) { firstTabPage = actTabPage; }

                ListView actListView = new ListView();
                actListView.Dock = DockStyle.Fill;
                actListView.Activation = ItemActivation.OneClick;
                actListView.ItemSelectionChanged += OnListView_ItemSelectionChanged;
                actListView.MultiSelect = false;
                actListView.LargeImageList = m_images;
                actListView.SmallImageList = m_images;
                actTabPage.Controls.Add(actListView);
                if(firstListView == null) { firstListView = actListView; }

                foreach (var actSample in actSampleGroup.Samples)
                {
                    // Generate the new list entry for the current sample
                    ListViewItem newListItem = new ListViewItem();
                    newListItem.Text = actSample.Name;
                    newListItem.Tag = actSample;
                    actListView.Items.Add(newListItem);
                    if(firstListViewItem == null) { firstListViewItem = newListItem; }

                    // Load the item's image
                    var sampleImageLink = actSample.TryGetSampleImageLink();
                    if (sampleImageLink != null)
                    {
                        using (Stream inStream = sampleImageLink.OpenRead())
                        {
                            m_images.Images.Add(Image.FromStream(inStream));
                            newListItem.ImageIndex = m_images.Images.Count - 1;
                        }
                    }
                }
            }

            if(firstTabPage != null) { m_tabControlSamples.SelectedTab = firstTabPage; }
            if(firstListViewItem != null) { firstListView.SelectedIndices.Add(0); }

            UpdateWindowState();
        }

        /// <summary>
        /// Changes the render resolution to given width and height.
        /// </summary>
        /// <param name="width">The width in pixels.</param>
        /// <param name="height">The height in pixels.</param>
        private void ChangeRenderResolution(int width, int height)
        {
            var renderControl = m_ctrlRenderPanel;
            if (renderControl == null) { return; }

            Size2 currentViewSize = renderControl.RenderLoop.CurrentViewSize;
            Size2 currentWindowSize = new Size2(this.Width, this.Height);
            Size2 difference = new Size2(
                currentWindowSize.Width - currentViewSize.Width,
                currentWindowSize.Height - currentViewSize.Height);
            Size2 newWindowSize = new Size2(width + difference.Width, height + difference.Height);

            this.WindowState = FormWindowState.Normal;
            this.Width = newWindowSize.Width;
            this.Height = newWindowSize.Height;
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
                this.UpdateWindowState();

                if (m_actSampleInfo == sampleInfo) { return; }

                // Clear previous sample
                if (m_actSampleInfo != null)
                {
                    await m_ctrlRenderPanel.RenderLoop.Scene.ManipulateSceneAsync((manipulator) =>
                    {
                        manipulator.Clear(true);
                    });
                    await m_ctrlRenderPanel.RenderLoop.Clear2DDrawingLayersAsync();
                    m_actSample.NotifyClosed();
                }
                if (this.IsDisposed || (!this.IsHandleCreated)) { return; }

                // Reset members
                m_actSample = null;
                m_actSampleInfo = null;

                // Apply new sample
                if (sampleInfo != null)
                {
                    SampleBase sampleObject = sampleInfo.CreateSampleObject();
                    await sampleObject.OnStartupAsync(m_ctrlRenderPanel.RenderLoop, sampleSettings);

                    m_actSample = sampleObject;
                    m_actSampleInfo = sampleInfo;

                    m_propertyGrid.SelectedObject = sampleSettings;
                    UpdateSampleCommands(sampleSettings);
                }
                else
                {
                    m_propertyGrid.SelectedObject = null;
                    UpdateSampleCommands(null);
                }
                if (this.IsDisposed || (!this.IsHandleCreated)) { return; }

                // Wait for next finished rendering
                await m_ctrlRenderPanel.RenderLoop.WaitForNextFinishedRenderAsync();
                if (this.IsDisposed || (!this.IsHandleCreated)) { return; }

                await m_ctrlRenderPanel.RenderLoop.WaitForNextFinishedRenderAsync();
                if (this.IsDisposed || (!this.IsHandleCreated)) { return; }

                //// Apply new camera on child windows
                //foreach (ChildRenderWindow actChildWindow in m_openedChildRenderers)
                //{
                //    actChildWindow.ApplyViewpoint(m_ctrlRenderer.Camera.GetViewPoint());
                //}
            }
            finally
            {
                m_isChangingSample = false;
            }

            this.UpdateWindowState();
        }

        private void UpdateSampleCommands(SampleSettings settings)
        {
            foreach(var actLastItem in m_sampleCommandToolbarItems)
            {
                m_barTools.Items.Remove(actLastItem);
                actLastItem.Dispose();
            }
            m_sampleCommandToolbarItems.Clear();

            if(settings == null) { return; }
            bool isFirst = true;
            foreach(var actCommand in settings.GetCommands())
            {
                var actCommandInner = actCommand;

                if(isFirst)
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

        private void UpdateWindowState()
        {
            var viewSize = m_ctrlRenderPanel.RenderLoop.ViewInformation.CurrentViewSize;
            m_lblResolution.Text = $"{viewSize.Width}x{viewSize.Height}";
            m_lblObjectCount.Text = m_ctrlRenderPanel.RenderLoop.Scene.CountObjects.ToString();
            m_lblDevice.Text = m_ctrlRenderPanel.RenderLoop.Device?.AdapterDescription ?? "-";
        }

        private void OnListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
            {
                e.Item.BackColor = System.Drawing.Color.Transparent;
                return;
            }
            else
            {
                e.Item.BackColor = System.Drawing.Color.LightBlue;
            }

            ListView actListView = sender as ListView;
            actListView.EnsureNotNull(nameof(actListView));
            e.Item.EnsureNotNull($"{nameof(e)}.{nameof(e.Item)}");

            SampleMetadata sampleInfo = e.Item.Tag as SampleMetadata;
            sampleInfo.EnsureNotNull(nameof(sampleInfo));

            // Clear selection on other ListViews
            foreach (ListView actOtherListView in m_generatedListViews)
            {
                if (actOtherListView == actListView) { continue; }
                actOtherListView.SelectedItems.Clear();
            }

            var sampleSettings = sampleInfo.CreateSampleSettingsObject();
            sampleSettings.SetEnvironment(m_ctrlRenderPanel.RenderLoop, sampleInfo);

            // Now apply the sample
            this.ApplySample(sampleInfo, sampleSettings);
        }

        private void OnRefreshTimer_Tick(object sender, EventArgs e)
        {
            UpdateWindowState();
        }

        private void OnCmdChangeResolution_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem?.Tag == null) { return; }

            var splittedResolution = menuItem.Tag.ToString().Split('x');
            if (splittedResolution.Length != 2) { return; }
            if (!int.TryParse(splittedResolution[0], out int width)) { return; }
            if (!int.TryParse(splittedResolution[1], out int height)) { return; }

            ChangeRenderResolution(width, height);
        }

        private async void OnCmdCopyScreenshot_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = await m_ctrlRenderPanel.RenderLoop.GetScreenshotGdiAsync();
            Clipboard.SetImage(bitmap);
        }

        private void OnCmdChangeDevice_Click(object sender, EventArgs e)
        {
            if (!(sender is ToolStripButton changeButton)) { return; }
            if(!(changeButton.Tag is EngineDevice device)) { return; }

            m_ctrlRenderPanel.RenderLoop.SetRenderingDevice(device);
        }
    }
}
