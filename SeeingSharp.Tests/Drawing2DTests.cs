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
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Multimedia.Views;
using SeeingSharp.Tests.Util;
using SeeingSharp.Util;
using SharpDX;
using SharpDX.Direct2D1;
using GDI = System.Drawing;

namespace SeeingSharp.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class Drawing2DTests
    {
        public const string TEST_CATEGORY = "SeeingSharp Multimedia Drawing 2D";

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task Collision_Ellipse_to_Ellipse()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            var ellipseGeometry01 = new EllipseGeometryResource(
                new Vector2(50, 50), 10f, 10f);
            var ellipseGeometry02 = new EllipseGeometryResource(
                new Vector2(50, 80), 10f, 10f);
            var ellipseGeometry03 = new EllipseGeometryResource(
                new Vector2(50, 70), 10f, 11f);
            try
            {
                Assert.IsFalse(ellipseGeometry01.IntersectsWith(ellipseGeometry02));
                Assert.IsTrue(ellipseGeometry03.IntersectsWith(ellipseGeometry02));
                Assert.IsTrue(ellipseGeometry02.IntersectsWith(ellipseGeometry03));
            }
            finally
            {
                SeeingSharpUtil.SafeDispose(ref ellipseGeometry01);
                SeeingSharpUtil.SafeDispose(ref ellipseGeometry02);
                SeeingSharpUtil.SafeDispose(ref ellipseGeometry03);
            }
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task Collision_Ellipse_to_Polygon()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            var ellipseGeometry01 = new EllipseGeometryResource(
                new Vector2(50, 50), 10f, 10f);
            var ellipseGeometry02 = new EllipseGeometryResource(
                new Vector2(50, 80), 10f, 10f);

            var polygonGeometry = new PolygonGeometryResource(new Polygon2D(new Vector2(55f, 50f), new Vector2(60f, 50f), new Vector2(55f, 55f)));

            try
            {
                Assert.IsTrue(ellipseGeometry01.IntersectsWith(polygonGeometry));
                Assert.IsFalse(ellipseGeometry02.IntersectsWith(polygonGeometry));
            }
            finally
            {
                SeeingSharpUtil.SafeDispose(ref ellipseGeometry01);
                SeeingSharpUtil.SafeDispose(ref ellipseGeometry02);
                SeeingSharpUtil.SafeDispose(ref polygonGeometry);
            }
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task Render_SimpleText_SimpleSingleColor()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var solidBrush = new SolidBrushResource(Color4Ex.RedColor))
            using (var textFormat = new TextFormatResource("Arial", 70))
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;
                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync(graphics =>
                {
                    // 2D rendering is made here
                    graphics.DrawText(
                        $"Just a dummy text ;){Environment.NewLine}Just a dummy text ;)",
                        textFormat,
                        new RectangleF(10, 10, 512, 512),
                        solidBrush);
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing2D", "SimpleText_SingleColor.png"));
                Assert.IsTrue(diff < 0.2, "Difference to reference image is to big!");
            }
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task Render_SimpleRoundedRect_Filled_Solid()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var solidBrush = new SolidBrushResource(Color4Ex.Gray))
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;
                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync(graphics =>
                {
                    // 2D rendering is made here
                    graphics.FillRoundedRectangle(
                        new RectangleF(10, 10, 512, 512), 30, 30,
                        solidBrush);
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing2D", "SimpleRoundedRectFilled.png"));
                Assert.IsTrue(diff < 0.2, "Difference to reference image is to big!");
            }
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task Render_SimpleGeometry()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            var polygon = new Polygon2D(new Vector2(10, 10), new Vector2(900, 100), new Vector2(800, 924), new Vector2(50, 1014), new Vector2(10, 10));

            using (var solidBrush = new SolidBrushResource(Color4Ex.LightGray))
            using (var solidBrushBorder = new SolidBrushResource(Color4Ex.Gray))
            using (var polygonGeometry = new PolygonGeometryResource(polygon))
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;
                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync(graphics =>
                {
                    // 2D rendering is made here
                    graphics.DrawGeometry(polygonGeometry, solidBrushBorder, 3f);
                    graphics.FillGeometry(polygonGeometry, solidBrush);
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing2D", "SimpleGeometry2D.png"));
                Assert.IsTrue(diff < 0.2, "Difference to reference image is to big!");
            }
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task Render_SimpleGeometry_Ellipse()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var solidBrush = new SolidBrushResource(Color4Ex.LightGray))
            using (var solidBrushBorder = new SolidBrushResource(Color4Ex.Gray))
            using (var ellipseGeometry = new EllipseGeometryResource(new Vector2(512, 512), 400f, 300f))
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync(graphics =>
                {
                    // 2D rendering is made here
                    graphics.DrawGeometry(ellipseGeometry, solidBrushBorder, 3f);
                    graphics.FillGeometry(ellipseGeometry, solidBrush);
                });

                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing2D", "SimpleGeometry2D_Ellipse.png"));
                Assert.IsTrue(diff < 0.2, "Difference to reference image is to big!");
            }
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task Render_SimpleRoundedRect_Filled_LinearGradient()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var gradientBrush = new LinearGradientBrushResource(
                new Vector2(0f, 0f),
                new Vector2(512f, 0f),
                new[]
                {
                    new GradientStop { Color = Color4Ex.Gray, Position = 0f },
                    new GradientStop { Color = Color4Ex.White, Position = 0.6f },
                    new GradientStop { Color = Color4Ex.Black, Position = 1f }
                },
                ExtendMode.Mirror))

            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync(graphics =>
                {
                    // 2D rendering is made here
                    graphics.FillRoundedRectangle(
                        new RectangleF(10, 10, 900, 900), 30, 30,
                        gradientBrush);
                });

                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing2D", "SimpleRoundedRectFilled_LinearGradient.png"));
                Assert.IsTrue(diff < 0.2, "Difference to reference image is to big!");
            }
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task Render_SimpleRoundedRect_Filled_RadialGradient()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var radialGradientBrush = new RadialGradientBrushResource(
                new Vector2(200f, 400f),
                new Vector2(0f, 0f),
                200f, 400f,
                new[]
                {
                    new GradientStop { Color = Color4Ex.Gray, Position = 0f },
                    new GradientStop { Color = Color4Ex.White, Position = 0.6f },
                    new GradientStop { Color = Color4Ex.BlueColor, Position = 1f }
                },
                ExtendMode.Clamp))
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;
                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync(graphics =>
                {
                    // 2D rendering is made here
                    graphics.FillRoundedRectangle(
                        new RectangleF(10, 10, 900, 900), 30, 30,
                        radialGradientBrush);
                });
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing2D", "SimpleRoundedRectFilled_RadialGradient.png"));
                Assert.IsTrue(diff < 0.2, "Difference to reference image is to big!");
            }
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task Render_SimpleRoundedRect_Filled_Over3D()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            var brushColor = Color4Ex.Gray;
            brushColor.ChangeAlphaTo(0.5f);

            using (var solidBrush = new SolidBrushResource(brushColor))
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

                // Get and configure the camera
                var camera = memRenderTarget.Camera as PerspectiveCamera3D;
                camera.Position = new Vector3(0f, 5f, -5f);
                camera.Target = new Vector3(0f, 1f, 0f);
                camera.UpdateCamera();

                // Define scene
                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    // Define object
                    var resGeometry = manipulator.AddResource(
                        device => new GeometryResource(new CubeGeometryFactory()));
                    var resMaterial = manipulator.AddStandardMaterialResource();

                    var newMesh = manipulator.AddMeshObject(resGeometry, resMaterial);
                    newMesh.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
                    newMesh.Scaling = new Vector3(2f, 2f, 2f);
                    newMesh.Color = Color4Ex.Goldenrod;
                    newMesh.EnableShaderGeneratedBorder();
                });

                // Define 2D overlay
                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync(graphics =>
                {
                    // 2D rendering is made here
                    graphics.FillRoundedRectangle(
                        new RectangleF(10, 10, 512, 512), 30, 30,
                        solidBrush);
                });

                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing2D", "RoundedRectOver3D.png"));
                Assert.IsTrue(diff < 0.2, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task Render_DebugLayer()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var debugLayer = new DebugDrawingLayer())
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;
                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync(debugLayer);
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing2D", "DebugDrawingLayer.png"));
                Assert.IsTrue(diff < 0.2, "Difference to reference image is to big!");
            }
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task Render_SimpleObject_D2D_Texture()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var solidBrush = new SolidBrushResource(Color4Ex.Gray))
            using (var textFormat = new TextFormatResource("Arial", 36))
            using (var textBrush = new SolidBrushResource(Color4Ex.RedColor))
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

                // Get and configure the camera
                var camera = memRenderTarget.Camera as PerspectiveCamera3D;
                camera.Position = new Vector3(0f, 5f, -7f);
                camera.Target = new Vector3(0f, 0f, 0f);
                camera.UpdateCamera();

                // 2D rendering is made here
                var d2dDrawingLayer = new Custom2DDrawingLayer(graphics =>
                {
                    var d2dRectangle = new RectangleF(10, 10, 236, 236);
                    graphics.Clear(Color4Ex.LightBlue);
                    graphics.FillRoundedRectangle(
                        d2dRectangle, 30, 30,
                        solidBrush);

                    d2dRectangle.Inflate(-10, -10);
                    graphics.DrawText("Hello Direct2D!", textFormat, d2dRectangle, textBrush);
                });

                // Define scene
                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    var resD2DTexture = manipulator.AddResource(
                        device => new Direct2DTextureResource(d2dDrawingLayer, 256, 256));
                    var resD2DMaterial = manipulator.AddStandardMaterialResource(resD2DTexture);
                    var geoResource = manipulator.AddResource(
                        device => new GeometryResource(new CubeGeometryFactory()));

                    var newMesh = manipulator.AddMeshObject(geoResource, resD2DMaterial);
                    newMesh.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
                    newMesh.Scaling = new Vector3(2f, 2f, 2f);
                });

                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing2D", "SimpleObject_D2DTexture.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task Render_SimpleBitmap_WithTransparency()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var solidBrush = new SolidBrushResource(Color4Ex.LightGray))
            using (var bitmap = new StandardBitmapResource(new AssemblyResourceLink(this.GetType(), "Resources.Bitmaps.Logo.png")))
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync(graphics =>
                {
                    // 2D rendering is made here
                    graphics.FillRectangle(graphics.ScreenBounds, solidBrush);
                    graphics.DrawBitmap(bitmap, new Vector2(100f, 100f));
                });

                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing2D", "SimpleBitmap_Transparency.png"));
                Assert.IsTrue(diff < 0.02, "Difference to reference image is to big!");

            }
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task Render_SimpleBitmap_Animated()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            using (var bitmap = new StandardBitmapResource(
                new AssemblyResourceLink(this.GetType(), "Resources.Bitmaps.Boom.png"),
                8, 8))
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

                await memRenderTarget.RenderLoop.Register2DDrawingLayerAsync(graphics =>
                {
                    // 2D rendering is made here
                    for (var loop = 0; loop < 8; loop++)
                    {
                        graphics.DrawBitmap(bitmap, new Vector2(100f * loop, 10f), frameIndex: 7);
                        graphics.DrawBitmap(bitmap, new Vector2(100f * loop, 100f), frameIndex: 15);
                        graphics.DrawBitmap(bitmap, new Vector2(100f * loop, 200f), frameIndex: 23);
                        graphics.DrawBitmap(bitmap, new Vector2(100f * loop, 300f), frameIndex: 31);
                        graphics.DrawBitmap(bitmap, new Vector2(100f * loop, 400f), frameIndex: 39);
                        graphics.DrawBitmap(bitmap, new Vector2(100f * loop, 500f), frameIndex: 47);
                        graphics.DrawBitmap(bitmap, new Vector2(100f * loop, 600f), frameIndex: 55);
                        graphics.DrawBitmap(bitmap, new Vector2(100f * loop, 700f), frameIndex: 63);
                    }
                });

                //await AsyncResourceLoader.Current.WaitForAllFinishedAsync();
                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                // TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var diff = BitmapComparison.CalculatePercentageDifference(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing2D", "SimpleBitmap_Animated.png"));
                Assert.IsTrue(diff < 0.02, "Difference to reference image is to big!");
            }
        }
    }
}