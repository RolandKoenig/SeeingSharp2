using System;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Core;
using SeeingSharp.Drawing2D;
using SeeingSharp.Drawing2D.Resources;
using SeeingSharp.Drawing3D;
using SeeingSharp.Drawing3D.Primitives;
using SeeingSharp.Drawing3D.Resources;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;
using SeeingSharp.Views;

namespace SeeingSharp.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class Drawing2DTests
    {
        [TestMethod]
        public async Task Collision_Ellipse_to_Ellipse()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

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
        public async Task Collision_Ellipse_to_Polygon()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

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
        public async Task Render_SimpleText_SimpleSingleColor()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var solidBrush = new SolidBrushResource(Color4.RedColor))
            using (var textFormat = new TextFormatResource("Arial", 70))
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4.CornflowerBlue;
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
        public async Task Render_SimpleRoundedRect_Filled_Solid()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var solidBrush = new SolidBrushResource(Color4.Gray))
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4.CornflowerBlue;
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
        public async Task Render_SimpleGeometry()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            var polygon = new Polygon2D(new Vector2(10, 10), new Vector2(900, 100), new Vector2(800, 924), new Vector2(50, 1014), new Vector2(10, 10));

            using (var solidBrush = new SolidBrushResource(Color4.LightGray))
            using (var solidBrushBorder = new SolidBrushResource(Color4.Gray))
            using (var polygonGeometry = new PolygonGeometryResource(polygon))
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4.CornflowerBlue;
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
        public async Task Render_SimpleGeometry_Ellipse()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var solidBrush = new SolidBrushResource(Color4.LightGray))
            using (var solidBrushBorder = new SolidBrushResource(Color4.Gray))
            using (var ellipseGeometry = new EllipseGeometryResource(new Vector2(512, 512), 400f, 300f))
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

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
        public async Task Render_SimpleRoundedRect_Filled_LinearGradient()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var gradientBrush = new LinearGradientBrushResource(
                new Vector2(0f, 0f),
                new Vector2(512f, 0f),
                new[]
                {
                    new GradientStop { Color = Color4.Gray, Position = 0f },
                    new GradientStop { Color = Color4.White, Position = 0.6f },
                    new GradientStop { Color = Color4.Black, Position = 1f }
                },
                ExtendMode.Mirror))
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

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
        public async Task Render_SimpleRoundedRect_Filled_RadialGradient()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var radialGradientBrush = new RadialGradientBrushResource(
                new Vector2(200f, 400f),
                new Vector2(0f, 0f),
                200f, 400f,
                new[]
                {
                    new GradientStop { Color = Color4.Gray, Position = 0f },
                    new GradientStop { Color = Color4.White, Position = 0.6f },
                    new GradientStop { Color = Color4.BlueColor, Position = 1f }
                }))
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4.CornflowerBlue;
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
        public async Task Render_SimpleRoundedRect_Filled_Over3D()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            var brushColor = Color4.Gray;
            brushColor.ChangeAlphaTo(0.5f);

            using (var solidBrush = new SolidBrushResource(brushColor))
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                var camera = (PerspectiveCamera3D)memRenderTarget.Camera;
                camera.Position = new Vector3(0f, 5f, -5f);
                camera.Target = new Vector3(0f, 1f, 0f);
                camera.UpdateCamera();

                // Define scene
                await memRenderTarget.Scene.ManipulateSceneAsync(manipulator =>
                {
                    // Define object
                    var resGeometry = manipulator.AddResource(
                        _ => new GeometryResource(new CubeGeometryFactory()));
                    var resMaterial = manipulator.AddResource(
                        _ => new StandardMaterialResource(enableShaderGeneratedBorder: true));

                    var newMesh = manipulator.AddMeshObject(resGeometry, resMaterial);
                    newMesh.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
                    newMesh.Scaling = new Vector3(2f, 2f, 2f);
                    newMesh.Color = Color4.Goldenrod;
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
            Assert.IsTrue(GraphicsCore.Current.MainLoop!.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        public async Task Render_DebugLayer()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var debugLayer = new DebugDrawingLayer())
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4.CornflowerBlue;
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
        public async Task Render_SimpleObject_D2D_Texture()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var solidBrush = new SolidBrushResource(Color4.Gray))
            using (var textFormat = new TextFormatResource("Arial", 36))
            using (var textBrush = new SolidBrushResource(Color4.RedColor))
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

                // Get and configure the camera
                var camera = (PerspectiveCamera3D)memRenderTarget.Camera;
                camera.Position = new Vector3(0f, 5f, -7f);
                camera.Target = new Vector3(0f, 0f, 0f);
                camera.UpdateCamera();

                // 2D rendering is made here
                var d2dDrawingLayer = new Custom2DDrawingLayer(graphics =>
                {
                    var d2dRectangle = new RectangleF(10, 10, 236, 236);
                    graphics.Clear(Color4.LightBlue);
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
                        _ => new Direct2DTextureResource(d2dDrawingLayer, 256, 256));
                    var resD2DMaterial = manipulator.AddStandardMaterialResource(resD2DTexture);
                    var geoResource = manipulator.AddResource(
                        _ => new GeometryResource(new CubeGeometryFactory()));

                    var newMesh = manipulator.AddMeshObject(geoResource, resD2DMaterial);
                    newMesh.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
                    newMesh.Scaling = new Vector3(2f, 2f, 2f);
                });

                await memRenderTarget.AwaitRenderAsync();

                // Take screenshot
                var screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();
                //TestUtilities.DumpToDesktop(screenshot, "Blub.png");

                // Calculate and check difference
                var isNearEqual = BitmapComparison.IsNearEqual(
                    screenshot, TestUtilities.LoadBitmapFromResource("Drawing2D", "SimpleObject_D2DTexture.png"));
                Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
            }

            // Finishing checks
            Assert.IsTrue(GraphicsCore.Current.MainLoop!.RegisteredRenderLoopCount == 0, "RenderLoops where not disposed correctly!");
        }

        [TestMethod]
        public async Task Render_SimpleBitmap_WithTransparency()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var solidBrush = new SolidBrushResource(Color4.LightGray))
            using (var bitmap = new StandardBitmapResource(new AssemblyResourceLink(this.GetType(), "Resources.Bitmaps.Logo.png")))
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

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
        public async Task Render_SimpleBitmap_Animated()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            using (var bitmap = new StandardBitmapResource(
                new AssemblyResourceLink(this.GetType(), "Resources.Bitmaps.Boom.png"),
                8, 8))
            using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
            {
                // Perform rendering
                memRenderTarget.ClearColor = Color4.CornflowerBlue;

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