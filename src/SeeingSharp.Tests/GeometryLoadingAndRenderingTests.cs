using System.Numerics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Core;
using SeeingSharp.Drawing3D;
using SeeingSharp.Views;

namespace SeeingSharp.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class GeometryLoadingAndRenderingTests
    {
        [TestMethod]
        public async Task LoadAndRender_ACFlatShadedObject()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                var camera = (PerspectiveCamera3D)memRenderTarget.Camera;
                camera.Position = new Vector3(-3f, 3f, -3f);
                camera.Target = new Vector3(2f, 0f, 2f);
                camera.UpdateCamera();

                // Define scene
                SceneObject newObject = null;

                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    var resGeometry = manipulator.AddResource(
                        device => new GeometryResource(ACFileLoader.ImportGeometry(
                            TestUtilities.CreateResourceLink("Models", "ModelFlatShading.ac"))));
                    var resMaterial = manipulator.AddStandardMaterialResource();

                    newObject = manipulator.AddMeshObject(resGeometry, resMaterial);
                });

                await memRenderTarget.Scene.WaitUntilVisibleAsync(newObject, memRenderTarget.RenderLoop);

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                //TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("GeometryLoadingAndRendering", "FlatShadedObject.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        public async Task LoadAndRender_ACShadedObject()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                var camera = (PerspectiveCamera3D)memRenderTarget.Camera;
                camera.Position = new Vector3(-1.5f, 3f, -1.5f);
                camera.Target = new Vector3(1f, -1f, 1f);
                camera.UpdateCamera();

                // Define scene
                SceneSpacialObject newMesh = null;
                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    var resGeometry = manipulator.AddResource(
                        device => new GeometryResource(ACFileLoader.ImportGeometry(
                            TestUtilities.CreateResourceLink("Models", "ModelShaded.ac"))));
                    var resMaterial = manipulator.AddStandardMaterialResource();

                    newMesh = manipulator.AddMeshObject(resGeometry, resMaterial);
                    newMesh.Scaling = new Vector3(0.5f, 0.5f, 0.5f);
                });
                await memRenderTarget.Scene.WaitUntilVisibleAsync(newMesh, memRenderTarget.RenderLoop);

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                //TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("GeometryLoadingAndRendering", "ShadedObject.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        public async Task LoadAndRender_ACTwoSidedObject()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                var camera = (PerspectiveCamera3D)memRenderTarget.Camera;
                camera.Position = new Vector3(-1.5f, 1.5f, -1.5f);
                camera.Target = new Vector3(1f, 0f, 1f);
                camera.UpdateCamera();

                // Define scene
                SceneObject newObject = null;

                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    var resGeometry = manipulator.AddResource(
                        device => new GeometryResource(ACFileLoader.ImportGeometry(
                            TestUtilities.CreateResourceLink("Models", "ModelTwoSided.ac"))));
                    var resMaterial = manipulator.AddStandardMaterialResource();

                    newObject = manipulator.AddMeshObject(resGeometry, resMaterial);
                });

                await memRenderTarget.Scene.WaitUntilVisibleAsync(newObject, memRenderTarget.RenderLoop);

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("GeometryLoadingAndRendering", "TwoSidedObject.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        public async Task LoadAndRender_ACSingleSidedObject()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                var camera = (PerspectiveCamera3D)memRenderTarget.Camera;
                camera.Position = new Vector3(-1.5f, 1.5f, -1.5f);
                camera.Target = new Vector3(1f, 0f, 1f);
                camera.UpdateCamera();

                // Define scene
                SceneObject newObject = null;

                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    var resGeometry = manipulator.AddResource(
                        device => new GeometryResource(ACFileLoader.ImportGeometry(
                            TestUtilities.CreateResourceLink("Models", "ModelSingleSided.ac"))));
                    var resMaterial = manipulator.AddStandardMaterialResource();

                    newObject = manipulator.AddMeshObject(resGeometry, resMaterial);
                });

                await memRenderTarget.Scene.WaitUntilVisibleAsync(newObject, memRenderTarget.RenderLoop);

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("GeometryLoadingAndRendering", "SingleSidedObject.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }
    }
}