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
            this._ctrlRenderPanel = new SeeingSharp.Multimedia.Views.SeeingSharpRendererControl();
            this._refreshTimer = new System.Windows.Forms.Timer(this.components);
            this._barStatus = new System.Windows.Forms.StatusStrip();
            this._lblResolutionDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblResolution = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblObjectCountDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblObjectCount = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblDeviceDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblDevice = new System.Windows.Forms.ToolStripStatusLabel();
            this._barTools = new System.Windows.Forms.ToolStrip();
            this._cmdScreenshot = new System.Windows.Forms.ToolStripButton();
            this._cmdNewChildWindow = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._mnuChangeResolution = new System.Windows.Forms.ToolStripDropDownButton();
            this.x600ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x768ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x1024ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x1080ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.to1024x1024ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._mnuChangeDevice = new System.Windows.Forms.ToolStripDropDownButton();
            this._tabControlSamples = new System.Windows.Forms.TabControl();
            this._images = new System.Windows.Forms.ImageList(this.components);
            this._propertyGrid = new System.Windows.Forms.PropertyGrid();
            this._splitter = new System.Windows.Forms.SplitContainer();
            this._renderWindowControlsComponent = new SeeingSharp.WinFormsSamples.RenderWindowControlsComponent();
            this._barStatus.SuspendLayout();
            this._barTools.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._splitter)).BeginInit();
            this._splitter.Panel1.SuspendLayout();
            this._splitter.Panel2.SuspendLayout();
            this._splitter.SuspendLayout();
            this.SuspendLayout();
            // 
            // _ctrlRenderPanel
            // 
            this._ctrlRenderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._ctrlRenderPanel.Location = new System.Drawing.Point(0, 0);
            this._ctrlRenderPanel.Name = "_ctrlRenderPanel";
            this._ctrlRenderPanel.Size = new System.Drawing.Size(671, 431);
            this._ctrlRenderPanel.TabIndex = 0;
            // 
            // _refreshTimer
            // 
            this._refreshTimer.Enabled = true;
            this._refreshTimer.Tick += new System.EventHandler(this.OnRefreshTimer_Tick);
            // 
            // _barStatus
            // 
            this._barStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._lblResolutionDesc,
            this._lblResolution,
            this.toolStripStatusLabel1,
            this._lblObjectCountDesc,
            this._lblObjectCount,
            this._lblDeviceDesc,
            this._lblDevice});
            this._barStatus.Location = new System.Drawing.Point(0, 576);
            this._barStatus.Name = "_barStatus";
            this._barStatus.Size = new System.Drawing.Size(843, 24);
            this._barStatus.TabIndex = 2;
            this._barStatus.Text = "statusStrip1";
            // 
            // _lblResolutionDesc
            // 
            this._lblResolutionDesc.Name = "_lblResolutionDesc";
            this._lblResolutionDesc.Size = new System.Drawing.Size(66, 19);
            this._lblResolutionDesc.Text = "Resolution:";
            // 
            // _lblResolution
            // 
            this._lblResolution.Name = "_lblResolution";
            this._lblResolution.Size = new System.Drawing.Size(92, 19);
            this._lblResolution.Text = "<widthxheight>";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(19, 19);
            this.toolStripStatusLabel1.Text = "px";
            // 
            // _lblObjectCountDesc
            // 
            this._lblObjectCountDesc.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this._lblObjectCountDesc.Name = "_lblObjectCountDesc";
            this._lblObjectCountDesc.Size = new System.Drawing.Size(64, 19);
            this._lblObjectCountDesc.Text = "# Objects:";
            // 
            // _lblObjectCount
            // 
            this._lblObjectCount.Name = "_lblObjectCount";
            this._lblObjectCount.Size = new System.Drawing.Size(54, 19);
            this._lblObjectCount.Text = "<count>";
            // 
            // _lblDeviceDesc
            // 
            this._lblDeviceDesc.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this._lblDeviceDesc.Name = "_lblDeviceDesc";
            this._lblDeviceDesc.Size = new System.Drawing.Size(49, 19);
            this._lblDeviceDesc.Text = "Device:";
            // 
            // _lblDevice
            // 
            this._lblDevice.Name = "_lblDevice";
            this._lblDevice.Size = new System.Drawing.Size(57, 19);
            this._lblDevice.Text = "<device>";
            // 
            // _barTools
            // 
            this._barTools.AllowMerge = false;
            this._barTools.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._barTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._cmdScreenshot,
            this.toolStripSeparator1,
            this._mnuChangeResolution,
            this._mnuChangeDevice,
            _cmdNewChildWindow});
            this._barTools.Location = new System.Drawing.Point(0, 0);
            this._barTools.Name = "_barTools";
            this._barTools.Size = new System.Drawing.Size(843, 25);
            this._barTools.TabIndex = 3;
            this._barTools.Text = "toolStrip1";
            // 
            // _cmdScreenshot
            // 
            this._cmdScreenshot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._cmdScreenshot.Image = global::SeeingSharp.WinFormsSamples.Properties.Resources.Camera16x16;
            this._cmdScreenshot.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._cmdScreenshot.Name = "_cmdScreenshot";
            this._cmdScreenshot.Size = new System.Drawing.Size(23, 22);
            this._cmdScreenshot.Text = "Copy Screenshot to Clipboard";
            this._cmdScreenshot.Click += new System.EventHandler(this.OnCmdCopyScreenshot_Click);
            // 
            // _cmdNewChildWindow
            // 
            this._cmdNewChildWindow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._cmdNewChildWindow.Name = "_cmdNewChildWindow";
            this._cmdNewChildWindow.Size = new System.Drawing.Size(23, 22);
            this._cmdNewChildWindow.Text = "New child window";
            this._cmdNewChildWindow.Click += new System.EventHandler(this.OnCmdNewChildWindow_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // _mnuChangeResolution
            // 
            this._mnuChangeResolution.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.x600ToolStripMenuItem,
            this.x768ToolStripMenuItem,
            this.x1024ToolStripMenuItem,
            this.x1080ToolStripMenuItem,
            this.toolStripSeparator2,
            this.to1024x1024ToolStripMenuItem});
            this._mnuChangeResolution.Image = global::SeeingSharp.WinFormsSamples.Properties.Resources.Output16x16;
            this._mnuChangeResolution.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._mnuChangeResolution.Name = "_mnuChangeResolution";
            this._mnuChangeResolution.Size = new System.Drawing.Size(133, 22);
            this._mnuChangeResolution.Text = "Change resolution";
            // 
            // x600ToolStripMenuItem
            // 
            this.x600ToolStripMenuItem.Name = "x600ToolStripMenuItem";
            this.x600ToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.x600ToolStripMenuItem.Tag = "800x600";
            this.x600ToolStripMenuItem.Text = "to 800x600";
            // 
            // x768ToolStripMenuItem
            // 
            this.x768ToolStripMenuItem.Name = "x768ToolStripMenuItem";
            this.x768ToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.x768ToolStripMenuItem.Tag = "1024x768";
            this.x768ToolStripMenuItem.Text = "to 1024x768";
            // 
            // x1024ToolStripMenuItem
            // 
            this.x1024ToolStripMenuItem.Name = "x1024ToolStripMenuItem";
            this.x1024ToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.x1024ToolStripMenuItem.Tag = "1280x1024";
            this.x1024ToolStripMenuItem.Text = "to 1280x1024";
            // 
            // x1080ToolStripMenuItem
            // 
            this.x1080ToolStripMenuItem.Name = "x1080ToolStripMenuItem";
            this.x1080ToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.x1080ToolStripMenuItem.Tag = "1600x1200";
            this.x1080ToolStripMenuItem.Text = "to 1600x1200";
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
            // 
            // _mnuChangeDevice
            // 
            this._mnuChangeDevice.Image = global::SeeingSharp.WinFormsSamples.Properties.Resources.Adapter16x16;
            this._mnuChangeDevice.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._mnuChangeDevice.Name = "_mnuChangeDevice";
            this._mnuChangeDevice.Size = new System.Drawing.Size(114, 22);
            this._mnuChangeDevice.Text = "Change device";
            // 
            // _tabControlSamples
            // 
            this._tabControlSamples.Dock = System.Windows.Forms.DockStyle.Top;
            this._tabControlSamples.Location = new System.Drawing.Point(0, 25);
            this._tabControlSamples.Name = "_tabControlSamples";
            this._tabControlSamples.SelectedIndex = 0;
            this._tabControlSamples.Size = new System.Drawing.Size(843, 120);
            this._tabControlSamples.TabIndex = 4;
            // 
            // _images
            // 
            this._images.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this._images.ImageSize = new System.Drawing.Size(64, 64);
            this._images.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // _propertyGrid
            // 
            this._propertyGrid.CanShowVisualStyleGlyphs = false;
            this._propertyGrid.CommandsVisibleIfAvailable = false;
            this._propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._propertyGrid.HelpVisible = false;
            this._propertyGrid.Location = new System.Drawing.Point(0, 0);
            this._propertyGrid.Name = "_propertyGrid";
            this._propertyGrid.Size = new System.Drawing.Size(168, 431);
            this._propertyGrid.TabIndex = 5;
            this._propertyGrid.ToolbarVisible = false;
            // 
            // _splitter
            // 
            this._splitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this._splitter.Location = new System.Drawing.Point(0, 145);
            this._splitter.Name = "_splitter";
            // 
            // _splitter.Panel1
            // 
            this._splitter.Panel1.Controls.Add(this._propertyGrid);
            // 
            // _splitter.Panel2
            // 
            this._splitter.Panel2.Controls.Add(this._ctrlRenderPanel);
            this._splitter.Size = new System.Drawing.Size(843, 431);
            this._splitter.SplitterDistance = 168;
            this._splitter.TabIndex = 6;
            // 
            // _renderWindowControlsComponent
            // 
            this._renderWindowControlsComponent.LblCurrentDevice = this._lblDevice;
            this._renderWindowControlsComponent.LblCurrentObjectCount = this._lblObjectCount;
            this._renderWindowControlsComponent.LblCurrentResolution = this._lblResolution;
            this._renderWindowControlsComponent.RenderControl = this._ctrlRenderPanel;
            this._renderWindowControlsComponent.MnuChooseDevice = this._mnuChangeDevice;
            this._renderWindowControlsComponent.TargetWindow = this;
            this._renderWindowControlsComponent.MnuChangeResolution = this._mnuChangeResolution;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(843, 600);
            this.Controls.Add(this._splitter);
            this.Controls.Add(this._tabControlSamples);
            this.Controls.Add(this._barTools);
            this.Controls.Add(this._barStatus);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainWindow";
            this.Text = "Seeing# 2 - Windows.Forms samples - Main window";
            this._barStatus.ResumeLayout(false);
            this._barStatus.PerformLayout();
            this._barTools.ResumeLayout(false);
            this._barTools.PerformLayout();
            this._splitter.Panel1.ResumeLayout(false);
            this._splitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitter)).EndInit();
            this._splitter.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        

        private Multimedia.Views.SeeingSharpRendererControl _ctrlRenderPanel;
        private System.Windows.Forms.Timer _refreshTimer;
        private System.Windows.Forms.StatusStrip _barStatus;
        private System.Windows.Forms.ToolStripStatusLabel _lblResolutionDesc;
        private System.Windows.Forms.ToolStripStatusLabel _lblResolution;
        private System.Windows.Forms.ToolStrip _barTools;
        private System.Windows.Forms.ToolStripDropDownButton _mnuChangeResolution;
        private System.Windows.Forms.ToolStripMenuItem x600ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x768ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x1024ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x1080ToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton _cmdScreenshot;
        private System.Windows.Forms.ToolStripButton _cmdNewChildWindow;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripStatusLabel _lblObjectCountDesc;
        private System.Windows.Forms.ToolStripStatusLabel _lblObjectCount;
        private System.Windows.Forms.ToolStripDropDownButton _mnuChangeDevice;
        private System.Windows.Forms.ToolStripStatusLabel _lblDeviceDesc;
        private System.Windows.Forms.ToolStripStatusLabel _lblDevice;
        private System.Windows.Forms.TabControl _tabControlSamples;
        private System.Windows.Forms.ImageList _images;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem to1024x1024ToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.PropertyGrid _propertyGrid;
        private System.Windows.Forms.SplitContainer _splitter;
        private RenderWindowControlsComponent _renderWindowControlsComponent;
    }
}

