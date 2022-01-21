using System;
using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Components.Input;
using SeeingSharp.Core;
using SeeingSharp.Core.Animations;
using SeeingSharp.Drawing3D;
using SeeingSharp.Drawing3D.Resources;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Primitives3D
{
    public abstract class Primitive3DSampleBase : SampleBase
    {
        private Primitive3DSampleSettings? _sampleSettings;

        public override Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            _sampleSettings = (Primitive3DSampleSettings) settings;

            return Task.CompletedTask;
        }

        public override async Task OnReloadAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            await mainRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Clear previous scene
                manipulator.Clear();

                // Create floor
                this.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create material resource
                NamedOrGenericKey resMaterial;
                if (_sampleSettings!.Textured)
                {
                    var resTexture = manipulator.AddTextureResource(
                        new AssemblyResourceLink(
                            typeof(Primitive3DSampleBase),
                            "PrimitiveTexture.png"));
                    resMaterial = manipulator.AddStandardMaterialResource(resTexture);
                }
                else
                {
                    resMaterial = manipulator.AddStandardMaterialResource();
                }

                // Create Sphere object
                var sphereMesh = this.CreateMesh(manipulator, settings, resMaterial);
                sphereMesh.Color = Color4.GreenColor;
                if (_sampleSettings.Animated)
                {
                    sphereMesh.BuildAnimationSequence()
                        .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                        .WaitFinished()
                        .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                        .WaitFinished()
                        .CallAction(() => sphereMesh.RotationEuler = Vector3.Zero)
                        .ApplyAndRewind();
                }
                manipulator.AddObject(sphereMesh);
            });
        }

        public override Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop)
        {
            var camera = mainOrChildRenderLoop.Camera;

            // ConfigureLoading camera
            camera.Position = new Vector3(3f, 3f, 3f);
            camera.Target = new Vector3(0f, 0.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            mainOrChildRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());

            // Add object filter for viewbox culling
            mainOrChildRenderLoop.ObjectFilters.Add(new SceneViewboxObjectFilter());

            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates the primitive mesh with the given settings and material.
        /// </summary>
        protected abstract Mesh CreateMesh(SceneManipulator manipulator, SampleSettings sampleSettings, NamedOrGenericKey resMaterial);
    }
}
