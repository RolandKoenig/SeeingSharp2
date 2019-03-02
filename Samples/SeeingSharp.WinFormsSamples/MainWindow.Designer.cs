namespace SeeingSharp.WinFormsSamples
{
    partial class MainWindow
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.m_ctrlRenderPanel = new SeeingSharp.Multimedia.Views.SeeingSharpRendererControl();
            this.m_refreshTimer = new System.Windows.Forms.Timer(this.components);
            this.m_barStatus = new System.Windows.Forms.StatusStrip();
            this.m_lblResolutionDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_lblResolution = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_lblObjectCountDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_lblObjectCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_lblDeviceDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_lblDevice = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_barTools = new System.Windows.Forms.ToolStrip();
            this.m_cmdScreenshot = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.m_mnuChangeResolution = new System.Windows.Forms.ToolStripDropDownButton();
            this.x600ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x768ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x1024ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x1080ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.to1024x1024ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m_mnuChangeDevice = new System.Windows.Forms.ToolStripDropDownButton();
            this.m_tabControlSamples = new System.Windows.Forms.TabControl();
            this.m_images = new System.Windows.Forms.ImageList(this.components);
            this.m_propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.m_splitter = new System.Windows.Forms.SplitContainer();
            this.m_barStatus.SuspendLayout();
            this.m_barTools.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_splitter)).BeginInit();
            this.m_splitter.Panel1.SuspendLayout();
            this.m_splitter.Panel2.SuspendLayout();
            this.m_splitter.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_ctrlRenderPanel
            // 
            this.m_ctrlRenderPanel.DiscardRendering = true;
            this.m_ctrlRenderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ctrlRenderPanel.Location = new System.Drawing.Point(0, 0);
            this.m_ctrlRenderPanel.Name = "m_ctrlRenderPanel";
            this.m_ctrlRenderPanel.Size = new System.Drawing.Size(671, 431);
            this.m_ctrlRenderPanel.TabIndex = 0;
            this.m_ctrlRenderPanel.ViewConfiguration.ViewNeedsRefresh = true;
            // 
            // m_refreshTimer
            // 
            this.m_refreshTimer.Enabled = true;
            this.m_refreshTimer.Tick += new System.EventHandler(this.OnRefreshTimer_Tick);
            // 
            // m_barStatus
            // 
            this.m_barStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_lblResolutionDesc,
            this.m_lblResolution,
            this.toolStripStatusLabel1,
            this.m_lblObjectCountDesc,
            this.m_lblObjectCount,
            this.m_lblDeviceDesc,
            this.m_lblDevice});
            this.m_barStatus.Location = new System.Drawing.Point(0, 576);
            this.m_barStatus.Name = "m_barStatus";
            this.m_barStatus.Size = new System.Drawing.Size(843, 24);
            this.m_barStatus.TabIndex = 2;
            this.m_barStatus.Text = "statusStrip1";
            // 
            // m_lblResolutionDesc
            // 
            this.m_lblResolutionDesc.Name = "m_lblResolutionDesc";
            this.m_lblResolutionDesc.Size = new System.Drawing.Size(66, 19);
            this.m_lblResolutionDesc.Text = "Resolution:";
            // 
            // m_lblResolution
            // 
            this.m_lblResolution.Name = "m_lblResolution";
            this.m_lblResolution.Size = new System.Drawing.Size(92, 19);
            this.m_lblResolution.Text = "<widthxheight>";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(19, 19);
            this.toolStripStatusLabel1.Text = "px";
            // 
            // m_lblObjectCountDesc
            // 
            this.m_lblObjectCountDesc.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.m_lblObjectCountDesc.Name = "m_lblObjectCountDesc";
            this.m_lblObjectCountDesc.Size = new System.Drawing.Size(64, 19);
            this.m_lblObjectCountDesc.Text = "# Objects:";
            // 
            // m_lblObjectCount
            // 
            this.m_lblObjectCount.Name = "m_lblObjectCount";
            this.m_lblObjectCount.Size = new System.Drawing.Size(54, 19);
            this.m_lblObjectCount.Text = "<count>";
            // 
            // m_lblDeviceDesc
            // 
            this.m_lblDeviceDesc.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.m_lblDeviceDesc.Name = "m_lblDeviceDesc";
            this.m_lblDeviceDesc.Size = new System.Drawing.Size(49, 19);
            this.m_lblDeviceDesc.Text = "Device:";
            // 
            // m_lblDevice
            // 
            this.m_lblDevice.Name = "m_lblDevice";
            this.m_lblDevice.Size = new System.Drawing.Size(57, 19);
            this.m_lblDevice.Text = "<device>";
            // 
            // m_barTools
            // 
            this.m_barTools.AllowMerge = false;
            this.m_barTools.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.m_barTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_cmdScreenshot,
            this.toolStripSeparator1,
            this.m_mnuChangeResolution,
            this.m_mnuChangeDevice});
            this.m_barTools.Location = new System.Drawing.Point(0, 0);
            this.m_barTools.Name = "m_barTools";
            this.m_barTools.Size = new System.Drawing.Size(843, 25);
            this.m_barTools.TabIndex = 3;
            this.m_barTools.Text = "toolStrip1";
            // 
            // m_cmdScreenshot
            // 
            this.m_cmdScreenshot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_cmdScreenshot.Image = global::SeeingSharp.WinFormsSamples.Properties.Resources.Camera16x16;
            this.m_cmdScreenshot.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_cmdScreenshot.Name = "m_cmdScreenshot";
            this.m_cmdScreenshot.Size = new System.Drawing.Size(23, 22);
            this.m_cmdScreenshot.Text = "Copy Screenshot to Clipboard";
            this.m_cmdScreenshot.Click += new System.EventHandler(this.OnCmdCopyScreenshot_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // m_mnuChangeResolution
            // 
            this.m_mnuChangeResolution.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.x600ToolStripMenuItem,
            this.x768ToolStripMenuItem,
            this.x1024ToolStripMenuItem,
            this.x1080ToolStripMenuItem,
            this.toolStripSeparator2,
            this.to1024x1024ToolStripMenuItem});
            this.m_mnuChangeResolution.Image = global::SeeingSharp.WinFormsSamples.Properties.Resources.Output16x16;
            this.m_mnuChangeResolution.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_mnuChangeResolution.Name = "m_mnuChangeResolution";
            this.m_mnuChangeResolution.Size = new System.Drawing.Size(133, 22);
            this.m_mnuChangeResolution.Text = "Change resolution";
            // 
            // x600ToolStripMenuItem
            // 
            this.x600ToolStripMenuItem.Name = "x600ToolStripMenuItem";
            this.x600ToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.x600ToolStripMenuItem.Tag = "800x600";
            this.x600ToolStripMenuItem.Text = "to 800x600";
            this.x600ToolStripMenuItem.Click += new System.EventHandler(this.OnCmdChangeResolution_Click);
            // 
            // x768ToolStripMenuItem
            // 
            this.x768ToolStripMenuItem.Name = "x768ToolStripMenuItem";
            this.x768ToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.x768ToolStripMenuItem.Tag = "1024x768";
            this.x768ToolStripMenuItem.Text = "to 1024x768";
            this.x768ToolStripMenuItem.Click += new System.EventHandler(this.OnCmdChangeResolution_Click);
            // 
            // x1024ToolStripMenuItem
            // 
            this.x1024ToolStripMenuItem.Name = "x1024ToolStripMenuItem";
            this.x1024ToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.x1024ToolStripMenuItem.Tag = "1280x1024";
            this.x1024ToolStripMenuItem.Text = "to 1280x1024";
            this.x1024ToolStripMenuItem.Click += new System.EventHandler(this.OnCmdChangeResolution_Click);
            // 
            // x1080ToolStripMenuItem
            // 
            this.x1080ToolStripMenuItem.Name = "x1080ToolStripMenuItem";
            this.x1080ToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.x1080ToolStripMenuItem.Tag = "1600x1200";
            this.x1080ToolStripMenuItem.Text = "to 1600x1200";
            this.x1080ToolStripMenuItem.Click += new System.EventHandler(this.OnCmdChangeResolution_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(138, 6);
            // 
            // to1024x1024ToolStripMenuItem
            // 
            this.to1024x1024ToolStripMenuItem.Name = "to1024x1024ToolStripMenuItem";
            this.to1024x1024ToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.to1024x1024ToolStripMenuItem.Tag = "1024x1024";
            this.to1024x1024ToolStripMenuItem.Text = "to 1024x1024";
            this.to1024x1024ToolStripMenuItem.Click += new System.EventHandler(this.OnCmdChangeResolution_Click);
            // 
            // m_mnuChangeDevice
            // 
            this.m_mnuChangeDevice.Image = global::SeeingSharp.WinFormsSamples.Properties.Resources.Adapter16x16;
            this.m_mnuChangeDevice.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_mnuChangeDevice.Name = "m_mnuChangeDevice";
            this.m_mnuChangeDevice.Size = new System.Drawing.Size(114, 22);
            this.m_mnuChangeDevice.Text = "Change device";
            // 
            // m_tabControlSamples
            // 
            this.m_tabControlSamples.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_tabControlSamples.Location = new System.Drawing.Point(0, 25);
            this.m_tabControlSamples.Name = "m_tabControlSamples";
            this.m_tabControlSamples.SelectedIndex = 0;
            this.m_tabControlSamples.Size = new System.Drawing.Size(843, 120);
            this.m_tabControlSamples.TabIndex = 4;
            // 
            // m_images
            // 
            this.m_images.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.m_images.ImageSize = new System.Drawing.Size(64, 64);
            this.m_images.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // m_propertyGrid
            // 
            this.m_propertyGrid.CanShowVisualStyleGlyphs = false;
            this.m_propertyGrid.CommandsVisibleIfAvailable = false;
            this.m_propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_propertyGrid.HelpVisible = false;
            this.m_propertyGrid.Location = new System.Drawing.Point(0, 0);
            this.m_propertyGrid.Name = "m_propertyGrid";
            this.m_propertyGrid.Size = new System.Drawing.Size(168, 431);
            this.m_propertyGrid.TabIndex = 5;
            this.m_propertyGrid.ToolbarVisible = false;
            // 
            // m_splitter
            // 
            this.m_splitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_splitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.m_splitter.Location = new System.Drawing.Point(0, 145);
            this.m_splitter.Name = "m_splitter";
            // 
            // m_splitter.Panel1
            // 
            this.m_splitter.Panel1.Controls.Add(this.m_propertyGrid);
            // 
            // m_splitter.Panel2
            // 
            this.m_splitter.Panel2.Controls.Add(this.m_ctrlRenderPanel);
            this.m_splitter.Size = new System.Drawing.Size(843, 431);
            this.m_splitter.SplitterDistance = 168;
            this.m_splitter.TabIndex = 6;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(843, 600);
            this.Controls.Add(this.m_splitter);
            this.Controls.Add(this.m_tabControlSamples);
            this.Controls.Add(this.m_barTools);
            this.Controls.Add(this.m_barStatus);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainWindow";
            this.Text = "Seeing# 2 - Windows.Forms samples";
            this.m_barStatus.ResumeLayout(false);
            this.m_barStatus.PerformLayout();
            this.m_barTools.ResumeLayout(false);
            this.m_barTools.PerformLayout();
            this.m_splitter.Panel1.ResumeLayout(false);
            this.m_splitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_splitter)).EndInit();
            this.m_splitter.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        

        private Multimedia.Views.SeeingSharpRendererControl m_ctrlRenderPanel;
        private System.Windows.Forms.Timer m_refreshTimer;
        private System.Windows.Forms.StatusStrip m_barStatus;
        private System.Windows.Forms.ToolStripStatusLabel m_lblResolutionDesc;
        private System.Windows.Forms.ToolStripStatusLabel m_lblResolution;
        private System.Windows.Forms.ToolStrip m_barTools;
        private System.Windows.Forms.ToolStripDropDownButton m_mnuChangeResolution;
        private System.Windows.Forms.ToolStripMenuItem x600ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x768ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x1024ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x1080ToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton m_cmdScreenshot;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripStatusLabel m_lblObjectCountDesc;
        private System.Windows.Forms.ToolStripStatusLabel m_lblObjectCount;
        private System.Windows.Forms.ToolStripDropDownButton m_mnuChangeDevice;
        private System.Windows.Forms.ToolStripStatusLabel m_lblDeviceDesc;
        private System.Windows.Forms.ToolStripStatusLabel m_lblDevice;
        private System.Windows.Forms.TabControl m_tabControlSamples;
        private System.Windows.Forms.ImageList m_images;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem to1024x1024ToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.PropertyGrid m_propertyGrid;
        private System.Windows.Forms.SplitContainer m_splitter;
    }
}

