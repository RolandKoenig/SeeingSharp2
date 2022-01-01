using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Core;
using SeeingSharp.Drawing2D;
using SeeingSharp.Drawing3D;
using SeeingSharp.Drawing3D.Primitives;
using SeeingSharp.Drawing3D.Resources;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;
using SeeingSharp.Views;
using GDI = System.Drawing;

namespace SeeingSharp.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class ErrorHandlingTests
    {
        [TestMethod]
        public async Task MemoryRenderTarget_2DInitError()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            // Ensure that any async disposal is  done before we create a new GraphicsCore
            await GraphicsCore.Current.MainLoop.WaitForNextPassedLoopAsync();
            await GraphicsCore.Current.MainLoop.WaitForNextPassedLoopAsync();

            GDI.Bitmap screenshot = null;
            using (TestUtilities.FailTestOnInternalExceptions())
            using (GraphicsCore.AutomatedTest_NewTestEnvironment())
            {
                await GraphicsCore.Loader
                    .ConfigureLoading(settings => settings.ThrowD2DInitDeviceError = true)
                    .LoadAsync();
                Assert.IsTrue(GraphicsCore.IsLoaded);
                Assert.IsFalse(GraphicsCore.Current.DefaultDevice.Supports2D);

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
                        var d2dRectangle = new GDI.RectangleF(10, 10, 236, 236);
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
                            device => new Direct2DTextureResource(d2dDrawingLayer, 256, 256));
                        var resD2DMaterial = manipulator.AddStandardMaterialResource(resD2DTexture);
                        var resGeometry = manipulator.AddResource(
                            device => new GeometryResource(new CubeGeometryFactory()));

                        var newMesh = manipulator.AddMeshObject(resGeometry, resD2DMaterial);
                        newMesh.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
                        newMesh.Scaling = new Vector3(2f, 2f, 2f);
                    });

                    // Take screenshot
                    await memRenderTarget.AwaitRenderAsync();
                    screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();

                    // TestUtilities.DumpToDesktop(screenshot, "Blub.png");
                }
            }

            // Calculate and check difference
            Assert.IsNotNull(screenshot);
            var isNearEqual = BitmapComparison.IsNearEqual(
                screenshot, TestUtilities.LoadBitmapFromResource("ErrorHandling", "SimpleObject.png"));
            Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
        }

        [TestMethod]
        public async Task MemoryRenderTarget_GraphicsInitError()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            // Ensure that any async disposal is  done before we create a new GraphicsCore
            await GraphicsCore.Current.MainLoop.WaitForNextPassedLoopAsync();
            await GraphicsCore.Current.MainLoop.WaitForNextPassedLoopAsync();

            var isRenderTargetOperational = true;
            var isGraphicsCoreInitialized = true;
            var registeredRenderLoopCount = 1;
            using (GraphicsCore.AutomatedTest_NewTestEnvironment())
            using (GraphicsCore.AutomatedTest_ForceDeviceInitError())
            {
                await GraphicsCore.Loader
                    .LoadAsync();

                using (var memRenderTarget = new MemoryRenderTarget(1024, 1024))
                {
                    isRenderTargetOperational = memRenderTarget.IsOperational;
                    isGraphicsCoreInitialized = GraphicsCore.IsLoaded;
                    registeredRenderLoopCount = GraphicsCore.Current.RegisteredRenderLoopCount;
                }
            }

            Assert.IsFalse(isRenderTargetOperational);
            Assert.IsFalse(isGraphicsCoreInitialized);
            Assert.IsTrue(registeredRenderLoopCount == 0);
        }

        [TestMethod]
        public async Task WinForms_Parent_Child_Switch()
        {
            await TestUtilities.InitializeWithGraphicsAsync();

            // Ensure that any async disposal is  done before we create a new GraphicsCore
            await GraphicsCore.Current.MainLoop.WaitForNextPassedLoopAsync();
            await GraphicsCore.Current.MainLoop.WaitForNextPassedLoopAsync();

            Panel hostPanel1 = null;
            Panel hostPanel2 = null;
            SeeingSharpRendererControl renderControl = null;
            var stepId = 0;
            Exception fakeUIThreadException = null;

            var fakeUIThread = new ObjectThread("Fake-UI", 100);

            fakeUIThread.ThreadException += (sender, eArgs) =>
            {
                fakeUIThreadException = eArgs.Exception;
            };
            fakeUIThread.Starting += (sender, eArgs) =>
            {
                hostPanel1 = new Panel
                {
                    Size = new GDI.Size(500, 500)
                };

                hostPanel2 = new Panel
                {
                    Size = new GDI.Size(500, 500)
                };

                renderControl = new SeeingSharpRendererControl
                {
                    Dock = DockStyle.Fill
                };

                hostPanel1.CreateControl();
                hostPanel2.CreateControl();
                hostPanel1.Controls.Add(renderControl);
            };
            fakeUIThread.Tick += (sender, eArgs) =>
            {
                Application.DoEvents();
                stepId++;

                switch (stepId)
                {
                    case 2:
                        hostPanel1.Controls.Remove(renderControl);
                        break;

                    case 4:
                        hostPanel2.Controls.Add(renderControl);
                        break;

                    case 8:
                        hostPanel2.Controls.Remove(renderControl);
                        break;

                    case 10:
                        renderControl.Dispose();
                        hostPanel2.Dispose();
                        hostPanel1.Dispose();
                        break;

                    case 20:
                        fakeUIThread.Stop();
                        break;
                }
            };
            fakeUIThread.Start();

            // Wait until the Fake-UI thread stopped
            await fakeUIThread.WaitUntilSoppedAsync();

            // Some checks after rendering
            Assert.IsTrue(GraphicsCore.Current.MainLoop.IsRunning);
            Assert.IsTrue(GraphicsCore.Current.MainLoop.RegisteredRenderLoopCount == 0);
            Assert.IsNull(fakeUIThreadException);
            Assert.IsTrue(renderControl.IsDisposed);
        }
    }
}
