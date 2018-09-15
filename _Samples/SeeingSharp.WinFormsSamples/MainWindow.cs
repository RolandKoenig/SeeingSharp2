using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.SampleContainer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SeeingSharp.Multimedia.Components;
using SharpDX;

namespace SeeingSharp.WinFormsSamples
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SampleRepository sampleRepo = new SampleRepository();
            await sampleRepo.SampleGroups
                .First()
                .Samples
                .First()
                .CreateSampleObject().OnStartupAsync(m_ctrlRenderPanel.RenderLoop);
        }
    }
}
