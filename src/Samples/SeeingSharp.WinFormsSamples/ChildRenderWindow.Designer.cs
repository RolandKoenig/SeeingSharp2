namespace SeeingSharp.WinFormsSamples
{
    partial class ChildRenderWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChildRenderWindow));
            this._ctrlRenderer = new SeeingSharp.Views.SeeingSharpRendererControl();
            this._refreshTimer = new System.Windows.Forms.Timer(this.components);
            this._barTools = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._mnuChangeResolution = new System.Windows.Forms.ToolStripDropDownButton();
            this.x600ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x768ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x1024ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x1080ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.to1024x1024ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._mnuChangeDevice = new System.Windows.Forms.ToolStripDropDownButton();
            this._barStatus = new System.Windows.Forms.StatusStrip();
            this._lblResolutionDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblResolution = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblResourceCountDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblResourceCount = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblObjectCountDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblObjectCount = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblDrawCallCountDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblDrawCallCount = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblDeviceDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblDevice = new System.Windows.Forms.ToolStripStatusLabel();
            this._renderWindowControlsComponent = new SeeingSharp.WinFormsSamples.RenderWindowControlsComponent();
            this._barTools.SuspendLayout();
            this._barStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // _ctrlRenderer
            // 
            this._ctrlRenderer.DiscardRendering = true;
            this._ctrlRenderer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._ctrlRenderer.Location = new System.Drawing.Point(0, 25);
            this._ctrlRenderer.Name = "_ctrlRenderer";
            this._ctrlRenderer.Size = new System.Drawing.Size(887, 427);
            this._ctrlRenderer.TabIndex = 0;
            this._ctrlRenderer.Configuration.ViewNeedsRefresh = true;
            // 
            // _refreshTimer
            // 
            this._refreshTimer.Enabled = true;
            this._refreshTimer.Tick += new System.EventHandler(this.OnRefreshTimer_Tick);
            // 
            // _barTools
            // 
            this._barTools.AllowMerge = false;
            this._barTools.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._barTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this._mnuChangeResolution,
            this._mnuChangeDevice});
            this._barTools.Location = new System.Drawing.Point(0, 0);
            this._barTools.Name = "_barTools";
            this._barTools.Size = new System.Drawing.Size(887, 25);
            this._barTools.TabIndex = 5;
            this._barTools.Text = "toolStrip1";
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
            this.x600ToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.x600ToolStripMenuItem.Tag = "800x600";
            this.x600ToolStripMenuItem.Text = "to 800x600";
            // 
            // x768ToolStripMenuItem
            // 
            this.x768ToolStripMenuItem.Name = "x768ToolStripMenuItem";
            this.x768ToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.x768ToolStripMenuItem.Tag = "1024x768";
            this.x768ToolStripMenuItem.Text = "to 1024x768";
            // 
            // x1024ToolStripMenuItem
            // 
            this.x1024ToolStripMenuItem.Name = "x1024ToolStripMenuItem";
            this.x1024ToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.x1024ToolStripMenuItem.Tag = "1280x1024";
            this.x1024ToolStripMenuItem.Text = "to 1280x1024";
            // 
            // x1080ToolStripMenuItem
            // 
            this.x1080ToolStripMenuItem.Name = "x1080ToolStripMenuItem";
            this.x1080ToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.x1080ToolStripMenuItem.Tag = "1600x1200";
            this.x1080ToolStripMenuItem.Text = "to 1600x1200";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(139, 6);
            // 
            // to1024x1024ToolStripMenuItem
            // 
            this.to1024x1024ToolStripMenuItem.Name = "to1024x1024ToolStripMenuItem";
            this.to1024x1024ToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
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
            // _barStatus
            // 
            this._barStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._lblResolutionDesc,
            this._lblResolution,
            this._lblResourceCountDesc,
            this._lblResourceCount,
            this._lblObjectCountDesc,
            this._lblObjectCount,
            this._lblDrawCallCountDesc,
            this._lblDrawCallCount,
            this._lblDeviceDesc,
            this._lblDevice});
            this._barStatus.Location = new System.Drawing.Point(0, 452);
            this._barStatus.Name = "_barStatus";
            this._barStatus.Size = new System.Drawing.Size(887, 24);
            this._barStatus.TabIndex = 6;
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
            this._lblResolution.Size = new System.Drawing.Size(25, 19);
            this._lblResolution.Text = "0x0";
            // 
            // _lblObjectCountDesc
            // 
            this._lblResourceCountDesc.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this._lblResourceCountDesc.Name = "_lblResourceCountDesc";
            this._lblResourceCountDesc.Size = new System.Drawing.Size(64, 19);
            this._lblResourceCountDesc.Text = "# 3D Resources:";
            // 
            // _lblObjectCount
            // 
            this._lblResourceCount.Name = "_lblResourceCount";
            this._lblResourceCount.Size = new System.Drawing.Size(30, 19);
            this._lblResourceCount.Text = "<count>";
            // 
            // _lblObjectCountDesc
            // 
            this._lblObjectCountDesc.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this._lblObjectCountDesc.Name = "_lblObjectCountDesc";
            this._lblObjectCountDesc.Size = new System.Drawing.Size(64, 19);
            this._lblObjectCountDesc.Text = "# 3D Objects:";
            // 
            // _lblObjectCount
            // 
            this._lblObjectCount.Name = "_lblObjectCount";
            this._lblObjectCount.Size = new System.Drawing.Size(30, 19);
            this._lblObjectCount.Text = "<count>";
            // 
            // _lblDrawCallCountDesc
            // 
            this._lblDrawCallCountDesc.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this._lblDrawCallCountDesc.Name = "_lblDrawCallCountDesc";
            this._lblDrawCallCountDesc.Size = new System.Drawing.Size(64, 19);
            this._lblDrawCallCountDesc.Text = "# Draw calls:";
            // 
            // _lblDrawCallCount
            // 
            this._lblDrawCallCount.Name = "_lblDrawCallCount";
            this._lblDrawCallCount.Size = new System.Drawing.Size(30, 19);
            this._lblDrawCallCount.Text = "<count>";
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
            this._lblDevice.Size = new System.Drawing.Size(12, 19);
            this._lblDevice.Text = "-";
            // 
            // _renderWindowControlsComponent
            // 
            this._renderWindowControlsComponent.LblCurrentDevice = this._lblDevice;
            this._renderWindowControlsComponent.LblCurrentResourceCount = this._lblResourceCount;
            this._renderWindowControlsComponent.LblCurrentObjectCount = this._lblObjectCount;
            this._renderWindowControlsComponent.LblCurrentDrawCallCount = this._lblDrawCallCount;
            this._renderWindowControlsComponent.LblCurrentResolution = this._lblResolution;
            this._renderWindowControlsComponent.RenderControl = this._ctrlRenderer;
            this._renderWindowControlsComponent.MnuChooseDevice = this._mnuChangeDevice;
            this._renderWindowControlsComponent.TargetWindow = this;
            this._renderWindowControlsComponent.MnuChangeResolution = this._mnuChangeResolution;
            // 
            // ChildRenderWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(887, 476);
            this.Controls.Add(this._ctrlRenderer);
            this.Controls.Add(this._barStatus);
            this.Controls.Add(this._barTools);
            this.Name = "ChildRenderWindow";
            this.Text = "SeeingSharp 2 - Windows.Forms samples - Child window";
            this._barTools.ResumeLayout(false);
            this._barTools.PerformLayout();
            this._barStatus.ResumeLayout(false);
            this._barStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Views.SeeingSharpRendererControl _ctrlRenderer;
        private System.Windows.Forms.Timer _refreshTimer;
        private System.Windows.Forms.ToolStrip _barTools;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton _mnuChangeResolution;
        private System.Windows.Forms.ToolStripMenuItem x600ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x768ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x1024ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x1080ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem to1024x1024ToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton _mnuChangeDevice;
        private System.Windows.Forms.StatusStrip _barStatus;
        private System.Windows.Forms.ToolStripStatusLabel _lblResolutionDesc;
        private System.Windows.Forms.ToolStripStatusLabel _lblResolution;
        private System.Windows.Forms.ToolStripStatusLabel _lblResourceCountDesc;
        private System.Windows.Forms.ToolStripStatusLabel _lblResourceCount;
        private System.Windows.Forms.ToolStripStatusLabel _lblObjectCountDesc;
        private System.Windows.Forms.ToolStripStatusLabel _lblObjectCount;
        private System.Windows.Forms.ToolStripStatusLabel _lblDrawCallCountDesc;
        private System.Windows.Forms.ToolStripStatusLabel _lblDrawCallCount;
        private System.Windows.Forms.ToolStripStatusLabel _lblDeviceDesc;
        private System.Windows.Forms.ToolStripStatusLabel _lblDevice;
        private RenderWindowControlsComponent _renderWindowControlsComponent;
    }
}