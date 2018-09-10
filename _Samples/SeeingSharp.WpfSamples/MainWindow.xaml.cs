using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Components;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SeeingSharp.WpfSamples
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Configure camera
            var camera = new PerspectiveCamera3D();
            camera.Position = new Vector3(-5f, 5f, 5f);
            camera.Target = new Vector3(0f, 0f, 0f);
            camera.UpdateCamera();
            this.CtrlRenderer.Camera = camera;

            await this.CtrlRenderer.Scene.ManipulateSceneAsync(manipulator =>
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

            this.CtrlRenderer.RenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());
        }
    }
}
