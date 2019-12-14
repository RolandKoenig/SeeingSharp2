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

using System.Threading.Tasks;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Multimedia.Views;
using SeeingSharp.Tests.Util;
using GDI = System.Drawing;

namespace SeeingSharp.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class PostprocessingTests
    {
        public const string TEST_DUMMY_FILE_NAME = "UnitTest.Screenshot.png";
        public const string TEST_CATEGORY = "SeeingSharp Multimedia Postprocessing";

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task Postprocessing_Focus()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                var camera = memRenderTarget.Camera as PerspectiveCamera3D;
                camera.Position = new Vector3(0f, 5f, -7f);
                camera.Target = new Vector3(0f, 0f, 0f);
                camera.UpdateCamera();

                // Define scene
                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    var keyPostprocess = manipulator.AddResource(
                        device => new FocusPostprocessEffectResource(false, 0f));

                    var defaultLayer = manipulator.GetLayer(Scene.DEFAULT_LAYER_NAME);
                    defaultLayer.PostprocessEffectKey = keyPostprocess;

                    var resGeometry = manipulator.AddResource(
                        device => new GeometryResource(new CubeGeometryFactory()));
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
                    screenshot, TestUtilities.LoadBitmapFromResource("Postprocessing", "PostProcess_Focus.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task Postprocessing_EdgeDetect()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                var camera = memRenderTarget.Camera as PerspectiveCamera3D;
                camera.Position = new Vector3(0f, 5f, -7f);
                camera.Target = new Vector3(0f, 0f, 0f);
                camera.UpdateCamera();

                // Define scene
                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    var keyPostprocess = manipulator.AddResource(
                        device => new EdgeDetectPostprocessEffectResource
                        {
                            Thickness = 10f
                        });

                    var defaultLayer = manipulator.GetLayer(Scene.DEFAULT_LAYER_NAME);
                    defaultLayer.PostprocessEffectKey = keyPostprocess;

                    var resGeometry = manipulator.AddResource(
                        device => new GeometryResource(new CubeGeometryFactory()));
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
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }
    }
}
