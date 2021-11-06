using System;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Components;
using SeeingSharp.Components.Input;
using SeeingSharp.Core;
using SeeingSharp.Core.Animations;
using SeeingSharp.Drawing3D;
using SeeingSharp.Drawing3D.Primitives;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.MassScenes.StaticCubes
{
    [SampleDescription(
        "Static Cubes", 1, nameof(MassScenes),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/MassScenes/StaticCubes",
        typeof(StaticCubesSampleSettings))]
    public class StaticCubesSample : SampleBase
    {
        public override Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            return Task.FromResult<object>(null);
        }

        public override Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop)
        {
            var camera = mainOrChildRenderLoop.Camera;

            // ConfigureLoading camera
            camera.Position = new Vector3(40f, 30f, 40f);
            camera.Target = new Vector3(0f, 5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            mainOrChildRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());

            // Add object filter for viewbox culling
            mainOrChildRenderLoop.ObjectFilters.Add(new SceneViewboxObjectFilter());

            return Task.FromResult<object>(null);
        }

        public override async Task OnReloadAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            var castedSettings = (StaticCubesSampleSettings)settings;

            await mainRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Clear previous scene
                manipulator.Clear();

                // Create floor
                this.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create resources
                var resGeometry = manipulator.AddGeometryResource(
                    new CubeGeometryFactory());
                var resColoredMaterial = manipulator.AddStandardMaterialResource(enableShaderGeneratedBorder: true);
                var resTexture = manipulator.AddTextureResource(
                    new AssemblyResourceLink(this.GetType(),
                        "SimpleTexture.png"));
                var resTexturedMaterial = manipulator.AddStandardMaterialResource(resTexture);

                // Create cubes
                var sideLength = castedSettings.CubeCountPerSide;
                var randomizer = new Random();
                for (var loopX = 0; loopX < sideLength; loopX++)
                {
                    for (var loopY = 0; loopY < sideLength; loopY++)
                    {
                        for (var loopZ = 0; loopZ < sideLength; loopZ++)
                        {
                            // Choose material
                            var material = resColoredMaterial;
                            if (castedSettings.HalfTextured)
                            {
                                if ((loopX + loopY + loopZ) % 2 == 1)
                                {
                                    material = resTexturedMaterial;
                                }
                            }

                            var cubeMesh = new Mesh(resGeometry, material);
                            cubeMesh.Color = Color4.GreenColor;
                            cubeMesh.Position = new Vector3(loopX * 1.5f, loopY * 1.5f, loopZ * 1.5f);

                            if (castedSettings.Animated)
                            {
                                cubeMesh.BuildAnimationSequence()
                                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f),
                                        TimeSpan.FromSeconds(1.0 + randomizer.NextDouble() * 2.0))
                                    .WaitFinished()
                                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f),
                                        TimeSpan.FromSeconds(1.0 + randomizer.NextDouble() * 2.0))
                                    .WaitFinished()
                                    .CallAction(() => cubeMesh.RotationEuler = Vector3.Zero)
                                    .ApplyAndRewind();
                            }

                            manipulator.AddObject(cubeMesh);
                        }
                    }
                }
            });
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class StaticCubesSampleSettings : SampleSettingsWith3D
        {
            private int _cubeCountPerSide = 15;
            private bool _animated;
            private bool _halfTextured;

            [Category("Static Cubes")]
            public int CubeCountPerSide
            {
                get => _cubeCountPerSide;
                set
                {
                    var givenValue = value;
                    if (givenValue < 1) { givenValue = 1; }
                    if (givenValue > 50) { givenValue = 50; }

                    _cubeCountPerSide = givenValue;
                    this.RaiseRecreateRequest();
                }
            }

            [Category("Static Cubes")]
            public bool Animated
            {
                get => _animated;
                set
                {
                    _animated = value;
                    this.RaiseRecreateRequest();
                }
            }

            [Category("Static Cubes")]
            public bool HalfTextured
            {
                get => _halfTextured;
                set
                {
                    _halfTextured = value;
                    this.RaiseRecreateRequest();
                }
            }
        }
    }
}