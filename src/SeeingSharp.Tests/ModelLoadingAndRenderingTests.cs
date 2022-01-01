using System.Numerics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Core;
using SeeingSharp.Drawing3D;
using SeeingSharp.Drawing3D.ImportExport;
using SeeingSharp.Mathematics;
using SeeingSharp.Views;

namespace SeeingSharp.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class ModelLoadingAndRenderingTests
    {
        [TestMethod]
        public async Task LoadAndRender_StlFile()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                var camera = (PerspectiveCamera3D)memRenderTarget.Camera;
                camera.Position = new Vector3(-4f, 4f, -4f);
                camera.Target = new Vector3(2f, 0f, 2f);
                camera.UpdateCamera();

                // Import Fox model
                var importOptions = new StlImportOptions
                {
                    ResourceCoordinateSystem = CoordinateSystem.LeftHanded_UpZ,
                    FitToCube = false
                };

                var loadedObjects = await memRenderTarget.Scene.ImportAsync(
                    TestUtilities.CreateResourceLink("Models", "Fox.stl"),
                    importOptions);

                // Wait for it to be visible
                await memRenderTarget.Scene.WaitUntilVisibleAsync(loadedObjects, memRenderTarget.RenderLoop);

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                //TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("ModelLoadingAndRendering", "ModelStl.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }
    }
}