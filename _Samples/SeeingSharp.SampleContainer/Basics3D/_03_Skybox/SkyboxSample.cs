using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using SharpDX;

namespace SeeingSharp.SampleContainer.Basics3D._03_Skybox
{
    [SampleDescription(
        "Skybox", 3, nameof(SeeingSharp.SampleContainer.Basics3D),
        sampleImageFileName: "PreviewImage.png",
        sourceCodeUrl: "https://github.com/RolandKoenig/SeeingSharp2/tree/master/_Samples/SeeingSharp.SampleContainer/Basics3D/_03_Skybox")]
    public class SkyboxSample : SampleBase
    {
        /// <summary>
        /// Called when the sample has to startup.
        /// </summary>
        /// <param name="targetRenderLoop">The target render loop.</param>
        public override async Task OnStartupAsync(RenderLoop targetRenderLoop)
        {
            targetRenderLoop.EnsureNotNull(nameof(targetRenderLoop));

            // Build dummy scene
            Scene scene = targetRenderLoop.Scene;
            Camera3DBase camera = targetRenderLoop.Camera as Camera3DBase;

            await scene.ManipulateSceneAsync((manipulator) =>
            {
                // Create floor
                this.BuildStandardFloor(manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create pallet geometry resource
                CubeType cubeType = new CubeType();
                var resPalletGeometry = manipulator.AddResource<GeometryResource>(
                    () => new GeometryResource(cubeType));

                // Create pallet object
                GenericObject cubeObject = manipulator.AddGeneric(resPalletGeometry);
                cubeObject.Color = Color4Ex.GreenColor;
                cubeObject.Position = new Vector3(0f, 0.5f, 0f);
                cubeObject.EnableShaderGeneratedBorder();
                cubeObject.BuildAnimationSequence()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .CallAction(() => cubeObject.RotationEuler = Vector3.Zero)
                    .ApplyAndRewind();

                var resSkyboxTexture = manipulator.AddTexture(
                    new AssemblyResourceLink(
                       this.GetType(),
                        "SkyBox.dds"));

                // Create the skybox on a new layer
                manipulator.AddLayer("Skybox");
                SkyboxObject skyboxObject = new SkyboxObject(resSkyboxTexture);
                manipulator.Add(skyboxObject, "Skybox");
            });

            // Configure camera
            camera.Position = new Vector3(3f, 1f, 3f);
            camera.Target = new Vector3(0f, 1.2f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            targetRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());
        }
    }
}
