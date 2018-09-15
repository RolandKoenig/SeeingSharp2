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

namespace SeeingSharp.SampleContainer.Rendering3D.ColoredCube
{
    [SampleDescription("Colored Cube", 1, nameof(SeeingSharp.SampleContainer.Rendering3D))]
    public class ColoredCubeSample : SampleBase
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

                // Create pallet geometry resource
                CubeType pType = new CubeType();
                var resPalletGeometry = manipulator.AddResource<GeometryResource>(
                    () => new GeometryResource(pType));

                // Create pallet object
                GenericObject palletObject = manipulator.AddGeneric(resPalletGeometry);
                palletObject.Color = Color4Ex.GreenColor;
                palletObject.EnableShaderGeneratedBorder();
                palletObject.BuildAnimationSequence()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .CallAction(() => palletObject.RotationEuler = Vector3.Zero)
                    .ApplyAndRewind();
            });

            // Configure camera
            camera.Position = new Vector3(2f, 2f, 2f);
            camera.Target = new Vector3(0f, 0.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            targetRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());
        }
    }
}
