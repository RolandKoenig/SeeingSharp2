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
            this.m_ctrlRenderer = new SeeingSharp.Multimedia.Views.SeeingSharpRendererControl();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.m_barMenu = new System.Windows.Forms.MenuStrip();
            this.m_cmdMnuClose = new System.Windows.Forms.ToolStripMenuItem();
            this.m_barMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_ctrlRenderer
            // 
            this.m_ctrlRenderer.DiscardRendering = true;
            this.m_ctrlRenderer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ctrlRenderer.Location = new System.Drawing.Point(0, 24);
            this.m_ctrlRenderer.Name = "m_ctrlRenderer";
            this.m_ctrlRenderer.Size = new System.Drawing.Size(942, 467);
            this.m_ctrlRenderer.TabIndex = 0;
            this.m_ctrlRenderer.ViewConfiguration.ViewNeedsRefresh = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 491);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(942, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "m_barStatus";
            // 
            // m_barMenu
            // 
            this.m_barMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_cmdMnuClose});
            this.m_barMenu.Location = new System.Drawing.Point(0, 0);
            this.m_barMenu.Name = "m_barMenu";
            this.m_barMenu.Size = new System.Drawing.Size(942, 24);
            this.m_barMenu.TabIndex = 2;
            this.m_barMenu.Text = "menuStrip1";
            // 
            // m_cmdMnuClose
            // 
            this.m_cmdMnuClose.Name = "m_cmdMnuClose";
            this.m_cmdMnuClose.Size = new System.Drawing.Size(48, 20);
            this.m_cmdMnuClose.Text = "Close";
            this.m_cmdMnuClose.Click += new System.EventHandler(this.OnCmdClose_Click);
            // 
            // ChildRenderWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(942, 513);
            this.Controls.Add(this.m_ctrlRenderer);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.m_barMenu);
            this.MainMenuStrip = this.m_barMenu;
            this.Name = "ChildRenderWindow";
            this.Text = "Seeing# 2 - Windows.Forms samples - Child window";
            this.m_barMenu.ResumeLayout(false);
            this.m_barMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Multimedia.Views.SeeingSharpRendererControl m_ctrlRenderer;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip m_barMenu;
        private System.Windows.Forms.ToolStripMenuItem m_cmdMnuClose;
    }
}