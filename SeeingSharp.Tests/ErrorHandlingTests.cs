﻿#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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

    using System;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Multimedia.Core;
    using Multimedia.Drawing2D;
    using Multimedia.Drawing3D;
    using Multimedia.Objects;
    using Multimedia.Views;
    using SeeingSharp.Util;
    using SharpDX;
    using Util;

    #endregion

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
                    var d2dDrawingLayer = new Custom2DDrawingLayer((graphics) =>
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
                    await memRenderTarget.Scene.ManipulateSceneAsync((manipulator) =>
                    {
                        var resD2DTexture = manipulator.AddResource(
                            () => new Direct2DTextureResource(d2dDrawingLayer, 256, 256));
                        var resD2DMaterial = manipulator.AddSimpleColoredMaterial(resD2DTexture);
                        var geoResource = manipulator.AddResource(
                            () => new GeometryResource(new CubeType { Material = resD2DMaterial }));

                        var newObject = manipulator.AddGeneric(geoResource);
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
            var isNearEqual = BitmapComparison.IsNearEqual(
                screenshot, TestUtilities.LoadBitmapFromResource("ErrorHandling", "SimpleObject.png"));
            Assert.IsTrue(isNearEqual, "Difference to reference image is to big!");
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public async Task MemoryRenderTarget_GraphicsInitError()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            var isRenderTargetOperational = true;
            var isGraphicsCoreInitialized = true;
            var registeredRenderLoopCount = 1;

            using (GraphicsCore.AutomatedTest_NewTestEnviornment())
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
        [TestCategory(TEST_CATEGORY)]
        public async Task WinForms_Parent_Child_Switch()
        {
            await TestUtilities.InitializeWithGrahicsAsync();

            Panel hostPanel1 = null;
            Panel hostPanel2 = null;
            SeeingSharpRendererControl renderControl = null;
            var stepID = 0;
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