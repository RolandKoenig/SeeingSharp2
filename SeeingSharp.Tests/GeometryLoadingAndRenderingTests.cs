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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Views;
using SeeingSharp.Tests.Util;
using System.Numerics;
using System.Threading.Tasks;

namespace SeeingSharp.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class GeometryLoadingAndRenderingTests
    {
        public const string TEST_CATEGORY = "SeeingSharp Multimedia Geometry Loading and Rendering";

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task LoadAndRender_ACFlatShadedObject()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                var camera = memRenderTarget.Camera as PerspectiveCamera3D;
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
        [TestCategory(TEST_CATEGORY)]
        public async Task LoadAndRender_ACShadedObject()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                var camera = memRenderTarget.Camera as PerspectiveCamera3D;
                camera.Position = new Vector3(-1.5f, 3f, -1.5f);
                camera.Target = new Vector3(1f, 0f, 1f);
                camera.UpdateCamera();

                // Define scene
                SceneObject newMesh = null;
                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    var resGeometry = manipulator.AddResource(
                        device => new GeometryResource(ACFileLoader.ImportGeometry(
                            TestUtilities.CreateResourceLink("Models", "ModelShaded.ac"))));
                    var resMaterial = manipulator.AddStandardMaterialResource();

                    newMesh = manipulator.AddMeshObject(resGeometry, resMaterial);
                });
                await memRenderTarget.Scene.WaitUntilVisibleAsync(newMesh, memRenderTarget.RenderLoop);

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("GeometryLoadingAndRendering", "ShadedObject.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task LoadAndRender_ACTwoSidedObject()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                var camera = memRenderTarget.Camera as PerspectiveCamera3D;
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
        [TestCategory(TEST_CATEGORY)]
        public async Task LoadAndRender_ACSingleSidedObject()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                var camera = memRenderTarget.Camera as PerspectiveCamera3D;
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