﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Windows;
using SeeingSharp.Core.Animations;
using SeeingSharp.Drawing3D;
using SeeingSharp.Components.Input;
using SeeingSharp.Drawing3D.Primitives;
using SeeingSharp.Drawing3D.Resources;
using SeeingSharp.Mathematics;

namespace GettingStarted.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += OnMainWindow_Loaded;
        }

        private async void OnMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Build the scene
            var scene = this.CtrlView3D.Scene;
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
            var camera = this.CtrlView3D.Camera;
            camera.Position = new Vector3(3f, 3f, 3f);
            camera.Target = new Vector3(0f, 0.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior (this enables simple input / movement)
            this.CtrlView3D.RenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());
        }
    }
}
