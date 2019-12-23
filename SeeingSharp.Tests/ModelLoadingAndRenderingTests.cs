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
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Multimedia.Views;
using SeeingSharp.Tests.Util;
using System.Numerics;
using System.Threading.Tasks;

namespace SeeingSharp.Tests
{
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
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

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

                var loadedObjects = await memRenderTarget.Scene.ImportAsync(
                    TestUtilities.CreateResourceLink("Models", "Fox.stl"),
                    importOptions);

                // Wait for it to be visible
                await memRenderTarget.Scene.WaitUntilVisibleAsync(loadedObjects, memRenderTarget.RenderLoop);

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

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