using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Views;
using SeeingSharp.Tests.Util;
using SeeingSharp.Util;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using GDI = System.Drawing;

namespace SeeingSharp.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class ModelLoadingAndRenderingTests
    {
        //public const string TEST_DUMMY_FILE_NAME = "UnitTest.Screenshot.png";
        //public const string TEST_CATEGORY = "SeeingSharp Multimedia Model Loading and Rendering";

        //[TestMethod]
        //[TestCategory(TEST_CATEGORY)]
        //public async Task LoadAndRender_StlFile()
        //{
        //    await TestUtilities.InitializeWithGrahicsAsync();

        //    using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
        //    {
        //        memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

        //        // Get and configure the camera
        //        PerspectiveCamera3D camera = memRenderTarget.Camera as PerspectiveCamera3D;
        //        camera.Position = new Vector3(-4f, 4f, -4f);
        //        camera.Target = new Vector3(2f, 0f, 2f);
        //        camera.UpdateCamera();

        //        // Import Fox model
        //        StlImportOptions importOptions = new StlImportOptions();
        //        importOptions.ResourceCoordinateSystem = CoordinateSystem.LeftHanded_UpZ;
        //        IEnumerable<SceneObject> loadedObjects = await memRenderTarget.Scene.ImportAsync(
        //            new AssemblyResourceLink(
        //                typeof(ModelLoadingAndRenderingTests),
        //                "Ressources.Models.Fox.stl"),
        //            importOptions);

        //        // Wait for it to be visible
        //        await memRenderTarget.Scene.WaitUntilVisibleAsync(loadedObjects, memRenderTarget.RenderLoop);

        //        // Take screenshot
        //        GDI.Bitmap screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();

        //        // Calculate and check difference
        //        bool isNearEqual = BitmapComparison.IsNearEqual(
        //            screenshot, Properties.Resources.ReferenceImage_ModelStl);
        //        Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
        //    }

        //    // Finishing checks
        //    Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        //}

        //[TestMethod]
        //[TestCategory(TEST_CATEGORY)]
        //public async Task LoadAndRender_ACFlatShadedObject()
        //{
        //    await TestUtilities.InitializeWithGrahicsAsync();

        //    using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
        //    {
        //        memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

        //        // Get and configure the camera
        //        PerspectiveCamera3D camera = memRenderTarget.Camera as PerspectiveCamera3D;
        //        camera.Position = new Vector3(-3f, 3f, -3f);
        //        camera.Target = new Vector3(2f, 0f, 2f);
        //        camera.UpdateCamera();

        //        // Define scene
        //        SceneObject newObject = null;
        //        await memRenderTarget.Scene.ManipulateSceneAsync((manipulator) =>
        //        {
        //            NamedOrGenericKey geoResource = manipulator.AddResource<GeometryResource>(
        //                () => new GeometryResource(ACFileLoader.ImportObjectType(Properties.Resources.ModelFlatShading)));

        //            newObject = manipulator.AddGeneric(geoResource);
        //        });
        //        await memRenderTarget.Scene.WaitUntilVisibleAsync(newObject, memRenderTarget.RenderLoop);

        //        // Take screenshot
        //        GDI.Bitmap screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();

        //        //screenshot.DumpToDesktop(TEST_DUMMY_FILE_NAME);

        //        // Calculate and check difference
        //        bool isNearEqual = BitmapComparison.IsNearEqual(
        //            screenshot, Properties.Resources.ReferenceImage_FlatShadedObject);
        //        Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
        //    }

        //    // Finishing checks
        //    Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        //}

        //[TestMethod]
        //[TestCategory(TEST_CATEGORY)]
        //public async Task LoadAndRender_ACShadedObject()
        //{
        //    await TestUtilities.InitializeWithGrahicsAsync();

        //    using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
        //    {
        //        memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

        //        // Get and configure the camera
        //        PerspectiveCamera3D camera = memRenderTarget.Camera as PerspectiveCamera3D;
        //        camera.Position = new Vector3(-1.5f, 3f, -1.5f);
        //        camera.Target = new Vector3(1f, 0f, 1f);
        //        camera.UpdateCamera();

        //        // Define scene
        //        SceneObject newObject = null;
        //        await memRenderTarget.Scene.ManipulateSceneAsync((manipulator) =>
        //        {
        //            NamedOrGenericKey geoResource = manipulator.AddResource<GeometryResource>(
        //                () => new GeometryResource(ACFileLoader.ImportObjectType(Properties.Resources.ModelShaded)));

        //            newObject = manipulator.AddGeneric(geoResource);
        //        });
        //        await memRenderTarget.Scene.WaitUntilVisibleAsync(newObject, memRenderTarget.RenderLoop);

        //        // Take screenshot
        //        GDI.Bitmap screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();

        //        //screenshot.DumpToDesktop(TEST_DUMMY_FILE_NAME);

        //        // Calculate and check difference
        //        bool isNearEqual = BitmapComparison.IsNearEqual(
        //            screenshot, Properties.Resources.ReferenceImage_ShadedObject);
        //        Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
        //    }

        //    // Finishing checks
        //    Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        //}

        //[TestMethod]
        //[TestCategory(TEST_CATEGORY)]
        //public async Task LoadAndRender_ACTwoSidedObject()
        //{
        //    await TestUtilities.InitializeWithGrahicsAsync();

        //    using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
        //    {
        //        memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

        //        // Get and configure the camera
        //        PerspectiveCamera3D camera = memRenderTarget.Camera as PerspectiveCamera3D;
        //        camera.Position = new Vector3(-1.5f, 1.5f, -1.5f);
        //        camera.Target = new Vector3(1f, 0f, 1f);
        //        camera.UpdateCamera();

        //        // Define scene
        //        SceneObject newObject = null;
        //        await memRenderTarget.Scene.ManipulateSceneAsync((manipulator) =>
        //        {
        //            NamedOrGenericKey geoResource = manipulator.AddResource<GeometryResource>(
        //                () => new GeometryResource(ACFileLoader.ImportObjectType(Properties.Resources.ModelTwoSided)));

        //            newObject = manipulator.AddGeneric(geoResource);
        //        });
        //        await memRenderTarget.Scene.WaitUntilVisibleAsync(newObject, memRenderTarget.RenderLoop);

        //        // Take screenshot
        //        GDI.Bitmap screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();

        //        //screenshot.DumpToDesktop(TEST_DUMMY_FILE_NAME);

        //        // Calculate and check difference
        //        bool isNearEqual = BitmapComparison.IsNearEqual(
        //            screenshot, Properties.Resources.ReferenceImage_TwoSidedObject);
        //        Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
        //    }

        //    // Finishing checks
        //    Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        //}

        //[TestMethod]
        //[TestCategory(TEST_CATEGORY)]
        //public async Task LoadAndRender_ACSingleSidedObject()
        //{
        //    await TestUtilities.InitializeWithGrahicsAsync();

        //    using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
        //    {
        //        memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

        //        // Get and configure the camera
        //        PerspectiveCamera3D camera = memRenderTarget.Camera as PerspectiveCamera3D;
        //        camera.Position = new Vector3(-1.5f, 1.5f, -1.5f);
        //        camera.Target = new Vector3(1f, 0f, 1f);
        //        camera.UpdateCamera();

        //        // Define scene
        //        SceneObject newObject = null;
        //        await memRenderTarget.Scene.ManipulateSceneAsync((manipulator) =>
        //        {
        //            NamedOrGenericKey geoResource = manipulator.AddResource<GeometryResource>(
        //                () => new GeometryResource(ACFileLoader.ImportObjectType(Properties.Resources.ModelSingleSided)));

        //            newObject = manipulator.AddGeneric(geoResource);
        //        });
        //        await memRenderTarget.Scene.WaitUntilVisibleAsync(newObject, memRenderTarget.RenderLoop);

        //        // Take screenshot
        //        GDI.Bitmap screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();

        //        //screenshot.DumpToDesktop(TEST_DUMMY_FILE_NAME);

        //        // Calculate and check difference
        //        bool isNearEqual = BitmapComparison.IsNearEqual(
        //            screenshot, Properties.Resources.ReferenceImage_SingleSidedObject);
        //        Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
        //    }

        //    // Finishing checks
        //    Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        //}

        //[TestMethod]
        //[TestCategory(TEST_CATEGORY)]
        //public async Task LoadAndRender_ACTexturedObject()
        //{
        //    await TestUtilities.InitializeWithGrahicsAsync();

        //    using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
        //    {
        //        memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

        //        // Get and configure the camera
        //        PerspectiveCamera3D camera = memRenderTarget.Camera as PerspectiveCamera3D;
        //        camera.Position = new Vector3(-1f, 1f, -1f);
        //        camera.Target = new Vector3(0f, 0f, 0f);
        //        camera.UpdateCamera();

        //        // Define scene
        //        SceneObject newObject = null;
        //        await memRenderTarget.Scene.ManipulateSceneAsync((manipulator) =>
        //        {
        //            NamedOrGenericKey geoResource = manipulator.AddResource<GeometryResource>(
        //                () => new GeometryResource(ACFileLoader.ImportObjectType(new AssemblyResourceLink(
        //                    Assembly.GetExecutingAssembly(),
        //                    "SeeingSharp.Tests.Rendering.Ressources.Models",
        //                    "ModelTextured.ac"))));

        //            newObject = manipulator.AddGeneric(geoResource);
        //        });
        //        await memRenderTarget.Scene.WaitUntilVisibleAsync(newObject, memRenderTarget.RenderLoop);

        //        // Take screenshot
        //        GDI.Bitmap screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();

        //        //screenshot.DumpToDesktop(TEST_DUMMY_FILE_NAME);

        //        // Calculate and check difference
        //        bool isNearEqual = BitmapComparison.IsNearEqual(
        //            screenshot, Properties.Resources.ReferenceImage_TexturedObject);
        //        Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
        //    }

        //    // Finishing checks
        //    Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        //}

        //[TestMethod]
        //[TestCategory(TEST_CATEGORY)]
        //public async Task ImportAndRender_ACShadedObject()
        //{
        //    await TestUtilities.InitializeWithGrahicsAsync();

        //    IEnumerable<SceneObject> importedObjects = null;

        //    using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
        //    {
        //        memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

        //        // Get and configure the camera
        //        PerspectiveCamera3D camera = memRenderTarget.Camera as PerspectiveCamera3D;
        //        camera.Position = new Vector3(-1.5f, 3f, -1.5f);
        //        camera.Target = new Vector3(1f, 0f, 1f);
        //        camera.UpdateCamera();

        //        ResourceLink objSource = new AssemblyResourceLink(
        //            typeof(ModelLoadingAndRenderingTests),
        //            "Ressources.Models.ModelShaded.ac");
        //        importedObjects = await memRenderTarget.Scene.ImportAsync(objSource);

        //        // Wait until ac file is completely loaded
        //        await memRenderTarget.Scene.WaitUntilVisibleAsync(importedObjects, memRenderTarget.RenderLoop);

        //        // Take screenshot
        //        GDI.Bitmap screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();

        //        //screenshot.DumpToDesktop(TEST_DUMMY_FILE_NAME);

        //        // Calculate and check difference
        //        bool isNearEqual = BitmapComparison.IsNearEqual(
        //            screenshot, Properties.Resources.ReferenceImage_ShadedObject);
        //        Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
        //    }

        //    // Finishing checks
        //    Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        //    Assert.IsNotNull(importedObjects);
        //    Assert.IsTrue(importedObjects.Count() == 1);
        //}
    }
}
