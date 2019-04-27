/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Multimedia.Views;
using SeeingSharp.Tests.Util;
using SharpDX;
using GDI = System.Drawing;

namespace SeeingSharp.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class Drawing3DTests
    {
        public const string TEST_CATEGORY = "SeeingSharp Multimedia Drawing 3D";

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task Render_ClearedScreen()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing3D", "ClearedScreen.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task Render_SimpleLine()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

                // Get and configure the camera
                var camera = memRenderTarget.Camera as PerspectiveCamera3D;
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
                        LineColor = Color4Ex.RedColor
                    };


                    manipulator.Add(wireObject);
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
        [TestCategory(TEST_CATEGORY)]
        public async Task Render_SimpleObject()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

                // Get and configure the camera
                var camera = memRenderTarget.Camera as PerspectiveCamera3D;
                camera.Position = new Vector3(0f, 5f, -7f);
                camera.Target = new Vector3(0f, 0f, 0f);
                camera.UpdateCamera();

                // Define scene
                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    var resGeometry = manipulator.AddResource(
                        () => new GeometryResource(new CubeGeometryFactory()));
                    var resMaterial = manipulator.AddSimpleColoredMaterial();

                    var newMesh = manipulator.AddMesh(resGeometry, resMaterial);
                    newMesh.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
                    newMesh.Scaling = new Vector3(2f, 2f, 2f);
                    newMesh.Color = Color4Ex.RedColor;
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
        [TestCategory(TEST_CATEGORY)]
        public async Task Render_SimpleObject_DiffuseColorFromMaterial()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

                // Get and configure the camera
                var camera = memRenderTarget.Camera as PerspectiveCamera3D;
                camera.Position = new Vector3(0f, 5f, -7f);
                camera.Target = new Vector3(0f, 0f, 0f);
                camera.UpdateCamera();

                // Define scene
                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    var resGeometry = manipulator.AddResource(
                        () => new GeometryResource(new CubeGeometryFactory()));
                    var resMaterial = manipulator.AddResource(
                        () => new SimpleColoredMaterialResource()
                        {
                            MaterialDiffuseColor = Color4Ex.BlueColor,
                            UseVertexColors = false
                        });

                    var newMesh = manipulator.AddMesh(resGeometry, resMaterial);
                    newMesh.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
                    newMesh.Scaling = new Vector3(2f, 2f, 2f);
                    newMesh.Color = Color4Ex.RedColor;
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
        [TestCategory(TEST_CATEGORY)]
        public async Task Render_SimpleObject_StackedGeometry()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

                // Get and configure the camera
                var camera = memRenderTarget.Camera as PerspectiveCamera3D;
                camera.Position = new Vector3(0f, 5f, -7f);
                camera.Target = new Vector3(0f, 1f, 0f);
                camera.UpdateCamera();

                // Define scene
                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    var cubeType = new CubeGeometryFactory
                    {
                        Size = 0.3f
                    };
                    var stackedType = new StackedGeometryFactory(cubeType, 10);

                    var resGeometry = manipulator.AddResource(
                        () => new GeometryResource(stackedType));
                    var resMaterial = manipulator.AddSimpleColoredMaterial();

                    var newMesh = manipulator.AddMesh(resGeometry, resMaterial);
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
        [TestCategory(TEST_CATEGORY)]
        public async Task Render_SimpleObject_Transparent()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

                // Get and configure the camera
                var camera = memRenderTarget.Camera as PerspectiveCamera3D;
                camera.Position = new Vector3(0f, 5f, -7f);
                camera.Target = new Vector3(0f, 0f, 0f);
                camera.UpdateCamera();

                // Define scene
                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    var resGeometry = manipulator.AddResource(
                        () => new GeometryResource(new CubeGeometryFactory()));
                    var resMaterial = manipulator.AddSimpleColoredMaterial();

                    var newMesh = manipulator.AddMesh(resGeometry, resMaterial);
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
        [TestCategory(TEST_CATEGORY)]
        public async Task Render_SimpleObject_Orthographic()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

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
                        () => new GeometryResource(new CubeGeometryFactory()));
                    var resMaterial = manipulator.AddSimpleColoredMaterial();

                    var newMesh = manipulator.AddMesh(resGeometry, resMaterial);
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
        [TestCategory(TEST_CATEGORY)]
        public async Task Render_Skybox()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

                // Get and configure the camera
                var camera = memRenderTarget.Camera as PerspectiveCamera3D;
                camera.Position = new Vector3(-3f, -3f, -7f);
                camera.Target = new Vector3(0f, 0f, 0f);
                camera.UpdateCamera();

                // Define scene
                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    // Create pallet geometry resource
                    var cubeType = new CubeGeometryFactory();
                    var resGeometry = manipulator.AddResource(
                        () => new GeometryResource(cubeType));
                    var resMaterial = manipulator.AddSimpleColoredMaterial();

                    // Create pallet object
                    var cubeMesh = manipulator.AddMesh(resGeometry, resMaterial);
                    cubeMesh.Color = Color4Ex.GreenColor;
                    cubeMesh.EnableShaderGeneratedBorder();
                    cubeMesh.BuildAnimationSequence()
                        .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                        .WaitFinished()
                        .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                        .WaitFinished()
                        .CallAction(() => cubeMesh.RotationEuler = Vector3.Zero)
                        .ApplyAndRewind();

                    var resSkyboxTexture = manipulator.AddTexture(TestUtilities.CreateResourceLink("Textures", "SkyBox.dds"));

                    // Create the skybox on a new layer
                    manipulator.AddLayer("Skybox");
                    var skyboxObject = new Skybox(resSkyboxTexture);
                    manipulator.Add(skyboxObject, "Skybox");
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