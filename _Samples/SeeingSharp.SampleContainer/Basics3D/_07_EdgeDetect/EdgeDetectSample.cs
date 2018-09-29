using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SharpDX;

namespace SeeingSharp.SampleContainer.Basics3D._07_EdgeDetect
{
    [SampleDescription(
        "Edge detect", 7, nameof(SeeingSharp.SampleContainer.Basics3D),
        sampleImageFileName:"PreviewImage.png")]
    public class EdgeDetectSample : SampleBase
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

            await targetRenderLoop.Scene.ManipulateSceneAsync((manipulator) =>
            {
                // Create floor
                base.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create edge detect resource
                var resEdgeDetect = manipulator.AddResource(() => new EdgeDetectPostprocessEffectResource()
                {
                    BorderColor = Color4Ex.BlueColor,
                    Thickness = 5f
                });

                var edgeLayer = manipulator.AddLayer("EdgeDetectLayer");
                edgeLayer.PostprocessEffectKey = resEdgeDetect;

                // Create pallet geometry resource
                CubeType cubeType = new CubeType();
                var resPalletGeometry = manipulator.AddResource<GeometryResource>(
                    () => new GeometryResource(cubeType));

                // Create pallet object
                GenericObject cubeObject = manipulator.AddGeneric(resPalletGeometry, "EdgeDetectLayer");
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
            });

            // Configure camera
            camera.Position = new Vector3(3f, 3f, 3f);
            camera.Target = new Vector3(0f, 0.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            targetRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());
        }
    }
}
