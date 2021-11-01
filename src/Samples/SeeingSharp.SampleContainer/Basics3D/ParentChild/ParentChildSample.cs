using System;
using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;

namespace SeeingSharp.SampleContainer.Basics3D.ParentChild
{
    [SampleDescription(
        "Parent/Child", 3, nameof(Basics3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Basics3D/ParentChild",
        typeof(SampleSettingsWith3D))]
    public class ParentChildSample : SampleBase
    {
        public override async Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            await mainRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Create floor
                this.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create cube geometry resource
                var resCubeGeometry = manipulator.AddGeometryResource(new CubeGeometryFactory());
                var resMaterial = manipulator.AddStandardMaterialResource(enableShaderGeneratedBorder: true);

                //********************************
                // Create parent object
                var cubeMesh = new Mesh(resCubeGeometry, resMaterial);
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

                //********************************
                // Create first level child
                var actChild = new Mesh(resCubeGeometry, resMaterial);
                actChild.Position = new Vector3(-2f, 0f, 0f);
                actChild.Scaling = new Vector3(0.5f, 0.5f, 0.5f);
                manipulator.AddObject(actChild);
                manipulator.AddChildObject(cubeMesh, actChild);

                actChild = new Mesh(resCubeGeometry, resMaterial);
                actChild.Position = new Vector3(0f, 0f, 2f);
                actChild.Scaling = new Vector3(0.5f, 0.5f, 0.5f);
                manipulator.AddObject(actChild);
                manipulator.AddChildObject(cubeMesh, actChild);

                //********************************
                // Create second level parent/child relationships
                var actSecondLevelParent = new Mesh(resCubeGeometry, resMaterial);
                actSecondLevelParent.Position = new Vector3(3f, 0f, 0f);
                actSecondLevelParent.Scaling = new Vector3(0.8f, 0.8f, 0.8f);
                actSecondLevelParent.Color = Color4.BlueColor;
                actSecondLevelParent.BuildAnimationSequence()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .CallAction(() => actSecondLevelParent.RotationEuler = Vector3.Zero)
                    .ApplyAndRewind();
                manipulator.AddObject(actSecondLevelParent);
                manipulator.AddChildObject(cubeMesh, actSecondLevelParent);

                var actSecondLevelChild = new Mesh(resCubeGeometry, resMaterial);
                actSecondLevelChild.Position = new Vector3(1f, 0f, 0f);
                actSecondLevelChild.Scaling = new Vector3(0.4f, 0.4f, 0.4f);
                manipulator.AddObject(actSecondLevelChild);
                manipulator.AddChildObject(actSecondLevelParent, actSecondLevelChild);

                actSecondLevelChild = new Mesh(resCubeGeometry, resMaterial);
                actSecondLevelChild.Position = new Vector3(-1f, 0f, 0f);
                actSecondLevelChild.Scaling = new Vector3(0.4f, 0.4f, 0.4f);
                manipulator.AddObject(actSecondLevelChild);
                manipulator.AddChildObject(actSecondLevelParent, actSecondLevelChild);

                actSecondLevelChild = new Mesh(resCubeGeometry, resMaterial);
                actSecondLevelChild.Position = new Vector3(0f, 0f, 1f);
                actSecondLevelChild.Scaling = new Vector3(0.4f, 0.4f, 0.4f);
                manipulator.AddObject(actSecondLevelChild);
                manipulator.AddChildObject(actSecondLevelParent, actSecondLevelChild);
            });
        }

        public override Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop)
        {
            var camera = mainOrChildRenderLoop.Camera;

            // ConfigureLoading camera
            camera.Position = new Vector3(5f, 5f, 5f);
            camera.Target = new Vector3(0f, 0.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            mainOrChildRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());

            // Add object filter for viewbox culling
            mainOrChildRenderLoop.ObjectFilters.Add(new SceneViewboxObjectFilter());

            return Task.FromResult<object>(null);
        }
    }
}