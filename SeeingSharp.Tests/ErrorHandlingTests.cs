using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Multimedia.Views;
using SeeingSharp.Tests.Util;
using SeeingSharp.Util;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using GDI = System.Drawing;

namespace SeeingSharp.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class ErrorHandlingTests
    {
        public const string TEST_CATEGORY = "SeeingSharp Multimedia ErrorHandling";

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task MemoryRenderTarget_2DInitError()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            GDI.Bitmap screenshot = null;
            using (TestUtilities.FailTestOnInternalExceptions())
            using (GraphicsCore.AutomatedTest_NewTestEnviornment())
            {
                await GraphicsCore.Loader
                    .Configure(settings => settings.ThrowD2DInitDeviceError = true)
                    .LoadAsync();
                Assert.IsTrue(GraphicsCore.IsLoaded);
                Assert.IsFalse(GraphicsCore.Current.DefaultDevice.Supports2D);

                using (SolidBrushResource solidBrush = new SolidBrushResource(Color4Ex.Gray))
                using (TextFormatResource textFormat = new TextFormatResource("Arial", 36))
                using (SolidBrushResource textBrush = new SolidBrushResource(Color4Ex.RedColor))
                using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
                {
                    memRenderTarget.ClearColor = Color4Ex.CornflowerBlue;

                    // Get and configure the camera
                    PerspectiveCamera3D camera = memRenderTarget.Camera as PerspectiveCamera3D;
                    camera.Position = new Vector3(0f, 5f, -7f);
                    camera.Target = new Vector3(0f, 0f, 0f);
                    camera.UpdateCamera();

                    // 2D rendering is made here
                    Custom2DDrawingLayer d2dDrawingLayer = new Custom2DDrawingLayer((graphics) =>
                    {
                        SharpDX.RectangleF d2dRectangle = new SharpDX.RectangleF(10, 10, 236, 236);
                        graphics.Clear(Color4Ex.LightBlue);
                        graphics.FillRoundedRectangle(
                            d2dRectangle, 30, 30,
                            solidBrush);

                        d2dRectangle.Inflate(-10, -10);
                        graphics.DrawText("Hello Direct2D!", textFormat, d2dRectangle, textBrush);
                    });

                    // Define scene
                    await memRenderTarget.Scene.ManipulateSceneAsync((manipulator) =>
                    {
                        var resD2DTexture = manipulator.AddResource<Direct2DTextureResource>(
                            () => new Direct2DTextureResource(d2dDrawingLayer, 256, 256));
                        var resD2DMaterial = manipulator.AddSimpleColoredMaterial(resD2DTexture);
                        var geoResource = manipulator.AddResource<GeometryResource>(
                            () => new GeometryResource(new CubeType() { Material = resD2DMaterial }));

                        GenericObject newObject = manipulator.AddGeneric(geoResource);
                        newObject.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
                        newObject.Scaling = new Vector3(2f, 2f, 2f);
                    });

                    // Take screenshot
                    await memRenderTarget.AwaitRenderAsync();
                    screenshot = await memRenderTarget.RenderLoop.GetScreenshotGdiAsync();

                    // TestUtilities.DumpToDesktop(screenshot, "Blub.png");
                }
            }

            // Calculate and check difference
            Assert.IsNotNull(screenshot);
            bool isNearEqual = BitmapComparison.IsNearEqual(
                screenshot, TestUtilities.LoadBitmapFromResource("ErrorHandling", "SimpleObject.png"));
            Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task MemoryRenderTarget_GraphicsInitError()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            bool isRenderTargetOperational = true;
            bool isGraphicsCoreInitialized = true;
            int registeredRenderLoopCount = 1;
            using (GraphicsCore.AutomatedTest_NewTestEnviornment())
            using (GraphicsCore.AutomatedTest_ForceDeviceInitError())
            {
                await GraphicsCore.Loader
                    .LoadAsync();

                using (MemoryRenderTarget memRenderTarget = new MemoryRenderTarget(1024, 1024))
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
        [TestCategory(TEST_CATEGORY)]
        public async Task WinForms_Parent_Child_Switch()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            Panel hostPanel1 = null;
            Panel hostPanel2 = null;
            SeeingSharpRendererControl renderControl = null;
            int stepID = 0;
            Exception fakeUIThreadException = null;

            ObjectThread fakeUIThread = new ObjectThread("Fake-UI", 100);
            fakeUIThread.ThreadException += (sender, eArgs) =>
            {
                fakeUIThreadException = eArgs.Exception;
            };
            fakeUIThread.Starting += (sender, eArgs) =>
            {
                hostPanel1 = new System.Windows.Forms.Panel();
                hostPanel1.Size = new Size(500, 500);
                hostPanel2 = new System.Windows.Forms.Panel();
                hostPanel2.Size = new Size(500, 500);

                renderControl = new SeeingSharpRendererControl();
                renderControl.Dock = System.Windows.Forms.DockStyle.Fill;

                hostPanel1.CreateControl();
                hostPanel2.CreateControl();
                hostPanel1.Controls.Add(renderControl);
            };
            fakeUIThread.Tick += (sender, eArgs) =>
            {
                Application.DoEvents();
                stepID++;

                switch (stepID)
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

                    case 11:
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
