﻿using System.Numerics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Core;
using SeeingSharp.Drawing3D;
using SeeingSharp.Drawing3D.Primitives;
using SeeingSharp.Drawing3D.Resources;
using SeeingSharp.Mathematics;
using SeeingSharp.Views;

namespace SeeingSharp.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class PostprocessingTests
    {
        public const string TEST_DUMMY_FILE_NAME = "UnitTest.Screenshot.png";

        [TestMethod]
        public async Task Postprocessing_Focus()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                var camera = (PerspectiveCamera3D)memRenderTarget.Camera;
                camera.Position = new Vector3(0f, 5f, -7f);
                camera.Target = new Vector3(0f, 0f, 0f);
                camera.UpdateCamera();

                // Define scene
                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    var keyPostprocess = manipulator.AddResource(
                        _ => new FocusPostprocessEffectResource(false, 0f));

                    var defaultLayer = manipulator.GetLayer(Scene.DEFAULT_LAYER_NAME);
                    var focusLayer = manipulator.AddLayer("Focus");
                    focusLayer.PostprocessEffectKey = keyPostprocess;

                    var resGeometry = manipulator.AddResource(
                        _ => new GeometryResource(new CubeGeometryFactory()));
                    var resMaterial = manipulator.AddStandardMaterialResource();

                    var frontMesh = manipulator.AddMeshObject(resGeometry, defaultLayer.Name, resMaterial);
                    frontMesh.Color = Color4.BlueColor;
                    frontMesh.Scaling = new Vector3(1f, 0.5f, 0.5f);
                    frontMesh.Position = new Vector3(0.5f, 2f, -3f);

                    var backMesh = manipulator.AddMeshObject(resGeometry, defaultLayer.Name, resMaterial);
                    backMesh.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 3f, 0f);
                    backMesh.Scaling = new Vector3(2f, 2f, 2f);
                    backMesh.Color = Color4.RedColor;

                    var focusMesh = manipulator.AddMeshObject(resGeometry, focusLayer.Name, resMaterial);
                    focusMesh.TransformSourceObject = backMesh;
                    focusMesh.TransformationType = SpacialTransformationType.TakeFromOtherObject;
                    focusMesh.Color = Color4.RedColor;
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("Postprocessing", "PostProcess_Focus.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop!.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        public async Task Postprocessing_EdgeDetect()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                var camera = (PerspectiveCamera3D)memRenderTarget.Camera;
                camera.Position = new Vector3(0f, 5f, -7f);
                camera.Target = new Vector3(0f, 0f, 0f);
                camera.UpdateCamera();

                // Define scene
                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    var keyPostprocess = manipulator.AddResource(
                        _ => new EdgeDetectPostprocessEffectResource
                        {
                            Thickness = 10f
                        });

                    var defaultLayer = manipulator.GetLayer(Scene.DEFAULT_LAYER_NAME);
                    defaultLayer.PostprocessEffectKey = keyPostprocess;

                    var resGeometry = manipulator.AddResource(
                        _ => new GeometryResource(new CubeGeometryFactory()));
                    var resMaterial = manipulator.AddStandardMaterialResource();

                    var newMesh = manipulator.AddMeshObject(resGeometry, resMaterial);
                    newMesh.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
                    newMesh.Scaling = new Vector3(2f, 2f, 2f);
                    newMesh.Color = Color4.RedColor;
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();

                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("Postprocessing", "PostProcess_EdgeDetect.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop!.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }
    }
}
