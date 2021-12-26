using System;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Core;
using SeeingSharp.Core.Animations;
using SeeingSharp.Drawing3D;
using SeeingSharp.Drawing3D.Primitives;
using SeeingSharp.Mathematics;
using SeeingSharp.Views;
using SeeingSharp.Util;

namespace SeeingSharp.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class Drawing3DTests
    {
        [TestMethod]
        public async Task Render_ClearedScreen()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4.CornflowerBlue;
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                //TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing3D", "ClearedScreen.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        public async Task Render_SimpleLine()
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
                    var wireObject = new WireObject
                    {
                        LineData = new[]
                        {
                            new Line(
                                new Vector3(-0.5f, 0f, -0.5f),
                                new Vector3(0.5f, 0f, -0.5f)),
                            new Line(
                                new Vector3(0.5f, 0f, -0.5f),
                                new Vector3(0.5f, 0f, 0.5f)),
                            new Line(
                                new Vector3(0.5f, 0f, 0.5f),
                                new Vector3(-0.5f, 0f, 0.5f)),
                            new Line(
                                new Vector3(-0.5f, 0f, 0.5f),
                                new Vector3(-0.5f, 0f, -0.5f))
                        },
                        Color = Color4.RedColor
                    };


                    manipulator.AddObject(wireObject);
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing3D", "SimpleLine.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }
        }

        [TestMethod]
        public async Task Render_SimpleObject()
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
                    var resGeometry = manipulator.AddResource(
                        device => new GeometryResource(new CubeGeometryFactory()));
                    var resMaterial = manipulator.AddStandardMaterialResource();

                    var newMesh = manipulator.AddMeshObject(resGeometry, resMaterial);
                    newMesh.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
                    newMesh.Scaling = new Vector3(2f, 2f, 2f);
                    newMesh.Color = Color4.RedColor;
                    newMesh.TrySetInitialVisibility(memRenderTarget.RenderLoop.ViewInformation, true);
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing3D", "SimpleObject.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        public async Task QueryInfo_FromSimpleMesh()
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
                Mesh newMesh = null;
                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    var resGeometry = manipulator.AddResource(
                        device => new GeometryResource(new CubeGeometryFactory()));
                    var resMaterial = manipulator.AddStandardMaterialResource();

                    newMesh = manipulator.AddMeshObject(resGeometry, resMaterial);
                    newMesh.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
                    newMesh.Scaling = new Vector3(2f, 2f, 2f);
                    newMesh.Color = Color4.RedColor;
                    newMesh.TrySetInitialVisibility(memRenderTarget.RenderLoop.ViewInformation, true);
                });
                await memRenderTarget.AwaitRenderAsync();

                // Query some information from the mesh
                var renderingChunkCount = newMesh.TryGetRenderingChunkCount(memRenderTarget.Device);
                var geoResource = newMesh.TryGetGeometryResource(memRenderTarget.Device);
                var materialResources = newMesh.TryGetMaterialResources(memRenderTarget.Device);

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing3D", "SimpleObject.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");

                // Check info from mesh
                Assert.IsTrue(renderingChunkCount == 1, "Invalid count of rendering chunks");
                Assert.IsTrue(geoResource != null, "Can not query GeometryResource");
                Assert.IsTrue((materialResources != null) && (materialResources.Length == 1), "Can not query MaterialResource");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        public async Task Render_FullScreenTexture()
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
                    var sourceBackgroundTexture = new AssemblyResourceLink(
                        typeof(Drawing3DTests),
                        "Resources.Textures", "Background.png");

                    var resBackgroundTexture = manipulator.AddTextureResource(sourceBackgroundTexture);
                    manipulator.AddObject(new FullscreenTexture(resBackgroundTexture));
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing3D", "FullscreenTexture.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        public async Task Render_SimpleObject_DiffuseColorFromMaterial()
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
                    var resGeometry = manipulator.AddResource(
                        device => new GeometryResource(new CubeGeometryFactory()));
                    var resMaterial = manipulator.AddResource(
                        device => new StandardMaterialResource
                        {
                            MaterialDiffuseColor = Color4.BlueColor,
                            UseVertexColors = false
                        });

                    var newMesh = manipulator.AddMeshObject(resGeometry, resMaterial);
                    newMesh.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
                    newMesh.Scaling = new Vector3(2f, 2f, 2f);
                    newMesh.Color = Color4.RedColor;
                    newMesh.TrySetInitialVisibility(memRenderTarget.RenderLoop.ViewInformation, true);
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing3D", "SimpleObject_DiffuseColorFromMaterial.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        public async Task Render_SimpleObject_StackedGeometry()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                var camera = (PerspectiveCamera3D)memRenderTarget.Camera;
                camera.Position = new Vector3(0f, 5f, -7f);
                camera.Target = new Vector3(0f, 1f, 0f);
                camera.UpdateCamera();

                // Define scene
                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    var cubeType = new CubeGeometryFactory
                    {
                         Width = 0.3f,
                         Height = 0.3f,
                         Depth = 0.3f
                    };
                    var stackedType = new StackedGeometryFactory(cubeType, 10);

                    var resGeometry = manipulator.AddResource(
                        device => new GeometryResource(stackedType));
                    var resMaterial = manipulator.AddStandardMaterialResource();

                    var newMesh = manipulator.AddMeshObject(resGeometry, resMaterial);
                    newMesh.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
                    newMesh.Scaling = new Vector3(5f, 1f, 5f);
                    newMesh.TrySetInitialVisibility(memRenderTarget.RenderLoop.ViewInformation, true);
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing3D", "SimpleObject_StackedGeometry.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        public async Task Render_SimpleObject_Transparent()
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
                    var resGeometry = manipulator.AddResource(
                        device => new GeometryResource(new CubeGeometryFactory()));
                    var resMaterial = manipulator.AddStandardMaterialResource();

                    var newMesh = manipulator.AddMeshObject(resGeometry, resMaterial);
                    newMesh.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
                    newMesh.Scaling = new Vector3(2f, 2f, 2f);
                    newMesh.Opacity = 0.5f;
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing3D", "SimpleObject_Transparent.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        public async Task Render_SimpleObject_Orthographic()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                var camera = new OrthographicCamera3D
                {
                    Position = new Vector3(0f, 5f, -7f),
                    Target = new Vector3(0f, 1f, 0f),
                    ZoomFactor = 200f
                };

                camera.UpdateCamera();
                memRenderTarget.RenderLoop.Camera = camera;

                // Define scene
                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    var resGeometry = manipulator.AddResource(
                        device => new GeometryResource(new CubeGeometryFactory()));
                    var resMaterial = manipulator.AddStandardMaterialResource();

                    var newMesh = manipulator.AddMeshObject(resGeometry, resMaterial);
                    newMesh.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
                    newMesh.Scaling = new Vector3(2f, 2f, 2f);
                    newMesh.TrySetInitialVisibility(memRenderTarget.RenderLoop.ViewInformation, true);
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing3D", "SimpleObject_Ortho.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        public async Task Render_Skybox()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                var camera = (PerspectiveCamera3D)memRenderTarget.Camera;
                camera.Position = new Vector3(-3f, -3f, -7f);
                camera.Target = new Vector3(0f, 0f, 0f);
                camera.UpdateCamera();

                // Define scene
                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    // Create pallet geometry resource
                    var cubeType = new CubeGeometryFactory();
                    var resGeometry = manipulator.AddResource(
                        device => new GeometryResource(cubeType));
                    var resMaterial = manipulator.AddResource(
                        device => new StandardMaterialResource(enableShaderGeneratedBorder: true));

                    // Create pallet object
                    var cubeMesh = manipulator.AddMeshObject(resGeometry, resMaterial);
                    cubeMesh.Color = Color4.GreenColor;
                    cubeMesh.BuildAnimationSequence()
                        .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                        .WaitFinished()
                        .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                        .WaitFinished()
                        .CallAction(() => cubeMesh.RotationEuler = Vector3.Zero)
                        .ApplyAndRewind();

                    var resSkyboxTexture = manipulator.AddTextureResource(TestUtilities.CreateResourceLink("Textures", "SkyBox.dds"));

                    // Create the skybox on a new layer
                    manipulator.AddLayer("Skybox");
                    var skyboxObject = new Skybox(resSkyboxTexture);
                    manipulator.AddObject(skyboxObject, "Skybox");
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing3D", "Skybox.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }
    }
}