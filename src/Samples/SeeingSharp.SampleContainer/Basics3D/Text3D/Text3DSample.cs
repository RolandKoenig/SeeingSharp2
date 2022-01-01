using System;
using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Components.Input;
using SeeingSharp.Core;
using SeeingSharp.Drawing3D.Geometries;
using SeeingSharp.Drawing3D.Resources;
using SeeingSharp.Mathematics;

namespace SeeingSharp.SampleContainer.Basics3D.Text3D
{
    [SampleDescription(
        "Text 3D", 2, nameof(Basics3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/src/Samples/SeeingSharp.SampleContainer/Basics3D/Text3D",
        typeof(SampleSettingsWith3D))]
    public class Text3DSample : SampleBase
    {
        public override async Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            await mainRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Create floor
                this.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create text geometry
                var textOptions = TextGeometryOptions.Default;
                textOptions.FontSize = 50;
                textOptions.MakeVolumetricText = true;
                textOptions.SurfaceVertexColor = Color.Blue;
                textOptions.VolumetricSideSurfaceVertexColor = Color4.CornflowerBlue;
                var resGeometry = manipulator.Add3DTextGeometryResource(
                    $"SeeingSharp 2 {Environment.NewLine} Text3D Sample",
                    textOptions);

                // Create material
                var resMaterial = manipulator.AddStandardMaterialResource();

                // Create text geometry and object
                var textObject = manipulator.AddMeshObject(resGeometry, resMaterial);
                textObject.YPos = textOptions.VolumetricTextDepth;
            });
        }

        public override Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop)
        {
            var camera = mainOrChildRenderLoop.Camera;

            // ConfigureLoading camera
            camera.Position = new Vector3(0.7f, 8.5f, -15f);
            camera.RelativeTarget = new Vector3(0.44f, -0.62f, 0.64f);
            camera.UpdateCamera();

            // Append camera behavior
            mainOrChildRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());

            // Add object filter for viewbox culling
            mainOrChildRenderLoop.ObjectFilters.Add(new SceneViewboxObjectFilter());

            return Task.FromResult<object>(null);
        }
    }
}