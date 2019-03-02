#region License information
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
#endregion
#region using

using GDI = System.Drawing;

#endregion

namespace SeeingSharp.Tests
{
    #region using

    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Multimedia.Core;
    using Multimedia.Drawing3D;
    using Multimedia.Objects;
    using Multimedia.Views;
    using SeeingSharp.Util;
    using SharpDX;
    using Util;

    #endregion

    [TestClass]
    [DoNotParallelize]
    public class ModelLoadingAndRenderingTests
    {
        public const string TEST_CATEGORY = "SeeingSharp Multimedia Model Loading and Rendering";

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task LoadAndRender_StlFile()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

                // Get and configure the camera
                var camera = memRenderTarget.Camera as PerspectiveCamera3D;
                camera.Position = new Vector3(-4f, 4f, -4f);
                camera.Target = new Vector3(2f, 0f, 2f);
                camera.UpdateCamera();

                // Import Fox model
                var importOptions = new StlImportOptions
                {
                    ResourceCoordinateSystem = CoordinateSystem.LeftHanded_UpZ
                };

                IEnumerable<SceneObject> loadedObjects = await memRenderTarget.Scene.ImportAsync(
                    TestUtilities.CreateResourceLink("Models", "Fox.stl"),
                    importOptions);

                // Wait for it to be visible
                await memRenderTarget.Scene.WaitUntilVisibleAsync(loadedObjects, memRenderTarget.RenderLoop);

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                bool isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("ModelLoadingAndRendering", "ModelStl.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task LoadAndRender_ACFlatShadedObject()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

                // Get and configure the camera
                var camera = memRenderTarget.Camera as PerspectiveCamera3D;
                camera.Position = new Vector3(-3f, 3f, -3f);
                camera.Target = new Vector3(2f, 0f, 2f);
                camera.UpdateCamera();

                // Define scene
                SceneObject newObject = null;

                await memRenderTarget.Scene.ManipulateSceneAsync((manipulator) =>
                {
                    var geoResource = manipulator.AddResource<GeometryResource>(
                        () => new GeometryResource(ACFileLoader.ImportObjectType(
                            TestUtilities.CreateResourceLink("Models", "ModelFlatShading.ac"))));

                    newObject = manipulator.AddGeneric(geoResource);
                });

                await memRenderTarget.Scene.WaitUntilVisibleAsync(newObject, memRenderTarget.RenderLoop);

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                bool isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("ModelLoadingAndRendering", "FlatShadedObject.png"));
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
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

                // Get and configure the camera
                var camera = memRenderTarget.Camera as PerspectiveCamera3D;
                camera.Position = new Vector3(-1.5f, 3f, -1.5f);
                camera.Target = new Vector3(1f, 0f, 1f);
                camera.UpdateCamera();

                // Define scene
                SceneObject newObject = null;

                await memRenderTarget.Scene.ManipulateSceneAsync((manipulator) =>
                {
                    var geoResource = manipulator.AddResource<GeometryResource>(
                        () => new GeometryResource(ACFileLoader.ImportObjectType(
                            TestUtilities.CreateResourceLink("Models", "ModelShaded.ac"))));

                    newObject = manipulator.AddGeneric(geoResource);
                });
                await memRenderTarget.Scene.WaitUntilVisibleAsync(newObject, memRenderTarget.RenderLoop);

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                bool isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("ModelLoadingAndRendering", "ShadedObject.png"));
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
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

                // Get and configure the camera
                var camera = memRenderTarget.Camera as PerspectiveCamera3D;
                camera.Position = new Vector3(-1.5f, 1.5f, -1.5f);
                camera.Target = new Vector3(1f, 0f, 1f);
                camera.UpdateCamera();

                // Define scene
                SceneObject newObject = null;

                await memRenderTarget.Scene.ManipulateSceneAsync((manipulator) =>
                {
                    var geoResource = manipulator.AddResource<GeometryResource>(
                        () => new GeometryResource(ACFileLoader.ImportObjectType(
                            TestUtilities.CreateResourceLink("Models", "ModelTwoSided.ac"))));

                    newObject = manipulator.AddGeneric(geoResource);
                });

                await memRenderTarget.Scene.WaitUntilVisibleAsync(newObject, memRenderTarget.RenderLoop);

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                bool isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("ModelLoadingAndRendering", "TwoSidedObject.png"));
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
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

                // Get and configure the camera
                var camera = memRenderTarget.Camera as PerspectiveCamera3D;
                camera.Position = new Vector3(-1.5f, 1.5f, -1.5f);
                camera.Target = new Vector3(1f, 0f, 1f);
                camera.UpdateCamera();

                // Define scene
                SceneObject newObject = null;

                await memRenderTarget.Scene.ManipulateSceneAsync((manipulator) =>
                {
                    var geoResource = manipulator.AddResource<GeometryResource>(
                        () => new GeometryResource(ACFileLoader.ImportObjectType(
                            TestUtilities.CreateResourceLink("Models", "ModelSingleSided.ac"))));

                    newObject = manipulator.AddGeneric(geoResource);
                });

                await memRenderTarget.Scene.WaitUntilVisibleAsync(newObject, memRenderTarget.RenderLoop);

                // Take screenshot
                GDI.Bitmap screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                bool isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("ModelLoadingAndRendering", "SingleSidedObject.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }
    }
}