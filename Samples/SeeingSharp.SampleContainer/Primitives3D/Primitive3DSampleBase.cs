using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Primitives3D
{
    public abstract class Primitive3DSampleBase : SampleBase
    {
        private Primitive3DSampleSettings m_sampleSettings;

        public override Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            m_sampleSettings = (Primitive3DSampleSettings) settings;

            return Task.FromResult<object>(null);
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
                if (m_sampleSettings.Textured)
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
                if (m_sampleSettings.Animated)
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

            // Configure camera
            camera.Position = new Vector3(3f, 3f, 3f);
            camera.Target = new Vector3(0f, 0.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            mainOrChildRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Creates the primitive mesh with the given settings and material.
        /// </summary>
        protected abstract Mesh CreateMesh(SceneManipulator manipulator, SampleSettings sampleSettings, NamedOrGenericKey resMaterial);
    }
}
