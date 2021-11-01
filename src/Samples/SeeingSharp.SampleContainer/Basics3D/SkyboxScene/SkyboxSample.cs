using System;
using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Drawing3D.Primitives;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Basics3D.SkyboxScene
{
    [SampleDescription(
        "Skybox", 1, nameof(Basics3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Basics3D/SkyboxScene",
        typeof(SampleSettingsWith3D))]
    public class SkyboxSample : SampleBase
    {
        public override async Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            await mainRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Create floor
                this.BuildStandardFloor(manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create resources
                var resGeometry = manipulator.AddGeometryResource(new CubeGeometryFactory());
                var resMaterial = manipulator.AddStandardMaterialResource(enableShaderGeneratedBorder: true);

                // Create cube object
                var cubeMesh = new Mesh(resGeometry, resMaterial);
                cubeMesh.Color = Color4.GreenColor;
                cubeMesh.Position = new Vector3(0f, 0.5f, 0f);
                cubeMesh.BuildAnimationSequence()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .CallAction(() => cubeMesh.RotationEuler = Vector3.Zero)
                    .ApplyAndRewind();
                manipulator.AddObject(cubeMesh);

                var resSkyboxTexture = manipulator.AddTextureResource(
                    new AssemblyResourceLink(this.GetType(),
                        "SkyBox.dds"));

                // Create the skybox on a new layer
                manipulator.AddLayer("Skybox");
                var skyboxObject = new Skybox(resSkyboxTexture);
                manipulator.AddObject(skyboxObject, "Skybox");
            });
        }

        public override Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop)
        {
            var camera = mainOrChildRenderLoop.Camera;

            // ConfigureLoading camera
            camera.Position = new Vector3(3f, 1f, 3f);
            camera.Target = new Vector3(0f, 1.2f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            mainOrChildRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());

            // Add object filter for viewbox culling
            mainOrChildRenderLoop.ObjectFilters.Add(new SceneViewboxObjectFilter());

            return Task.FromResult<object>(null);
        }
    }
}