using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Windows.Forms;
using SeeingSharp;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;

namespace GettingStarted.WinForms
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <inheritdoc />
        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Build the scene
            var scene = _ctrlView3D.Scene;
            await scene.ManipulateSceneAsync(manipulator =>
            {
                // Create material resource
                var resMaterial = manipulator.AddStandardMaterialResource();
                var resGeometry = manipulator.AddResource(
                    device => new GeometryResource(new CubeGeometryFactory()));

                // Create the mesh and animate it
                var mesh = new Mesh(resGeometry, resMaterial);
                mesh.Position = new Vector3(0f, 1f, 0f);
                mesh.Color = Color4.GreenColor;
                mesh.BuildAnimationSequence()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .CallAction(() => mesh.RotationEuler = Vector3.Zero)
                    .ApplyAndRewind();
                manipulator.AddObject(mesh);
            });

            // Configure camera
            var camera = _ctrlView3D.Camera;
            camera.Position = new Vector3(3f, 3f, 3f);
            camera.Target = new Vector3(0f, 0.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior (this enables simple input / movement)
            _ctrlView3D.RenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());
        }
    }
}
