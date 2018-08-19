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

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.m_mainMenu = new System.Windows.Forms.MenuStrip();
            this.m_ctrlRenderPanel = new SeeingSharp.Multimedia.Views.SeeingSharpRendererControl();
            this.SuspendLayout();
            // 
            // m_mainMenu
            // 
            this.m_mainMenu.Location = new System.Drawing.Point(0, 0);
            this.m_mainMenu.Name = "m_mainMenu";
            this.m_mainMenu.Size = new System.Drawing.Size(800, 24);
            this.m_mainMenu.TabIndex = 1;
            this.m_mainMenu.Text = "menuStrip1";
            // 
            // m_ctrlRenderPanel
            // 
            this.m_ctrlRenderPanel.DiscardRendering = true;
            this.m_ctrlRenderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ctrlRenderPanel.Location = new System.Drawing.Point(0, 24);
            this.m_ctrlRenderPanel.Name = "m_ctrlRenderPanel";
            this.m_ctrlRenderPanel.Size = new System.Drawing.Size(800, 426);
            this.m_ctrlRenderPanel.TabIndex = 0;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.m_ctrlRenderPanel);
            this.Controls.Add(this.m_mainMenu);
            this.MainMenuStrip = this.m_mainMenu;
            this.Name = "MainWindow";
            this.Text = "Seeing# 2 - Windows.Forms samples";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Multimedia.Views.SeeingSharpRendererControl m_ctrlRenderPanel;
        private System.Windows.Forms.MenuStrip m_mainMenu;
    }
}

