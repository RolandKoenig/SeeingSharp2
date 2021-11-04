using System.Windows.Forms;

namespace GettingStarted.WinForms
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
            this._ctrlView3D = new SeeingSharp.Views.SeeingSharpRendererControl();
            this.SuspendLayout();
            // 
            // _ctrlView3D
            // 
            this._ctrlView3D.Location = new System.Drawing.Point(104, 73);
            this._ctrlView3D.Name = "_ctrlView3D";
            this._ctrlView3D.Size = new System.Drawing.Size(200, 100);
            this._ctrlView3D.TabIndex = 0;
            this._ctrlView3D.Dock = DockStyle.Fill;
            // 
            // Form1
            // 
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "SeeingSharp2 - Getting Started (Windows.Forms)";
            this.Controls.Add(this._ctrlView3D);
            this.Name = "MainWindow";
            this.ResumeLayout(false);
        }

        #endregion

        private SeeingSharp.Views.SeeingSharpRendererControl _ctrlView3D;
    }
}

