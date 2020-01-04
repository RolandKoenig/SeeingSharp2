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
            this.m_ctrlRenderer = new SeeingSharp.Multimedia.Views.SeeingSharpRendererControl();
            this.m_refreshTimer = new System.Windows.Forms.Timer(this.components);
            this.m_barTools = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.m_mnuChangeResolution = new System.Windows.Forms.ToolStripDropDownButton();
            this.x600ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x768ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x1024ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x1080ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.to1024x1024ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m_mnuChangeDevice = new System.Windows.Forms.ToolStripDropDownButton();
            this.m_barStatus = new System.Windows.Forms.StatusStrip();
            this.m_lblResolutionDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_lblResolution = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_lblObjectCountDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_lblObjectCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_lblDeviceDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_lblDevice = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_renderWindowControlsComponent = new SeeingSharp.WinFormsSamples.RenderWindowControlsComponent();
            this.m_barTools.SuspendLayout();
            this.m_barStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_ctrlRenderer
            // 
            this.m_ctrlRenderer.DiscardRendering = true;
            this.m_ctrlRenderer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ctrlRenderer.Location = new System.Drawing.Point(0, 25);
            this.m_ctrlRenderer.Name = "m_ctrlRenderer";
            this.m_ctrlRenderer.Size = new System.Drawing.Size(887, 427);
            this.m_ctrlRenderer.TabIndex = 0;
            this.m_ctrlRenderer.ViewConfiguration.ViewNeedsRefresh = true;
            // 
            // m_refreshTimer
            // 
            this.m_refreshTimer.Enabled = true;
            this.m_refreshTimer.Tick += new System.EventHandler(this.OnRefreshTimer_Tick);
            // 
            // m_barTools
            // 
            this.m_barTools.AllowMerge = false;
            this.m_barTools.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.m_barTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.m_mnuChangeResolution,
            this.m_mnuChangeDevice});
            this.m_barTools.Location = new System.Drawing.Point(0, 0);
            this.m_barTools.Name = "m_barTools";
            this.m_barTools.Size = new System.Drawing.Size(887, 25);
            this.m_barTools.TabIndex = 5;
            this.m_barTools.Text = "toolStrip1";
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
            // m_mnuChangeDevice
            // 
            this.m_mnuChangeDevice.Image = global::SeeingSharp.WinFormsSamples.Properties.Resources.Adapter16x16;
            this.m_mnuChangeDevice.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_mnuChangeDevice.Name = "m_mnuChangeDevice";
            this.m_mnuChangeDevice.Size = new System.Drawing.Size(114, 22);
            this.m_mnuChangeDevice.Text = "Change device";
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
            this.m_barStatus.Location = new System.Drawing.Point(0, 452);
            this.m_barStatus.Name = "m_barStatus";
            this.m_barStatus.Size = new System.Drawing.Size(887, 24);
            this.m_barStatus.TabIndex = 6;
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
            this.m_lblResolution.Size = new System.Drawing.Size(25, 19);
            this.m_lblResolution.Text = "0x0";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(20, 19);
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
            this.m_lblObjectCount.Size = new System.Drawing.Size(30, 19);
            this.m_lblObjectCount.Text = "0 / 0";
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
            this.m_lblDevice.Size = new System.Drawing.Size(12, 19);
            this.m_lblDevice.Text = "-";
            // 
            // m_renderWindowControlsComponent
            // 
            this.m_renderWindowControlsComponent.LblCurrentDevice = this.m_lblDevice;
            this.m_renderWindowControlsComponent.LblCurrentObjectCount = this.m_lblObjectCount;
            this.m_renderWindowControlsComponent.LblCurrentResolution = this.m_lblResolution;
            this.m_renderWindowControlsComponent.RenderControl = this.m_ctrlRenderer;
            this.m_renderWindowControlsComponent.MnuChooseDevice = this.m_mnuChangeDevice;
            this.m_renderWindowControlsComponent.TargetWindow = this;
            this.m_renderWindowControlsComponent.MnuChangeResolution = this.m_mnuChangeResolution;
            // 
            // ChildRenderWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(887, 476);
            this.Controls.Add(this.m_ctrlRenderer);
            this.Controls.Add(this.m_barStatus);
            this.Controls.Add(this.m_barTools);
            this.Name = "ChildRenderWindow";
            this.Text = "Seeing# 2 - Windows.Forms samples - Child window";
            this.m_barTools.ResumeLayout(false);
            this.m_barTools.PerformLayout();
            this.m_barStatus.ResumeLayout(false);
            this.m_barStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Multimedia.Views.SeeingSharpRendererControl m_ctrlRenderer;
        private System.Windows.Forms.Timer m_refreshTimer;
        private System.Windows.Forms.ToolStrip m_barTools;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton m_mnuChangeResolution;
        private System.Windows.Forms.ToolStripMenuItem x600ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x768ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x1024ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem x1080ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem to1024x1024ToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton m_mnuChangeDevice;
        private System.Windows.Forms.StatusStrip m_barStatus;
        private System.Windows.Forms.ToolStripStatusLabel m_lblResolutionDesc;
        private System.Windows.Forms.ToolStripStatusLabel m_lblResolution;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel m_lblObjectCountDesc;
        private System.Windows.Forms.ToolStripStatusLabel m_lblObjectCount;
        private System.Windows.Forms.ToolStripStatusLabel m_lblDeviceDesc;
        private System.Windows.Forms.ToolStripStatusLabel m_lblDevice;
        private RenderWindowControlsComponent m_renderWindowControlsComponent;
    }
}