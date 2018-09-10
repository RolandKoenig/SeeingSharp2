using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Multimedia.Views;
using SharpDX;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace SeeingSharp.UwpSamples
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SeeingSharpPanelPainter m_panelPainter;

        public MainPage()
        {
            this.InitializeComponent();

            this.Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            m_panelPainter = new SeeingSharpPanelPainter(CtrlSwapChain);

            // Configure camera
            var camera = new PerspectiveCamera3D();
            camera.Position = new Vector3(-5f, 5f, 5f);
            camera.Target = new Vector3(0f, 0f, 0f);
            camera.UpdateCamera();
            m_panelPainter.Camera = camera;

            await m_panelPainter.Scene.ManipulateSceneAsync(manipulator =>
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

            m_panelPainter.RenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());
        }
    }
}
