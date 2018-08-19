using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Multimedia.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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

            // Configure camera
            var camera = new PerspectiveCamera3D();
            camera.Position = new Vector3(-5f, 5f, 5f);
            camera.Target = new Vector3(0f, 0f, 0f);
            camera.UpdateCamera();
            m_ctrlRenderPanel.Camera = camera;

            await m_ctrlRenderPanel.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Create pallet geometry resource
                CubeType objType = new CubeType();
                var resCubeGeometry = manipulator.AddResource<GeometryResource>(
                    () => new GeometryResource(objType));

                // Create pallet object
                GenericObject cubeObject = manipulator.AddGeneric(resCubeGeometry);
                cubeObject.Color = Color4Ex.GreenColor;
                cubeObject.EnableShaderGeneratedBorder();
                cubeObject.BuildAnimationSequence()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .CallAction(() => cubeObject.RotationEuler = Vector3.Zero)
                    .ApplyAndRewind();
            });

        }
    }
}
